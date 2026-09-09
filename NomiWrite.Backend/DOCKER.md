# Docker Development

## Prerequisites

- Docker and Docker Compose (v2+)
- All Supabase databases must already exist and have EF Core migrations applied locally

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
