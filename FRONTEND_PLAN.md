# NomiWrite Frontend Plan

Last updated: 2026-09-05

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
- User, Payment, and AI Coordinator services are still default/template minimal APIs with `/weatherforecast`, so frontend should keep those parts mocked until real contracts arrive.

Legacy Next.js MVP status:

Frontend is currently a polished static prototype in the legacy `nomiwrite` folder, not an API-ready app yet.

Main legacy frontend findings:

- `app/login/page.tsx` and `app/register/page.tsx` are visual forms only. Submit is an `<a href="/dashboard">`, no validation/session/error state.
- `app/write/page.tsx` owns writing types, topics, prompts, writing guide data, local essay state, basic word count, simple client-side AI Coach heuristics, and writes submission data to `localStorage`.
- `app/result/page.tsx` reads `localStorage.nomiwrite_submission`, calculates fake score from word count, and renders hard-coded grammar/vocabulary feedback.
- `app/dashboard/page.tsx`, `app/history/page.tsx`, `app/profile/page.tsx`, `app/vocabulary/page.tsx`, and `app/quiz/page.tsx` all contain local hard-coded data.
- `app/guide/page.tsx` and `app/components/GuideModal.tsx` duplicate large writing guide data.
- `app/components/AppShell.tsx` and `AppSidebar.tsx` provide desktop app layout, but mobile navigation still needs attention.
- `app/layout.tsx` uses `next/font/google` with Geist, which caused build failure in restricted network because Google Fonts could not be fetched.
- `README.md` is still the default create-next-app README and should be rewritten for NomiWrite later.
- Many Vietnamese UI strings appear mojibake/encoding-corrupted in file output, so UI text should be reviewed in browser and normalized while touching each feature.
- `app/guide/page.tsx.bak` exists beside the active guide page; review/remove it when guide data is centralized.
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

- Auth has real endpoints and should be integrated first through the gateway.
- User, Payment, and AI Coordinator still need final contracts before removing their frontend fallback behavior.
- Writing/submission/feedback/history/vocabulary/quiz pages must be converted from static MVP data to API-backed data as soon as those backend modules expose endpoints.

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

- [ ] Create `lib/types.ts`.
- [ ] Create `lib/constants/grammar-categories.ts`.
- [ ] Inventory every hard-coded sample array/object in `app/**` and decide whether it is static content, API data, or temporary fixture.
- [ ] Move writing types, topics, prompts, and guide content out of `app/write/page.tsx`.
- [ ] Move guide data out of `app/guide/page.tsx` and `app/components/GuideModal.tsx`.
- [ ] Move quiz questions out of `app/quiz/page.tsx`.
- [ ] Move vocabulary data out of `app/vocabulary/page.tsx`.
- [ ] Move dashboard/history/profile/result sample records out of page files.
- [ ] Create `lib/api/client.ts`, `mock-client.ts`, `real-client.ts`, and `routes.ts`.
- [ ] Implement typed real API functions against gateway routes.
- [ ] Keep typed mock responses only as fallback when real backend endpoints are not ready.
- [ ] Add a cleanup check that blocks feature completion if page-level hard-coded data remains.

### Phase 2 - Auth And Layout

- [ ] Convert `app/login/page.tsx` from static link to controlled form.
- [ ] Add email/password validation.
- [ ] Add loading and error states.
- [ ] Connect login/register to real `POST /api/auth/login` and `POST /api/auth/register` through the gateway.
- [ ] Store real access/refresh token response from backend; use mock session only when `NEXT_PUBLIC_API_MODE=mock`.
- [ ] Convert `app/register/page.tsx` to controlled form.
- [ ] Save selected level and target through profile/user API when available; otherwise store only through API client fallback.
- [ ] Add logout behavior in `AppSidebar`.
- [ ] Add simple route guard for app pages.
- [ ] Add mobile navigation for `AppShell`.

### Phase 3 - Writing Flow

- [ ] Connect writing type/topic selectors to shared constants.
- [ ] Add draft autosave keyed by user and writing type.
- [ ] Recover draft on page load.
- [ ] Validate minimum word count by writing type.
- [ ] Submit through `api.submitSubmission()`.
- [ ] Use real submission/grading endpoint when available; no direct `localStorage` submission handoff for production mode.
- [ ] Navigate to `/result?submissionId=...`.
- [ ] Keep client-side AI Coach suggestions as temporary helper until backend feedback exists.
- [ ] Remove local fake scoring logic once backend returns grading/feedback.

### Phase 4 - Result And Feedback

- [ ] Load result by `submissionId`.
- [ ] Add loading, error, and empty states.
- [ ] Render criteria scores from `WritingFeedback`.
- [ ] Render grammar errors from API-shaped data.
- [ ] Render vocabulary suggestions from API-shaped data.
- [ ] Remove hard-coded grammar/vocabulary feedback from `app/result/page.tsx`.
- [ ] Read persisted feedback from backend/database through API client in real mode.
- [ ] Keep AI score disclaimer visible.
- [ ] Add CTA to generate quiz from current submission.

### Phase 5 - Dashboard And History

- [ ] Replace dashboard hard-coded stats with `api.getDashboardSummary()`.
- [ ] Add empty dashboard state for new user.
- [ ] Use backend/database summary data for score trend and grammar error profile when available.
- [ ] Keep frontend aggregation only as fallback over API client fixture data.
- [ ] Replace history hard-coded list with `api.listSubmissions()`.
- [ ] Add search, filter, and sort.
- [ ] Link each row to `/result?submissionId=...`.

### Phase 6 - Vocabulary

- [ ] Replace local vocabulary array with `api.listVocabulary()`.
- [ ] Persist mastered state through `api.updateVocabularyMastered()`.
- [ ] Use database-backed vocabulary records in real mode.
- [ ] Keep search/category/mastered filters.
- [ ] Add empty states.
- [ ] Generate vocabulary quiz from filtered or weak words.

### Phase 7 - Quiz

- [ ] Split quiz source into grammar quiz and vocabulary quiz.
- [ ] Generate quiz through `api.generateQuiz()`.
- [ ] Save attempt through `api.submitQuizAttempt()`.
- [ ] Read quiz questions from backend/database in real mode, not static page arrays.
- [ ] Show score and wrong-answer review.
- [ ] Support quiz from a specific submission.
- [ ] Support quiz from weakest grammar categories.

### Phase 8 - Profile And Plan

- [ ] Load user profile through `api.getMe()`.
- [ ] Edit display name, current level, target type, and target band.
- [ ] Save profile through `api.updateMe()`.
- [ ] Show current plan and monthly usage.
- [ ] Remove mocked achievements when backend exposes achievements/progress; otherwise hide or label as demo-only.

### Phase 9 - Upgrade Payment Stub

- [ ] Keep billing toggle and payment method UI.
- [ ] Add card form validation.
- [ ] Submit checkout through real payment API when available; mock checkout only in local/demo mode.
- [ ] Show pending, success, and failure states.
- [ ] Prepare boundary for VNPay, VietQR, and MoMo.

### Phase 10 - Mock Data Removal And Real Backend Cutover

- [ ] Set default `.env.example` mode to real once backend gateway is ready for frontend integration.
- [ ] Verify every page uses `lib/api/client.ts` instead of local sample arrays.
- [ ] Delete obsolete fixtures from `lib/mock-data/` after equivalent backend/database data exists.
- [ ] Remove fake score generation, fake history generation, fake dashboard aggregation, and fake quiz/vocabulary records from production code paths.
- [ ] Confirm real API smoke tests: auth, current user, writing submit, grading/feedback, dashboard, history, vocabulary, quiz attempt, and payment status where available.
- [ ] Keep a small documented demo fixture set only if the team still needs offline presentation mode.

## Testing Checklist

- [ ] `npm run build`
- [ ] `npm run lint`
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
- [ ] Review npm audit output separately: current install reports 8 vulnerabilities from dependency tree.

## Current Status For Next Session

Next recommended task: start Phase 1 on a new branch from `develop` or the latest frontend integration branch, for example `PhmHai0702/fe/api-boundary-real-first`.

Concrete first commands/files to work on:

1. Create `NomiWrite.Frontend/lib/types.ts`.
2. Create `NomiWrite.Frontend/lib/api/routes.ts`, `client.ts`, `real-client.ts`, and `mock-client.ts`.
3. Wire real Auth endpoints first because backend already exposes `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, and `/api/auth/logout/{userId}`.
4. Inventory hard-coded data in `NomiWrite.Frontend/app/**`.
5. Move temporary fixtures into `lib/mock-data/` only when the equivalent backend endpoint is not ready.
6. Remove page-level hard-coded API records feature by feature as real endpoints become available.
7. Run frontend `npm run lint` and `npm run build`.
8. Update this Progress Log after finishing each backend integration slice.
