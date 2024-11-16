import logging
import os

from deep_sort_realtime.deepsort_tracker import DeepSort
from ultralytics import YOLO

from azure_event_hub_client import AzureEventHubClient
from azure_service_bus_client import AzureServiceBusClient
from configuration import AppConfigClient
from video_processing_service import VideoProcessingService

from dotenv import load_dotenv

# Load environment variables
load_dotenv()

def main_case1():
    """Case 1 is when single source of video stream is processed.
    There are no multi-cameras to detect similarities between
    sources to track the same object between different sources"""
    # Fetch configuration settings from Azure App Configuration
    config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))

    service_bus_connection_str = config_client.fetch_configuration_value("ServiceBusConnectionString")
    queue_name = config_client.fetch_configuration_value("SG_QueueName")

    event_hub_connection_str = config_client.fetch_configuration_value("EventHubConnectionString")
    event_hub_name = config_client.fetch_configuration_value("EventHubName")

    if not all([service_bus_connection_str, queue_name]):
        logging.error("Missing configuration values for Service Bus. Exiting.")
        exit(1)

    if not all([event_hub_connection_str, event_hub_name]):
        logging.error("Missing configuration values for Event Hub. Exiting.")
        exit(1)

    # Initialize Azure Service Bus Client
    service_bus_client = AzureServiceBusClient(service_bus_connection_str)

    # Initialize Azure Event Hub Client
    event_hub_client = AzureEventHubClient(event_hub_connection_str, event_hub_name)

    try:
        # Initialize YOLO model
        model = YOLO("yolov8n.pt")
        tracker = DeepSort(max_age=30)

        # Create the Video Processing Service instance
        video_service = VideoProcessingService(model, tracker, event_hub_client, service_bus_client, queue_name)

        # Run the event hub listener to process frames from Azure Event Hub
        video_service.run_event_hub_listener()

    except Exception as e:
        logging.error(f"Service encountered an error: {e}")
    finally:
        service_bus_client.close()
        if event_hub_client:
            event_hub_client.close()


if __name__ == "__main__":
    main_case1()