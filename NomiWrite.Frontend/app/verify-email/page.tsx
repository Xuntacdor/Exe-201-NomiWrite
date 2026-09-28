"use client";

import { useLocale } from "@/lib/i18n/locale";


import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ArrowLeft, Mail, Send, ShieldCheck } from "lucide-react";
import BrandMark from "../components/BrandMark";
import { apiClient } from "@/lib/api/client";

function VerifyEmailContent() {
  const { t: translateUi, errorText } = useLocale();
  const params = useSearchParams();
  const [token, setToken] = useState(params.get("token") ?? "");
  const [email, setEmail] = useState(params.get("email") ?? "");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState<"verify" | "resend" | "">("");

  async function handleVerify(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!token.trim()) {
      setError("Verification token is required.");
      return;
    }

    try {
      setSubmitting("verify");
      const result = await apiClient.verifyEmail({ token: token.trim() });
      setMessage(result.message || "Email verified.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not verify email.");
    } finally {
      setSubmitting("");
    }
  }

  async function handleResend() {
    setError("");
    setMessage("");

    if (!email.trim()) {
      setError("Enter your email before resending verification.");
      return;
    }

    try {
      setSubmitting("resend");
      const result = await apiClient.resendVerificationEmail({ email: email.trim() });
      setMessage(result.message || "Verification email resent.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not resend verification email.");
    } finally {
      setSubmitting("");
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
            <BrandMark size={40} />
            <div>
              <h1 className="text-xl font-bold text-ink">{translateUi("Verify email")}</h1>
              <p className="text-sm text-muted">{translateUi("Confirm or resend account verification.")}</p>
            </div>
          </div>

          <form className="space-y-4" onSubmit={handleVerify}>
            <label className="block text-xs font-semibold text-muted">
              {translateUi("Verification token")}<input
                value={token}
                onChange={event => setToken(event.target.value)}
                className="mt-1.5 w-full rounded-xl border border-line bg-surface px-4 py-3 text-sm text-ink placeholder:text-muted focus:border-focus focus:outline-none"
              />
            </label>

            <label className="block text-xs font-semibold text-muted">
              {translateUi("Email for resend")}<span className="relative mt-1.5 block">
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

            <div className="grid grid-cols-2 gap-2">
              <button
                type="submit"
                disabled={Boolean(submitting)}
                className="flex items-center justify-center gap-2 rounded-xl bg-accent py-3 text-xs font-bold text-ink transition-all hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-60"
              >
                <ShieldCheck className="h-4 w-4" />
                {translateUi(submitting === "verify" ? "Verifying..." : "Verify")}
              </button>
              <button
                type="button"
                onClick={handleResend}
                disabled={Boolean(submitting)}
                className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface py-3 text-xs font-bold text-muted transition-all hover:bg-surface disabled:cursor-not-allowed disabled:opacity-60"
              >
                <Send className="h-4 w-4" />
                {translateUi(submitting === "resend" ? "Sending..." : "Resend")}
              </button>
            </div>
          </form>
        </div>
      </div>
    </main>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense>
      <VerifyEmailContent />
    </Suspense>
  );
}
