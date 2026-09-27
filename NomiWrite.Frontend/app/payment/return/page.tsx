"use client";

import { useLocale } from "@/lib/i18n/locale";


import Link from "next/link";
import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { ArrowLeft, Check, CheckCircle2, CreditCard, PenLine, ShieldCheck, Wallet, XCircle } from "lucide-react";

function PaymentReturnContent() {
  const { t: translateUi, locale } = useLocale();
  const params = useSearchParams();

  const responseCode = params.get("vnp_ResponseCode") ?? params.get("resultCode") ?? "";
  const transactionStatus = params.get("vnp_TransactionStatus") ?? params.get("status") ?? "";
  const orderReference = params.get("vnp_TxnRef") ?? params.get("orderId") ?? params.get("orderInfo") ?? "";
  const amount = params.get("vnp_Amount") ?? params.get("amount") ?? "";
  const provider = params.has("vnp_ResponseCode") ? "vnpay" : params.has("resultCode") ? "momo" : "unknown";

  const success =
    responseCode === "00" ||
    transactionStatus === "00" ||
    responseCode === "0";

  const providerLabel = provider === "vnpay" ? "VNPay" : provider === "momo" ? "MoMo" : "payment provider";
  const ProviderIcon = provider === "momo" ? Wallet : provider === "vnpay" ? CreditCard : CreditCard;
  const displayAmount = amount ? `${(Number(amount) / 100).toLocaleString(locale)} VND` : "";

  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden bg-canvas px-4 py-10">

      <div className="relative w-full max-w-md">
        <Link
          href="/upgrade"
          className="mb-6 inline-flex items-center gap-2 text-sm font-bold text-muted transition-colors hover:text-accent-ink"
        >
          <ArrowLeft className="h-4 w-4" />
          {translateUi("Back to upgrade")}</Link>

        <div className="overflow-hidden rounded-xl border border-line bg-surface shadow-sm">
          <div
            className={`flex items-center gap-4 px-8 py-7 text-ink ${
              success
                ? " bg-success "
                : " bg-accent "
            }`}
          >
            <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-xl bg-surface">
              {success ? (
                <CheckCircle2 className="h-7 w-7 text-ink" />
              ) : (
                <XCircle className="h-7 w-7 text-ink" />
              )}
            </div>
            <div className="min-w-0">
              <h1 className="text-xl font-bold leading-tight tracking-tight">
                {translateUi(success ? "Payment successful" : "Payment not completed")}
              </h1>
              <p className="mt-0.5 text-sm font-semibold text-ink">{translateUi(providerLabel)} {translateUi(" return status")}</p>
            </div>
          </div>

          <div className="px-8 py-7">
            {displayAmount && (
              <div className="mb-6 text-center">
                <p className="text-xs font-bold uppercase tracking-[0.18em] text-muted">{translateUi("Total")}</p>
                <p className="mt-1 text-4xl font-bold tracking-tight text-ink">{translateUi(displayAmount)}</p>
              </div>
            )}

            <div className="divide-y divide-line rounded-xl bg-canvas px-5 py-2">
              <div className="flex items-center justify-between py-3">
                <span className="text-sm font-semibold text-muted">{translateUi("Provider")}</span>
                <span className="flex items-center gap-1.5 text-sm font-bold text-ink">
                  <ProviderIcon className="h-4 w-4 text-rose-ink" />
                  {translateUi(providerLabel)}
                </span>
              </div>
              {orderReference && (
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm font-semibold text-muted">{translateUi("Order reference")}</span>
                  <span className="max-w-[55%] truncate text-sm font-bold text-ink">{translateUi(orderReference)}</span>
                </div>
              )}
              {responseCode && (
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm font-semibold text-muted">{translateUi("Response code")}</span>
                  <span className="text-sm font-bold text-ink">{translateUi(responseCode)}</span>
                </div>
              )}
            </div>

            <p
              className={`mt-6 flex items-start gap-3 rounded-xl px-4 py-3 text-xs font-semibold leading-relaxed ${
                success ? "bg-success text-success-ink" : "bg-accent text-accent-ink"
              }`}
            >
              <ShieldCheck className="mt-0.5 h-4 w-4 shrink-0" />
              {translateUi(success
                ? "Your plan unlock may take a moment to update. Check your subscription status on the dashboard."
                : "If you believe this is an error, check your payment history or try again.")}
            </p>

            <div className="mt-7 grid grid-cols-2 gap-3">
              <Link
                href="/upgrade"
                className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface py-3.5 text-sm font-bold text-ink transition-all hover:border-line hover:bg-accent-hover hover:text-accent-ink"
              >
                <ProviderIcon className="h-4 w-4" />
                {translateUi("Upgrade")}</Link>
              <Link
                href="/dashboard"
                className="flex items-center justify-center gap-2 rounded-xl bg-accent py-3.5 text-sm font-bold text-ink shadow-sm transition-all  hover:bg-accent-hover"
              >
                <PenLine className="h-4 w-4" />
                {translateUi("Dashboard")}</Link>
            </div>

            <p className="mt-6 flex items-center justify-center gap-1.5 text-center text-xs font-semibold text-muted">
              <Check className="h-3.5 w-3.5 text-success-ink" />
              {translateUi("Secured by ")}{translateUi(providerLabel)}
            </p>
          </div>
        </div>
      </div>
    </main>
  );
}

export default function PaymentReturnPage() {
  return (
    <Suspense>
      <PaymentReturnContent />
    </Suspense>
  );
}
