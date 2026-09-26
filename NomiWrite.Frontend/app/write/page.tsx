"use client";

import { Suspense, useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  AlignLeft,
  BookOpen,
  ChevronDown,
  Clock,
  Lightbulb,
  Loader2,
  PenLine,
  Send,
  Sparkles,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { WritingPrompt, WritingType } from "@/lib/types";

const fallbackType: WritingType = {
  id: "fallback",
  label: "Writing practice",
  badge: "Practice",
  minWords: 120,
};

function countWords(content: string) {
  return content.trim() ? content.trim().split(/\s+/).length : 0;
}

function parseJsonArray(raw: string | null): string[] {
  if (!raw) return [];
  try {
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed.filter((item): item is string => typeof item === "string") : [];
  } catch {
    return [];
  }
}

function buildScaffold(input: { prompt: string; hints: string[]; vocab: string }) {
  const parts = [input.prompt];
  if (input.hints.length > 0) {
    parts.push("", "Ideas to develop:", ...input.hints.map(hint => `• ${hint}`));
  }
  if (input.vocab.trim()) {
    parts.push("", `Topic vocabulary: ${input.vocab.trim()}`);
  }
  parts.push("", "Your essay:");
  return parts.join("\n");
}

function WriteContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const initialType = searchParams.get("type");
  const customTitle = searchParams.get("title");
  const customPrompt = searchParams.get("prompt");
  const customFocus = searchParams.get("focus");
  const customVocab = searchParams.get("vocab");
  const customHints = useMemo(() => parseJsonArray(searchParams.get("hints")), [searchParams]);
  const hasCustomTopic = Boolean(customPrompt);

  const [types, setTypes] = useState<WritingType[]>([]);
  const [prompts, setPrompts] = useState<WritingPrompt[]>([]);
  const [selectedTypeId, setSelectedTypeId] = useState(initialType ?? "");
  const [selectedPromptId, setSelectedPromptId] = useState("");
  const [content, setContent] = useState("");
  const [sampleAnswer, setSampleAnswer] = useState<string | null>(null);
  const [timerOn, setTimerOn] = useState(false);
  const [loadingTypes, setLoadingTypes] = useState(true);
  const [loadingPrompts, setLoadingPrompts] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [userSelectedType, setUserSelectedType] = useState(Boolean(initialType));

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoadingTypes(true);
      apiClient.listWritingTypes()
        .then(items => {
          if (ignore) return;
          setTypes(items);
          const nextType = initialType && items.some(item => item.id === initialType)
            ? initialType
            : items[0]?.id ?? "";
          setSelectedTypeId(nextType);
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load writing types.");
        })
        .finally(() => {
          if (!ignore) setLoadingTypes(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [initialType, router]);

  useEffect(() => {
    if (!selectedTypeId) return;

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoadingPrompts(true);
      setError("");
      apiClient.listWritingPrompts(selectedTypeId)
        .then(items => {
          if (ignore) return;
          setPrompts(items);
          setSelectedPromptId(items[0]?.id ?? "");
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load writing prompts.");
        })
        .finally(() => {
          if (!ignore) setLoadingPrompts(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [selectedTypeId]);

  useEffect(() => {
    if (userSelectedType || !selectedTypeId || loadingPrompts || prompts.length > 0 || loadingTypes) return;
    if (initialType && selectedTypeId === initialType) return;

    let ignore = false;
    const loadTimer = setTimeout(() => {
      apiClient.listWritingPrompts()
        .then(items => {
          if (ignore) return;
          const firstAvailable = types.find(type => items.some(prompt => prompt.writingTypeId === type.id));
          if (firstAvailable && firstAvailable.id !== selectedTypeId) {
            setSelectedTypeId(firstAvailable.id);
          }
        })
        .catch(() => {});
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [initialType, loadingPrompts, loadingTypes, prompts.length, selectedTypeId, types, userSelectedType]);

  useEffect(() => {
    if (!selectedPromptId) {
      const clearTimer = setTimeout(() => setSampleAnswer(null), 0);
      return () => clearTimeout(clearTimer);
    }

    let ignore = false;
    apiClient.getPromptSampleAnswer(selectedPromptId)
      .then(answer => {
        if (!ignore) setSampleAnswer(answer);
      })
      .catch(() => {
        if (!ignore) setSampleAnswer(null);
      });

    return () => {
      ignore = true;
    };
  }, [selectedPromptId]);

  const currentType = types.find(type => type.id === selectedTypeId) ?? fallbackType;
  const currentPrompt = prompts.find(prompt => prompt.id === selectedPromptId);
  const minimumWords = currentPrompt?.minWords ?? currentType.minWords;
  const wordCount = countWords(content);
  const progress = Math.min((wordCount / minimumWords) * 100, 100);
  const shortContent = wordCount > 0 && wordCount < minimumWords;
  const canSubmit = Boolean(currentPrompt && content.trim()) && !submitting;

  const draftKey = useMemo(
    () => hasCustomTopic
      ? `nomiwrite_draft_study_${customFocus || "custom"}`
      : `nomiwrite_draft_${selectedPromptId || selectedTypeId || "default"}`,
    [hasCustomTopic, customFocus, selectedPromptId, selectedTypeId],
  );

  useEffect(() => {
    const loadTimer = setTimeout(() => {
      try {
        const saved = localStorage.getItem(draftKey);
        if (saved) {
          setContent(saved);
        } else if (hasCustomTopic) {
          setContent(buildScaffold({ prompt: customPrompt ?? "", hints: customHints, vocab: customVocab ?? "" }));
        } else {
          setContent("");
        }
      } catch {
        setContent("");
      }
    }, 0);

    return () => clearTimeout(loadTimer);
  }, [draftKey, hasCustomTopic, customPrompt, customHints, customVocab]);

  useEffect(() => {
    try {
      localStorage.setItem(draftKey, content);
    } catch {}
  }, [content, draftKey]);

  async function handleSubmit() {
    if (!currentPrompt || !content.trim()) return;

    if (apiMode === "real" && !getSession()?.accessToken) {
      router.push("/login");
      return;
    }

    setSubmitting(true);
    setError("");
    try {
      const submission = await apiClient.submitSubmission({
        writingPromptId: currentPrompt.id,
        isTimed: timerOn,
        content,
      });
      localStorage.removeItem(draftKey);
      router.push(`/result?submissionId=${submission.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not submit your writing.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <AppShell activePath="/write">
      {/* Sticky Header */}
      <div className="write-heading sticky top-0 z-20 flex min-h-[72px] items-center justify-between border-b border-line bg-surface px-8">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent text-accent-ink">
            <PenLine className="h-5 w-5" strokeWidth={2.5} />
          </div>
          <div>
            <h1 className="text-lg font-bold text-ink">IELTS Writing Practice</h1>
            <p className="text-xs font-semibold text-muted">Computer-delivered format</p>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <button
            type="button"
            onClick={() => setTimerOn(value => !value)}
            className={`flex items-center gap-2 rounded-xl border px-4 py-2 text-sm font-bold transition-all ${
              timerOn ? "border-line bg-accent text-accent-ink" : "border-line bg-surface text-muted hover:border-line hover:bg-canvas"
            }`}
          >
            <Clock className="h-4 w-4" />
            {timerOn ? "Timed Mode" : "Untimed"}
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!canSubmit}
            className="flex items-center gap-2 rounded-xl bg-accent px-6 py-2 text-sm font-bold text-ink shadow-sm transition-all hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
          >
            {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
            Submit for Grading
          </button>
        </div>
      </div>

      {/* Split Screen Container */}
      <div className="write-layout">

        {/* Left Column: Prompt & Controls */}
        <div className="write-prompt flex flex-col">
          {/* Controls Bar */}
          <div className="border-b border-line bg-surface p-6 shadow-sm z-10">
            <div className="write-controls grid grid-cols-2 gap-4">
              <div>
                <label className="mb-1.5 block text-xs font-bold uppercase tracking-wide text-muted">Writing Task</label>
                <div className="relative">
                  <select
                    value={selectedTypeId}
                    onChange={event => {
                      setUserSelectedType(true);
                      setSelectedTypeId(event.target.value);
                    }}
                    disabled={loadingTypes}
                    className="w-full cursor-pointer appearance-none rounded-xl border border-line bg-canvas px-4 py-3 pr-10 text-[15px] font-semibold text-ink outline-none transition-all focus:border-focus focus:bg-surface focus:ring-4 focus:ring-focus disabled:opacity-50"
                  >
                    {types.map(type => (
                      <option key={type.id} value={type.id}>{type.label}</option>
                    ))}
                  </select>
                  <ChevronDown className="pointer-events-none absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
                </div>
              </div>
              <div>
                <label className="mb-1.5 block text-xs font-bold uppercase tracking-wide text-muted">Select Prompt</label>
                <div className="relative">
                  {loadingPrompts ? (
                    <div className="flex w-full items-center gap-2 rounded-xl border border-line bg-canvas px-4 py-3 text-[15px] font-semibold text-muted">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Loading...
                    </div>
                  ) : prompts.length ? (
                    <select
                      value={selectedPromptId}
                      onChange={event => setSelectedPromptId(event.target.value)}
                      className="w-full cursor-pointer appearance-none rounded-xl border border-line bg-canvas px-4 py-3 pr-10 text-[15px] font-semibold text-ink outline-none transition-all focus:border-focus focus:bg-surface focus:ring-4 focus:ring-focus"
                    >
                      {prompts.map(prompt => (
                        <option key={prompt.id} value={prompt.id}>{prompt.topic}</option>
                      ))}
                    </select>
                  ) : (
                    <div className="flex w-full items-center rounded-xl border border-line bg-warning px-4 py-3 text-[15px] font-semibold text-warning-ink">
                      No prompts available
                    </div>
                  )}
                  <ChevronDown className="pointer-events-none absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
                </div>
              </div>
            </div>

            {error && (
              <div className="mt-4 flex items-center gap-2 rounded-lg bg-danger p-3 text-sm font-semibold text-danger-ink">
                <AlertCircle className="h-4 w-4" />
                {error}
              </div>
            )}
          </div>

          {/* Prompt Area */}
          <div className="flex-1 overflow-y-auto p-8">
            {hasCustomTopic && (
              <div className="mb-6 overflow-hidden rounded-xl border border-line bg-accent shadow-sm">
                <div className="flex items-center gap-2 border-b border-line px-6 py-3.5">
                  <Lightbulb className="h-4 w-4 text-accent-ink" />
                  <p className="text-sm font-bold text-ink">{customTitle || "Topic from your Study Plan"}</p>
                </div>
                <div className="p-6">
                  <p className="text-[15px] font-medium leading-relaxed text-ink">{customPrompt}</p>
                  {customHints.length > 0 && (
                    <div className="mt-4">
                      <p className="text-xs font-bold uppercase tracking-wide text-accent-ink">Ideas to develop</p>
                      <ul className="mt-2 space-y-1.5">
                        {customHints.map((hint, index) => (
                          <li key={index} className="flex items-start gap-2 text-sm leading-relaxed text-muted">
                            <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-accent" />
                            {hint}
                          </li>
                        ))}
                      </ul>
                    </div>
                  )}
                  {customVocab && (
                    <div className="mt-4 flex flex-wrap gap-2">
                      {customVocab.split(",").map((term, index) => (
                        <span key={index} className="rounded-full bg-success px-3 py-1 text-xs font-bold text-success-ink">
                          {term.trim()}
                        </span>
                      ))}
                    </div>
                  )}
                  <p className="mt-4 text-xs font-semibold text-muted">
                    Choose a writing task above to submit for grading — or keep this topic as your reference while you type.
                  </p>
                </div>
              </div>
            )}
            {currentPrompt ? (
              <div className="rounded-xl border border-line bg-surface p-8 shadow-sm">
                <div className="mb-6 flex items-center gap-3">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-surface-muted text-muted">
                    <AlignLeft className="h-4 w-4" />
                  </div>
                  <h2 className="text-lg font-bold text-ink">Topic: {currentPrompt.topic}</h2>
                </div>
                <div className="prose prose-slate max-w-none text-[15px] leading-relaxed text-ink">
                  {currentPrompt.prompt.split('\n').map((paragraph, idx) => (
                    <p key={idx} className="mb-4">{paragraph}</p>
                  ))}
                </div>
              </div>
            ) : (
              <div className="flex h-full flex-col items-center justify-center text-muted">
                <BookOpen className="mb-4 h-12 w-12 opacity-20" />
                <p className="font-semibold text-muted">Select a prompt to start practicing</p>
              </div>
            )}

            {sampleAnswer && (
              <div className="mt-6 overflow-hidden rounded-xl border border-line bg-accent">
                <details className="group">
                  <summary className="flex cursor-pointer items-center justify-between p-6 font-bold text-accent-ink outline-none transition-colors hover:bg-accent-hover">
                    <div className="flex items-center gap-2">
                      <Sparkles className="h-5 w-5 text-accent-ink" />
                      Show Sample Answer
                    </div>
                    <ChevronDown className="h-5 w-5 text-accent-ink transition-transform group-open:rotate-180" />
                  </summary>
                  <div className="border-t border-line bg-surface p-6">
                    <div className="prose prose-slate max-w-none text-[15px] leading-relaxed text-ink">
                      {sampleAnswer.split('\n').map((paragraph, idx) => (
                        <p key={idx} className="mb-4">{paragraph}</p>
                      ))}
                    </div>
                  </div>
                </details>
              </div>
            )}
          </div>
        </div>

        {/* Right Column: Writing Area */}
        <div className="write-editor">
          <div className="write-editor-body">
            <textarea
              aria-label="B?i vi?t c?a b?n"
              value={content}
              onChange={event => setContent(event.target.value)}
              placeholder="Type your essay here...

Remember to:
- Read the prompt carefully
- Plan your paragraphs
- Check for grammar and vocabulary
- Reach the minimum word count"
              className="h-full w-full resize-none text-[16px] leading-loose text-ink placeholder:text-muted outline-none focus:ring-0"
              spellCheck="false"
            />
          </div>

          {error && (
            <p className="rounded-xl border border-line bg-danger px-4 py-3 text-sm font-semibold text-danger-ink">
              {error}
            </p>
          )}

          {/* Bottom Bar: Word Count & Status */}
          <div className="write-toolbar">
            <div className="flex items-center gap-4">
              <div>
                <span className={`text-2xl font-bold ${wordCount >= minimumWords ? "text-success-ink" : "text-ink"}`}>
                  {wordCount}
                </span>
                <span className="ml-1.5 text-xs font-bold uppercase tracking-wide text-muted">Words</span>
              </div>
              <div className="h-8 w-px bg-surface-muted"></div>
              <div>
                <p className={`text-[13px] font-semibold ${shortContent ? "text-warning-ink" : "text-muted"}`}>
                  {wordCount === 0
                    ? "Start writing to track progress"
                    : shortContent
                      ? `${minimumWords - wordCount} more words to reach target (${minimumWords})`
                      : `Target of ${minimumWords} words reached!`}
                </p>
                <div className="mt-1.5 h-1.5 w-48 overflow-hidden rounded-full bg-surface-muted">
                  <div
                    className={`h-full rounded-full transition-all duration-300 ${wordCount >= minimumWords ? "bg-success" : "bg-accent"}`}
                    style={{ width: `${progress}%` }}
                  />
                </div>
              </div>
            </div>
            {apiMode === "mock" && (
              <div className="flex items-center gap-1.5 rounded-lg bg-warning px-2.5 py-1 text-[11px] font-bold text-warning-ink">
                <AlertCircle className="h-3 w-3" />
                MOCK MODE
              </div>
            )}
          </div>
        </div>
      </div>
    </AppShell>
  );
}

export default function WritePage() {
  return (
    <Suspense>
      <WriteContent />
    </Suspense>
  );
}
