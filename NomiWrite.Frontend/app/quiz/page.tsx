"use client";

import { useLocale } from "@/lib/i18n/locale";


import { Suspense, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { Quiz, QuizAttempt, QuizSummary, VocabGroup } from "@/lib/types";
import { ArrowRight, BrainCircuit, Home, Loader2, RotateCcw, Trophy, FolderSearch } from "lucide-react";

function QuizContent() {
  const { t: translateUi, errorText, locale } = useLocale();
  const params = useSearchParams();
  const submissionId = params.get("submissionId") ?? undefined;
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [submittedAttempt, setSubmittedAttempt] = useState<QuizAttempt | null>(null);
  const [quizHistory, setQuizHistory] = useState<QuizSummary[]>([]);
  const [current, setCurrent] = useState(0);
  const [loading, setLoading] = useState(Boolean(submissionId));
  const [loadingHistory, setLoadingHistory] = useState(!submissionId);
  const [vocabGroups, setVocabGroups] = useState<VocabGroup[]>([]);
  const [loadingGroups, setLoadingGroups] = useState(!submissionId);
  const [error, setError] = useState("");
  const [mode, setMode] = useState<"test" | "flashcards">("flashcards");
  const [isFlipped, setIsFlipped] = useState(false);

  useEffect(() => {
    if (!submissionId) {
      return;
    }

    let ignore = false;
    apiClient.generateQuiz({ sourceSubmissionId: submissionId })
      .then(result => {
        if (!ignore) setQuiz(result);
      })
      .catch(err => {
        if (!ignore) setError(err instanceof Error ? err.message : "Could not generate quiz.");
      })
      .finally(() => {
        if (!ignore) setLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, [submissionId]);

  useEffect(() => {
    if (submissionId) {
      return;
    }

    let ignore = false;
    apiClient.listQuizzes()
      .then(items => {
        if (!ignore) setQuizHistory(items);
      })
      .catch(() => {
        if (!ignore) setQuizHistory([]);
      })
      .finally(() => {
        if (!ignore) setLoadingHistory(false);
      });

    apiClient.listVocabGroups()
      .then(items => {
        if (!ignore) setVocabGroups(items);
      })
      .catch(() => {
        if (!ignore) setVocabGroups([]);
      })
      .finally(() => {
        if (!ignore) setLoadingGroups(false);
      });

    return () => {
      ignore = true;
    };
  }, [submissionId]);

  const questions = useMemo(() => quiz?.questions ?? [], [quiz]);
  const question = questions[current];
  const selected = question ? answers[question.id] : undefined;
  const isAnswered = Boolean(selected?.trim());
  const isFreeTextQuestion = question?.type === "fill_blank" || question?.type === "rewrite";

  function handleSelect(answer: string) {
    if (!question || (!isFreeTextQuestion && isAnswered)) return;
    setAnswers(currentAnswers => ({ ...currentAnswers, [question.id]: answer }));
  }

  async function handleFinish() {
    if (!quiz) return;

    try {
      const attempt = await apiClient.submitQuizAttempt({ quizId: quiz.id, answers });
      setSubmittedAttempt(attempt);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not submit quiz attempt.");
    }
  }

  function resetQuiz() {
    setAnswers({});
    setSubmittedAttempt(null);
    setCurrent(0);
    setIsFlipped(false);
    setError("");
  }

  async function startSavedQuiz(id: string) {
    setLoading(true);
    setError("");
    setAnswers({});
    setSubmittedAttempt(null);
    setCurrent(0);
    setIsFlipped(false);
    try {
      const detail = await apiClient.getQuiz(id);
      setQuiz(detail);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load this quiz.");
    } finally {
      setLoading(false);
    }
  }

  async function generateQuizFromGroup(groupId: string, vocabularyIds: string[]) {
    if (vocabularyIds.length === 0) {
      setError("This group has no vocabulary words.");
      return;
    }
    setLoading(true);
    setError("");
    setAnswers({});
    setSubmittedAttempt(null);
    setCurrent(0);
    setIsFlipped(false);
    try {
      const result = await apiClient.generateQuiz({ vocabularyIds });
      setQuiz(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not generate quiz.");
    } finally {
      setLoading(false);
    }
  }

  function goNextOrFinish() {
    if (current + 1 === questions.length) {
      void handleFinish();
      return;
    }

    setCurrent(index => index + 1);
    setIsFlipped(false);
  }

  if (submittedAttempt) {
    const total = submittedAttempt.totalQuestions ?? questions.length;
    const score = submittedAttempt.score;
    const pct = total ? Math.round((score / total) * 100) : 0;
    return (
      <AppShell activePath="/quiz">
        <div className="sticky top-0 z-10 flex h-14 items-center gap-2 border-b border-line bg-surface px-6">
          <button type="button" onClick={resetQuiz} className="text-muted hover:text-muted">
            <Home className="h-4 w-4" />
          </button>
          <span className="text-sm font-bold text-ink">{translateUi("Quiz result")}</span>
        </div>
        <div className="mx-auto w-full max-w-4xl space-y-8 p-6 lg:p-10">
          <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
            <div className="bg-accent p-8 text-center text-ink">
              <Trophy className="mx-auto mb-4 h-12 w-12 text-warning-ink" />
              <p className="text-4xl font-bold tracking-tight">{score} / {questions.length}</p>
              <p className="mt-2 text-lg font-medium text-accent-ink">{translateUi("Total Score (")}{pct}%)</p>
            </div>

            {submittedAttempt.questionBreakdown?.length ? (
              <div className="divide-y divide-line">
                {submittedAttempt.questionBreakdown.map((item, index) => (
                  <div key={item.questionId} className="p-8 lg:p-10">
                    <div className="mb-4 flex items-center justify-between">
                      <span className="text-sm font-bold tracking-wider text-muted uppercase">{translateUi("Question ")}{index + 1}</span>
                      <span className={`inline-flex items-center rounded px-3 py-1 text-sm font-bold ${
                        item.isCorrect ? "bg-success text-success-ink" : "bg-danger text-danger-ink"
                      }`}>
                        {translateUi(item.isCorrect ? "Correct" : "Incorrect")}
                      </span>
                    </div>

                    <p className="mb-6 text-lg font-semibold text-ink">{item.question}</p>

                    <div className="grid gap-6 md:grid-cols-2">
                      <div className="rounded-lg border border-line bg-canvas p-5">
                        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-muted">{translateUi("Your Answer")}</p>
                        <p className={`text-lg font-semibold ${item.isCorrect ? "text-success-ink" : "text-danger-ink"}`}>
                          {item.userAnswer || translateUi("No answer")}
                        </p>
                      </div>
                      <div className="rounded-lg border border-line bg-success p-5">
                        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-success-ink">{translateUi("Correct Answer")}</p>
                        <p className="text-lg font-semibold text-success-ink">{item.correctAnswer}</p>
                      </div>
                    </div>

                    {item.explanation && (
                      <div className="mt-6 rounded-lg bg-accent p-5 border border-line">
                        <p className="text-sm font-bold text-accent-ink mb-2">{translateUi("Explanation")}</p>
                        <p className="text-base text-accent-ink leading-relaxed">{item.explanation}</p>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : null}

            <div className="bg-canvas p-8 border-t border-line flex justify-center">
              <button
                type="button"
                onClick={resetQuiz}
                className="flex items-center gap-3 rounded bg-accent px-8 py-3 text-lg font-semibold text-ink transition-colors hover:bg-accent-hover active:bg-accent-hover"
              >
                <RotateCcw className="h-5 w-5" />
                {translateUi("Practice Again")}</button>
            </div>
          </div>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell activePath="/quiz">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          <BrainCircuit className="h-4 w-4 text-rose-ink" />
          <h1 className="text-sm font-bold text-ink">{translateUi("Quiz practice")}</h1>
        </div>
        {!loading && question && !submittedAttempt && (
          <div className="absolute left-1/2 -translate-x-1/2 flex items-center gap-1 rounded-lg bg-surface-muted p-1 hidden sm:flex">
            <button
              onClick={() => setMode("test")}
              className={`rounded-md px-4 py-1 text-xs font-bold transition ${mode === "test" ? "bg-surface shadow-sm text-ink" : "text-muted hover:text-ink"}`}
            >
              {translateUi("Test Mode")}</button>
            <button
              onClick={() => setMode("flashcards")}
              className={`rounded-md px-4 py-1 text-xs font-bold transition ${mode === "flashcards" ? "bg-surface shadow-sm text-ink" : "text-muted hover:text-ink"}`}
            >
              {translateUi("Flashcards")}</button>
          </div>
        )}
        <span className="text-xs font-semibold text-muted">{questions.length} {translateUi(" questions")}</span>
      </div>

      <div className="mx-auto flex min-h-[calc(100vh-3.5rem)] w-full max-w-[1400px] flex-col p-6 lg:p-10">
        {loading && (
          <div className="mt-4 flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            {translateUi("Preparing quiz")}</div>
        )}

        {error && <p className="mt-4 rounded-xl border border-line bg-danger p-4 text-sm font-semibold text-danger-ink">{errorText(error)}</p>}

        {!loading && !error && !submissionId && !question && (
          <div className="w-full space-y-5 mt-2">
            {loadingHistory ? (
              <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
                <Loader2 className="h-4 w-4 animate-spin" />
                {translateUi("Loading saved quizzes")}</div>
            ) : quizHistory.length ? (
              <div className="space-y-3">
                <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                  <p className="text-sm font-bold text-ink">{translateUi("Saved quizzes")}</p>
                  <p className="mt-1 text-xs leading-relaxed text-muted">
                    {translateUi("Practice again from quizzes generated from your previous writing feedback.")}</p>
                </div>
                {quizHistory.map(item => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => void startSavedQuiz(item.id)}
                    className="w-full rounded-xl border border-line bg-surface p-5 text-left shadow-sm transition-all hover:border-line hover:bg-rose"
                  >
                    <div className="mb-2 flex items-center justify-between gap-3">
                      <span className="rounded-full bg-rose px-3 py-1 text-xs font-bold text-rose-ink">
                        {translateUi(item.category || "Quiz practice")}
                      </span>
                      <span className="text-xs font-semibold text-muted">
                        {translateUi(item.questionCount)} {translateUi(" questions")}</span>
                    </div>
                    <p className="text-sm font-bold text-ink">
                      {item.latestScore == null
                        ? translateUi("Not attempted yet")
                        : translateUi("Last score: {score}/{total}", { score: item.latestScore, total: item.latestTotalQuestions ?? item.questionCount })}
                    </p>
                    <p className="mt-1 text-xs text-muted">
                      {translateUi(item.latestAttemptedAt ? "Last practiced {date}" : "Created {date}", { date: new Date(item.latestAttemptedAt ?? item.createdAt).toLocaleString(locale) })}
                    </p>
                  </button>
                ))}
              </div>
            ) : null}

            {!loadingGroups && vocabGroups.length > 0 && (
              <div className="space-y-3 mt-8">
                <div className="rounded-2xl border border-line bg-surface p-5 shadow-sm">
                  <p className="text-sm font-extrabold text-ink">Vocabulary Groups</p>
                  <p className="mt-1 text-xs leading-relaxed text-muted">
                    Practice vocabulary words from your saved groups.
                  </p>
                </div>
                {vocabGroups.map(group => (
                  <button
                    key={group.id}
                    type="button"
                    onClick={() => void generateQuizFromGroup(group.id, group.vocabularyIds)}
                    className="w-full rounded-2xl border border-line bg-surface p-5 text-left shadow-sm transition-all hover:border-line hover:bg-accent"
                  >
                    <div className="mb-2 flex items-center justify-between gap-3">
                      <span className="rounded-full bg-accent px-3 py-1 text-xs font-bold text-accent-ink flex items-center gap-1">
                        <FolderSearch className="h-3 w-3" />
                        Custom Group
                      </span>
                      <span className="text-xs font-semibold text-muted">
                        {group.wordCount} words
                      </span>
                    </div>
                    <p className="text-sm font-extrabold text-ink">
                      {group.name}
                    </p>
                    <p className="mt-1 text-xs text-muted">
                      Created {new Date(group.createdAt).toLocaleDateString()}
                    </p>
                  </button>
                ))}
              </div>
            )}

            {!loadingHistory && !quizHistory.length && !loadingGroups && !vocabGroups.length && (
              <div className="rounded-2xl border border-line bg-surface p-8 text-center shadow-sm">
                <p className="text-sm font-extrabold text-ink">Choose a graded writing first</p>
                <p className="mt-1 mb-6 text-xs leading-relaxed text-muted">
                  Personalized quizzes are generated from a submitted writing after AI feedback is ready.
                </p>
                <Link href="/history" className="inline-flex items-center justify-center gap-2 rounded-full bg-accent px-6 py-3 text-sm font-bold text-ink transition-all hover:bg-accent">
                  Go to History
                  <ArrowRight className="h-4 w-4" />
                </Link>
              </div>
            )}
          </div>
        )}

        {!loading && question && mode === "test" && (
          <div className="w-full overflow-hidden rounded-xl border border-line bg-surface shadow-sm mt-2">
            {/* Header: Solid blue background with white text, typical of test software */}
            <div className="flex items-center justify-between bg-accent px-8 py-5 text-ink">
              <div className="flex items-center gap-4">
                <BrainCircuit className="h-6 w-6" />
                <span className="text-xl font-semibold tracking-wide">{translateUi(question.category)}</span>
              </div>
              <div className="text-base font-medium">
                {translateUi("Question ")}{current + 1} {translateUi(" of ")}{questions.length}
              </div>
            </div>

            {/* Split Content Area */}
            <div className="flex flex-col lg:flex-row lg:divide-x lg:divide-line">
              {/* Left Side: Question and Context */}
              <div className="flex-1 bg-canvas p-10 lg:p-14">
                <h2 className="mb-8 text-xl font-semibold leading-relaxed text-ink lg:text-2xl">
                  {question.question}
                </h2>
                {question.sentence && (
                  <div className="border-l-4 border-line bg-surface p-6 shadow-sm">
                    <p className="text-lg leading-relaxed text-ink italic">
                      &quot;{question.sentence}&quot;
                    </p>
                  </div>
                )}
              </div>

              {/* Right Side: Options & Actions */}
              <div className="flex flex-1 flex-col justify-between bg-surface p-10 lg:p-14">
                <div className="space-y-8">
                  {isFreeTextQuestion ? (
                    <div className="space-y-5">
                      <label htmlFor={`quiz-answer-${question.id}`} className="block text-base font-semibold text-ink">
                        {translateUi("Your Answer:")}</label>
                      <input
                        id={`quiz-answer-${question.id}`}
                        type="text"
                        value={selected ?? ""}
                        onChange={event => handleSelect(event.target.value)}
                        onKeyDown={event => {
                          if (event.key === "Enter" && selected?.trim()) {
                            event.preventDefault();
                            goNextOrFinish();
                          }
                        }}
                        className="w-full border-b-2 border-line bg-canvas px-5 py-4 text-xl font-medium text-ink transition-colors focus:border-line focus:bg-surface focus:outline-none"
                        placeholder={question.type === "rewrite" ? "Type your rewritten sentence..." : "Type the missing word or phrase..."}
                        autoFocus
                      />
                    </div>
                  ) : (
                    <div className="space-y-4">
                      {(question.options ?? []).map(option => {
                        const active = selected === option;
                        return (
                          <button
                            key={option}
                            type="button"
                            onClick={() => handleSelect(option)}
                            disabled={isAnswered}
                            className={`flex w-full items-center gap-5 border p-5 text-left transition-colors ${
                              active
                                ? "border-line bg-accent"
                                : "border-line bg-surface hover:bg-canvas"
                            } ${isAnswered && !active ? 'opacity-50' : ''}`}
                          >
                            <div className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full border ${
                              active ? "border-line bg-accent" : "border-line bg-surface"
                            }`}>
                              {active && <div className="h-2.5 w-2.5 rounded-full bg-surface" />}
                            </div>
                            <span className={`text-lg ${active ? "font-semibold text-accent-ink" : "text-ink"}`}>
                              {option}
                            </span>
                          </button>
                        );
                      })}
                    </div>
                  )}

                  {isAnswered && question.explanation && (
                    <div className="mt-8 rounded border border-line bg-success p-6">
                      <p className="mb-3 text-base font-bold text-success-ink">{translateUi("Feedback / Explanation")}</p>
                      <p className="text-base leading-relaxed text-success-ink">{question.explanation}</p>
                    </div>
                  )}
                </div>

                {isAnswered && (
                  <div className="mt-10 flex justify-end pt-8">
                    <button
                      type="button"
                      onClick={goNextOrFinish}
                      className="flex items-center justify-center gap-3 rounded bg-accent px-10 py-4 text-lg font-semibold text-ink transition-colors hover:bg-accent-hover active:bg-accent-hover"
                    >
                      {translateUi(current + 1 === questions.length ? "Finish Quiz" : "Next Question")}
                      <ArrowRight className="h-5 w-5" />
                    </button>
                  </div>
                )}
              </div>
            </div>
          </div>
        )}

        {!loading && question && mode === "flashcards" && (
          <div className="flex w-full flex-col items-center justify-start space-y-8 py-6 mt-2">
            {/* Mobile Mode Switcher (Visible only on small screens) */}
            <div className="flex sm:hidden items-center gap-1 rounded-lg bg-surface-muted p-1 mb-2">
              <button
                onClick={() => setMode("test")}
                className="rounded-md px-4 py-1.5 text-sm font-bold transition text-muted hover:text-ink"
              >
                {translateUi("Test Mode")}</button>
              <button
                onClick={() => setMode("flashcards")}
                className="rounded-md px-4 py-1.5 text-sm font-bold transition bg-surface shadow-sm text-ink"
              >
                {translateUi("Flashcards")}</button>
            </div>

            <div
              className="group relative h-[28rem] w-full max-w-3xl cursor-pointer"
              style={{ perspective: "1000px" }}
              onClick={() => setIsFlipped(!isFlipped)}
            >
              <div
                className="relative h-full w-full rounded-xl shadow-sm transition-all duration-500"
                style={{ transformStyle: "preserve-3d", transform: isFlipped ? "rotateY(180deg)" : "rotateY(0deg)" }}
              >
                {/* Front */}
                <div
                  className="absolute inset-0 flex flex-col items-center justify-center rounded-xl border border-line bg-surface p-10 lg:p-16"
                  style={{ backfaceVisibility: "hidden" }}
                >
                  <p className="mb-6 text-center text-sm font-bold tracking-widest text-rose-ink uppercase">
                    {translateUi(question.category)}
                  </p>
                  <h2 className="text-center text-2xl font-bold leading-relaxed text-ink lg:text-3xl">
                    {question.question}
                  </h2>
                  {question.sentence && (
                    <p className="mt-8 rounded-xl bg-canvas p-6 text-center text-lg italic text-muted border border-line">
                      &quot;{question.sentence}&quot;
                    </p>
                  )}
                  <p className="absolute bottom-8 flex items-center gap-2 text-sm font-bold text-muted">
                    <RotateCcw className="h-4 w-4" />
                    {translateUi("Click card to flip")}</p>
                </div>

                {/* Back */}
                <div
                  className="absolute inset-0 flex flex-col items-center justify-center rounded-xl border border-line bg-accent p-10 lg:p-16 text-ink"
                  style={{ backfaceVisibility: "hidden", transform: "rotateY(180deg)" }}
                >
                  <p className="mb-6 text-center text-sm font-bold tracking-widest text-accent-ink uppercase">
                    {translateUi("Answer")}</p>
                  <h2 className="text-center text-2xl font-bold leading-relaxed lg:text-3xl">
                    {question.correctAnswer || "Check explanation for details"}
                  </h2>
                  {question.explanation && (
                    <div className="mt-8 max-h-[12rem] overflow-y-auto rounded-xl bg-surface p-6 text-center text-lg text-accent-ink scrollbar-thin scrollbar-thumb-line">
                      {question.explanation}
                    </div>
                  )}
                  <p className="absolute bottom-8 flex items-center gap-2 text-sm font-bold text-accent-ink">
                    <RotateCcw className="h-4 w-4" />
                    {translateUi("Click card to flip back")}</p>
                </div>
              </div>
            </div>

            <div className="flex w-full max-w-3xl items-center justify-between px-4 sm:px-0">
              <button
                type="button"
                onClick={() => {
                  if (current > 0) {
                    setCurrent(c => c - 1);
                    setIsFlipped(false);
                  }
                }}
                disabled={current === 0}
                className="flex h-14 items-center justify-center rounded-full border border-line bg-surface px-8 font-bold text-ink shadow-sm transition-all hover:bg-canvas hover:shadow disabled:opacity-50 disabled:hover:shadow-sm"
              >
                {translateUi("Previous")}</button>
              <div className="flex items-center gap-3 font-bold text-muted">
                <span className="text-lg text-ink">{current + 1}</span>
                <span className="text-muted">/</span>
                <span>{questions.length}</span>
              </div>
              <button
                type="button"
                onClick={() => {
                  if (current < questions.length - 1) {
                    setCurrent(c => c + 1);
                    setIsFlipped(false);
                  } else {
                    void handleFinish();
                  }
                }}
                className="flex h-14 items-center justify-center rounded-full bg-rose px-8 font-bold text-ink shadow-sm transition-all hover:bg-rose hover:shadow-sm active:scale-95"
              >
                {translateUi(current === questions.length - 1 ? "Finish" : "Next")}
              </button>
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}

export default function QuizPage() {
  return (
    <Suspense>
      <QuizContent />
    </Suspense>
  );
}
