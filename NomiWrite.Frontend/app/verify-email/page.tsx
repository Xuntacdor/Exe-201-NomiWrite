"use client";

import Link from "next/link";
import { FormEvent, Suspense, useState } from "react";
import { useSearchParams } from "next/navigation";
import { ArrowLeft, Mail, PenLine, Send, ShieldCheck } from "lucide-react";
import { apiClient } from "@/lib/api/client";

function VerifyEmailContent() {
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
    <main className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 p-4">
      <div className="w-full max-w-sm">
        <Link href="/login" className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-slate-300 hover:text-white">
          <ArrowLeft className="h-4 w-4" />
          Back to sign in
        </Link>

        <div className="rounded-3xl border border-white/10 bg-white/5 p-8 shadow-2xl backdrop-blur-xl">
          <div className="mb-7 flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-600 shadow-lg shadow-blue-900/40">
              <PenLine className="h-5 w-5 text-white" />
            </div>
            <div>
              <h1 className="text-xl font-extrabold text-white">Verify email</h1>
              <p className="text-sm text-slate-400">Confirm or resend account verification.</p>
            </div>
          </div>

          <form className="space-y-4" onSubmit={handleVerify}>
            <label className="block text-xs font-semibold text-slate-300">
              Verification token
              <input
                value={token}
                onChange={event => setToken(event.target.value)}
                className="mt-1.5 w-full rounded-xl border border-white/10 bg-white/8 px-4 py-3 text-sm text-white placeholder:text-slate-500 focus:border-blue-500 focus:outline-none"
              />
            </label>

            <label className="block text-xs font-semibold text-slate-300">
              Email for resend
              <span className="relative mt-1.5 block">
                <Mail className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-500" />
                <input
                  type="email"
                  value={email}
                  onChange={event => setEmail(event.target.value)}
                  placeholder="student@nomiwrite.local"
                  className="w-full rounded-xl border border-white/10 bg-white/8 py-3 pl-10 pr-4 text-sm text-white placeholder:text-slate-500 focus:border-blue-500 focus:outline-none"
                />
              </span>
            </label>

            {message && <p className="rounded-xl border border-emerald-400/30 bg-emerald-500/10 px-3 py-2 text-xs font-medium text-emerald-200">{message}</p>}
            {error && <p className="rounded-xl border border-red-400/30 bg-red-500/10 px-3 py-2 text-xs font-medium text-red-200">{error}</p>}

            <div className="grid grid-cols-2 gap-2">
              <button
                type="submit"
                disabled={Boolean(submitting)}
                className="flex items-center justify-center gap-2 rounded-xl bg-blue-600 py-3 text-xs font-bold text-white transition-all hover:bg-blue-500 disabled:cursor-not-allowed disabled:opacity-60"
              >
                <ShieldCheck className="h-4 w-4" />
                {submitting === "verify" ? "Verifying..." : "Verify"}
              </button>
              <button
                type="button"
                onClick={handleResend}
                disabled={Boolean(submitting)}
                className="flex items-center justify-center gap-2 rounded-xl border border-white/10 bg-white/8 py-3 text-xs font-bold text-slate-200 transition-all hover:bg-white/12 disabled:cursor-not-allowed disabled:opacity-60"
              >
                <Send className="h-4 w-4" />
                {submitting === "resend" ? "Sending..." : "Resend"}
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
