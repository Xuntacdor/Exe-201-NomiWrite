"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import AppShell from "../components/AppShell";
import {
  AlertCircle,
  ArrowRight,
  Bell,
  BrainCircuit,
  BookOpen,
  ChevronRight,
  Flame,
  Loader2,
  PenLine,
  Sparkles,
  TrendingUp,
  X,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { Submission, User } from "@/lib/types";

const onboardingSteps = [
  { icon: PenLine, color: "bg-blue-600", title: "Choose a prompt", desc: "Writing types and prompts now come from the backend Writing service." },
  { icon: Sparkles, color: "bg-violet-600", title: "Submit for AI grading", desc: "Draft creation, content saving, and submission happen through authenticated APIs." },
  { icon: BrainCircuit, color: "bg-emerald-600", title: "Review and practice", desc: "Results are loaded from the grading endpoint when the AI result is ready." },
];

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(value));
}

export default function DashboardPage() {
  const router = useRouter();
  const [showOnboarding, setShowOnboarding] = useState(() => {
    if (typeof window === "undefined") return false;
    try {
      return !localStorage.getItem("nomiwrite_onboarded");
    } catch {
      return false;
    }
  });
  const [user, setUser] = useState<User | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      Promise.all([
        apiClient.getMyAccount().catch(() => apiClient.getMe()),
        apiClient.listSubmissions(),
      ])
        .then(([profile, history]) => {
          if (ignore) return;
          setUser(profile);
          setSubmissions(history);
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load dashboard.");
        })
        .finally(() => {
          if (!ignore) setLoading(false);
        });
    }, 0);

    return () => {
      ignore = true;
      clearTimeout(loadTimer);
    };
  }, [router]);

  const gradedScores = submissions.map(item => item.overallScore).filter((score): score is number => typeof score === "number");
  const averageScore = gradedScores.length
    ? (gradedScores.reduce((sum, score) => sum + score, 0) / gradedScores.length).toFixed(1)
    : "--";
  const recentSubmissions = useMemo(
    () => [...submissions].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()).slice(0, 4),
    [submissions],
  );
  const bandHistory = gradedScores.slice(-12);
  const maxBand = 9;

  const dismissOnboarding = () => {
    try {
      localStorage.setItem("nomiwrite_onboarded", "1");
    } catch {}
    setShowOnboarding(false);
  };

  return (
    <AppShell activePath="/dashboard">
      {showOnboarding && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <button aria-label="Close onboarding" className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={dismissOnboarding} />
          <div className="relative w-full max-w-md rounded-3xl bg-white p-8 shadow-2xl">
            <button
              type="button"
              onClick={dismissOnboarding}
              className="absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded-full bg-slate-100 text-slate-400 transition-colors hover:bg-slate-200"
            >
              <X className="h-4 w-4" />
            </button>
            <div className="mb-6 flex items-center gap-2.5">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 shadow-lg shadow-blue-200">
                <PenLine className="h-5 w-5 text-white" strokeWidth={2.5} />
              </div>
              <span className="text-lg font-extrabold text-slate-900">NomiWrite</span>
            </div>
            <h2 className="mb-2 text-2xl font-extrabold text-slate-900">Welcome back</h2>
            <p className="mb-7 text-sm leading-relaxed text-slate-500">
              The main writing loop is now connected to backend APIs where available.
            </p>
            <div className="mb-7 space-y-4">
              {onboardingSteps.map(step => {
                const Icon = step.icon;
                return (
                  <div key={step.title} className="flex items-start gap-4">
                    <div className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${step.color} shadow-sm`}>
                      <Icon className="h-4.5 w-4.5 text-white" style={{ width: 18, height: 18 }} />
                    </div>
                    <div>
                      <p className="text-sm font-bold text-slate-800">{step.title}</p>
                      <p className="mt-0.5 text-xs leading-relaxed text-slate-500">{step.desc}</p>
                    </div>
                  </div>
                );
              })}
            </div>
            <Link
              href="/write"
              onClick={dismissOnboarding}
              className="flex w-full items-center justify-center gap-2 rounded-2xl bg-blue-600 py-3.5 text-sm font-extrabold text-white shadow-lg shadow-blue-200 transition-all hover:-translate-y-0.5 hover:bg-blue-700"
            >
              <PenLine className="h-4 w-4" />
              Start writing
            </Link>
          </div>
        </div>
      )}

      <div className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-slate-100 bg-white/80 px-6 backdrop-blur">
        <div>
          <h1 className="text-base font-extrabold text-slate-900">Hello, {user?.displayName ?? "writer"}!</h1>
          <p className="text-xs text-slate-500">Write one piece today and keep the feedback loop warm.</p>
        </div>
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-1.5 rounded-full border border-orange-200 bg-orange-50 px-3 py-1.5 text-xs font-bold text-orange-600">
            <Flame className="h-3.5 w-3.5" />
            {submissions.length ? "Active" : "New"}
          </div>
          <button className="relative flex h-9 w-9 items-center justify-center rounded-full bg-slate-100 text-slate-500 transition-colors hover:bg-slate-200">
            <Bell className="h-4 w-4" />
          </button>
          <div className="flex h-9 w-9 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-violet-500 text-sm font-bold text-white">
            {(user?.displayName ?? "N").slice(0, 1).toUpperCase()}
          </div>
        </div>
      </div>

      <div className="w-full space-y-6 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading dashboard
          </div>
        )}

        {!loading && error && (
          <p className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>
        )}

        {!loading && !error && (
          <>
            <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
              {[
                { label: "Submissions", value: submissions.length, sub: "from Writing API", color: "text-blue-600", bg: "bg-blue-50", Icon: PenLine },
                { label: "Average band", value: averageScore, sub: "graded only", color: "text-emerald-600", bg: "bg-emerald-50", Icon: TrendingUp },
                { label: "Plan", value: user?.plan === "premium" ? "Premium" : "Free", sub: user?.subscriptionEndDate ? `until ${formatDate(user.subscriptionEndDate)}` : "subscription API", color: "text-violet-600", bg: "bg-violet-50", Icon: BrainCircuit },
                { label: "Prompts", value: "API", sub: "backend driven", color: "text-orange-600", bg: "bg-orange-50", Icon: BookOpen },
              ].map(({ label, value, sub, color, bg, Icon }) => (
                <div key={label} className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
                  <div className={`mb-3 flex h-9 w-9 items-center justify-center rounded-xl ${bg}`}>
                    <Icon className={color} style={{ width: 18, height: 18 }} />
                  </div>
                  <p className={`text-2xl font-extrabold ${color}`}>{value}</p>
                  <p className="mt-0.5 text-xs font-semibold text-slate-700">{label}</p>
                  <p className="mt-0.5 text-xs text-slate-400">{sub}</p>
                </div>
              ))}
            </div>

            <div className="grid grid-cols-1 gap-5 lg:grid-cols-5">
              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm lg:col-span-3">
                <div className="mb-5 flex items-center justify-between">
                  <div>
                    <h3 className="text-sm font-extrabold text-slate-900">Band history</h3>
                    <p className="mt-0.5 text-xs text-slate-400">Recent graded submissions</p>
                  </div>
                  <span className="rounded-full bg-emerald-50 px-2.5 py-1 text-xs font-bold text-emerald-700">
                    {bandHistory.length ? "Tracking" : "Waiting"}
                  </span>
                </div>
                {bandHistory.length ? (
                  <div className="flex h-28 items-end gap-1.5">
                    {bandHistory.map((band, index) => {
                      const pct = (band / maxBand) * 100;
                      const isLast = index === bandHistory.length - 1;
                      return (
                        <div key={`${band}-${index}`} className="flex flex-1 flex-col items-center gap-1">
                          <span className={`text-[10px] font-bold ${isLast ? "text-blue-600" : "text-slate-400"}`}>{isLast ? band : ""}</span>
                          <div className="flex w-full flex-col justify-end" style={{ height: 80 }}>
                            <div className={`w-full rounded-t-md ${isLast ? "bg-blue-600" : "bg-blue-200"}`} style={{ height: `${pct}%` }} />
                          </div>
                        </div>
                      );
                    })}
                  </div>
                ) : (
                  <p className="rounded-xl bg-slate-50 p-5 text-sm text-slate-500">Submit and grade a writing to see your score trend.</p>
                )}
              </div>

              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm lg:col-span-2">
                <div className="mb-5 flex items-center justify-between">
                  <div>
                    <h3 className="text-sm font-extrabold text-slate-900">Backend coverage</h3>
                    <p className="mt-0.5 text-xs text-slate-400">Connected modules</p>
                  </div>
                  <AlertCircle className="h-4 w-4 text-slate-300" />
                </div>
                <div className="space-y-3.5">
                  {["Auth", "User profile", "Writing", "Grading", "Subscription", "Payment"].map(label => (
                    <div key={label}>
                      <div className="mb-1 flex items-center justify-between">
                        <span className="text-xs font-medium text-slate-700">{label}</span>
                        <span className="text-xs font-bold text-slate-500">ready</span>
                      </div>
                      <div className="h-1.5 overflow-hidden rounded-full bg-slate-100">
                        <div className="h-full rounded-full bg-blue-500" style={{ width: "100%" }} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
              <div className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm lg:col-span-2">
                <div className="flex items-center justify-between border-b border-slate-50 px-5 py-4">
                  <h3 className="text-sm font-extrabold text-slate-900">Recent submissions</h3>
                  <Link href="/history" className="flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-700">
                    View all <ChevronRight className="h-3 w-3" />
                  </Link>
                </div>
                {recentSubmissions.length ? (
                  <div className="divide-y divide-slate-50">
                    {recentSubmissions.map(submission => (
                      <Link key={submission.id} href={`/result?submissionId=${submission.id}`} className="flex items-center justify-between px-5 py-3.5 transition-colors hover:bg-slate-50/60 group">
                        <div className="min-w-0 flex-1">
                          <p className="truncate text-sm font-semibold text-slate-800">{submission.topic}</p>
                          <div className="mt-0.5 flex items-center gap-2">
                            <span className="text-xs text-slate-400">{formatDate(submission.submittedAt)}</span>
                            <span className="text-slate-200">/</span>
                            <span className="text-xs font-medium text-slate-500">{submission.status}</span>
                          </div>
                        </div>
                        <ChevronRight className="ml-4 h-4 w-4 shrink-0 text-slate-300 transition-colors group-hover:text-slate-500" />
                      </Link>
                    ))}
                  </div>
                ) : (
                  <div className="p-8 text-center">
                    <p className="text-sm font-bold text-slate-800">No writing yet</p>
                    <p className="mt-1 text-xs text-slate-500">Your first submitted essay will appear here.</p>
                  </div>
                )}
              </div>

              <div className="space-y-4">
                <Link href="/write" className="block rounded-2xl bg-gradient-to-br from-blue-600 to-violet-600 p-5 text-white shadow-lg shadow-blue-200 transition-all hover:-translate-y-1 group">
                  <PenLine className="mb-3 h-6 w-6 text-blue-200" />
                  <p className="mb-1 text-base font-extrabold">New writing</p>
                  <p className="text-sm text-blue-100">Use backend prompts and grading.</p>
                  <div className="mt-3 flex items-center gap-1 text-xs font-semibold text-blue-200 transition-all group-hover:gap-2">
                    Start now <ArrowRight className="h-3.5 w-3.5" />
                  </div>
                </Link>
                <Link href="/upgrade" className="block rounded-2xl border border-violet-200 bg-white p-5 shadow-sm transition-all hover:-translate-y-1 hover:shadow-md group">
                  <BrainCircuit className="mb-3 h-6 w-6 text-violet-500" />
                  <p className="mb-0.5 text-sm font-extrabold text-slate-900">Subscription</p>
                  <p className="text-xs text-slate-500">Plans are loaded from the Subscription service.</p>
                </Link>
              </div>
            </div>
          </>
        )}
      </div>
    </AppShell>
  );
}
