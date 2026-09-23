"use client";

import Link from "next/link";
import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { CheckCircle2, CreditCard, PenLine, Wallet, XCircle } from "lucide-react";

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
    <main className="flex min-h-screen items-center justify-center bg-gradient-to-br from-slate-900 via-blue-950 to-slate-900 p-4">
      <div className="w-full max-w-sm">
        <Link href="/upgrade" className="mb-6 inline-flex items-center gap-2 text-sm font-semibold text-slate-300 hover:text-white">
          ← Back to upgrade
        </Link>

        <div className="rounded-3xl border border-white/10 bg-white/5 p-8 shadow-2xl backdrop-blur-xl">
          <div className="mb-7 flex items-center gap-3">
            <div
              className={`flex h-12 w-12 items-center justify-center rounded-xl shadow-lg ${
                success ? "bg-emerald-600 shadow-emerald-900/40" : "bg-red-600 shadow-red-900/40"
              }`}
            >
              {success ? <CheckCircle2 className="h-6 w-6 text-white" /> : <XCircle className="h-6 w-6 text-white" />}
            </div>
            <div>
              <h1 className="text-xl font-extrabold text-white">
                {success ? "Payment successful" : "Payment not completed"}
              </h1>
              <p className="text-sm text-slate-400">{providerLabel} return status</p>
            </div>
          </div>

          {orderReference && (
            <p className="text-xs text-slate-400">
              Order reference: <span className="font-semibold text-slate-200">{orderReference}</span>
            </p>
          )}
          {displayAmount && (
            <p className="mt-1 text-xs text-slate-400">
              Amount: <span className="font-semibold text-slate-200">{displayAmount}</span>
            </p>
          )}
          {responseCode && (
            <p className="mt-1 text-xs text-slate-400">
              Response code: <span className="font-semibold text-slate-200">{responseCode}</span>
            </p>
          )}

          <p className="mt-4 rounded-xl border border-white/10 bg-white/8 px-3 py-2 text-xs font-medium text-slate-300">
            {success
              ? "Your plan unlock may take a moment to update. Check your subscription status on the dashboard."
              : "If you believe this is an error, check your payment history or try again."}
          </p>

          <div className="mt-6 grid grid-cols-2 gap-2">
            <Link
              href="/upgrade"
              className="flex items-center justify-center gap-2 rounded-xl border border-white/10 bg-white/8 py-3 text-xs font-bold text-slate-200 transition-all hover:bg-white/12"
            >
              <ProviderIcon className="h-4 w-4" />
              Upgrade
            </Link>
            <Link
              href="/dashboard"
              className="flex items-center justify-center gap-2 rounded-xl bg-blue-600 py-3 text-xs font-bold text-white transition-all hover:bg-blue-500"
            >
              <PenLine className="h-4 w-4" />
              Dashboard
            </Link>
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