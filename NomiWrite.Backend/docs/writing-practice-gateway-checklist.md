# Writing Practice Gateway Checklist

Use this checklist for Phase 4 smoke testing through the Gateway.

Gateway base URL:

- Local: `http://localhost:5097`
- Writing routes: `/api/writing/*`

## Catalog

1. Call `GET /api/writing/types`.
2. Confirm the catalog includes IELTS, TOEFL, PTE, Cambridge, VSTEP,
   professional, and academic writing formats.
3. Call `GET /api/writing/prompts?typeId=<typeId>`.
4. Call `GET /api/writing/prompts?difficulty=Advanced`.
5. Call `GET /api/writing/prompts?random=true`.
6. Confirm anonymous/free users do not see VIP-only prompts in the prompt list.
7. Confirm an authenticated premium user can see VIP-only prompts.
8. Confirm the IELTS Task 1 Academic chart prompt exposes `imageUrl`.

## Draft And Resume

1. Login through Auth and send `Authorization: Bearer <accessToken>` to every
   protected Writing route.
2. Create a draft with `POST /api/writing/submissions`.
3. Update the draft with `PUT /api/writing/submissions/{id}`.
4. Resume the draft with `GET /api/writing/submissions/{id}` and confirm the
   latest `content`, `wordCount`, `isTimed`, and `deadlineAt` are returned.
5. Confirm `GET /api/writing/submissions` returns only the current user's
   submissions.

## Timed Mode

1. Create a timed submission for a prompt with `timeLimitMinutes`.
2. Confirm the response includes `isTimed = true` and a non-null `deadlineAt`.
3. Call `GET /api/writing/submissions/{id}/time-remaining`.
4. Submit after the deadline in a lower environment if practical and confirm
   `submittedLate = true`.

Late submissions are accepted and still flow to grading. The backend records the
lateness so grading feedback and UI can show the penalty.

## VIP Gating

1. As a free user, call `GET /api/writing/prompts/{id}/sample-answer` for a
   prompt with a sample answer and confirm `403`.
2. As a premium user, call the same endpoint and confirm the sample answer is
   returned.
3. As a free user, try `POST /api/writing/submissions` with a VIP-only
   `writingPromptId` and confirm `403`.
4. As a premium user, create the same VIP-only submission and confirm a draft is
   created.

## Submit And Rewrite

1. Submit a draft with non-empty content using
   `POST /api/writing/submissions/{id}/submit`.
2. Confirm the response status is `Submitted` and a `WritingSubmittedEvent` is
   published.
3. Confirm submitting an already submitted draft returns `409`.
4. Create another submission for the same prompt after submit and confirm it is a
   new draft. This is the rewrite/resubmit workflow source of truth.

## Word And Character Limits

The backend is the source of truth for `wordCount`, `minWords`, `maxWords`, and
the hard `20000` character content limit. Frontend counters are UX helpers only.
