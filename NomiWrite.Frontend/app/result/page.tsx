"use client";

import Link from "next/link";
import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  ArrowRight,
  BrainCircuit,
  ChevronDown,
  FileText,
  Info,
  Loader2,
  Sparkles,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { WritingFeedback } from "@/lib/types";

function getExcerpt(content: string): string {
  if (!content) return "";
  const normalized = content.replace(/\s+/g, " ").trim();
  return normalized.length > 180 ? `${normalized.slice(0, 180)}...` : normalized;
}

function ResultContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const submissionId = searchParams.get("submissionId");
  const [feedback, setFeedback] = useState<WritingFeedback | null>(null);
  const [essayExpanded, setEssayExpanded] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    if (!submissionId) {
      const missingTimer = setTimeout(() => {
        setLoading(false);
        setError("Missing submissionId. Open a graded essay from History or submit a new writing.");
      }, 0);
      return () => clearTimeout(missingTimer);
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      setError("");
      apiClient.getFeedback(submissionId)
        .then(result => {
          if (!ignore) setFeedback(result);
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load grading result yet.");
        })
        .finally(() => {
          if (!ignore) setLoading(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [router, submissionId]);

  const submission = feedback?.submission;
  const band = submission?.overallScore ?? 0;
  const criteriaScores = Object.entries(feedback?.criteriaScores ?? {}).filter(([, score]) => typeof score === "number");
  const excerpt = getExcerpt(submission?.content ?? "");

  return (
    <AppShell activePath="/write">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/dashboard" className="text-slate-400 hover:text-slate-600">Dashboard</Link>
          <ChevronDown className="h-3 w-3 -rotate-90 text-slate-300" />
          <span className="font-bold text-slate-900">Writing result</span>
        </div>
        <Link
          href={submissionId ? `/quiz?submissionId=${submissionId}` : "/quiz"}
          className="flex items-center gap-2 rounded-full bg-violet-600 px-4 py-2 text-xs font-bold text-white transition-all hover:bg-violet-700"
        >
          <BrainCircuit className="h-3.5 w-3.5" />
          Practice quiz
        </Link>
      </div>

      <div className="w-full space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading grading result
          </div>
        )}

        {!loading && error && (
          <div className="rounded-2xl border border-amber-200 bg-amber-50 p-5">
            <div className="mb-2 flex items-center gap-2">
              <AlertCircle className="h-4 w-4 text-amber-600" />
              <p className="text-sm font-extrabold text-amber-900">Result unavailable</p>
            </div>
            <p className="text-sm leading-relaxed text-amber-800">{error}</p>
            <Link href="/write" className="mt-4 inline-flex items-center gap-2 text-sm font-bold text-amber-700 hover:text-amber-900">
              Submit another writing <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        )}

        {!loading && feedback && submission && (
          <>
            <div className="flex items-start gap-3 rounded-2xl border border-slate-200 bg-slate-50 p-4">
              <FileText className="mt-0.5 h-4 w-4 shrink-0 text-slate-400" />
              <div className="min-w-0 flex-1">
                <div className="mb-1 flex flex-wrap items-center gap-2">
                  <span className="text-xs font-extrabold text-slate-700">{submission.topic}</span>
                  <span className="text-slate-300">/</span>
                  <span className="text-xs text-slate-500">{submission.wordCount} words</span>
                </div>
                {excerpt && <p className="line-clamp-2 text-xs italic leading-relaxed text-slate-500">&quot;{excerpt}&quot;</p>}
              </div>
            </div>

            <div className="relative overflow-hidden rounded-3xl">
              <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700" />
              <div className="relative flex flex-col items-start gap-6 p-7 sm:flex-row sm:items-center">
                <div className="shrink-0 text-center">
                  <div className="flex h-24 w-24 flex-col items-center justify-center rounded-2xl border border-white/20 bg-white/15 shadow-xl">
                    <p className="text-4xl font-extrabold leading-none text-white">{band || "--"}</p>
                    <p className="mt-1 text-xs font-semibold text-blue-200">Band Score</p>
                  </div>
                </div>
                <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-2">
                  {criteriaScores.length ? criteriaScores.map(([label, score]) => (
                    <div key={label} className="rounded-xl border border-white/10 bg-white/10 p-3">
                      <p className="mb-1 truncate text-xs font-medium text-blue-200">{label}</p>
                      <div className="flex items-center gap-2">
                        <p className="text-lg font-extrabold text-white">{score}</p>
                        <div className="h-1 flex-1 overflow-hidden rounded-full bg-white/20">
                          <div className="h-full rounded-full bg-white/70" style={{ width: `${(Number(score) / 9) * 100}%` }} />
                        </div>
                      </div>
                    </div>
                  )) : (
                    <div className="rounded-xl border border-white/10 bg-white/10 p-4 text-sm text-blue-50">
                      Criteria scores are not available yet.
                    </div>
                  )}
                </div>
              </div>
              <div className="relative px-7 pb-6">
                <div className="rounded-xl border border-white/10 bg-white/10 p-4">
                  <div className="flex items-start gap-2">
                    <Info className="mt-0.5 h-4 w-4 shrink-0 text-blue-200" />
                    <p className="text-sm leading-relaxed text-blue-50">
                      {submission.overallFeedback || "The backend grading service returned this submission without overall feedback."}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="flex items-start gap-2 rounded-xl border border-amber-200 bg-amber-50 p-3">
              <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-amber-600" />
              <p className="text-xs text-amber-700">
                AI scores are learning guidance, not an official exam result.
              </p>
            </div>

            {submission.content && (
              <div className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
                <button
                  type="button"
                  onClick={() => setEssayExpanded(value => !value)}
                  className="flex w-full items-center justify-between px-5 py-4 transition-all hover:bg-slate-50"
                >
                  <div className="flex items-center gap-2">
                    <FileText className="h-4 w-4 text-blue-500" />
                    <h3 className="text-sm font-extrabold text-slate-900">Submitted essay</h3>
                  </div>
                  <ChevronDown className={`h-4 w-4 text-slate-400 transition-transform ${essayExpanded ? "rotate-180" : ""}`} />
                </button>
                {essayExpanded && (
                  <div className="border-t border-slate-50 px-5 pb-5">
                    <div className="mt-4 max-h-80 overflow-y-auto rounded-xl border border-slate-100 bg-slate-50/50 p-4">
                      <p className="whitespace-pre-wrap text-sm leading-relaxed text-slate-700">{submission.content}</p>
                    </div>
                  </div>
                )}
              </div>
            )}

            <div className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center justify-between border-b border-slate-50 px-5 py-4">
                <div className="flex items-center gap-2">
                  <AlertCircle className="h-4 w-4 text-red-500" />
                  <h3 className="text-sm font-extrabold text-slate-900">Grammar feedback</h3>
                  <span className="rounded-full bg-red-50 px-2 py-0.5 text-xs font-bold text-red-600">{feedback.grammarErrors.length}</span>
                </div>
              </div>
              <div className="divide-y divide-slate-50">
                {feedback.grammarErrors.length ? feedback.grammarErrors.map(error => (
                  <div key={error.id} className="p-5">
                    <p className="mb-2 text-sm leading-relaxed text-slate-700">{error.sentence}</p>
                    <div className="grid gap-3 text-xs sm:grid-cols-2">
                      <div>
                        <p className="mb-0.5 font-semibold text-slate-400">Suggestion</p>
                        <p className="font-bold text-emerald-700">{error.suggestion}</p>
                      </div>
                      <div>
                        <p className="mb-0.5 font-semibold text-slate-400">Explanation</p>
                        <p className="leading-relaxed text-slate-600">{error.explanation}</p>
                      </div>
                    </div>
                  </div>
                )) : (
                  <p className="p-5 text-sm text-slate-500">No grammar issues were returned.</p>
                )}
              </div>
            </div>

            <div className="flex items-center justify-center gap-2 py-2 text-xs text-slate-400">
              <Sparkles className="h-3.5 w-3.5" />
              Analysis by NomiWrite AI
            </div>
          </>
        )}
      </div>
    </AppShell>
  );
}

export default function ResultPage() {
  return (
    <Suspense>
      <ResultContent />
    </Suspense>
  );
}
