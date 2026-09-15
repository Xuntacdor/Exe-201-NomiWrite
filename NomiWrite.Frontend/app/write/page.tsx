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
      {/* Sticky Header */}
      <div className="sticky top-0 z-50 flex h-[72px] items-center justify-between border-b border-slate-200/50 bg-white/80 px-8 backdrop-blur-xl">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
            <PenLine className="h-5 w-5" strokeWidth={2.5} />
          </div>
          <div>
            <h1 className="text-lg font-extrabold text-slate-900">IELTS Writing Practice</h1>
            <p className="text-xs font-semibold text-slate-500">Computer-delivered format</p>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <button
            type="button"
            onClick={() => setTimerOn(value => !value)}
            className={`flex items-center gap-2 rounded-xl border px-4 py-2 text-sm font-bold transition-all ${
              timerOn ? "border-orange-200 bg-orange-50 text-orange-600" : "border-slate-200 bg-white text-slate-500 hover:border-slate-300 hover:bg-slate-50"
            }`}
          >
            <Clock className="h-4 w-4" />
            {timerOn ? "Timed Mode" : "Untimed"}
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={!canSubmit}
            className="flex items-center gap-2 rounded-xl bg-blue-600 px-6 py-2 text-sm font-bold text-white shadow-md shadow-blue-200 transition-all hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {submitting ? <Loader2 className="h-4 w-4 animate-spin" /> : <Send className="h-4 w-4" />}
            Submit for Grading
          </button>
        </div>
      </div>

      {/* Split Screen Container */}
      <div className="flex h-[calc(100vh-72px)] flex-col lg:flex-row">
        
        {/* Left Column: Prompt & Controls */}
        <div className="flex w-full flex-col border-r border-slate-200 bg-slate-50 lg:w-1/2">
          {/* Controls Bar */}
          <div className="border-b border-slate-200 bg-white p-6 shadow-sm z-10">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="mb-1.5 block text-xs font-bold uppercase tracking-wide text-slate-500">Writing Task</label>
                <div className="relative">
                  <select
                    value={selectedTypeId}
                    onChange={event => {
                      setUserSelectedType(true);
                      setSelectedTypeId(event.target.value);
                    }}
                    disabled={loadingTypes}
                    className="w-full cursor-pointer appearance-none rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 pr-10 text-[15px] font-semibold text-slate-800 outline-none transition-all focus:border-blue-500 focus:bg-white focus:ring-4 focus:ring-blue-500/10 disabled:opacity-50"
                  >
                    {types.map(type => (
                      <option key={type.id} value={type.id}>{type.label}</option>
                    ))}
                  </select>
                  <ChevronDown className="pointer-events-none absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                </div>
              </div>
              <div>
                <label className="mb-1.5 block text-xs font-bold uppercase tracking-wide text-slate-500">Select Prompt</label>
                <div className="relative">
                  {loadingPrompts ? (
                    <div className="flex w-full items-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-[15px] font-semibold text-slate-400">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Loading...
                    </div>
                  ) : prompts.length ? (
                    <select
                      value={selectedPromptId}
                      onChange={event => setSelectedPromptId(event.target.value)}
                      className="w-full cursor-pointer appearance-none rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 pr-10 text-[15px] font-semibold text-slate-800 outline-none transition-all focus:border-blue-500 focus:bg-white focus:ring-4 focus:ring-blue-500/10"
                    >
                      {prompts.map(prompt => (
                        <option key={prompt.id} value={prompt.id}>{prompt.topic}</option>
                      ))}
                    </select>
                  ) : (
                    <div className="flex w-full items-center rounded-xl border border-amber-200 bg-amber-50 px-4 py-3 text-[15px] font-semibold text-amber-600">
                      No prompts available
                    </div>
                  )}
                  <ChevronDown className="pointer-events-none absolute right-4 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                </div>
              </div>
            </div>
            
            {error && (
              <div className="mt-4 flex items-center gap-2 rounded-lg bg-red-50 p-3 text-sm font-semibold text-red-600">
                <AlertCircle className="h-4 w-4" />
                {error}
              </div>
            )}
          </div>

          {/* Prompt Area */}
          <div className="flex-1 overflow-y-auto p-8">
            {currentPrompt ? (
              <div className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
                <div className="mb-6 flex items-center gap-3">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-100 text-slate-600">
                    <AlignLeft className="h-4 w-4" />
                  </div>
                  <h2 className="text-lg font-extrabold text-slate-900">Topic: {currentPrompt.topic}</h2>
                </div>
                <div className="prose prose-slate max-w-none text-[15px] leading-relaxed text-slate-700">
                  {currentPrompt.prompt.split('\n').map((paragraph, idx) => (
                    <p key={idx} className="mb-4">{paragraph}</p>
                  ))}
                </div>
              </div>
            ) : (
              <div className="flex h-full flex-col items-center justify-center text-slate-400">
                <BookOpen className="mb-4 h-12 w-12 opacity-20" />
                <p className="font-semibold text-slate-500">Select a prompt to start practicing</p>
              </div>
            )}

            {sampleAnswer && (
              <div className="mt-6 overflow-hidden rounded-2xl border border-blue-100 bg-blue-50/50">
                <details className="group">
                  <summary className="flex cursor-pointer items-center justify-between p-6 font-extrabold text-blue-900 outline-none transition-colors hover:bg-blue-50">
                    <div className="flex items-center gap-2">
                      <Sparkles className="h-5 w-5 text-blue-600" />
                      Show Sample Answer
                    </div>
                    <ChevronDown className="h-5 w-5 text-blue-400 transition-transform group-open:rotate-180" />
                  </summary>
                  <div className="border-t border-blue-100 bg-white p-6">
                    <div className="prose prose-slate max-w-none text-[15px] leading-relaxed text-slate-700">
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
        <div className="flex w-full flex-col bg-white lg:w-1/2">
          <div className="flex-1 p-8">
            <textarea
              value={content}
              onChange={event => setContent(event.target.value)}
              placeholder="Type your essay here... 

Remember to:
- Read the prompt carefully
- Plan your paragraphs
- Check for grammar and vocabulary
- Reach the minimum word count"
              className="h-full w-full resize-none text-[16px] leading-loose text-slate-800 placeholder:text-slate-300 outline-none focus:ring-0"
              spellCheck="false"
            />
          </div>
          
          {error && (
            <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">
              {error}
            </p>
          )}
          
          {/* Bottom Bar: Word Count & Status */}
          <div className="flex items-center justify-between border-t border-slate-200 bg-slate-50 px-8 py-4 z-10">
            <div className="flex items-center gap-4">
              <div>
                <span className={`text-2xl font-extrabold ${wordCount >= minimumWords ? "text-emerald-600" : "text-slate-700"}`}>
                  {wordCount}
                </span>
                <span className="ml-1.5 text-xs font-bold uppercase tracking-wide text-slate-500">Words</span>
              </div>
              <div className="h-8 w-px bg-slate-200"></div>
              <div>
                <p className={`text-[13px] font-semibold ${shortContent ? "text-amber-600" : "text-slate-500"}`}>
                  {wordCount === 0 
                    ? "Start writing to track progress" 
                    : shortContent 
                      ? `${minimumWords - wordCount} more words to reach target (${minimumWords})` 
                      : `Target of ${minimumWords} words reached!`}
                </p>
                <div className="mt-1.5 h-1.5 w-48 overflow-hidden rounded-full bg-slate-200">
                  <div 
                    className={`h-full rounded-full transition-all duration-300 ${wordCount >= minimumWords ? "bg-emerald-500" : "bg-blue-500"}`} 
                    style={{ width: `${progress}%` }} 
                  />
                </div>
              </div>
            </div>
            {apiMode === "mock" && (
              <div className="flex items-center gap-1.5 rounded-lg bg-amber-100 px-2.5 py-1 text-[11px] font-bold text-amber-700">
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
