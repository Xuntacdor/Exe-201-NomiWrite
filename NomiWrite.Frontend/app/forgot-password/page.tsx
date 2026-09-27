"use client";

import { useLocale } from "@/lib/i18n/locale";


import Link from "next/link";
import { FormEvent, useState } from "react";
import { ArrowLeft, Mail, PenLine, Send } from "lucide-react";
import { apiClient } from "@/lib/api/client";

export default function ForgotPasswordPage() {
  const { t: translateUi, errorText } = useLocale();
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!email.trim()) {
      setError("Please enter your email.");
      return;
    }

    try {
      setSubmitting(true);
      const result = await apiClient.forgotPassword({ email: email.trim() });
      setMessage(result.message || "Reset instructions have been sent if the email exists.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not request password reset.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-surface-muted p-4">
      <div className="w-full max-w-sm">
        <Link href="/login" className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-muted hover:text-ink">
          <ArrowLeft className="h-4 w-4" />
          {translateUi("Back to sign in")}</Link>

        <div className="rounded-xl border border-line bg-surface p-8 shadow-sm">
          <div className="mb-7 flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent shadow-sm">
              <PenLine className="h-5 w-5 text-ink" />
            </div>
            <div>
              <h1 className="text-xl font-bold text-ink">{translateUi("Reset password")}</h1>
              <p className="text-sm text-muted">{translateUi("Use your NomiWrite account email.")}</p>
            </div>
          </div>

          <form className="space-y-4" onSubmit={handleSubmit}>
            <label className="block text-xs font-semibold text-muted">
              {translateUi("Email")}<span className="relative mt-1.5 block">
                <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
                <input
                  type="email"
                  value={email}
                  onChange={event => setEmail(event.target.value)}
                  placeholder={translateUi("student@nomiwrite.local")}
                  className="w-full rounded-xl border border-line bg-surface py-3 pl-10 pr-4 text-sm text-ink placeholder:text-muted focus:border-focus focus:outline-none"
                />
              </span>
            </label>

            {message && <p className="rounded-xl border border-focus bg-success px-3 py-2 text-xs font-medium text-success-ink">{errorText(message)}</p>}
            {error && <p className="rounded-xl border border-focus bg-danger px-3 py-2 text-xs font-medium text-danger-ink">{errorText(error)}</p>}

            <button
              type="submit"
              disabled={submitting}
              className="flex w-full items-center justify-center gap-2 rounded-xl bg-accent py-3.5 text-sm font-bold text-ink shadow-sm transition-all hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              <Send className="h-4 w-4" />
              {translateUi(submitting ? "Sending..." : "Send reset link")}
            </button>
          </form>
        </div>
      </div>
    </main>
  );
}
