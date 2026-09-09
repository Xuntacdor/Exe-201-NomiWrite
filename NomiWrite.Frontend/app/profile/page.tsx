"use client";

import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import AppShell from "../components/AppShell";
import {
  Award,
  BookOpen,
  Check,
  Edit2,
  Loader2,
  Mail,
  PenLine,
  Save,
  Target,
  TrendingUp,
  User as UserIcon,
  X,
  Zap,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { User } from "@/lib/types";

const levels = ["Beginner", "Elementary", "Intermediate", "UpperIntermediate", "Advanced", "Proficient"];
const targets = ["IELTS", "TOEFL", "Business Email", "Academic", "Cover Letter"];

function formatDate(value?: string) {
  if (!value) return "Not available";
  return new Intl.DateTimeFormat("en", { dateStyle: "medium" }).format(new Date(value));
}

export default function ProfilePage() {
  const router = useRouter();
  const [profile, setProfile] = useState<User | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [currentLevel, setCurrentLevel] = useState("");
  const [targetType, setTargetType] = useState("");
  const [targetBand, setTargetBand] = useState("");
  const [editing, setEditing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getSession()?.accessToken) {
      router.replace("/login");
      return;
    }

    let ignore = false;
    const loadTimer = setTimeout(() => {
      setLoading(true);
      apiClient.getMyAccount()
        .then(user => {
          if (ignore) return;
          setProfile(user);
          setDisplayName(user.displayName);
          setCurrentLevel(user.currentLevel ?? "");
          setTargetType(user.targetType ?? "");
          setTargetBand(user.targetBand?.toString() ?? "");
        })
        .catch(err => {
          if (!ignore) setError(err instanceof Error ? err.message : "Could not load profile.");
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

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSaving(true);
    setError("");
    try {
      const updated = await apiClient.updateMe({
        displayName: displayName.trim(),
        currentLevel: currentLevel || undefined,
        targetType: targetType || undefined,
        targetBand: targetBand ? Number(targetBand) : undefined,
      });
      setProfile(updated);
      setEditing(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not update profile.");
    } finally {
      setSaving(false);
    }
  }

  const initial = (profile?.displayName ?? "N").slice(0, 1).toUpperCase();
  const planLabel = profile?.plan === "premium" ? "Premium" : "Free";

  return (
    <AppShell activePath="/profile">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <UserIcon className="h-4 w-4 text-slate-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Learning profile</h1>
        </div>
        <button
          type="button"
          onClick={() => setEditing(value => !value)}
          className="flex items-center gap-1.5 rounded-lg bg-slate-100 px-3 py-1.5 text-xs font-semibold text-slate-600 transition-colors hover:bg-slate-200"
        >
          {editing ? <X className="h-3.5 w-3.5" /> : <Edit2 className="h-3.5 w-3.5" />}
          {editing ? "Cancel" : "Edit"}
        </button>
      </div>

      <div className="w-full space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading profile
          </div>
        )}

        {!loading && error && (
          <p className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>
        )}

        {!loading && profile && (
          <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
            <div className="space-y-4 lg:col-span-1">
              <div className="rounded-2xl border border-slate-100 bg-white p-6 text-center shadow-sm">
                <div className="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-violet-600 text-3xl font-extrabold text-white shadow-lg shadow-blue-200">
                  {initial}
                </div>
                <h2 className="text-base font-extrabold text-slate-900">{profile.displayName}</h2>
                <div className="mt-1 flex items-center justify-center gap-1.5 text-xs text-slate-500">
                  <Mail className="h-3 w-3" />
                  <span>{profile.email ?? getSession()?.email ?? "No email in profile service"}</span>
                </div>

                <div className="mt-4 flex flex-wrap items-center justify-center gap-2">
                  <span className="rounded-full border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-bold text-blue-700">
                    {profile.currentLevel ?? "Level not set"}
                  </span>
                  <span className="rounded-full border border-violet-200 bg-violet-50 px-3 py-1 text-xs font-bold text-violet-700">
                    {planLabel}
                  </span>
                </div>

                <div className="mt-5 border-t border-slate-100 pt-5">
                  <div className="mb-2 flex items-center gap-2 text-xs text-slate-500">
                    <Target className="h-3.5 w-3.5 text-blue-500" />
                    <span className="font-semibold text-slate-700">Goal:</span>
                    {profile.targetType ?? "Not set"} {profile.targetBand ? `Band ${profile.targetBand}` : ""}
                  </div>
                  <div className="flex items-center gap-2 text-xs text-slate-500">
                    <TrendingUp className="h-3.5 w-3.5 text-emerald-500" />
                    <span className="font-semibold text-slate-700">Subscription end:</span>
                    {formatDate(profile.subscriptionEndDate)}
                  </div>
                </div>
              </div>

              <div className="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <p className="mb-3 text-xs font-bold uppercase tracking-widest text-slate-400">Current plan</p>
                <p className="mb-1 text-base font-extrabold text-slate-900">{planLabel}</p>
                <p className="mb-4 text-xs leading-relaxed text-slate-500">
                  Subscription data is loaded from `/api/users/me/account`.
                </p>
                <Link href="/upgrade" className="block w-full rounded-xl bg-gradient-to-r from-blue-600 to-violet-600 py-2.5 text-center text-xs font-bold text-white transition-opacity hover:opacity-90">
                  Upgrade Premium
                </Link>
              </div>
            </div>

            <div className="space-y-4 lg:col-span-2">
              {editing ? (
                <form onSubmit={handleSubmit} className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                  <h3 className="mb-4 text-sm font-extrabold text-slate-900">Edit profile</h3>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="text-xs font-semibold text-slate-600">
                      Display name
                      <input
                        value={displayName}
                        onChange={event => setDisplayName(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-800 focus:border-blue-400 focus:outline-none"
                      />
                    </label>
                    <label className="text-xs font-semibold text-slate-600">
                      Current level
                      <select
                        value={currentLevel}
                        onChange={event => setCurrentLevel(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-800 focus:border-blue-400 focus:outline-none"
                      >
                        <option value="">Not set</option>
                        {levels.map(level => <option key={level} value={level}>{level}</option>)}
                      </select>
                    </label>
                    <label className="text-xs font-semibold text-slate-600">
                      Target
                      <select
                        value={targetType}
                        onChange={event => setTargetType(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-800 focus:border-blue-400 focus:outline-none"
                      >
                        <option value="">Not set</option>
                        {targets.map(target => <option key={target} value={target}>{target}</option>)}
                      </select>
                    </label>
                    <label className="text-xs font-semibold text-slate-600">
                      Target band
                      <input
                        type="number"
                        min="0"
                        max="9"
                        step="0.5"
                        value={targetBand}
                        onChange={event => setTargetBand(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-slate-200 px-3 py-2.5 text-sm text-slate-800 focus:border-blue-400 focus:outline-none"
                      />
                    </label>
                  </div>
                  <button
                    type="submit"
                    disabled={saving}
                    className="mt-5 flex items-center gap-2 rounded-xl bg-blue-600 px-4 py-2.5 text-xs font-bold text-white transition-colors hover:bg-blue-700 disabled:opacity-60"
                  >
                    {saving ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Save className="h-3.5 w-3.5" />}
                    Save profile
                  </button>
                </form>
              ) : (
                <div className="grid grid-cols-3 gap-3">
                  {[
                    { label: "Level", value: profile.currentLevel ?? "--", color: "text-slate-900", icon: PenLine },
                    { label: "Target", value: profile.targetType ?? "--", color: "text-blue-600", icon: Target },
                    { label: "Plan", value: planLabel, color: "text-orange-500", icon: Award },
                  ].map(({ label, value, color, icon: Icon }) => (
                    <div key={label} className="rounded-2xl border border-slate-100 bg-white p-4 text-center shadow-sm">
                      <Icon className="mx-auto mb-2 h-4 w-4 text-slate-300" />
                      <p className={`text-xl font-extrabold ${color}`}>{value}</p>
                      <p className="mt-0.5 text-xs text-slate-500">{label}</p>
                    </div>
                  ))}
                </div>
              )}

              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                <h3 className="mb-4 text-sm font-extrabold text-slate-900">API-backed profile fields</h3>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                  {[
                    { icon: UserIcon, label: "Profile", done: true },
                    { icon: Target, label: "Learning target", done: true },
                    { icon: Award, label: "Subscription", done: true },
                    { icon: BookOpen, label: "Vocabulary", done: false },
                    { icon: Zap, label: "Achievements", done: false },
                    { icon: PenLine, label: "Usage quota", done: false },
                  ].map(({ icon: Icon, label, done }) => (
                    <div key={label} className={`flex items-center gap-3 rounded-xl border p-3 ${done ? "border-emerald-200 bg-emerald-50" : "border-slate-100 bg-slate-50 opacity-60"}`}>
                      <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ${done ? "bg-emerald-500" : "bg-slate-200"}`}>
                        {done ? <Check className="h-4 w-4 text-white" /> : <Icon className="h-4 w-4 text-slate-400" />}
                      </div>
                      <span className={`text-xs font-semibold leading-tight ${done ? "text-emerald-800" : "text-slate-500"}`}>{label}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </AppShell>
  );
}
