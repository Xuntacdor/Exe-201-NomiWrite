"use client";

import { Suspense, useEffect, useMemo, useState } from "react";
import { useSearchParams } from "next/navigation";
import AppShell from "../components/AppShell";
import { apiClient, apiMode } from "@/lib/api/client";
import type { Quiz, QuizAttempt } from "@/lib/types";
import { ArrowRight, BrainCircuit, Check, Home, Loader2, RotateCcw, Sparkles, Trophy, X, Zap } from "lucide-react";

function QuizContent() {
  const params = useSearchParams();
  const submissionId = params.get("submissionId") ?? undefined;
  const [quiz, setQuiz] = useState<Quiz | null>(null);
  const [answers, setAnswers] = useState<Record<string, string>>({});
  const [submittedAttempt, setSubmittedAttempt] = useState<QuizAttempt | null>(null);
  const [current, setCurrent] = useState(0);
  const [loading, setLoading] = useState(apiMode === "mock");
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real") {
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

  const questions = useMemo(() => quiz?.questions ?? [], [quiz]);
  const question = questions[current];
  const selected = question ? answers[question.id] : undefined;
  const isAnswered = Boolean(selected);
  const score = useMemo(
    () => questions.filter(item => answers[item.id] === item.correctAnswer).length,
    [answers, questions],
  );

  function handleSelect(answer: string) {
    if (!question || isAnswered) return;
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

  if (apiMode === "real") {
    return (
      <AppShell activePath="/quiz">
        <div className="sticky top-0 z-10 flex h-14 items-center gap-2 border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
          <BrainCircuit className="h-4 w-4 text-violet-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Quiz practice</h1>
        </div>
        <div className="w-full p-6">
          <div className="rounded-2xl border border-violet-100 bg-violet-50 p-6">
            <div className="mb-3 flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-violet-600" />
              <h2 className="text-base font-extrabold text-violet-900">Quiz API pending</h2>
            </div>
            <p className="max-w-2xl text-sm leading-relaxed text-violet-700">
              This frontend flow is ready, but the pulled backend does not expose quiz generation or quiz attempt endpoints yet. Plan status stays 1/2 until those APIs arrive.
            </p>
          </div>
        </div>
      </AppShell>
    );
  }

  if (submittedAttempt) {
    const pct = questions.length ? Math.round((score / questions.length) * 100) : 0;
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

            <div className="grid grid-cols-1 gap-2.5">
              {(question.options ?? []).map(option => {
                const correct = isAnswered && option === question.correctAnswer;
                const wrong = isAnswered && selected === option && !correct;
                return (
                  <button
                    key={option}
                    type="button"
                    onClick={() => handleSelect(option)}
                    disabled={isAnswered}
                    className={`flex items-center gap-3 rounded-xl border-2 px-4 py-3 text-left text-sm font-semibold transition-all ${
                      correct ? "border-emerald-500 bg-emerald-50 text-emerald-700" :
                      wrong ? "border-red-400 bg-red-50 text-red-600" :
                      "border-slate-200 bg-white text-slate-700 hover:border-violet-300 hover:bg-violet-50"
                    }`}
                  >
                    {correct ? <Check className="h-4 w-4" /> : wrong ? <X className="h-4 w-4" /> : <Zap className="h-4 w-4 text-slate-300" />}
                    {option}
                  </button>
                );
              })}
            </div>

            {isAnswered && (
              <div className="rounded-xl border border-blue-100 bg-blue-50 p-4">
                <p className="text-xs font-bold text-blue-800">{question.explanation}</p>
              </div>
            )}

            {isAnswered && (
              <button
                type="button"
                onClick={() => current + 1 === questions.length ? void handleFinish() : setCurrent(index => index + 1)}
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
