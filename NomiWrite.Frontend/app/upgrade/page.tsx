"use client";

import { useLocale } from "@/lib/i18n/locale";


import { FormEvent, type ElementType, ReactNode, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import AppDialog from "../components/AppDialog";
import AppShell from "../components/AppShell";
import Navbar from "../components/Navbar";
import Footer from "../components/Footer";
import VietQrCheckoutDialog from "./VietQrCheckoutDialog";
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
  { id: "vietqr", label: "VietQR", icon: Building2, sub: "Scan a QR code and transfer via your bank" },
  { id: "momo", label: "MoMo", icon: Wallet, sub: "Redirect to MoMo payment" },
];

function fmt(amount: number, locale: string) {
  return new Intl.NumberFormat(locale, { style: "currency", currency: "VND" }).format(amount);
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
  const { t: translateUi, errorText, locale } = useLocale();
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
  const [vietQrCheckout, setVietQrCheckout] = useState<CheckoutResponse | null>(null);
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
      setPromoMessage("Promo code applied.");
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

      if (checkout.vietQr) {
        // VietQR has no redirect: the customer scans a QR or transfers by hand,
        // then the dialog polls until SePay confirms the webhook landed.
        setVietQrCheckout(checkout);
        return;
      }

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

  function handleVietQrPaid(paid: CheckoutResponse) {
    setCheckoutResult(paid);
    setPaymentHistory(items => (
      items.length
        ? items.map(item => (item.id === paid.id ? { ...item, status: paid.status } : item))
        : items
    ));
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
        <main className={`flex min-h-screen items-center justify-center bg-canvas px-4 hero-gradient ${signedIn ? "" : "pt-16"}`}>
          <div className="my-16 w-full max-w-md rounded-xl border border-line bg-surface p-10 text-center shadow-sm">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-xl bg-accent shadow-sm">
              <BadgeCheck className="h-10 w-10 text-ink" />
            </div>
            <h2 className="mb-3 text-2xl font-bold text-ink">{translateUi("Payment order created")}</h2>
            <p className="mb-8 text-sm leading-relaxed text-muted">
              {translateUi("The backend returned a pending payment without a redirect URL. Check payment status from the backend service.")}</p>
            {checkoutResult && (
              <div className="mb-6 rounded-xl border border-line bg-canvas p-4 text-left">
                <div className="mb-2 flex items-center justify-between gap-3">
                  <span className="text-xs font-bold text-muted">{translateUi("Order")}</span>
                  <span className="truncate text-xs font-bold text-ink">{translateUi(checkoutResult.orderReference)}</span>
                </div>
                <div className="mb-4 flex items-center justify-between gap-3">
                  <span className="text-xs font-bold text-muted">{translateUi("Status")}</span>
                  <span className="rounded-full bg-accent px-2.5 py-1 text-xs font-bold text-accent-ink">{translateUi(checkoutResult.status)}</span>
                </div>
                <button
                  type="button"
                  onClick={() => handleStatusCheck(checkoutResult.id)}
                  disabled={checkingPaymentId === checkoutResult.id}
                  className="flex w-full items-center justify-center gap-2 rounded-xl bg-surface-muted py-2.5 text-xs font-bold text-ink transition-colors hover:bg-surface-muted disabled:opacity-60"
                >
                  {checkingPaymentId === checkoutResult.id ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <RefreshCw className="h-3.5 w-3.5" />}
                  {translateUi("Refresh status")}</button>
              </div>
            )}
            <Link href="/dashboard" className="flex w-full items-center justify-center gap-2 rounded-xl bg-accent py-4 text-sm font-bold text-ink shadow-sm transition-all hover:opacity-90">
              {translateUi("Go to Dashboard ")}<ChevronRight className="h-4 w-4" />
            </Link>
          </div>
        </main>
      </PageFrame>
    );
  }

  return (
    <PageFrame signedIn={signedIn}>
      <VietQrCheckoutDialog
        key={vietQrCheckout?.id ?? "vietqr-closed"}
        open={Boolean(vietQrCheckout)}
        checkout={vietQrCheckout}
        onClose={() => setVietQrCheckout(null)}
        onPaid={handleVietQrPaid}
        onFinish={() => {
          setVietQrCheckout(null);
          window.location.assign("/dashboard");
        }}
      />
      <AppDialog
        open={Boolean(refundPaymentId)}
        title={translateUi("Request refund")}
        description="Send a refund request to the backend payment service for this payment order."
        confirmLabel="Send request"
        promptLabel="Reason"
        promptPlaceholder="Example: I chose the wrong billing cycle."
        onCancel={() => setRefundPaymentId("")}
        onConfirm={value => handleRefundRequest(refundPaymentId, value ?? "")}
      />
      <main className={`min-h-screen bg-canvas ${signedIn ? "" : "pt-16"}`}>
        <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
          <div className="flex items-center gap-2">
            {signedIn ? (
              <Link href="/profile" className="inline-flex items-center gap-1.5 text-sm font-medium text-muted hover:text-ink">
                <ArrowLeft className="h-4 w-4" />
                {translateUi("Back to profile")}</Link>
            ) : (
              <Link href="/#pricing" className="inline-flex items-center gap-1.5 text-sm font-medium text-muted hover:text-ink">
                <ArrowLeft className="h-4 w-4" />
                {translateUi("Back to pricing")}</Link>
            )}
          </div>
          <span className="text-sm font-bold text-ink">{translateUi("Premium checkout")}</span>
        </div>

        <div className="relative overflow-hidden hero-gradient">
          <div className="absolute inset-0 pointer-events-none">
          </div>
          <div className="relative mx-auto max-w-5xl px-4 py-16 text-center sm:px-6">
            <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-line bg-accent px-4 py-1.5 shadow-sm">
              <Zap className="h-4 w-4 fill-warning-ink text-warning-ink" />
              <span className="text-xs font-bold tracking-wider text-accent-ink uppercase">{translateUi("Upgrade to Premium")}</span>
            </div>
            <h1 className="mb-4 text-4xl font-bold text-ink sm:text-5xl tracking-tight">{translateUi("Premium checkout")}</h1>
            <p className="mx-auto max-w-md text-base text-muted">
              {translateUi("Securely upgrade your account using our backend Subscription and Payment services.")}</p>
          </div>
        </div>

        <div className="mx-auto max-w-5xl px-4 pb-20 sm:px-6">
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-5" style={{ animationDelay: "0.2s" }}>
            <div className="space-y-6 lg:col-span-2">
              <div className="rounded-xl border border-line bg-surface p-5 shadow-sm">
                <p className="mb-4 text-xs font-bold uppercase tracking-widest text-muted">{translateUi("Billing cycle")}</p>
                <div className="flex gap-2 rounded-xl bg-canvas p-1 border border-line">
                  {(["monthly", "yearly"] as const).map(option => (
                    <button
                      key={option}
                      type="button"
                      onClick={() => setBilling(option)}
                      className={`flex-1 rounded-xl py-3 text-sm font-bold capitalize transition-all ${
                        billing === option ? "bg-surface text-ink shadow-sm border border-line" : "text-muted hover:text-ink"
                      }`}
                    >
                      {option} {option === "yearly" && <span className="ml-1 text-[10px] font-bold text-success-ink bg-success px-1.5 py-0.5 rounded-full">{translateUi("Save 20%")}</span>}
                    </button>
                  ))}
                </div>
              </div>

              <div className="relative overflow-hidden rounded-xl shadow-sm card-hover">
                <div className="absolute inset-0 bg-accent" />
                <div className="relative p-8">
                  <div className="mb-4 flex justify-between items-center">
                    <p className="text-xs font-bold uppercase tracking-widest text-accent-ink">
                      {translateUi(selectedPlan?.name ?? "Premium")}
                    </p>
                    <div className="inline-flex items-center gap-1 rounded-full bg-warning px-3 py-1 text-[10px] font-bold text-warning-ink shadow-sm">
                      <Sparkles className="h-3 w-3" />
                      {translateUi("Backend plan")}</div>
                  </div>
                  <div className="mb-1 flex items-end gap-1.5">
                    <span className="text-5xl font-bold text-ink tracking-tight">{translateUi(fmt(monthlyEquivalent, locale))}</span>
                    <span className="mb-2 text-sm font-medium text-accent-ink">{translateUi("/ month")}</span>
                  </div>
                  {billing === "yearly" && (
                    <p className="mb-6 text-sm text-accent-ink font-medium bg-accent inline-block px-3 py-1 rounded-full border border-focus">
                      {translateUi("Total ")}<span className="font-bold text-ink">{translateUi(fmt(total, locale))}</span> {translateUi(" / year")}</p>
                  )}
                  {billing === "monthly" && <div className="h-6 mb-6" />}

                  <div className="mt-8 space-y-4">
                    {proFeatures.map(feature => (
                      <div key={feature} className="flex items-start gap-3">
                        <div className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-surface shadow-sm">
                          <Check className="h-3 w-3 text-ink" />
                        </div>
                        <span className="text-sm font-medium leading-relaxed text-accent-ink">{translateUi(feature)}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </div>

              <div className="space-y-3 rounded-xl border border-line bg-surface p-4 shadow-sm">
                {[
                  { icon: ShieldCheck, text: "Payment is created by the backend Payment service" },
                  { icon: Lock, text: "No card details are collected by this frontend page" },
                  { icon: BadgeCheck, text: "VNPay, VietQR, and MoMo are the current backend providers" },
                ].map(({ icon: Icon, text }) => (
                  <div key={text} className="flex items-center gap-3">
                    <Icon className="h-4 w-4 shrink-0 text-success-ink" />
                    <span className="text-xs text-muted">{text}</span>
                  </div>
                ))}
              </div>

              {signedIn && (
                <div className="rounded-xl border border-line bg-surface p-4 shadow-sm">
                  <div className="mb-3 flex items-center gap-2">
                    <History className="h-4 w-4 text-muted" />
                    <p className="text-xs font-bold uppercase tracking-wider text-muted">{translateUi("Payment history")}</p>
                  </div>
                  {paymentHistory.length ? (
                    <div className="space-y-2">
                      {paymentHistory.map(item => (
                        <div key={item.id} className="rounded-xl bg-canvas px-3 py-2">
                          <div className="flex items-center justify-between gap-3">
                          <div className="min-w-0">
                            <p className="truncate text-xs font-bold text-ink">{translateUi(item.provider)}</p>
                            <p className="text-[11px] text-muted">{translateUi(item.status)}</p>
                          </div>
                          <span className="text-xs font-bold text-ink">{translateUi(fmt(item.amount, locale))}</span>
                          </div>
                          <div className="mt-2 flex gap-2">
                            <button
                              type="button"
                              onClick={() => handleStatusCheck(item.id)}
                              disabled={checkingPaymentId === item.id}
                              className="flex flex-1 items-center justify-center gap-1.5 rounded-lg border border-line bg-surface px-2 py-1.5 text-[11px] font-bold text-muted transition-colors hover:border-line hover:text-accent-ink disabled:opacity-60"
                            >
                              {checkingPaymentId === item.id ? <Loader2 className="h-3 w-3 animate-spin" /> : <RefreshCw className="h-3 w-3" />}
                              {translateUi("Status")}</button>
                            <button
                              type="button"
                              onClick={() => setRefundPaymentId(item.id)}
                              disabled={requestingRefundId === item.id}
                              className="flex-1 rounded-lg border border-line bg-surface px-2 py-1.5 text-[11px] font-bold text-muted transition-colors hover:border-line hover:text-warning-ink disabled:opacity-60"
                            >
                              {translateUi(requestingRefundId === item.id ? "Sending" : "Refund")}
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs leading-relaxed text-muted">{translateUi("No payment records returned yet.")}</p>
                  )}
                </div>
              )}

              {signedIn && (
                <div className="rounded-xl border border-line bg-surface p-4 shadow-sm">
                  <div className="mb-3 flex items-center gap-2">
                    <RefreshCw className="h-4 w-4 text-muted" />
                    <p className="text-xs font-bold uppercase tracking-wider text-muted">{translateUi("Refund requests")}</p>
                  </div>
                  {refundRequests.length ? (
                    <div className="space-y-2">
                      {refundRequests.map(item => (
                        <div key={item.id} className="rounded-xl bg-canvas px-3 py-2">
                          <div className="mb-1 flex items-center justify-between gap-3">
                            <p className="truncate text-xs font-bold text-ink">{translateUi(item.reason)}</p>
                            <span className="rounded-full bg-warning px-2 py-0.5 text-[11px] font-bold text-warning-ink">{translateUi(item.status)}</span>
                          </div>
                          <p className="text-[11px] text-muted">{translateUi(new Date(item.requestedAt).toLocaleString(locale))}</p>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs leading-relaxed text-muted">{translateUi("No refund requests returned yet.")}</p>
                  )}
                </div>
              )}
            </div>

            <form onSubmit={handleSubmit} className="space-y-6 lg:col-span-3">
              <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
                <p className="mb-5 text-xs font-bold uppercase tracking-widest text-muted">{translateUi("Payment method")}</p>
                <div className="space-y-3">
                  {paymentMethods.map(({ id, label, icon: Icon, sub }) => (
                    <label
                      key={id}
                      className={`group flex cursor-pointer items-center gap-4 rounded-xl border-2 p-4 transition-all duration-300 ${
                        method === id ? "border-focus bg-accent shadow-sm " : "border-line bg-canvas hover:border-line hover:bg-canvas"
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
                      <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-xl transition-colors ${method === id ? "bg-accent" : "bg-surface border border-line shadow-sm group-hover:border-line"}`}>
                        <Icon className={`h-6 w-6 ${method === id ? "text-accent-ink" : "text-muted group-hover:text-muted"}`} />
                      </div>
                      <div className="min-w-0 flex-1">
                        <p className={`text-base font-bold ${method === id ? "text-accent-ink" : "text-ink group-hover:text-ink"}`}>{translateUi(label)}</p>
                        <p className={`mt-0.5 text-xs font-medium ${method === id ? "text-accent-ink" : "text-muted"}`}>{translateUi(sub)}</p>
                      </div>
                      <div className={`flex h-6 w-6 shrink-0 items-center justify-center rounded-full border-2 transition-all ${method === id ? "border-focus bg-accent scale-110" : "border-line bg-surface"}`}>
                        {method === id && <div className="h-2.5 w-2.5 rounded-full bg-surface" />}
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              <div className="rounded-xl border border-line bg-surface p-6 shadow-sm">
                <p className="mb-5 text-xs font-bold uppercase tracking-widest text-muted">{translateUi("Order summary")}</p>
                <div className="space-y-4">
                  <div className="flex gap-2">
                    <div className="relative flex-1">
                      <Percent className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
                      <input
                        value={promoCode}
                        onChange={event => {
                          setPromoCode(event.target.value);
                          setPromoDiscount(null);
                          setPromoMessage("");
                        }}
                        placeholder={translateUi("Promo code")}
                        className="w-full rounded-xl border border-line bg-canvas py-2.5 pl-9 pr-3 text-sm font-semibold text-ink placeholder:text-muted focus:border-focus focus:outline-none"
                      />
                    </div>
                    <button
                      type="button"
                      onClick={handlePromoCheck}
                      disabled={checkingPromo}
                      className="rounded-xl bg-surface-muted px-4 py-2.5 text-xs font-bold text-ink transition-colors hover:bg-surface-muted disabled:opacity-60"
                    >
                      {translateUi(checkingPromo ? "Checking" : "Apply")}
                    </button>
                  </div>
                  {promoMessage && (
                    <p className={`text-xs font-semibold ${promoDiscount !== null ? "text-success-ink" : "text-muted"}`}>
                      {errorText(promoMessage)}
                    </p>
                  )}
                  <div className="flex justify-between text-base">
                    <span className="font-semibold text-muted">{translateUi(selectedPlan?.name ?? "NomiWrite Premium")} - <span className="capitalize">{translateUi(billing)}</span></span>
                    <span className="font-bold text-ink">{translateUi(fmt(subtotal, locale))}</span>
                  </div>
                  {promoDiscount !== null && promoDiscount > 0 && (
                    <div className="flex justify-between text-base">
                      <span className="font-semibold text-success-ink">{translateUi("Promo discount")}</span>
                      <span className="font-bold text-success-ink">-{translateUi(promoDiscount)}%</span>
                    </div>
                  )}
                  {/* Backend plan id removed as requested */}
                  <div className="my-2 border-t border-dashed border-line" />
                  <div className="flex items-center justify-between">
                    <span className="text-base font-bold text-ink">{translateUi("Total to pay")}</span>
                    <span className="text-3xl font-bold text-accent-ink tracking-tight">{translateUi(fmt(total, locale))}</span>
                  </div>
                </div>
              </div>

              <label className="flex cursor-pointer items-start gap-4 rounded-xl bg-canvas p-4 border border-line transition-colors hover:bg-surface-muted">
                <button
                  type="button"
                  onClick={() => setAgreed(value => !value)}
                  className={`mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-lg border-2 transition-all ${agreed ? "border-focus bg-accent scale-105" : "border-line bg-surface"}`}
                >
                  {agreed && <Check className="h-4 w-4 text-ink" />}
                </button>
                <span className="text-sm font-medium leading-relaxed text-muted">
                  {translateUi("I agree to create a payment order through the selected backend provider securely.")}</span>
              </label>

              {error && (
                <p className="rounded-xl border border-line bg-danger px-4 py-3 text-sm font-bold text-danger-ink">{errorText(error)}</p>
              )}

              <button
                type="submit"
                disabled={!agreed || loading}
                className={`flex w-full items-center justify-center gap-3 rounded-xl py-5 text-base font-bold transition-all duration-300 ${
                  agreed && !loading
                    ? " bg-accent   text-ink shadow-sm   hover:shadow-sm "
                    : "cursor-not-allowed bg-surface-muted text-muted"
                }`}
              >
                {loading ? (
                  <>
                    <Loader2 className="h-5 w-5 animate-spin" />
                    {translateUi("Creating payment...")}</>
                ) : (
                  <>
                    <Lock className="h-5 w-5" />
                    {translateUi("Pay with ")}{translateUi(method.toUpperCase())} - {translateUi(fmt(total, locale))}
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
