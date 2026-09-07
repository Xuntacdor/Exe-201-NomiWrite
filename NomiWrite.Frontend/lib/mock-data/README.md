# Mock Data Policy

Mock data is a temporary fallback for local development and demos while backend services are unfinished.

Rules:

- Keep fixtures in `lib/mock-data/` only.
- Pages and components should call `lib/api/client.ts`, not import fixtures directly.
- Prefer `NEXT_PUBLIC_API_MODE=real` once the matching backend endpoint works through the API Gateway.
- Delete obsolete fixtures after equivalent database-backed endpoints are available.

Current inventory:

| Area | Current frontend state | Backend priority |
| --- | --- | --- |
| Auth | Login/register now call `apiClient`; mock fallback lives in `auth.ts`. | Replace fallback once Auth service no longer throws `NotImplementedException`. |
| Writing setup | Writing types, topics, prompts, and coach guide still live in `app/write/page.tsx`. | Move to constants/API client next. |
| Result feedback | Fake feedback still lives in `app/result/page.tsx` and reads `localStorage.nomiwrite_submission`. | Replace with persisted feedback from submission/AI backend. |
| Dashboard | Stats and trend data still live in `app/dashboard/page.tsx`. | Replace with `/api/dashboard/summary`. |
| History | Submission list still lives in `app/history/page.tsx`. | Replace with `/api/submissions`. |
| Vocabulary | Word records still live in `app/vocabulary/page.tsx`. | Replace with `/api/vocabulary`. |
| Quiz | Question bank still lives in `app/quiz/page.tsx`. | Replace with quiz generation and attempt endpoints. |
| Profile | Achievements/progress still live in `app/profile/page.tsx`. | Replace or hide when user/profile service contract is ready. |
