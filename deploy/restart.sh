#!/bin/bash
# Restart rápido sem rebuild. Use após mexer no server.env.
# Rode no servidor: cd /www/wwwroot/myfood && ./deploy/restart.sh

set -e
APP_DIR="${APP_DIR:-/www/wwwroot/myfood}"
cd "$APP_DIR"

docker compose down
docker compose up -d
echo "✅ Restart"
docker compose ps
