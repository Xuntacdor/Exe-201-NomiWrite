# NomiWrite Frontend Plan

Last updated: 2026-09-10

## Location

Canonical plan file: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite\FRONTEND_PLAN.md`

Active repo: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`

Legacy frontend prototype source: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\nomiwrite`

Git repo info:

- Base branch: `develop`
- Current work branch for this plan update: `PhmHai0702/fe/api-boundary-real-first`
- Remote: `git@github.com:Xuntacdor/Exe-201-NomiWrite.git`
- The active project repository is `FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`.
- This active repository now contains both backend code and the migrated Next.js frontend in `NomiWrite.Frontend/`.

Project naming note:

- Older docs use `WriteWise`.
- Current code and CP3 materials use `NomiWrite`.
- Frontend should display `NomiWrite` consistently unless the team decides to rename.

## Sources Already Read

- `SRS_WriteWise.md`
- `WriteWise_Presentation_CP1_2.docx`
- `NomiWrite_Presentation_CP3.docx`
- `NomiWrite-Project.pptx`
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/app/**` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/package.json` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/.gitignore` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/eslint.config.mjs` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/Exe-201-NomiWrite/NomiWrite.Backend/**`

Legacy location previously inspected before the workspace move:

- `D:/FPT_Uni/SUMMER26/EXE101/StaticMVP/nomiwrite`

Active development must use `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`.

## Current Repo Snapshot

Tracked files in the active repo now include the backend solution, service files, migrated frontend, and this plan:

- `NomiWrite.Backend/NomiWrite.sln`
- `NomiWrite.Backend/src/Gateway/NomiWrite.Gateway/**`
- `NomiWrite.Backend/src/Services/Auth/**`
- `NomiWrite.Backend/src/Services/User/**`
- `NomiWrite.Backend/src/Services/Writing/**`
- `NomiWrite.Backend/src/Services/Payment/**`
- `NomiWrite.Backend/src/Services/AICoordinator/**`
- `NomiWrite.Backend/src/Services/Subscription/**`
- `NomiWrite.Backend/src/Shared/NomiWrite.Shared.Contracts/**`
- `NomiWrite.Frontend/**`
- `FRONTEND_PLAN.md`
- `.gitignore`

Current `git status --short --branch` after pulling/reading latest `develop`:

- `PhmHai0702/fe/api-boundary-real-first...origin/PhmHai0702/fe/api-boundary-real-first [ahead 8]`
- Working tree is clean before this plan update.
- Local branch HEAD is currently at `0757b0e`, the same commit as `origin/develop`.

Important implications:

- The frontend folder decision is settled as `NomiWrite.Frontend/`.
- The old legacy prototype remains useful only as historical reference.
- Continue feature work in the active repo, not the legacy `nomiwrite` folder.
- Keep mobile source separate unless the team explicitly decides otherwise.

## Git Workflow

Before editing code for each checklist item or feature, create and checkout a dedicated branch from `develop` or the latest agreed integration branch.

Branch naming convention:

```text
<git-username>/fe/<feature-or-check-name>
```

Examples:

```text
PhmHai0702/fe/frontend-plan-tracking
PhmHai0702/fe/phase-0-repo-hygiene
PhmHai0702/fe/auth-api-session
PhmHai0702/fe/api-client-stub
PhmHai0702/fe/writing-submit-flow
PhmHai0702/fe/result-feedback-api
PhmHai0702/fe/dashboard-history-data
PhmHai0702/fe/vocabulary-api
PhmHai0702/fe/quiz-attempts
PhmHai0702/fe/profile-editing
PhmHai0702/fe/payment-stub
```

Per-branch workflow:

1. `git checkout develop`
2. Pull latest code if network/auth is available.
3. `git checkout -b <git-username>/fe/<feature-or-check-name>`
4. Implement only that feature/check.
5. Run relevant verification.
6. Update this Progress Log.
7. Commit/PR when the user asks or when the feature is ready for review.

## Current MVP Summary

The migrated Next.js MVP now has API-aware UI for:

- Landing page
- Login and register
- Dashboard
- Writing editor
- Result page
- Writing guide
- Grammar quiz
- Vocabulary book
- Writing history
- Profile
- Upgrade/payment checkout boundary

Main tech stack from `package.json`:

- Next.js `16.2.9`
- React `19.2.4`
- React DOM `19.2.4`
- Tailwind CSS v4
- lucide-react
- TypeScript strict mode
- ESLint 9 + Next core web vitals config

## Current Code Findings

Active repo status:

- The active `Exe-201-NomiWrite` repo contains the .NET backend and `NomiWrite.Frontend/` Next.js app.
- Gateway is implemented with YARP reverse proxy and loads routes/clusters from the `ReverseProxy` config section.
- Tracked gateway config currently lacks `ReverseProxy`; `NomiWrite.Gateway/appsettings.Development.json` only contains logging. Local real-mode smoke testing needs a restored local proxy config.
- Auth service has real endpoints: register, login, refresh, authorized logout, verify email, resend verification email, forgot password, reset password, and deactivate account.
- User service has real profile/account/progress endpoints under `/api/users/me`.
- Writing service is now real: writing types, prompts, prompt detail, sample answer, submission create/update/submit/list/detail, and timed-submission remaining time under `/api/writing`.
- AI Coordinator grading service is now real for grading result by submission, grading history, compare with previous attempt, tutor review request/list, and feedback flagging under `/api/grading`.
- Payment service is real for checkout/status/history/refund requests and VNPay/MoMo IPN/webhook boundaries under `/api/payment`.
- Subscription service is real for plans, current subscription, cancel subscription, and promo-code validation under `/api/subscriptions`.
- Google/OAuth backend endpoint is requested by the backend team as `POST /api/auth/google` with `{ idToken }`; current pulled backend code has not exposed it yet.
- There are still no dedicated backend modules/endpoints for dashboard summary, vocabulary list/mastered state, quiz generation, quiz attempts, notifications, forum, or usage quota.

Frontend status:

- `NomiWrite.Frontend/lib/api/routes.ts`, `real-client.ts`, `mock-client.ts`, and `client.ts` are in place.
- Login/register use controlled forms and save API-backed sessions.
- Login/register include Google Identity Services buttons. Frontend receives Google `credential`/`id_token`, sends it to `apiClient.googleLogin()`, and only stores the NomiWrite JWT returned by backend.
- Writing page uses the API client for types/prompts/sample answer and submission flow.
- Result page loads by `submissionId` and renders API-shaped grading, grammar, vocabulary, rewrite, compare, tutor-review, and feedback-flag data.
- Result page now also shows recent tutor review requests from the grading service.
- Dashboard/history/profile/upgrade use API client data where backend contracts exist, with frontend-derived fallback where no dedicated endpoint exists.
- Upgrade page now exposes subscription plans, promo validation, checkout, payment status refresh, payment history, refund request creation, and refund request history.
- Vocabulary and quiz no longer keep page-level mock arrays; real mode shows pending states until backend contracts exist.
- `app/guide/page.tsx` and `app/components/GuideModal.tsx` still duplicate large writing guide data.
- `app/components/AppShell.tsx` and `AppSidebar.tsx` provide desktop app layout, but mobile navigation still needs attention.
- Some Vietnamese UI strings/comments in backend and frontend still appear mojibake/encoding-corrupted in file output; review and normalize while touching each file.
- Latest known frontend verification before this pull/read: `npm run lint` and `npm run build` passed.

## Product Direction From Docs

Core product loop:

1. User writes an English text.
2. Backend sends it to AI.
3. AI returns structured JSON with score, criteria scores, grammar errors, and vocabulary suggestions.
4. Backend stores submission, errors, vocabulary, quiz, and attempts in database.
5. Frontend shows feedback, history, vocabulary, quiz, and measurable progress.

Architecture direction from CP3/system diagram:

- Web user app: Next.js, server-side rendering where useful.
- Admin/CMS: separate React SPA later.
- Mobile app: Android/iOS later.
- Backend: `.NET` services behind API Gateway.
- Database: PostgreSQL for business data, MongoDB/NoSQL for logs/activity if needed.
- Storage: S3/MinIO.
- AI Coordinator: backend-only service that talks to ChatGPT, Llama, DeepSeek, etc.
- Payment: VNPay, VietQR, MoMo later.
- Analytics/notification: Google Analytics, Firebase Messaging later.

Frontend rule:

- Never call AI providers directly from frontend.
- Treat backend/API Gateway as the source of truth as soon as endpoints and database-backed contracts are available.
- Mock data is temporary only: keep it behind the API client for local fallback/demo mode, never embedded directly in pages/components.
- Remove hard-coded product data from pages while integrating each feature with real API responses.

## Backend Integration Priority

Backend/database integration should take priority over expanding mock-only frontend behavior.

Use this order for every feature area:

1. Connect to the real API Gateway endpoint if the backend contract exists.
2. If the endpoint is not ready, define the typed frontend contract in `lib/api/routes.ts` and keep a matching mock implementation in `lib/api/mock-client.ts`.
3. When the backend endpoint becomes available, switch the feature to `real-client.ts`, verify the response shape, and delete page-level hard-coded data for that feature.
4. Keep only seed/demo fixtures in `lib/mock-data/`; do not leave sample arrays inside `app/**/page.tsx`.

Current known backend status:

- Auth has real endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, `POST /api/auth/logout`, `POST /api/auth/verify-email`, `POST /api/auth/resend-verification-email`, `POST /api/auth/forgot-password`, `POST /api/auth/reset-password`, and `POST /api/auth/deactivate`.
- User profile has real endpoints: `GET /api/users/me`, `PUT /api/users/me`, `GET /api/users/me/account`, and `GET /api/users/me/progress`.
- Writing has real endpoints for types, prompts, prompt detail, sample answer, draft creation, content update, submit, list, detail, and timed-submission remaining time under `/api/writing`.
- AI Coordinator has real grading endpoints for result detail, grading history, compare with previous attempt, tutor review request/list, and flagging feedback under `/api/grading`.
- Payment has real create/status/history/refund endpoints under `/api/payment`, plus VNPay and MoMo IPN handlers.
- Subscription has real plan/status/cancel/promo-code endpoints under `/api/subscriptions`.
- Dashboard is still derived in frontend from User/Writing/Grading data because there is no dedicated dashboard endpoint in the pulled backend.
- Vocabulary, quiz, quiz-attempts, notifications, forum, and usage quota are not exposed as dedicated backend modules yet; keep these pages at status 1/2 and do not call missing real endpoints.
- Gateway still depends on a local `ReverseProxy` config, while tracked `NomiWrite.Gateway/appsettings.json` was removed from source after the latest backend merge.

## Frontend MVP Scope

Do first:

- Auth UX connected to real backend auth when available; mock session only as fallback.
- Protected app shell.
- Writing editor.
- Submit essay through API client; use real backend when submission/grading endpoints exist.
- Result page from structured feedback data.
- Dashboard from API summary data.
- History list/detail navigation.
- Vocabulary book with mastered toggle.
- Quiz generated from grammar/vocabulary API data.

Do later:

- Admin/CMS.
- Real payment.
- Notification.
- Mobile app.
- Full spaced repetition.
- PDF reports.
- Direct frontend AI integration is out of scope; AI integration belongs behind backend AI Coordinator.

## Recommended Frontend Structure

Inside `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`, add a team-approved frontend folder. Recommended folder:

```text
NomiWrite.Frontend/
```

Then migrate the legacy Next.js MVP toward:

```text
NomiWrite.Frontend/
  app/
    dashboard/
    guide/
    history/
    login/
    profile/
    quiz/
    register/
    result/
    upgrade/
    vocabulary/
    write/
    components/
      layout/
      ui/
  features/
    auth/
    dashboard/
    writing/
    feedback/
    quiz/
    vocabulary/
    profile/
  lib/
    api/
      client.ts
      mock-client.ts
      real-client.ts
      routes.ts
    constants/
      grammar-categories.ts
      writing-guides.ts
      writing-prompts.ts
    mock-data/
    types.ts
  public/
  package.json
  next.config.ts
  tsconfig.json
```

Keep page files thin. Put data, types, and API logic outside pages.

`lib/mock-data/` is allowed only for fallback/demo fixtures. Delete or move any hard-coded arrays from `app/**` as each page is connected to the API client.

## TypeScript Domain Models

Define these first in `lib/types.ts`:

```ts
export type UserPlan = "free" | "premium";

export interface User {
  id: string;
  email: string;
  displayName: string;
  currentLevel: string;
  targetType: string;
  targetBand?: number;
  plan: UserPlan;
  createdAt: string;
}

export interface Submission {
  id: string;
  userId: string;
  writingType: string;
  topic: string;
  prompt: string;
  content: string;
  wordCount: number;
  overallScore?: number;
  overallFeedback?: string;
  submittedAt: string;
  status: "draft" | "submitted" | "grading" | "graded" | "failed";
}

export interface CriteriaScores {
  taskResponse?: number;
  taskAchievement?: number;
  coherenceCohesion?: number;
  lexicalResource?: number;
  grammaticalRangeAccuracy?: number;
  contentIdeas?: number;
  organization?: number;
  toneRegister?: number;
  format?: number;
}

export interface GrammarError {
  id: string;
  submissionId: string;
  userId: string;
  grammarCategory: string;
  sentence: string;
  errorPart: string;
  suggestion: string;
  explanation: string;
}

export interface VocabSuggestion {
  id: string;
  submissionId: string;
  userId: string;
  topic: string;
  originalWord: string;
  suggestedWord: string;
  exampleSentence: string;
  isMastered: boolean;
}

export interface WritingFeedback {
  submission: Submission;
  criteriaScores: CriteriaScores;
  grammarErrors: GrammarError[];
  vocabSuggestions: VocabSuggestion[];
}

export interface QuizQuestion {
  id: string;
  category: string;
  type: "multiple_choice" | "fill_blank" | "rewrite";
  question: string;
  sentence: string;
  options?: string[];
  correctAnswer: string;
  explanation: string;
}

export interface Quiz {
  id: string;
  userId: string;
  sourceSubmissionId?: string;
  questions: QuizQuestion[];
  createdAt: string;
}

export interface QuizAttempt {
  id: string;
  quizId: string;
  userId: string;
  answers: Record<string, string>;
  score: number;
  attemptedAt: string;
}
```

## Stub API Contract

Suggested backend-aligned endpoints:

```text
POST /auth/login
POST /auth/register
POST /auth/logout
GET  /users/me
PATCH /users/me

GET  /writing/types
GET  /writing/prompts?type=&topic=

POST /submissions
GET  /submissions
GET  /submissions/:id
POST /submissions/:id/grade
GET  /feedback/:submissionId

GET  /dashboard/summary

GET   /vocabulary
PATCH /vocabulary/:id/mastered

POST /quizzes/generate
GET  /quizzes/:id
POST /quiz-attempts

POST /payments/checkout
GET  /payments/:id/status
```

Frontend implementation pattern:

- Add `.env.example`.
- Use `NEXT_PUBLIC_API_MODE=real` when backend endpoints are available.
- Use `NEXT_PUBLIC_API_MODE=mock` only for local/demo fallback.
- Use `NEXT_PUBLIC_API_BASE_URL=http://localhost:5000` or backend team's gateway URL later.
- `lib/api/mock-client.ts` returns local mock data with fake delay.
- `lib/api/real-client.ts` wraps `fetch`.
- `lib/api/client.ts` exports the selected client.
- Components call API client functions only.
- No page should import from `lib/mock-data/` directly.
- Before marking a feature done, confirm it does not keep hard-coded sample records inside `app/**`.

## Fixed Grammar Categories

Initial list from SRS:

1. Mạo từ
2. Hòa hợp chủ ngữ - động từ
3. Chia thì động từ
4. Số ít / số nhiều của danh từ
5. Giới từ
6. Câu điều kiện
7. Mệnh đề quan hệ
8. Câu bị động
9. Trật tự từ trong câu
10. Liên từ và từ nối
11. Đại từ
12. So sánh hơn / so sánh nhất
13. Động từ khuyết thiếu
14. Danh động từ và động từ nguyên mẫu
15. Cấu trúc câu
16. Dùng từ sai loại
17. Collocation
18. Dấu câu
19. Lặp từ / diễn đạt dài dòng
20. Thiếu/thừa thành phần câu
21. Khác

Backend must only return `grammar_category` values from this list, or `Khác`.

## Implementation Roadmap

### Phase 0 - Repo Hygiene And Build Reliability

- [x] Add `FRONTEND_PLAN.md` to git tracking when ready.
- [x] Confirm frontend folder name with team; recommended `NomiWrite.Frontend/`.
- [x] Copy or migrate the legacy Next.js MVP into the chosen frontend folder.
- [x] Replace `next/font/google` in frontend `app/layout.tsx` with a network-safe approach.
- [x] Add frontend `.env.example` with `NEXT_PUBLIC_API_MODE=mock` and gateway base URL.
- [x] Rewrite frontend README/setup notes for NomiWrite.
- [x] Install dependencies inside the frontend folder if missing.
- [x] Confirm frontend `npm run build`.
- [x] Confirm frontend `npm run lint`.
- [ ] Optional only if mobile/Flutter artifacts are copied into this repo later: update `.gitignore` and relevant tooling ignores.

### Phase 1 - Shared Data And API Boundary

- [x] Create `lib/types.ts`.
- [x] Create `lib/constants/grammar-categories.ts`.
- [x] Inventory every hard-coded sample array/object in `app/**` and decide whether it is static content, API data, or temporary fixture.
- [ ] Move writing types, topics, prompts, and guide content out of `app/write/page.tsx`.
- [ ] Move guide data out of `app/guide/page.tsx` and `app/components/GuideModal.tsx`.
- [ ] Move quiz questions out of `app/quiz/page.tsx`.
- [ ] Move vocabulary data out of `app/vocabulary/page.tsx`.
- [ ] Move dashboard/history/profile/result sample records out of page files.
- [x] Create `lib/api/client.ts`, `mock-client.ts`, `real-client.ts`, and `routes.ts`.
- [x] Implement typed real API functions against gateway routes.
- [x] Add frontend API contracts for newly pulled Auth/User/Writing/Grading/Payment/Subscription endpoints.
- [x] Keep typed mock responses only as fallback when real backend endpoints are not ready.
- [ ] Add a cleanup check that blocks feature completion if page-level hard-coded data remains.

### Phase 2 - Auth And Layout

- [x] Convert `app/login/page.tsx` from static link to controlled form.
- [x] Add email/password validation.
- [x] Add loading and error states.
- [x] Connect login/register to real `POST /api/auth/login` and `POST /api/auth/register` through the gateway.
- [x] Add frontend Google Identity Services flow for `POST /api/auth/google` once backend exposes it.
- [x] Store real access/refresh token response from backend; use mock session only when `NEXT_PUBLIC_API_MODE=mock`.
- [x] Convert `app/register/page.tsx` to controlled form.
- [x] Add frontend pages for email verification, resend verification email, forgot password, and reset password.
- [x] Add frontend account deactivation action through `POST /api/auth/deactivate`.
- [ ] Save selected level and target through profile/user API when available; otherwise store only through API client fallback.
- [ ] Add logout behavior in `AppSidebar`.
- [x] Add simple route guard for API-backed app pages in real mode.
- [ ] Add mobile navigation for `AppShell`.

### Phase 3 - Writing Flow

- [x] Connect writing type/topic selectors to backend `api.listWritingTypes()` and `api.listWritingPrompts()`.
- [x] Connect prompt sample answer UI to `GET /api/writing/prompts/{id}/sample-answer`.
- [x] Add draft autosave keyed by selected writing prompt/type.
- [x] Recover draft on page load.
- [x] Validate/display recommended minimum word count by writing type.
- [x] Submit through `api.submitSubmission()`.
- [x] Use real submission/grading endpoint when available; no direct `localStorage` submission handoff for production mode.
- [x] Navigate to `/result?submissionId=...`.
- [x] Replace page-level client-side AI Coach heuristics with backend-first submission flow notes.
- [x] Remove local fake scoring logic once backend returns grading/feedback.

### Phase 4 - Result And Feedback

- [x] Load result by `submissionId`.
- [x] Add loading, error, and empty states.
- [x] Render criteria scores from `WritingFeedback`.
- [x] Render grammar errors from API-shaped data.
- [x] Render vocabulary suggestions from API-shaped data returned by grading.
- [x] Render restructuring/rewrite suggestions from API-shaped data returned by grading.
- [x] Remove hard-coded grammar/vocabulary feedback from `app/result/page.tsx`.
- [x] Read persisted feedback from backend/database through API client in real mode.
- [x] Keep AI score disclaimer visible.
- [x] Add CTA to generate quiz from current submission.
- [x] Add frontend actions for compare with previous attempt, request tutor review, and flag feedback.
- [x] Show recent tutor review requests from `GET /api/grading/tutor-review-requests`.

### Phase 5 - Dashboard And History

- [x] Replace dashboard hard-coded stats with frontend aggregation over real `api.listSubmissions()` and `api.getMyAccount()` until a dedicated dashboard endpoint exists.
- [x] Add empty dashboard state for new user.
- [ ] Use backend/database summary data for score trend and grammar error profile when available.
- [x] Keep frontend aggregation only as fallback/derived behavior over API client data.
- [x] Replace history hard-coded list with `api.listSubmissions()`.
- [x] Merge grading history bands into history rows when `GET /api/grading/history` returns data.
- [ ] Add search, filter, and sort.
- [x] Link each row to `/result?submissionId=...`.

### Phase 6 - Vocabulary

- [x] Replace page-level vocabulary array with `api.listVocabulary()` in mock/demo mode.
- [x] Persist mastered state through `api.updateVocabularyMastered()` in mock/demo mode.
- [ ] 1/2: Use database-backed vocabulary records in real mode after backend exposes dedicated vocabulary list/update endpoints.
- [ ] 1/2: Keep category/mastered filters when real vocabulary taxonomy is defined by backend.
- [x] Add empty/pending states.
- [ ] 1/2: Generate vocabulary quiz from filtered or weak words after quiz/vocabulary APIs exist.

### Phase 7 - Quiz

- [ ] 1/2: Split quiz source into grammar quiz and vocabulary quiz after backend exposes quiz source contracts.
- [x] Generate quiz through `api.generateQuiz()` in mock/demo mode.
- [x] Save attempt through `api.submitQuizAttempt()` in mock/demo mode.
- [ ] 1/2: Read quiz questions from backend/database in real mode after quiz endpoints exist.
- [ ] Show score and wrong-answer review.
- [x] Support quiz from a specific submission in the frontend contract.
- [ ] 1/2: Support quiz from weakest grammar categories after backend exposes weakness/category data.

### Phase 8 - Profile And Plan

- [x] Load user profile/account through `api.getMyAccount()` and fallback `api.getMe()`.
- [x] Edit display name, current level, target type, and target band.
- [x] Save profile through `api.updateMe()`.
- [x] Show current plan/subscription status where backend exposes it.
- [x] Load progress/badges from `GET /api/users/me/progress` when backend returns data.
- [x] Add cancel subscription action through `POST /api/subscriptions/me/cancel`.
- [x] Remove mocked achievements; show API progress/status instead.

### Phase 9 - Upgrade Payment Stub

- [x] Keep billing toggle and backend-supported payment method UI.
- [x] Remove fake card form; backend currently supports provider checkout, not direct card capture.
- [x] Load subscription plans through `api.listSubscriptionPlans()` and pass `planId` into checkout when available.
- [x] Validate promo codes through `GET /api/subscriptions/promo-codes/{code}/validate` and pass `promoCode` into checkout.
- [x] Load payment history through `GET /api/payment/history`.
- [x] Add refund request API boundary for `POST /api/payment/{paymentOrderId}/refund-request`.
- [x] Add refund request UI and refund request history from `GET /api/payment/refund-requests`.
- [x] Add payment status refresh UI through `GET /api/payment/{paymentId}`.
- [x] Submit checkout through real payment API when available; mock checkout only in local/demo mode.
- [x] Show pending, success, and failure states.
- [x] Prepare boundary for VNPay, VietQR, and MoMo.

### Phase 10 - Mock Data Removal And Real Backend Cutover

- [ ] Set default `.env.example` mode to real once backend gateway is ready for frontend integration.
- [ ] Verify every page uses `lib/api/client.ts` instead of local sample arrays; completed for write, result, dashboard, history, profile, upgrade, vocabulary, and quiz. Guide/static learning content still needs centralization.
- [ ] Delete obsolete fixtures from `lib/mock-data/` after equivalent backend/database data exists.
- [ ] Remove fake score generation, fake history generation, fake dashboard aggregation, and fake quiz/vocabulary records from production code paths.
- [ ] Confirm real API smoke tests: auth, current user, writing submit, grading/feedback, dashboard, history, vocabulary, quiz attempt, and payment status where available.
- [ ] Keep a small documented demo fixture set only if the team still needs offline presentation mode.

## Testing Checklist

- [x] `npm run build`
- [x] `npm run lint`
- [ ] Login validation works
- [ ] Register validation works
- [ ] Real login redirects to dashboard when backend auth is available
- [ ] Mock login redirects to dashboard only in `NEXT_PUBLIC_API_MODE=mock`
- [ ] App pages guard unauthenticated users
- [ ] Write page autosaves draft
- [ ] Submit essay creates backend/database submission in real mode
- [ ] Result page loads by `submissionId`
- [ ] Quiz can be generated from feedback
- [ ] Vocabulary mastered toggle persists through backend/database in real mode
- [ ] No page-level hard-coded API records remain in completed feature areas
- [ ] Dashboard works for new and active users
- [ ] Mobile layout checked at 375px width
- [ ] Desktop layout checked at 1440px width

## Progress Log

### 2026-09-05

- [x] Read root documents and presentations.
- [x] Read original static MVP source.
- [x] Created first frontend plan at `StaticMVP\nomiwrite\FRONTEND_PLAN.md`.
- [x] Re-read cloned git repo structure.
- [x] Confirmed actual repo is `StaticMVP\nomiwrite`, branch `main`, remote `git@github.com:Xuntacdor/nomiwrite.git`.
- [x] Confirmed copied Flutter SDK and zip are untracked and should not affect web frontend tooling.
- [x] Moved canonical frontend plan to root `EXE101\FRONTEND_PLAN.md`.
- [x] Updated plan to match cloned repo state.
- [x] Created branch `fe/frontend-plan-tracking` before changing tracking documentation.
- [x] Renamed tracking branch to `Xuntacdor/fe/frontend-plan-tracking` based on remote owner, then corrected it after user clarified git username.
- [x] Corrected branch naming owner to `PhmHai0702`.
- [x] Moved canonical frontend plan back into `StaticMVP\nomiwrite\FRONTEND_PLAN.md` so git can track it.
- [x] Added branch naming workflow: `<git-username>/fe/<feature-or-check-name>`.
- [x] User clarified active workspace moved from `SUMMER26\EXE101` to `FALL26\EXE201`.
- [x] Located new frontend repo at `D:\FPT_Uni\FALL26\EXE201\NomiWrite\nomiwrite`.
- [x] Created branch `PhmHai0702/fe/frontend-plan-tracking` in the Fall26 repo.
- [x] Copied plan into the Fall26 repo so git can track it there.
- [x] Re-read Fall26 repo structure and confirmed it matches the existing Next.js MVP shape.
- [x] Re-read Fall26 codebase against `FRONTEND_PLAN.md`.
- [x] Confirmed Fall26 repo currently has no copied Flutter artifacts; adjusted Phase 0 to avoid stale Summer26 cleanup tasks.
- [x] Confirmed current Fall26 git status only shows untracked `FRONTEND_PLAN.md`.
- [x] Confirmed `node_modules` is absent in Fall26 repo, so build/lint needs dependency setup before verification.
- [x] User switched active repo to `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`.
- [x] Located new active repo on branch `develop`, remote `git@github.com:Xuntacdor/Exe-201-NomiWrite.git`.
- [x] Created branch `PhmHai0702/fe/frontend-plan-tracking` in the new active repo.
- [x] Read backend solution structure and key Auth/Gateway/User/Payment/AI service files.
- [x] Confirmed new active repo currently has backend only; no Next.js frontend folder is present yet.
- [x] Moved/copy-tracked this frontend plan into the new active repo root.
- [x] Started Phase 0 implementation in the new active repo.
- [x] Created branch `PhmHai0702/fe/phase-0-repo-hygiene` from `develop`.
- [x] Migrated legacy Next.js MVP into `NomiWrite.Frontend/`, excluding `.git`, `.next`, `node_modules`, and the legacy copy of `FRONTEND_PLAN.md`.
- [x] Confirmed frontend layout no longer imports `next/font/google`; normalized root metadata to network-safe ASCII text.
- [x] Added/copied frontend `.env.example` with mock API mode and local gateway base URL.
- [x] Updated frontend README/setup notes for NomiWrite and the new folder location.
- [x] Installed frontend dependencies with `npm install`.
- [x] Fixed Phase 0 lint blockers from the migrated MVP: internal root links now use `next/link`, JSX quote escaping is lint-safe, and React effect state updates were adjusted.
- [x] Confirmed `npm run lint` passes in `NomiWrite.Frontend/`.
- [x] Confirmed `npm run build` passes in `NomiWrite.Frontend/`.
- [x] Cleaned migrated frontend files: removed assistant-only `AGENTS.md` and `CLAUDE.md`, removed stale `app/guide/page.tsx.bak`, removed unused default Next public SVG assets, and deleted local `.next` build cache.
- [x] Updated frontend `.gitignore` so `.env.example` and `next-env.d.ts` are kept as source files while `node_modules` and build output stay ignored.
- [x] Updated frontend plan to be backend-first: mock data is temporary fallback only, page-level hard-coded API data must be removed, and real API/database integration is the priority when endpoints are ready.
- [x] Added Phase 10 for mock data removal and real backend cutover.
- [x] Committed Phase 0 as `481d10d Add frontend MVP foundation`.
- [x] Created Phase 1 branch `PhmHai0702/fe/api-boundary-real-first` from the Phase 0 commit.
- [x] Added frontend domain/API types in `NomiWrite.Frontend/lib/types.ts`.
- [x] Added API route map plus real/mock API clients in `NomiWrite.Frontend/lib/api/`.
- [x] Added mock fixtures under `NomiWrite.Frontend/lib/mock-data/` with a README policy and hard-coded data inventory.
- [x] Converted login/register from static navigation to controlled forms that call `apiClient`, save auth session data, and redirect after success.
- [x] Moved register level/goal options into `NomiWrite.Frontend/lib/constants/profile-options.ts`.
- [x] Confirmed Phase 1 slice passes `npm run lint` and `npm run build`.
- [ ] Review npm audit output separately: current install reports 8 vulnerabilities from dependency tree.

### 2026-09-08

- [x] Pulled latest `origin/develop` into `PhmHai0702/fe/api-boundary-real-first`.
- [x] Re-read newly merged backend controllers and DTOs for Auth, User, Writing, Grading, Payment, and Subscription.
- [x] Updated frontend API routes to match backend paths under `/api/writing`, `/api/grading`, `/api/users`, `/api/payment`, and `/api/subscriptions`.
- [x] Added real-client DTO mappers so frontend types stay stable while consuming backend response shapes.
- [x] Connected `app/write/page.tsx` to backend writing types/prompts and authenticated draft/create/update/submit flow.
- [x] Replaced result `localStorage` handoff and fake scoring with `submissionId` query loading through the backend grading endpoint.
- [x] Replaced history hard-coded submissions with `api.listSubmissions()` and result links using `submissionId`.
- [x] Replaced dashboard hard-coded stats with derived data from real user/account and writing submissions APIs.
- [x] Connected profile to `GET /api/users/me/account` and `PUT /api/users/me`.
- [x] Connected upgrade to subscription plans and payment checkout with optional backend `planId`.
- [x] Removed mock testimonials/reviews and fake card-entry UI from upgrade page because backend has no review/card-capture API.
- [x] Confirmed frontend `npm run lint` and `npm run build` pass after backend API integration.

### 2026-09-09

- [x] Pulled latest backend updates from `origin/develop` into `PhmHai0702/fe/api-boundary-real-first`.
- [x] Rechecked backend controllers/DTOs after the pull and confirmed new APIs for auth recovery/verification, user progress, writing sample answers/time remaining, grading history/compare/tutor review/flag, payment history/refunds, subscription cancel, and promo code validation.
- [x] Restored mock-enabled frontend behavior after the real-only mock removal broke login when the gateway/backend was unavailable.
- [x] Extended frontend API routes, shared types, real client, and mock client for newly available backend endpoints.
- [x] Added frontend pages for forgot password, reset password, and verify/resend email.
- [x] Added profile progress, cancel subscription, and deactivate account UI wired through the API boundary.
- [x] Added upgrade promo-code validation and payment-history UI wired through the API boundary.
- [x] Added result-page vocabulary suggestions, rewrite suggestions, compare, tutor review, and feedback flag actions.
- [x] Reworked vocabulary and quiz pages so they no longer keep page-level mock arrays; real mode now shows 1/2 pending states instead of calling missing backend endpoints.
- [x] Confirmed frontend `npm run lint` and `npm run build` pass.

### 2026-09-10

- [x] Confirmed local branch `PhmHai0702/fe/api-boundary-real-first` is clean and currently points at `0757b0e`, same as `origin/develop`.
- [x] Re-read code after the latest `develop` pull/merge, including frontend API boundary files and backend Auth/User/Writing/Grading/Payment/Subscription controllers.
- [x] Updated this plan's repo snapshot because the active repo now includes `NomiWrite.Frontend/`, not backend-only code.
- [x] Confirmed backend Writing service is now merged into `develop` with real type/prompt/submission/sample-answer/time-remaining endpoints.
- [x] Confirmed real backend contracts currently exist for Auth, User, Writing, Grading, Payment, and Subscription.
- [x] Confirmed backend contracts still do not exist for dedicated dashboard summary, vocabulary list/mastered state, quiz generation, quiz attempts, notifications, forum, or usage quota.
- [x] Confirmed gateway still depends on a `ReverseProxy` config section, but tracked development config currently only has logging.
- [x] Updated next-work plan to prioritize gateway config, real-mode smoke testing, API mapper fixes discovered during smoke tests, and remaining frontend cleanup.
- [x] Rechecked backend route list and confirmed pulled backend code still has no Google/OAuth login endpoint yet.
- [x] Added frontend Google Identity Services integration: configured `NEXT_PUBLIC_GOOGLE_CLIENT_ID`, receives Google credential/id token, sends it to `POST /api/auth/google`, then saves backend-issued NomiWrite JWT.
- [x] Added tutor review request list UI to the result page.
- [x] Added payment status refresh, refund request creation, and refund request history UI to the upgrade page.
- [x] Confirmed frontend `npm run lint` and `npm run build` pass after the added UI.

## Current Status For Next Session

Next recommended task: prepare local gateway config and run real-mode smoke testing against backend services on `PhmHai0702/fe/api-boundary-real-first`.

Concrete first commands/files to work on:

1. Restore or create local Gateway `ReverseProxy` config because tracked `NomiWrite.Gateway/appsettings.json` was removed and `appsettings.Development.json` currently has logging only.
2. Start required backend services plus dependencies, then verify gateway routing for `/api/auth`, `/api/users`, `/api/writing`, `/api/grading`, `/api/payment`, and `/api/subscriptions`.
3. Run frontend in `NEXT_PUBLIC_API_MODE=real` and smoke test login/register, current user, writing types/prompts, submission submit, grading result, history, profile, subscription plans, checkout, promo code, and payment history.
4. Fix any frontend real-client mapper mismatches found during smoke tests, especially `MyAccountDto`, subscription cancel response shape, payment status response shape, grading history shape, and prompt/submission list DTO differences.
5. Add backend prompt seed data if `/api/writing/prompts` is empty after migrations.
6. Move duplicated guide data out of `app/guide/page.tsx` and `app/components/GuideModal.tsx`.
7. Add a cleanup check for page-level hard-coded API records.
8. Keep vocabulary and quiz mocked/pending in real mode until backend modules expose real endpoints.
9. Run frontend `npm run lint` and `npm run build`.
10. Update this Progress Log after finishing each backend integration slice.
