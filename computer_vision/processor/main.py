import sys
import json
import os
from azure.eventhub import EventHubConsumerClient
import time
import requests

EVENTHUB_CONNECTION_STR = os.environ.get('EVENTHUB_CONNECTION_STR')
EVENTHUB_NAME = ''

ORCHESTRATOR_URL = os.environ.get('ORCHESTRATOR_URL')

def on_event(partition_context, event):
    frame_data = event.body_as_str()
    print(f"Received frame from {EVENTHUB_NAME}: {frame_data}")

    person_detected = True
    if person_detected:
        features = {
            'camera_id': EVENTHUB_NAME,
            'features': [0.1, 0.2, 0.3],
            'timestamp': time.time()
        }
        try:
            response = requests.post(ORCHESTRATOR_URL, json=features)
            if response.status_code == 200:
                print(f"Features sent to orchestrator from {EVENTHUB_NAME}")
            else:
                print(f"Failed to send features: {response.status_code}")
        except Exception as e:
            print(f"Error sending features to orchestrator: {e}")

    partition_context.update_checkpoint(event)

def main():
    global EVENTHUB_NAME
    if len(sys.argv) != 2:
        print("Usage: python processor.py <camera_id>")
        sys.exit(1)

    EVENTHUB_NAME = sys.argv[1]

    if not EVENTHUB_CONNECTION_STR:
        print("EVENTHUB_CONNECTION_STR environment variable not set")
        sys.exit(1)

    consumer_client = EventHubConsumerClient.from_connection_string(
        conn_str=EVENTHUB_CONNECTION_STR,
        consumer_group='$Default',
        eventhub_name=EVENTHUB_NAME
    )

    print(f"Processor for {EVENTHUB_NAME} started")

    try:
        with consumer_client:
            consumer_client.receive(
                on_event=on_event,
                starting_position="-1",
            )
    except KeyboardInterrupt:
        print("Processor stopped")

if __name__ == "__main__":
    main()
