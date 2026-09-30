"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  BadgeCheck,
  Building2,
  Check,
  Copy,
  Loader2,
  QrCode,
  RefreshCw,
  X,
} from "lucide-react";
import { useLocale } from "@/lib/i18n/locale";
import { apiClient } from "@/lib/api/client";
import type { CheckoutResponse, VietQrCheckout } from "@/lib/types";

/** SePay delivers bank transfers asynchronously, so the browser has to poll for the result. */
const POLL_INTERVAL_MS = 2500;
const MAX_POLL_ATTEMPTS = 120;

interface VietQrCheckoutDialogProps {
  open: boolean;
  checkout: CheckoutResponse | null;
  onClose: () => void;
  onPaid: (checkout: CheckoutResponse) => void;
  /** Invoked by the success screen's call to action. Falls back to onClose. */
  onFinish?: () => void;
}

type CopyField = "bankName" | "accountNumber" | "amount" | "transferContent";

export default function VietQrCheckoutDialog({
  open,
  checkout,
  onClose,
  onPaid,
  onFinish,
}: VietQrCheckoutDialogProps) {
  const { t: translateUi, errorText, locale } = useLocale();
  const [copied, setCopied] = useState<CopyField | null>(null);
  const [pollError, setPollError] = useState("");
  const [nowChecking, setNowChecking] = useState(false);
  const [paid, setPaid] = useState<CheckoutResponse | null>(null);
  const attempts = useRef(0);
  const paidRef = useRef(false);

  const vietQr: VietQrCheckout | undefined = checkout?.vietQr;
  // Poll by payment id first, falling back to the order reference, which is what
  // the transfer actually quotes.
  const orderId = checkout?.orderReference || checkout?.id || "";

  const confirmPayment = useCallback(
    async (orderIdToCheck: string) => {
      if (!orderIdToCheck || paidRef.current) return;

      setNowChecking(true);
      try {
        const status = await apiClient.getOrderStatus(orderIdToCheck);
        attempts.current = 0;
        setPollError("");

        if (status.status === "success") {
          paidRef.current = true;
          setPaid(status);
          onPaid(status);
        }
      } catch (err) {
        // A transient failure must not end the session: the next tick retries.
        setPollError(err instanceof Error ? err.message : errorText("Could not refresh payment status."));
      } finally {
        setNowChecking(false);
      }
    },
    [errorText, onPaid],
  );

  useEffect(() => {
    if (!open || paidRef.current) return;
    if (!orderId) return;

    const timer = setInterval(() => {
      if (paidRef.current) return;
      if (attempts.current >= MAX_POLL_ATTEMPTS) {
        clearInterval(timer);
        return;
      }
      attempts.current += 1;
      void confirmPayment(orderId);
    }, POLL_INTERVAL_MS);

    return () => clearInterval(timer);
  }, [confirmPayment, open, orderId]);

  useEffect(() => {
    if (!copied) return;
    const timer = setTimeout(() => setCopied(null), 2000);
    return () => clearTimeout(timer);
  }, [copied]);

  async function handleCopy(field: CopyField, value: string) {
    try {
      await navigator.clipboard.writeText(value);
      setCopied(field);
    } catch {
      setPollError(errorText("Could not copy to clipboard."));
    }
  }

  if (!open || !checkout || !vietQr) return null;

  const amountLabel = new Intl.NumberFormat(locale, {
    style: "currency",
    currency: checkout.currency ?? "VND",
    maximumFractionDigits: 0,
  }).format(vietQr.amount);

  // The four fields a customer has to retype or select on their banking app. The
  // order is deliberate: bank, account, then the two values that must match exactly.
  const copyFields: { field: CopyField; label: string; value: string }[] = [
    { field: "bankName", label: "Bank name", value: vietQr.bankName },
    { field: "accountNumber", label: "Account number", value: vietQr.accountNumber },
    { field: "amount", label: "Amount to transfer", value: String(vietQr.amount) },
    { field: "transferContent", label: "Transfer content", value: vietQr.transferContent },
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-black/55 p-4">
      <div
        role="dialog"
        aria-modal="true"
        aria-label={translateUi("Scan to pay with VietQR")}
        className="my-8 w-full max-w-md rounded-xl border border-line bg-surface-raised shadow-sm"
      >
        <div className="flex items-start gap-3 border-b border-line px-5 py-4">
          <div className="mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-accent text-accent-ink">
            <QrCode className="h-5 w-5" />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-base font-bold text-ink">
              {paid ? translateUi("Payment received") : translateUi("Scan to pay with VietQR")}
            </h2>
            <p className="mt-1 text-sm leading-relaxed text-muted">
              {paid
                ? translateUi("Your premium plan is active. Welcome to NomiWrite.")
                : translateUi("Open your banking app, scan the code and keep the transfer content exactly as shown.")}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-muted transition-colors hover:bg-surface-muted hover:text-ink"
            aria-label={translateUi("Close dialog")}
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        {paid ? (
          <div className="px-5 py-8 text-center">
            <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-xl bg-success">
              <BadgeCheck className="h-8 w-8 text-success-ink" />
            </div>
            <p className="mb-6 text-sm font-bold text-ink">
              {translateUi(fmtAmount(vietQr.amount, locale, checkout.currency))}
            </p>
            <button
              type="button"
              onClick={onFinish ?? onClose}
              className="w-full rounded-xl bg-accent px-4 py-3.5 text-sm font-bold text-ink shadow-sm transition-colors hover:bg-accent-hover"
            >
              {translateUi("Go to Dashboard")}
            </button>
          </div>
        ) : (
          <div className="space-y-5 px-5 py-5">
            <div className="flex flex-col items-center gap-4">
              {/* Plain <img> on purpose: the URL is assembled by the Payment service so
                  next/image would need a new remotePatterns entry, and the optimizer
                  would re-encode a QR code that banking apps must scan losslessly. */}
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={vietQr.qrImageUrl}
                width={224}
                height={224}
                alt={translateUi("VietQR payment code")}
                className="rounded-xl border border-line bg-white p-2"
              />
              <div className="text-center">
                <p className="text-xs font-bold uppercase tracking-wider text-muted">
                  {translateUi("Amount to transfer")}
                </p>
                <p className="mt-1 text-3xl font-bold tracking-tight text-ink">{translateUi(amountLabel)}</p>
              </div>
            </div>

            <div className="space-y-2 rounded-xl border border-line bg-canvas p-3">
              <CopyRow
                icon={<Building2 className="h-3.5 w-3.5" />}
                label={translateUi("Account name")}
                value={vietQr.accountName}
              />
              {copyFields
                // Bank name is an optional setting, so never offer to copy a blank.
                .filter(({ value }) => value.length > 0)
                .map(({ field, label, value }) => (
                  <CopyRow
                    key={field}
                    icon={<Copy className="h-3.5 w-3.5" />}
                    label={translateUi(label)}
                    value={value}
                    highlight={field === "transferContent"}
                    copied={copied === field}
                    onCopy={() => handleCopy(field, value)}
                  />
                ))}
            </div>

            <div className="flex items-center gap-2 rounded-xl border border-line bg-surface-muted px-3 py-2.5">
              {nowChecking ? (
                <Loader2 className="h-3.5 w-3.5 shrink-0 animate-spin text-muted" />
              ) : (
                <RefreshCw className="h-3.5 w-3.5 shrink-0 text-muted" />
              )}
              <p className="min-w-0 flex-1 truncate text-[11px] text-muted">
                {pollError
                  ? errorText(pollError)
                  : translateUi("Waiting for the bank to confirm your transfer. This page updates automatically.")}
              </p>
              <button
                type="button"
                onClick={() => void confirmPayment(orderId)}
                disabled={nowChecking}
                className="shrink-0 rounded-lg border border-line bg-surface px-2.5 py-1 text-[11px] font-bold text-muted transition-colors hover:text-ink disabled:opacity-60"
              >
                {translateUi("Refresh")}
              </button>
            </div>

            <p className="text-[11px] leading-relaxed text-muted">
              {translateUi("Keep the transfer content unchanged. If you cancel now the order stays pending and you can pay again from the same reference.")}
            </p>
          </div>
        )}
      </div>
    </div>
  );
}

function fmtAmount(amount: number, locale: string, currency?: string) {
  return new Intl.NumberFormat(locale, {
    style: "currency",
    currency: currency ?? "VND",
    maximumFractionDigits: 0,
  }).format(amount);
}

interface CopyRowProps {
  icon: React.ReactNode;
  label: string;
  value: string;
  highlight?: boolean;
  copied?: boolean;
  onCopy?: () => void;
}

function CopyRow({ icon, label, value, highlight, copied, onCopy }: CopyRowProps) {
  return (
    <div className="flex items-center gap-3 rounded-lg bg-surface px-3 py-2">
      <span className="shrink-0 text-muted">{icon}</span>
      <div className="min-w-0 flex-1">
        <p className="text-[10px] font-bold uppercase tracking-wider text-muted">{label}</p>
        <p className={`truncate text-xs font-bold text-ink ${highlight ? "font-mono" : ""}`}>{value}</p>
      </div>
      {onCopy && (
        <button
          type="button"
          onClick={onCopy}
          aria-label={`${label}: copy`}
          className="flex h-7 shrink-0 items-center justify-center rounded-lg border border-line bg-canvas px-2 text-muted transition-colors hover:text-ink"
        >
          {copied ? <Check className="h-3.5 w-3.5 text-success-ink" /> : <Copy className="h-3.5 w-3.5" />}
        </button>
      )}
    </div>
  );
}
