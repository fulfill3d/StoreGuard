import os
import logging
import json
from azure.identity import ClientSecretCredential, DefaultAzureCredential
from azure.appconfiguration import AzureAppConfigurationClient
from azure.keyvault.secrets import SecretClient
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Set up logging
logging.basicConfig(level=logging.INFO)


class AppConfigClient:
    def __init__(self, app_config_url):
        self.app_config_url = app_config_url
        self.environment = os.getenv("ENVIRONMENT", "production").lower()

        # Determine which credential to use based on the environment
        if self.environment == "local":
            logging.info("Using ClientSecretCredential for local development.")
            self.credential = ClientSecretCredential(
                tenant_id=os.getenv("AZURE_TENANT_ID"),
                client_id=os.getenv("AZURE_CLIENT_ID"),
                client_secret=os.getenv("AZURE_CLIENT_SECRET")
            )
        else:
            logging.info("Using DefaultAzureCredential for non-local environments.")
            self.credential = DefaultAzureCredential()

        # Initialize the Azure App Configuration client
        if self.app_config_url:
            self.app_config_client = AzureAppConfigurationClient(
                self.app_config_url,
                credential=self.credential
            )
        else:
            logging.error("App Configuration URL not provided.")
            raise ValueError("App Configuration URL is missing.")

    def fetch_configuration_value(self, key):
        """Fetch a single configuration setting from Azure App Configuration."""
        try:
            setting = self.app_config_client.get_configuration_setting(key=key)
            if setting:
                value = setting.value
                try:
                    # Check if the value is a JSON object containing a Key Vault URI
                    parsed_value = json.loads(value)
                    if isinstance(parsed_value, dict) and "uri" in parsed_value:
                        secret_uri = parsed_value["uri"]
                        return self.fetch_secret_from_keyvault_uri(secret_uri)
                    else:
                        return value
                except json.JSONDecodeError:
                    return value
            else:
                logging.error(f"Key '{key}' not found in App Configuration.")
                return None
        except Exception as e:
            logging.error(f"Error fetching configuration setting '{key}': {e}")
            return None

    def fetch_secret_from_keyvault_uri(self, secret_uri):
        """Fetch a secret using its full URI from Azure Key Vault."""
        try:
            if "/secrets/" in secret_uri:
                vault_url = secret_uri.split("/secrets/")[0]
                secret_name = secret_uri.split("/secrets/")[1].split("?")[0]

                # Use the same credential to access Key Vault
                secret_client = SecretClient(vault_url=vault_url, credential=self.credential)
                secret = secret_client.get_secret(secret_name)
                logging.info(f"Secret '{secret_name}' fetched successfully from Key Vault.")
                return secret.value
            else:
                logging.error("Invalid Key Vault URI format.")
                return None
        except Exception as e:
            logging.error(f"Error fetching secret from Key Vault: {e}")
            return None


if __name__ == "__main__":
    # Load the App Configuration URL from environment variables
    url = os.getenv('APP_CONFIG_URL')

    # Create an instance of AppConfigClient
    config_client = AppConfigClient(url)

    # Example keys to fetch
    keys_to_fetch = ["ServiceBusConnectionString", "EventHubConnectionString"]

    # Fetch configuration values for each key
    for key in keys_to_fetch:
        value = config_client.fetch_configuration_value(key)
        if value:
            print(f"{key}: {value}")
        else:
            print(f"Failed to fetch value for key: {key}")
