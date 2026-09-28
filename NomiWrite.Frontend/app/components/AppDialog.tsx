"use client";

import { useLocale } from "@/lib/i18n/locale";


import { FormEvent, ReactNode, useEffect, useState } from "react";
import { AlertTriangle, X } from "lucide-react";

interface AppDialogProps {
  open: boolean;
  title: string;
  description?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  tone?: "default" | "danger";
  promptLabel?: string;
  promptPlaceholder?: string;
  initialValue?: string;
  children?: ReactNode;
  onCancel: () => void;
  onConfirm: (value?: string) => void;
}

export default function AppDialog({
  open,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  tone = "default",
  promptLabel,
  promptPlaceholder,
  initialValue = "",
  children,
  onCancel,
  onConfirm,
}: AppDialogProps) {
  const { t: translateUi } = useLocale();
  const [value, setValue] = useState(initialValue);

  useEffect(() => {
    if (!open) return;
    const timer = setTimeout(() => setValue(initialValue), 0);
    return () => clearTimeout(timer);
  }, [initialValue, open]);

  if (!open) return null;

  const danger = tone === "danger";

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    onConfirm(promptLabel ? value : undefined);
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/55 p-4">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md max-h-[90dvh] overflow-y-auto rounded-xl border border-line bg-surface-raised shadow-sm"
      >
        <div className="flex items-start gap-3 border-b border-line px-5 py-4">
          <div className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${danger ? "bg-danger text-danger-ink" : "bg-accent text-accent-ink"}`}>
            <AlertTriangle className="h-5 w-5" />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-base font-bold text-ink">{translateUi(title)}</h2>
            {description && <p className="mt-1 text-sm leading-relaxed text-muted">{translateUi(description)}</p>}
          </div>
          <button
            type="button"
            onClick={onCancel}
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-muted transition-colors hover:bg-surface-muted hover:text-ink"
            aria-label={translateUi("Close dialog")}
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="space-y-4 px-5 py-4">
          {children}
          {promptLabel && (
            <label className="block text-xs font-bold uppercase tracking-wider text-muted">
              {translateUi(promptLabel)}
              <textarea
                value={value}
                onChange={event => setValue(event.target.value)}
                placeholder={translateUi(promptPlaceholder)}
                className="mt-2 min-h-28 w-full resize-none rounded-xl border border-line bg-canvas px-3 py-2.5 text-sm font-medium normal-case tracking-normal text-ink placeholder:text-muted focus:border-focus focus:bg-surface focus:outline-none"
                autoFocus
              />
            </label>
          )}
        </div>

        <div className="flex items-center justify-end gap-2 border-t border-line bg-canvas px-5 py-4">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-xl border border-line bg-surface px-4 py-2.5 text-sm font-bold text-muted transition-colors hover:bg-surface-muted"
          >
            {translateUi(cancelLabel)}
          </button>
          <button
            type="submit"
            className={`rounded-xl px-4 py-2.5 text-sm font-bold text-ink shadow-sm transition-colors ${
              danger ? "bg-danger hover:bg-danger" : "bg-accent hover:bg-accent-hover"
            }`}
          >
            {translateUi(confirmLabel)}
          </button>
        </div>
      </form>
    </div>
  );
}
