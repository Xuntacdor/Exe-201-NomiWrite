# NomiWrite — Database Backup & Restore

Production-ready PostgreSQL backup tooling for the NomiWrite microservice fleet.

- [`backup-db.sh`](backup-db.sh) — one gzip-compressed SQL dump per service database.
- [`restore-db.sh`](restore-db.sh) — safe, confirmation-gated restore.
- [`docker-compose.prod.yml`](../docker-compose.prod.yml) — optional backup sidecar service.

---

## 1. Discovered database architecture

**One separate PostgreSQL DATABASE per microservice — NOT shared schemas.**

| Database            | Service           | Docker service    |
|---------------------|-------------------|-------------------|
| `nomiwrite_auth`    | Auth              | `auth-service`    |
| `nomiwrite_user`    | User              | `user-service`    |
| `nomiwrite_payment` | Payment           | `payment-service` |
| `nomiwrite_writing` | Writing           | `writing-service` |
| `nomiwrite_grading` | AI Coordinator    | `aicoordinator-service` |
| `nomiwrite_subscription` | Subscription | `subscription-service` |
| `nomiwrite_notification` | Notification | `notification-service` |
| `nomiwrite_admin`   | Admin             | `admin-service`   |
| `nomiwrite_learning`| Learning          | `learning-service`|

PostgreSQL is **external (Supabase)** and is **not containerized**. There is NO
PostgreSQL container in any compose file — Compose cannot control or order an
external database, so services never `depends_on` a database.

Because databases are the unit of backup, `backup-db.sh` runs one `pg_dump`
per database above (space-separated `BACKUP_DB_NAMES`, defaults to all nine).

### Supabase connection: direct vs. pooler

- The application runtime may use the Supabase **session/transaction pooler**
  (PgBouncer, port **6543**). PgBouncer is unsuitable for `pg_dump`.
- **Backups MUST use the Supabase DIRECT connection** (host `db.<ref>.supabase.co`,
  port **5432**). `pg_dump` needs a session-level connection; the pooler can
  break it or silently fall back to direct anyway.
- This is why backup credentials are configured **separately**:

  - `BACKUP_DB_HOST`, `BACKUP_DB_PORT=5432`, `BACKUP_DB_USER`, `BACKUP_DB_PASSWORD`
  - (fallback names `DB_HOST` / `DB_PORT` / `DB_USER` / `DB_PASSWORD` are also accepted)

### Backup user

Use a dedicated PostgreSQL role that only needs the privileges required for
`pg_dump` of the relevant databases (on Supabase that is typically the
`postgres` role, since per-database roles are not self-serviced). Do **not**
grant superuser privileges to a regular backup role just to "make it work";
if Supabase limitations require broader access, document it and keep the role
scoped to read-only access on the nine databases.

---

## 2. Format, naming, location, retention

| Aspect       | Value                                                              |
|--------------|--------------------------------------------------------------------|
| Format       | Plain SQL (`pg_dump --format=plain`) piped into `gzip -9` → `.sql.gz` |
| Filename     | `<database>_backup_YYYYMMDD_HHMMSS.sql.gz` (e.g. `nomiwrite_auth_backup_20260912_020000.sql.gz`) |
| Location     | `BACKUP_DIR` (sidecar: mounted `/backups` volume; host cron: e.g. `/var/backups/nomiwrite`) |
| Retention    | `RETENTION_DAYS` (default `7`). Files matching `*_backup_*.sql.gz` older than N days are deleted. Unrelated files (`backup.log`, others' temp files) are never touched. |
| Permissions  | `umask 077` → backups are `0600`; directory must be `0700`/`0755` |

The script intentionally does **NOT** use `pg_dump -Fc` (custom format). If you
switch to `-Fc`, backups become `*.dump`/`*.backup` and restoration must use
`pg_restore` — do not mix `-Fc` with `.sql.gz`.

Integrity: each backup is validated via `gzip -t`, written to a `*.sql.gz.tmp`
file, and only then **atomically renamed** to the final name. A failed dump is
removed and never looks valid.

---

## 3. Deploying backups

### Option A — Native Linux cron (recommended for a dedicated host)

1. Make the scripts executable and restrict permissions:

   ```bash
   chmod 700 scripts/backup-db.sh scripts/restore-db.sh
   ```

2. Create a protected environment file (only readable by the backup user):

   ```bash
   install -m 600 /dev/null /etc/nomiwrite/backup.env
   # add the required variables:
   #   BACKUP_DB_HOST, BACKUP_DB_PORT=5432, BACKUP_DB_USER,
   #   BACKUP_DB_PASSWORD, BACKUP_DB_NAMES, BACKUP_DIR=</abs/path>,
   #   RETENTION_DAYS=7
   ```

3. Install the cron entry (edit with `crontab -e` for the backup user):

   ```cron
   0 2 * * * set -a; . /etc/nomiwrite/backup.env; set +a; /opt/nomiwrite/scripts/backup-db.sh >> /var/log/nomiwrite-backup.log 2>&1
   ```

   - **The password is never placed in the crontab or on the command line.**
     It is sourced from the `0600`-permission env file inside the cron job.
   - Symlink or place the scripts at an absolute path (`/opt/nomiwrite/scripts`).
   - `BACKUP_DIR` must be an absolute path.

4. Verify cron actually ran (do this monthly):

   ```bash
   grep -c SUCCESS /var/log/nomiwrite-backup.log
   ls -la /var/backups/nomiwrite/   # expect 9 fresh .sql.gz per night
   ```

### Timezone of the schedule

The `0 2 * * *` line runs at **02:00 in the server's configured timezone** —
cron does not translate timezones. All script log lines embed the timestamp
**with an explicit UTC offset** (`2026-09-12T02:00:00+07:00`), so there is no
ambiguity about when a backup ran.

- If the server is on `Asia/Ho_Chi_Minh` (UTC+7): 02:00 local.
- If the server runs in UTC: 02:00 UTC = **09:00 Vietnam time (ICT)**.

Check the host timezone with `timedatectl` (or `cat /etc/timezone`). To force
a timezone for cron jobs, prefix the job with `TZ=Asia/Ho_Chi_Minh`.

### Option B — Docker backup sidecar (`docker-compose.prod.yml`)

`docker-compose.prod.yml` ships a `backup-service`:

```
services:
  backup-service:
    image: postgres:17-alpine
    volumes:
      - ./scripts:/scripts:ro
      - nomiwrite-backups:/backups
    # runs backup-db.sh immediately, then every BACKUP_INTERVAL_SECONDS
```

- **Image choice**: the official `postgres:17-alpine` image. It ships
  `pg_dump`, `psql`, `pg_isready`, `gzip`, and the `flock` capability via
  busybox. It avoids a third-party image and keeps the client version current.
  Versions: `pg_dump` must be **≥ the server major version**. Supabase is
  currently on PostgreSQL 15–17; pin/raise `postgres:17-alpine` accordingly.
- **Security**: internal-only network, no published ports, mounts `./scripts`
  read-only, credentials come from `.env.production` via `env_file` (injected
  through `PGPASSWORD`, never CLI); backups land in the `nomiwrite-backups`
  volume with `0600` permissions.
- **Limitations**: the sidecar runs as root (the postgres image's default)
  so it can freshly mount `/backups`. It is an ops-only helper with no network
  exposure. A simple `sleep` loop is used instead of cron because the image
  does not ship cron — interval is `BACKUP_INTERVAL_SECONDS` (default 86400).
- **Off-host**: a Docker volume is NOT disaster recovery (see below).

---

## 4. Restore

```bash
BACKUP_DB_HOST=... BACKUP_DB_USER=... BACKUP_DB_PASSWORD=... \
./scripts/restore-db.sh \
  /backups/nomiwrite_auth_backup_20260912_020000.sql.gz \
  nomiwrite_auth
```

The script:

1. Requires a `.sql.gz` file (refuses `*.dump`/`*.backup`).
2. Validates `gzip -t` integrity.
3. Verifies connectivity to the target database.
4. Prints the **exact target database, host, and backup file**.
5. Requires you to type `RESTORE` (all caps) before any data is touched.
6. Restores with `ON_ERROR_STOP=1` so failures abort loudly.

Non-destructive validation (recommended before a real restore):

```bash
./scripts/restore-db.sh <backup.sql.gz> nomiwrite_auth_restore_test --check
```

> **Never restore against a production database without a current backup of
> what is about to be overwritten.** Prefer restoring into a throwaway
> database first to validate the dump.

---

## 5. Backup verification procedure (monthly)

1. **Connectivity**: `pg_isready -h "$BACKUP_DB_HOST" -p 5432 -U "$BACKUP_DB_USER"`
2. **Dump succeeds**: run `backup-db.sh` manually; expect `SUCCESS` for all 9 DBs.
3. **Files non-empty**: `find $BACKUP_DIR -name '*_backup_*.sql.gz' -size 0` must return nothing.
4. **gzip integrity**: `for f in $BACKUP_DIR/*.sql.gz; do gzip -t "$f" || echo FAIL:$f; done`
5. **Retention works**: confirm `-mtime +$RETENTION_DAYS` deletes only matching files.
6. **Restore works**: `restore-db.sh <file> nomiwrite_auth_restore_test` (restore into a test DB), then spot-check row counts.
7. **Disaster drill**: restore the latest full set into ephemeral databases and compare `\dt` counts with production.

To verify a restore without a real target, use `--check`.

---

## 6. Disaster recovery — be honest about the limits

| Level        | What it protects against                            |
|--------------|-----------------------------------------------------|
| Local backup | Accidental `DELETE`/corruption — if the .sql.gz survived |
| Off-host copy| Server disk failure, host loss, ransomware          |
| True DR      | Loss of the whole region / infrastructure failure   |

A local Docker volume and a cron job on the same host do **NOT** protect
against host loss. Recommended next step: push nightly backups to an external
destination — for example:

- S3-compatible object storage (`s3cmd`/`aws s3 cp`, or a purpose-built tool),
- a second server,
- Supabase PITR (Supabase already offers point-in-time recovery — enable it as
  the primary DR mechanism and use these dumps as an independent export).

Cloud credentials are intentionally out of scope here — wire them up in the
backup pipeline only after the base restore path is proven.

---

## 7. Troubleshooting

| Symptom                                           | Fix                                                        |
|---------------------------------------------------|-------------------------------------------------------------|
| `pg_dump: error: connection to server ... refused` | Wrong `BACKUP_DB_HOST`/port; pooler endpoint selected — use `db.<ref>.supabase.co:5432`. |
| `server version (17) is newer than client`        | Bump the `postgres:` image tag (or host `pg_dump`) to ≥ server major version. |
| `password authentication failed`                  | `BACKUP_DB_USER`/`BACKUP_DB_PASSWORD` mismatch for the DIRECT endpoint. |
| No `SUCCESS` lines in `backup.log`                | Check `backup.log` and the cron log; the script exits 1 on any failure and continues on per-DB errors. |
| `flock` warning                                   | Tool missing in the environment; install `util-linux` or run in the sidecar. |
| File `0600` too restrictive for a shared user     | Deliberate; adjust with a `chmod` in the pipeline, keep `600` for security. |