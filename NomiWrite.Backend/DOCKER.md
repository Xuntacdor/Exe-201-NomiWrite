# Docker Development

## Prerequisites

- Docker and Docker Compose (v2+)
- All Supabase databases must already exist and have EF Core migrations applied locally

## Local Configuration

Per-service `appsettings.json` files are **git-ignored** because they hold real
credentials. Only sanitized `appsettings.Example.json` templates are tracked.
Copy them once after cloning (required for running a service directly with
`dotnet run`; containers ignore these files and take config from `.env*`):

```bash
cp src/Services/Auth/NomiWrite.Auth.API/appsettings.Example.json \
   src/Services/Auth/NomiWrite.Auth.API/appsettings.json
cp src/Services/Payment/NomiWrite.Payment.API/appsettings.Example.json \
   src/Services/Payment/NomiWrite.Payment.API/appsettings.json
# ...and fill in the <placeholder> values
```

Never commit a filled-in `appsettings.json`. If you previously used
`git update-index --skip-worktree` to hide one, drop the flag with
`git update-index --no-skip-worktree <path>` — it hides real changes from
`git status` and is how credentials end up committed by accident.

## Quick Start

```bash
# 1. Create your environment file
cp .env.example .env
#    Edit .env and fill in real values for every variable

# 2. Start all services
docker-compose up --build

# 3. Tail logs for a single service
docker-compose logs -f gateway
docker-compose logs -f auth-service
docker-compose logs -f user-service
docker-compose logs -f payment-service
docker-compose logs -f writing-service
docker-compose logs -f aicoordinator-service
docker-compose logs -f subscription-service
docker-compose logs -f rabbitmq
```

## Architecture

| Container             | Host Port | Purpose                                |
| --------------------- | --------- | -------------------------------------- |
| `gateway`             | 5097      | YARP reverse proxy — single entry point |
| `auth-service`        | 5100      | Authentication & JWT                   |
| `user-service`        | 5101      | User profiles                          |
| `payment-service`     | 5132      | Payment processing (VNPay/Momo)        |
| `writing-service`     | 5133      | Essay writing                          |
| `aicoordinator-service` | 5150    | AI grading (Gemini)                    |
| `subscription-service` | 5160     | Subscription management                |
| `rabbitmq`            | 5672/15672 | Message broker                        |

## Database

Databases live on **Supabase** and are **not** containerized. The 6 databases (`nomiwrite_auth`, `nomiwrite_user`, `nomiwrite_payment`, `nomiwrite_writing`, `nomiwrite_grading`, `nomiwrite_subscription`) must already exist and have their schema applied before running Docker.

## Runtime AI credentials

The Admin app can rotate the Gemini API key and switch the primary/fallback
models without rebuilding containers. API keys saved from Admin are encrypted
before being persisted. Configure a stable 32-byte encryption key once in the
deployment environment as `AiSecretProtection__MasterKey`.

Generate a value locally and place only the output in `.env.production` or the
deployment secret store (never commit the generated value):

```powershell
$bytes = New-Object byte[] 32
[Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[Convert]::ToBase64String($bytes)
```

Changing this master key makes previously stored API keys unreadable. Keep a
secure backup and rotate the Gemini key through Admin after any intentional
master-key rotation.

EF Core migrations continue to be run locally from the host machine:

```bash
dotnet ef database update \
  --project src/Services/<Service>/<Service>.Infrastructure \
  --startup-project src/Services/<Service>/<Service>.API \
  --context <DbContext>
```

Docker only runs the already-migrated application — migrations are a one-time schema operation, not a runtime concern.

## RabbitMQ

The RabbitMQ management UI is available at `http://localhost:15672` (default creds from your `.env`).

## VNPay / Momo IPN & ngrok

IPN callbacks need a public URL. When testing payments end-to-end with Docker running, ngrok should still point at the Gateway's host port:

```bash
ngrok http 5097
```

Update `VnPaySettings__IpnUrl` and `MomoSettings__IpnUrl` in `.env` each time the ngrok URL changes, then restart:

```bash
docker-compose up -d payment-service
```

This is the same workflow as local development — Docker does not change this requirement.

## Stopping

```bash
docker-compose down          # stop containers, keep volumes
docker-compose down -v       # stop containers AND delete RabbitMQ data
```
