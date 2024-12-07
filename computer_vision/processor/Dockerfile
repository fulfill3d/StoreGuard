# ProcessorDockerfile
FROM python:3.12-slim

RUN pip install azure-eventhub requests

COPY processor.py /app/
WORKDIR /app

CMD ["python", "main.py"]
