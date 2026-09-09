"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import AppShell from "../components/AppShell";
import { AlertCircle, Check, ChevronRight, Clock, Filter, Loader2, PenLine } from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { handleAuthFailure } from "@/lib/auth/handle-auth-error";
import { getSession } from "@/lib/auth/session";
import type { Submission } from "@/lib/types";

function bandColor(band?: number) {
  if (!band) return "text-slate-500";
  if (band >= 7.0) return "text-emerald-600";
  if (band >= 6.0) return "text-blue-600";
  return "text-orange-500";
}

function bandBg(band?: number) {
  if (!band) return "bg-slate-50 border-slate-200";
  if (band >= 7.0) return "bg-emerald-50 border-emerald-200";
  if (band >= 6.0) return "bg-blue-50 border-blue-200";
  return "bg-orange-50 border-orange-200";
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(value));
}

export default function HistoryPage() {
  const router = useRouter();
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      apiClient.listSubmissions()
        .then(items => {
          if (!ignore) setSubmissions(items);
        })
        .catch(err => {
          if (ignore) return;
          if (handleAuthFailure(err, router)) return;
          setError(err instanceof Error ? err.message : "Could not load writing history.");
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
  const avg = gradedScores.length
    ? (gradedScores.reduce((sum, score) => sum + score, 0) / gradedScores.length).toFixed(1)
    : "--";
  const best = gradedScores.length ? Math.max(...gradedScores).toFixed(1) : "--";
  const sorted = useMemo(
    () => [...submissions].sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()),
    [submissions],
  );

  return (
    <AppShell activePath="/history">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <Clock className="h-4 w-4 text-slate-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Writing history</h1>
        </div>
        <div className="flex items-center gap-2">
          <button className="flex items-center gap-1.5 rounded-lg bg-slate-100 px-3 py-1.5 text-xs font-semibold text-slate-600 transition-colors hover:bg-slate-200">
            <Filter className="h-3.5 w-3.5" />
            Filter
          </button>
          <Link href="/write" className="flex items-center gap-1.5 rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-bold text-white transition-colors hover:bg-blue-700">
            <PenLine className="h-3.5 w-3.5" />
            New writing
          </Link>
        </div>
      </div>

      <div className="w-full space-y-5 p-6">
        <div className="grid grid-cols-3 gap-4">
          {[
            { label: "Total", value: submissions.length, unit: " essays", color: "text-slate-900" },
            { label: "Average band", value: avg, unit: "", color: "text-blue-600" },
            { label: "Best band", value: best, unit: "", color: "text-emerald-600" },
          ].map(({ label, value, unit, color }) => (
            <div key={label} className="rounded-2xl border border-slate-100 bg-white p-4 text-center shadow-sm">
              <p className={`text-2xl font-extrabold ${color}`}>{value}<span className="ml-0.5 text-base font-semibold">{unit}</span></p>
              <p className="mt-0.5 text-xs text-slate-500">{label}</p>
            </div>
          ))}
        </div>

        <div className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
          <div className="grid grid-cols-12 gap-4 border-b border-slate-100 px-5 py-3 text-xs font-bold uppercase tracking-widest text-slate-400">
            <span className="col-span-5">Writing</span>
            <span className="col-span-2">Status</span>
            <span className="col-span-2 text-center">Band</span>
            <span className="col-span-2 text-center">Words</span>
            <span className="col-span-1" />
          </div>

          {loading && (
            <div className="flex items-center justify-center gap-2 p-8 text-sm font-semibold text-slate-500">
              <Loader2 className="h-4 w-4 animate-spin" />
              Loading history
            </div>
          )}

          {!loading && error && (
            <p className="p-5 text-sm font-semibold text-red-600">{error}</p>
          )}

          {!loading && !error && sorted.length === 0 && (
            <div className="p-8 text-center">
              <p className="text-sm font-bold text-slate-800">No submissions yet</p>
              <p className="mt-1 text-xs text-slate-500">Start a writing session to create your first record.</p>
            </div>
          )}

          {!loading && !error && sorted.length > 0 && (
            <div className="divide-y divide-slate-50">
              {sorted.map(submission => (
                <Link
                  key={submission.id}
                  href={`/result?submissionId=${submission.id}`}
                  className="grid grid-cols-12 items-center gap-4 px-5 py-4 transition-colors hover:bg-slate-50 group"
                >
                  <div className="col-span-5">
                    <p className="truncate text-sm font-semibold text-slate-800 transition-colors group-hover:text-blue-600">{submission.topic}</p>
                    <p className="mt-0.5 text-xs text-slate-400">{formatDate(submission.submittedAt)}</p>
                  </div>
                  <div className="col-span-2">
                    <span className="rounded-full bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
                      {submission.status}
                    </span>
                  </div>
                  <div className="col-span-2 flex justify-center">
                    <span className={`rounded-xl border px-3 py-1 text-sm font-extrabold ${bandBg(submission.overallScore)} ${bandColor(submission.overallScore)}`}>
                      {submission.overallScore ?? "--"}
                    </span>
                  </div>
                  <div className="col-span-2 flex justify-center">
                    <div className="flex items-center gap-1.5">
                      <AlertCircle className="h-3.5 w-3.5 text-slate-300" />
                      <span className="text-sm font-semibold text-slate-700">{submission.wordCount}</span>
                    </div>
                  </div>
                  <div className="col-span-1 flex justify-end">
                    <ChevronRight className="h-4 w-4 text-slate-300 transition-colors group-hover:text-blue-500" />
                  </div>
                </Link>
              ))}
            </div>
          )}
        </div>

        <div className="flex items-start gap-3 rounded-2xl border border-blue-100 bg-blue-50 p-4">
          <Check className="mt-0.5 h-4 w-4 shrink-0 text-blue-500" />
          <div>
            <p className="text-sm font-semibold text-blue-800">History is connected to the Writing service.</p>
            <p className="mt-0.5 text-xs text-blue-600">Scores appear once the grading service has completed a submission.</p>
          </div>
        </div>
      </div>
    </AppShell>
  );
}
