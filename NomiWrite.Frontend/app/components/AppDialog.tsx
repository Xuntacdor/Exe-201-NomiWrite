"use client";

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
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/60 p-4 backdrop-blur-sm">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-2xl"
      >
        <div className="flex items-start gap-3 border-b border-slate-100 px-5 py-4">
          <div className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-xl ${danger ? "bg-red-50 text-red-600" : "bg-blue-50 text-blue-600"}`}>
            <AlertTriangle className="h-5 w-5" />
          </div>
          <div className="min-w-0 flex-1">
            <h2 className="text-base font-extrabold text-slate-900">{title}</h2>
            {description && <p className="mt-1 text-sm leading-relaxed text-slate-500">{description}</p>}
          </div>
          <button
            type="button"
            onClick={onCancel}
            className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-700"
            aria-label="Close dialog"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="space-y-4 px-5 py-4">
          {children}
          {promptLabel && (
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-500">
              {promptLabel}
              <textarea
                value={value}
                onChange={event => setValue(event.target.value)}
                placeholder={promptPlaceholder}
                className="mt-2 min-h-28 w-full resize-none rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm font-medium normal-case tracking-normal text-slate-800 placeholder:text-slate-400 focus:border-blue-400 focus:bg-white focus:outline-none"
                autoFocus
              />
            </label>
          )}
        </div>

        <div className="flex items-center justify-end gap-2 border-t border-slate-100 bg-slate-50 px-5 py-4">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-600 transition-colors hover:bg-slate-100"
          >
            {cancelLabel}
          </button>
          <button
            type="submit"
            className={`rounded-xl px-4 py-2.5 text-sm font-bold text-white shadow-sm transition-colors ${
              danger ? "bg-red-600 hover:bg-red-700" : "bg-blue-600 hover:bg-blue-700"
            }`}
          >
            {confirmLabel}
          </button>
        </div>
      </form>
    </div>
  );
}
