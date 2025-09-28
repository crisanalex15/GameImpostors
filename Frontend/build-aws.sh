#!/bin/bash

# Script pentru build frontend pe AWS
echo "Building frontend for AWS..."

# Instalează dependențele
npm install

# Build pentru producție
npm run build

echo "Frontend build completed!"
echo "Dist folder created in ./dist"
echo "You can serve it with: npx serve -s dist -l 5173"
