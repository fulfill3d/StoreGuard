import logging
from datetime import datetime
import threading
import numpy as np
from scipy.spatial.distance import cosine
import uuid


class GlobalTracker:
    """Centralized tracker for global person tracking across cameras."""
    def __init__(self):
        self.global_tracks = {}
        self.lock = threading.Lock()

    def match_and_update(self, source_id, camera_id, features):
        """
        Match features to global tracks or create a new global track.
        """
        global_id = None
        best_score = float('inf')
        threshold = 0.5  # Similarity threshold

        with self.lock:
            for gid, track_data in self.global_tracks.items():
                avg_feature = np.mean(track_data["features"], axis=0)
                # Print the shapes of avg_feature and features
                logging.info(f"avg_feature shape: {avg_feature.shape}, features shape: {features.shape}")

                score = cosine(avg_feature.flatten(), features.flatten())

                print(f"Score for global track {gid} is {score}")
                if score < best_score and score < threshold:
                    global_id = gid
                    best_score = score

            if global_id:
                # Update existing global track
                print(f"Updating global track {global_id} with camera {camera_id}")
                average_features = (self.global_tracks[global_id]["features"] + features) / 2
                self.global_tracks[global_id]["features"] = average_features
                self.global_tracks[global_id]["cameras"].add(camera_id)
                self.global_tracks[global_id]["last_seen"] = datetime.now()
            else:
                # Create a new global track
                print(f"Creating new global track with camera {camera_id}")
                global_id = str(uuid.uuid4())
                self.global_tracks[global_id] = {
                    "source_id": source_id,
                    "cameras": {camera_id},
                    "features": features,
                    "first_seen": datetime.now(),
                    "last_seen": datetime.now(),
                }

    def handle_global_tracks(self):
        """Handle active and disappeared tracks globally."""
        timeout = 10  # Timeout period in seconds
        current_time = datetime.now()

        with self.lock:
            for gid, track_data in list(self.global_tracks.items()):
                last_seen = track_data["last_seen"]
                if (current_time - last_seen).total_seconds() > timeout:
                    print(f"Person with global track {gid} has disappeared from all sources.")
                    del self.global_tracks[gid]
