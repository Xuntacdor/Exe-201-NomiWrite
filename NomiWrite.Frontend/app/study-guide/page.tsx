"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { StudyGuide, StudyGuideInsight, StudyGuideStep, StudyGuideTopic } from "@/lib/types";
import {
  AlertTriangle,
  ArrowRight,
  BookOpen,
  Check,
  ChevronDown,
  Clock,
  Compass,
  Copy,
  Languages,
  Lightbulb,
  Loader2,
  PenLine,
  RefreshCw,
  Sparkles,
  TrendingUp,
  Trophy,
  Zap,
} from "lucide-react";

const focusStyles: Record<string, { label: string; className: string }> = {
  task_response: { label: "Task Response", className: "bg-[#EAF0FC] text-[#3F63B8]" },
  coherence: { label: "Coherence", className: "bg-[#EEEDFB] text-[#4547B8]" },
  lexical: { label: "Lexical", className: "bg-[#DBE5FB] text-[#2F54B0]" },
  vocabulary: { label: "Vocabulary", className: "bg-[#EEEDFB] text-[#4547B8]" },
  grammar: { label: "Grammar", className: "bg-[#FFF0EB] text-[#E6531F]" },
  structure: { label: "Structure", className: "bg-[#E7F6EE] text-[#1F9E6B]" },
};

const focusVi: Record<string, string> = {
  task_response: "Trả lời đúng trọng tâm câu hỏi",
  coherence: "Tính mạch lạc giữa các đoạn văn",
  lexical: "Vốn từ vựng học thuật",
  vocabulary: "Vốn từ vựng",
  grammar: "Ngữ pháp đa dạng và chính xác",
  structure: "Bố cục bài viết",
};

const criteriaLegend: { en: string; vi: string }[] = [
  { en: "Task Achievement", vi: "Trả lời đúng trọng tâm" },
  { en: "Coherence and Cohesion", vi: "Tính mạch lạc" },
  { en: "Lexical Resource", vi: "Vốn từ vựng" },
  { en: "Grammatical Range & Accuracy", vi: "Ngữ pháp" },
];

const actionMeta: Record<string, { label: string; labelVi: string; icon: typeof PenLine }> = {
  write_essay: { label: "Write essay", labelVi: "Viết bài mới", icon: PenLine },
  review_history: { label: "Review essays", labelVi: "Xem bài đã viết", icon: Clock },
  practice_vocabulary: { label: "Practice words", labelVi: "Luyện từ vựng", icon: BookOpen },
  practice_quiz: { label: "Practice quiz", labelVi: "Làm quiz", icon: Zap },
};

function resolveFocusBadge(focus: string) {
  const key = focus.trim().toLowerCase().replace(/[\s_-]+/g, "_");
  return focusStyles[key] ?? { label: focus || "Practice", className: "bg-[#F2F3F5] text-[#808890]" };
}

function formatBand(band: number | null | undefined) {
  if (band == null) return "—";
  return Number.isInteger(band) ? String(band) : band.toFixed(1);
}

function toInsight(item: StudyGuideInsight | string): StudyGuideInsight {
  return typeof item === "string" ? { text: item, explanationVi: undefined } : item;
}

function isSafeRoute(target?: string): target is string {
  if (!target) return false;
  return (
    target.startsWith("/") &&
    !target.startsWith("//") &&
    !/^https?:\/\//i.test(target) &&
    !target.startsWith("javascript:")
  );
}

function buildWriteHref(topic: StudyGuideTopic): string {
  const params = new URLSearchParams();
  if (topic.title) params.set("title", topic.title);
  if (topic.suggestedPrompt) params.set("prompt", topic.suggestedPrompt);
  if (topic.ideaHints?.length) params.set("hints", JSON.stringify(topic.ideaHints));
  if (topic.keyVocabulary?.length) params.set("vocab", topic.keyVocabulary.join(", "));
  return `/write?${params.toString()}`;
}

function StepAction({ step }: { step: StudyGuideStep }) {
  if (!step.actionType || step.actionType === "none" || !isSafeRoute(step.actionTarget)) return null;
  const meta = actionMeta[step.actionType];

  if (!meta) {
    return (
      <Link
        href={step.actionTarget}
        className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-[#565FCC]/10 px-3.5 py-2 text-xs font-bold text-[#4547B8] transition-colors hover:bg-[#565FCC]/20"
      >
        Open <ArrowRight className="h-3.5 w-3.5" />
      </Link>
    );
  }

  return (
    <Link
      href={step.actionTarget}
      className="group inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-[#FF6D3A]/10 px-3.5 py-2 text-xs font-bold text-[#E6531F] transition-colors hover:bg-[#FF6D3A]/20"
    >
      <meta.icon className="h-3.5 w-3.5" />
      {meta.label}
      <ArrowRight className="h-3.5 w-3.5 transition-transform group-hover:translate-x-0.5" />
      <span className="hidden font-semibold text-[#E6531F]/70 sm:inline">· {meta.labelVi}</span>
    </Link>
  );
}

function ViLine({ text }: { text?: string }) {
  if (!text) return null;
  return (
    <p className="mt-1.5 flex items-start gap-1.5 text-xs leading-relaxed text-[#808890]">
      <span className="mt-px shrink-0 rounded bg-[#565FCC]/10 px-1 py-px text-[10px] font-bold leading-4 text-[#565FCC]">
        VI
      </span>
      {text}
    </p>
  );
}

function StudyPlanContent() {
  const [guide, setGuide] = useState<StudyGuide | null>(null);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);
  const [error, setError] = useState("");
  const [copied, setCopied] = useState(false);
  const [topicOpen, setTopicOpen] = useState(true);

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

  const strengths = useMemo(() => (guide?.strengths ?? []).map(toInsight), [guide]);
  const weaknesses = useMemo(() => (guide?.weaknesses ?? []).map(toInsight), [guide]);

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
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-[rgba(60,60,67,0.18)] bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <Compass className="h-4 w-4 text-[#565FCC]" />
          <h1 className="text-sm font-extrabold text-[#1D1D1F]">Study Plan</h1>
        </div>
        {guide && (
          <button
            type="button"
            onClick={() => void generate(true)}
            disabled={generating}
            className="flex items-center gap-2 rounded-full bg-[#FF6D3A] px-4 py-1.5 text-xs font-bold text-white shadow-sm shadow-[#FF6D3A]/30 transition-colors hover:bg-[#E6531F] disabled:cursor-not-allowed disabled:opacity-60"
          >
            <RefreshCw className={`h-3.5 w-3.5 ${generating ? "animate-spin" : ""}`} />
            {generating ? "Regenerating…" : "Regenerate"}
          </button>
        )}
      </div>

      <div className="mx-auto w-full max-w-5xl space-y-6 p-6 lg:p-10">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white p-10 text-sm font-semibold text-[#808890] shadow-[0_12px_32px_rgba(43,50,66,0.06)]">
            <Loader2 className="h-4 w-4 animate-spin text-[#FF6D3A]" />
            Loading your study plan
          </div>
        )}

        {error && (
          <div className="rounded-2xl border border-[#F3C4B2] bg-[#FFF0EB] px-4 py-3 text-sm font-semibold text-[#C2541F]">
            {error}
          </div>
        )}

        {!loading && !guide && !generating && (
          <div className="my-auto space-y-5 py-10">
            <div className="overflow-hidden rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white shadow-[0_24px_60px_rgba(43,50,66,0.08)]">
              <div className="relative overflow-hidden bg-[#2B3242] p-10 lg:p-14 lg:pb-24">
                <div className="pointer-events-none absolute -right-16 -top-16 h-56 w-56 rounded-full bg-[#FF6D3A]/30 blur-3xl" />
                <div className="pointer-events-none absolute -bottom-20 left-1/3 h-52 w-52 rounded-full bg-[#5A88E5]/25 blur-3xl" />
                <div className="relative mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
                  <Compass className="h-6 w-6 text-[#8DD8B5]" />
                </div>
                <h2 className="relative text-2xl font-extrabold tracking-tight text-white lg:text-3xl">
                  Your personalized roadmap to the band you want
                </h2>
                <p className="relative mt-3 max-w-2xl text-sm leading-relaxed text-[#C6CCD9] lg:text-base">
                  NomiWrite analyzes your graded essays, grammar errors, and vocabulary to build a
                  step-by-step study plan — your current band, exact strengths and weaknesses, and
                  three next moves to practice.
                </p>
              </div>
              <div className="-mt-10 flex justify-center lg:justify-start lg:px-14">
                <button
                  type="button"
                  onClick={() => void generate(false)}
                  className="flex items-center gap-2 rounded-2xl bg-[#FF6D3A] px-7 py-3.5 text-sm font-bold text-white shadow-lg shadow-[#FF6D3A]/30 transition-all hover:-translate-y-0.5 hover:bg-[#E6531F]"
                >
                  <Sparkles className="h-4 w-4" />
                  Generate my study plan
                </button>
              </div>
            </div>
            {!error && (
              <p className="text-center text-xs font-medium text-[#808890]">
                Needs at least one graded writing — add one in{" "}
                <Link href="/write" className="text-[#E6531F] hover:underline">Write</Link> or{" "}
                <Link href="/history" className="text-[#E6531F] hover:underline">History</Link>.
              </p>
            )}
          </div>
        )}

        {generating && (
          <div className="rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white p-10 text-center shadow-[0_12px_32px_rgba(43,50,66,0.06)]">
            <div className="relative mx-auto mb-4 h-12 w-12">
              <div className="absolute inset-0 animate-spin-slow rounded-full border-4 border-[#FFEDE4] border-t-[#FF6D3A]" />
              <Sparkles className="absolute inset-0 m-auto h-5 w-5 text-[#FF6D3A]" />
            </div>
            <p className="text-sm font-extrabold text-[#1D1D1F]">Analyzing your writing history…</p>
            <p className="mt-1 text-xs text-[#808890]">Reading graded essays, grammar errors, and vocabulary. This can take up to a minute.</p>
          </div>
        )}

        {!loading && guide && !generating && (
          <>
            <div className="overflow-hidden rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white shadow-[0_24px_60px_rgba(43,50,66,0.08)]">
              <div className="relative grid gap-0 overflow-hidden bg-[#2B3242] lg:grid-cols-[auto_1fr]">
                <div className="pointer-events-none absolute -right-10 -top-10 h-48 w-48 rounded-full bg-[#5A88E5]/25 blur-3xl" />
                <div className="pointer-events-none absolute -bottom-16 left-10 h-48 w-48 rounded-full bg-[#FF6D3A]/20 blur-3xl" />
                <div className="relative flex flex-col items-center justify-center gap-2 p-8 lg:min-w-64 lg:border-r lg:border-white/10">
                  <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-[#8DD8B5]">
                    <Trophy className="h-3.5 w-3.5 text-[#8DD8B5]" />
                    Estimated Band
                  </div>
                  {guide.analyzedEssayCount > 0 ? (
                    <>
                      <span className="text-6xl font-extrabold tracking-tight text-white lg:text-7xl">
                        {formatBand(guide.estimatedBand)}
                      </span>
                      {guide.targetBand != null && (
                        <div className="flex items-center gap-1.5 rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-white/90">
                          <TrendingUp className="h-3 w-3 text-[#8DD8B5]" />
                          Mục tiêu {formatBand(guide.targetBand)}
                        </div>
                      )}
                      {bandGap != null && (
                        <p className={`text-xs font-semibold ${bandGap > 0 ? "text-[#F0B79D]" : "text-[#8DD8B5]"}`}>
                          {bandGap > 0 ? `Còn ${formatBand(bandGap)} để đạt mục tiêu` : "Target reached"}
                        </p>
                      )}
                    </>
                  ) : (
                    <>
                      <span className="text-6xl font-extrabold tracking-tight text-white/40 lg:text-7xl">—</span>
                      <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-white/90">
                        Not graded yet
                      </span>
                      <Link
                        href="/write"
                        className="text-xs font-bold text-[#8DD8B5] transition-colors hover:text-[#B8EBD6]"
                      >
                        Write an essay to unlock your band →
                      </Link>
                    </>
                  )}
                </div>
                <div className="relative p-8 lg:p-10">
                  <div className="mb-3 flex flex-wrap gap-2">
                    <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-white/90">
                      {guide.targetExam || "IELTS Writing"}
                    </span>
                    <span className="rounded-full bg-white/10 px-3 py-1 text-xs font-bold text-white/90">
                      {guide.analyzedEssayCount > 0
                        ? `${guide.analyzedEssayCount} essay${guide.analyzedEssayCount === 1 ? "" : "s"} analyzed`
                        : "Grades pending"}
                    </span>
                  </div>
                  <p className="text-base leading-relaxed text-[#DDE1EA] lg:text-lg">{guide.summary}</p>
                  <p className="mt-4 text-xs font-semibold text-[#9AA3B5]">
                    Generated {new Date(guide.createdAt).toLocaleString()}
                  </p>
                </div>
              </div>
            </div>

            <div className="flex items-start gap-3 rounded-2xl border border-[rgba(60,60,67,0.10)] bg-white p-4 shadow-[0_12px_32px_rgba(43,50,66,0.06)]">
              <span className="mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-[#EEEDFB]">
                <Languages className="h-3.5 w-3.5 text-[#565FCC]" />
              </span>
              <div>
                <p className="text-xs font-extrabold text-[#1D1D1F]">IELTS criteria, in plain words</p>
                <p className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-xs text-[#808890]">
                  {criteriaLegend.map(item => (
                    <span key={item.en}>
                      <span className="font-bold text-[#4547B8]">{item.en}</span>
                      <span className="mx-1 text-[#C6CCD9]">/</span>
                      {item.vi}
                    </span>
                  ))}
                </p>
              </div>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              <div className="rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white p-6 shadow-[0_12px_32px_rgba(43,50,66,0.06)]">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-[#E7F6EE]">
                    <Check className="h-4 w-4 text-[#37C181]" />
                  </span>
                  <h3 className="text-base font-extrabold text-[#1D1D1F]">What you do well</h3>
                </div>
                <ul className="space-y-3">
                  {strengths.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-2xl bg-[#E7F6EE]/60 p-3.5">
                      <Check className="mt-0.5 h-4 w-4 shrink-0 text-[#37C181]" />
                      <div className="min-w-0">
                        <p className="text-sm leading-relaxed text-[#3A3A3D]">{item.text}</p>
                        <ViLine text={item.explanationVi} />
                      </div>
                    </li>
                  ))}
                  {strengths.length === 0 && (
                    <li className="text-sm text-[#808890]">No strengths recorded yet.</li>
                  )}
                </ul>
              </div>

              <div className="rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white p-6 shadow-[0_12px_32px_rgba(43,50,66,0.06)]">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-[#FFF0EB]">
                    <AlertTriangle className="h-4 w-4 text-[#FF6D3A]" />
                  </span>
                  <h3 className="text-base font-extrabold text-[#1D1D1F]">What to fix</h3>
                </div>
                <ul className="space-y-3">
                  {weaknesses.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-2xl bg-[#FFF0EB]/60 p-3.5">
                      <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-[#E6531F]" />
                      <div className="min-w-0">
                        <p className="text-sm leading-relaxed text-[#3A3A3D]">{item.text}</p>
                        <ViLine text={item.explanationVi} />
                      </div>
                    </li>
                  ))}
                  {weaknesses.length === 0 && (
                    <li className="text-sm text-[#808890]">No weaknesses flagged.</li>
                  )}
                </ul>
              </div>
            </div>

            <div className="rounded-3xl border border-[rgba(60,60,67,0.10)] bg-white p-6 shadow-[0_12px_32px_rgba(43,50,66,0.06)] lg:p-8">
              <div className="mb-6 flex items-center justify-between">
                <h3 className="text-base font-extrabold text-[#1D1D1F]">Your next three moves</h3>
                <span className="text-xs font-semibold text-[#808890]">Do these in order</span>
              </div>
              <div className="space-y-4">
                {guide.nextSteps.map((step, index) => {
                  const badge = resolveFocusBadge(step.focus);
                  const badgeKey = step.focus.trim().toLowerCase().replace(/[\s_-]+/g, "_");
                  const vi = focusVi[badgeKey];
                  return (
                    <div key={index} className="rounded-2xl border border-[rgba(60,60,67,0.10)] bg-[#FFFAF6] p-4">
                      <div className="flex items-start gap-4">
                        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-[#FF6D3A] to-[#E6531F] text-sm font-extrabold text-white shadow-sm shadow-[#FF6D3A]/30">
                          {index + 1}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="mb-1 flex flex-wrap items-center gap-2">
                            <p className="text-sm font-extrabold text-[#1D1D1F]">{step.title}</p>
                            <span
                              className={`cursor-help rounded-full px-2.5 py-0.5 text-[11px] font-bold ${badge.className}`}
                              title={vi ? `${badge.label} — ${vi}` : badge.label}
                            >
                              {badge.label}
                            </span>
                          </div>
                          <p className="text-sm leading-relaxed text-[#808890]">{step.description}</p>
                          <ViLine text={step.explanationVi} />
                        </div>
                        <StepAction step={step} />
                      </div>
                    </div>
                  );
                })}
                {guide.nextSteps.length === 0 && (
                  <p className="text-sm text-[#808890]">No steps planned yet.</p>
                )}
              </div>
            </div>

            <div className="overflow-hidden rounded-3xl border border-[#DBE5FB] bg-gradient-to-br from-[#EAF0FC] via-[#DBE5FB]/60 to-[#EEEDFB] shadow-[0_12px_32px_rgba(86,95,204,0.10)]">
              <div className="p-6 lg:p-8">
                <div className="mb-3 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-white">
                    <Sparkles className="h-4 w-4 text-[#565FCC]" />
                  </span>
                  <h3 className="text-base font-extrabold text-[#1D1D1F]">Practice topic recommended for you</h3>
                </div>
                <p className="text-sm font-bold text-[#2B3242]">{guide.recommendedTopic.title}</p>
                <p className="mt-1 text-sm leading-relaxed text-[#5A6072]">{guide.recommendedTopic.reason}</p>
                {guide.recommendedTopic.suggestedPrompt && (
                  <div className="mt-4 flex items-start gap-3 rounded-2xl border border-[rgba(60,60,67,0.10)] bg-white p-4">
                    <p className="flex-1 text-sm italic leading-relaxed text-[#3A3A3D]">
                      &ldquo;{guide.recommendedTopic.suggestedPrompt}&rdquo;
                    </p>
                    <button
                      type="button"
                      onClick={() => void copyPrompt()}
                      className="flex shrink-0 items-center gap-1.5 rounded-xl border border-[rgba(60,60,67,0.18)] px-3 py-1.5 text-xs font-bold text-[#565FCC] transition-colors hover:border-[#565FCC] hover:bg-[#EEEDFB]"
                    >
                      {copied ? <Check className="h-3.5 w-3.5 text-[#37C181]" /> : <Copy className="h-3.5 w-3.5" />}
                      {copied ? "Copied" : "Copy prompt"}
                    </button>
                  </div>
                )}

                {(guide.recommendedTopic.ideaHints?.length || guide.recommendedTopic.keyVocabulary?.length) ? (
                  <div className="mt-4 overflow-hidden rounded-2xl border border-[rgba(86,95,204,0.15)] bg-white">
                    <button
                      type="button"
                      onClick={() => setTopicOpen(value => !value)}
                      className="flex w-full items-center justify-between px-5 py-3.5 text-left"
                    >
                      <span className="flex items-center gap-2 text-sm font-extrabold text-[#2B3242]">
                        <Lightbulb className="h-4 w-4 text-[#565FCC]" />
                        Ideas & vocabulary to start writing
                      </span>
                      <ChevronDown className={`h-4 w-4 text-[#808890] transition-transform ${topicOpen ? "rotate-180" : ""}`} />
                    </button>
                    {topicOpen && (
                      <div className="grid gap-4 border-t border-[rgba(86,95,204,0.12)] px-5 py-4 sm:grid-cols-2">
                        {guide.recommendedTopic.ideaHints && guide.recommendedTopic.ideaHints.length > 0 && (
                          <div>
                            <p className="mb-2 text-[11px] font-bold uppercase tracking-widest text-[#565FCC]">
                              Brainstorming hints
                            </p>
                            <ul className="space-y-1.5">
                              {guide.recommendedTopic.ideaHints.map((hint, index) => (
                                <li key={index} className="flex items-start gap-2 text-xs leading-relaxed text-[#3A3A3D]">
                                  <span className="mt-0.5 h-1.5 w-1.5 shrink-0 rounded-full bg-[#565FCC]" />
                                  {hint}
                                </li>
                              ))}
                            </ul>
                          </div>
                        )}
                        {guide.recommendedTopic.keyVocabulary && guide.recommendedTopic.keyVocabulary.length > 0 && (
                          <div>
                            <p className="mb-2 text-[11px] font-bold uppercase tracking-widest text-[#565FCC]">
                              Topic vocabulary to reuse
                            </p>
                            <div className="flex flex-wrap gap-2">
                              {guide.recommendedTopic.keyVocabulary.map((term, index) => (
                                <span key={index} className="rounded-full bg-[#E7F6EE] px-3 py-1 text-xs font-bold text-[#1F9E6B]">
                                  {term}
                                </span>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                ) : null}
              </div>
              <div className="border-t border-[rgba(86,95,204,0.15)] bg-white/60 px-6 py-4 lg:px-8">
                <Link
                  href={buildWriteHref(guide.recommendedTopic)}
                  className="inline-flex items-center gap-2 rounded-2xl bg-[#FF6D3A] px-5 py-2.5 text-sm font-bold text-white shadow-md shadow-[#FF6D3A]/30 transition-all hover:-translate-y-0.5 hover:bg-[#E6531F]"
                >
                  Start writing this topic
                  <ArrowRight className="h-4 w-4" />
                </Link>
                <span className="ml-3 hidden text-xs font-medium text-[#5A6072] sm:inline">
                  Prompt + ideas auto-filled in the editor
                </span>
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