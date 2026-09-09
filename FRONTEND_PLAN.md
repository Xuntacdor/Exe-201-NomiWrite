# NomiWrite Frontend Plan

Last updated: 2026-09-08

## Location

Canonical plan file: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite\FRONTEND_PLAN.md`

Active repo: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`

Legacy frontend prototype source: `D:\FPT_Uni\FALL26\EXE201\NomiWrite\nomiwrite`

Git repo info:

- Base branch: `develop`
- Current work branch for this plan update: `PhmHai0702/fe/frontend-plan-tracking`
- Remote: `git@github.com:Xuntacdor/Exe-201-NomiWrite.git`
- The active project repository is `FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`.
- This active repository currently contains backend code only; the Next.js MVP lives in the legacy frontend prototype source above.

Project naming note:

- Older docs use `WriteWise`.
- Current code and CP3 materials use `NomiWrite`.
- Frontend should display `NomiWrite` consistently unless the team decides to rename.

## Sources Already Read

- `SRS_WriteWise.md`
- `WriteWise_Presentation_CP1_2.docx`
- `NomiWrite_Presentation_CP3.docx`
- `NomiWrite-Project.pptx`
- `C:/Users/DELL/Downloads/Usecase List for NomiWrite.docx`
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/app/**` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/package.json` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/.gitignore` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/nomiwrite/eslint.config.mjs` legacy Next.js MVP
- `D:/FPT_Uni/FALL26/EXE201/NomiWrite/Exe-201-NomiWrite/NomiWrite.Backend/**`

Legacy location previously inspected before the workspace move:

- `D:/FPT_Uni/SUMMER26/EXE101/StaticMVP/nomiwrite`

Active development must use `D:\FPT_Uni\FALL26\EXE201\NomiWrite\Exe-201-NomiWrite`.

## Current Repo Snapshot

Tracked files in the active repo are backend solution and service files only:

- `NomiWrite.Backend/NomiWrite.sln`
- `NomiWrite.Backend/src/Gateway/NomiWrite.Gateway/**`
- `NomiWrite.Backend/src/Services/Auth/**`
- `NomiWrite.Backend/src/Services/User/**`
- `NomiWrite.Backend/src/Services/Payment/**`
- `NomiWrite.Backend/src/Services/AICoordinator/**`
- `NomiWrite.Backend/src/Shared/NomiWrite.Shared.Contracts/**`
- `.gitignore`

Current `git status --short` in the active repo before moving this plan:

- clean on `develop`

After moving this plan, `FRONTEND_PLAN.md` should be the only frontend-tracking file added until the frontend app location is agreed.

Important implication:

- The active repo currently has no Next.js frontend folder.
- Decide whether the frontend should be added as `NomiWrite.Frontend/`, `frontend/`, or another team-approved folder inside this repo.
- The legacy Next.js MVP can be copied/migrated into that frontend folder after the target location is agreed.
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

The existing Next.js MVP already has static UI for:

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
- Upgrade/payment stub

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

- The active `Exe-201-NomiWrite` repo currently contains backend code only.
- Gateway is implemented with YARP reverse proxy.
- Gateway routes are configured for `/api/auth/**`, `/api/payment/**`, `/api/users/**`, and `/api/ai/**`.
- Auth service has real controller endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, and `POST /api/auth/logout/{userId}`.
- Auth DTOs currently include email, password, fullName, accessToken, refreshToken, expiresAt, userId, and email.
- User, Payment, Writing, Grading, and Subscription now expose usable backend APIs for the main MVP flows.
- Features without backend contracts must show a frontend-ready `1/2` status shell instead of local fake records.

Legacy Next.js MVP status:

Frontend is currently a polished static prototype in the legacy `nomiwrite` folder, not an API-ready app yet.

Main legacy frontend findings:

- `app/login/page.tsx` and `app/register/page.tsx` are visual forms only. Submit is an `<a href="/dashboard">`, no validation/session/error state.
- `app/write/page.tsx` owns writing types, topics, prompts, writing guide data, local essay state, basic word count, simple client-side AI Coach heuristics, and writes submission data to `localStorage`.
- `app/result/page.tsx` now reads backend grading data by `submissionId`; old `localStorage` handoff and generated score were removed.
- `app/dashboard/page.tsx`, `app/history/page.tsx`, `app/profile/page.tsx`, `app/result/page.tsx`, `app/write/page.tsx`, and `app/upgrade/page.tsx` are connected to available backend APIs or frontend aggregation over backend responses.
- `app/vocabulary/page.tsx` and `app/quiz/page.tsx` are frontend-ready `1/2` screens waiting for backend modules; local vocabulary/quiz records were removed.
- `app/guide/page.tsx` and `app/components/GuideModal.tsx` duplicate large writing guide data.
- `app/components/AppShell.tsx` and `AppSidebar.tsx` provide desktop app layout, but mobile navigation still needs attention.
- `app/layout.tsx` uses `next/font/google` with Geist, which caused build failure in restricted network because Google Fonts could not be fetched.
- `README.md` is still the default create-next-app README and should be rewritten for NomiWrite later.
- Many Vietnamese UI strings appear mojibake/encoding-corrupted in file output, so UI text should be reviewed in browser and normalized while touching each feature.
- Guide content is still static duplicated content in `app/guide/page.tsx` and `app/components/GuideModal.tsx`; centralize it later or back it with CMS/content APIs.
- `node_modules` is not part of the frontend source and should be installed locally when verifying the frontend.

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
- No production page should use mock records.
- If backend is missing, keep the frontend route/UI ready and mark it `1/2` in this plan until the backend API is available.

## Backend Integration Priority

Backend/database integration should take priority over expanding frontend-only behavior.

Use this order for every feature area:

1. Connect to the real API Gateway endpoint if the backend contract exists.
2. If the endpoint is not ready, define the typed frontend contract in `lib/api/routes.ts` and keep a `1/2` UI shell that explains the required backend contract.
3. When the backend endpoint becomes available, implement the route in `real-client.ts`, verify the response shape, and mark the feature `2/2`.
4. Do not leave sample API records inside `app/**/page.tsx` or a fixture directory.

Current known backend status:

- Auth has real endpoints: `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh`, and `POST /api/auth/logout`.
- User profile has real endpoints: `GET /api/users/me`, `PUT /api/users/me`, and `GET /api/users/me/account`.
- Writing has real endpoints for types, prompts, draft creation, content update, submit, list, and detail under `/api/writing`.
- AI Coordinator has a real grading read endpoint: `GET /api/grading/submissions/{submissionId}`.
- Payment has real create/status endpoints under `/api/payment`, plus VNPay and MoMo IPN handlers.
- Subscription has real plan/status endpoints: `GET /api/subscriptions/plans` and `GET /api/subscriptions/me`.
- Dashboard, vocabulary, quiz, quiz-attempts, forum/community, notifications, admin/moderation, billing history, invoices, refunds, content CMS, and dedicated vocabulary suggestions are not exposed as real backend modules yet.
- Dashboard currently derives from Writing/User APIs. Vocabulary and Quiz are `1/2` frontend shells.
- Gateway still depends on a local `ReverseProxy` config, while tracked `NomiWrite.Gateway/appsettings.json` was removed from source after the latest backend merge.
- Local backend services require the backend team's database and RabbitMQ configuration. Without those, HTTP ports can listen but data APIs may timeout or return database/message-bus errors.

## Frontend MVP Scope

Do first:

- Auth UX connected to real backend auth.
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
      real-client.ts
      routes.ts
    constants/
      grammar-categories.ts
      writing-guides.ts
      writing-prompts.ts
    types.ts
  public/
  package.json
  next.config.ts
  tsconfig.json
```

Keep page files thin. Put data, types, and API logic outside pages.

The old frontend fixture directory has been removed. If the team needs demo fixtures later, keep them outside production source or behind an explicitly separate demo build.

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

## Status Scale

- `2/2`: Frontend UI is implemented and connected to a real backend/database API.
- `1/2`: Frontend route/UI/typed contract is prepared, but backend API/database support is still missing.
- `0/2`: Not implemented on frontend yet.

## API Contract

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
POST  /vocabulary/from-feedback

POST /quizzes/generate
GET  /quizzes/:id
POST /quiz-attempts
GET  /quiz-attempts?userId=me

POST /payments/checkout
GET  /payments/:id/status
```

Frontend implementation pattern:

- Add `.env.example`.
- Use `NEXT_PUBLIC_API_BASE_URL=http://localhost:5097` or backend team's gateway URL later.
- `lib/api/real-client.ts` wraps `fetch`.
- `lib/api/client.ts` exports the real client directly.
- Components call API client functions only.
- No page should import local API fixtures.
- Before marking a feature done, confirm it does not keep hard-coded sample records inside `app/**`.

## Usecase Gap Review From Word Doc

The Word usecase list is broader than the current MVP UI/backend. These frontend-relevant usecases must be added to the roadmap or backend backlog before the feature can be marked `2/2`.

Auth/account gaps:

- `1/2` UC01 OAuth registration/login UI entry points; backend OAuth provider flow is missing.
- `1/2` UC02 email verification UI/status; backend verification token flow is missing.
- `1/2` UC05 password reset request/confirm pages; backend reset flow is missing.
- `1/2` UC07 delete/deactivate account UI; backend account deletion API is missing.

Writing gaps:

- `1/2` UC09-UC10 full writing-type catalogue from Word: IELTS, TOEFL, PTE, Cambridge, VSTEP, professional, and academic formats need backend seed/content coverage.
- `1/2` UC12 timed session has UI toggle only; backend should persist timed metadata and remaining time if resume is required.
- `1/2` UC15-UC16 save/resume draft is local autosave plus backend draft creation; dedicated draft list/resume API UX still needs completion.
- `1/2` UC18 chart/image attachment for Task 1; frontend upload/view UI and backend storage API are missing.
- `1/2` UC19 sample/model answers for VIP; frontend library route and backend content/subscription gate are missing.
- `1/2` UC20 rewrite/resubmit previous prompt; frontend CTA and backend attempt linking are missing.

AI feedback gaps:

- `1/2` UC24 inline grammar/spelling highlights; result page has list rendering, but inline span offsets need backend data.
- `1/2` UC25 vocabulary suggestions; grading DTO does not expose saved vocabulary records yet.
- `1/2` UC26 sentence restructuring; backend response and frontend comparison UI are missing.
- `1/2` UC27 compare score to previous attempts; dashboard/result trend API is missing.
- `1/2` UC28 human/tutor feedback for VIP; tutor review workflow is missing.
- `1/2` UC29 flag AI result inaccurate; report form/API is missing.

Progress gaps:

- `1/2` UC31 score chart is derived minimally on dashboard; real analytics endpoint is missing.
- `1/2` UC32 strengths/weaknesses needs backend aggregation.
- `1/2` UC33 goals target band/exam date partly exists in profile; exam date API/UI is missing.
- `1/2` UC34 streak/reminder and UC35 badges are not implemented.

Community, notification, admin gaps:

- `0/2` UC36-UC47 forum/community module is not in frontend plan yet; add routes for forum list, post detail, create post, comments, peer feedback, bookmarks, report, follow, DM, and study groups after backend scope is confirmed.
- `0/2` UC67-UC70 notification center/preferences are not implemented and need backend events/preferences.
- `0/2` UC71-UC78 admin/moderator portal is not implemented and should likely live in a separate admin app after backend authorization is ready.

Payment/VIP gaps:

- `1/2` UC51 confirmation/invoice page, UC56 billing history, and UC57 refund flow need backend billing records.
- `1/2` UC53 downgrade, UC54 cancel, and UC55 promo code are not exposed by backend yet.
- `1/2` UC60 model answer library, UC61 exclusive prompts, UC62 tutor review, UC63 PDF report, UC64 ad-free, UC65 VIP forum badge/support, and UC66 full exam simulation/certificate need backend/API scope before frontend completion.

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
- [x] Add frontend `.env.example` with gateway base URL.
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
- [x] Remove local quiz questions from `app/quiz/page.tsx`; page is now `1/2` waiting for backend quiz API.
- [x] Remove local vocabulary data from `app/vocabulary/page.tsx`; page is now `1/2` waiting for backend vocabulary API.
- [x] Move dashboard/history/profile/result sample records out of page files.
- [x] Create `lib/api/client.ts`, `real-client.ts`, and `routes.ts`.
- [x] Implement typed real API functions against gateway routes.
- [x] Remove frontend mock API client and local fixture directory.
- [ ] Add a cleanup check that blocks feature completion if page-level hard-coded API records remain.

### Phase 2 - Auth And Layout

- [x] Convert `app/login/page.tsx` from static link to controlled form.
- [x] Add email/password validation.
- [x] Add loading and error states.
- [x] Connect login/register to real `POST /api/auth/login` and `POST /api/auth/register` through the gateway.
- [x] Store real access/refresh token response from backend.
- [x] Convert `app/register/page.tsx` to controlled form.
- [ ] Save selected level and target through profile/user API when available; otherwise store only through API client fallback.
- [ ] Add logout behavior in `AppSidebar`.
- [x] Add simple route guard for API-backed app pages in real mode.
- [ ] Add mobile navigation for `AppShell`.

### Phase 3 - Writing Flow

- [x] Connect writing type/topic selectors to backend `api.listWritingTypes()` and `api.listWritingPrompts()`.
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
- [ ] Render vocabulary suggestions from API-shaped data; backend grading DTO does not expose vocabulary suggestions yet.
- [x] Remove hard-coded grammar/vocabulary feedback from `app/result/page.tsx`.
- [x] Read persisted feedback from backend/database through API client in real mode.
- [x] Keep AI score disclaimer visible.
- [x] Add CTA to generate quiz from current submission.

### Phase 5 - Dashboard And History

- [x] Replace dashboard hard-coded stats with frontend aggregation over real `api.listSubmissions()` and `api.getMyAccount()` until a dedicated dashboard endpoint exists.
- [x] Add empty dashboard state for new user.
- [ ] Use backend/database summary data for score trend and grammar error profile when available.
- [x] Keep frontend aggregation only as fallback/derived behavior over API client data.
- [x] Replace history hard-coded list with `api.listSubmissions()`.
- [ ] Add search, filter, and sort.
- [x] Link each row to `/result?submissionId=...`.

### Phase 6 - Vocabulary (`1/2`)

- [x] Remove local vocabulary array and mock vocabulary records.
- [x] Add frontend-ready `1/2` route that documents required backend API.
- [x] Keep typed client boundary for `api.listVocabulary()` and `api.updateVocabularyMastered()`.
- [ ] Connect `api.listVocabulary()` after backend module exists.
- [ ] Persist mastered state through `api.updateVocabularyMastered()` after backend module exists.
- [ ] Restore search/category/mastered filters over real API data.
- [ ] Generate vocabulary quiz from filtered or weak words after quiz/vocabulary APIs exist.

### Phase 7 - Quiz (`1/2`)

- [x] Remove static quiz question bank from `app/quiz/page.tsx`.
- [x] Add frontend-ready `1/2` route that documents required backend API.
- [x] Keep typed client boundary for `api.generateQuiz()`, `api.getQuiz()`, and `api.submitQuizAttempt()`.
- [ ] Split quiz source into grammar quiz and vocabulary quiz after backend supports both.
- [ ] Generate quiz through `api.generateQuiz()` after backend module exists.
- [ ] Save attempt through `api.submitQuizAttempt()` after backend module exists.
- [ ] Read quiz questions from backend/database, not static page arrays.
- [ ] Show score and wrong-answer review over saved attempt data.
- [ ] Support quiz from a specific submission.
- [ ] Support quiz from weakest grammar categories.

### Phase 8 - Profile And Plan

- [x] Load user profile/account through `api.getMyAccount()` and fallback `api.getMe()`.
- [x] Edit display name, current level, target type, and target band.
- [x] Save profile through `api.updateMe()`.
- [x] Show current plan/subscription status where backend exposes it.
- [x] Remove mocked achievements; show API coverage/status instead until achievements/progress exists.

### Phase 9 - Upgrade Payment Stub

- [x] Keep billing toggle and backend-supported payment method UI.
- [x] Remove fake card form; backend currently supports provider checkout, not direct card capture.
- [x] Load subscription plans through `api.listSubscriptionPlans()` and pass `planId` into checkout when available.
- [x] Submit checkout through real payment API.
- [x] Show pending, success, and failure states.
- [x] Prepare boundary for VNPay, VietQR, and MoMo.

### Phase 10 - Mock Data Removal And Real Backend Cutover

- [x] Remove `NEXT_PUBLIC_API_MODE`; frontend now exports the real API client only.
- [x] Verify every completed page uses `lib/api/client.ts` instead of local sample arrays; completed for write, result, dashboard, history, profile, and upgrade.
- [x] Delete obsolete fixtures from `lib/mock-data/`.
- [x] Delete `lib/api/mock-client.ts`.
- [x] Remove fake score generation, fake history generation, and fake quiz/vocabulary records from production code paths.
- [x] Mark vocabulary and quiz as `1/2` instead of keeping mock/demo behavior.
- [ ] Confirm real API smoke tests: auth, current user, writing submit, grading/feedback, dashboard/history derived data, and payment status where available.

## Testing Checklist

- [x] `npm run build`
- [x] `npm run lint`
- [ ] Login validation works
- [ ] Register validation works
- [ ] Real login redirects to dashboard when backend auth is available
- [x] Login redirects use the real auth response only
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
- [x] Committed backend API integration and navbar/profile upgrade fixes as `ca19919 feat(frontend): connect available backend flows`.
- [x] Read `C:/Users/DELL/Downloads/Usecase List for NomiWrite.docx` and compared it with current plan/UI/backend coverage.
- [x] Added usecase gap review for Auth, Writing, AI feedback, Progress, Community, Notifications, Admin, Payment, and VIP scope.
- [x] Removed the frontend mock API client and deleted the old local fixture directory.
- [x] Switched `lib/api/client.ts` to real-client only and removed `NEXT_PUBLIC_API_MODE` from `.env.example`.
- [x] Replaced static quiz and vocabulary data with `1/2` frontend-ready pages that wait for backend APIs.
- [x] Updated README to describe real-only API integration and `1/2` status shells.
- [x] Confirmed frontend `npm run lint` and `npm run build` pass after mock data removal.
- [x] Added local gateway `ReverseProxy` development routes for Auth, User, Writing, Grading, Payment, and Subscription service ports.
- [x] Added development appsettings for User, Writing, AICoordinator, and Subscription, plus local overrides for Auth/Payment.
- [x] Confirmed `dotnet build NomiWrite.Backend/NomiWrite.sln` passes after running outside the sandbox so .NET can read NuGet config.
- [x] Started frontend, gateway, and backend HTTP services locally; ports are open, but data APIs still need the backend team's database and RabbitMQ configuration.

## Current Status For Next Session

Next recommended task: run real-mode smoke testing against locally running backend services on `PhmHai0702/fe/api-boundary-real-first`, then implement backend modules for the `1/2` frontend shells.

Concrete first commands/files to work on:

1. Restore or create local Gateway `ReverseProxy` config because tracked `NomiWrite.Gateway/appsettings.json` was removed by backend changes.
2. Move duplicated guide data out of `app/guide/page.tsx` and `app/components/GuideModal.tsx`.
3. Add backend prompt seed data if `/api/writing/prompts` is empty after migrations.
4. Add a cleanup check for page-level hard-coded API records.
5. Implement backend modules for vocabulary and quiz, then switch the `1/2` frontend shells to real data.
6. Run frontend `npm run lint` and `npm run build`.
7. Update this Progress Log after finishing each backend integration slice.
