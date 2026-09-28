import type { Submission } from "./types";

export function localDayKey(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

export function countWritingDays(submissions: Submission[]) {
  const counts: Record<string, number> = {};
  const seen = new Set<string>();
  for (const submission of submissions) {
    if (submission.status === "draft" || seen.has(submission.id)) continue;
    const date = new Date(submission.submittedAt);
    if (Number.isNaN(date.getTime())) continue;
    seen.add(submission.id);
    const key = localDayKey(date);
    counts[key] = (counts[key] ?? 0) + 1;
  }
  return counts;
}

export function monthDays(year: number, month: number) {
  const offset = (new Date(year, month, 1).getDay() + 6) % 7;
  const length = new Date(year, month + 1, 0).getDate();
  return Array.from({ length: Math.ceil((offset + length) / 7) * 7 }, (_, index) =>
    index < offset || index >= offset + length ? null : new Date(year, month, index - offset + 1));
}
