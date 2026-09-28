"use client";

import { useState } from "react";
import { CalendarDays, ChevronLeft, ChevronRight } from "lucide-react";
import { useLocale } from "@/lib/i18n/locale";
import { countWritingDays, localDayKey, monthDays } from "@/lib/writing-activity";
import type { Submission } from "@/lib/types";

export default function WritingCalendar({ submissions }: { submissions: Submission[] }) {
  const { t, locale } = useLocale();
  const [today] = useState(() => new Date());
  const [month, setMonth] = useState(() => new Date(today.getFullYear(), today.getMonth(), 1));
  const [selected, setSelected] = useState(() => localDayKey(today));
  const counts = countWritingDays(submissions);
  const days = monthDays(month.getFullYear(), month.getMonth());
  const total = days.reduce((sum, day) => sum + (day ? counts[localDayKey(day)] ?? 0 : 0), 0);
  const activeDays = days.filter(day => day && counts[localDayKey(day)]).length;
  const isCurrentMonth = month.getFullYear() === today.getFullYear() && month.getMonth() === today.getMonth();
  function moveMonth(delta: number) {
    const next = new Date(month.getFullYear(), month.getMonth() + delta, 1);
    setMonth(next);
    setSelected(localDayKey(next));
  }
  return (
    <section className="min-w-0 rounded-xl border border-line bg-surface p-5 shadow-sm" aria-label={t("Writing activity")}>
      <h3 className="flex items-center gap-2 font-bold text-ink"><CalendarDays size={18} className="text-accent-ink" />{t("Writing activity")}</h3>
      <div className="my-4 flex items-center justify-between gap-2">
        <button type="button" onClick={() => moveMonth(-1)} aria-label={t("Previous month")} className="rounded-lg p-2 text-muted hover:bg-accent focus-visible:outline-2 focus-visible:outline-focus"><ChevronLeft size={18} /></button>
        <span className="text-sm font-bold text-ink" aria-live="polite">{month.toLocaleDateString(locale, { month: "long", year: "numeric" })}</span>
        <button type="button" onClick={() => moveMonth(1)} disabled={isCurrentMonth} aria-label={t("Next month")} className="rounded-lg p-2 text-muted hover:bg-accent disabled:opacity-30 focus-visible:outline-2 focus-visible:outline-focus"><ChevronRight size={18} /></button>
      </div>
      <div className="grid grid-cols-7 gap-1 text-center">
        {Array.from({ length: 7 }, (_, i) => <span key={i} className="py-1 text-xs text-muted">{new Date(2024, 0, 1 + i).toLocaleDateString(locale, { weekday: "short" })}</span>)}
        {days.map((day, i) => {
          if (!day) return <span key={`empty-${i}`} />;
          const key = localDayKey(day);
          const count = counts[key] ?? 0;
          return <button key={key} type="button" onClick={() => setSelected(key)} aria-pressed={key === selected} aria-current={key === localDayKey(today) ? "date" : undefined}
            aria-label={`${day.toLocaleDateString(locale)}: ${t("{count} essays", { count })}`}
            className={`flex min-h-12 min-w-0 flex-col items-center justify-center rounded-lg border text-sm focus-visible:outline-2 focus-visible:outline-focus ${key === selected ? "border-focus" : "border-transparent"} ${count ? "bg-accent text-accent-ink font-bold" : "text-muted hover:bg-canvas"}`}>
            <span>{day.getDate()}</span><span className="text-[10px] leading-3">{count || "\u00a0"}</span>
          </button>;
        })}
      </div>
      <p className="mt-4 text-sm font-semibold text-ink" aria-live="polite">{new Date(`${selected}T12:00:00`).toLocaleDateString(locale, { day: "numeric", month: "short" })}: {t("{count} essays", { count: counts[selected] ?? 0 })}</p>
      <p className="mt-1 text-xs text-muted">{t("{days} active days · {count} essays this month", { days: activeDays, count: total })}</p>
      <p className="mt-2 text-xs text-muted">{t("Submitted essays only, shown in your local time.")}</p>
    </section>
  );
}
