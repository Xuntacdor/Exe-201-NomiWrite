"use client";

import { Suspense, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import Link from "next/link";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { Quiz, QuizAttempt, QuizSummary } from "@/lib/types";
import { ArrowRight, BrainCircuit, Check, Home, Loader2, RotateCcw, Sparkles, Trophy, Zap } from "lucide-react";

function QuizContent() {
  const params = useSearchParams();
  const submissionId = params.get("submissionId") ?? undefined;
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [submittedAttempt, setSubmittedAttempt] = useState<QuizAttempt | null>(null);
  const [quizHistory, setQuizHistory] = useState<QuizSummary[]>([]);
  const [current, setCurrent] = useState(0);
  const [loading, setLoading] = useState(Boolean(submissionId));
  const [loadingHistory, setLoadingHistory] = useState(!submissionId);
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
        <div className="sticky top-0 z-10 flex h-14 items-center gap-2 border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
          <button type="button" onClick={resetQuiz} className="text-slate-400 hover:text-slate-600">
            <Home className="h-4 w-4" />
          </button>
          <span className="text-sm font-extrabold text-slate-900">Quiz result</span>
        </div>
        <div className="mx-auto w-full max-w-4xl space-y-8 p-6 lg:p-10">
          <div className="overflow-hidden rounded-xl border border-slate-300 bg-white shadow-sm">
            <div className="bg-[#19325B] p-8 text-center text-white">
              <Trophy className="mx-auto mb-4 h-12 w-12 text-yellow-400" />
              <p className="text-4xl font-bold tracking-tight">{score} / {questions.length}</p>
              <p className="mt-2 text-lg font-medium text-blue-100">Total Score ({pct}%)</p>
            </div>
            
            {submittedAttempt.questionBreakdown?.length ? (
              <div className="divide-y divide-slate-200">
                {submittedAttempt.questionBreakdown.map((item, index) => (
                  <div key={item.questionId} className="p-8 lg:p-10">
                    <div className="mb-4 flex items-center justify-between">
                      <span className="text-sm font-bold tracking-wider text-slate-500 uppercase">Question {index + 1}</span>
                      <span className={`inline-flex items-center rounded px-3 py-1 text-sm font-bold ${
                        item.isCorrect ? "bg-emerald-100 text-emerald-800" : "bg-red-100 text-red-800"
                      }`}>
                        {item.isCorrect ? "Correct" : "Incorrect"}
                      </span>
                    </div>
                    
                    <p className="mb-6 text-lg font-semibold text-slate-900">{item.question}</p>
                    
                    <div className="grid gap-6 md:grid-cols-2">
                      <div className="rounded-lg border border-slate-200 bg-slate-50 p-5">
                        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-slate-500">Your Answer</p>
                        <p className={`text-lg font-semibold ${item.isCorrect ? "text-emerald-700" : "text-red-700"}`}>
                          {item.userAnswer || "No answer"}
                        </p>
                      </div>
                      <div className="rounded-lg border border-emerald-200 bg-emerald-50/50 p-5">
                        <p className="mb-2 text-xs font-bold uppercase tracking-wider text-emerald-600">Correct Answer</p>
                        <p className="text-lg font-semibold text-emerald-800">{item.correctAnswer}</p>
                      </div>
                    </div>
                    
                    {item.explanation && (
                      <div className="mt-6 rounded-lg bg-blue-50 p-5 border border-blue-100">
                        <p className="text-sm font-bold text-blue-800 mb-2">Explanation</p>
                        <p className="text-base text-blue-900 leading-relaxed">{item.explanation}</p>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : null}
            
            <div className="bg-slate-50 p-8 border-t border-slate-200 flex justify-center">
              <button
                type="button"
                onClick={resetQuiz}
                className="flex items-center gap-3 rounded bg-[#19325B] px-8 py-3 text-lg font-semibold text-white transition-colors hover:bg-blue-900 active:bg-blue-950"
              >
                <RotateCcw className="h-5 w-5" />
                Practice Again
              </button>
            </div>
          </div>
        </div>
      </AppShell>
    );
  }

  return (
    <AppShell activePath="/quiz">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <BrainCircuit className="h-4 w-4 text-violet-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Quiz practice</h1>
        </div>
        {!loading && question && !submittedAttempt && (
          <div className="absolute left-1/2 -translate-x-1/2 flex items-center gap-1 rounded-lg bg-slate-100 p-1 hidden sm:flex">
            <button
              onClick={() => setMode("test")}
              className={`rounded-md px-4 py-1 text-xs font-bold transition ${mode === "test" ? "bg-white shadow-sm text-slate-900" : "text-slate-500 hover:text-slate-700"}`}
            >
              Test Mode
            </button>
            <button
              onClick={() => setMode("flashcards")}
              className={`rounded-md px-4 py-1 text-xs font-bold transition ${mode === "flashcards" ? "bg-white shadow-sm text-slate-900" : "text-slate-500 hover:text-slate-700"}`}
            >
              Flashcards
            </button>
          </div>
        )}
        <span className="text-xs font-semibold text-slate-400">{questions.length} questions</span>
      </div>

      <div className="mx-auto flex min-h-[calc(100vh-3.5rem)] w-full max-w-[1400px] flex-col p-6 lg:p-10">
        {loading && (
          <div className="mt-4 flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Preparing quiz
          </div>
        )}

        {error && <p className="mt-4 rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>}

        {!loading && !error && !submissionId && !question && (
          <div className="w-full space-y-5 mt-2">
            {loadingHistory ? (
              <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading saved quizzes
              </div>
            ) : quizHistory.length ? (
              <div className="space-y-3">
                <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                  <p className="text-sm font-extrabold text-slate-900">Saved quizzes</p>
                  <p className="mt-1 text-xs leading-relaxed text-slate-500">
                    Practice again from quizzes generated from your previous writing feedback.
                  </p>
                </div>
                {quizHistory.map(item => (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => void startSavedQuiz(item.id)}
                    className="w-full rounded-2xl border border-slate-100 bg-white p-5 text-left shadow-sm transition-all hover:border-violet-200 hover:bg-violet-50/40"
                  >
                    <div className="mb-2 flex items-center justify-between gap-3">
                      <span className="rounded-full bg-violet-50 px-3 py-1 text-xs font-bold text-violet-700">
                        {item.category || "Quiz practice"}
                      </span>
                      <span className="text-xs font-semibold text-slate-400">
                        {item.questionCount} questions
                      </span>
                    </div>
                    <p className="text-sm font-extrabold text-slate-900">
                      {item.latestScore == null
                        ? "Not attempted yet"
                        : `Last score: ${item.latestScore}/${item.latestTotalQuestions ?? item.questionCount}`}
                    </p>
                    <p className="mt-1 text-xs text-slate-500">
                      {item.latestAttemptedAt
                        ? `Last practiced ${new Date(item.latestAttemptedAt).toLocaleString()}`
                        : `Created ${new Date(item.createdAt).toLocaleString()}`}
                    </p>
                  </button>
                ))}
              </div>
            ) : (
              <div className="rounded-2xl border border-slate-100 bg-white p-8 text-center shadow-sm">
                <p className="text-sm font-extrabold text-slate-900">Choose a graded writing first</p>
                <p className="mt-1 mb-6 text-xs leading-relaxed text-slate-500">
                  Personalized quizzes are generated from a submitted writing after AI feedback is ready.
                </p>
                <Link href="/history" className="inline-flex items-center justify-center gap-2 rounded-full bg-violet-600 px-6 py-3 text-sm font-bold text-white transition-all hover:bg-violet-700">
                  Go to History
                  <ArrowRight className="h-4 w-4" />
                </Link>
              </div>
            )}
          </div>
        )}

        {!loading && question && mode === "test" && (
          <div className="w-full overflow-hidden rounded-xl border border-slate-300 bg-white shadow-sm mt-2">
            {/* Header: Solid blue background with white text, typical of test software */}
            <div className="flex items-center justify-between bg-[#19325B] px-8 py-5 text-white">
              <div className="flex items-center gap-4">
                <BrainCircuit className="h-6 w-6" />
                <span className="text-xl font-semibold tracking-wide">{question.category}</span>
              </div>
              <div className="text-base font-medium">
                Question {current + 1} of {questions.length}
              </div>
            </div>

            {/* Split Content Area */}
            <div className="flex flex-col lg:flex-row lg:divide-x lg:divide-slate-200">
              {/* Left Side: Question and Context */}
              <div className="flex-1 bg-slate-50 p-10 lg:p-14">
                <h2 className="mb-8 text-xl font-semibold leading-relaxed text-slate-800 lg:text-2xl">
                  {question.question}
                </h2>
                {question.sentence && (
                  <div className="border-l-4 border-[#19325B] bg-white p-6 shadow-sm">
                    <p className="text-lg leading-relaxed text-slate-700 italic">
                      "{question.sentence}"
                    </p>
                  </div>
                )}
              </div>

              {/* Right Side: Options & Actions */}
              <div className="flex flex-1 flex-col justify-between bg-white p-10 lg:p-14">
                <div className="space-y-8">
                  {isFreeTextQuestion ? (
                    <div className="space-y-5">
                      <label htmlFor={`quiz-answer-${question.id}`} className="block text-base font-semibold text-slate-700">
                        Your Answer:
                      </label>
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
                        className="w-full border-b-2 border-slate-300 bg-slate-50 px-5 py-4 text-xl font-medium text-slate-800 transition-colors focus:border-[#19325B] focus:bg-white focus:outline-none"
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
                                ? "border-[#19325B] bg-blue-50/50"
                                : "border-slate-300 bg-white hover:bg-slate-50"
                            } ${isAnswered && !active ? 'opacity-50' : ''}`}
                          >
                            <div className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full border ${
                              active ? "border-[#19325B] bg-[#19325B]" : "border-slate-400 bg-white"
                            }`}>
                              {active && <div className="h-2.5 w-2.5 rounded-full bg-white" />}
                            </div>
                            <span className={`text-lg ${active ? "font-semibold text-[#19325B]" : "text-slate-700"}`}>
                              {option}
                            </span>
                          </button>
                        );
                      })}
                    </div>
                  )}

                  {isAnswered && question.explanation && (
                    <div className="mt-8 rounded border border-emerald-200 bg-emerald-50 p-6">
                      <p className="mb-3 text-base font-bold text-emerald-800">Feedback / Explanation</p>
                      <p className="text-base leading-relaxed text-emerald-900">{question.explanation}</p>
                    </div>
                  )}
                </div>

                {isAnswered && (
                  <div className="mt-10 flex justify-end pt-8">
                    <button
                      type="button"
                      onClick={goNextOrFinish}
                      className="flex items-center justify-center gap-3 rounded bg-[#19325B] px-10 py-4 text-lg font-semibold text-white transition-colors hover:bg-blue-900 active:bg-blue-950"
                    >
                      {current + 1 === questions.length ? "Finish Quiz" : "Next Question"}
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
            <div className="flex sm:hidden items-center gap-1 rounded-lg bg-slate-100 p-1 mb-2">
              <button
                onClick={() => setMode("test")}
                className={`rounded-md px-4 py-1.5 text-sm font-bold transition ${mode === "test" ? "bg-white shadow-sm text-slate-900" : "text-slate-500 hover:text-slate-700"}`}
              >
                Test Mode
              </button>
              <button
                onClick={() => setMode("flashcards")}
                className={`rounded-md px-4 py-1.5 text-sm font-bold transition ${mode === "flashcards" ? "bg-white shadow-sm text-slate-900" : "text-slate-500 hover:text-slate-700"}`}
              >
                Flashcards
              </button>
            </div>

            <div
              className="group relative h-[28rem] w-full max-w-3xl cursor-pointer"
              style={{ perspective: "1000px" }}
              onClick={() => setIsFlipped(!isFlipped)}
            >
              <div
                className="relative h-full w-full rounded-3xl shadow-lg transition-all duration-500"
                style={{ transformStyle: "preserve-3d", transform: isFlipped ? "rotateY(180deg)" : "rotateY(0deg)" }}
              >
                {/* Front */}
                <div
                  className="absolute inset-0 flex flex-col items-center justify-center rounded-3xl border border-slate-200 bg-white p-10 lg:p-16"
                  style={{ backfaceVisibility: "hidden" }}
                >
                  <p className="mb-6 text-center text-sm font-bold tracking-widest text-violet-500 uppercase">
                    {question.category}
                  </p>
                  <h2 className="text-center text-2xl font-bold leading-relaxed text-slate-800 lg:text-3xl">
                    {question.question}
                  </h2>
                  {question.sentence && (
                    <p className="mt-8 rounded-xl bg-slate-50 p-6 text-center text-lg italic text-slate-600 border border-slate-100">
                      "{question.sentence}"
                    </p>
                  )}
                  <p className="absolute bottom-8 flex items-center gap-2 text-sm font-bold text-slate-400">
                    <RotateCcw className="h-4 w-4" />
                    Click card to flip
                  </p>
                </div>

                {/* Back */}
                <div
                  className="absolute inset-0 flex flex-col items-center justify-center rounded-3xl border border-[#19325B] bg-[#19325B] p-10 lg:p-16 text-white"
                  style={{ backfaceVisibility: "hidden", transform: "rotateY(180deg)" }}
                >
                  <p className="mb-6 text-center text-sm font-bold tracking-widest text-blue-200/80 uppercase">
                    Answer
                  </p>
                  <h2 className="text-center text-2xl font-bold leading-relaxed lg:text-3xl">
                    {question.correctAnswer || "Check explanation for details"}
                  </h2>
                  {question.explanation && (
                    <div className="mt-8 max-h-[12rem] overflow-y-auto rounded-xl bg-white/10 p-6 text-center text-lg text-blue-50 backdrop-blur-sm scrollbar-thin scrollbar-thumb-white/20">
                      {question.explanation}
                    </div>
                  )}
                  <p className="absolute bottom-8 flex items-center gap-2 text-sm font-bold text-blue-300">
                    <RotateCcw className="h-4 w-4" />
                    Click card to flip back
                  </p>
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
                className="flex h-14 items-center justify-center rounded-full border border-slate-200 bg-white px-8 font-bold text-slate-700 shadow-sm transition-all hover:bg-slate-50 hover:shadow disabled:opacity-50 disabled:hover:shadow-sm"
              >
                Previous
              </button>
              <div className="flex items-center gap-3 font-bold text-slate-500">
                <span className="text-lg text-slate-900">{current + 1}</span>
                <span className="text-slate-300">/</span>
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
                className="flex h-14 items-center justify-center rounded-full bg-violet-600 px-8 font-bold text-white shadow-md transition-all hover:bg-violet-700 hover:shadow-lg active:scale-95"
              >
                {current === questions.length - 1 ? "Finish" : "Next"}
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
