#!/bin/bash

# Script pentru rulare backend pe AWS
echo "Starting backend on AWS..."

# Restaurează dependențele
dotnet restore

# Build aplicația
dotnet build

# Rulează aplicația pe HTTPS
echo "Starting backend on https://0.0.0.0:5087"
dotnet run --launch-profile https

echo "Backend started on AWS!"
