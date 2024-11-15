import logging
import os
from azure.eventhub import EventHubConsumerClient, EventData
from dotenv import load_dotenv

from configuration import AppConfigClient

# Set up logging
logging.basicConfig(level=logging.INFO)


class AzureEventHubClient:
    """A Client for receiving messages from an Azure Event Hub."""

    def __init__(self, connection_string: str, event_hub_name: str, consumer_group: str = "$Default"):
        """
        Initialize the Event Hub client.

        :param connection_string: The connection string for the Event Hub.
        :param event_hub_name: The name of the Event Hub.
        :param consumer_group: The consumer group to listen on (default is "$Default").
        """
        self.connection_string = connection_string
        self.event_hub_name = event_hub_name
        self.consumer_group = consumer_group
        self.client = EventHubConsumerClient.from_connection_string(
            conn_str=connection_string,
            consumer_group=consumer_group,
            eventhub_name=event_hub_name
        )
        logging.info("Azure Event Hub Client initialized.")

    def receive_events(self, on_event_callback):
        """
        Start receiving events using the provided callback function.

        :param on_event_callback: A callback function to process each event.
        """
        try:
            logging.info("Listening for events from Azure Event Hub...")
            self.client.receive(
                on_event=on_event_callback,
                starting_position="-1"  # Read from the beginning of the stream
            )
        except KeyboardInterrupt:
            logging.info("Event Hub listener stopped by user.")
        except Exception as e:
            logging.error(f"Error receiving events: {e}")
        finally:
            self.close()

    def close(self):
        """Close the Event Hub client."""
        if self.client:
            self.client.close()
            logging.info("Azure Event Hub Client closed.")


# Test the Event Hub Client
if __name__ == "__main__":
    # Load environment variables from .env
    load_dotenv()

    # Fetch the Event Hub connection details from Azure App Configuration
    config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))
    event_hub_connection_string = config_client.fetch_configuration_value("EventHubConnectionString")
    event_hub_name = config_client.fetch_configuration_value("EventHubName")

    if not event_hub_connection_string or not event_hub_name:
        logging.error("Missing Event Hub configuration details. Exiting.")
        exit(1)

    # Initialize the Azure Event Hub Client
    event_hub_client = AzureEventHubClient(event_hub_connection_string, event_hub_name)


    def process_event(partition_context, event: EventData):
        """
        Callback function to handle received events.
        """
        try:
            message = event.body_as_str()
            logging.info(f"Received event: {message}")

            # Update checkpoint after processing event
            partition_context.update_checkpoint()
        except Exception as e:
            logging.error(f"Error processing event: {e}")


    # Start listening for events
    event_hub_client.receive_events(process_event)
