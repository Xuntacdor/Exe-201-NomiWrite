# Gateway smoke checklist

Run these checks from `NomiWrite.Backend` after the gateway and downstream services are running.

## Public entry point

- `GET http://localhost:5097/health` returns 200 from the gateway itself.
- `GET http://localhost:5097/api/writing/types` reaches Writing service.
- `GET http://localhost:5097/api/subscriptions/plans` reaches Subscription service.
- `POST http://localhost:5097/api/auth/login` reaches Auth service and returns the same contract as direct service access.

## Authenticated routing

- Send `Authorization: Bearer <access-token>` through the gateway.
- `GET http://localhost:5097/api/users/me` reaches User service.
- `GET http://localhost:5097/api/writing/submissions` reaches Writing service.
- `GET http://localhost:5097/api/notifications` reaches Notification service.

## Admin route ownership

- `/api/admin/users/*` -> Auth service.
- `/api/admin/prompts/*` and `/api/admin/submissions/*` -> Writing service.
- `/api/admin/payments/*` -> Payment service.
- `/api/admin/plans/*` and `/api/admin/subscriptions/*` -> Subscription service.
- `/api/admin/ai-config/*` -> AI Coordinator service.
- `/api/admin/analytics/*`, `/api/admin/announcements/*`, and `/api/moderation/reports/*` -> Admin service.

## Expected exposure

- Development compose intentionally publishes gateway plus service ports for Swagger/debug access.
- Production compose publishes only `80:8080` on the gateway; services and RabbitMQ remain internal.
- Swagger stays service-local. The gateway remains proxy-only and should not expose its own Swagger UI.
