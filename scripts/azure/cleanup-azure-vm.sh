#!/bin/bash

# Azure VM Cleanup Script for Cognizant Summer Practice
# This script removes all Azure resources created by the deployment script.

set -e  # Exit on any error

# Configuration variables - Must match deploy-azure-vm.sh
RESOURCE_GROUP="csp-rg"
VM_NAME="cognizant-vm"

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

# Function to confirm deletion
confirm_deletion() {
    echo ""
    echo "=========================================="
    echo "           CLEANUP CONFIRMATION           "
    echo "=========================================="
    echo ""
    echo "This script will remove the following Azure resources:"
    echo "  - Virtual Machine: $VM_NAME"
    echo "  - Network Interface: cognizant-nic"
    echo "  - Public IP: cognizant-ip"
    echo "  - Network Security Group: cognizant-nsg"
    echo "  - Virtual Network: cognizant-vnet"
    echo "  - Resource Group: $RESOURCE_GROUP (and all resources within it)"
    echo ""
    echo "⚠️  WARNING: This action cannot be undone!"
    echo ""
    
    read -p "Are you sure you want to continue? (yes/no): " confirmation
    
    if [ "$confirmation" != "yes" ]; then
        print_warning "Cleanup cancelled by user"
        exit 0
    fi
    
    echo ""
}

# Function to stop and remove VM
remove_vm() {
    print_status "Removing virtual machine: $VM_NAME"
    
    if az vm show --resource-group $RESOURCE_GROUP --name $VM_NAME &> /dev/null; then
        # Stop the VM first
        print_status "Stopping VM..."
        az vm deallocate --resource-group $RESOURCE_GROUP --name $VM_NAME
        
        # Delete the VM
        print_status "Deleting VM..."
        az vm delete --resource-group $RESOURCE_GROUP --name $VM_NAME --yes
        print_success "Virtual machine removed"
    else
        print_warning "Virtual machine $VM_NAME does not exist"
    fi
}

# Function to remove network resources
remove_network_resources() {
    print_status "Removing network resources"
    
    # Remove network interface
    if az network nic show --resource-group $RESOURCE_GROUP --name cognizant-nic &> /dev/null; then
        az network nic delete --resource-group $RESOURCE_GROUP --name cognizant-nic
        print_success "Network interface removed"
    fi
    
    # Remove public IP
    if az network public-ip show --resource-group $RESOURCE_GROUP --name cognizant-ip &> /dev/null; then
        az network public-ip delete --resource-group $RESOURCE_GROUP --name cognizant-ip
        print_success "Public IP removed"
    fi
    
    # Remove network security group
    if az network nsg show --resource-group $RESOURCE_GROUP --name cognizant-nsg &> /dev/null; then
        az network nsg delete --resource-group $RESOURCE_GROUP --name cognizant-nsg
        print_success "Network security group removed"
    fi
    
    # Remove virtual network
    if az network vnet show --resource-group $RESOURCE_GROUP --name cognizant-vnet &> /dev/null; then
        az network vnet delete --resource-group $RESOURCE_GROUP --name cognizant-vnet
        print_success "Virtual network removed"
    fi
}

# Function to remove resource group
remove_resource_group() {
    print_status "Removing resource group: $RESOURCE_GROUP"
    
    if az group show --name $RESOURCE_GROUP &> /dev/null; then
        az group delete --name $RESOURCE_GROUP --yes --no-wait
        print_success "Resource group deletion initiated (this may take a few minutes)"
        print_status "You can check the status with: az group show --name $RESOURCE_GROUP"
    else
        print_warning "Resource group $RESOURCE_GROUP does not exist"
    fi
}

# Function to list remaining resources
list_remaining_resources() {
    print_status "Checking for remaining resources in resource group: $RESOURCE_GROUP"
    
    if az group show --name $RESOURCE_GROUP &> /dev/null; then
        echo ""
        echo "Remaining resources in $RESOURCE_GROUP:"
        az resource list --resource-group $RESOURCE_GROUP --output table
        echo ""
    else
        print_success "Resource group $RESOURCE_GROUP has been completely removed"
    fi
}

# Main execution
main() {
    echo "=========================================="
    echo "  Azure VM Cleanup Script"
    echo "  Cognizant Summer Practice"
    echo "=========================================="
    echo ""
    
    # Check prerequisites
    check_azure_cli
    check_azure_login
    
    # Confirm deletion
    confirm_deletion
    
    # Remove resources
    remove_vm
    remove_network_resources
    remove_resource_group
    
    # List remaining resources
    list_remaining_resources
    
    echo ""
    echo "=========================================="
    echo "           CLEANUP COMPLETE              "
    echo "=========================================="
    echo ""
    print_success "All Azure resources have been removed or scheduled for deletion"
    echo ""
    echo "Note: Resource group deletion may take a few minutes to complete."
    echo "You can check the status with: az group show --name $RESOURCE_GROUP"
    echo ""
}

# Check if script is run with arguments
if [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
    echo "Usage: $0 [--help]"
    echo ""
    echo "This script removes all Azure resources created by the deployment script."
    echo ""
    echo "⚠️  WARNING: This action cannot be undone!"
    echo ""
    echo "The script will remove:"
    echo "  - Virtual Machine"
    echo "  - Network Interface"
    echo "  - Public IP"
    echo "  - Network Security Group"
    echo "  - Virtual Network"
    echo "  - Resource Group (and all resources within it)"
    echo ""
    echo "Prerequisites:"
    echo "  - Azure CLI installed and configured"
    echo "  - Azure subscription with sufficient permissions"
    echo ""
    exit 0
fi

# Run main function
main "$@"
