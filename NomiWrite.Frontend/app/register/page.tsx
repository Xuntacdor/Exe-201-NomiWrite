"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useState } from "react";
import { PenLine, Mail, Lock, User, ArrowRight, Globe2, Check } from "lucide-react";
import { apiClient, apiMode } from "@/lib/api/client";
import { saveSession } from "@/lib/auth/session";
import { levelOptions, writingGoalOptions } from "@/lib/constants/profile-options";

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
      const session = await apiClient.register({
        fullName: fullName.trim(),
        email: email.trim(),
        password,
      });
      saveSession(session);
      localStorage.setItem(
        "nomiwrite_onboarding",
        JSON.stringify({ currentLevel: selectedLevel, targetType: selectedTarget }),
      );
      router.push("/dashboard");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Registration failed. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 flex items-center justify-center p-4 py-10">
      <div className="fixed inset-0 pointer-events-none overflow-hidden">
        <div className="absolute top-1/4 -left-32 w-96 h-96 bg-blue-600/10 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 -right-32 w-96 h-96 bg-violet-600/10 rounded-full blur-3xl" />
      </div>

      <div className="relative w-full max-w-md">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center gap-2.5">
            <div className="w-10 h-10 rounded-xl bg-blue-600 flex items-center justify-center shadow-lg shadow-blue-900/40">
              <PenLine className="w-5 h-5 text-white" strokeWidth={2.5} />
            </div>
            <span className="text-xl font-extrabold text-white">NomiWrite</span>
          </Link>
          <p className="mt-3 text-sm text-slate-400">
            Create your writing practice account
          </p>
        </div>

        <div className="bg-white/5 backdrop-blur-xl border border-white/10 rounded-3xl p-8 shadow-2xl">
          <button
            type="button"
            className="w-full flex items-center justify-center gap-3 py-3 rounded-xl bg-white text-slate-700 text-sm font-semibold hover:bg-slate-50 transition-colors shadow-sm mb-6"
          >
            <Globe2 className="w-4 h-4 text-blue-500" />
            Sign up with Google
          </button>

          <div className="flex items-center gap-3 mb-6">
            <div className="flex-1 h-px bg-white/10" />
            <span className="text-xs text-slate-500 font-medium">or enter details</span>
            <div className="flex-1 h-px bg-white/10" />
          </div>

          <form className="space-y-4" onSubmit={handleSubmit}>
            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Full name</label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="text"
                  value={fullName}
                  onChange={event => setFullName(event.target.value)}
                  placeholder="Nguyen Van A"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Email</label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="email"
                  value={email}
                  onChange={event => setEmail(event.target.value)}
                  placeholder="student@nomiwrite.local"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-1.5">Password</label>
              <div className="relative">
                <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500" />
                <input
                  type="password"
                  value={password}
                  onChange={event => setPassword(event.target.value)}
                  placeholder="At least 8 characters"
                  className="w-full pl-10 pr-4 py-3 rounded-xl bg-white/8 border border-white/10 text-white placeholder-slate-500 text-sm focus:outline-none focus:border-blue-500 focus:bg-white/10 transition-all"
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-2">Current level</label>
              <div className="grid grid-cols-2 gap-2">
                {levelOptions.map(level => (
                  <button
                    key={level}
                    type="button"
                    onClick={() => setSelectedLevel(level)}
                    className={`flex items-center gap-2 p-2.5 rounded-xl border cursor-pointer transition-all text-left ${
                      selectedLevel === level
                        ? "border-blue-500 bg-blue-500/15 text-blue-300"
                        : "border-white/10 bg-white/5 text-slate-400 hover:border-white/20"
                    }`}
                  >
                    <span className={`w-3.5 h-3.5 rounded-full border-2 flex items-center justify-center shrink-0 ${
                      selectedLevel === level ? "border-blue-500 bg-blue-500" : "border-slate-600"
                    }`}>
                      {selectedLevel === level && <Check className="w-2 h-2 text-white" />}
                    </span>
                    <span className="text-xs font-medium">{level}</span>
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-slate-300 mb-2">Writing goal</label>
              <div className="flex flex-wrap gap-2">
                {writingGoalOptions.map(target => (
                  <button
                    type="button"
                    key={target}
                    onClick={() => setSelectedTarget(target)}
                    className={`px-3 py-1.5 text-xs font-semibold rounded-full border cursor-pointer transition-all ${
                      selectedTarget === target
                        ? "border-blue-500 bg-blue-500/15 text-blue-300"
                        : "border-white/10 text-slate-400 hover:border-white/20"
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
              className="flex items-start gap-3 cursor-pointer text-left"
            >
              <span className={`w-4 h-4 rounded border-2 flex items-center justify-center shrink-0 mt-0.5 ${
                acceptedTerms ? "border-blue-500 bg-blue-500/20" : "border-slate-600"
              }`}>
                {acceptedTerms && <Check className="w-2.5 h-2.5 text-blue-300" />}
              </span>
              <span className="text-xs text-slate-400 leading-relaxed">
                I agree to the terms of use and privacy policy.
              </span>
            </button>

            {error && (
              <p className="rounded-xl border border-red-400/30 bg-red-500/10 px-3 py-2 text-xs font-medium text-red-200">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="flex items-center justify-center gap-2 w-full py-3.5 rounded-xl bg-blue-600 hover:bg-blue-500 text-white text-sm font-bold transition-all shadow-lg shadow-blue-900/40 hover:-translate-y-0.5 mt-2 disabled:opacity-60 disabled:cursor-not-allowed disabled:translate-y-0"
            >
              {submitting ? "Creating account..." : "Create account"}
              <ArrowRight className="w-4 h-4" />
            </button>
          </form>

          <p className="mt-4 text-center text-[11px] text-slate-500">
            API mode: {apiMode}
          </p>
        </div>

        <p className="text-center mt-6 text-sm text-slate-500">
          Already have an account?{" "}
          <Link href="/login" className="text-blue-400 hover:text-blue-300 font-semibold transition-colors">
            Sign in
          </Link>
        </p>
      </div>
    </div>
  );
}
