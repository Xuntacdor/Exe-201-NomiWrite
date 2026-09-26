"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import AppShell from "../components/AppShell";
import { AlertCircle, ChevronRight, Clock, Filter, Loader2, PenLine } from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { Submission } from "@/lib/types";

function bandColor(band?: number) {
  if (!band) return "text-muted";
  if (band >= 7.0) return "text-success-ink";
  if (band >= 6.0) return "text-accent-ink";
  return "text-accent-ink";
}

function bandBg(band?: number) {
  if (!band) return "bg-canvas border-line";
  if (band >= 7.0) return "bg-success border-line";
  if (band >= 6.0) return "bg-accent border-line";
  return "bg-accent border-line";
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(value));
}

export default function HistoryPage() {
  const router = useRouter();
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      Promise.all([
        apiClient.listSubmissions(),
        apiClient.listGradingHistory().catch(() => []),
      ])
        .then(([items, gradingHistory]) => {
          const scoreBySubmission = new Map(gradingHistory.map(item => [item.submissionId, item.overallBand]));
          const merged = items.map(item => ({
            ...item,
            overallScore: item.overallScore ?? scoreBySubmission.get(item.id),
            status: scoreBySubmission.has(item.id) ? "graded" as const : item.status,
          }));
          if (!ignore) setSubmissions(merged);
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load writing history.");
        })
        .finally(() => {
          if (!ignore) setLoading(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [router]);

  const gradedScores = submissions.map(item => item.overallScore).filter((score): score is number => typeof score === "number");
  const avg = gradedScores.length
    ? (gradedScores.reduce((sum, score) => sum + score, 0) / gradedScores.length).toFixed(1)
    : "--";
  const best = gradedScores.length ? Math.max(...gradedScores).toFixed(1) : "--";
  const sorted = useMemo(
    () => [...submissions].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()),
    [submissions],
  );

  return (
    <AppShell activePath="/history">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          <Clock className="h-4 w-4 text-muted" />
          <h1 className="text-sm font-bold text-ink">Writing history</h1>
        </div>
        <div className="flex items-center gap-2">
          <button className="flex items-center gap-1.5 rounded-lg bg-surface-muted px-3 py-1.5 text-xs font-semibold text-muted transition-colors hover:bg-surface-muted">
            <Filter className="h-3.5 w-3.5" />
            Filter
          </button>
          <Link href="/write" className="flex items-center gap-1.5 rounded-lg bg-accent px-3 py-1.5 text-xs font-bold text-ink transition-colors hover:bg-accent-hover">
            <PenLine className="h-3.5 w-3.5" />
            New writing
          </Link>
        </div>
      </div>

      <div className="w-full space-y-5 p-6">
        <div className="grid grid-cols-3 gap-4">
          {[
            { label: "Total", value: submissions.length, unit: " essays", color: "text-ink" },
            { label: "Average band", value: avg, unit: "", color: "text-accent-ink" },
            { label: "Best band", value: best, unit: "", color: "text-success-ink" },
          ].map(({ label, value, unit, color }) => (
            <div key={label} className="rounded-xl border border-line bg-surface p-4 text-center shadow-sm">
              <p className={`text-2xl font-bold ${color}`}>{value}<span className="ml-0.5 text-base font-semibold">{unit}</span></p>
              <p className="mt-0.5 text-xs text-muted">{label}</p>
            </div>
          ))}
        </div>

        <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
          <div className="grid grid-cols-12 gap-4 border-b border-line px-5 py-3 text-xs font-bold uppercase tracking-widest text-muted">
            <span className="col-span-5">Writing</span>
            <span className="col-span-2">Status</span>
            <span className="col-span-2 text-center">Band</span>
            <span className="col-span-2 text-center">Words</span>
            <span className="col-span-1" />
          </div>

          {loading && (
            <div className="flex items-center justify-center gap-2 p-8 text-sm font-semibold text-muted">
              <Loader2 className="h-4 w-4 animate-spin" />
              Loading history
            </div>
          )}

          {!loading && error && (
            <p className="p-5 text-sm font-semibold text-danger-ink">{error}</p>
          )}

          {!loading && !error && sorted.length === 0 && (
            <div className="p-8 text-center">
              <p className="text-sm font-bold text-ink">No submissions yet</p>
              <p className="mt-1 text-xs text-muted">Start a writing session to create your first record.</p>
            </div>
          )}

          {!loading && !error && sorted.length > 0 && (
            <div className="divide-y divide-line">
              {sorted.map(submission => (
                <Link
                  key={submission.id}
                  href={`/result?submissionId=${submission.id}`}
                  className="grid grid-cols-12 items-center gap-4 px-5 py-4 transition-colors hover:bg-canvas group"
                >
                  <div className="col-span-5">
                    <p className="truncate text-sm font-semibold text-ink transition-colors group-hover:text-accent-ink">{submission.topic}</p>
                    <p className="mt-0.5 text-xs text-muted">{formatDate(submission.submittedAt)}</p>
                  </div>
                  <div className="col-span-2">
                    <span className="rounded-full bg-surface-muted px-2 py-1 text-xs font-semibold text-muted">
                      {submission.status}
                    </span>
                  </div>
                  <div className="col-span-2 flex justify-center">
                    <span className={`rounded-xl border px-3 py-1 text-sm font-bold ${bandBg(submission.overallScore)} ${bandColor(submission.overallScore)}`}>
                      {submission.overallScore ?? "--"}
                    </span>
                  </div>
                  <div className="col-span-2 flex justify-center">
                    <div className="flex items-center gap-1.5">
                      <AlertCircle className="h-3.5 w-3.5 text-muted" />
                      <span className="text-sm font-semibold text-ink">{submission.wordCount}</span>
                    </div>
                  </div>
                  <div className="col-span-1 flex justify-end">
                    <ChevronRight className="h-4 w-4 text-muted transition-colors group-hover:text-accent-ink" />
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>


      </div>
    </AppShell>
  );
}
