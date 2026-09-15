# Database and migration readiness

Phase 2 establishes how the NomiWrite backend database fleet is migrated,
seeded, and verified for local smoke testing.

## Database strategy

Use a hybrid strategy:

- Local development and demo smoke tests may point services at Supabase databases
  through the connection strings in `.env`.
- Docker Compose owns RabbitMQ and service containers, but PostgreSQL remains
  external. There is no local PostgreSQL container in the current compose files.
- A local PostgreSQL container can be added later for offline integration tests,
  but it should not replace the Supabase-compatible path until backup/restore
  and migrations are tested against PostgreSQL directly.

## Migration command

Run all service migrations from `NomiWrite.Backend`:

```powershell
.\scripts\migrate-all.ps1
```

Preview the commands without touching any database:

```powershell
.\scripts\migrate-all.ps1 -DryRun
```

Run one or more services:

```powershell
.\scripts\migrate-all.ps1 -Service Auth,User,Writing
```

The script reads `.env` by default and expects these connection strings:

- `ConnectionStrings__AuthDb`
- `ConnectionStrings__UserDb`
- `ConnectionStrings__WritingDb`
- `ConnectionStrings__GradingDb`
- `ConnectionStrings__PaymentDb`
- `ConnectionStrings__SubscriptionDb`
- `ConnectionStrings__LearningDb`
- `ConnectionStrings__NotificationDb`
- `ConnectionStrings__AdminDb`

## DbContext inventory

| Service | DbContext | Database | DbSets | Migration project | Startup project |
| --- | --- | --- | --- | --- | --- |
| Auth | `AuthDbContext` | `nomiwrite_auth` | Users, RefreshTokens, EmailVerificationTokens, PasswordResetTokens | `src/Services/Auth/NomiWrite.Auth.Infrastructure` | `src/Services/Auth/NomiWrite.Auth.API` |
| User | `UserDbContext` | `nomiwrite_user` | UserProfiles | `src/Services/User/NomiWrite.User.Infrastructure` | `src/Services/User/NomiWrite.User.API` |
| Writing | `WritingDbContext` | `nomiwrite_writing` | WritingTypes, WritingPrompts, WritingSubmissions | `src/Services/Writing/NomiWrite.Writing.Infrastructure` | `src/Services/Writing/NomiWrite.Writing.API` |
| Grading | `GradingDbContext` | `nomiwrite_grading` | GradingResults, TutorReviewRequests, GradingFeedbackFlags, AiGradingConfigs | `src/Services/AICoordinator/NomiWrite.AICoordinator.Infrastructure` | `src/Services/AICoordinator/NomiWrite.AICoordinator.API` |
| Payment | `PaymentDbContext` | `nomiwrite_payment` | Payments, PaymentTransactions, RefundRequests | `src/Services/Payment/NomiWrite.Payment.Infrastructure` | `src/Services/Payment/NomiWrite.Payment.API` |
| Subscription | `SubscriptionDbContext` | `nomiwrite_subscription` | SubscriptionPlans, UserSubscriptions, PromoCodes | `src/Services/Subscription/NomiWrite.Subscription.Infrastructure` | `src/Services/Subscription/NomiWrite.Subscription.API` |
| Learning | `LearningDbContext` | `nomiwrite_learning` | VocabSuggestions, GrammarErrors, Quizzes, QuizAttempts | `src/Services/Learning/NomiWrite.Learning.Infrastructure` | `src/Services/Learning/NomiWrite.Learning.API` |
| Notification | `NotificationDbContext` | `nomiwrite_notification` | Notifications, NotificationPreferences | `src/Services/Notification/NomiWrite.Notification.Infrastructure` | `src/Services/Notification/NomiWrite.Notification.API` |
| Admin | `AdminDbContext` | `nomiwrite_admin` | ContentReports | `src/Services/Admin/NomiWrite.Admin.Infrastructure` | `src/Services/Admin/NomiWrite.Admin.API` |

## Migration chain inventory

| Service | Migration chain |
| --- | --- |
| Auth | `20260905035931_InitialCreate` -> `20260908023211_AddEmailVerificationAndPasswordReset` -> `20260910081142_AddGoogleIdAndAvatarUrl` -> `20260911130302_AddAccountStatusAndRoles` |
| User | `20260907090242_InitialCreate` -> `20260908064624_AddTargetExamDate` |
| Writing | `20260907054639_InitialCreate` -> `20260908031025_AddTimingImageAndSampleAnswer` -> `20260911130319_AddPromptWordLimitsAndVipFlag` -> `20260911150315_AddSubmissionGradedAt` -> `20260913145748_SeedWritingCatalogPrompts` |
| Grading | `20260907072548_InitialCreate` -> `20260907073959_FixJsonbColumnMapping` -> `20260908034024_AddVocabularyRestructuringTutorReviewAndFeedbackFlag` -> `20260911130406_AddAiGradingConfig` |
| Payment | `20260905042533_InitialCreate` -> `20260907083534_AddPlanIdToPaymentOrder` -> `20260908064906_AddRefundRequestsAndDiscount` -> `20260909014726_AddRefundRequestAndDiscount` |
| Subscription | `20260907083615_InitialCreate` -> `20260908064812_AddPromoCodes` -> `20260909014705_AddPromoCode` -> `20260911084111_AddExpirySweepTracking` -> `20260911130321_AddPlanFeaturesJson` |
| Learning | `20260912023025_InitialCreate` |
| Notification | `20260911083705_InitialCreate` -> `20260911143654_AllowNullableUserIdForBroadcast` |
| Admin | `20260911144036_InitialCreate` |

## Seed readiness

Current model-managed seed data:

- Writing types: IELTS Task 1 Academic, IELTS Task 1 General Training, IELTS Task 2,
  TOEFL Integrated, TOEFL Independent, PTE Academic Summarize Written Text,
  Cambridge B2 First Essay, VSTEP Task 2 Essay, Cover Letter, Business Email,
  Meeting Minutes, Paragraph Writing, Personal Statement / SOP, Academic Essay,
  Research Abstract.
- Writing prompts: 12 smoke-test prompts across exam, professional, and academic
  writing types, including free and VIP-only examples.
- Subscription plans: VIP Monthly, VIP Yearly.
- Promo code: `WELCOME20`.
- AI grading config: one active default configuration.

Known seed gaps before broader smoke testing:

- The seed prompt set is intentionally compact. Phase 4 should expand it into the
  full DOCX-derived writing catalog and add real chart/image assets for visual
  Task 1 prompts.
- Admin/moderator users should not be seeded with plain credentials in EF migrations.
  Use a safe setup path instead: create users through Auth, then promote roles with
  an environment-gated admin seed script or direct non-production SQL runbook.

## Backup and restore verification

The selected database setup uses the existing scripts:

- `scripts/backup-db.sh`
- `scripts/restore-db.sh`

Verification checklist:

- Use the Supabase direct PostgreSQL endpoint on port `5432`, not the pooler.
- Run `backup-db.sh` against all nine databases and confirm one non-empty `.sql.gz`
  file per database.
- Run `gzip -t` for every generated backup.
- Restore at least one backup into a throwaway database with `restore-db.sh --check`
  first, then perform a real test restore into a non-production database.
- Do not restore into production without a fresh backup of the current production
  state.
