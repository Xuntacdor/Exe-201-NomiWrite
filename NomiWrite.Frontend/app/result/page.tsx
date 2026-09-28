"use client";

import { useLocale } from "@/lib/i18n/locale";


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
  Check,
  FolderPlus,
  X,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { ApiRequestError } from "@/lib/api/real-client";
import { getSession } from "@/lib/auth/session";
import type { TutorReviewRequest, WritingFeedback, VocabGroup, VocabSuggestion } from "@/lib/types";

function getExcerpt(content: string): string {
  if (!content) return "";
  const normalized = content.replace(/\s+/g, " ").trim();
  return normalized.length > 180 ? `${normalized.slice(0, 180)}...` : normalized;
}

const GRADING_CHECK_INTERVAL_MS = 20000;
const GRADING_WAIT_TIMEOUT_MS = 10 * 60 * 1000;

function ResultContent() {
  const { t: translateUi, errorText, locale } = useLocale();
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

  // Vocab grouping state
  const [isGrouping, setIsGrouping] = useState(false);
  const [selectedVocabIds, setSelectedVocabIds] = useState<Set<string>>(new Set());
  const [createGroupOpen, setCreateGroupOpen] = useState(false);
  const [creatingGroup, setCreatingGroup] = useState(false);
  const [vocabGroups, setVocabGroups] = useState<VocabGroup[]>([]);
  const [realVocabs, setRealVocabs] = useState<VocabSuggestion[]>([]);
  const [selectedExistingGroupId, setSelectedExistingGroupId] = useState("");
  const [newGroupName, setNewGroupName] = useState("");
  const [groupError, setGroupError] = useState("");

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
    if (!submissionId || !feedback?.submission?.id) return;
    let ignore = false;
    apiClient.listVocabGroups().then(groups => {
      if (!ignore) setVocabGroups(groups);
    }).catch(console.error);
    apiClient.listVocabularyBySubmission(feedback.submission.id).then(vocabs => {
      if (!ignore) setRealVocabs(vocabs);
    }).catch(console.error);
    return () => { ignore = true; };
  }, [feedback?.submission?.id, submissionId]);

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
      setActionMessage("Tutor review request created.");
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

  async function submitGroupSelection() {
    setCreatingGroup(true);
    try {
      const selectedArr = Array.from(selectedVocabIds);
      // Refresh to make sure we have the latest items from the database in case of RabbitMQ delay
      let latestRealVocabs = realVocabs;
      if (feedback?.submission?.id) {
        try {
          latestRealVocabs = await apiClient.listVocabularyBySubmission(feedback.submission.id);
          setRealVocabs(latestRealVocabs);
        } catch (err) {
          console.error("Failed to fetch latest vocabs", err);
        }
      }

      if (latestRealVocabs.length === 0) {
        throw new Error("The vocabulary is still being saved to your notebook in the background. Please wait a few seconds and try again.");
      }

      const realGuidsToGroup: string[] = [];

      for (const fakeId of selectedArr) {
        const fakeVocab = feedback?.vocabSuggestions.find(v => v.id === fakeId);
        if (fakeVocab) {
          const realVocab = latestRealVocabs.find(v =>
            v.originalWord.trim().toLowerCase() === fakeVocab.originalWord.trim().toLowerCase()
          );
          if (realVocab) {
            realGuidsToGroup.push(realVocab.id);
          } else {
            console.warn("Could not match fakeVocab:", fakeVocab, "against realVocabs:", latestRealVocabs);
          }
        }
      }

      const uniqueRealGuids = Array.from(new Set(realGuidsToGroup));
      if (uniqueRealGuids.length === 0 && selectedArr.length > 0) {
        throw new Error("Could not match the selected AI vocabulary to your saved notebook. This might be a syncing issue.");
      }

      if (selectedExistingGroupId) {
        // Add to existing group
        const group = vocabGroups.find(g => g.id === selectedExistingGroupId);
        if (!group) throw new Error("Selected group not found");

        // Filter out ones already in the group
        const newIds = uniqueRealGuids.filter(id => !group.vocabularyIds.includes(id));
        if (newIds.length === 0) {
          throw new Error("All selected words are already in this group.");
        }
        await apiClient.addVocabGroupItems(selectedExistingGroupId, { vocabularyIds: newIds });
      } else if (newGroupName.trim()) {
        // Create new group
        await apiClient.createVocabGroup({
          name: newGroupName.trim(),
          vocabularyIds: uniqueRealGuids
        });
      } else {
        throw new Error("Please select an existing group or enter a new group name.");
      }

      // Refresh groups
      const updatedGroups = await apiClient.listVocabGroups();
      setVocabGroups(updatedGroups);

      setCreateGroupOpen(false);
      setIsGrouping(false);
      setSelectedVocabIds(new Set());
      setNewGroupName("");
      setSelectedExistingGroupId("");
      setGroupError("");
    } catch (err) {
      console.error(err);
      setGroupError(err instanceof Error ? err.message : "Failed to group vocabulary");
    } finally {
      setCreatingGroup(false);
    }
  }

  function toggleVocabSelection(id: string) {
    const next = new Set(selectedVocabIds);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }
    setSelectedVocabIds(next);
  }

  function getGroupsForFakeId(fakeId: string) {
    const fakeVocab = feedback?.vocabSuggestions.find(v => v.id === fakeId);
    if (!fakeVocab) return [];
    const realVocab = realVocabs.find(v =>
      v.originalWord === fakeVocab.originalWord &&
      v.suggestedWord.includes(fakeVocab.suggestedWord)
    );
    if (!realVocab) return [];
    return vocabGroups.filter(g => g.vocabularyIds.includes(realVocab.id));
  }

  async function handleRemoveGroupItem(groupId: string, fakeId: string) {
    try {
      const realId = getGroupsForFakeId(fakeId)[0]?.vocabularyIds.find(id => realVocabs.find(rv => rv.id === id)?.originalWord === feedback?.vocabSuggestions.find(v => v.id === fakeId)?.originalWord);
      // Wait, a better way to find the realId is:
      const fakeVocab = feedback?.vocabSuggestions.find(v => v.id === fakeId);
      const realVocab = realVocabs.find(v => v.originalWord === fakeVocab?.originalWord && v.suggestedWord.includes(fakeVocab?.suggestedWord ?? ""));
      if (!realVocab) return;

      await apiClient.removeVocabGroupItem(groupId, realVocab.id);

      // Update local state to reflect removal immediately
      setVocabGroups(groups => groups.map(g => {
        if (g.id === groupId) {
          return { ...g, vocabularyIds: g.vocabularyIds.filter(id => id !== realVocab.id) };
        }
        return g;
      }));
    } catch (err) {
      console.error(err);
      alert(err instanceof Error ? err.message : "Failed to remove from group");
    }
  }

  return (
    <AppShell activePath="/write">
      <AppDialog
        open={flagDialogOpen}
        title={translateUi("Flag AI feedback")}
        description="Tell the team what looks wrong so the grading result can be reviewed."
        confirmLabel="Submit flag"
        promptLabel="Reason"
        promptPlaceholder="Example: The grammar correction changes my intended meaning."
        onCancel={() => setFlagDialogOpen(false)}
        onConfirm={value => submitFlag(value ?? "")}
      />

      <AppDialog
        open={createGroupOpen}
        title="Group Vocabulary"
        description={`Select an existing group or create a new one for the ${selectedVocabIds.size} selected words.`}
        confirmLabel={creatingGroup ? "Saving..." : "Save"}
        onCancel={() => { setCreateGroupOpen(false); setNewGroupName(""); setSelectedExistingGroupId(""); setGroupError(""); }}
        onConfirm={() => submitGroupSelection()}
      >
        <div className="space-y-4">
          {groupError && (
            <div className="rounded-xl border border-line bg-danger p-3 text-sm font-semibold text-danger-ink">
              {groupError}
            </div>
          )}
          {vocabGroups.length > 0 && (
            <div>
              <label className="mb-1 block text-xs font-bold uppercase tracking-wider text-muted">Add to existing group</label>
              <select
                value={selectedExistingGroupId}
                onChange={e => { setSelectedExistingGroupId(e.target.value); setNewGroupName(""); setGroupError(""); }}
                className="w-full rounded-xl border border-line bg-canvas px-3 py-2.5 text-sm font-medium text-ink focus:border-focus focus:bg-surface focus:outline-none"
              >
                <option value="">-- Select a group --</option>
                {vocabGroups.map(g => (
                  <option key={g.id} value={g.id}>{g.name}</option>
                ))}
              </select>
            </div>
          )}
          <div>
            <label className="mb-1 block text-xs font-bold uppercase tracking-wider text-muted">Or create new group</label>
            <input
              type="text"
              value={newGroupName}
              onChange={e => { setNewGroupName(e.target.value); setSelectedExistingGroupId(""); setGroupError(""); }}
              placeholder="e.g., Academic words"
              className="w-full rounded-xl border border-line bg-canvas px-3 py-2.5 text-sm font-medium text-ink placeholder:text-muted focus:border-focus focus:bg-surface focus:outline-none"
            />
          </div>
        </div>
      </AppDialog>
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/dashboard" className="text-muted hover:text-muted">{translateUi("Dashboard")}</Link>
          <ChevronDown className="h-3 w-3 -rotate-90 text-muted" />
          <span className="font-bold text-ink">{translateUi("Writing result")}</span>
        </div>
        <Link
          href={submissionId ? `/quiz?submissionId=${submissionId}` : "/quiz"}
          className="flex items-center gap-2 rounded-full bg-rose px-4 py-2 text-xs font-bold text-ink transition-all hover:bg-rose"
        >
          <BrainCircuit className="h-3.5 w-3.5" />
          {translateUi("Practice quiz")}</Link>
      </div>

      <div className="w-full space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            {translateUi("Loading grading result")}</div>
        )}

        {!loading && waitingForGrading && (
          <div className="flex min-h-[calc(100vh-9rem)] items-center justify-center">
            <div className="w-full max-w-xl rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
              <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-xl bg-accent text-accent-ink">
                <BrainCircuit className="h-8 w-8" />
              </div>
              <h1 className="text-2xl font-bold text-ink">{translateUi("AI is grading your writing")}</h1>
              <p className="mt-3 text-base leading-relaxed text-muted">
                {translateUi("Your submission is in the grading queue. This page refreshes automatically while NomiWrite prepares feedback.")}</p>
              <div className="mt-6 flex items-center justify-center gap-2 text-sm font-bold text-accent-ink">
                <Loader2 className="h-4 w-4 animate-spin" />
                {translateUi("Checking result...")}</div>
              <Link href="/history" className="mt-6 inline-flex items-center gap-2 text-sm font-bold text-muted hover:text-ink">
                {translateUi("View history ")}<ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        )}

        {!loading && error && !waitingForGrading && (
          <div className="rounded-xl border border-line bg-warning p-5">
            <div className="mb-2 flex items-center gap-2">
              <AlertCircle className="h-4 w-4 text-warning-ink" />
              <p className="text-sm font-bold text-warning-ink">{translateUi("Result unavailable")}</p>
            </div>
            <p className="text-base leading-relaxed text-warning-ink">{errorText(error)}</p>
            <Link href="/write" className="mt-4 inline-flex items-center gap-2 text-sm font-bold text-warning-ink hover:text-warning-ink">
              {translateUi("Submit another writing ")}<ArrowRight className="h-4 w-4" />
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
                  <span className="text-xs text-muted">{translateUi(submission.wordCount)} {translateUi(" words")}</span>
                </div>
                {excerpt && <p className="line-clamp-2 text-xs italic leading-relaxed text-muted">&quot;{translateUi(excerpt)}&quot;</p>}
              </div>
            </div>

            <div className="result-summary relative overflow-hidden rounded-xl">
              <div className="absolute inset-0 bg-accent" />
              <div className="relative flex flex-col items-start gap-6 p-7 sm:flex-row sm:items-center">
                <div className="shrink-0 text-center">
                  <div className="flex h-24 w-24 flex-col items-center justify-center rounded-xl border border-line bg-surface shadow-sm">
                    <p className="text-4xl font-bold leading-none text-ink">{translateUi(band || "--")}</p>
                    <p className="mt-1 text-xs font-semibold text-accent-ink">{translateUi("Band Score")}</p>
                  </div>
                </div>
                <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-2">
                  {criteriaScores.length ? criteriaScores.map(([label, score]) => (
                    <div key={label} className="rounded-xl border border-line bg-surface p-3">
                      <p className="mb-1 truncate text-xs font-medium text-accent-ink">{translateUi(label)}</p>
                      <div className="flex items-center gap-2">
                        <p className="text-lg font-bold text-ink">{score}</p>
                        <div className="h-1 flex-1 overflow-hidden rounded-full bg-surface">
                          <div className="h-full rounded-full bg-surface" style={{ width: `${(Number(score) / 9) * 100}%` }} />
                        </div>
                      </div>
                    </div>
                  )) : (
                    <div className="rounded-xl border border-line bg-surface p-4 text-sm text-accent-ink">
                      {translateUi("Criteria scores are not available yet.")}</div>
                  )}
                </div>
              </div>
              <div className="relative px-7 pb-6">
                <div className="rounded-xl border border-line bg-surface p-4">
                  <div className="flex items-start gap-2">
                    <Info className="mt-0.5 h-4 w-4 shrink-0 text-accent-ink" />
                    <p className="text-base leading-relaxed text-accent-ink">
                      {translateUi(submission.status === "failed"
                        ? "AI grading is temporarily unavailable. Your essay is saved and can be graded again."
                        : submission.overallFeedback || "The backend grading service returned this submission without overall feedback.")}
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
                  {translateUi("Grade again")}</button>
                {actionMessage && <p className="text-sm text-warning-ink">{errorText(actionMessage)}</p>}
              </div>
            )}

            <div className="flex items-start gap-2 rounded-xl border border-line bg-warning p-3">
              <AlertCircle className="mt-0.5 h-4 w-4 shrink-0 text-warning-ink" />
              <p className="text-xs text-warning-ink">
                {translateUi("AI scores are learning guidance, not an official exam result.")}</p>
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
                  {translateUi(label)}
                </button>
              ))}
            </div>}

            {(comparisonMessage || actionMessage) && (
              <div className="rounded-xl border border-line bg-accent p-3 text-xs font-semibold text-accent-ink">
                {comparisonMessage
                  ? comparisonMessage.startsWith("Band change versus previous feedback: ")
                    ? translateUi("Band change versus previous feedback: {difference}", { difference: comparisonMessage.slice("Band change versus previous feedback: ".length) })
                    : translateUi(comparisonMessage)
                  : errorText(actionMessage)}
              </div>
            )}

            <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
              <div className="flex items-center justify-between border-b border-line px-5 py-4">
                <div className="flex items-center gap-2">
                  <MessagesSquare className="h-4 w-4 text-rose-ink" />
                  <h3 className="text-sm font-bold text-ink">{translateUi("Tutor review requests")}</h3>
                </div>
              </div>
              {reviewRequests.length ? (
                <div className="divide-y divide-line">
                  {reviewRequests.map(request => (
                    <div key={request.id} className="flex items-center justify-between gap-3 px-5 py-3">
                      <div className="min-w-0">
                        <p className="truncate text-xs font-bold text-ink">{translateUi("Submission ")}{request.submissionId}</p>
                        <p className="text-[11px] text-muted">
                          {translateUi(new Date(request.requestedAt).toLocaleString(locale))}
                        </p>
                      </div>
                      <span className="rounded-full bg-rose px-2.5 py-1 text-[11px] font-bold text-rose-ink">
                        {translateUi(request.status)}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="p-5 text-sm text-muted">{translateUi("No tutor review requests were returned.")}</p>
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
                    <h3 className="text-sm font-bold text-ink">{translateUi("Submitted essay")}</h3>
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
                  <h3 className="text-sm font-bold text-ink">{translateUi("Grammar feedback")}</h3>
                  <span className="rounded-full bg-danger px-2 py-0.5 text-xs font-bold text-danger-ink">{feedback.grammarErrors.length}</span>
                </div>
              </div>
              <div className="divide-y divide-line">
                {feedback.grammarErrors.length ? feedback.grammarErrors.map(error => (
                  <div key={error.id} className="p-5">
                    <p className="mb-2 text-base leading-relaxed text-ink">{error.sentence}</p>
                    <div className="grid gap-3 text-xs sm:grid-cols-2">
                      <div>
                        <p className="mb-0.5 font-semibold text-muted">{translateUi("Suggestion")}</p>
                        <p className="font-bold text-success-ink">{error.suggestion}</p>
                      </div>
                      <div>
                        <p className="mb-0.5 font-semibold text-muted">{translateUi("Explanation")}</p>
                        <p className="leading-relaxed text-muted">{error.explanation}</p>
                      </div>
                    </div>
                  </div>
                )) : (
                  <p className="p-5 text-sm text-muted">{translateUi("No grammar issues were returned.")}</p>
                )}
              </div>
            </div>

            <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
              <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <div className="flex items-center justify-between border-b border-line px-5 py-4">
                  <div className="flex items-center gap-2">
                    <Sparkles className="h-4 w-4 text-success-ink" />
                    <h3 className="text-sm font-bold text-ink">{translateUi("Vocabulary suggestions")}</h3>
                    <span className="rounded-full bg-success px-2 py-0.5 text-xs font-bold text-success-ink">{feedback.vocabSuggestions.length}</span>
                  </div>
                  {feedback.vocabSuggestions.length > 0 && (
                    <div className="flex items-center gap-2">
                      {isGrouping ? (
                        <>
                          <button
                            onClick={() => { setIsGrouping(false); setSelectedVocabIds(new Set()); }}
                            className="text-xs font-semibold text-muted hover:text-ink"
                          >
                            Cancel
                          </button>
                          <button
                            onClick={() => setCreateGroupOpen(true)}
                            disabled={selectedVocabIds.size === 0}
                            className="flex items-center gap-1 rounded-full bg-accent px-3 py-1.5 text-xs font-bold text-accent-ink hover:bg-accent-hover disabled:opacity-50"
                          >
                            <FolderPlus className="h-3 w-3" />
                            Group ({selectedVocabIds.size})
                          </button>
                        </>
                      ) : (
                        <button
                          onClick={() => setIsGrouping(true)}
                          className="flex items-center gap-1 rounded-full bg-surface-muted px-3 py-1.5 text-xs font-bold text-ink hover:bg-surface-muted"
                        >
                          Select & Group
                        </button>
                      )}
                    </div>
                  )}
                </div>
                <div className="divide-y divide-line">
                  {feedback.vocabSuggestions.length ? feedback.vocabSuggestions.map(item => {
                    const existingGroups = getGroupsForFakeId(item.id);
                    return (
                    <div
                      key={item.id}
                      className={`relative p-5 transition-colors ${isGrouping ? 'cursor-pointer hover:bg-canvas' : ''} ${selectedVocabIds.has(item.id) ? 'bg-accent' : ''}`}
                      onClick={() => {
                        if (isGrouping) toggleVocabSelection(item.id);
                      }}
                    >
                      {isGrouping && (
                        <div className={`absolute right-5 top-5 flex h-5 w-5 items-center justify-center rounded-md border ${selectedVocabIds.has(item.id) ? 'border-focus bg-accent text-accent-ink' : 'border-line'}`}>
                          {selectedVocabIds.has(item.id) && <Check className="h-3.5 w-3.5" />}
                        </div>
                      )}
                      <p className="text-xs font-semibold text-muted">{item.originalWord}</p>
                      <p className="mt-1 text-sm font-extrabold text-success-ink">{item.suggestedWord}</p>
                      {item.exampleSentence && <p className="mt-2 text-xs leading-relaxed text-muted pr-8">{item.exampleSentence}</p>}
                      {existingGroups.length > 0 && (
                        <div className="mt-3 flex flex-wrap gap-1.5">
                          {existingGroups.map(g => (
                            <span key={g.id} className="inline-flex items-center gap-1 rounded bg-surface-muted px-2 py-0.5 text-[10px] font-bold text-muted group/badge">
                              <FolderPlus className="h-3 w-3" />
                              {g.name}
                              <button
                                onClick={e => {
                                  e.stopPropagation();
                                  handleRemoveGroupItem(g.id, item.id);
                                }}
                                className="ml-1 rounded hover:bg-surface-muted"
                                title="Remove from group"
                              >
                                <X className="h-3 w-3 text-muted hover:text-danger-ink transition-colors" />
                              </button>
                            </span>
                          ))}
                        </div>
                      )}
                    </div>
                    );
                  }) : (
                    <p className="p-5 text-sm text-muted">No vocabulary suggestions were returned.</p>
                  )}
                </div>
              </div>

              <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
                <div className="flex items-center justify-between border-b border-line px-5 py-4">
                  <div className="flex items-center gap-2">
                    <SplitSquareHorizontal className="h-4 w-4 text-accent-ink" />
                    <h3 className="text-sm font-bold text-ink">{translateUi("Rewrite suggestions")}</h3>
                    <span className="rounded-full bg-accent px-2 py-0.5 text-xs font-bold text-accent-ink">{feedback.restructuringSuggestions?.length ?? 0}</span>
                  </div>
                </div>
                <div className="divide-y divide-line">
                  {feedback.restructuringSuggestions?.length ? feedback.restructuringSuggestions.map(item => (
                    <div key={item.id} className="p-5">
                      <p className="text-base leading-relaxed text-muted">{item.originalSentence}</p>
                      <p className="mt-2 text-sm font-bold text-accent-ink">{item.suggestedRewrite}</p>
                      {item.reason && <p className="mt-2 text-base leading-relaxed text-muted">{translateUi(item.reason)}</p>}
                    </div>
                  )) : (
                    <p className="p-5 text-sm text-muted">{translateUi("No rewrite suggestions were returned.")}</p>
                  )}
                </div>
              </div>
            </div>

            <div className="flex items-center justify-center gap-2 py-2 text-xs text-muted">
              <Sparkles className="h-3.5 w-3.5" />
              {translateUi("Analysis by NomiWrite AI")}</div>
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
