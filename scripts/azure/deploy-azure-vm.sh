#!/bin/bash

# Azure VM Deployment Script for Cognizant Summer Practice
# This script creates an Azure VM, installs Docker, clones the repository,
# and starts the database services with proper port configuration.

set -e  # Exit on any error

# Configuration variables - Using existing azure.env configuration
RESOURCE_GROUP="csp-rg"
LOCATION="westeurope"
VM_NAME="cognizant-vm"
VM_SIZE="Standard_B2s"  # 2 vCPUs, 4 GB RAM - suitable for development
ADMIN_USERNAME="azureuser"
SSH_KEY_PATH="~/.ssh/id_rsa.pub"
REPO_URL="https://github.com/Cognizant-Summer-Practice-2025/Cognizant-Summer-Practice.git"
REPO_DIR="Cognizant-Summer-Practice"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Azure CLI is installed
check_azure_cli() {
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first:"
        echo "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    print_success "Azure CLI is installed"
}

# Function to check if user is logged in to Azure
check_azure_login() {
    if ! az account show &> /dev/null; then
        print_warning "You are not logged in to Azure. Please log in:"
        az login
    fi
    print_success "Logged in to Azure as: $(az account show --query user.name -o tsv)"
}

# Function to create resource group
create_resource_group() {
    print_status "Creating resource group: $RESOURCE_GROUP"
    if az group show --name $RESOURCE_GROUP &> /dev/null; then
        print_warning "Resource group $RESOURCE_GROUP already exists"
    else
        az group create --name $RESOURCE_GROUP --location $LOCATION
        print_success "Resource group created"
    fi
}

# Function to create virtual network and subnet
create_network() {
    print_status "Creating virtual network and subnet"
    
    # Create virtual network
    if ! az network vnet show --resource-group $RESOURCE_GROUP --name cognizant-vnet &> /dev/null; then
        az network vnet create \
            --resource-group $RESOURCE_GROUP \
            --name cognizant-vnet \
            --address-prefix 10.0.0.0/16 \
            --subnet-name default \
            --subnet-prefix 10.0.1.0/24
        print_success "Virtual network created"
    else
        print_warning "Virtual network cognizant-vnet already exists"
    fi
}

# Function to create network security group with database ports
create_nsg() {
    print_status "Creating network security group with database ports"
    
    if ! az network nsg show --resource-group $RESOURCE_GROUP --name cognizant-nsg &> /dev/null; then
        # Create NSG
        az network nsg create \
            --resource-group $RESOURCE_GROUP \
            --name cognizant-nsg
        
        # Allow SSH
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-ssh \
            --protocol tcp \
            --priority 1000 \
            --destination-port-range 22
        
        # Allow PostgreSQL port 5432
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-postgres-5432 \
            --protocol tcp \
            --priority 1001 \
            --destination-port-range 5432
        
        # Allow PostgreSQL port 5433
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-postgres-5433 \
            --protocol tcp \
            --priority 1002 \
            --destination-port-range 5433
        
        # Allow PostgreSQL port 5434
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-postgres-5434 \
            --protocol tcp \
            --priority 1003 \
            --destination-port-range 5434
        
        # Allow HTTP for potential web access
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-http \
            --protocol tcp \
            --priority 1004 \
            --destination-port-range 80
        
        # Allow HTTPS
        az network nsg rule create \
            --resource-group $RESOURCE_GROUP \
            --nsg-name cognizant-nsg \
            --name allow-https \
            --protocol tcp \
            --priority 1005 \
            --destination-port-range 443
        
        print_success "Network security group created with database ports (5432, 5433, 5434)"
    else
        print_warning "Network security group cognizant-nsg already exists"
    fi
}

# Function to create public IP
create_public_ip() {
    print_status "Creating public IP address"
    
    if ! az network public-ip show --resource-group $RESOURCE_GROUP --name cognizant-ip &> /dev/null; then
        az network public-ip create \
            --resource-group $RESOURCE_GROUP \
            --name cognizant-ip \
            --allocation-method Static \
            --sku Standard
        print_success "Public IP created"
    else
        print_warning "Public IP cognizant-ip already exists"
    fi
}

# Function to create network interface
create_nic() {
    print_status "Creating network interface"
    
    if ! az network nic show --resource-group $RESOURCE_GROUP --name cognizant-nic &> /dev/null; then
        az network nic create \
            --resource-group $RESOURCE_GROUP \
            --name cognizant-nic \
            --vnet-name cognizant-vnet \
            --subnet default \
            --network-security-group cognizant-nsg \
            --public-ip-address cognizant-ip
        print_success "Network interface created"
    else
        print_warning "Network interface cognizant-nic already exists"
    fi
}

# Function to create VM
create_vm() {
    print_status "Creating virtual machine: $VM_NAME"
    
    if ! az vm show --resource-group $RESOURCE_GROUP --name $VM_NAME &> /dev/null; then
        az vm create \
            --resource-group $RESOURCE_GROUP \
            --name $VM_NAME \
            --location $LOCATION \
            --size $VM_SIZE \
            --admin-username $ADMIN_USERNAME \
            --ssh-key-values $SSH_KEY_PATH \
            --nics cognizant-nic \
            --image "Canonical:0001-com-ubuntu-server-focal:20_04-lts-gen2:latest" \
            --generate-ssh-keys
        print_success "Virtual machine created"
    else
        print_warning "Virtual machine $VM_NAME already exists"
    fi
}

# Function to get VM public IP
get_vm_ip() {
    # Return only the IP on stdout so callers can safely capture it
    az vm show -d --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --query publicIps -o tsv
}

# Function to wait for VM to be ready
wait_for_vm_ready() {
    print_status "Waiting for VM to be ready..."
    print_status "Getting VM public IP address"
    VM_IP=$(get_vm_ip)
    print_success "VM public IP: $VM_IP"

    # Probe SSH readiness using ssh with a short timeout (more reliable than nc)
    ATTEMPTS=0
    MAX_ATTEMPTS=30
    until ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 -o BatchMode=yes "$ADMIN_USERNAME@$VM_IP" 'true' 2>/dev/null; do
        ATTEMPTS=$((ATTEMPTS+1))
        if [ "$ATTEMPTS" -ge "$MAX_ATTEMPTS" ]; then
            print_error "SSH did not become available on $VM_IP after $MAX_ATTEMPTS attempts"
            exit 1
        fi
        print_status "Waiting for SSH to be available on $VM_IP (attempt $ATTEMPTS/$MAX_ATTEMPTS)..."
        sleep 10
    done

    # Give the system a few extra seconds after SSH becomes available
    sleep 10
    print_success "VM is ready"
}

# Function to install Docker on VM
install_docker() {
    print_status "Installing Docker on VM..."
    VM_IP=$(get_vm_ip)
    
    # Install Docker using the convenience script
    ssh -o StrictHostKeyChecking=no $ADMIN_USERNAME@$VM_IP << 'EOF'
        # Update package list
        sudo apt-get update
        
        # Install required packages
        sudo apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release
        
        # Add Docker's official GPG key
        curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
        
        # Add Docker repository
        echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
        
        # Update package list again
        sudo apt-get update
        
        # Install Docker
        sudo apt-get install -y docker-ce docker-ce-cli containerd.io
        
        # Add user to docker group
        sudo usermod -aG docker $USER
        
        # Start and enable Docker service
        sudo systemctl start docker
        sudo systemctl enable docker
        
        # Install Docker Compose
        sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
        sudo chmod +x /usr/local/bin/docker-compose
        
        # Verify installations
        docker --version
        docker-compose --version
EOF
    
    print_success "Docker installed successfully"
}

# Function to clone repository and start services
deploy_application() {
    print_status "Deploying application..."
    VM_IP=$(get_vm_ip)
    
    ssh -o StrictHostKeyChecking=no $ADMIN_USERNAME@$VM_IP << EOF
        # Clone repository
        if [ ! -d "$REPO_DIR" ]; then
            git clone $REPO_URL
            cd $REPO_DIR
        else
            cd $REPO_DIR
            git pull origin master
        fi
        
        # Create .env file if it doesn't exist
        if [ ! -f ".env" ]; then
            cat > .env << 'ENVEOF'
# Database Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB_USER=user_db
POSTGRES_DB_MESSAGES=messages_db
POSTGRES_DB_PORTFOLIO=portfolio_db

# Connection Strings
CONNECTION_STRINGS_DEFAULT_CONNECTION=Host=localhost;Port=5432;Database=user_db;Username=postgres;Password=postgres
CONNECTION_STRINGS_MESSAGES_CONNECTION=Host=localhost;Port=5433;Database=messages_db;Username=postgres;Password=postgres
CONNECTION_STRINGS_PORTFOLIO_CONNECTION=Host=localhost;Port=5434;Database=portfolio_db;Username=postgres;Password=postgres

# Environment
ASPNETCORE_ENVIRONMENT=Development
ENVEOF
        fi
        
        # Start database services
        sudo docker-compose up -d user-db messages-db portfolio-db
        
        # Wait for databases to be ready
        echo "Waiting for databases to be ready..."
        sleep 30
        
        # Check database status
        sudo docker-compose ps
        
        # Test database connections
        echo "Testing database connections..."
        sudo docker exec user_postgresql pg_isready -U postgres -d user_db
        sudo docker exec messages_postgresql pg_isready -U postgres -d messages_db
        sudo docker exec portfolio_postgresql pg_isready -U postgres -d portfolio_db
        
        echo "Application deployed successfully!"
        echo "Database ports:"
        echo "  - User DB: 5432"
        echo "  - Messages DB: 5433"
        echo "  - Portfolio DB: 5434"
EOF
    
    print_success "Application deployed successfully"
}

# Function to display connection information
display_connection_info() {
    VM_IP=$(get_vm_ip)
    
    echo ""
    echo "=========================================="
    echo "           DEPLOYMENT COMPLETE            "
    echo "=========================================="
    echo ""
    echo "VM Information:"
    echo "  - Name: $VM_NAME"
    echo "  - Resource Group: $RESOURCE_GROUP"
    echo "  - Location: $LOCATION"
    echo "  - Public IP: $VM_IP"
    echo "  - Admin Username: $ADMIN_USERNAME"
    echo ""
    echo "Database Services:"
    echo "  - User Database: $VM_IP:5432"
    echo "  - Messages Database: $VM_IP:5433"
    echo "  - Portfolio Database: $VM_IP:5434"
    echo ""
    echo "Connection Commands:"
    echo "  - SSH: ssh $ADMIN_USERNAME@$VM_IP"
    echo "  - User DB: psql -h $VM_IP -p 5432 -U postgres -d user_db"
    echo "  - Messages DB: psql -h $VM_IP -p 5433 -U postgres -d messages_db"
    echo "  - Portfolio DB: psql -h $VM_IP -p 5434 -U postgres -d portfolio_db"
    echo ""
    echo "Database Credentials:"
    echo "  - Username: postgres"
    echo "  - Password: postgres"
    echo ""
    echo "To stop services: ssh $ADMIN_USERNAME@$VM_IP 'cd $REPO_DIR && sudo docker-compose down'"
    echo "To start services: ssh $ADMIN_USERNAME@$VM_IP 'cd $REPO_DIR && sudo docker-compose up -d'"
    echo ""
}

# Main execution
main() {
    echo "=========================================="
    echo "  Azure VM Deployment Script"
    echo "  Cognizant Summer Practice"
    echo "=========================================="
    echo ""
    
    # Check prerequisites
    check_azure_cli
    check_azure_login
    
    # Deploy infrastructure
    create_resource_group
    create_network
    create_nsg
    create_public_ip
    create_nic
    create_vm
    
    # Wait for VM to be ready
    wait_for_vm_ready
    
    # Install Docker
    install_docker
    
    # Deploy application
    deploy_application
    
    # Display connection information
    display_connection_info
}

# Check if script is run with arguments
if [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
    echo "Usage: $0 [--help]"
    echo ""
    echo "This script deploys a complete Azure VM with Docker and the Cognizant Summer Practice application."
    echo ""
    echo "Prerequisites:"
    echo "  - Azure CLI installed and configured"
    echo "  - SSH key pair available"
    echo "  - Azure subscription with sufficient permissions"
    echo ""
    echo "Configuration:"
    echo "  Edit the script variables at the top to customize:"
    echo "  - RESOURCE_GROUP: Azure resource group name"
    echo "  - LOCATION: Azure region"
    echo "  - VM_NAME: Virtual machine name"
    echo "  - VM_SIZE: VM size (Standard_B2s recommended for development)"
    echo "  - ADMIN_USERNAME: Admin username for the VM"
    echo "  - SSH_KEY_PATH: Path to your public SSH key"
    echo ""
    echo "The script will:"
    echo "  1. Create Azure resources (resource group, network, NSG, VM)"
    echo "  2. Install Docker and Docker Compose on the VM"
    echo "  3. Clone the repository and start database services"
    echo "  4. Configure firewall rules for database ports (5432, 5433, 5434)"
    echo ""
    exit 0
fi

# Run main function
main "$@"
