#!/bin/bash

# Azure VM Deployment Script (Docker Only)
# - Creates a VM in existing RG/location
# - Opens ports 22 (SSH) and 8080 (app)
# - Installs Docker + Docker Compose
# - No repo clone or docker compose run

set -e

# Configuration (aligned with scripts/azure/deploy-azure-vm.sh and .state/azure.env)
RESOURCE_GROUP="csp-rg"
LOCATION="westeurope"
VM_NAME="cognizant-vm-docker"
VM_SIZE="Standard_B2s"
ADMIN_USERNAME="azureuser"
SSH_KEY_PATH="~/.ssh/id_rsa.pub"

# Reuse existing network resources from the main VM deployment
VNET_NAME="cognizant-vnet"
SUBNET_NAME="default"
NSG_NAME="cognizant-nsg"
PUBLIC_IP_NAME="cognizant-ip-docker"
NIC_NAME="cognizant-nic-docker"

# Utility output
BLUE='\033[0;34m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'
log() { echo -e "${BLUE}[INFO]${NC} $1"; }
ok() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
warn() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
err() { echo -e "${RED}[ERROR]${NC} $1"; }

require_az() {
  command -v az >/dev/null 2>&1 || { err "Azure CLI not found"; exit 1; }
  ok "Azure CLI found"
}

ensure_login() {
  if ! az account show >/dev/null 2>&1; then
    warn "Not logged in. Opening browser..."
    az login >/dev/null
  fi
  ok "Logged in to Azure"
}

ensure_rg() {
  log "Ensuring resource group $RESOURCE_GROUP in $LOCATION"
  if ! az group show -n "$RESOURCE_GROUP" >/dev/null 2>&1; then
    az group create -n "$RESOURCE_GROUP" -l "$LOCATION" >/dev/null
    ok "Created RG $RESOURCE_GROUP"
  else
    warn "RG $RESOURCE_GROUP already exists"
  fi
}

check_existing_network() {
  log "Verifying existing VNet/Subnet/NSG"
  az network vnet show -g "$RESOURCE_GROUP" -n "$VNET_NAME" >/dev/null 2>&1 || { err "VNet $VNET_NAME not found"; exit 1; }
  az network vnet subnet show -g "$RESOURCE_GROUP" --vnet-name "$VNET_NAME" -n "$SUBNET_NAME" >/dev/null 2>&1 || { err "Subnet $SUBNET_NAME not found"; exit 1; }
  az network nsg show -g "$RESOURCE_GROUP" -n "$NSG_NAME" >/dev/null 2>&1 || { err "NSG $NSG_NAME not found"; exit 1; }
  ok "Existing network resources found"
}

ensure_public_ip() {
  log "Ensuring Public IP $PUBLIC_IP_NAME"
  if ! az network public-ip show -g "$RESOURCE_GROUP" -n "$PUBLIC_IP_NAME" >/dev/null 2>&1; then
    az network public-ip create -g "$RESOURCE_GROUP" -n "$PUBLIC_IP_NAME" --sku Standard --allocation-method Static >/dev/null
    ok "Created Public IP $PUBLIC_IP_NAME"
  else
    warn "Public IP $PUBLIC_IP_NAME already exists"
  fi
}

ensure_nic() {
  log "Ensuring NIC $NIC_NAME"
  if ! az network nic show -g "$RESOURCE_GROUP" -n "$NIC_NAME" >/dev/null 2>&1; then
    az network nic create -g "$RESOURCE_GROUP" -n "$NIC_NAME" \
      --vnet-name "$VNET_NAME" --subnet "$SUBNET_NAME" \
      --network-security-group "$NSG_NAME" --public-ip-address "$PUBLIC_IP_NAME" >/dev/null
    ok "Created NIC $NIC_NAME"
  else
    warn "NIC $NIC_NAME already exists"
  fi
}

ensure_vm() {
  log "Ensuring VM $VM_NAME"
  if ! az vm show -g "$RESOURCE_GROUP" -n "$VM_NAME" >/dev/null 2>&1; then
    az vm create -g "$RESOURCE_GROUP" -n "$VM_NAME" -l "$LOCATION" \
      --image "Canonical:0001-com-ubuntu-server-focal:20_04-lts-gen2:latest" \
      --size "$VM_SIZE" --admin-username "$ADMIN_USERNAME" \
      --ssh-key-values "$SSH_KEY_PATH" --nics "$NIC_NAME" --generate-ssh-keys >/dev/null
    ok "Created VM $VM_NAME"
  else
    warn "VM $VM_NAME already exists"
  fi
}

vm_ip() {
  az vm show -d -g "$RESOURCE_GROUP" -n "$VM_NAME" --query publicIps -o tsv
}

wait_for_ssh() {
  local ip; ip=$(vm_ip); log "VM Public IP: $ip"; ok "IP acquired"
  local attempts=0; local max=30
  until ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 -o BatchMode=yes "$ADMIN_USERNAME@$ip" 'true' 2>/dev/null; do
    attempts=$((attempts+1))
    if [ "$attempts" -ge "$max" ]; then
      err "SSH did not become available on $ip after $max attempts"; exit 1
    fi
    log "Waiting for SSH on $ip (attempt $attempts/$max)..."; sleep 10
  done
  ok "SSH is available on $ip"
}

install_docker() {
  local ip; ip=$(vm_ip)
  log "Installing Docker on $ip"
  ssh -o StrictHostKeyChecking=no "$ADMIN_USERNAME@$ip" << 'EOF'
    set -e
    sudo apt-get update
    sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list >/dev/null
    sudo apt-get update
    sudo apt-get install -y docker-ce docker-ce-cli containerd.io
    sudo usermod -aG docker $USER
    sudo systemctl enable docker
    sudo systemctl start docker
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    docker --version
    docker-compose --version
EOF
  ok "Docker installed on $ip"
}

summary() {
  local ip; ip=$(vm_ip)
  echo ""
  echo "=========================================="
  echo " Docker-only VM ready"
  echo " VM: $VM_NAME"
  echo " RG: $RESOURCE_GROUP"
  echo " Location: $LOCATION"
  echo " Public IP: $ip"
  echo " Open ports: 22 (SSH), 8080"
  echo " SSH: ssh $ADMIN_USERNAME@$ip"
  echo "=========================================="
}

main() {
  echo "=========================================="
  echo " Azure VM (Docker Only)"
  echo "=========================================="
  require_az
  ensure_login
  ensure_rg
  check_existing_network
  ensure_public_ip
  ensure_nic
  ensure_vm
  wait_for_ssh
  install_docker
  summary
}

if [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
  echo "Usage: $0"
  echo "Creates an Azure VM, opens ports 22 and 8080, and installs Docker."
  exit 0
fi

main "$@"


