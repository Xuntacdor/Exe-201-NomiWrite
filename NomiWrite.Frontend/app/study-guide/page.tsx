"use client";

import { useLocale } from "@/lib/i18n/locale";


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
  task_response: { label: "Task Response", className: "bg-accent text-accent-ink" },
  coherence: { label: "Coherence", className: "bg-rose text-rose-ink" },
  lexical: { label: "Lexical", className: "bg-accent text-accent-ink" },
  vocabulary: { label: "Vocabulary", className: "bg-rose text-rose-ink" },
  grammar: { label: "Grammar", className: "bg-accent text-accent-ink" },
  structure: { label: "Structure", className: "bg-success text-success-ink" },
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
  return focusStyles[key] ?? { label: focus || "Practice", className: "bg-surface-muted text-muted" };
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
  const { t: translateUi } = useLocale();
  if (!step.actionType || step.actionType === "none" || !isSafeRoute(step.actionTarget)) return null;
  const meta = actionMeta[step.actionType];

  if (!meta) {
    return (
      <Link
        href={step.actionTarget}
        className="inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-rose px-3.5 py-2 text-xs font-bold text-rose-ink transition-colors hover:bg-rose"
      >
        {translateUi("Open ")}<ArrowRight className="h-3.5 w-3.5" />
      </Link>
    );
  }

  return (
    <Link
      href={step.actionTarget}
      className="group inline-flex shrink-0 items-center gap-1.5 rounded-xl bg-accent px-3.5 py-2 text-xs font-bold text-accent-ink transition-colors hover:bg-accent-hover"
    >
      <meta.icon className="h-3.5 w-3.5" />
      {translateUi(meta.label)}
      <ArrowRight className="h-3.5 w-3.5 transition-transform " />
    </Link>
  );
}

function ViLine({ text }: { text?: string }) {
  const { locale } = useLocale();
  if (!text || locale !== "vi") return null;
  return (
    <p className="mt-1.5 flex items-start gap-1.5 text-xs leading-relaxed text-muted">
      <span className="mt-px shrink-0 rounded bg-rose px-1 py-px text-[10px] font-bold leading-4 text-rose-ink">
        VI</span>
      {text}
    </p>
  );
}

function StudyPlanContent() {
  const { t: translateUi, errorText, locale } = useLocale();
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
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          <Compass className="h-4 w-4 text-rose-ink" />
          <h1 className="text-sm font-bold text-ink">{translateUi("Study Plan")}</h1>
        </div>
        {guide && (
          <button
            type="button"
            onClick={() => void generate(true)}
            disabled={generating}
            className="flex items-center gap-2 rounded-full bg-accent px-4 py-1.5 text-xs font-bold text-ink shadow-sm transition-colors hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-60"
          >
            <RefreshCw className={`h-3.5 w-3.5 ${generating ? "animate-spin" : ""}`} />
            {translateUi(generating ? "Regenerating…" : "Regenerate")}
          </button>
        )}
      </div>

      <div className="mx-auto w-full max-w-5xl space-y-6 p-6 lg:p-10">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-10 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin text-accent-ink" />
            {translateUi("Loading your study plan")}</div>
        )}

        {error && (
          <div className="rounded-xl border border-line bg-accent px-4 py-3 text-sm font-semibold text-accent-ink">
            {errorText(error)}
          </div>
        )}

        {!loading && !guide && !generating && (
          <div className="my-auto space-y-5 py-10">
            <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
              <div className="relative overflow-hidden bg-accent p-10 lg:p-14 lg:pb-24">
                <div className="relative mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-surface">
                  <Compass className="h-6 w-6 text-success-ink" />
                </div>
                <h2 className="relative text-2xl font-bold tracking-tight text-ink lg:text-3xl">
                  {translateUi("Your personalized roadmap to the band you want")}</h2>
                <p className="relative mt-3 max-w-2xl text-base leading-relaxed text-muted lg:text-base">
                  {translateUi("NomiWrite analyzes your graded essays, grammar errors, and vocabulary to build a step-by-step study plan — your current band, exact strengths and weaknesses, and three next moves to practice.")}</p>
              </div>
              <div className="-mt-10 flex justify-center lg:justify-start lg:px-14">
                <button
                  type="button"
                  onClick={() => void generate(false)}
                  className="flex items-center gap-2 rounded-xl bg-accent px-7 py-3.5 text-sm font-bold text-ink shadow-sm transition-all  hover:bg-accent-hover"
                >
                  <Sparkles className="h-4 w-4" />
                  {translateUi("Generate my study plan")}</button>
              </div>
            </div>
            {!error && (
              <p className="text-center text-xs font-medium text-muted">
                {translateUi("Needs at least one graded writing — add one in")}{" "}
                <Link href="/write" className="text-accent-ink hover:underline">{translateUi("Write")}</Link> {translateUi(" or")}{" "}
                <Link href="/history" className="text-accent-ink hover:underline">{translateUi("History")}</Link>.
              </p>
            )}
          </div>
        )}

        {generating && (
          <div className="rounded-xl border border-line bg-surface p-10 text-center shadow-sm">
            <div className="relative mx-auto mb-4 h-12 w-12">
              <div className="absolute inset-0 animate-spin-slow rounded-full border-4 border-line border-t-line" />
              <Sparkles className="absolute inset-0 m-auto h-5 w-5 text-accent-ink" />
            </div>
            <p className="text-sm font-bold text-ink">{translateUi("Analyzing your writing history…")}</p>
            <p className="mt-1 text-xs text-muted">{translateUi("Reading graded essays, grammar errors, and vocabulary. This can take up to a minute.")}</p>
          </div>
        )}

        {!loading && guide && !generating && (
          <>
            <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
              <div className="relative grid gap-0 overflow-hidden bg-accent lg:grid-cols-[auto_1fr]">
                <div className="relative flex flex-col items-center justify-center gap-2 p-8 lg:min-w-64 lg:border-r lg:border-line">
                  <div className="flex items-center gap-2 text-xs font-bold uppercase tracking-widest text-success-ink">
                    <Trophy className="h-3.5 w-3.5 text-success-ink" />
                    {translateUi("Estimated Band")}</div>
                  {guide.analyzedEssayCount > 0 ? (
                    <>
                      <span className="text-6xl font-bold tracking-tight text-ink lg:text-7xl">
                        {translateUi(formatBand(guide.estimatedBand))}
                      </span>
                      {guide.targetBand != null && (
                        <div className="flex items-center gap-1.5 rounded-full bg-surface px-3 py-1 text-xs font-bold text-ink">
                          <TrendingUp className="h-3 w-3 text-success-ink" />
                          {translateUi("Mục tiêu ")}{translateUi(formatBand(guide.targetBand))}
                        </div>
                      )}
                      {bandGap != null && (
                        <p className={`text-xs font-semibold ${bandGap > 0 ? "text-accent-ink" : "text-success-ink"}`}>
                          {bandGap > 0 ? translateUi("{gap} points to your target", { gap: formatBand(bandGap) }) : translateUi("Target reached")}
                        </p>
                      )}
                    </>
                  ) : (
                    <>
                      <span className="text-6xl font-bold tracking-tight text-ink lg:text-7xl">—</span>
                      <span className="rounded-full bg-surface px-3 py-1 text-xs font-bold text-ink">
                        {translateUi("Not graded yet")}</span>
                      <Link
                        href="/write"
                        className="text-xs font-bold text-success-ink transition-colors hover:text-success-ink"
                      >
                        {translateUi("Write an essay to unlock your band →")}</Link>
                    </>
                  )}
                </div>
                <div className="relative p-8 lg:p-10">
                  <div className="mb-3 flex flex-wrap gap-2">
                    <span className="rounded-full bg-surface px-3 py-1 text-xs font-bold text-ink">
                      {translateUi(guide.targetExam || "IELTS Writing")}
                    </span>
                    <span className="rounded-full bg-surface px-3 py-1 text-xs font-bold text-ink">
                      {guide.analyzedEssayCount > 0
                        ? translateUi("Essays analyzed: {count}", { count: guide.analyzedEssayCount })
                        : translateUi("Grades pending")}
                    </span>
                  </div>
                  <p className="text-base leading-relaxed text-ink lg:text-lg">{translateUi(guide.summary)}</p>
                  <p className="mt-4 text-xs font-semibold text-muted">
                    {translateUi("Generated ")}{translateUi(new Date(guide.createdAt).toLocaleString(locale))}
                  </p>
                </div>
              </div>
            </div>

            <div className="flex items-start gap-3 rounded-xl border border-line bg-surface p-4 shadow-sm">
              <span className="mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-rose">
                <Languages className="h-3.5 w-3.5 text-rose-ink" />
              </span>
              <div>
                <p className="text-xs font-bold text-ink">{translateUi("IELTS criteria, in plain words")}</p>
                <p className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted">
                  {criteriaLegend.map(item => (
                    <span key={item.en}>
                      <span className="font-bold text-rose-ink">{translateUi(item.en)}</span>
                    </span>
                  ))}
                </p>
              </div>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-success">
                    <Check className="h-4 w-4 text-success-ink" />
                  </span>
                  <h3 className="text-base font-bold text-ink">{translateUi("What you do well")}</h3>
                </div>
                <ul className="space-y-3">
                  {strengths.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-xl bg-success p-3.5">
                      <Check className="mt-0.5 h-4 w-4 shrink-0 text-success-ink" />
                      <div className="min-w-0">
                        <p className="text-base leading-relaxed text-ink">{translateUi(item.text)}</p>
                        <ViLine text={item.explanationVi} />
                      </div>
                    </li>
                  ))}
                  {strengths.length === 0 && (
                    <li className="text-sm text-muted">{translateUi("No strengths recorded yet.")}</li>
                  )}
                </ul>
              </div>

              <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
                <div className="mb-4 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-accent">
                    <AlertTriangle className="h-4 w-4 text-accent-ink" />
                  </span>
                  <h3 className="text-base font-bold text-ink">{translateUi("What to fix")}</h3>
                </div>
                <ul className="space-y-3">
                  {weaknesses.map((item, index) => (
                    <li key={index} className="flex items-start gap-3 rounded-xl bg-accent p-3.5">
                      <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-accent-ink" />
                      <div className="min-w-0">
                        <p className="text-base leading-relaxed text-ink">{translateUi(item.text)}</p>
                        <ViLine text={item.explanationVi} />
                      </div>
                    </li>
                  ))}
                  {weaknesses.length === 0 && (
                    <li className="text-sm text-muted">{translateUi("No weaknesses flagged.")}</li>
                  )}
                </ul>
              </div>
            </div>

            <div className="rounded-xl border border-line bg-surface p-6 shadow-sm lg:p-8">
              <div className="mb-6 flex items-center justify-between">
                <h3 className="text-base font-bold text-ink">{translateUi("Your next three moves")}</h3>
                <span className="text-xs font-semibold text-muted">{translateUi("Do these in order")}</span>
              </div>
              <div className="space-y-4">
                {guide.nextSteps.map((step, index) => {
                  const badge = resolveFocusBadge(step.focus);
                  const badgeKey = step.focus.trim().toLowerCase().replace(/[\s_-]+/g, "_");
                  const vi = focusVi[badgeKey];
                  return (
                    <div key={index} className="rounded-xl border border-line bg-canvas p-4">
                      <div className="flex items-start gap-4">
                        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-accent text-sm font-bold text-ink shadow-sm">
                          {index + 1}
                        </div>
                        <div className="min-w-0 flex-1">
                          <div className="mb-1 flex flex-wrap items-center gap-2">
                            <p className="text-sm font-bold text-ink">{translateUi(step.title)}</p>
                            <span
                              className={`cursor-help rounded-full px-2.5 py-0.5 text-[11px] font-bold ${badge.className}`}
                              title={translateUi(locale === "vi" && vi ? vi : badge.label)}
                            >
                              {translateUi(badge.label)}
                            </span>
                          </div>
                          <p className="text-base leading-relaxed text-muted">{translateUi(step.description)}</p>
                          <ViLine text={step.explanationVi} />
                        </div>
                        <StepAction step={step} />
                      </div>
                    </div>
                  );
                })}
                {guide.nextSteps.length === 0 && (
                  <p className="text-sm text-muted">{translateUi("No steps planned yet.")}</p>
                )}
              </div>
            </div>

            <div className="overflow-hidden rounded-xl border border-line bg-accent shadow-sm">
              <div className="p-6 lg:p-8">
                <div className="mb-3 flex items-center gap-2">
                  <span className="flex h-8 w-8 items-center justify-center rounded-full bg-surface">
                    <Sparkles className="h-4 w-4 text-rose-ink" />
                  </span>
                  <h3 className="text-base font-bold text-ink">{translateUi("Practice topic recommended for you")}</h3>
                </div>
                <p className="text-sm font-bold text-accent-ink">{guide.recommendedTopic.title}</p>
                <p className="mt-1 text-base leading-relaxed text-muted">{translateUi(guide.recommendedTopic.reason)}</p>
                {guide.recommendedTopic.suggestedPrompt && (
                  <div className="mt-4 flex items-start gap-3 rounded-xl border border-line bg-surface p-4">
                    <p className="flex-1 text-sm italic leading-relaxed text-ink">
                      &ldquo;{guide.recommendedTopic.suggestedPrompt}&rdquo;
                    </p>
                    <button
                      type="button"
                      onClick={() => void copyPrompt()}
                      className="flex shrink-0 items-center gap-1.5 rounded-xl border border-line px-3 py-1.5 text-xs font-bold text-rose-ink transition-colors hover:border-line hover:bg-rose"
                    >
                      {copied ? <Check className="h-3.5 w-3.5 text-success-ink" /> : <Copy className="h-3.5 w-3.5" />}
                      {translateUi(copied ? "Copied" : "Copy prompt")}
                    </button>
                  </div>
                )}

                {(guide.recommendedTopic.ideaHints?.length || guide.recommendedTopic.keyVocabulary?.length) ? (
                  <div className="mt-4 overflow-hidden rounded-xl border border-line bg-surface">
                    <button
                      type="button"
                      onClick={() => setTopicOpen(value => !value)}
                      className="flex w-full items-center justify-between px-5 py-3.5 text-left"
                    >
                      <span className="flex items-center gap-2 text-sm font-bold text-accent-ink">
                        <Lightbulb className="h-4 w-4 text-rose-ink" />
                        {translateUi("Ideas & vocabulary to start writing")}</span>
                      <ChevronDown className={`h-4 w-4 text-muted transition-transform ${topicOpen ? "rotate-180" : ""}`} />
                    </button>
                    {topicOpen && (
                      <div className="grid gap-4 border-t border-line px-5 py-4 sm:grid-cols-2">
                        {guide.recommendedTopic.ideaHints && guide.recommendedTopic.ideaHints.length > 0 && (
                          <div>
                            <p className="mb-2 text-[11px] font-bold uppercase tracking-widest text-rose-ink">
                              {translateUi("Brainstorming hints")}</p>
                            <ul className="space-y-1.5">
                              {guide.recommendedTopic.ideaHints.map((hint, index) => (
                                <li key={index} className="flex items-start gap-2 text-xs leading-relaxed text-ink">
                                  <span className="mt-0.5 h-1.5 w-1.5 shrink-0 rounded-full bg-rose" />
                                  {hint}
                                </li>
                              ))}
                            </ul>
                          </div>
                        )}
                        {guide.recommendedTopic.keyVocabulary && guide.recommendedTopic.keyVocabulary.length > 0 && (
                          <div>
                            <p className="mb-2 text-[11px] font-bold uppercase tracking-widest text-rose-ink">
                              {translateUi("Topic vocabulary to reuse")}</p>
                            <div className="flex flex-wrap gap-2">
                              {guide.recommendedTopic.keyVocabulary.map((term, index) => (
                                <span key={index} className="rounded-full bg-success px-3 py-1 text-xs font-bold text-success-ink">
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
              <div className="border-t border-line bg-surface px-6 py-4 lg:px-8">
                <Link
                  href={buildWriteHref(guide.recommendedTopic)}
                  className="inline-flex items-center gap-2 rounded-xl bg-accent px-5 py-2.5 text-sm font-bold text-ink shadow-sm transition-all  hover:bg-accent-hover"
                >
                  {translateUi("Start writing this topic")}<ArrowRight className="h-4 w-4" />
                </Link>
                <span className="ml-3 hidden text-xs font-medium text-muted sm:inline">
                  {translateUi("Prompt + ideas auto-filled in the editor")}</span>
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
