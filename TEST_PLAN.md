# NomiWrite — Comprehensive Test Plan (Draft for Review)

> **Status:** Planning phase only. No test code has been written. All findings below are derived from direct inspection of the repository (`NomiWrite.Backend/`, `NomiWrite.Frontend/`, docker files, scripts). Anything marked **⚠️ needs confirmation** is an open question the team must answer before or during execution.

---

## 1. Findings — System Architecture Summary (Mandatory Survey)

### 1.1 Repository layout
| Area | Location | Notes |
|---|---|---|
| Backend solution | `NomiWrite.Backend/NomiWrite.sln` | **38 projects** (9 services × 4 clean-architecture layers + `NomiWrite.Shared.Contracts` + `NomiWrite.Gateway`) |
| Frontend | `NomiWrite.Frontend/` | Next.js **16.2.9**, React 19, App Router, Tailwind v4, no UI/state/form libraries |
| Infra | `NomiWrite.Backend/docker-compose.yml`, `docker-compose.prod.yml` | Dev (10 svc) vs Prod (12 svc) |
| Scripts | `NomiWrite.Backend/scripts/backup-db.sh`, `restore-db.sh`, `README.md` | Production-grade pg_dump/pg_restore workflow |

### 1.2 Microservices (all .NET 8, all identical auth + healthcheck template)
| Service | Port (dev) | Route prefix | Key DB (one per service, Supabase/Npgsql) |
|---|---|---|---|
| Gateway (YARP) | 5097 (host) / 8080 | `/api/**` reverse proxy | none |
| Auth | 5100 | `/api/auth` | AuthDb |
| User | 5101 | `/api/users` | UserDb |
| Payment | 5132 | `/api/payment` | PaymentDb |
| Writing | 5133 | `/api/writing` | WritingDb |
| AICoordinator | 5150 | `/api/grading` | GradingDb |
| Subscription | 5160 | `/api/subscriptions` | SubscriptionDb |
| Notification | 5170 | `/api/notifications` | NotificationDb |
| Admin | 5180 | `/api/admin/*`, `/api/moderation/*` | AdminDb |
| Learning | 5181 | `/api/quizzes`, `/api/vocabulary` | LearningDb |

**Existing test projects:** ❌ **Does not exist** — no `*.Tests`, `*.UnitTests`, or `*.IntegrationTests` project anywhere in the solution; no xUnit/NUnit/MSTest/coverlet reference in any `.csproj`. **Propose creating new test projects** (see §4 tooling).

**Code coverage reports:** ❌ **Does not exist** — no coverage tooling, no reports, no CI gates.

### 1.3 Service-to-service communication
- **HTTP (via Gateway, only ingress):** 9 YARP clusters mapped by URL prefix. Admin → 4 upstreams; User → Subscription/Writing/AICoordinator; Writing → Subscription; AICoordinator → Subscription; Payment → Subscription (promo validation). All inter-service `HttpClient` calls use **3s timeouts** and forward the caller's Bearer token, most **fail-soft** (default to "no subscription"/empty).
- **RabbitMQ via MassTransit 8** (no raw client). **Auto-named topology** — no explicit exchanges/queues/routing keys, no DLQ, no `UseMessageRetry`/`UseDelayedRedelivery`. A failing consume goes to MassTransit's default `_error` queue behavior only.

| Event | Publisher | Consumers |
|---|---|---|
| `UserRegisteredEvent` | Auth | User, Notification |
| `WritingSubmittedEvent` | Writing | AICoordinator → grades |
| `GradingCompletedEvent` | AICoordinator | Notification, Learning (persists vocab+grammar), Writing (sets GradedAt) |
| `PaymentCompletedEvent` | Payment | Subscription (activates plan) |
| `PaymentFailedEvent` | Payment | **none** ⚠️ |
| `PaymentCreatedEvent` | Payment | **none** ⚠️ |
| `SubscriptionActivatedEvent` | Subscription | **none** ⚠️ |
| `SubscriptionExpiringEvent` / `SubscriptionExpiredEvent` | Subscription sweep job | Notification |
| `SystemAnnouncementCreatedEvent` | Admin | Notification |
| `ForumCommentCreatedEvent` / `PostLikedEvent` | **no publisher** (no Forum service) | Notification (dead contacts — excluded per scope) |
| `UserLoggedInEvent` | **no publisher** | **none** (dead contract) |

**⚠️ needs confirmation:** whether the 4 orphan events are intentional (roadmap) or dead code to remove. Not in testing scope either way, but affects queue-attrition assertions.

### 1.4 External integration points (mock/sandbox requirements)
| Integration | Service | Mechanism | Test approach |
|---|---|---|---|
| **Google Gemini** (grading) | AICoordinator `GeminiGradingProvider` | direct HTTP `POST {endpoint}` to `generativelanguage.googleapis.com/v1beta/{model}:generateContent`, `x-goog-api-key` header, JSON response schema. **No SDK.** ⚠️ **HttpClient timeout not configured** → .NET default 100s (Learning quiz provider is 60s). | Mock HTTP via WireMock; live contract tests against Gemini are gated/expensive |
| **Google Gemini** (quiz gen) | Learning `GeminiQuizProvider` | direct HTTP via named client `"Gemini"`, 60s timeout. Fallback: `DeterministicQuizProvider` (pure local). | Mock HTTP; unit-test fallback entirely offline |
| **VNPay** | Payment `VnPayGatewayService` | HMAC-SHA512, v2.1.0, sandbox URL in config. **⚠️ not the active `IPaymentGatewayService`** | VNPay sandbox account + mock signature tests |
| **MoMo** | Payment `MomoGatewayService` | HMAC sign, test gateway. **⚠️ not the active `IPaymentGatewayService`** | MoMo test account + mock |
| **VietQR** | Payment enum `VietQR = 2` | **⚠️ no gateway service implementation exists** — only enum + frontend support | not yet testable end-to-end; see §2.2 |
| **Google OAuth** | Auth `GoogleLoginAsync` | `Google.Apis.Auth` SDK validates ID token | unit mock `GoogleJsonWebSignature`; dev fake tokens |
| **SMTP (Gmail app password)** | Auth `SmtpEmailSender` via MailKit | outbound email | Mailpit/Inbucket in test env, never real inboxes |
| **PostgreSQL/Supabase** | all services | Npgsql, EF Core, retry×3 | ephemeral Postgres containers per CI run |

**⚠️ needs confirmation:** The DI graph (`Payment.Infrastructure/DependencyInjection.cs`) registers `MockedPaymentGatewayService` as **the** `IPaymentGatewayService` (dev shim with TODOs). The concrete VNPay/MoMo gateway classes exist but are **not wired into `PaymentService`** today. Integration tests must decide whether to (a) test the real flow against sandbox after wiring, or (b) test the current mock-first flow and defer sandbox conformance. Recommend (a) with a feature flag.

### 1.5 Authentication & endpoint protection
- **JWT Bearer everywhere**, shared HMAC-SHA256 secret (issuer `NomiWriteAuthService`, audience `NomiWriteClients`, 5-min clock skew). Access token + opaque refresh token (7 d), email-verify token (24 h), reset token (1 h).
- **Refresh rotation:** `RefreshTokenAsync` revokes used token and, on reuse of a revoked token (`IsRevoked`), **revokes the whole token family** (`RevokeTokenFamilyAsync`).
- Controllers: `AuthController` public endpoints `AllowAnonymous` (register/login/refresh/verify/forgot/reset/google); rest `[Authorize]`. `AdminController` `[Authorize(Roles="Admin")]`. Same pattern across services: public = plans, promo validate, payment webhooks + `vnpay/ipn`/`momo/ipn` (`AllowAnonymous`); everything else `[Authorize]`; admin namespaces role-gated. **Gateway has no auth logic** — it forwards headers untouched.
- ⚠️ **needs confirmation:** JWT secret values in committed `.env` — assume replaced/rotated in test/staging; never reuse prod secrets in tests.

### 1.6 Frontend (15 pages)
- Dual-mode API client: `NEXT_PUBLIC_API_MODE = real|mock` (`lib/api/client.ts`). `.env.local` currently `real`, base `http://localhost:5097`.
- **12 pages fully wired to real API** (auth pages, dashboard, write, result, history, profile, upgrade); `/` and `/guide` static.
- **⚠️ `/quiz` and `/vocabulary`:** real-client methods (`generateQuiz`, `submitQuizAttempt`, `getQuiz`, `listVocabulary`, `updateVocabularyMastered`) **already call the Learning endpoints**, but **both pages hard-return early in real mode and render "API pending" banners** (`app/quiz/page.tsx:74`, `app/vocabulary/page.tsx:75`). The backend Learning module fully exists. **Connecting the pages to the backend is an open frontend task before E2E can run these flows.**
- Auth is `localStorage`-based (`nomiwrite_session`); per-page guards; **no refresh-token logic wired** in the client (a `refresh()` exists but unused) ⚠️ — E2E must assume short-lived sessions may hit 401.

### 1.7 CI/CD
❌ **Does not exist.** No `.github/workflows`, no Jenkins/GitLab config. Current branch is named `ci/build-and-test-pipeline` but contains no pipeline files. **Propose creating a CI pipeline** (§2.5).

### 1.8 Infrastructure
- `docker-compose.prod.yml`: 12 services; **network segmentation** — `nomiwrite-public` (gateway only) + `nomiwrite-internal`; RabbitMQ management UI **not published**; no service ports published except gateway `80:8080`; every API `read_only + tmpfs + cap_drop + no-new-privileges` + resource limits; `/health` curl probes (30s/5s/3); `restart: unless-stopped`; `backup-service` (postgres:17-alpine) runs `backup-db.sh` on a 24 h loop, mounts `./scripts:/scripts:ro`, volume `nomiwrite-backups`.
- `backup-db.sh`: per-DB gzip SQL dumps (9 DBs), atomic `.tmp→mv`, integrity check, retention 7 d, `PGSSLMODE=require`. `restore-db.sh`: `--check` (non-destructive) vs manual `RESTORE` confirm; `ON_ERROR_STOP=1`.
- ⚠️ Dev compose excludes **admin-service** and **backup-service**; gateway dev routing references an `admin-cluster` → `localhost:5180` that is unreachable in dev Docker. E2E against Admin endpoints must use prod-style compose or host-run Admin service.
- ⚠️ **Credential hygiene:** real-looking secrets committed in `NomiWrite.Backend/.env`, `.env.example`, `appsettings.json`, `NomiWrite.Frontend/.env.local` (Supabase password, Gmail app password, Google client id, Gemini key, VNPay/MoMo keys). Covered in security tests as an explicit "no secrets in logs/responses/images" check; rotation is an ops risk, not a test.

### 1.9 Sensitive data policy
Use **only synthetic/seed data** in all environments. Never seed real user PII, real emails, or prod payment references. See §2.4.4.

---

## 2. Assumptions Requiring Confirmation

| # | Assumption | Impact if wrong |
|---|---|---|
| A1 | VietQR payment gateway will be implemented; until then VietQR is out of E2E scope | VietQR cases reduced to "provider not supported" behavior tests |
| A2 | CI host will be GitHub Actions (repo already hosted on GitHub) | Pipeline YAML changes if Jenkins/self-hosted |
| A3 | Test/staging DBs are disposable Postgres in Docker, **not** the prod Supabase | Prevents any destructive/restore practice against prod |
| A4 | Gemini will be stubbed in CI (never real calls) and only smoke-tested against live API manually | Cost + non-determinism control |
| A5 | Frontend `/quiz` + `/vocabulary` will be un-gated to real mode as part of this work | E2E of Core Learning Loop blocked until switch |
| A6 | AICoordinator Gemini `HttpClient` (no explicit timeout) is intentional/default (100 s) | Load-test thresholds must be set accordingly |
| A7 | The 4 orphan events (§1.3) are roadmap-expected, not bugs | Queue assertions tuned to current consumers |
| A8 | The `MockedPaymentGatewayService` → real gateways swap will be gated by a feature flag/env | Sandbox integration tests depend on the switch |

---

## 3. Test Plan by Layer

### 3.1 Unit Testing (per service)

**Goal:** test pure business logic in `Application/` and `Domain/` layers with no I/O. All map to the existing clean-architecture seams → `Application` projects are the natural test targets (they already depend only on interfaces).

**Proposed framework:** xUnit (see §4.1). Each service gets one `NomiWrite.<Service>.Application.UnitTests` project.

#### 3.1.1 Auth — **Critical**
| # | Test case | Priority |
|---|---|---|
| U-A1 | Refresh token rotation: expired token → `InvalidRefreshTokenException` | Critical |
| U-A2 | Refresh token reuse detection: reusing a revoked token revokes the **entire token family** | Critical |
| U-A3 | Login rejects deactivated (`IsDeleted`) / banned / non-Active accounts with correct exceptions | Critical |
| U-A4 | Password hasher round-trip + failure on wrong password | High |
| U-A5 | `ForgotPasswordAsync`/`ResendVerificationEmailAsync` return generic message (no account enumeration) for both existing & missing email | High |
| U-A6 | `ResetPasswordAsync` invalidates all prior refresh tokens + marks reset token used | Critical |
| U-A7 | Email verification idempotency — repeated/re-used token returns success for verified account | High |
| U-A8 | `RegisterAsync` normalizes email, rejects duplicates, publishes `UserRegisteredEvent` | High |

#### 3.1.2 Writing — **High**
| # | Test case | Priority |
|---|---|---|
| U-W1 | Submission lifecycle: only `Draft → Submitted`; submit of non-draft throws | Critical |
| U-W2 | Ownership guard on get/update submission — other user's submission rejected | Critical |
| U-W3 | `SubmitSubmissionAsync` publishes `WritingSubmittedEvent` with correct content/word count | High |
| U-W4 | VIP gating: subscription client fail-soft → prompt/sample-answer treated as non-VIP | High |
| U-W5 | Admin prompt CRUD validations (word limits, VIP flag, sample answer) | Medium |

#### 3.1.3 AICoordinator — **Critical** (product core)
| # | Test case | Priority |
|---|---|---|
| U-G1 | **Grading parse/classification:** mock `IAiGradingProvider` returns valid schema → `GrammarErrorsJson`/`VocabularySuggestionsJson`/`RestructuringSuggestionsJson` serialized correctly, `OverallBand` set, `Status = Completed` | Critical |
| U-G2 | **Provider failure path:** provider throws → `Status = Failed`, error persists, **no** `GradingCompletedEvent` published (chain halts) | Critical |
| U-G3 | Grading idempotency: re-grade (duplicate `WritingSubmittedEvent`) → reuses existing `GradingResult`, no duplicate row | Critical |
| U-G4 | `GetGradingResultBySubmissionIdAsync` returns null vs completed; Pending barrier | High |
| U-G5 | `CompareWithPreviousAttemptAsync` band difference math + no-previous-attempt case | High |
| U-G6 | `RequestTutorReviewAsync` VIP gating (free user → `TutorReviewSubscriptionRequiredException`; subscription client failure → treated as free) | High |
| U-G7 | `FlagGradingResultAsync` ownership check | High |
| U-G8 | Gemini response **parsing robustness**: markdown-fenced JSON, missing `candidates`, empty `parts`, extra text → graceful failure/exception (mirrors `ExtractText` in GeminiQuizProvider; grading provider uses `ReadFromJsonAsync`) | High |

#### 3.1.4 Payment — **Critical**
| # | Test case | Priority |
|---|---|---|
| U-P1 | **Webhook idempotency:** duplicate success callback for a `Completed` payment → no new transaction, no event | Critical |
| U-P2 | Provider-mismatch webhook (callback provider ≠ payment provider) → `InvalidWebhookException` | Critical |
| U-P3 | VNPay signature: valid/invalid `vnp_SecureHash`, constant-time compare, `vnp_Amount` decimal/100 parsing, success = code `00` + status `00` | Critical |
| U-P4 | MoMo signature + `resultCode` check | Critical |
| U-P5 | `CreatePaymentAsync` discount math (promo valid/invalid/expired/subs-unreachable fail-open), order-reference uniqueness | High |
| U-P6 | Refund: only `Completed` refundable, ownership enforced, duplicate pending refund rejected | High |
| U-P7 | Ownership guards on `GetPaymentStatusAsync`/history | Critical |
| U-P8 | CSV export format + admin analytics math | Medium |

#### 3.1.5 Subscription — **High**
| # | Test case | Priority |
|---|---|---|
| U-S1 | `ActivateSubscriptionFromPaymentAsync`: switch vs extend vs create; publish `SubscriptionActivatedEvent` | Critical |
| U-S2 | **Payment-activation idempotency:** duplicate `PaymentCompletedEvent` → no double-extension | Critical |
| U-S3 | Promo validation window/max-redeem (⚠️ `TimesRedeemed` currently **not** incremented — test documents current behavior) | High |
| U-S4 | Expiry sweep job: ≤3d emits `SubscriptionExpiringEvent` once (`ExpiryWarningsSent` guard); expired flips status | High |
| U-S5 | Cancel-subscription rules | Medium |
| U-S6 | `GetCurrentSubscriptionAsync` days-remaining math | Medium |

#### 3.1.6 Notification — **Medium**
| # | Test case | Priority |
|---|---|---|
| U-N1 | Dedup by `ReferenceId`/submission on `GradingCompletedEvent` | Medium |
| U-N2 | Preference gating (InApp/GradingAlerts off → no notification) | Medium |
| U-N3 | Broadcast announcements (`UserId == null`) visible to all, not duplicated | Medium |
| U-N4 | Paging/filter unread | Low |

#### 3.1.7 Admin — **Medium**
| # | Test case | Priority |
|---|---|---|
| U-AD1 | `AnalyticsAggregatorService` parallel aggregation; per-upstream failure → `Warnings` set, others still populated | High |
| U-AD2 | Announcement validation + event publish (202) | Medium |
| U-AD3 | Moderation report duplicate-pending guard, resolve/reject flow, role gating | Medium |

#### 3.1.8 User — **High**
| # | Test case | Priority |
|---|---|---|
| U-U1 | `UserRegisteredEventConsumer` creates profile; duplicate event idempotent | High |
| U-U2 | `GetProgressAsync` aggregation math (band history, streak, grammar stats); fails-soft on upstream errors | High |
| U-U3 | Profile update validation | Medium |

#### 3.1.9 Learning — Vocabulary & Quiz & Attempts — **Critical** (new modules, IDOR-critical)
| # | Test case | Priority |
|---|---|---|
| U-L1 | **IDOR — vocabulary:** `UpdateMasteredAsync` with another user's id → `ForbiddenLearningAccessException` | Critical |
| U-L2 | **IDOR — quiz get:** `GetQuizAsync` with another user's quiz id → `ForbiddenLearningAccessException`; answers/explanations **not** exposed pre-attempt | Critical |
| U-L3 | **IDOR — quiz attempt:** `SubmitAttemptAsync` against another user's quiz → `ForbiddenLearningAccessException` | Critical |
| U-L4 | **Quiz generation with missing sources:** no grammar errors + no vocab → `QuizGenerationSourceNotFoundException` (both submission-scoped and general) | Critical |
| U-L5 | **Quiz generation with only `VocabularyIds` of another user** → returns empty/throws, no data leak | Critical |
| U-L6 | Attempt grading: case-insensitive exact-string match, empty/missing answer = incorrect, score/total math, attempt persisted with normalized answers | Critical |
| U-L7 | Quiz answer-key leakage: `GetQuizAsync` before completion must omit `CorrectAnswers`/`Explanations` | Critical |
| U-L8 | `CollectQuizSourcesAsync` filters: submission-scoped, vocab-id-scoped, recent-history window (5 submissions, 50 items) | High |
| U-L9 | Deterministic fallback: `DeterministicQuizProvider` produces valid options (dedupe distractors), blank-out logic, category normalization; `NormalizeQuestions` caps at 10, normalizes type | High |
| U-L10 | Gemini quiz provider JSON hardening: markdown fences stripped, empty/`null` questions → fallback | High |
| U-L11 | `GradingCompletedEventConsumer` dedupe of vocab (per user+submission+originalWord+suggested) and grammar (suggestion/sentence match); ~17 Vietnamese grammar-category classifier | High |

#### Coverage thresholds (proposed, justified by service risk)
| Service | Target line coverage | Rationale |
|---|---|---|
| Payment, Auth, AICoordinator, Learning | **≥ 80%** Application layer | Money, identity, AI parsing, IDOR — highest blast radius |
| Writing, Subscription, User | ≥ 70% Application layer | Core-loop adjacent; ownership + idempotency logic |
| Admin, Notification | ≥ 50% Application layer | Fail-soft aggregation + dedupe; lower risk if regressions slip |

---

### 3.2 Integration Testing

**Goal:** verify real components against real dependencies (Postgres, RabbitMQ, Gateway) but **stubbed external parties** (Gemini, payment sandboxes, SMTP, Google). Run in ephemeral Dockerized environments (Testcontainers).

#### 3.2.1 Service ↔ Database — **Critical/High**
| # | Test case | Priority |
|---|---|---|
| I-D1 | Transaction rollback on mid-process failure (e.g., grading save fails after result persisted → no partial data) | Critical |
| I-D2 | Npgsql connection retry (`EnableRetryOnFailure(3)`) survives a temporary DB restart | High |
| I-D3 | EF migrations apply cleanly to a fresh Postgres (all 27 migration sets across 9 services) | Critical |
| I-D4 | JSON (jsonb) columns (GradingResult JSONs, Quiz questions/answers, Plan features) round-trip with Vietnamese diacritics | High |
| I-D5 | `ThrowDbException` mid `SaveChanges` → no orphaned rows | High |

#### 3.2.2 Service ↔ RabbitMQ — **Critical**
| # | Test case | Priority |
|---|---|---|
| I-M1 | Happy-path publish→consume for `WritingSubmittedEvent`, `GradingCompletedEvent`, `PaymentCompletedEvent` end-to-end across the real services | Critical |
| I-M2 | **Consumer downtime:** stop Learning consumer before publishing `GradingCompletedEvent` → message retained and processed on restart (MassTransit durable queue) | Critical |
| I-M3 | **No DLQ today** — document observed behavior: failing consumer + no retry config → message lands in MassTransit `_error` queue; assert **not silently redelivered** | Critical (behavior verification) |
| I-M4 | Consumer idempotency under redelivery (e.g., duplicate `GradingCompletedEvent` → exactly one vocab row) | High |
| I-M5 | Publisher failure handling: publishing when RabbitMQ is down → MassTransit default retry/fail behavior; assert callers don't lose data silently (or code fix needed) | High |
| I-M6 | Orphan-event queues (PaymentCreated/PaymentFailed/SubscriptionActivated) accumulate no consumers → no growth in unmanaged queues | Low |

#### 3.2.3 Service ↔ Gateway (YARP) — **High**
| # | Test case | Priority |
|---|---|---|
| I-G1 | Every route prefix maps to the right cluster (write a route→destination contract test mirroring `appsettings.json`) | High |
| I-G2 | Authorization header forwarded intact through gateway to downstream `[Authorize]` endpoints | Critical |
| I-G3 | Unknown `/api/foo` → 404/404-behavior; gateway `/health` independent of downstream | Medium |
| I-G4 | CORS policy permissive in dev but verify it does **not** pass through to downstream (Gateway is the only origin) | Medium |
| I-G5 | Admin cluster reachability in prod compose (⚠️ missing in dev compose — test must run prod-style topology) | High |

#### 3.2.4 Service ↔ Third-party (VnPay/VietQR/MoMo) — **Critical/High**
| # | Test case | Priority |
|---|---|---|
| I-P1 | VNPay sandbox: build param URL, pay via sandbox, verify IPN callback success path updates PaymentOrder → `PaymentCompletedEvent` | Critical |
| I-P2 | VNPay failure/delayed callback: failed transaction status, late duplicate IPN → idempotent, no double-activation | Critical |
| I-P3 | Tampered signature on `vnpay/ipn`/`momo/ipn` → rejected, no status change | Critical |
| I-P4 | Amount mismatch between IPN and DB → rejected | Critical |
| I-P5 | MoMo test gateway captureWallet + IPN (needs test credentials) | Critical |
| I-P6 | VietQR — **blocked until gateway implemented (A1)**; placeholder | Low |
| I-P7 | Webhook ordering race: IPN before PaymentOrder exists → `PaymentNotFoundException` and recovery on retry | High |

#### 3.2.5 AICoordinator ↔ LLM (Gemini) — **Critical**
| # | Test case | Priority |
|---|---|---|
| I-A1 | **Timeout:** mock a Gemini endpoint delaying >100 s → grading marked `Failed`, no event (bottleneck protection) | Critical |
| I-A2 | **Rate-limit/429:** provider returns 429 → failure path documented (⚠️ no retry/backoff today) | Critical |
| I-A3 | **Fallback:** TypeError/HttpRequest exception in Quiz generation → `DeterministicQuizProvider` produces questions, quiz still created | Critical |
| I-A4 | Malformed/non-JSON LLM output → grading `Failed` (no crash), quiz falls back | High |
| I-A5 | Config-drive: admin-updated prompt/temperature/model picked up (60 s cache; `InvalidateActiveConfigCache`), bad config falls back to defaults | High |
| I-A6 | Vocabulary/grammar extracted from a **real (sampled) Gemini grading response** persists into Learning vocab/grammar tables via the event chain | High |

---

### 3.3 System / End-to-End Testing

**Goal:** full flows from the actual Next.js UI through real backend, **no mocks**. Runs against a staging topology (prod-style compose with **only Gemini + payment gateways stubbed at the process boundary** or a dedicated "staging mode" — decide via assumption A4/A8).

**Tool:** **Playwright** (see §4.3 rationale). New test project `NomiWrite.Frontend/e2e/`.

#### 3.3.1 Core Learning Loop (E2E-1) — **Critical**
1. New synthetic user registers & logs in (email verification bypassed or driven via mail catcher).
2. Submits a writing sample to a real prompt.
3. Polls until `GradingStatus.Completed`; asserts band + criteria + **grammar errors & vocab suggestions rendered** (∅ fallback if AI stubbed): verify exactly the saved errors/words appear.
4. Generates a quiz from the submission → **questions reference the actual errors/vocab** (with deterministic provider if Gemini stubbed, this is fully assertable).
5. Answers the quiz → `quiz_attempts` persisted; result shows score; attempt row in DB.
6. Vocabulary page lists the suggested words; mark one **mastered** → PATCH persists (`isMastered=true` row).
7. Progress endpoint reflects new submission/band history/streak.
> Requires frontend fix first (**A5**): `/quiz` + `/vocabulary` must be switched to real mode.

#### 3.3.2 Payment / Subscription Upgrade Flow (E2E-2) — **Critical**
1. Select plan on `/upgrade` → create payment (mock gateway returns sandbox URL).
2. Complete checkout (sandbox VNPay/MoMo) → **webhook updates the PaymentOrder** → `PaymentCompletedEvent` → Subscription activation.
3. Assert `/api/users/me/account` now reports `HasActiveSubscription=true`, plan name/end date.
4. VIP gates now open (e.g., sample answer, tutor review).
5. Cancel subscription → status transitions; expiry sweep flips to Expired.

#### 3.3.3 Regression flows — **High/Medium**
| E2E | Scenario | Priority |
|---|---|---|
| E2E-3 | Write draft → autosave → submit after timer → time-remaining behavior | High |
| E2E-4 | Result page: compare with previous attempt, flag feedback, tutor review request (VIP vs free) | High |
| E2E-5 | Admin: announcements → broadcast notification visible; analytics overview page loads with real aggregate numbers | High |
| E2E-6 | Forgot/reset password with expiry + token reuse rejection | High |
| E2E-7 | Deactivate account within session → subsequent calls 401; login rejected with clear error | Medium |
| E2E-8 | Search/filter vocabulary by topic/mastered; paging | Medium |
| E2E-9 | Profile update persists; progress bands chart updates | Medium |

---

### 3.4 Non-functional Testing

#### 3.4.1 Security
| # | Test case | Priority |
|---|---|---|
| S-1 | **Auth matrix:** every endpoint enumerated (from controllers) exercised without a token (expect 401), with a student token (expect 200 or 403), with admin token (admin-only lists) — automate via an endpoint/role contract test | Critical |
| S-2 | **IDOR sweep:** all `user_id`-scoped resources — submissions, grading results, quiz, vocab, payment status/history, refund requests, notifications, tutor-review requests — user A must never read/modify user B's data (leverages U-L1..L3, U-P7 patterns across services) | Critical |
| S-3 | Secrets not leaked in: API responses, error logs, stack traces, `/health`, export CSV, `exception` middleware payloads | Critical |
| S-4 | RabbitMQ management UI + tenant DBs unreachable from public network; only gateway egress (`docker-compose.prod.yml` segmentation) | Critical |
| S-5 | JWT: expired/forged/tampered tokens rejected; refresh rotation prevents replay (reuse → family revoked) | Critical |
| S-6 | Webhook/IPN endpoints: unauthorized callers, replay, tampered signatures rejected (overlaps I-P2/I-P3) | Critical |
| S-7 | No secrets in committed artifacts: scan images (`docker build` layers exclude `appsettings*/.env*`), CI secret-scan gate | High |
| S-8 | Rate/abuse: register/login/flooding; password-reset generic response prevents enumeration | Medium |

#### 3.4.2 Performance & Load
Focused on the **AICoordinator** (LLM latency is the bottleneck); no blanket load on lightweight services.
| # | Test case | Priority |
|---|---|---|
| P-1 | **Grading throughput:** submit N essays → measure p95/p99 for `GradingResult` flip to Completed under a **stubbed Gemini latency profile** (simulate provider at 3/8/15 s); establish baseline vs config TTL/cache effects | Critical |
| P-2 | **Quiz generation:** `/quiz/generate` latency under concurrent users; fallback path is fast (assert immediate) vs Gemini path | Critical |
| P-3 | Concurrency at RabbitMQ: 100 concurrent `WritingSubmittedEvent` → no message loss, consumers keep up, no DB row-sprawl (dedupe holds) | High |
| P-4 | Payment webhook flood: duplicate+burst callbacks → idempotent, no double activation | High |
| P-5 | Dashboard/history pagination at realistic row counts (e.g., 1k submissions, 10k vocab rows) | Medium |
| P-6 | Tail latency of User `/me/progress` when upstreams (Subscription/Writing/Grading) are slow (3 s timeout, fail-soft) | Medium |
> **Tool:** k6 (§4.4). Baselines to be confirmed with the team (A6 informs expected ceiling).

#### 3.4.3 Resilience / Chaos
| # | Test case | Priority |
|---|---|---|
| R-1 | **Stop RabbitMQ mid-grade** → consumer reconnect on recovery; verify no silent message loss (publisher behavior) | Critical |
| R-2 | Stop a dependent service (Subscription up while Writing down, etc.) → fail-soft paths verified (`GetSubscriptionStatusOrDefaultAsync`) | High |
| R-3 | Kill AICoordinator mid-processing → message retained/redelivered per MassTransit default; grading recovers or is flagged Failed with retry-ability | Critical |
| R-4 | Healthcheck + restart policy: `docker kill` each service → container restarts within compose policy; `/health` gates Gateway → when downstream is down, Gateway still healthy per its own health (independent) | High |
| R-5 | Simulate Gemini 5xx storm → every submission lands `Failed` (documented), no process crash, queue not poisoned | High |
| R-6 | Disk/log rotation: capped logs (20 m/5 files) never run out; backup volume fills → retention frees space | Medium |

#### 3.4.4 Backup / Restore Drill
| # | Test case | Priority |
|---|---|---|
| B-1 | **Full drill in staging (mandatory):** run `backup-db.sh` → confirm 9 gzip files, integrity OK → `restore-db.sh --check` → destructive restore to a **throwaway DB** → row-count + checksum comparison | Critical |
| B-2 | **Measure RTO/RPO:** time-box restore of the largest DB (queue all 9); record actual elapsed (not estimates); RPO = 24 h interval → document true data-loss window | Critical |
| B-3 | Restore tampered/corrupt backup → scripts fail cleanly (gzip -t, ON_ERROR_STOP), operator-failsafe (`RESTORE` prompt) works | High |
| B-4 | Backup-service container: verify cron loop, `.env.production` mount, `BACKUP_DIR` volume persistence across container restart | High |
| B-5 | Partial restore: single-DB restore (auth only) leaves others untouched | Medium |

---

### 3.5 Regression Strategy & CI Integration

**CI proposal — GitHub Actions (A2).** One workflow repo with jobs:
| Job | Runs on | Contents |
|---|---|---|
| `backend-build` | every PR | `dotnet build -warnaserror` across 38 projects |
| `backend-unit` | every PR | all 9 unit test projects via `dotnet test`; coverage gate = **baseline ratchet** (`NomiWrite.Backend/tests/coverage-baseline.json` + `coverage-gate.py`) — fails on regression below committed baselines; §3.1 thresholds above are tracked goals, raise baselines as suites improve |
| `backend-integration` | every PR (fast) / nightly (full) | Testcontainers suite §3.2 |
| `frontend-unit` | every PR | Vitest component tests (new) |
| `frontend-e2e` | PRs with `e2e` label + nightly + pre-release | Playwright against staging topology |
| `non-functional` | nightly + scheduled weekly | k6 load, security (auth matrix/IDOR/secret scan), resilience chaos script |
| `deploy-smoke` | after every prod deployment | **§3.5.1 smoke suite** |

#### 3.5.1 Post-deployment smoke suite (minimal, distinct from regression)
1. Gateway `/health` + each service `/health` via prod compose.
2. Login as synthetic user → GET `/api/users/me` 200.
3. Create + submit one writing → poll grading → Completed (stubbed LLM).
4. Generate + attempt one quiz; list vocabulary; patch 1 mastered.
5. Create payment (mock) → complete via sandbox webhook → subscription active.
6. RabbitMQ queue depth for key consumers returns to 0.

---

## 4. Prioritization Table (Risk-based)

Consolidated from §3. **P1 = do first.**
| Rank | Category / item | Layer | Why | Effort |
|---|---|---|---|---|
| P1 | Payment webhook idempotency + signature (I-P1..P4, S-6, U-P1..P4) | Unit/Int/Sec | Money; TODO comments left in `PaymentService` ("verify signature") | 5–6 pd |
| P1 | Auth token rotation/family-revocation + account-state machine (U-A1..A8, S-5) | Unit/Sec | Identity; already partially correct — regressions here are catastrophic | 3–4 pd |
| P1 | Learning IDOR + quiz-source/attempt logic (U-L1..L11) | Unit | New modules, ownership-critical, previously mocked | 4–5 pd |
| P1 | AICoordinator grading parse/fail/retry + LLM robustness (U-G1..G8, I-A1..A6) | Unit/Int | Product core; single-point LLM dependency, no retry today | 5–6 pd |
| P1 | Core Learning Loop E2E + frontend real-mode fix (E2E-1, A5) | E2E | Validates the entire product thesis end-to-end | 4–5 pd |
| P2 | RabbitMQ downtime/DLQ/redelivery verification (I-M1..M6, R-1, R-3) | Int/Chaos | No DLQ/retry config — behavior must be *proven*, not assumed | 3–4 pd |
| P2 | Gateway routing/auth-forwarding contract (I-G1..G5, S-1) | Int/Sec | Single ingress; silent misroutes = IM data breach | 2–3 pd |
| P2 | Payment→Subscription activation idempotency (U-S1..S2, I-P1) | Unit/Int | Double-extension = revenue leak | 2 pd |
| P2 | Backup/Restore drill + RTO/RPO measurement (B-1..B5) | NF | Guaranteed data-loss scenario; currently zero evidence | 2–3 pd |
| P2 | CI pipeline + coverage gates (§3.5) | CI | Enables everything else to run repeatedly | 3 pd |
| P3 | Writing lifecycle + VIP fail-soft (U-W1..W5) | Unit | Medium risk | 2 pd |
| P3 | User progress + subscription merge (U-U1..U3) | Unit | Aggregation correctness | 2 pd |
| P3 | Security matrix + IDOR sweep automation (S-1, S-2, S-4, S-7) | Sec | Breadth task, high value | 3–4 pd |
| P3 | Load tests on AI endpoints + queue (P-1..P3) | Perf | Bottleneck evidence | 2 pd |
| P3 | Recovery: healthchecks/restart/chaos (R-2, R-4..R-6) | Chaos | Prod hardening verification | 2 pd |
| P4 | Notification dedupe/preferences (U-N1..N4) | Unit | Low blast radius | 1–1.5 pd |
| P4 | Admin analytics fail-soft + moderation (U-AD1..AD3) | Unit/Int | Medium | 1.5–2 pd |
| P4 | Payment→-Subscription (U-S3..S6), promo semantics | Unit | Current behavior documented (not necessarily fixed) | 1.5 pd |
| P4 | Remaining E2E regressions (E2E-3..9) | E2E | Nice-to-have breadth | 3–4 pd |

---

## 5. Tool Recommendations (per layer, with rationale grounded in this repo)

| Layer | Tool | Why (repo-specific) |
|---|---|---|
| Backend unit | **xUnit + FluentAssertions** | xUnit is the de-facto default for ASP.NET Core Clean Architecture; Application layers are pure C# with interface deps → trivial to mock. Coverlet + `dotnet reportgenerator` for coverage gates. No existing framework to conflict with. |
| Backend unit mocking | **NSubstitute** or Moq | Both fine; pick **NSubstitute** for cleaner Arrange syntax on the many `I*DbContext`/validator/`IPublishEndpoint` seams. MassTransit `IPublishEndpoint` mockable without a broker. |
| Backend integration | **Testcontainers (Postgres + RabbitMQ)** | .NET-native, spins real Postgres/Npgsql and MassTransit/RabbitMQ per test class — exercises actual EF mappings, migrations, and durable queues. Avoids the "works on my machine" gap of mocks. |
| Contract / HTTP stub | **WireMock.NET** (or `Microsoft.AspNetCore.Mvc.Testing` WebApplicationFactory) | WebApplicationFactory gives in-memory HTTP host per service (perfect for Gateway auth-forward tests); WireMock stubs Gemini/payment callbacks with controllable latency/status. |
| E2E | **Playwright (.NET or TS)** | Only sensible choice: repo has **no** Cypress/Playwright/Jest today; Playwright is the standard for Next.js App Router, auto-waits on client-heavy pages, built-in API testing + trailers, works headless in CI, and `.env.local` real-mode is directly consumable. |
| Frontend unit (new) | **Vitest + React Testing Library** | Next 16/React 19/Vite-era toolchain; vitest config inherits `tsconfig` paths (`@/*`), zero Babel friction vs Jest with ESM. Test the `ApiClient` contract (mock vs real responses shape) and page guards. |
| Load | **k6** | Scripted in JS (fits repo language), first-class HTTP + soak + spike profiles, CI-friendly, no SaaS required. Targets: `/api/grading/*`, `/api/quizzes/*` only. |
| Security scan | **gitleaks** + OWASP ZAP (optional), or `trivy config` | gitleaks catches the committed-secret class immediately (S-7 evidence already exists in the repo); ZAP optional for deeper DAST later. |
| Chaos | **docker compose + shell scripts** (no extra tool) | The resilience test surface is Docker-level (stop/kill/restart, network). A small `scripts/chaos/*.sh` repo stays zero-dependency. |
| Coverage reporting | **coverlet.collector + reportgenerator** | Standard `dotnet test --collect:"XPlat Code Coverage"`; merges naturally for the 9-service solution. |

---

## 6. Effort Estimates (preliminary, person-days)

Preliminary figures assume one engineer familiar with the codebase; add 20–30% for a new hire. **No code written yet.**

| Group | Phase 1 (P1) | Phase 2 (P2) | Phase 3 (P3) | Phase 4 (P4) | Total |
|---|---|---|---|---|---|
| Unit (per service suites, 9 projects) | 10–11 | 6–7 | 6–7 | 4–5 | **26–30** |
| Integration (DB/Queue/Gateway/3rd-party/LLM) | 8–9 | 5–6 | 2 | 2 | **17–19** |
| E2E (Playwright, incl. frontend real-mode fix) | 4–5 | — | 2–3 | 3–4 | **9–12** |
| Non-functional (Security, Load, Chaos, Backup) | 2 | 3–4 | 6–7 | 2 | **13–15** |
| CI pipeline + posts-deploy smoke | — | 3 | — | — | **3** |
| **Subtotal** | **24–27** | **17–20** | **16–19** | **11–13** | **68–79 pd** |

**Sequencing recommendation:** begin with P1 unit suites + CI bump (`backend-build` + `backend-unit` gating) so every subsequent test runs in CI immediately; then P1 integration (payment webhooks, LLM) and the Core-Loop E2E; schedule P2 chaos + backup drill shortly before first production cutover.

---

## Appendix — Open Items Log (tracked, not silently assumed)
- [ ] A1 VietQR gateway implementation status
- [ ] A5 Frontend `/quiz` `/vocabulary` un-gate to real mode
- [ ] A8 Payment gateway wiring feature flag
- [ ] PaymentService webhook signature TODO — is VNPay IPN verification wired to the controller/DI?
- [ ] AICoordinator Gemini HttpClient timeout (default 100 s confirmed absent from DI)
- [ ] Effort/sequencing sign-off; then Phase-1 kickoff