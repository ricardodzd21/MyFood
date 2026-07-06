#!/bin/bash
# ============================================================
# Primeira instalação do MyFood no servidor. Roda 1 vez.
# Depois use update.sh ou o GitHub Actions (push na main).
#
# Pré-requisitos no servidor (já instalados p/ os outros projetos):
#   Docker + docker compose, Nginx, Certbot, Git
# ============================================================

set -e

# ===== ajuste estes valores =====
DOMAIN="${DOMAIN:-CHANGE_ME.com}"
APP_DIR="${APP_DIR:-/www/wwwroot/myfood}"
REPO_URL="${REPO_URL:-git@github.com:ricardodzd21/MyFood.git}"
EMAIL="${EMAIL:-ricardodzd21@gmail.com}"

if [ "$DOMAIN" = "CHANGE_ME.com" ]; then
  echo "❌ Defina DOMAIN antes de rodar. Ex:"
  echo "   DOMAIN=meufood.com ./deploy/setup.sh"
  exit 1
fi

echo "============================================"
echo "MyFood setup"
echo "  Domínio: $DOMAIN"
echo "  App dir: $APP_DIR"
echo "  Repo:    $REPO_URL"
echo "============================================"

# 1. Clonar repo
if [ ! -d "$APP_DIR/.git" ]; then
  echo "[1/6] Clonando repo..."
  mkdir -p "$(dirname "$APP_DIR")"
  git clone "$REPO_URL" "$APP_DIR"
else
  echo "[1/6] Repo já clonado, fazendo pull..."
  cd "$APP_DIR" && git pull
fi

cd "$APP_DIR"

# 2. server.env
if [ ! -f "$APP_DIR/server.env" ]; then
  echo "[2/6] Criando server.env a partir do template..."
  cp server.env.example server.env
  echo "  ⚠️  Edite server.env: senha do Postgres, JwtSecret, Admin e (opcional) Gemini__ApiKey."
  echo "  Pressione ENTER quando tiver editado..."
  read
else
  echo "[2/6] server.env já existe, pulando."
fi

# 3. Banco: garante que o database MyFood existe no Postgres do host
echo "[3/6] Garantindo database MyFood no Postgres..."
docker exec -i "$(docker ps --filter ancestor=postgres --format '{{.Names}}' | head -1)" \
  psql -U postgres -tc "SELECT 1 FROM pg_database WHERE datname='MyFood'" | grep -q 1 || \
  docker exec -i "$(docker ps --filter ancestor=postgres --format '{{.Names}}' | head -1)" \
  psql -U postgres -c "CREATE DATABASE \"MyFood\"" || \
  echo "  (crie o database MyFood manualmente se o comando acima falhar)"

# 4. Nginx do host + Let's Encrypt
echo "[4/6] Configurando nginx (bloco HTTP temporário p/ Certbot)..."
NGINX_CONF="/etc/nginx/sites-available/myfood"
mkdir -p /var/www/certbot
cat > "$NGINX_CONF" <<EOF
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    location /.well-known/acme-challenge/ { root /var/www/certbot; }
    location / { return 200 'Setup em andamento'; }
}
EOF
ln -sf "$NGINX_CONF" /etc/nginx/sites-enabled/myfood
nginx -t && systemctl reload nginx

if [ ! -d "/etc/letsencrypt/live/$DOMAIN" ]; then
  echo "  Gerando certificado SSL..."
  certbot certonly --webroot -w /var/www/certbot \
    -d "$DOMAIN" -d "www.$DOMAIN" \
    --email "$EMAIL" --agree-tos --non-interactive
fi

# 5. Nginx final (HTTPS)
echo "[5/6] Aplicando nginx final (HTTPS)..."
sed "s/__DOMAIN__/$DOMAIN/g" deploy/nginx.conf > "$NGINX_CONF"
nginx -t && systemctl reload nginx

# 6. Build + up
echo "[6/6] Build e up dos containers..."
docker compose build --no-cache
docker compose up -d

echo ""
echo "============================================"
echo "✅ MyFood no ar!"
echo "  Site:   https://$DOMAIN"
echo "  Logs:   docker compose logs -f"
echo "  Status: docker compose ps"
echo "============================================"
