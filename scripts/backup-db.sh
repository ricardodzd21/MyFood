#!/bin/bash
# Backup diário do banco MyFood (Postgres do host Docker).
# Uso no servidor:
#   chmod +x scripts/backup-db.sh
#   ./scripts/backup-db.sh
# Cron (diário às 3h):
#   0 3 * * * /www/wwwroot/myfood/scripts/backup-db.sh >> /var/log/myfood-backup.log 2>&1

set -e

DB_NAME="${DB_NAME:-MyFood}"
DB_USER="${DB_USER:-postgres}"
BACKUP_DIR="${BACKUP_DIR:-/www/backup/myfood}"
KEEP_DAYS="${KEEP_DAYS:-14}"

# Nome do container do Postgres (mesmo dos outros projetos)
PG_CONTAINER="${PG_CONTAINER:-$(docker ps --filter ancestor=postgres --format '{{.Names}}' | head -1)}"
if [ -z "$PG_CONTAINER" ]; then
  PG_CONTAINER="$(docker ps --format '{{.Names}}' | grep -i postgres | head -1)"
fi
if [ -z "$PG_CONTAINER" ]; then
  echo "❌ Container do Postgres não encontrado. Defina PG_CONTAINER=nome"
  exit 1
fi

mkdir -p "$BACKUP_DIR"
STAMP="$(date +%Y%m%d-%H%M%S)"
FILE="$BACKUP_DIR/myfood-$STAMP.sql.gz"

echo "[backup] $DB_NAME via container $PG_CONTAINER -> $FILE"
docker exec "$PG_CONTAINER" pg_dump -U "$DB_USER" "$DB_NAME" | gzip > "$FILE"

# Remove backups antigos
find "$BACKUP_DIR" -name 'myfood-*.sql.gz' -mtime +"$KEEP_DAYS" -delete 2>/dev/null || true

echo "[backup] OK ($(du -h "$FILE" | cut -f1))"
