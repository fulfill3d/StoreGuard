import os
import cv2
import datetime
import logging
from dotenv import load_dotenv
from deep_sort_realtime.deepsort_tracker import DeepSort
from ultralytics import YOLO

# Load environment variables
load_dotenv()

VIDEO_STREAM_URL = os.getenv("VIDEO_STREAM_URL")
SERVICE_BUS_CONNECTION_STR = os.getenv("SERVICE_BUS_CONNECTION_STRING")
QUEUE_NAME = os.getenv("QUEUE_NAME")

# Logging configuration
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")


# Initialize YOLO and DeepSORT with dependency injection
class VideoProcessingService:
    def __init__(self, model, tracker):
        self.model = model
        self.tracker = tracker

    def process_frame(self, frame):
        # Optimize for Apple Silicon: Resize for faster inference
        resized_frame = cv2.resize(frame, (640, 640))  # Adjust dimensions based on model input requirements

        # Perform detection with YOLO
        results = self.model.predict(source=resized_frame, save=False)
        detections = []

        # Extract "person" detections
        for detection in results[0].boxes:
            x1, y1, x2, y2 = detection.xyxy[0]  # Coordinates of bounding box
            conf = detection.conf[0]  # Confidence score
            cls = detection.cls[0]  # Class ID

            if int(cls) == 0:  # Class 0 is "person" in COCO dataset
                # Convert bounding box to integers and store as (x, y, width, height)
                detections.append(((int(x1), int(y1), int(x2 - x1), int(y2 - y1)), conf.item(), "person"))

        # Update tracks with DeepSORT
        tracks = self.tracker.update_tracks(detections, frame=resized_frame)
        for track in tracks:
            if not track.is_confirmed() or track.time_since_update > 1:
                continue

            # Get track details and assign UUID
            track_id = str(track.track_id)
            bbox = track.to_ltwh()
            timestamp = datetime.datetime.now().isoformat()
            action = "inside"  # Placeholder, can expand this

            # Construct message payload
            data = {
                "uuid": track_id,
                "bbox": bbox,
                "timestamp": timestamp,
                "action": action
            }

            print(data)

    def run_video_stream(self, url):
        # Use optimized video capture settings for Apple Silicon
        cap = cv2.VideoCapture(url, cv2.CAP_FFMPEG)  # AVFoundation backend for macOS

        if not cap.isOpened():
            logging.error("Error: Could not open video stream.")
            return

        while True:
            ret, frame = cap.read()
            if not ret:
                logging.error("Error: Could not read frame.")
                break

            # Process each frame for detection and tracking
            self.process_frame(frame)

            # Display for testing (optional)
            cv2.imshow("Live Tracking", frame)
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break

        cap.release()
        cv2.destroyAllWindows()


# Dependency injection for YOLO, DeepSORT, and Service Bus
def main():
    try:
        # Initialize YOLO model with Metal backend on Apple Silicon
        model = YOLO("yolov8n.pt")  # Choose a small YOLO variant to optimize for speed
        tracker = DeepSort(max_age=30)

        # Create video processing service instance
        video_processing_service = VideoProcessingService(model, tracker)

        # Start processing the live video stream
        video_processing_service.run_video_stream(VIDEO_STREAM_URL)
    except Exception as e:
        logging.error(f"Service encountered an error: {e}")


if __name__ == "__main__":
    main()
