"use client";

import { createContext, useContext, useEffect, useSyncExternalStore, type ReactNode } from "react";
import messages from "./messages.json";

export type Locale = "en" | "vi";
const storageKey = "nomiwrite_locale";
const eventName = "nomiwrite-locale-change";
let memoryLocale: Locale | null = null;
const LocaleContext = createContext<Locale>("vi");
const catalog: Record<string, { en: string; vi: string }> = messages;

function snapshot(): Locale {
  if (memoryLocale) return memoryLocale;
  try { return localStorage.getItem(storageKey) === "en" ? "en" : "vi"; } catch { return "vi"; }
}
function subscribe(update: () => void) {
  const storage = (event: StorageEvent) => {
    if (event.key === storageKey || event.key === null) { memoryLocale = null; update(); }
  };
  window.addEventListener("storage", storage);
  window.addEventListener(eventName, update);
  return () => { window.removeEventListener("storage", storage); window.removeEventListener(eventName, update); };
}
export function setLocale(locale: Locale) {
  memoryLocale = locale;
  try { localStorage.setItem(storageKey, locale); } catch {}
  window.dispatchEvent(new Event(eventName));
}
export function translate<T>(value: T, locale: Locale, values?: Record<string, string | number>): T | string {
  if (typeof value !== "string") return value;
  const trimmed = value.trim();
  const entry = Object.hasOwn(catalog, trimmed) ? catalog[trimmed] : undefined;
  const result = entry ? value.replace(trimmed, entry[locale]) : value;
  return values ? result.replace(/\{(\w+)\}/g, (token, key) => String(values[key] ?? token)) : result;
}
export function LocaleProvider({ children }: { children: ReactNode }) {
  const locale = useSyncExternalStore(subscribe, snapshot, () => "vi" as Locale);
  useEffect(() => {
    document.documentElement.lang = locale;
    document.title = locale === "vi" ? "NomiWrite - Luyện viết tiếng Anh" : "NomiWrite - English writing practice";
  }, [locale]);
  return <LocaleContext.Provider value={locale}>{children}</LocaleContext.Provider>;
}
export function useLocale() {
  const locale = useContext(LocaleContext);
  // Only UI copy is passed here. Exercise data and user-authored text stay untouched.
  const t = <T,>(value: T, values?: Record<string, string | number>): T | string => translate(value, locale, values);
  const errorText = (message: string) => translate(Object.hasOwn(catalog, message.trim()) ? message : "Something went wrong. Please try again.", locale);
  return { locale, t, errorText, setLocale };
}
