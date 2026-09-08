"use client";

import { FormEvent, type ElementType, ReactNode, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AppShell from "../components/AppShell";
import Navbar from "../components/Navbar";
import Footer from "../components/Footer";
import {
  ArrowLeft,
  BadgeCheck,
  Building2,
  Check,
  ChevronRight,
  CreditCard,
  Loader2,
  Lock,
  ShieldCheck,
  Sparkles,
  Wallet,
  Zap,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { SubscriptionPlan } from "@/lib/types";

type PaymentMethod = "vnpay" | "vietqr" | "momo";

const proFeatures = [
  "Unlimited writing submissions",
  "Detailed AI grading by criteria",
  "Progress tracking from saved submissions",
  "Long-term writing history",
  "Premium learning analytics when backend support is available",
];

const paymentMethods: { id: PaymentMethod; label: string; icon: ElementType; sub: string }[] = [
  { id: "vnpay", label: "VNPay", icon: CreditCard, sub: "Redirect to the VNPay gateway" },
  { id: "vietqr", label: "VietQR", icon: Building2, sub: "Create a QR payment order" },
  { id: "momo", label: "MoMo", icon: Wallet, sub: "Redirect to MoMo payment" },
];

function fmt(amount: number) {
  return `${amount.toLocaleString("vi-VN")}d`;
}

function planMatches(plan: SubscriptionPlan, billing: "monthly" | "yearly") {
  const cycle = plan.billingCycle.toLowerCase();
  const name = plan.name.toLowerCase();
  return billing === "monthly"
    ? cycle.includes("month") || name.includes("month")
    : cycle.includes("year") || name.includes("year");
}

function PageFrame({ signedIn, children }: { signedIn: boolean; children: ReactNode }) {
  if (signedIn) {
    return <AppShell activePath="/upgrade">{children}</AppShell>;
  }

  return (
    <>
      <Navbar />
      {children}
      <Footer />
    </>
  );
}

export default function UpgradePage() {
  const [signedIn] = useState(() => {
    if (typeof window === "undefined") return false;
    return Boolean(getSession()?.accessToken);
  });
  const [method, setMethod] = useState<PaymentMethod>("vnpay");
  const [billing, setBilling] = useState<"monthly" | "yearly">("monthly");
  const [agreed, setAgreed] = useState(false);
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);
  const [error, setError] = useState("");
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);

  const fallbackMonthly = 199_000;
  const fallbackYearlyTotal = 1_908_000;

  useEffect(() => {
    let ignore = false;
    apiClient.listSubscriptionPlans()
      .then(items => {
        if (!ignore) setPlans(items);
      })
      .catch(() => {
        if (!ignore) setPlans([]);
      });

    return () => {
      ignore = true;
    };
  }, []);

  const selectedPlan = useMemo(
    () => plans.find(plan => planMatches(plan, billing)),
    [billing, plans],
  );

  const total = selectedPlan?.price ?? (billing === "monthly" ? fallbackMonthly : fallbackYearlyTotal);
  const monthlyEquivalent = billing === "monthly" ? total : Math.round(total / 12);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!agreed) return;

    if (!getSession()?.accessToken) {
      window.location.href = "/login";
      return;
    }

    setLoading(true);
    setError("");

    try {
      const checkout = await apiClient.createCheckout({
        plan: "premium",
        billingCycle: billing,
        paymentMethod: method,
        amount: total,
        currency: selectedPlan?.currency ?? "VND",
        planId: selectedPlan?.id,
      });

      if (checkout.checkoutUrl) {
        window.location.assign(checkout.checkoutUrl);
        return;
      }

      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not create payment. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  if (done) {
    return (
      <PageFrame signedIn={signedIn}>
        <main className={`flex min-h-screen items-center justify-center bg-slate-50 px-4 ${signedIn ? "" : "pt-16"}`}>
          <div className="my-16 w-full max-w-md rounded-3xl border border-slate-100 bg-white p-10 text-center shadow-xl">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-violet-600 shadow-xl shadow-blue-200">
              <BadgeCheck className="h-10 w-10 text-white" />
            </div>
            <h2 className="mb-3 text-2xl font-extrabold text-slate-900">Payment order created</h2>
            <p className="mb-8 text-sm leading-relaxed text-slate-500">
              The backend returned a pending payment without a redirect URL. Check payment status from the backend service.
            </p>
            <Link href="/dashboard" className="flex w-full items-center justify-center gap-2 rounded-2xl bg-gradient-to-r from-blue-600 to-violet-600 py-4 text-sm font-extrabold text-white shadow-lg shadow-blue-200 transition-all hover:opacity-90">
              Go to Dashboard <ChevronRight className="h-4 w-4" />
            </Link>
          </div>
        </main>
      </PageFrame>
    );
  }

  return (
    <PageFrame signedIn={signedIn}>
      <main className={`min-h-screen bg-slate-50 ${signedIn ? "" : "pt-16"}`}>
        <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
          <div className="flex items-center gap-2">
            {signedIn ? (
              <Link href="/profile" className="inline-flex items-center gap-1.5 text-sm font-medium text-slate-500 hover:text-slate-800">
                <ArrowLeft className="h-4 w-4" />
                Back to profile
              </Link>
            ) : (
              <Link href="/#pricing" className="inline-flex items-center gap-1.5 text-sm font-medium text-slate-500 hover:text-slate-800">
                <ArrowLeft className="h-4 w-4" />
                Back to pricing
              </Link>
            )}
          </div>
          <span className="text-sm font-extrabold text-slate-900">Premium checkout</span>
        </div>

        <div className="relative overflow-hidden bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700">
          <div className="relative mx-auto max-w-5xl px-4 py-10 text-center sm:px-6">
            <div className="mb-4 inline-flex items-center gap-1.5 rounded-full border border-white/20 bg-white/10 px-3 py-1">
              <Zap className="h-3.5 w-3.5 fill-amber-300 text-amber-300" />
              <span className="text-xs font-bold text-white">Upgrade to Premium</span>
            </div>
            <h1 className="mb-3 text-3xl font-extrabold text-white sm:text-4xl">Premium checkout</h1>
            <p className="mx-auto max-w-md text-sm text-blue-100">
              Plans and checkout use the backend Subscription and Payment services.
            </p>
          </div>
        </div>

        <div className="mx-auto max-w-5xl px-4 py-10 sm:px-6">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-5">
            <div className="space-y-5 lg:col-span-2">
              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                <p className="mb-3 text-xs font-bold uppercase tracking-wider text-slate-400">Billing cycle</p>
                <div className="flex gap-2">
                  {(["monthly", "yearly"] as const).map(option => (
                    <button
                      key={option}
                      type="button"
                      onClick={() => setBilling(option)}
                      className={`flex-1 rounded-xl py-3 text-sm font-bold capitalize transition-all ${
                        billing === option ? "bg-blue-600 text-white shadow-md shadow-blue-200" : "bg-slate-100 text-slate-500 hover:bg-slate-200"
                      }`}
                    >
                      {option}
                    </button>
                  ))}
                </div>
              </div>

              <div className="relative overflow-hidden rounded-2xl shadow-md">
                <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-blue-700 to-violet-700" />
                <div className="relative p-6">
                  <div className="mb-3 flex justify-end">
                    <div className="inline-flex items-center gap-1 rounded-full bg-amber-400 px-2.5 py-1 text-[10px] font-extrabold text-amber-900">
                      <Sparkles className="h-3 w-3" />
                      Backend plan
                    </div>
                  </div>
                  <p className="mb-1 text-xs font-bold uppercase tracking-widest text-blue-200">
                    {selectedPlan?.name ?? "Premium"}
                  </p>
                  <div className="mb-1 flex items-end gap-1">
                    <span className="text-4xl font-extrabold text-white">{fmt(monthlyEquivalent)}</span>
                    <span className="mb-1.5 text-xs text-blue-200">/ month</span>
                  </div>
                  {billing === "yearly" && (
                    <p className="mb-4 text-xs text-blue-200">
                      Total <span className="font-bold text-white">{fmt(total)}</span> / year
                    </p>
                  )}
                  <div className="mt-4 space-y-2.5">
                    {proFeatures.map(feature => (
                      <div key={feature} className="flex items-start gap-2.5">
                        <div className="mt-0.5 flex h-4 w-4 shrink-0 items-center justify-center rounded-full bg-white/20">
                          <Check className="h-2.5 w-2.5 text-white" />
                        </div>
                        <span className="text-xs leading-relaxed text-blue-50">{feature}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              <div className="space-y-3 rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
                {[
                  { icon: ShieldCheck, text: "Payment is created by the backend Payment service" },
                  { icon: Lock, text: "No card details are collected by this frontend page" },
                  { icon: BadgeCheck, text: "VNPay, VietQR, and MoMo are the current backend providers" },
                ].map(({ icon: Icon, text }) => (
                  <div key={text} className="flex items-center gap-3">
                    <Icon className="h-4 w-4 shrink-0 text-emerald-500" />
                    <span className="text-xs text-slate-500">{text}</span>
                  </div>
                ))}
              </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5 lg:col-span-3">
              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                <p className="mb-4 text-xs font-bold uppercase tracking-wider text-slate-400">Payment method</p>
                <div className="space-y-2">
                  {paymentMethods.map(({ id, label, icon: Icon, sub }) => (
                    <label
                      key={id}
                      className={`flex cursor-pointer items-center gap-4 rounded-xl border-2 p-4 transition-all ${
                        method === id ? "border-blue-500 bg-blue-50/60" : "border-slate-100 hover:border-slate-200"
                      }`}
                    >
                      <input
                        type="radio"
                        name="method"
                        value={id}
                        checked={method === id}
                        onChange={() => setMethod(id)}
                        className="sr-only"
                      />
                      <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${method === id ? "bg-blue-100" : "bg-slate-100"}`}>
                        <Icon className={`h-5 w-5 ${method === id ? "text-blue-600" : "text-slate-400"}`} />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className={`text-sm font-bold ${method === id ? "text-blue-800" : "text-slate-700"}`}>{label}</p>
                        <p className="mt-0.5 text-xs text-slate-400">{sub}</p>
                      </div>
                      <div className={`flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 transition-all ${method === id ? "border-blue-500 bg-blue-500" : "border-slate-300"}`}>
                        {method === id && <div className="h-2 w-2 rounded-full bg-white" />}
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              <div className="rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
                <p className="mb-4 text-xs font-bold uppercase tracking-wider text-slate-400">Order summary</p>
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-slate-500">{selectedPlan?.name ?? "NomiWrite Premium"} - {billing}</span>
                    <span className="font-bold text-slate-800">{fmt(total)}</span>
                  </div>
                  {selectedPlan && (
                    <div className="flex justify-between text-xs">
                      <span className="text-slate-400">Backend plan id</span>
                      <span className="max-w-52 truncate font-semibold text-slate-600">{selectedPlan.id}</span>
                    </div>
                  )}
                  <div className="flex items-center justify-between border-t border-slate-100 pt-3">
                    <span className="text-sm font-extrabold text-slate-900">Total</span>
                    <span className="text-xl font-extrabold text-blue-600">{fmt(total)}</span>
                  </div>
                </div>
              </div>

              <label className="flex cursor-pointer items-start gap-3">
                <button
                  type="button"
                  onClick={() => setAgreed(value => !value)}
                  className={`mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-md border-2 transition-all ${agreed ? "border-blue-600 bg-blue-600" : "border-slate-300"}`}
                >
                  {agreed && <Check className="h-3 w-3 text-white" />}
                </button>
                <span className="text-xs leading-relaxed text-slate-500">
                  I agree to create a payment order through the selected backend provider.
                </span>
              </label>

              {error && (
                <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-semibold text-red-700">{error}</p>
              )}

              <button
                type="submit"
                disabled={!agreed || loading}
                className={`flex w-full items-center justify-center gap-2.5 rounded-2xl py-4 text-sm font-extrabold transition-all ${
                  agreed && !loading
                    ? "bg-gradient-to-r from-blue-600 to-violet-600 text-white shadow-xl shadow-blue-200 hover:-translate-y-0.5 hover:from-blue-700 hover:to-violet-700"
                    : "cursor-not-allowed bg-slate-200 text-slate-400"
                }`}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-4 w-4 animate-spin" />
                    Creating payment...
                  </>
                ) : (
                  <>
                    <Lock className="h-4 w-4" />
                    Pay with {method.toUpperCase()} - {fmt(total)}
                  </>
                )}
              </button>
            </form>
          </div>
        </div>
      </main>
    </PageFrame>
  );
}
