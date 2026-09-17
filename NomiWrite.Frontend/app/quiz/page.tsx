"use client";

import { Suspense, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { Quiz, QuizAttempt, QuizSummary } from "@/lib/types";
import { ArrowRight, BrainCircuit, Check, Home, Loader2, RotateCcw, Trophy, Zap } from "lucide-react";

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
    setError("");
  }

  async function startSavedQuiz(id: string) {
    setLoading(true);
    setError("");
    setAnswers({});
    setSubmittedAttempt(null);
    setCurrent(0);
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
        <div className="mx-auto w-full max-w-md space-y-5 p-6">
          <div className="rounded-3xl bg-gradient-to-br from-violet-600 to-blue-600 p-7 text-center text-white">
            <Trophy className="mx-auto mb-4 h-12 w-12" />
            <p className="text-4xl font-extrabold">{score}/{questions.length}</p>
            <p className="mt-1 text-sm text-violet-100">{pct}% correct</p>
          </div>

          {submittedAttempt.questionBreakdown?.length ? (
            <div className="space-y-3">
              {submittedAttempt.questionBreakdown.map((item, index) => (
                <div
                  key={item.questionId}
                  className={`rounded-2xl border bg-white p-4 shadow-sm ${
                    item.isCorrect ? "border-emerald-100" : "border-red-100"
                  }`}
                >
                  <div className="mb-3 flex items-center justify-between gap-3">
                    <span className="text-xs font-bold text-slate-400">Question {index + 1}</span>
                    <span
                      className={`rounded-full px-2.5 py-1 text-[11px] font-extrabold ${
                        item.isCorrect
                          ? "bg-emerald-50 text-emerald-700"
                          : "bg-red-50 text-red-600"
                      }`}
                    >
                      {item.isCorrect ? "Correct" : "Incorrect"}
                    </span>
                  </div>
                  <p className="text-sm font-bold leading-relaxed text-slate-900">{item.question}</p>
                  <div className="mt-3 grid gap-2 text-xs sm:grid-cols-2">
                    <div className="rounded-xl bg-slate-50 p-3">
                      <p className="mb-1 font-bold uppercase tracking-wide text-slate-400">Your answer</p>
                      <p className={`font-bold ${item.isCorrect ? "text-emerald-700" : "text-red-600"}`}>
                        {item.userAnswer || "No answer"}
                      </p>
                    </div>
                    <div className="rounded-xl bg-emerald-50 p-3">
                      <p className="mb-1 font-bold uppercase tracking-wide text-emerald-500">Correct answer</p>
                      <p className="font-bold text-emerald-800">{item.correctAnswer}</p>
                    </div>
                  </div>
                  {item.explanation && (
                    <div className="mt-3 rounded-xl border border-blue-100 bg-blue-50 p-3">
                      <p className="text-xs font-semibold leading-relaxed text-blue-800">{item.explanation}</p>
                    </div>
                  )}
                </div>
              ))}
            </div>
          ) : null}

          <button
            type="button"
            onClick={resetQuiz}
            className="flex w-full items-center justify-center gap-2 rounded-full border border-slate-200 bg-white py-3 text-sm font-bold text-slate-700 transition-all hover:bg-slate-50"
          >
            <RotateCcw className="h-4 w-4" />
            Practice again
          </button>
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
        <span className="text-xs font-semibold text-slate-400">{questions.length} questions</span>
      </div>

      <div className="mx-auto w-full max-w-xl space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Preparing quiz
          </div>
        )}

        {error && <p className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>}

        {!loading && !error && !submissionId && !question && (
          <>
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
                <p className="mt-1 text-xs leading-relaxed text-slate-500">
                  Personalized quizzes are generated from a submitted writing after AI feedback is ready.
                </p>
              </div>
            )}
          </>
        )}

        {!loading && question && (
          <>
            <div className="rounded-2xl border border-slate-100 bg-white p-6 shadow-sm">
              <div className="mb-4 flex items-center gap-2">
                <span className="rounded-full bg-violet-50 px-3 py-1 text-xs font-bold text-violet-700">{question.category}</span>
                <span className="text-xs text-slate-400">{current + 1}/{questions.length}</span>
              </div>
              <p className="mb-3 text-sm font-bold text-slate-900">{question.question}</p>
              <p className="text-sm leading-relaxed text-slate-600">{question.sentence}</p>
            </div>

            {isFreeTextQuestion ? (
              <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm">
                <label htmlFor={`quiz-answer-${question.id}`} className="mb-2 block text-xs font-bold uppercase tracking-wide text-slate-400">
                  Your answer
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
                  className="w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-3 text-sm font-semibold text-slate-800 outline-none transition-all focus:border-violet-400 focus:bg-white focus:ring-4 focus:ring-violet-500/10"
                  placeholder={question.type === "rewrite" ? "Type your rewritten sentence..." : "Type the missing word or phrase..."}
                  autoFocus
                />
              </div>
            ) : (
              <div className="grid grid-cols-1 gap-2.5">
                {(question.options ?? []).map(option => {
                  const active = selected === option;
                  return (
                    <button
                      key={option}
                      type="button"
                      onClick={() => handleSelect(option)}
                      disabled={isAnswered}
                      className={`flex items-center gap-3 rounded-xl border-2 px-4 py-3 text-left text-sm font-semibold transition-all ${
                        active
                          ? "border-violet-500 bg-violet-50 text-violet-700"
                          : "border-slate-200 bg-white text-slate-700 hover:border-violet-300 hover:bg-violet-50"
                      }`}
                    >
                      {active ? <Check className="h-4 w-4" /> : <Zap className="h-4 w-4 text-slate-300" />}
                      {option}
                    </button>
                  );
                })}
              </div>
            )}

            {isAnswered && question.explanation && (
              <div className="rounded-xl border border-blue-100 bg-blue-50 p-4">
                <p className="text-xs font-bold text-blue-800">{question.explanation}</p>
              </div>
            )}

            {isAnswered && (
              <button
                type="button"
                onClick={goNextOrFinish}
                className="flex w-full items-center justify-center gap-2 rounded-full bg-violet-600 py-3.5 text-sm font-bold text-white transition-all hover:bg-violet-700"
              >
                {current + 1 === questions.length ? "Finish quiz" : "Next question"}
                <ArrowRight className="h-4 w-4" />
              </button>
            )}
          </>
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
