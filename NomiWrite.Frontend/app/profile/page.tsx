"use client";

import { useLocale } from "@/lib/i18n/locale";


import Link from "next/link";
import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import AppDialog from "../components/AppDialog";
import AppShell from "../components/AppShell";
import {
  Award,
  BookOpen,
  Check,
  Edit2,
  LoaderCircle,
  Loader2,
  Mail,
  PenLine,
  Save,
  ShieldOff,
  Target,
  TrendingUp,
  User as UserIcon,
  X,
  Zap,
} from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { clearSession, getSession } from "@/lib/auth/session";
import type { User, UserProgress } from "@/lib/types";

const levels = ["Beginner", "Elementary", "Intermediate", "UpperIntermediate", "Advanced", "Proficient"];
const targets = ["IELTS", "TOEFL", "Business Email", "Academic", "Cover Letter"];

function formatDate(value: string | undefined, locale: string) {
  if (!value) return "Not available";
  return new Intl.DateTimeFormat(locale, { dateStyle: "medium" }).format(new Date(value));
}

export default function ProfilePage() {
  const { t: translateUi, errorText, locale } = useLocale();
  const router = useRouter();
  const [profile, setProfile] = useState<User | null>(null);
  const [displayName, setDisplayName] = useState("");
  const [currentLevel, setCurrentLevel] = useState("");
  const [targetType, setTargetType] = useState("");
  const [targetBand, setTargetBand] = useState("");
  const [editing, setEditing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [progress, setProgress] = useState<UserProgress | null>(null);
  const [loadingProgress, setLoadingProgress] = useState(false);
  const [saving, setSaving] = useState(false);
  const [accountAction, setAccountAction] = useState<"cancel" | "deactivate" | "">("");
  const [confirmAction, setConfirmAction] = useState<"cancel" | "deactivate" | "">("");
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real" && !getSession()?.accessToken) {
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

      setLoadingProgress(true);
      apiClient.getUserProgress()
        .then(data => {
          if (!ignore) setProgress(data);
        })
        .catch(() => {
          if (!ignore) setProgress(null);
        })
        .finally(() => {
          if (!ignore) setLoadingProgress(false);
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

  async function handleCancelSubscription() {
    setAccountAction("cancel");
    setError("");
    try {
      const status = await apiClient.cancelSubscription();
      setProfile(current => current ? {
        ...current,
        plan: status.hasSubscription ? "premium" : "free",
        subscriptionEndDate: status.status?.endDate ?? current.subscriptionEndDate,
      } : current);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not cancel subscription.");
    } finally {
      setAccountAction("");
    }
  }

  async function handleDeactivateAccount() {
    setAccountAction("deactivate");
    setError("");
    try {
      await apiClient.deactivateAccount();
      clearSession();
      router.push("/login");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not deactivate account.");
    } finally {
      setAccountAction("");
    }
  }

  function handleConfirmAccountAction() {
    const action = confirmAction;
    setConfirmAction("");

    if (action === "cancel") {
      void handleCancelSubscription();
    }

    if (action === "deactivate") {
      void handleDeactivateAccount();
    }
  }

  return (
    <AppShell activePath="/profile">
      <AppDialog
        open={confirmAction === "cancel"}
        title={translateUi("Cancel subscription?")}
        description="Your current subscription will be cancelled through the backend subscription service."
        confirmLabel="Cancel subscription"
        tone="danger"
        onCancel={() => setConfirmAction("")}
        onConfirm={handleConfirmAccountAction}
      />
      <AppDialog
        open={confirmAction === "deactivate"}
        title={translateUi("Deactivate account?")}
        description="You will be signed out after the account deactivation request succeeds."
        confirmLabel="Deactivate account"
        tone="danger"
        onCancel={() => setConfirmAction("")}
        onConfirm={handleConfirmAccountAction}
      />
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          <UserIcon className="h-4 w-4 text-muted" />
          <h1 className="text-sm font-bold text-ink">{translateUi("Learning profile")}</h1>
        </div>
        <button
          type="button"
          onClick={() => setEditing(value => !value)}
          className="flex items-center gap-1.5 rounded-lg bg-surface-muted px-3 py-1.5 text-xs font-semibold text-muted transition-colors hover:bg-surface-muted"
        >
          {editing ? <X className="h-3.5 w-3.5" /> : <Edit2 className="h-3.5 w-3.5" />}
          {translateUi(editing ? "Cancel" : "Edit")}
        </button>
      </div>

      <div className="w-full space-y-5 p-6">
        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            {translateUi("Loading profile")}</div>
        )}

        {!loading && error && (
          <p className="rounded-xl border border-line bg-danger p-4 text-sm font-semibold text-danger-ink">{errorText(error)}</p>
        )}

        {!loading && profile && (
          <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
            <div className="space-y-4 lg:col-span-1">
              <div className="rounded-xl border border-line bg-surface p-6 text-center shadow-sm">
                <div className="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-xl bg-accent text-3xl font-bold text-ink shadow-sm">
                  {translateUi(initial)}
                </div>
                <h2 className="text-base font-bold text-ink">{profile.displayName}</h2>
                <div className="mt-1 flex items-center justify-center gap-1.5 text-xs text-muted">
                  <Mail className="h-3 w-3" />
                  <span>{translateUi(profile.email ?? getSession()?.email ?? "No email in profile service")}</span>
                </div>

                <div className="mt-4 flex flex-wrap items-center justify-center gap-2">
                  <span className="rounded-full border border-line bg-accent px-3 py-1 text-xs font-bold text-accent-ink">
                    {translateUi(profile.currentLevel ?? "Level not set")}
                  </span>
                  <span className="rounded-full border border-line bg-rose px-3 py-1 text-xs font-bold text-rose-ink">
                    {translateUi(planLabel)}
                  </span>
                </div>

                <div className="mt-5 border-t border-line pt-5">
                  <div className="mb-2 flex items-center gap-2 text-xs text-muted">
                    <Target className="h-3.5 w-3.5 text-accent-ink" />
                    <span className="font-semibold text-ink">{translateUi("Goal:")}</span>
                    {translateUi(profile.targetType ?? "Not set")} {profile.targetBand ? translateUi("Score {score}", { score: profile.targetBand }) : ""}
                  </div>
                  <div className="flex items-center gap-2 text-xs text-muted">
                    <TrendingUp className="h-3.5 w-3.5 text-success-ink" />
                    <span className="font-semibold text-ink">{translateUi("Subscription end:")}</span>
                    {translateUi(formatDate(profile.subscriptionEndDate, locale))}
                  </div>
                </div>
              </div>

              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <p className="mb-3 text-xs font-bold uppercase tracking-widest text-muted">{translateUi("Current plan")}</p>
                <p className="mb-1 text-base font-bold text-ink">{translateUi(planLabel)}</p>
                <Link href="/upgrade" className="block w-full rounded-xl bg-accent py-2.5 text-center text-xs font-bold text-ink transition-opacity hover:opacity-90">
                  {translateUi("Upgrade Premium")}</Link>
                {profile.plan === "premium" && (
                  <button
                    type="button"
                    onClick={() => setConfirmAction("cancel")}
                    disabled={accountAction === "cancel"}
                    className="mt-2 flex w-full items-center justify-center gap-2 rounded-xl border border-line bg-warning py-2.5 text-xs font-bold text-warning-ink transition-colors hover:bg-warning disabled:opacity-60"
                  >
                    {accountAction === "cancel" && <LoaderCircle className="h-3.5 w-3.5 animate-spin" />}
                    {translateUi("Cancel subscription")}</button>
                )}
              </div>

              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <p className="mb-2 text-xs font-bold uppercase tracking-widest text-danger-ink">{translateUi("Account safety")}</p>
                <button
                  type="button"
                  onClick={() => setConfirmAction("deactivate")}
                  disabled={accountAction === "deactivate"}
                  className="flex w-full items-center justify-center gap-2 rounded-xl border border-line bg-danger py-2.5 text-xs font-bold text-danger-ink transition-colors hover:bg-danger disabled:opacity-60"
                >
                  {accountAction === "deactivate" ? <LoaderCircle className="h-3.5 w-3.5 animate-spin" /> : <ShieldOff className="h-3.5 w-3.5" />}
                  {translateUi("Deactivate account")}</button>
              </div>
            </div>

            <div className="space-y-4 lg:col-span-2">
              {editing ? (
                <form onSubmit={handleSubmit} className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                  <h3 className="mb-4 text-sm font-bold text-ink">{translateUi("Edit profile")}</h3>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <label className="text-xs font-semibold text-muted">
                      {translateUi("Display name")}<input
                        value={displayName}
                        onChange={event => setDisplayName(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-line px-3 py-2.5 text-sm text-ink focus:border-focus focus:outline-none"
                      />
                    </label>
                    <label className="text-xs font-semibold text-muted">
                      {translateUi("Current level")}<select
                        value={currentLevel}
                        onChange={event => setCurrentLevel(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-line px-3 py-2.5 text-sm text-ink focus:border-focus focus:outline-none"
                      >
                        <option value="">{translateUi("Not set")}</option>
                        {levels.map(level => <option key={level} value={level}>{translateUi(level)}</option>)}
                      </select>
                    </label>
                    <label className="text-xs font-semibold text-muted">
                      {translateUi("Target")}<select
                        value={targetType}
                        onChange={event => setTargetType(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-line px-3 py-2.5 text-sm text-ink focus:border-focus focus:outline-none"
                      >
                        <option value="">{translateUi("Not set")}</option>
                        {targets.map(target => <option key={target} value={target}>{translateUi(target)}</option>)}
                      </select>
                    </label>
                    <label className="text-xs font-semibold text-muted">
                      {translateUi("Target band")}<input
                        type="number"
                        min="0"
                        max="9"
                        step="0.5"
                        value={targetBand}
                        onChange={event => setTargetBand(event.target.value)}
                        className="mt-1.5 w-full rounded-xl border border-line px-3 py-2.5 text-sm text-ink focus:border-focus focus:outline-none"
                      />
                    </label>
                  </div>
                  <button
                    type="submit"
                    disabled={saving}
                    className="mt-5 flex items-center gap-2 rounded-xl bg-accent px-4 py-2.5 text-xs font-bold text-ink transition-colors hover:bg-accent-hover disabled:opacity-60"
                  >
                    {saving ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <Save className="h-3.5 w-3.5" />}
                    {translateUi("Save profile")}</button>
                </form>
              ) : (
                <div className="grid grid-cols-3 gap-3">
                  {[
                    { label: "Level", value: profile.currentLevel ?? "--", color: "text-ink", icon: PenLine },
                    { label: "Target", value: profile.targetType ?? "--", color: "text-accent-ink", icon: Target },
                    { label: "Plan", value: planLabel, color: "text-accent-ink", icon: Award },
                  ].map(({ label, value, color, icon: Icon }) => (
                    <div key={label} className="rounded-xl border border-line bg-surface p-4 text-center shadow-sm">
                      <Icon className="mx-auto mb-2 h-4 w-4 text-muted" />
                      <p className={`text-xl font-bold ${color}`}>{translateUi(value)}</p>
                      <p className="mt-0.5 text-xs text-muted">{translateUi(label)}</p>
                    </div>
                  ))}
                </div>
              )}

              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <div className="mb-4 flex items-center justify-between">
                  <h3 className="text-sm font-bold text-ink">{translateUi("Learning progress")}</h3>
                  {loadingProgress && <Loader2 className="h-4 w-4 animate-spin text-muted" />}
                </div>
                {progress ? (
                  <div className="space-y-4">
                    <div className="grid grid-cols-3 gap-3">
                      {[
                        { label: "Submissions", value: progress.totalSubmissions, icon: PenLine },
                        { label: "Streak", value: progress.currentStreak, icon: Zap },
                        { label: "Target", value: progress.targetBand ?? "--", icon: Target },
                      ].map(({ label, value, icon: Icon }) => (
                        <div key={label} className="rounded-xl border border-line bg-canvas p-3 text-center">
                          <Icon className="mx-auto mb-2 h-4 w-4 text-accent-ink" />
                          <p className="text-xl font-bold text-ink">{value}</p>
                          <p className="text-xs text-muted">{translateUi(label)}</p>
                        </div>
                      ))}
                    </div>
                    {progress.strengthsWeaknesses && (
                      <p className="rounded-xl border border-line bg-accent p-3 text-xs leading-relaxed text-accent-ink">
                        {translateUi(progress.strengthsWeaknesses)}
                      </p>
                    )}
                    <div className="flex flex-wrap gap-2">
                      {progress.badges.map(badge => (
                        <span
                          key={badge.name}
                          className={`rounded-full px-3 py-1 text-xs font-bold ${badge.achieved ? "bg-success text-success-ink" : "bg-surface-muted text-muted"}`}
                        >
                          {translateUi(badge.name)}
                        </span>
                      ))}
                    </div>
                  </div>
                ) : (
                  <p className="rounded-xl bg-canvas p-4 text-sm text-muted">
                    {translateUi("Progress endpoint is ready in the frontend; no progress data was returned yet.")}</p>
                )}
              </div>

              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <h3 className="mb-4 text-sm font-bold text-ink">{translateUi("API-backed profile fields")}</h3>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3">
                  {[
                    { icon: UserIcon, label: "Profile", done: true },
                    { icon: Target, label: "Learning target", done: true },
                    { icon: Award, label: "Subscription", done: true },
                    { icon: BookOpen, label: "Vocabulary", done: false },
                    { icon: Zap, label: "Progress", done: Boolean(progress) },
                    { icon: PenLine, label: "Usage quota", done: false },
                  ].map(({ icon: Icon, label, done }) => (
                    <div key={label} className={`flex items-center gap-3 rounded-xl border p-3 ${done ? "border-line bg-success" : "border-line bg-canvas opacity-60"}`}>
                      <div className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg ${done ? "bg-success" : "bg-surface-muted"}`}>
                        {done ? <Check className="h-4 w-4 text-ink" /> : <Icon className="h-4 w-4 text-muted" />}
                      </div>
                      <span className={`text-xs font-semibold leading-tight ${done ? "text-success-ink" : "text-muted"}`}>{translateUi(label)}</span>
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
