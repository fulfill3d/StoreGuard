import logging
import os

from azure.servicebus import ServiceBusClient, ServiceBusMessage

from azure_app_config_client import AppConfigClient

# Set up logging
logging.basicConfig(level=logging.INFO)


class AzureServiceBusClient:
    """A Service Bus Client that can send messages to a queue or topic."""

    def __init__(self, connection_string: str):
        """
        Initialize the Service Bus Client with the provided connection string.
        """
        self.connection_string = connection_string
        self.client = ServiceBusClient.from_connection_string(connection_string)
        logging.info("Azure Service Bus Client initialized.")

    def send_message_to_queue(self, queue_name: str, message_content: str, session_id: str = None):
        """
        Send a message to the specified queue.
        """
        try:
            with self.client.get_queue_sender(queue_name) as sender:
                # Set the SessionId if provided
                message = ServiceBusMessage(message_content, session_id=session_id)
                sender.send_messages(message)
                logging.info(f"Message sent to queue '{queue_name}' with SessionId '{session_id}': {message_content}")
        except Exception as e:
            logging.error(f"Failed to send message to queue '{queue_name}': {e}")

    def send_message_to_topic(self, topic_name: str, message_content: str, session_id: str = None):
        """
        Send a message to the specified topic.
        """
        try:
            with self.client.get_topic_sender(topic_name) as sender:
                # Set the SessionId if provided
                message = ServiceBusMessage(message_content, session_id=session_id)
                sender.send_messages(message)
                logging.info(f"Message sent to topic '{topic_name}' with SessionId '{session_id}': {message_content}")
        except Exception as e:
            logging.error(f"Failed to send message to topic '{topic_name}': {e}")

    def close(self):
        """Close the Service Bus Client."""
        self.client.close()
        logging.info("Azure Service Bus Client closed.")



if __name__ == "__main__":
    # Fetch the connection string from the .env file
    config_client = AppConfigClient(os.getenv('APP_CONFIG_URL'))
    service_bus_connection_string = config_client.fetch_configuration_value("ServiceBusConnectionString")

    # Check if the connection string is available
    if not service_bus_connection_string:
        logging.error("Service Bus connection string is not set in .env")
        exit(1)

    # Initialize the Azure Service Bus Client
    service_bus_client = AzureServiceBusClient(service_bus_connection_string)

    # Example usage: Sending a message to a queue
    queue = "sg-video-service"
    service_bus_client.send_message_to_queue(queue, "Hello, Queue!", None)

    # Example usage: Sending a message to a topic
    # topic = "my-topic"
    # service_bus_client.send_message_to_topic(topic, "Hello, Topic!")

    # Close the client
    service_bus_client.close()
