"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { StudyGuide } from "@/lib/types";
import {
  AlertTriangle,
  ArrowRight,
  Check,
  Compass,
  Copy,
  Loader2,
  RefreshCw,
  Sparkles,
  Trophy,
  TrendingUp,
} from "lucide-react";

const focusStyles: Record<string, { label: string; className: string }> = {
  task_response: { label: "Task Response", className: "bg-blue-50 text-blue-700" },
  coherence: { label: "Coherence", className: "bg-cyan-50 text-cyan-700" },
  lexical: { label: "Lexical", className: "bg-violet-50 text-violet-700" },
  vocabulary: { label: "Vocabulary", className: "bg-violet-50 text-violet-700" },
  grammar: { label: "Grammar", className: "bg-orange-50 text-orange-700" },
  structure: { label: "Structure", className: "bg-teal-50 text-teal-700" },
};

function resolveFocusBadge(focus: string) {
  const key = focus.trim().toLowerCase().replace(/[\s_-]+/g, "_");
  return focusStyles[key] ?? { label: focus || "Practice", className: "bg-slate-100 text-slate-600" };
}

function formatBand(band: number | null | undefined) {
  if (band == null) return "—";
  return Number.isInteger(band) ? String(band) : band.toFixed(1);
}

function StudyPlanContent() {
  const [guide, setGuide] = useState<StudyGuide | null>(null);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState("");
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    let ignore = false;
    apiClient.getStudyGuide()
      .then(result => {
        if (!ignore) setGuide(result);
      })
      .catch(err => {
        if (!ignore) setError(err instanceof Error ? err.message : "Could not load your study plan.");
      })
      .finally(() => {
        if (!ignore) setLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, []);

  const generate = useCallback(async (forceRefresh: boolean) => {
    setGenerating(true);
    setError("");
    setCopied(false);
    try {
      const result = await apiClient.generateStudyGuide({ forceRefresh });
      setGuide(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not generate your study plan.");
    } finally {
      setGenerating(false);
    }
  }, []);

  const bandGap = useMemo(() => {
    if (!guide || guide.targetBand == null) return null;
    return guide.targetBand - guide.estimatedBand;
  }, [guide]);

  async function copyPrompt() {
    if (!guide?.recommendedTopic.suggestedPrompt) return;
    try {
      await navigator.clipboard.writeText(guide.recommendedTopic.suggestedPrompt);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {}
  }

  return (
    <AppShell activePath="/study-guide">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <Compass className="h-4 w-4 text-blue-600" />
          <h1 className="text-sm font-extrabold text-slate-900">Study Plan</h1>
        </div>
        {guide && (
          <button
            type="button"
            onClick={() => void generate(true)}
            disabled={generating}
            className="flex items-center gap-2 rounded-full bg-[#19325B] px-4 py-1.5 text-xs font-bold text-white transition-colors hover:bg-blue-900 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <RefreshCw className={`h-3.5 w-3.5 ${generating ? "animate-spin" : ""}`} />
            {generating ? "Regenerating…" : "Regenerate"}
          </button>
        )}
      </div>

      <div className="mx-auto w-full max-w-5xl space-y-6 p-6 lg:p-10">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-10 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading your study plan
          </div>
        )}

        {error && (
          <div className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">
            {error}
          </div>
        )}

        {!loading && !guide && !generating && (
          <div className="my-auto space-y-5 py-10">
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
              <div className="bg-[#19325B] p-10 lg:p-14 lg:pb-24">
                <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
                  <Compass className="h-6 w-6 text-blue-200" />
                </div>
                <h2 className="text-2xl font-extrabold tracking-tight text-white lg:text-3xl">
                  Your personalized roadmap to the band you want
                </h2>
                <p className="mt-3 max-w-2xl text-sm leading-relaxed text-blue-100 lg:text-base">
                  NomiWrite analyzes your graded essays, grammar errors, and vocabulary to build a
                  step-by-step study plan — your current band, exact strengths and weaknesses, and
                  three next moves to practice.
                </p>
              </div>
              <div className="-mt-10 flex justify-center lg:justify-start lg:px-14">
                <button
                  type="button"
                  onClick={() => void generate(false)}
                  className="flex items-center gap-2 rounded-2xl bg-blue-600 px-7 py-3.5 text-sm font-bold text-white shadow-lg shadow-blue-200 transition-all hover:-translate-y-0.5 hover:bg-blue-700"
                >
                  <Sparkles className="h-4 w-4" />
                  Generate my study plan
                </button>
              </div>
            </div>
            {!error && (
              <p className="text-center text-xs font-medium text-slate-400">
                Needs at least one graded writing — add one in{" "}
                <Link href="/write" className="text-blue-600 hover:underline">Write</Link> or{" "}
                <Link href="/history" className="text-blue-600 hover:underline">History</Link>.
              </p>
            )}
          </div>
        )}

        {generating && (
          <div className="rounded-2xl border border-slate-100 bg-white p-10 text-center shadow-sm">
            <Loader2 className="mx-auto mb-4 h-8 w-8 animate-spin text-blue-600" />
            <p className="text-sm font-extrabold text-slate-900">Analyzing your writing history…</p>
            <p className="mt-1 text-xs text-slate-500">Reading graded essays, grammar errors, and vocabulary. This can take up to a minute.</p>
          </div>
        )}

        {!loading && guide && !generating && (
          <>
            <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
              <div className="grid gap-0 bg-[#19325B] lg:grid-cols-[auto_1fr]">
                <div className="flex flex-col items-center justify-center gap-2 p-8 lg:min-w-64 lg:border-r lg:border-white/10">
                  <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-blue-200">
                    <Trophy className="h-3.5 w-3.5 text-yellow-400" />
                    Estimated Band
                  </div>
                  <span className="text-6xl font-extrabold tracking-tight text-white lg:text-7xl">
                    {formatBand(guide.estimatedBand)}
                  </span>
                  {guide.targetBand != null && (
                    <div className="flex items-center gap-1.5 rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-blue-100">
                      <TrendingUp className="h-3 w-3" />
                      Target {formatBand(guide.targetBand)}
                    </div>
                  )}
                  {bandGap != null && (
                    <p className={`text-xs font-semibold ${bandGap > 0 ? "text-amber-300" : "text-emerald-300"}`}>
                      {bandGap > 0 ? `${formatBand(bandGap)} to go` : "Target reached"}
                    </p>
                  )}
                </div>
                <div className="p-8 lg:p-10">
                  <div className="mb-3 flex flex-wrap gap-2">
                    <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-blue-100">
                      {guide.targetExam || "IELTS Writing"}
                    </span>
                    <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-blue-100">
                      {guide.analyzedEssayCount} essay{guide.analyzedEssayCount === 1 ? "" : "s"} analyzed
                    </span>
                  </div>
                  <p className="text-base leading-relaxed text-blue-50 lg:text-lg">{guide.summary}</p>
                  <p className="mt-4 text-xs font-semibold text-blue-300">
                    Generated {new Date(guide.createdAt).toLocaleString()}
                  </p>
                </div>
              </div>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-emerald-100">
                    <Check className="h-4 w-4 text-emerald-700" />
                  </span>
                  <h3 className="text-base font-extrabold text-slate-900">What you do well</h3>
                </div>
                <ul className="space-y-3">
                  {guide.strengths.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-xl bg-emerald-50/60 p-3.5">
                      <Check className="mt-0.5 h-4 w-4 shrink-0 text-emerald-600" />
                      <span className="text-sm leading-relaxed text-slate-700">{item}</span>
                    </li>
                  ))}
                  {guide.strengths.length === 0 && (
                    <li className="text-sm text-slate-400">No strengths recorded yet.</li>
                  )}
                </ul>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-red-100">
                    <AlertTriangle className="h-4 w-4 text-red-700" />
                  </span>
                  <h3 className="text-base font-extrabold text-slate-900">What to fix</h3>
                </div>
                <ul className="space-y-3">
                  {guide.weaknesses.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-xl bg-red-50/60 p-3.5">
                      <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-red-600" />
                      <span className="text-sm leading-relaxed text-slate-700">{item}</span>
                    </li>
                  ))}
                  {guide.weaknesses.length === 0 && (
                    <li className="text-sm text-slate-400">No weaknesses flagged.</li>
                  )}
                </ul>
              </div>
            </div>

            <div className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm lg:p-8">
              <div className="mb-6 flex items-center justify-between">
                <h3 className="text-base font-extrabold text-slate-900">Your next three moves</h3>
                <span className="text-xs font-semibold text-slate-400">Do these in order</span>
              </div>
              <div className="space-y-4">
                {guide.nextSteps.map((step, index) => {
                  const badge = resolveFocusBadge(step.focus);
                  return (
                    <div key={index} className="flex items-start gap-4 rounded-xl border border-slate-100 bg-slate-50/60 p-4">
                      <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-[#19325B] text-sm font-extrabold text-white">
                        {index + 1}
                      </div>
                      <div className="min-w-0 flex-1">
                        <div className="mb-1 flex flex-wrap items-center gap-2">
                          <p className="text-sm font-extrabold text-slate-900">{step.title}</p>
                          <span className={`rounded-full px-2.5 py-0.5 text-[11px] font-bold ${badge.className}`}>
                            {badge.label}
                          </span>
                        </div>
                        <p className="text-sm leading-relaxed text-slate-600">{step.description}</p>
                      </div>
                    </div>
                  );
                })}
                {guide.nextSteps.length === 0 && (
                  <p className="text-sm text-slate-400">No steps planned yet.</p>
                )}
              </div>
            </div>

            <div className="overflow-hidden rounded-2xl border border-blue-200 bg-gradient-to-br from-blue-50/60 to-violet-50/40 shadow-sm">
              <div className="p-6 lg:p-8">
                <div className="mb-3 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100">
                    <Sparkles className="h-4 w-4 text-blue-700" />
                  </span>
                  <h3 className="text-base font-extrabold text-slate-900">Practice topic recommended for you</h3>
                </div>
                <p className="text-sm font-bold text-slate-900">{guide.recommendedTopic.title}</p>
                <p className="mt-1 text-sm leading-relaxed text-slate-600">{guide.recommendedTopic.reason}</p>
                {guide.recommendedTopic.suggestedPrompt && (
                  <div className="mt-4 flex items-start gap-3 rounded-xl border border-slate-200 bg-white p-4">
                    <p className="flex-1 text-sm italic leading-relaxed text-slate-700">
                      &ldquo;{guide.recommendedTopic.suggestedPrompt}&rdquo;
                    </p>
                    <button
                      type="button"
                      onClick={() => void copyPrompt()}
                      className="flex shrink-0 items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-bold text-slate-600 transition-colors hover:border-blue-200 hover:bg-blue-50 hover:text-blue-700"
                    >
                      {copied ? <Check className="h-3.5 w-3.5 text-emerald-600" /> : <Copy className="h-3.5 w-3.5" />}
                      {copied ? "Copied" : "Copy prompt"}
                    </button>
                  </div>
                )}
              </div>
              <div className="border-t border-blue-100/60 bg-white/60 px-6 py-4 lg:px-8">
                <Link
                  href="/write"
                  className="inline-flex items-center gap-2 text-sm font-bold text-blue-700 transition-colors hover:text-blue-800"
                >
                  Start writing this topic
                  <ArrowRight className="h-4 w-4" />
                </Link>
              </div>
            </div>
          </>
        )}
      </div>
    </AppShell>
  );
}

export default function StudyGuidePage() {
  return <StudyPlanContent />;
}