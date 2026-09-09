"use client";

import Link from "next/link";
import { Suspense, useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import AppShell from "../components/AppShell";
import GuideModal from "../components/GuideModal";
import {
  AlertCircle,
  AlignLeft,
  BookOpen,
  ChevronDown,
  Clock,
  ExternalLink,
  Loader2,
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

function WriteContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const initialType = searchParams.get("type");

  const [types, setTypes] = useState<WritingType[]>([]);
  const [prompts, setPrompts] = useState<WritingPrompt[]>([]);
  const [selectedTypeId, setSelectedTypeId] = useState(initialType ?? "");
  const [selectedPromptId, setSelectedPromptId] = useState("");
  const [content, setContent] = useState("");
  const [sampleAnswer, setSampleAnswer] = useState<string | null>(null);
  const [timerOn, setTimerOn] = useState(false);
  const [guideOpen, setGuideOpen] = useState(false);
  const [loadingTypes, setLoadingTypes] = useState(true);
  const [loadingPrompts, setLoadingPrompts] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

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
  const wordCount = countWords(content);
  const progress = Math.min((wordCount / currentType.minWords) * 100, 100);
  const shortContent = wordCount > 0 && wordCount < currentType.minWords;
  const canSubmit = Boolean(currentPrompt && content.trim()) && !submitting;

  const draftKey = useMemo(
    () => `nomiwrite_draft_${selectedPromptId || selectedTypeId || "default"}`,
    [selectedPromptId, selectedTypeId],
  );

  useEffect(() => {
    const loadTimer = setTimeout(() => {
      try {
        setContent(localStorage.getItem(draftKey) ?? "");
      } catch {
        setContent("");
      }
    }, 0);

    return () => clearTimeout(loadTimer);
  }, [draftKey]);

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
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <h1 className="text-sm font-extrabold text-slate-900">New writing</h1>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => setTimerOn(value => !value)}
            className={`flex items-center gap-1.5 rounded-full border px-3 py-1.5 text-xs font-semibold transition-all ${
              timerOn ? "border-orange-300 bg-orange-50 text-orange-600" : "border-slate-200 bg-slate-50 text-slate-500 hover:border-slate-300"
            }`}
          >
            <Clock className="h-3.5 w-3.5" />
            {timerOn ? "Timed" : "Untimed"}
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!canSubmit}
            className="flex items-center gap-2 rounded-full bg-blue-600 px-4 py-2 text-xs font-bold text-white shadow-sm shadow-blue-200 transition-all hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40"
          >
            {submitting ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Send className="h-3.5 w-3.5" />}
            Submit
          </button>
        </div>
      </div>

      <div className="grid w-full grid-cols-1 gap-4 p-5 lg:grid-cols-12">
        <div className="space-y-3 lg:col-span-3">
          <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
            <h3 className="mb-2.5 text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Writing type</h3>
            <div className="relative">
              <select
                value={selectedTypeId}
                onChange={event => setSelectedTypeId(event.target.value)}
                disabled={loadingTypes}
                className="w-full cursor-pointer appearance-none rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 pr-8 text-sm font-semibold text-slate-800 focus:border-blue-400 focus:outline-none"
              >
                {types.map(type => (
                  <option key={type.id} value={type.id}>{type.label}</option>
                ))}
              </select>
              <ChevronDown className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            </div>
            <div className="mt-2 flex items-center justify-between gap-2">
              <span className="rounded-md bg-blue-50 px-2 py-0.5 text-[10px] font-bold text-blue-600">{currentType.badge}</span>
              <Link href="/guide" className="text-[10px] font-semibold text-blue-500 hover:text-blue-600">Guide</Link>
            </div>
          </div>

          <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
            <h3 className="mb-2.5 text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Prompt</h3>
            {loadingPrompts ? (
              <div className="flex items-center gap-2 rounded-xl bg-slate-50 px-3 py-3 text-xs font-semibold text-slate-500">
                <Loader2 className="h-3.5 w-3.5 animate-spin" />
                Loading prompts
              </div>
            ) : prompts.length ? (
              <div className="space-y-1">
                {prompts.map(prompt => (
                  <button
                    key={prompt.id}
                    type="button"
                    onClick={() => setSelectedPromptId(prompt.id)}
                    className={`w-full rounded-lg px-2.5 py-1.5 text-left text-[11px] font-medium transition-all ${
                      selectedPromptId === prompt.id ? "bg-blue-600 text-white" : "text-slate-600 hover:bg-slate-50"
                    }`}
                  >
                    {prompt.topic}
                  </button>
                ))}
              </div>
            ) : (
              <p className="rounded-xl border border-amber-100 bg-amber-50 px-3 py-3 text-xs font-medium text-amber-800">
                No prompt is available for this type yet.
              </p>
            )}
          </div>
        </div>

        <div className="space-y-3 lg:col-span-6">
          {currentPrompt && (
            <div className="rounded-2xl border border-amber-100 bg-amber-50 p-4">
              <div className="mb-2 flex items-center gap-1.5">
                <AlignLeft className="h-3.5 w-3.5 text-amber-600" />
                <span className="text-[10px] font-extrabold uppercase tracking-widest text-amber-700">Prompt</span>
              </div>
              <p className="text-[12px] leading-relaxed text-amber-900">{currentPrompt.prompt}</p>
            </div>
          )}

          {sampleAnswer && (
            <details className="rounded-2xl border border-emerald-100 bg-emerald-50 p-4">
              <summary className="cursor-pointer text-[10px] font-extrabold uppercase tracking-widest text-emerald-700">
                Sample answer
              </summary>
              <p className="mt-3 text-[12px] leading-relaxed text-emerald-900">{sampleAnswer}</p>
            </details>
          )}

          <div className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
            <div className="flex items-center justify-between border-b border-slate-50 px-4 py-3">
              <span className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">Your essay</span>
              <div className="flex items-center gap-2">
                <div className="h-1.5 w-20 overflow-hidden rounded-full bg-slate-100">
                  <div className={wordCount >= currentType.minWords ? "h-full rounded-full bg-emerald-500" : "h-full rounded-full bg-blue-500"} style={{ width: `${progress}%` }} />
                </div>
                <span className={`text-xs font-bold ${wordCount >= currentType.minWords ? "text-emerald-600" : "text-slate-500"}`}>
                  {wordCount}/{currentType.minWords}
                </span>
              </div>
            </div>
            <textarea
              value={content}
              onChange={event => setContent(event.target.value)}
              placeholder="Start writing here..."
              className="min-h-96 w-full resize-none p-4 text-sm leading-relaxed text-slate-700 placeholder:text-slate-300 focus:outline-none"
            />
          </div>

          {error && (
            <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">
              {error}
            </p>
          )}

          <div className="flex items-center justify-between rounded-2xl border border-slate-100 bg-white px-4 py-3 shadow-sm">
            <p className={`text-xs ${shortContent ? "text-amber-600" : "text-slate-500"}`}>
              {wordCount === 0 ? "No content yet" : shortContent ? `${currentType.minWords - wordCount} more words recommended` : "Ready to submit"}
            </p>
            <button
              type="button"
              onClick={handleSubmit}
              disabled={!canSubmit}
              className="flex items-center gap-2 rounded-full bg-blue-600 px-4 py-2.5 text-xs font-bold text-white shadow-md shadow-blue-200 transition-all hover:-translate-y-0.5 hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-40 disabled:translate-y-0"
            >
              {submitting ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Send className="h-3.5 w-3.5" />}
              Submit for grading
            </button>
          </div>
        </div>

        <div className="space-y-3 lg:col-span-3">
          <div className="rounded-2xl bg-gradient-to-br from-blue-600 to-violet-600 p-4 text-white">
            <div className="mb-1 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Sparkles className="h-4 w-4 text-blue-200" />
                <span className="text-xs font-extrabold uppercase tracking-widest text-blue-100">Coach</span>
              </div>
              <button
                type="button"
                onClick={() => setGuideOpen(true)}
                className="flex items-center gap-1 rounded-lg bg-white/10 px-2 py-1 text-[10px] font-bold text-blue-100 transition-all hover:bg-white/20 hover:text-white"
              >
                <ExternalLink className="h-3 w-3" />
                Guide
              </button>
            </div>
            <p className="text-sm font-bold">{currentType.label}</p>
            <p className="mt-0.5 text-[11px] text-blue-200">Recommended minimum: {currentType.minWords} words</p>
          </div>

          <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
            <div className="mb-3 flex items-center gap-2">
              <BookOpen className="h-4 w-4 text-blue-500" />
              <p className="text-sm font-extrabold text-slate-900">Submission flow</p>
            </div>
            <div className="space-y-3 text-xs text-slate-500">
              <p className="rounded-xl bg-slate-50 p-3">1. Create a draft from the selected backend prompt.</p>
              <p className="rounded-xl bg-slate-50 p-3">2. Save essay content to the writing service.</p>
              <p className="rounded-xl bg-slate-50 p-3">3. Submit the draft so AI grading can process it.</p>
              <p className="rounded-xl bg-slate-50 p-3">4. Timed drafts can read remaining time from the backend endpoint.</p>
            </div>
          </div>

          {apiMode === "mock" && (
            <div className="rounded-2xl border border-amber-100 bg-amber-50 p-4">
              <div className="mb-1 flex items-center gap-2">
                <AlertCircle className="h-4 w-4 text-amber-600" />
                <p className="text-xs font-extrabold text-amber-900">Mock mode</p>
              </div>
              <p className="text-xs leading-relaxed text-amber-800">
                Set NEXT_PUBLIC_API_MODE=real to use the backend writing API.
              </p>
            </div>
          )}
        </div>
      </div>

      <GuideModal
        typeId={guideOpen ? selectedTypeId : null}
        onClose={() => setGuideOpen(false)}
        hideCTA
      />
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
