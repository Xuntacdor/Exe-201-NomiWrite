#!/usr/bin/env bash
#
# NomiWrite — PostgreSQL restore script (for .sql.gz plain-SQL backups
# produced by scripts/backup-db.sh).
#
#   ./restore-db.sh <backup.sql.gz> <target_database> [--check]
#
#   --check   NON-DESTRUCTIVE validation only: verifies gzip integrity,
#             database connectivity, and prints the target's current table
#             count. Does NOT touch data.
#
# Security:
#   * Password is passed via PGPASSWORD (never on the command line).
#   * A directive BEFORE any data is touched: the exact target database and
#     host are printed and the operator must type RESTORE to continue.
#
set -Eeuo pipefail

# ── Connection (same variable scheme as backup-db.sh) ─────────────────────────
BACKUP_HOST="${BACKUP_DB_HOST:-${DB_HOST:-}}"
BACKUP_PORT="${BACKUP_DB_PORT:-${DB_PORT:-5432}}"
BACKUP_USER="${BACKUP_DB_USER:-${DB_USER:-}}"
BACKUP_PASSWORD="${BACKUP_DB_PASSWORD:-${DB_PASSWORD:-}}"

usage() {
    cat >&2 <<'EOF'
Usage:
  ./restore-db.sh <backup.sql.gz> <target_database> [--check]

Examples:
  ./restore-db.sh /backups/nomiwrite_auth_backup_20260912_020000.sql.gz nomiwrite_auth
  ./restore-db.sh /backups/nomiwrite_auth_backup_20260912_020000.sql.gz nomiwrite_auth_restore_test --check
EOF
    exit 2
}

if [[ $# -lt 2 || $# -gt 3 ]]; then
    usage
fi
BACKUP_FILE="$1"
TARGET_DB="$2"
MODE="${3:-restore}"
if [[ "$MODE" != "restore" && "$MODE" != "--check" ]]; then
    usage
fi

# ── Preconditions ──────────────────────────────────────────────────────────────
fail=0
for var in BACKUP_HOST BACKUP_USER BACKUP_PASSWORD; do
    if [[ -z "${!var:-}" ]]; then
        echo "ERROR: ${var} is required but is not set." >&2
        fail=1
    fi
done
if [[ ! -f "$BACKUP_FILE" ]]; then
    echo "ERROR: backup file not found: $BACKUP_FILE" >&2
    fail=1
fi
case "$BACKUP_FILE" in
    *.sql.gz)
        ;;
    *.dump|*.backup|*)
        echo "ERROR: expected a gzip-compressed SQL backup (*.sql.gz)." >&2
        echo "       This project uses plain SQL + gzip (NOT pg_dump -Fc)." >&2
        fail=1
        ;;
esac
if [[ -z "$TARGET_DB" ]]; then
    echo "ERROR: no target database given." >&2
    fail=1
fi
if [[ $fail -eq 1 ]]; then
    usage
fi

for tool in psql gzip; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "ERROR: required tool '$tool' not found in PATH." >&2
        exit 1
    fi
done

export PGPASSWORD="$BACKUP_PASSWORD"
export PGSSLMODE="${PGSSLMODE:-require}"
export PGCONNECT_TIMEOUT="${PGCONNECT_TIMEOUT:-15}"

now() { date '+%Y-%m-%dT%H:%M:%S%z'; }
log() { printf '[%s] %s\n' "$(now)" "$*"; }

# ── Step 1: gzip integrity ─────────────────────────────────────────────────────
log "Validating gzip integrity of $BACKUP_FILE ..."
if ! gzip -t "$BACKUP_FILE"; then
    echo "ERROR: backup file failed gzip integrity check." >&2
    exit 1
fi
log "gzip integrity OK."

# ── Step 2: connectivity (read-only) ───────────────────────────────────────────
if ! psql -h "$BACKUP_HOST" -p "$BACKUP_PORT" -U "$BACKUP_USER" \
        -d "$TARGET_DB" -tAc 'SELECT 1' >/dev/null 2>&1; then
    echo "ERROR: cannot connect to database '$TARGET_DB' on $BACKUP_HOST:$BACKUP_PORT." >&2
    exit 1
fi
log "Connected to '$TARGET_DB' on $BACKUP_HOST:$BACKUP_PORT."

if [[ "$MODE" == "--check" ]]; then
    table_count="$(psql -h "$BACKUP_HOST" -p "$BACKUP_PORT" -U "$BACKUP_USER" \
        -d "$TARGET_DB" -tAc \
        "SELECT count(*) FROM information_schema.tables WHERE table_schema='public'")"
    log "Non-destructive check passed (no data touched). Current table count in '$TARGET_DB': ${table_count:-0}."
    log "To actually restore, run this script without --check."
    exit 0
fi

# ── Step 3: explicit confirmation ──────────────────────────────────────────────
cat <<EOF

⚠ THIS OPERATION MAY MODIFY OR OVERWRITE DATA.

  Backup file : $BACKUP_FILE
  Target DB   : $TARGET_DB
  Host        : $BACKUP_HOST:$BACKUP_PORT
  User        : $BACKUP_USER

Existing objects/tables in '$TARGET_DB' may be overwritten or fail to restore
if they conflict with the backup contents. This is DESTRUCTIVE.

EOF
read -r -p "Type RESTORE (all caps) to continue, anything else to abort: " answer
if [[ "$answer" != "RESTORE" ]]; then
    echo "Aborted. No changes were made." >&2
    exit 1
fi

# ── Step 4: restore (ON_ERROR_STOP makes failures fail loudly) ─────────────────
log "Restoring $BACKUP_FILE into '$TARGET_DB' ..."
if gunzip -c "$BACKUP_FILE" \
    | psql -h "$BACKUP_HOST" -p "$BACKUP_PORT" -U "$BACKUP_USER" \
        -d "$TARGET_DB" -v ON_ERROR_STOP=1 --quiet; then
    log "Restore completed successfully into '$TARGET_DB'."
    log "Sanity check: psql -d '$TARGET_DB' -c '\\dt'"
else
    echo "ERROR: restore failed. See output above." >&2
    exit 1
fi