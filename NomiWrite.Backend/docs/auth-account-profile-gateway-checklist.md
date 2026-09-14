# Auth, Account, And Profile Gateway Checklist

Use this checklist for Phase 3 smoke testing through the Gateway.

Gateway base URL:

- Local: `http://localhost:5000`
- Auth service routes: `/api/auth/*`
- User profile routes: `/api/users/*`

## JWT Contract

All protected backend services expect the Auth access token to keep these claims stable:

- `sub`: authenticated user id as a GUID string.
- `role`: application role, for example `Student`, `Admin`, or `Moderator`.
- `email`: authenticated email address.
- `full_name`: display name at token issuance time.

Each service configures `MapInboundClaims = false`, `NameClaimType = sub`, and
`RoleClaimType = role`, so controllers should read the user id with
`JwtRegisteredClaimNames.Sub`.

## Registration And Event Fan-Out

1. Register with `POST /api/auth/register`.
2. Confirm the response contains `userId`, `email`, `fullName`, `role`,
   `accessToken`, `refreshToken`, and `expiresAt`.
3. Confirm Auth publishes `UserRegisteredEvent` only for a new Auth user.
4. Confirm User consumes that event and creates exactly one profile with the
   registered full name as `displayName`.
5. Confirm Notification consumes that event and creates exactly one default
   `NotificationPreference` row with email, in-app, grading, and marketing
   alerts enabled.
6. Repeat the same event in a lower environment if possible; User profile and
   Notification preference creation must remain idempotent.

User and Notification must use separate service-prefixed MassTransit endpoint
names. If both services share one queue name, only one consumer will receive a
given registration event and the other side effect will be missing.

## Login, Refresh, Logout

1. Login with `POST /api/auth/login`.
2. Call a protected profile route, for example `GET /api/users/me`, with
   `Authorization: Bearer <accessToken>`.
3. Refresh with `POST /api/auth/refresh`; the old refresh token must be revoked
   and replaced.
4. Logout with `POST /api/auth/logout`; every active refresh token for the
   current user must be revoked.
5. Reusing a revoked refresh token must revoke the whole refresh-token family and
   return `401`.

## Google Login

1. Login with `POST /api/auth/google` using a valid Google ID token.
2. For a new email, Auth creates a verified Student account, stores `googleId`
   and `avatarUrl`, and publishes `UserRegisteredEvent`.
3. For an existing email, Auth links `googleId` if missing, backfills
   `avatarUrl` if empty, verifies the email, and does not publish another
   registration event.

## Account Status

Expected behavior:

- `Active`: can login, refresh, and call protected endpoints with a valid token.
- `Banned`: login and refresh return `403`; active refresh tokens are revoked
  when an admin changes the account to `Banned`.
- `Deactivated`: login and refresh return `403`; active refresh tokens are
  revoked when the user self-deactivates or an admin changes status to
  `Deactivated`.

Access tokens are stateless JWTs. A token already issued before a ban or
deactivation can remain accepted by downstream services until it expires, unless
the service adds online introspection or a shared account-status authorization
check. Current mitigation is refresh-token revocation plus short access-token
expiry.

## Profile Contract

Frontend profile screens can rely on these fields on `GET /api/users/me`,
`PUT /api/users/me`, and `GET /api/users/me/account`:

- `userId`
- `displayName`
- `avatarUrl`
- `bio`
- `targetExam`
- `targetBand`
- `targetExamDate`
- `englishLevel`

`GET /api/users/me/account` also includes subscription projection:

- `hasActiveSubscription`
- `subscriptionPlanName`
- `subscriptionEndDate`

`GET /api/users/me/progress` includes profile goals plus progress data:

- `targetExam`
- `targetBand`
- `targetExamDate`
- `bandHistory`
- `strengthsWeaknesses`
- `currentStreak`
- `totalSubmissions`
- `badges`
