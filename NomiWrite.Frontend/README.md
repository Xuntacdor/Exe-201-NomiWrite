# NomiWrite Frontend

Next.js frontend MVP for NomiWrite, an English writing practice app that turns essays into structured feedback, grammar-error insights, vocabulary suggestions, quizzes, and progress tracking.

## Tech Stack

- Next.js 16
- React 19
- TypeScript
- Tailwind CSS v4
- lucide-react

## Local Setup

Run these commands from `NomiWrite.Frontend/`.

Copy environment defaults if needed:

```bash
cp .env.example .env.local
```

Install dependencies:

```bash
npm install
```

Run the development server:

```bash
npm run dev
```

Open `http://localhost:3000`.

## Useful Commands

```bash
npm run build
npm run lint
```

## Backend Integration

Backend and database are being developed in parallel. The frontend now uses the real API client only; pages whose backend modules are not ready show a `1/2` status shell instead of local sample data.

Default API base URL:

```text
NEXT_PUBLIC_API_BASE_URL=http://localhost:5097
```

Planned API boundary:

- Auth: login, register, logout, current user
- Writing: submit essay, get feedback/result, list submissions
- Dashboard: score trend, common error profile, recent activity
- Vocabulary: list saved words, update mastered state, quiz from saved words
- Quiz: generate quiz from errors/vocabulary, submit quiz attempt
- Payment: create checkout session for VNPay, VietQR, or MoMo

Track implementation progress in `FRONTEND_PLAN.md`.
