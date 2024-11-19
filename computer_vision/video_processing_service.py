import io
import base64
import os
import cv2
import datetime
import logging
import json

import numpy as np
from dotenv import load_dotenv
from deep_sort_realtime.deepsort_tracker import DeepSort
from ultralytics import YOLO

from configuration import AppConfigClient
from azure_service_bus_client import AzureServiceBusClient

# Load environment variables
load_dotenv()

# Set up logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")


class VideoProcessingService:
    """Class to process video stream or Event Hub frames for human detection and send messages."""
    def __init__(self, model, tracker, event_hub_client, service_bus_client, queue_name):
        self.model = model
        self.tracker = tracker
        self.service_bus_client = service_bus_client
        self.event_hub_client = event_hub_client
        self.queue_name = queue_name
        self.source_id = None
        self.camera_id = None
        self.active_tracks = {}

    def process_frame(self, frame):
        """Process a single frame for human detection."""
        # Perform detection with YOLO
        results = self.model.predict(source=frame, save=False)
        detections = []

        # Extract detections for "person" class
        for detection in results[0].boxes:
            x1, y1, x2, y2 = detection.xyxy[0]
            conf = detection.conf[0]
            cls = detection.cls[0]

            if int(cls) == 0:  # Class 0 is "person"
                x1, y1, x2, y2 = int(x1), int(y1), int(x2), int(y2)
                width, height = x2 - x1, y2 - y1
                detections.append(((x1, y1, width, height), conf.item(), "person"))

        tracks = self.tracker.update_tracks(detections, frame=frame)
        self.handle_tracks(tracks)

    def handle_tracks(self, tracks):
        """Handle active and disappeared tracks."""
        current_time = datetime.datetime.now(datetime.UTC).isoformat()

        logging.info(f"Handling tracks")
        for track in tracks:
            if not track.is_confirmed() or track.time_since_update > 1:
                continue

            track_id = str(track.track_id)

            if track_id not in self.active_tracks:
                self.active_tracks[track_id] = {"start_time": current_time}
                logging.info(f"Person {track_id} appeared at {current_time}")
            else:
                self.active_tracks[track_id]["last_seen"] = current_time

        for track_id, info in list(self.active_tracks.items()):
            if track_id not in [str(track.track_id) for track in tracks]:
                end_time = current_time
                start_time = info.get("start_time")
                message_content = json.dumps({
                    "uuid": track_id,
                    "start_time": start_time,
                    "end_time": end_time,
                    "source_id": self.source_id,
                    "camera_id": self.camera_id,
                    "action": "disappeared"
                })
                self.service_bus_client.send_message_to_queue(self.queue_name, message_content, track_id)
                logging.info(f"Person {track_id} disappeared at {end_time}")
                del self.active_tracks[track_id]

    def run_video_stream(self, url):
        """Read video stream from a URL."""
        cap = cv2.VideoCapture(url)

        if not cap.isOpened():
            logging.error("Error: Could not open video stream.")
            return

        while True:
            ret, frame = cap.read()
            if not ret:
                logging.error("Error: Could not read frame.")
                break

            self.process_frame(frame)

            cv2.imshow("Live Tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        cap.release()
        cv2.destroyAllWindows()

    def run_event_hub_listener(self):
        """Listen for video frames from Azure Event Hub."""
        def process_event(partition_context, event):
            logging.info(f"Received event: {event.body_as_str()}")
            payload = json.loads(event.body_as_str())
            self.source_id = payload["SourceId"]
            self.camera_id = payload["CameraId"]
            frame_data = payload["FrameData"]

            frame_bytes = base64.b64decode(frame_data)

            # Save frame bytes for debugging
            # with open("debug_frame.jpg", "wb") as f:
            #     f.write(frame_bytes)

            np_frame = cv2.imdecode(np.frombuffer(frame_bytes, np.uint8), cv2.IMREAD_COLOR)
            if np_frame is not None:
                logging.info(f"NumPy Frame is not None")
                self.process_frame(np_frame)
            partition_context.update_checkpoint()

        if self.event_hub_client:
            self.event_hub_client.receive_events(process_event)


if __name__ == "__main__":
    # Fetch configuration settings from Azure App Configuration
    config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))
    service_bus_connection_str = config_client.fetch_configuration_value("ServiceBusConnectionString")
    queue = config_client.fetch_configuration_value("SG_QueueName")

    if not all([service_bus_connection_str, queue]):
        logging.error("Missing configuration values for Service Bus. Exiting.")
        exit(1)

    # Initialize Azure Service Bus Client
    sb_client = AzureServiceBusClient(service_bus_connection_str)

    try:
        # Initialize YOLO model
        yolo = YOLO("yolov8n.pt")
        deep_sort = DeepSort(max_age=30)

        # Fetch configuration settings from Azure App Configuration
        video_stream_url = os.getenv("VIDEO_STREAM_URL")

        # Create the Video Processing Service instance
        video_service = VideoProcessingService(yolo, deep_sort, None, sb_client, queue)

        # Run the video stream from the specified URL
        video_service.run_video_stream(video_stream_url)

    except Exception as e:
        logging.error(f"Service encountered an error: {e}")
    finally:
        sb_client.close()
