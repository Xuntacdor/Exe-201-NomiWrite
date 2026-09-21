"use client";

import { FormEvent, type ElementType, ReactNode, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AppDialog from "../components/AppDialog";
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
  History,
  Loader2,
  Lock,
  Percent,
  RefreshCw,
  ShieldCheck,
  Sparkles,
  Wallet,
  Zap,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { getSession } from "@/lib/auth/session";
import type { CheckoutResponse, PaymentHistoryItem, RefundRequest, SubscriptionPlan } from "@/lib/types";

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
  const [signedIn, setSignedIn] = useState(false);
  const [method, setMethod] = useState<PaymentMethod>("vnpay");
  const [billing, setBilling] = useState<"monthly" | "yearly">("monthly");
  const [agreed, setAgreed] = useState(false);
  const [loading, setLoading] = useState(false);
  const [done, setDone] = useState(false);
  const [error, setError] = useState("");
  const [plans, setPlans] = useState<SubscriptionPlan[]>([]);
  const [paymentHistory, setPaymentHistory] = useState<PaymentHistoryItem[]>([]);
  const [refundRequests, setRefundRequests] = useState<RefundRequest[]>([]);
  const [checkoutResult, setCheckoutResult] = useState<CheckoutResponse | null>(null);
  const [promoCode, setPromoCode] = useState("");
  const [promoDiscount, setPromoDiscount] = useState<number | null>(null);
  const [promoMessage, setPromoMessage] = useState("");
  const [checkingPromo, setCheckingPromo] = useState(false);
  const [checkingPaymentId, setCheckingPaymentId] = useState("");
  const [requestingRefundId, setRequestingRefundId] = useState("");
  const [refundPaymentId, setRefundPaymentId] = useState("");

  const fallbackMonthly = 199_000;
  const fallbackYearlyTotal = 1_908_000;

  useEffect(() => {
    const timer = setTimeout(() => {
      setSignedIn(Boolean(getSession()?.accessToken));
    }, 0);

    return () => clearTimeout(timer);
  }, []);

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

  useEffect(() => {
    if (!signedIn) return;

    let ignore = false;
    apiClient.listPaymentHistory()
      .then(items => {
        if (!ignore) setPaymentHistory(items.slice(0, 3));
      })
      .catch(() => {
        if (!ignore) setPaymentHistory([]);
      });

    apiClient.listRefundRequests()
      .then(items => {
        if (!ignore) setRefundRequests(items.slice(0, 3));
      })
      .catch(() => {
        if (!ignore) setRefundRequests([]);
      });

    return () => {
      ignore = true;
    };
  }, [signedIn]);

  const selectedPlan = useMemo(
    () => plans.find(plan => planMatches(plan, billing)),
    [billing, plans],
  );

  const subtotal = selectedPlan?.price ?? (billing === "monthly" ? fallbackMonthly : fallbackYearlyTotal);
  const total = promoDiscount ? Math.max(0, Math.round(subtotal * (100 - promoDiscount) / 100)) : subtotal;
  const monthlyEquivalent = billing === "monthly" ? total : Math.round(total / 12);

  async function handlePromoCheck() {
    const code = promoCode.trim();
    setPromoMessage("");
    setPromoDiscount(null);

    if (!code) {
      setPromoMessage("Enter a promo code first.");
      return;
    }

    if (!getSession()?.accessToken) {
      setPromoMessage("Sign in before validating a promo code.");
      return;
    }

    try {
      setCheckingPromo(true);
      const result = await apiClient.validatePromoCode(code);
      if (!result.valid) {
        setPromoMessage("Promo code is not valid.");
        return;
      }
      setPromoDiscount(result.discountPercent ?? 0);
      setPromoMessage(`Promo applied: ${result.discountPercent ?? 0}% off.`);
    } catch (err) {
      setPromoMessage(err instanceof Error ? err.message : "Could not validate promo code.");
    } finally {
      setCheckingPromo(false);
    }
  }

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
        promoCode: promoCode.trim() || undefined,
      });

      if (checkout.checkoutUrl) {
        window.location.assign(checkout.checkoutUrl);
        return;
      }

      setCheckoutResult(checkout);
      setDone(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not create payment. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  async function handleStatusCheck(paymentId: string) {
    setCheckingPaymentId(paymentId);
    setError("");
    try {
      const status = await apiClient.getPaymentStatus(paymentId);
      setCheckoutResult(status);
      setPaymentHistory(items => items.map(item => (
        item.id === paymentId ? { ...item, status: status.status } : item
      )));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not refresh payment status.");
    } finally {
      setCheckingPaymentId("");
    }
  }

  async function handleRefundRequest(paymentOrderId: string, reason: string) {
    if (!reason.trim()) {
      setError("Please enter a reason before creating a refund request.");
      return;
    }

    setRequestingRefundId(paymentOrderId);
    setError("");
    try {
      const refund = await apiClient.createRefundRequest(paymentOrderId, reason.trim());
      setRefundPaymentId("");
      setRefundRequests(items => [refund, ...items.filter(item => item.id !== refund.id)].slice(0, 3));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not create refund request.");
    } finally {
      setRequestingRefundId("");
    }
  }

  if (done) {
    return (
      <PageFrame signedIn={signedIn}>
        <main className={`flex min-h-screen items-center justify-center bg-slate-50 px-4 hero-gradient ${signedIn ? "" : "pt-16"}`}>
          <div className="my-16 w-full max-w-md rounded-3xl border border-slate-200 bg-white/90 p-10 text-center shadow-2xl backdrop-blur-xl animate-fade-in-up">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-2xl bg-gradient-to-br from-blue-500 to-violet-600 shadow-xl shadow-blue-500/30 animate-float">
              <BadgeCheck className="h-10 w-10 text-white" />
            </div>
            <h2 className="mb-3 text-2xl font-extrabold text-slate-900">Payment order created</h2>
            <p className="mb-8 text-sm leading-relaxed text-slate-500">
              The backend returned a pending payment without a redirect URL. Check payment status from the backend service.
            </p>
            {checkoutResult && (
              <div className="mb-6 rounded-2xl border border-slate-100 bg-slate-50 p-4 text-left">
                <div className="mb-2 flex items-center justify-between gap-3">
                  <span className="text-xs font-bold text-slate-500">Order</span>
                  <span className="truncate text-xs font-extrabold text-slate-800">{checkoutResult.orderReference}</span>
                </div>
                <div className="mb-4 flex items-center justify-between gap-3">
                  <span className="text-xs font-bold text-slate-500">Status</span>
                  <span className="rounded-full bg-blue-50 px-2.5 py-1 text-xs font-extrabold text-blue-700">{checkoutResult.status}</span>
                </div>
                <button
                  type="button"
                  onClick={() => handleStatusCheck(checkoutResult.id)}
                  disabled={checkingPaymentId === checkoutResult.id}
                  className="flex w-full items-center justify-center gap-2 rounded-xl bg-slate-900 py-2.5 text-xs font-bold text-white transition-colors hover:bg-slate-700 disabled:opacity-60"
                >
                  {checkingPaymentId === checkoutResult.id ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <RefreshCw className="h-3.5 w-3.5" />}
                  Refresh status
                </button>
              </div>
            )}
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
      <AppDialog
        open={Boolean(refundPaymentId)}
        title="Request refund"
        description="Send a refund request to the backend payment service for this payment order."
        confirmLabel="Send request"
        promptLabel="Reason"
        promptPlaceholder="Example: I chose the wrong billing cycle."
        onCancel={() => setRefundPaymentId("")}
        onConfirm={value => handleRefundRequest(refundPaymentId, value ?? "")}
      />
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

        <div className="relative overflow-hidden hero-gradient">
          <div className="absolute inset-0 pointer-events-none">
            <div className="absolute -top-32 -left-32 w-96 h-96 bg-blue-500/20 rounded-full blur-3xl animate-pulse-slow" />
            <div className="absolute top-32 -right-32 w-96 h-96 bg-violet-500/20 rounded-full blur-3xl animate-pulse-slow" style={{ animationDelay: "1s" }} />
          </div>
          <div className="relative mx-auto max-w-5xl px-4 py-16 text-center sm:px-6 animate-fade-in-up">
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-blue-200 bg-blue-50/80 px-4 py-1.5 shadow-sm backdrop-blur">
              <Zap className="h-4 w-4 fill-amber-400 text-amber-400" />
              <span className="text-xs font-extrabold tracking-wider text-blue-700 uppercase">Upgrade to Premium</span>
            </div>
            <h1 className="mb-4 text-4xl font-extrabold text-slate-900 sm:text-5xl tracking-tight">Premium checkout</h1>
            <p className="mx-auto max-w-md text-base text-slate-500">
              Securely upgrade your account using our backend Subscription and Payment services.
            </p>
          </div>
        </div>

        <div className="mx-auto max-w-5xl px-4 pb-20 sm:px-6">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-5 animate-fade-in-up" style={{ animationDelay: "0.2s" }}>
            <div className="space-y-6 lg:col-span-2">
              <div className="rounded-3xl border border-slate-200 bg-white p-5 shadow-lg">
                <p className="mb-4 text-xs font-extrabold uppercase tracking-widest text-slate-400">Billing cycle</p>
                <div className="flex gap-2 rounded-2xl bg-slate-50 p-1 border border-slate-100">
                  {(["monthly", "yearly"] as const).map(option => (
                    <button
                      key={option}
                      type="button"
                      onClick={() => setBilling(option)}
                      className={`flex-1 rounded-xl py-3 text-sm font-bold capitalize transition-all ${
                        billing === option ? "bg-white text-slate-900 shadow-sm border border-slate-200" : "text-slate-500 hover:text-slate-700"
                      }`}
                    >
                      {option} {option === "yearly" && <span className="ml-1 text-[10px] font-extrabold text-emerald-500 bg-emerald-50 px-1.5 py-0.5 rounded-full">Save 20%</span>}
                    </button>
                  ))}
                </div>
              </div>

              <div className="relative overflow-hidden rounded-3xl shadow-2xl glow-blue card-hover">
                <div className="absolute inset-0 bg-gradient-to-br from-blue-600 via-indigo-600 to-violet-700" />
                <div className="absolute top-0 right-0 p-32 bg-white/10 blur-3xl rounded-full" />
                <div className="absolute bottom-0 left-0 p-32 bg-indigo-500/20 blur-3xl rounded-full" />
                <div className="relative p-8">
                  <div className="mb-4 flex justify-between items-center">
                    <p className="text-xs font-extrabold uppercase tracking-widest text-blue-200">
                      {selectedPlan?.name ?? "Premium"}
                    </p>
                    <div className="inline-flex items-center gap-1 rounded-full bg-gradient-to-r from-amber-300 to-amber-500 px-3 py-1 text-[10px] font-extrabold text-amber-950 shadow-md">
                      <Sparkles className="h-3 w-3" />
                      Backend plan
                    </div>
                  </div>
                  <div className="mb-1 flex items-end gap-1.5">
                    <span className="text-5xl font-extrabold text-white tracking-tight">{fmt(monthlyEquivalent)}</span>
                    <span className="mb-2 text-sm font-medium text-blue-200">/ month</span>
                  </div>
                  {billing === "yearly" && (
                    <p className="mb-6 text-sm text-blue-200 font-medium bg-blue-900/30 inline-block px-3 py-1 rounded-full border border-blue-400/20">
                      Total <span className="font-bold text-white">{fmt(total)}</span> / year
                    </p>
                  )}
                  {billing === "monthly" && <div className="h-6 mb-6" />}
                  
                  <div className="mt-8 space-y-4">
                    {proFeatures.map(feature => (
                      <div key={feature} className="flex items-start gap-3">
                        <div className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-white/20 backdrop-blur-sm shadow-sm">
                          <Check className="h-3 w-3 text-white" />
                        </div>
                        <span className="text-sm font-medium leading-relaxed text-blue-50">{feature}</span>
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

              {signedIn && (
                <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
                  <div className="mb-3 flex items-center gap-2">
                    <History className="h-4 w-4 text-slate-400" />
                    <p className="text-xs font-bold uppercase tracking-wider text-slate-400">Payment history</p>
                  </div>
                  {paymentHistory.length ? (
                    <div className="space-y-2">
                      {paymentHistory.map(item => (
                        <div key={item.id} className="rounded-xl bg-slate-50 px-3 py-2">
                          <div className="flex items-center justify-between gap-3">
                          <div className="min-w-0">
                            <p className="truncate text-xs font-bold text-slate-700">{item.provider}</p>
                            <p className="text-[11px] text-slate-400">{item.status}</p>
                          </div>
                          <span className="text-xs font-extrabold text-slate-800">{fmt(item.amount)}</span>
                          </div>
                          <div className="mt-2 flex gap-2">
                            <button
                              type="button"
                              onClick={() => handleStatusCheck(item.id)}
                              disabled={checkingPaymentId === item.id}
                              className="flex flex-1 items-center justify-center gap-1.5 rounded-lg border border-slate-200 bg-white px-2 py-1.5 text-[11px] font-bold text-slate-600 transition-colors hover:border-blue-200 hover:text-blue-700 disabled:opacity-60"
                            >
                              {checkingPaymentId === item.id ? <Loader2 className="h-3 w-3 animate-spin" /> : <RefreshCw className="h-3 w-3" />}
                              Status
                            </button>
                            <button
                              type="button"
                              onClick={() => setRefundPaymentId(item.id)}
                              disabled={requestingRefundId === item.id}
                              className="flex-1 rounded-lg border border-slate-200 bg-white px-2 py-1.5 text-[11px] font-bold text-slate-600 transition-colors hover:border-amber-200 hover:text-amber-700 disabled:opacity-60"
                            >
                              {requestingRefundId === item.id ? "Sending" : "Refund"}
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs leading-relaxed text-slate-500">No payment records returned yet.</p>
                  )}
                </div>
              )}

              {signedIn && (
                <div className="rounded-2xl border border-slate-100 bg-white p-4 shadow-sm">
                  <div className="mb-3 flex items-center gap-2">
                    <RefreshCw className="h-4 w-4 text-slate-400" />
                    <p className="text-xs font-bold uppercase tracking-wider text-slate-400">Refund requests</p>
                  </div>
                  {refundRequests.length ? (
                    <div className="space-y-2">
                      {refundRequests.map(item => (
                        <div key={item.id} className="rounded-xl bg-slate-50 px-3 py-2">
                          <div className="mb-1 flex items-center justify-between gap-3">
                            <p className="truncate text-xs font-bold text-slate-700">{item.reason}</p>
                            <span className="rounded-full bg-amber-50 px-2 py-0.5 text-[11px] font-bold text-amber-700">{item.status}</span>
                          </div>
                          <p className="text-[11px] text-slate-400">{new Date(item.requestedAt).toLocaleString()}</p>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs leading-relaxed text-slate-500">No refund requests returned yet.</p>
                  )}
                </div>
              )}
            </div>

            <form onSubmit={handleSubmit} className="space-y-6 lg:col-span-3">
              <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
                <p className="mb-5 text-xs font-extrabold uppercase tracking-widest text-slate-400">Payment method</p>
                <div className="space-y-3">
                  {paymentMethods.map(({ id, label, icon: Icon, sub }) => (
                    <label
                      key={id}
                      className={`group flex cursor-pointer items-center gap-4 rounded-2xl border-2 p-4 transition-all duration-300 ${
                        method === id ? "border-blue-500 bg-blue-50/50 shadow-md shadow-blue-500/10" : "border-slate-100 bg-slate-50/50 hover:border-slate-200 hover:bg-slate-50"
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
                      <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-xl transition-colors ${method === id ? "bg-blue-100" : "bg-white border border-slate-100 shadow-sm group-hover:border-slate-200"}`}>
                        <Icon className={`h-6 w-6 ${method === id ? "text-blue-600" : "text-slate-400 group-hover:text-slate-500"}`} />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className={`text-base font-extrabold ${method === id ? "text-blue-900" : "text-slate-700 group-hover:text-slate-900"}`}>{label}</p>
                        <p className={`mt-0.5 text-xs font-medium ${method === id ? "text-blue-600/70" : "text-slate-400"}`}>{sub}</p>
                      </div>
                      <div className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full border-2 transition-all ${method === id ? "border-blue-500 bg-blue-500 scale-110" : "border-slate-300 bg-white"}`}>
                        {method === id && <div className="h-2.5 w-2.5 rounded-full bg-white" />}
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              <div className="rounded-3xl border border-slate-200 bg-white p-6 shadow-xl">
                <p className="mb-5 text-xs font-extrabold uppercase tracking-widest text-slate-400">Order summary</p>
                <div className="space-y-4">
                  <div className="flex gap-2">
                    <div className="relative flex-1">
                      <Percent className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                      <input
                        value={promoCode}
                        onChange={event => {
                          setPromoCode(event.target.value);
                          setPromoDiscount(null);
                          setPromoMessage("");
                        }}
                        placeholder="Promo code"
                        className="w-full rounded-xl border border-slate-200 bg-slate-50 py-2.5 pl-9 pr-3 text-sm font-semibold text-slate-700 placeholder:text-slate-400 focus:border-blue-400 focus:outline-none"
                      />
                    </div>
                    <button
                      type="button"
                      onClick={handlePromoCheck}
                      disabled={checkingPromo}
                      className="rounded-xl bg-slate-900 px-4 py-2.5 text-xs font-bold text-white transition-colors hover:bg-slate-700 disabled:opacity-60"
                    >
                      {checkingPromo ? "Checking" : "Apply"}
                    </button>
                  </div>
                  {promoMessage && (
                    <p className={`text-xs font-semibold ${promoDiscount !== null ? "text-emerald-600" : "text-slate-500"}`}>
                      {promoMessage}
                    </p>
                  )}
                  <div className="flex justify-between text-base">
                    <span className="font-semibold text-slate-600">{selectedPlan?.name ?? "NomiWrite Premium"} - <span className="capitalize">{billing}</span></span>
                    <span className="font-extrabold text-slate-900">{fmt(subtotal)}</span>
                  </div>
                  {promoDiscount !== null && promoDiscount > 0 && (
                    <div className="flex justify-between text-base">
                      <span className="font-semibold text-emerald-600">Promo discount</span>
                      <span className="font-extrabold text-emerald-600">-{promoDiscount}%</span>
                    </div>
                  )}
                  {/* Backend plan id removed as requested */}
                  <div className="my-2 border-t border-dashed border-slate-200" />
                  <div className="flex items-center justify-between">
                    <span className="text-base font-extrabold text-slate-900">Total to pay</span>
                    <span className="text-3xl font-extrabold text-blue-600 tracking-tight">{fmt(total)}</span>
                  </div>
                </div>
              </div>

              <label className="flex cursor-pointer items-start gap-4 rounded-2xl bg-slate-50 p-4 border border-slate-100 transition-colors hover:bg-slate-100/50">
                <button
                  type="button"
                  onClick={() => setAgreed(value => !value)}
                  className={`mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-lg border-2 transition-all ${agreed ? "border-blue-600 bg-blue-600 scale-105" : "border-slate-300 bg-white"}`}
                >
                  {agreed && <Check className="h-4 w-4 text-white" />}
                </button>
                <span className="text-sm font-medium leading-relaxed text-slate-600">
                  I agree to create a payment order through the selected backend provider securely.
                </span>
              </label>

              {error && (
                <p className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-bold text-red-700">{error}</p>
              )}

              <button
                type="submit"
                disabled={!agreed || loading}
                className={`flex w-full items-center justify-center gap-3 rounded-2xl py-5 text-base font-extrabold transition-all duration-300 ${
                  agreed && !loading
                    ? "bg-gradient-to-r from-blue-600 via-indigo-600 to-violet-600 text-white shadow-xl shadow-blue-500/30 hover:-translate-y-1 hover:shadow-2xl hover:shadow-blue-500/40"
                    : "cursor-not-allowed bg-slate-200 text-slate-400"
                }`}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-5 w-5 animate-spin" />
                    Creating payment...
                  </>
                ) : (
                  <>
                    <Lock className="h-5 w-5" />
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
