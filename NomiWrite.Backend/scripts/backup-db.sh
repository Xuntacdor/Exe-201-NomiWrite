#!/usr/bin/env bash
#
# NomiWrite — PostgreSQL backup script (one gzip-compressed SQL dump per DB).
#
# Architecture: each NomiWrite microservice uses its OWN separate PostgreSQL
# DATABASE on Supabase (nomiwrite_auth, nomiwrite_user, ...) — NOT schemas.
# This script runs one `pg_dump` per database. Use the Supabase DIRECT
# endpoint (port 5432), NOT the PgBouncer/pooler endpoint (port 6543).
#
# Security:
#   * The password is passed via the PGPASSWORD environment variable and is
#     NEVER placed on the command line (no -W, no connection-string argument),
#     so it cannot be observed with `ps aux`.
#   * The password is never written to logs, filenames, or error output.
#
# Usage:
#   BACKUP_DB_HOST=... BACKUP_DB_USER=... BACKUP_DB_PASSWORD=... ./backup-db.sh
#
# Optional environment (prefixed BACKUP_DB_* take precedence over DB_*):
#   BACKUP_DB_HOST / DB_HOST        Supabase direct host (default: none)
#   BACKUP_DB_PORT / DB_PORT        default 5432 (direct). Keep 6543 (pooler)
#                                    OFF unless you verified pg_dump works.
#   BACKUP_DB_USER / DB_USER        default: postgres (or dedicated backup role)
#   BACKUP_DB_PASSWORD / DB_PASSWORD
#   BACKUP_DB_NAMES / DB_NAMES      space-separated database list (defaults to
#                                    all 9 NomiWrite databases)
#   BACKUP_DIR                      destination dir (default: /backups)
#   RETENTION_DAYS                  delete *.sql.gz older than N days (default 7)
#   BACKUP_LOG_FILE                 log path (default: $BACKUP_DIR/backup.log)
#   BACKUP_LOCK_FILE                lock file for concurrency (default /tmp/...)
#
set -Eeuo pipefail

# ── Configuration (environment) ───────────────────────────────────────────────
BACKUP_HOST="${BACKUP_DB_HOST:-${DB_HOST:-}}"
BACKUP_PORT="${BACKUP_DB_PORT:-${DB_PORT:-5432}}"
BACKUP_USER="${BACKUP_DB_USER:-${DB_USER:-}}"
BACKUP_PASSWORD="${BACKUP_DB_PASSWORD:-${DB_PASSWORD:-}}"
BACKUP_DIR="${BACKUP_DIR:-/backups}"
RETENTION_DAYS="${RETENTION_DAYS:-7}"
BACKUP_LOG_FILE="${BACKUP_LOG_FILE:-${BACKUP_DIR}/backup.log}"
BACKUP_LOCK_FILE="${BACKUP_LOCK_FILE:-/tmp/nomiwrite-backup.lock}" # container /tmp is writable

DEFAULT_DB_NAMES="nomiwrite_auth nomiwrite_user nomiwrite_payment nomiwrite_writing nomiwrite_grading nomiwrite_subscription nomiwrite_notification nomiwrite_admin nomiwrite_learning"
DB_NAMES="${BACKUP_DB_NAMES:-${DB_NAMES:-${DEFAULT_DB_NAMES}}}"

# ── Validation ────────────────────────────────────────────────────────────────
fail=0
for var in BACKUP_HOST BACKUP_USER BACKUP_PASSWORD; do
    if [[ -z "${!var:-}" ]]; then
        echo "ERROR: ${var} is required but is not set." >&2
        fail=1
    fi
done
if [[ -z "$BACKUP_DIR" || ! -d "$BACKUP_DIR" ]]; then
    echo "ERROR: BACKUP_DIR ('$BACKUP_DIR') is not an existing directory." >&2
    fail=1
fi
if ! [[ "$RETENTION_DAYS" =~ ^[0-9]+$ ]]; then
    echo "ERROR: RETENTION_DAYS must be a non-negative integer, got '$RETENTION_DAYS'." >&2
    fail=1
fi
if [[ $fail -eq 1 ]]; then
    echo "Set the required variables (see header of this script)." >&2
    exit 1
fi

# ── Tool availability ──────────────────────────────────────────────────────────
for tool in pg_dump gzip date; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "ERROR: required tool '$tool' not found in PATH." >&2
        exit 1
    fi
done

# ── Logging (timestamps always include the UTC offset) ──────────────────────
now() { date '+%Y-%m-%dT%H:%M:%S%z'; }
log() { printf '[%s] %s\n' "$(now)" "$*" | tee -a "$BACKUP_LOG_FILE"; }

# ── Password sanitisation (defence-in-depth: even if pg_dump leaked the
#    password, it would be redacted before any log output) ───────────────────
esc_regex() { sed -e 's/[\/&]/\\&/g' <<< "$1"; }
PASS_ESC="$(esc_regex "$BACKUP_PASSWORD")"
sanitize() { sed "s/${PASS_ESC}/***REDACTED***/g"; }

# ── Concurrency lock ──────────────────────────────────────────────────────────
export PGPASSWORD="$BACKUP_PASSWORD"
export PGSSLMODE="${PGSSLMODE:-require}"
export PGCONNECT_TIMEOUT="${PGCONNECT_TIMEOUT:-15}"
umask 077

if command -v flock >/dev/null 2>&1; then
    exec 9>"$BACKUP_LOCK_FILE"
    if ! flock -n 9; then
        log "ERROR: another backup job is already running (lock held: $BACKUP_LOCK_FILE). Exiting."
        exit 0
    fi
else
    log "WARNING: 'flock' not found; concurrency protection is disabled."
fi

# ── Connectivity pre-check (optional but informative) ────────────────────────
if command -v pg_isready >/dev/null 2>&1; then
    pg_isready -h "$BACKUP_HOST" -p "$BACKUP_PORT" -U "$BACKUP_USER" \
        >>"$BACKUP_LOG_FILE" 2>/dev/null \
        || log "WARNING: pg_isready reported the server as not ready (host=$BACKUP_HOST port=$BACKUP_PORT); continuing anyway (pg_dump is the real test)."
fi

overall=0
log "Starting backup (host=$BACKUP_HOST port=$BACKUP_PORT user=$BACKUP_USER databases=[$DB_NAMES])"

# ── Per-database dump ─────────────────────────────────────────────────────────
for db in $DB_NAMES; do
    [[ -z "$db" ]] && continue
    stamp="$(date '+%Y%m%d_%H%M%S')"
    final="${BACKUP_DIR}/${db}_backup_${stamp}.sql.gz"
    tmp="${final}.tmp"          # atomic write target
    err="${final}.err.log"      # pg_dump stderr (temporary, removed on success)
    rm -f "$tmp" "$err"

    if ! pg_dump \
            -h "$BACKUP_HOST" -p "$BACKUP_PORT" -U "$BACKUP_USER" \
            -d "$db" \
            --format=plain \
            --no-owner --no-privileges \
            2>"$err" \
        | gzip -9 > "$tmp"; then
        log "ERROR: pg_dump failed for '${db}' (details sanitized below):"
        [[ -s "$err" ]] && sed 's/^/         /' "$err" | sanitize | tee -a "$BACKUP_LOG_FILE" || true
        rm -f "$tmp" "$err"
        overall=1
        continue
    fi

    # ── Integrity: must exist, be non-empty, and pass gzip validation ──────
    if [[ ! -s "$tmp" ]]; then
        log "ERROR: backup file for '${db}' is empty or missing."
        rm -f "$tmp" "$err"
        overall=1
        continue
    fi
    if ! gzip -t "$tmp" 2>"$err"; then
        log "ERROR: gzip integrity check failed for '${db}'."
        [[ -s "$err" ]] && sed 's/^/         /' "$err" | sanitize | tee -a "$BACKUP_LOG_FILE" || true
        rm -f "$tmp" "$err"
        overall=1
        continue
    fi

    # ── Atomic rename; previous valid backups are never clobbered ───────────
    if mv -f "$tmp" "$final"; then
        log "SUCCESS: ${final} ($(du -h "$final" | cut -f1))"
        rm -f "$err"
    else
        log "ERROR: could not move temporary backup into place for '${db}'."
        rm -f "$tmp" "$err"
        overall=1
    fi
done

# ── Retention: delete only files matching the backup naming pattern ─────────
if [[ "$RETENTION_DAYS" -gt 0 ]]; then
    before="$(find "$BACKUP_DIR" -maxdepth 1 -type f -name '*_backup_*.sql.gz' | wc -l)"
    find "$BACKUP_DIR" -maxdepth 1 -type f -name '*_backup_*.sql.gz' \
        -mtime +"$RETENTION_DAYS" -delete
    after="$(find "$BACKUP_DIR" -maxdepth 1 -type f -name '*_backup_*.sql.gz' | wc -l)"
    log "Retention (${RETENTION_DAYS}d): removed $((before - after)) old backup(s); kept ${after}."
fi

# ── Clean stale temporary files from interrupted runs (only our pattern) ────
find "$BACKUP_DIR" -maxdepth 1 -type f \( -name '*.sql.gz.tmp' -o -name '*.sql.gz.err.log' \) -mtime +1 -delete

if [[ $overall -eq 0 ]]; then
    log "Backup completed successfully."
    exit 0
else
    log "Backup completed with errors."
    exit 1
fi