"use client";

import { useLocale } from "@/lib/i18n/locale";


import { useSyncExternalStore } from "react";
import { Moon, Sun } from "lucide-react";

const storageKey = "nomiwrite_theme";
const changeEvent = "nomiwrite-theme-change";
let temporaryPreference: string | null = null;

function syncPreference() {
  let preference = temporaryPreference;
  try { preference ??= localStorage.getItem(storageKey); } catch {}
  const systemDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
  document.documentElement.dataset.theme = preference === "dark" || (preference !== "light" && systemDark) ? "dark" : "light";
}

function subscribe(onChange: () => void) {
  const media = window.matchMedia("(prefers-color-scheme: dark)");
  const update = () => { syncPreference(); onChange(); };
  const storageUpdate = (event: StorageEvent) => {
    if (event.key === storageKey || event.key === null) {
      temporaryPreference = null;
      update();
    }
  };
  media.addEventListener("change", update);
  window.addEventListener("storage", storageUpdate);
  window.addEventListener(changeEvent, update);
  update();
  return () => {
    media.removeEventListener("change", update);
    window.removeEventListener("storage", storageUpdate);
    window.removeEventListener(changeEvent, update);
  };
}

export default function ThemeToggle({ collapsed = false }: { collapsed?: boolean }) {
  const { t: translateUi } = useLocale();
  const dark = useSyncExternalStore(subscribe, () => document.documentElement.dataset.theme === "dark", () => false);
  const label = dark ? "Chuyển sang chế độ sáng" : "Chuyển sang chế độ tối";
  return (
    <button
      type="button"
      className="theme-toggle sidebar-action"
      aria-label={translateUi(label)}
      aria-pressed={dark}
      title={translateUi(label)}
      onPointerDown={event => {
        // A pointer toggle keeps the caret in the editor; keyboard focus stays accessible.
        if (event.button === 0 && document.activeElement?.matches("textarea, input")) event.preventDefault();
      }}
      onClick={() => {
        temporaryPreference = dark ? "light" : "dark";
        try { localStorage.setItem(storageKey, temporaryPreference); } catch {}
        window.dispatchEvent(new Event(changeEvent));
      }}
    >
      {dark ? <Sun size={20} aria-hidden="true" /> : <Moon size={20} aria-hidden="true" />}
      {!collapsed && <span className="sidebar-label">{translateUi(dark ? "Chế độ sáng" : "Chế độ tối")}</span>}
    </button>
  );
}
