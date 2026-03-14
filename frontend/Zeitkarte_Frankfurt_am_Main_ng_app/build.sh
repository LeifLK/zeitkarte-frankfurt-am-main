#!/bin/bash
set -e

# Replace placeholder with the real key from Cloudflare's environment
sed -i "s|API_KEY_PLACEHOLDER|$API_KEY|g" src/environments/environment.prod.ts

# Build the app
ng build --configuration production