#!/bin/bash

# Configuration for VPN
VPN_SERVER="vdi.fhict.nl"
VPN_USER="i472595"
VPN_PASSWORD="Eqp40XwOw5"
APP_COMMAND="dotnet FileStorage.dll"

# Function to establish VPN connection
connect_vpn() {
  echo "Starting VPN connection to $VPN_SERVER..."
  echo "$VPN_PASSWORD" | openconnect --user="$VPN_USER" "$VPN_SERVER" --passwd-on-stdin --background
}

# Function to check VPN status
check_vpn() {
  ip route | grep -q "tun0"  # Check if the VPN tunnel is active
}

# Connect to VPN
connect_vpn

# Wait until VPN is connected
until check_vpn; do
  echo "Waiting for VPN connection..."
  sleep 5
done

echo "VPN connected successfully."

# Start the application
echo "Starting the application..."
$APP_COMMAND &

# Monitor VPN and reconnect if needed
while true; do
  if ! check_vpn; then
    echo "VPN disconnected. Reconnecting..."
    connect_vpn
  fi
  sleep 5
done
