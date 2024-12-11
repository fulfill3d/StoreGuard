import base64
import glob
import os
import threading
from pyexpat import features

import cv2
import datetime
import logging
import json

import numpy as np
from dotenv import load_dotenv
from deep_sort_realtime.deepsort_tracker import DeepSort
from ultralytics import YOLO

from azure_app_config_client import AppConfigClient
from azure_service_bus_client import AzureServiceBusClient
from global_tracker import GlobalTracker

# Load environment variables
load_dotenv()

# Set up logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")


class VideoProcessingService:
    """Class to process video stream or Event Hub frames for human detection and send messages."""
    def __init__(self, model, tracker, global_tracker, service_bus_client, queue_name):
        self.event_hub_client = None
        self.model = model
        self.tracker = tracker
        self.global_tracker = global_tracker
        self.service_bus_client = service_bus_client
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

        # Match tracks to global tracker
        for track in tracks:
            if not track.is_confirmed() or track.time_since_update > 1:
                continue

            self.global_tracker.match_and_update(
                self.source_id,
                self.camera_id,
                np.array(track.features)
            )

        # self.handle_tracks(tracks)

    def handle_tracks(self, tracks):
        """Handle active and disappeared tracks."""
        current_time = datetime.datetime.now(datetime.UTC).isoformat()
        logging.info(f"Handling tracks")
        logging.info(f"Tracks: {tracks}")

        # Keep track of currently visible track IDs
        active_track_ids = set()

        for track in tracks:
            if not track.is_confirmed() or track.time_since_update > 1:
                continue

            track_id = str(track.track_id)
            active_track_ids.add(track_id)

            if track_id not in self.active_tracks:
                # New track appeared
                self.active_tracks[track_id] = {"start_time": current_time, "last_seen": current_time}
                logging.info(f"Person {track_id} appeared at {current_time}")
            else:
                # Update last seen time for active track
                self.active_tracks[track_id]["last_seen"] = current_time

        # Check for disappeared tracks
        for track_id in list(self.active_tracks.keys()):
            if track_id not in active_track_ids:
                # Track has disappeared
                start_time = self.active_tracks[track_id]["start_time"]
                end_time = self.active_tracks[track_id]["last_seen"]

                # Prepare and send disappearance message
                message_content = json.dumps({
                    "uuid": track_id,
                    "start_time": start_time,
                    "end_time": end_time,
                    "source_id": self.source_id,
                    "camera_id": self.camera_id
                })
                self.service_bus_client.send_message_to_queue(self.queue_name, message_content)
                logging.info(f"Person {track_id} disappeared at {end_time}")

                # Remove the disappeared track
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

            np_frame = cv2.imdecode(np.frombuffer(frame_bytes, np.uint8), cv2.IMREAD_COLOR)
            if np_frame is not None:
                logging.info(f"NumPy Frame is not None")
                self.process_frame(np_frame)
            partition_context.update_checkpoint()

        if self.event_hub_client:
            self.event_hub_client.receive_events(process_event)

    def run_capture_frame(self, frame_directory, camera_name):
        """
        Simulate video processing by reading frames stored as .png files in a directory.

        :param frame_directory: Directory where frame files are stored.
        :param camera_name: Name of the camera to filter the frames.
        """
        self.camera_id = camera_name
        frame_pattern = os.path.join(frame_directory, f"camera_{camera_name}_frame_*.png")
        frame_files = sorted(glob.glob(frame_pattern))  # Sorted to ensure sequential processing

        if not frame_files:
            logging.error(f"No frames found for camera {camera_name} in directory {frame_directory}.")
            return

        logging.info(f"Starting frame processing for camera {camera_name}. Total frames: {len(frame_files)}")

        for frame_file in frame_files:
            try:
                # Load the frame
                frame = cv2.imread(frame_file)

                if frame is None:
                    logging.warning(f"Could not read frame {frame_file}. Skipping.")
                    continue

                # Process the frame
                self.process_frame(frame)

                # Show the frame for debugging or monitoring purposes
                cv2.imshow("Frame Processing", frame)
                if cv2.waitKey(1) & 0xFF == ord('q'):
                    break

            except Exception as e:
                logging.error(f"Error processing frame {frame_file}: {e}")
                continue

        logging.info("Completed frame processing.")
        cv2.destroyAllWindows()


# if __name__ == "__main__":
#     # Fetch configuration settings from Azure App Configuration
#     config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))
#     service_bus_connection_str = config_client.fetch_configuration_value("ServiceBusConnectionString")
#     queue = config_client.fetch_configuration_value("SG_QueueName")
#
#     if not all([service_bus_connection_str, queue]):
#         logging.error("Missing configuration values for Service Bus. Exiting.")
#         exit(1)
#
#     # Initialize Azure Service Bus Client
#     sb_client = AzureServiceBusClient(service_bus_connection_str)
#
#     try:
#         # Initialize YOLO model
#         yolo = YOLO("yolov8n.pt")
#         deep_sort = DeepSort(max_age=30)
#
#         # Fetch configuration settings from Azure App Configuration
#         video_stream_url = os.getenv("VIDEO_STREAM_URL")
#
#         # Create the Video Processing Service instance
#         video_service = VideoProcessingService(yolo, deep_sort, None, sb_client, queue)
#
#         # Run the video stream from the specified URL
#         video_service.run_video_stream(video_stream_url)
#
#     except Exception as e:
#         logging.error(f"Service encountered an error: {e}")
#     finally:
#         sb_client.close()


def start_camera_stream(camera_name, global_tracker, model, service_bus_client, queue_name):
    tracker = DeepSort(max_age=30)
    service = VideoProcessingService(model, tracker, global_tracker, service_bus_client, queue_name)
    service.run_capture_frame("/Users/abdurrahmangaziyavuz/StoreGuardAI/unity/CapturedFrames/", camera_name)


if __name__ == "__main__":
    config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))
    service_bus_connection_str = config_client.fetch_configuration_value("ServiceBusConnectionString")
    queue = config_client.fetch_configuration_value("SG_QueueName")

    # Initialize Azure Service Bus Client
    sb_client = AzureServiceBusClient(service_bus_connection_str)
    yolo = YOLO("yolov8n.pt")
    global_tracker = GlobalTracker()

    try:

        # List of camera URLs
        # camera_sources = ["cam1", "cam2", "cam3", "cam4", "cam5", "cam6", "cam7", "cam8"]
        camera_sources = ["cam1", "cam2"]
        # tracker = DeepSort(max_age=30)
        # service = VideoProcessingService(yolo, tracker, global_tracker, sb_client, queue)
        # service.run_capture_frame("/Users/abdurrahmangaziyavuz/StoreGuardAI/unity/CapturedFrames/", camera_sources[0])

        # Start a thread for each camera
        threads = []
        for camera_name in camera_sources:
            t = threading.Thread(
                target=start_camera_stream,
                args=(camera_name, global_tracker, yolo, sb_client, queue)
            )
            threads.append(t)
            t.start()

        # Wait for all threads to complete
        for t in threads:
            t.join()

    except Exception as e:
        logging.error(f"Service encountered an error: {e}")
    finally:
        sb_client.close()
        print(global_tracker.global_tracks)
