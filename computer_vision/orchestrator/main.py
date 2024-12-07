from flask import Flask, request
import threading
import time
import json
import os
from azure.servicebus import ServiceBusClient, ServiceBusMessage

app = Flask(__name__)

SERVICEBUS_CONNECTION_STR = os.environ.get('SERVICEBUS_CONNECTION_STR')
QUEUE_NAME = os.environ.get('QUEUE_NAME')

active_persons = {}
lock = threading.Lock()

def cleanup_inactive_persons():
    while True:
        time.sleep(5)
        current_time = time.time()
        with lock:
            inactive_persons = [person_id for person_id, last_seen in active_persons.items() if current_time - last_seen > 5]
            for person_id in inactive_persons:
                print(f"Person {person_id} has left all cameras")
                send_message_to_service_bus(person_id)
                del active_persons[person_id]

def send_message_to_service_bus(person_id):
    servicebus_client = ServiceBusClient.from_connection_string(conn_str=SERVICEBUS_CONNECTION_STR)
    with servicebus_client:
        sender = servicebus_client.get_queue_sender(queue_name=QUEUE_NAME)
        with sender:
            message = ServiceBusMessage(json.dumps({'person_id': person_id}))
            sender.send_messages(message)
            print(f"Sent message to Service Bus for person {person_id}")

@app.route('/process_features', methods=['POST'])
def process_features():
    data = request.json
    camera_id = data['camera_id']
    features = data['features']
    timestamp = data['timestamp']

    person_id = 'person123'

    with lock:
        active_persons[person_id] = timestamp

    print(f"Received features from {camera_id} for person {person_id}")
    return '', 200

if __name__ == '__main__':
    cleanup_thread = threading.Thread(target=cleanup_inactive_persons, daemon=True)
    cleanup_thread.start()
    app.run(host='0.0.0.0', port=5000)
