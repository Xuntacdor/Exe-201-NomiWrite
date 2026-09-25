"use client";

import Link from "next/link";
import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { ArrowLeft, Check, CheckCircle2, CreditCard, PenLine, ShieldCheck, Wallet, XCircle } from "lucide-react";

function PaymentReturnContent() {
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
  const displayAmount = amount ? `${(Number(amount) / 100).toLocaleString("vi-VN")} VND` : "";

  return (
    <main className="relative flex min-h-screen items-center justify-center overflow-hidden bg-[#FFFAF6] px-4 py-10">
      <div className="pointer-events-none absolute -left-24 -top-24 h-72 w-72 rounded-full bg-[#FFD9C7]/50 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-32 -right-20 h-80 w-80 rounded-full bg-[#D3EFE3]/60 blur-3xl" />
      <div className="pointer-events-none absolute left-1/2 top-1/3 h-64 w-64 -translate-x-1/2 rounded-full bg-[#DBE5FB]/50 blur-3xl" />

      <div className="relative w-full max-w-md">
        <Link
          href="/upgrade"
          className="mb-6 inline-flex items-center gap-2 text-sm font-bold text-[#808890] transition-colors hover:text-[#FF6D3A]"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to upgrade
        </Link>

        <div className="overflow-hidden rounded-[28px] border border-white bg-white shadow-[0_24px_70px_rgba(43,50,66,0.10)]">
          <div
            className={`flex items-center gap-4 px-8 py-7 text-white ${
              success
                ? "bg-gradient-to-br from-[#37C181] to-[#2BA96D]"
                : "bg-gradient-to-br from-[#FF6D3A] to-[#E6531F]"
            }`}
          >
            <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-white/20 backdrop-blur">
              {success ? (
                <CheckCircle2 className="h-7 w-7 text-white" />
              ) : (
                <XCircle className="h-7 w-7 text-white" />
              )}
            </div>
            <div className="min-w-0">
              <h1 className="text-xl font-extrabold leading-tight tracking-tight">
                {success ? "Payment successful" : "Payment not completed"}
              </h1>
              <p className="mt-0.5 text-sm font-semibold text-white/80">{providerLabel} return status</p>
            </div>
          </div>

          <div className="px-8 py-7">
            {displayAmount && (
              <div className="mb-6 text-center">
                <p className="text-xs font-bold uppercase tracking-[0.18em] text-[#808890]">Total</p>
                <p className="mt-1 text-4xl font-extrabold tracking-tight text-[#1D1D1F]">{displayAmount}</p>
              </div>
            )}

            <div className="divide-y divide-[rgba(60,60,67,0.18)] rounded-2xl bg-[#FFFAF6] px-5 py-2">
              <div className="flex items-center justify-between py-3">
                <span className="text-sm font-semibold text-[#808890]">Provider</span>
                <span className="flex items-center gap-1.5 text-sm font-bold text-[#1D1D1F]">
                  <ProviderIcon className="h-4 w-4 text-[#565FCC]" />
                  {providerLabel}
                </span>
              </div>
              {orderReference && (
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm font-semibold text-[#808890]">Order reference</span>
                  <span className="max-w-[55%] truncate text-sm font-bold text-[#1D1D1F]">{orderReference}</span>
                </div>
              )}
              {responseCode && (
                <div className="flex items-center justify-between py-3">
                  <span className="text-sm font-semibold text-[#808890]">Response code</span>
                  <span className="text-sm font-bold text-[#1D1D1F]">{responseCode}</span>
                </div>
              )}
            </div>

            <p
              className={`mt-6 flex items-start gap-3 rounded-2xl px-4 py-3 text-xs font-semibold leading-relaxed ${
                success ? "bg-[#E7F6EE] text-[#1F9E6B]" : "bg-[#FFF0EB] text-[#C2541F]"
              }`}
            >
              <ShieldCheck className="mt-0.5 h-4 w-4 shrink-0" />
              {success
                ? "Your plan unlock may take a moment to update. Check your subscription status on the dashboard."
                : "If you believe this is an error, check your payment history or try again."}
            </p>

            <div className="mt-7 grid grid-cols-2 gap-3">
              <Link
                href="/upgrade"
                className="flex items-center justify-center gap-2 rounded-2xl border border-[rgba(60,60,67,0.18)] bg-white py-3.5 text-sm font-bold text-[#1D1D1F] transition-all hover:border-[#FF6D3A] hover:bg-[#FFF0EB] hover:text-[#E6531F]"
              >
                <ProviderIcon className="h-4 w-4" />
                Upgrade
              </Link>
              <Link
                href="/dashboard"
                className="flex items-center justify-center gap-2 rounded-2xl bg-[#FF6D3A] py-3.5 text-sm font-bold text-white shadow-lg shadow-[#FF6D3A]/30 transition-all hover:-translate-y-0.5 hover:bg-[#E6531F]"
              >
                <PenLine className="h-4 w-4" />
                Dashboard
              </Link>
            </div>

            <p className="mt-6 flex items-center justify-center gap-1.5 text-center text-xs font-semibold text-[#808890]">
              <Check className="h-3.5 w-3.5 text-[#37C181]" />
              Secured by {providerLabel}
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