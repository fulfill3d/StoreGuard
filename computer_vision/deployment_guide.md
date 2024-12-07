# Deployment Instructions

```bash
# Step 5: Build Docker Images

# Build Processor Image
docker build -t processor-image:latest -f ProcessorDockerfile .

# Build Orchestrator Image
docker build -t orchestrator-image:latest -f OrchestratorDockerfile .

# Build Frame Sender Image (if using frame_sender.py)
docker build -t frame-sender-image:latest -f FrameSenderDockerfile .

# Step 6: Deploy to Kubernetes

# Apply ConfigMaps and Secrets
kubectl apply -f processor-config.yaml
kubectl apply -f orchestrator-config.yaml
kubectl apply -f secrets.yaml

# Deploy Processor Deployments
kubectl apply -f processors.yaml

# Deploy Orchestrator Deployment and Service
kubectl apply -f orchestrator.yaml

# Deploy Frame Sender Deployments (if using frame_sender.py)
kubectl apply -f frame-senders.yaml

# Step 7: Test the Application Locally

# Verify that all Pods are running
kubectl get pods

# View Logs of Orchestrator
kubectl logs deployment/orchestrator

# View Logs of Processor for cam1
kubectl logs deployment/processor-cam1

# View Logs of Processor for cam2
kubectl logs deployment/processor-cam2
```