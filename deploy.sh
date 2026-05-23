#!/bin/bash
set -e

cd /home/bsedwards/apps/matted-labs-uptime

git pull

docker compose up -d --build

echo "Deploy complete. API: http://localhost:5100  Web: http://localhost:4100"
