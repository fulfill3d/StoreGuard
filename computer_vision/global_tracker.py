import logging
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
                score = cosine(avg_feature, features)
                if score < best_score and score < threshold:
                    global_id = gid
                    best_score = score

            if global_id:
                # Update existing global track
                self.global_tracks[global_id]["features"].append(features)
                self.global_tracks[global_id]["cameras"].add(camera_id)
            else:
                # Create a new global track
                global_id = str(uuid.uuid4())
                self.global_tracks[global_id] = {
                    "source_id": source_id,
                    "cameras": {camera_id},
                    "features": [features],
                    "first_seen": "",
                    "last_seen": "",
                }

    def handle_global_tracks(self):
        """Handle active and disappeared tracks globally."""
        pass
