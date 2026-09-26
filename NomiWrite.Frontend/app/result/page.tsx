"use client";

import Link from "next/link";
import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import AppDialog from "../components/AppDialog";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  ArrowRight,
  BrainCircuit,
  ChevronDown,
  FileText,
  Flag,
  Info,
  Loader2,
  MessagesSquare,
  RotateCcw,
  Sparkles,
  SplitSquareHorizontal,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { ApiRequestError } from "@/lib/api/real-client";
import { getSession } from "@/lib/auth/session";
import type { TutorReviewRequest, WritingFeedback } from "@/lib/types";

function getExcerpt(content: string): string {
  if (!content) return "";
  const normalized = content.replace(/\s+/g, " ").trim();
  return normalized.length > 180 ? `${normalized.slice(0, 180)}...` : normalized;
}

const GRADING_CHECK_INTERVAL_MS = 20000;
const GRADING_WAIT_TIMEOUT_MS = 10 * 60 * 1000;

function ResultContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const submissionId = searchParams.get("submissionId");
  const [feedback, setFeedback] = useState<WritingFeedback | null>(null);
  const [comparisonMessage, setComparisonMessage] = useState("");
  const [actionMessage, setActionMessage] = useState("");
  const [reviewRequests, setReviewRequests] = useState<TutorReviewRequest[]>([]);
  const [flagDialogOpen, setFlagDialogOpen] = useState(false);
  const [essayExpanded, setEssayExpanded] = useState(false);
  const [workingAction, setWorkingAction] = useState<"compare" | "tutor" | "flag" | "retry" | "">("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [waitingForGrading, setWaitingForGrading] = useState(false);
  const [retryVersion, setRetryVersion] = useState(0);

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
    let checkTimer: number | undefined;
    const startedAt = Date.now();

    const checkFeedback = () => {
      if (ignore) return;
      apiClient.getFeedback(submissionId)
        .then(result => {
          if (ignore) return;
          setFeedback(result);
          setWaitingForGrading(false);
          setError("");
        })
        .catch(err => {
          if (ignore) return;

          const message = err instanceof Error ? err.message : "Could not load grading result yet.";
          const isPendingResult =
            message.toLowerCase().includes("pending") ||
            (err instanceof ApiRequestError && err.status === 404);

          if (isPendingResult && Date.now() - startedAt < GRADING_WAIT_TIMEOUT_MS) {
            setWaitingForGrading(true);
            setError("");
            checkTimer = window.setTimeout(checkFeedback, GRADING_CHECK_INTERVAL_MS);
            return;
          }

          setWaitingForGrading(false);
          setError(message);
        })
        .finally(() => {
          if (!ignore) setLoading(false);
        });
    };

    checkFeedback();

    return () => {
      ignore = true;
      if (checkTimer) window.clearTimeout(checkTimer);
    };
  }, [router, submissionId, retryVersion]);

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) return;

    let ignore = false;
    apiClient.listTutorReviewRequests()
      .then(items => {
        if (!ignore) setReviewRequests(items.slice(0, 4));
      })
      .catch(() => {
        if (!ignore) setReviewRequests([]);
      });

    return () => {
      ignore = true;
    };
  }, []);

  const submission = feedback?.submission;
  const band = submission?.overallScore ?? 0;
  const criteriaScores = Object.entries(feedback?.criteriaScores ?? {}).filter(([, score]) => typeof score === "number");
  const excerpt = getExcerpt(submission?.content ?? "");
  const gradingResultId = feedback?.id;

  async function handleRetryGrading() {
    if (!submissionId) return;
    setWorkingAction("retry");
    setActionMessage("");
    try {
      await apiClient.retryGrading(submissionId);
      setFeedback(null);
      setWaitingForGrading(true);
      setRetryVersion(value => value + 1);
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : "Could not retry grading.");
    } finally {
      setWorkingAction("");
    }
  }

  async function handleCompare() {
    if (!submissionId) return;

    setWorkingAction("compare");
    setActionMessage("");
    setComparisonMessage("");
    try {
      const comparison = await apiClient.compareSubmissionFeedback(submissionId);
      const diff = comparison.bandDifference;
      setComparisonMessage(
        typeof diff === "number"
          ? `Band change versus previous feedback: ${diff > 0 ? "+" : ""}${diff.toFixed(1)}`
          : "No previous graded result is available for comparison yet.",
      );
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : "Could not compare grading results.");
    } finally {
      setWorkingAction("");
    }
  }

  async function handleTutorReview() {
    if (!submissionId) return;

    setWorkingAction("tutor");
    setActionMessage("");
    try {
      const request = await apiClient.requestTutorReview(submissionId);
      setActionMessage(`Tutor review request created: ${request.status}.`);
      setReviewRequests(items => [request, ...items.filter(item => item.id !== request.id)].slice(0, 4));
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : "Could not request tutor review.");
    } finally {
      setWorkingAction("");
    }
  }

  async function submitFlag(reason: string) {
    if (!gradingResultId) {
      setActionMessage("This result cannot be flagged because the grading result id is missing.");
      return;
    }

    if (!reason.trim()) {
      setActionMessage("Please enter a reason before flagging feedback.");
      return;
    }

    setWorkingAction("flag");
    setActionMessage("");
    try {
      await apiClient.flagFeedback(gradingResultId, { reason: reason.trim() });
      setFlagDialogOpen(false);
      setActionMessage("Feedback flag submitted.");
    } catch (err) {
      setActionMessage(err instanceof Error ? err.message : "Could not flag feedback.");
    } finally {
      setWorkingAction("");
    }
  }

  return (
    <AppShell activePath="/write">
      <AppDialog
        open={flagDialogOpen}
        title="Flag AI feedback"
        description="Tell the team what looks wrong so the grading result can be reviewed."
        confirmLabel="Submit flag"
        promptLabel="Reason"
        promptPlaceholder="Example: The grammar correction changes my intended meaning."
        onCancel={() => setFlagDialogOpen(false)}
        onConfirm={value => submitFlag(value ?? "")}
      />
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/dashboard" className="text-muted hover:text-muted">Dashboard</Link>
          <ChevronDown className="h-3 w-3 -rotate-90 text-muted" />
          <span className="font-bold text-ink">Writing result</span>
        </div>
        <Link
          href={submissionId ? `/quiz?submissionId=${submissionId}` : "/quiz"}
          className="flex items-center gap-2 rounded-full bg-rose px-4 py-2 text-xs font-bold text-ink transition-all hover:bg-rose"
        >
          <BrainCircuit className="h-3.5 w-3.5" />
          Practice quiz
        </Link>
      </div>

      <div className="w-full space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading grading result
          </div>
        )}

        {!loading && waitingForGrading && (
          <div className="flex min-h-[calc(100vh-9rem)] items-center justify-center">
            <div className="w-full max-w-xl rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
              <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-xl bg-accent text-accent-ink">
                <BrainCircuit className="h-8 w-8" />
              </div>
              <h1 className="text-2xl font-bold text-ink">AI is grading your writing</h1>
              <p className="mt-3 text-base leading-relaxed text-muted">
                Your submission is in the grading queue. This page refreshes automatically while NomiWrite prepares feedback.
              </p>
              <div className="mt-6 flex items-center justify-center gap-2 text-sm font-bold text-accent-ink">
                <Loader2 className="h-4 w-4 animate-spin" />
                Checking result...
              </div>
              <Link href="/history" className="mt-6 inline-flex items-center gap-2 text-sm font-bold text-muted hover:text-ink">
                View history <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        )}

        {!loading && error && !waitingForGrading && (
          <div className="rounded-xl border border-line bg-warning p-5">
            <div className="mb-2 flex items-center gap-2">
              <AlertCircle className="h-4 w-4 text-warning-ink" />
              <p className="text-sm font-bold text-warning-ink">Result unavailable</p>
            </div>
            <p className="text-base leading-relaxed text-warning-ink">{error}</p>
            <Link href="/write" className="mt-4 inline-flex items-center gap-2 text-sm font-bold text-warning-ink hover:text-warning-ink">
              Submit another writing <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        )}

        {!loading && feedback && submission && (
          <>
            <div className="flex items-start gap-3 rounded-xl border border-line bg-canvas p-4">
              <FileText className="mt-0.5 h-4 w-4 shrink-0 text-muted" />
              <div className="min-w-0 flex-1">
                <div className="mb-1 flex flex-wrap items-center gap-2">
                  <span className="text-xs font-bold text-ink">{submission.topic}</span>
                  <span className="text-muted">/</span>
                  <span className="text-xs text-muted">{submission.wordCount} words</span>
                </div>
                {excerpt && <p className="line-clamp-2 text-xs italic leading-relaxed text-muted">&quot;{excerpt}&quot;</p>}
              </div>
            </div>

            <div className="result-summary relative overflow-hidden rounded-xl">
              <div className="absolute inset-0 bg-accent" />
              <div className="relative flex flex-col items-start gap-6 p-7 sm:flex-row sm:items-center">
                <div className="shrink-0 text-center">
                  <div className="flex h-24 w-24 flex-col items-center justify-center rounded-xl border border-line bg-surface shadow-sm">
                    <p className="text-4xl font-bold leading-none text-ink">{band || "--"}</p>
                    <p className="mt-1 text-xs font-semibold text-accent-ink">Band Score</p>
                  </div>
                </div>
                <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-2">
                  {criteriaScores.length ? criteriaScores.map(([label, score]) => (
                    <div key={label} className="rounded-xl border border-line bg-surface p-3">
                      <p className="mb-1 truncate text-xs font-medium text-accent-ink">{label}</p>
                      <div className="flex items-center gap-2">
                        <p className="text-lg font-bold text-ink">{score}</p>
                        <div className="h-1 flex-1 overflow-hidden rounded-full bg-surface">
                          <div className="h-full rounded-full bg-surface" style={{ width: `${(Number(score) / 9) * 100}%` }} />
                        </div>
                      </div>
                    </div>
                  )) : (
                    <div className="rounded-xl border border-line bg-surface p-4 text-sm text-accent-ink">
                      Criteria scores are not available yet.
                    </div>
                  )}
                </div>
              </div>
              <div className="relative px-7 pb-6">
                <div className="rounded-xl border border-line bg-surface p-4">
                  <div className="flex items-start gap-2">
                    <Info className="mt-0.5 h-4 w-4 shrink-0 text-accent-ink" />
                    <p className="text-base leading-relaxed text-accent-ink">
                      {submission.status === "failed"
                        ? "AI grading is temporarily unavailable. Your essay is saved and can be graded again."
                        : submission.overallFeedback || "The backend grading service returned this submission without overall feedback."}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {submission.status === "failed" && (
              <div className="flex flex-wrap items-center gap-3 rounded-lg border border-line bg-warning p-4">
                <button
                  type="button"
                  onClick={handleRetryGrading}
                  disabled={Boolean(workingAction)}
                  className="inline-flex items-center gap-2 rounded-md bg-accent px-4 py-2 text-sm font-semibold text-ink hover:bg-accent-hover disabled:opacity-60"
                >
                  {workingAction === "retry" ? <Loader2 className="h-4 w-4 animate-spin" /> : <RotateCcw className="h-4 w-4" />}
                  Grade again
                </button>
                {actionMessage && <p className="text-sm text-warning-ink">{actionMessage}</p>}
              </div>
            )}

            <div className="flex items-start gap-2 rounded-xl border border-line bg-warning p-3">
              <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-warning-ink" />
              <p className="text-xs text-warning-ink">
                AI scores are learning guidance, not an official exam result.
              </p>
            </div>

            {submission.status !== "failed" && <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
              {[
                { label: "Compare", icon: SplitSquareHorizontal, action: handleCompare, key: "compare" as const },
                { label: "Tutor review", icon: MessagesSquare, action: handleTutorReview, key: "tutor" as const },
                { label: "Flag feedback", icon: Flag, action: () => setFlagDialogOpen(true), key: "flag" as const },
              ].map(({ label, icon: Icon, action, key }) => (
                <button
                  key={label}
                  type="button"
                  onClick={action}
                  disabled={Boolean(workingAction)}
                  className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface px-4 py-3 text-xs font-bold text-ink shadow-sm transition-all hover:border-line hover:bg-accent-hover hover:text-accent-ink disabled:opacity-60"
                >
                  {workingAction === key ? <Loader2 className="h-4 w-4 animate-spin" /> : <Icon className="h-4 w-4" />}
                  {label}
                </button>
              ))}
            </div>}

            {(comparisonMessage || actionMessage) && (
              <div className="rounded-xl border border-line bg-accent p-3 text-xs font-semibold text-accent-ink">
                {comparisonMessage || actionMessage}
              </div>
            )}

            <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
              <div className="flex items-center justify-between border-b border-line px-5 py-4">
                <div className="flex items-center gap-2">
                  <MessagesSquare className="h-4 w-4 text-rose-ink" />
                  <h3 className="text-sm font-bold text-ink">Tutor review requests</h3>
                </div>
              </div>
              {reviewRequests.length ? (
                <div className="divide-y divide-line">
                  {reviewRequests.map(request => (
                    <div key={request.id} className="flex items-center justify-between gap-3 px-5 py-3">
                      <div className="min-w-0">
                        <p className="truncate text-xs font-bold text-ink">Submission {request.submissionId}</p>
                        <p className="text-[11px] text-muted">
                          {new Date(request.requestedAt).toLocaleString()}
                        </p>
                      </div>
                      <span className="rounded-full bg-rose px-2.5 py-1 text-[11px] font-bold text-rose-ink">
                        {request.status}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="p-5 text-sm text-muted">No tutor review requests were returned.</p>
              )}
            </div>

            {submission.content && (
              <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <button
                  type="button"
                  onClick={() => setEssayExpanded(value => !value)}
                  className="flex w-full items-center justify-between px-5 py-4 transition-all hover:bg-canvas"
                >
                  <div className="flex items-center gap-2">
                    <FileText className="h-4 w-4 text-accent-ink" />
                    <h3 className="text-sm font-bold text-ink">Submitted essay</h3>
                  </div>
                  <ChevronDown className={`h-4 w-4 text-muted transition-transform ${essayExpanded ? "rotate-180" : ""}`} />
                </button>
                {essayExpanded && (
                  <div className="border-t border-line px-5 pb-5">
                    <div className="mt-4 max-h-80 overflow-y-auto rounded-xl border border-line bg-canvas p-4">
                      <p className="whitespace-pre-wrap text-base leading-relaxed text-ink">{submission.content}</p>
                    </div>
                  </div>
                )}
              </div>
            )}

            <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
              <div className="flex items-center justify-between border-b border-line px-5 py-4">
                <div className="flex items-center gap-2">
                  <AlertCircle className="h-4 w-4 text-danger-ink" />
                  <h3 className="text-sm font-bold text-ink">Grammar feedback</h3>
                  <span className="rounded-full bg-danger px-2 py-0.5 text-xs font-bold text-danger-ink">{feedback.grammarErrors.length}</span>
                </div>
              </div>
              <div className="divide-y divide-line">
                {feedback.grammarErrors.length ? feedback.grammarErrors.map(error => (
                  <div key={error.id} className="p-5">
                    <p className="mb-2 text-base leading-relaxed text-ink">{error.sentence}</p>
                    <div className="grid gap-3 text-xs sm:grid-cols-2">
                      <div>
                        <p className="mb-0.5 font-semibold text-muted">Suggestion</p>
                        <p className="font-bold text-success-ink">{error.suggestion}</p>
                      </div>
                      <div>
                        <p className="mb-0.5 font-semibold text-muted">Explanation</p>
                        <p className="leading-relaxed text-muted">{error.explanation}</p>
                      </div>
                    </div>
                  </div>
                )) : (
                  <p className="p-5 text-sm text-muted">No grammar issues were returned.</p>
                )}
              </div>
            </div>

            <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
              <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <div className="flex items-center justify-between border-b border-line px-5 py-4">
                  <div className="flex items-center gap-2">
                    <Sparkles className="h-4 w-4 text-success-ink" />
                    <h3 className="text-sm font-bold text-ink">Vocabulary suggestions</h3>
                    <span className="rounded-full bg-success px-2 py-0.5 text-xs font-bold text-success-ink">{feedback.vocabSuggestions.length}</span>
                  </div>
                </div>
                <div className="divide-y divide-line">
                  {feedback.vocabSuggestions.length ? feedback.vocabSuggestions.map(item => (
                    <div key={item.id} className="p-5">
                      <p className="text-xs font-semibold text-muted">{item.originalWord}</p>
                      <p className="mt-1 text-sm font-bold text-success-ink">{item.suggestedWord}</p>
                      {item.exampleSentence && <p className="mt-2 text-base leading-relaxed text-muted">{item.exampleSentence}</p>}
                    </div>
                  )) : (
                    <p className="p-5 text-sm text-muted">No vocabulary suggestions were returned.</p>
                  )}
                </div>
              </div>

              <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <div className="flex items-center justify-between border-b border-line px-5 py-4">
                  <div className="flex items-center gap-2">
                    <SplitSquareHorizontal className="h-4 w-4 text-accent-ink" />
                    <h3 className="text-sm font-bold text-ink">Rewrite suggestions</h3>
                    <span className="rounded-full bg-accent px-2 py-0.5 text-xs font-bold text-accent-ink">{feedback.restructuringSuggestions?.length ?? 0}</span>
                  </div>
                </div>
                <div className="divide-y divide-line">
                  {feedback.restructuringSuggestions?.length ? feedback.restructuringSuggestions.map(item => (
                    <div key={item.id} className="p-5">
                      <p className="text-base leading-relaxed text-muted">{item.originalSentence}</p>
                      <p className="mt-2 text-sm font-bold text-accent-ink">{item.suggestedRewrite}</p>
                      {item.reason && <p className="mt-2 text-base leading-relaxed text-muted">{item.reason}</p>}
                    </div>
                  )) : (
                    <p className="p-5 text-sm text-muted">No rewrite suggestions were returned.</p>
                  )}
                </div>
              </div>
            </div>

            <div className="flex items-center justify-center gap-2 py-2 text-xs text-muted">
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
