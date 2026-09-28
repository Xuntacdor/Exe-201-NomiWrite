"use client";

import { useLocale } from "@/lib/i18n/locale";


import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { Mail, Lock, ArrowRight } from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { redirectAfterAuth, saveSession } from "@/lib/auth/session";
import GoogleSignInButton from "../components/GoogleSignInButton";
import BrandMark from "../components/BrandMark";

export default function LoginPage() {
  const { t: translateUi, errorText } = useLocale();
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");

    if (!email.trim() || !password) {
      setError("Please enter your email and password.");
      return;
    }

    try {
      setSubmitting(true);
      const session = await apiClient.login({ email: email.trim(), password });
      saveSession(session);
      redirectAfterAuth(session, router);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Login failed. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-canvas flex items-center justify-center p-4 hero-gradient">
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
      </div>

      <div className="relative w-full max-w-sm">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2.5">
            <BrandMark size={40} />
            <span className="text-xl font-bold text-ink">{translateUi("NomiWrite")}</span>
          </Link>
          <p className="mt-3 text-sm text-muted font-medium">
            {translateUi("Continue your writing practice")}</p>
        </div>

        <div className="bg-surface border border-line rounded-xl p-8 shadow-sm">
          <GoogleSignInButton mode="login" />

          <div className="flex items-center gap-3 mb-6">
            <div className="flex-1 h-px bg-surface-muted" />
            <span className="text-xs text-muted font-semibold uppercase tracking-wider">{translateUi("or")}</span>
            <div className="flex-1 h-px bg-surface-muted" />
          </div>

          <form className="space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="block text-xs font-bold text-ink mb-1.5">
                {translateUi("Email")}</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
                <input
                  type="email"
                  value={email}
                  onChange={event => setEmail(event.target.value)}
                  placeholder={translateUi("student@nomiwrite.local")}
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-canvas border border-line text-ink placeholder:text-muted text-sm focus:outline-none focus:border-focus focus:ring-2 focus:ring-focus focus:bg-surface transition-all"
                />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-1.5">
                <label className="text-xs font-bold text-ink">{translateUi("Password")}</label>
                <Link href="/forgot-password" className="text-xs font-bold text-accent-ink hover:text-accent-ink transition-colors">
                  {translateUi("Forgot password?")}</Link>
              </div>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
                <input
                  type="password"
                  value={password}
                  onChange={event => setPassword(event.target.value)}
                  placeholder={translateUi("At least 8 characters")}
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-canvas border border-line text-ink placeholder:text-muted text-sm focus:outline-none focus:border-focus focus:ring-2 focus:ring-focus focus:bg-surface transition-all"
                />
              </div>
            </div>

            {error && (
              <p className="rounded-xl border border-line bg-danger px-3 py-2 text-xs font-bold text-danger-ink">
                {errorText(error)}
              </p>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="flex items-center justify-center gap-2 w-full py-3.5 rounded-xl bg-accent hover:bg-accent-hover text-ink text-sm font-bold transition-all shadow-sm  mt-2 disabled:opacity-60 disabled:cursor-not-allowed disabled:translate-y-0"
            >
              {translateUi(submitting ? "Signing in..." : "Sign in")}
              <ArrowRight className="w-4 h-4" />
            </button>
          </form>
        </div>

        <p className="text-center mt-6 text-sm font-medium text-muted">
          {translateUi("New to NomiWrite?")}{" "}
          <Link href="/register" className="text-accent-ink hover:text-accent-ink font-bold transition-colors">
            {translateUi("Create a free account")}</Link>
        </p>
      </div>
    </div>
  );
}
