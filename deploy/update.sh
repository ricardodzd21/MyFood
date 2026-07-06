#!/bin/bash
# Atualiza o MyFood: git pull + rebuild + restart.
# Rode no servidor: cd /www/wwwroot/myfood && ./deploy/update.sh

set -e
APP_DIR="${APP_DIR:-/www/wwwroot/myfood}"
cd "$APP_DIR"

echo "[1/3] git pull..."
git pull

echo "[2/3] docker compose build..."
docker compose build --no-cache

echo "[3/3] docker compose up..."
docker compose up -d

echo ""
echo "✅ Atualizado"
docker compose ps
