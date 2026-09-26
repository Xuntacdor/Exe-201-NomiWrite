"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { PenLine, Mail, Lock, User, ArrowRight, Check } from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { levelOptions, writingGoalOptions } from "@/lib/constants/profile-options";
import GoogleSignInButton from "../components/GoogleSignInButton";

export default function RegisterPage() {
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [selectedLevel, setSelectedLevel] = useState<string>(levelOptions[1]);
  const [selectedTarget, setSelectedTarget] = useState<string>(writingGoalOptions[0]);
  const [acceptedTerms, setAcceptedTerms] = useState(true);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");

    if (!fullName.trim() || !email.trim() || password.length < 8) {
      setError("Please enter your name, email, and a password with at least 8 characters.");
      return;
    }

    if (!acceptedTerms) {
      setError("Please accept the terms before creating an account.");
      return;
    }

    try {
      setSubmitting(true);
      await apiClient.register({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
      });
      localStorage.setItem(
        "nomiwrite_onboarding",
        JSON.stringify({ currentLevel: selectedLevel, targetType: selectedTarget }),
      );
      router.push("/login");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-canvas flex items-center justify-center p-4 py-10 hero-gradient">
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
      </div>

      <div className="relative w-full max-w-md">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-xl bg-accent flex items-center justify-center shadow-sm">
              <PenLine className="w-5 h-5 text-ink" strokeWidth={2.5} />
            </div>
            <span className="text-xl font-bold text-ink">NomiWrite</span>
          </Link>
          <p className="mt-3 text-sm font-medium text-muted">
            Create your writing practice account
          </p>
        </div>

        <div className="bg-surface border border-line rounded-xl p-8 shadow-sm">
          <GoogleSignInButton mode="register" />

          <div className="flex items-center gap-3 mb-6">
            <div className="flex-1 h-px bg-surface-muted" />
            <span className="text-xs text-muted font-semibold uppercase tracking-wider">or enter details</span>
            <div className="flex-1 h-px bg-surface-muted" />
          </div>

          <form className="space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="block text-xs font-bold text-ink mb-1.5">Full name</label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
                <input
                  type="text"
                  value={fullName}
                  onChange={event => setFullName(event.target.value)}
                  placeholder="Nguyen Van A"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-canvas border border-line text-ink placeholder:text-muted text-sm focus:outline-none focus:border-focus focus:ring-2 focus:ring-focus focus:bg-surface transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-ink mb-1.5">Email</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
                <input
                  type="email"
                  value={email}
                  onChange={event => setEmail(event.target.value)}
                  placeholder="student@nomiwrite.local"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-canvas border border-line text-ink placeholder:text-muted text-sm focus:outline-none focus:border-focus focus:ring-2 focus:ring-focus focus:bg-surface transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-ink mb-1.5">Password</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
                <input
                  type="password"
                  value={password}
                  onChange={event => setPassword(event.target.value)}
                  placeholder="At least 8 characters"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-canvas border border-line text-ink placeholder:text-muted text-sm focus:outline-none focus:border-focus focus:ring-2 focus:ring-focus focus:bg-surface transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-ink mb-2">Current level</label>
              <div className="grid grid-cols-2 gap-2">
                {levelOptions.map(level => (
                  <button
                    key={level}
                    type="button"
                    onClick={() => setSelectedLevel(level)}
                    className={`flex items-center gap-2 p-2.5 rounded-xl border cursor-pointer transition-all text-left ${
                      selectedLevel === level
                        ? "border-focus bg-accent text-accent-ink shadow-sm"
                        : "border-line bg-surface text-muted hover:border-line hover:bg-canvas"
                    }`}
                  >
                    <span className={`w-3.5 h-3.5 rounded-full border-2 flex items-center justify-center shrink-0 ${
                      selectedLevel === level ? "border-focus bg-accent" : "border-line"
                    }`}>
                      {selectedLevel === level && <Check className="w-2 h-2 text-ink" />}
                    </span>
                    <span className="text-xs font-bold">{level}</span>
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-ink mb-2">Writing goal</label>
              <div className="flex flex-wrap gap-2">
                {writingGoalOptions.map(target => (
                  <button
                    type="button"
                    key={target}
                    onClick={() => setSelectedTarget(target)}
                    className={`px-3 py-1.5 text-xs font-bold rounded-full border cursor-pointer transition-all ${
                      selectedTarget === target
                        ? "border-focus bg-accent text-accent-ink shadow-sm"
                        : "border-line bg-surface text-muted hover:border-line hover:bg-canvas"
                    }`}
                  >
                    {target}
                  </button>
                ))}
              </div>
            </div>

            <button
              type="button"
              onClick={() => setAcceptedTerms(value => !value)}
              className="flex items-start gap-3 cursor-pointer text-left group"
            >
              <span className={`w-4 h-4 rounded border-2 flex items-center justify-center shrink-0 mt-0.5 transition-colors ${
                acceptedTerms ? "border-focus bg-accent" : "border-line bg-surface group-hover:border-line"
              }`}>
                {acceptedTerms && <Check className="w-3 h-3 text-ink" />}
              </span>
              <span className="text-xs font-medium text-muted leading-relaxed">
                I agree to the terms of use and privacy policy.
              </span>
            </button>

            {error && (
              <p className="rounded-xl border border-line bg-danger px-3 py-2 text-xs font-bold text-danger-ink">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="flex items-center justify-center gap-2 w-full py-3.5 rounded-xl bg-accent hover:bg-accent-hover text-ink text-sm font-bold transition-all shadow-sm  mt-2 disabled:opacity-60 disabled:cursor-not-allowed disabled:translate-y-0"
            >
              {submitting ? "Creating account..." : "Create account"}
              <ArrowRight className="w-4 h-4" />
            </button>
          </form>
        </div>

        <p className="text-center mt-6 text-sm font-medium text-muted">
          Already have an account?{" "}
          <Link href="/login" className="text-accent-ink hover:text-accent-ink font-bold transition-colors">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
