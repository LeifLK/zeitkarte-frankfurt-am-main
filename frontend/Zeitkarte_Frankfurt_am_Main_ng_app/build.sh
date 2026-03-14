#!/bin/bash
set -e
sed -i "s|API_KEY_PLACEHOLDER|$API_KEY|g" src/environments/environment.ts
ng build --configuration production