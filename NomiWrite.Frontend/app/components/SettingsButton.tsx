"use client";

import { useRef } from "react";
import { Settings, X } from "lucide-react";
import { useLocale } from "@/lib/i18n/locale";
import ThemeToggle from "./ThemeToggle";

export default function SettingsButton({ sidebar = false, collapsed = false }: { sidebar?: boolean; collapsed?: boolean }) {
  const dialog = useRef<HTMLDialogElement>(null);
  const { locale, setLocale } = useLocale();
  const vi = locale === "vi";
  return <>
    <button type="button" className={sidebar ? "sidebar-action" : "settings-trigger"} aria-label={vi ? "Cài đặt" : "Settings"} title={vi ? "Cài đặt" : "Settings"} onClick={() => dialog.current?.showModal()}>
      <Settings size={20} aria-hidden="true" />
      {sidebar && !collapsed && <span className="sidebar-label">{vi ? "Cài đặt" : "Settings"}</span>}
    </button>
    <dialog ref={dialog} className="settings-dialog" aria-labelledby={sidebar ? "sidebar-settings-title" : "public-settings-title"} onClick={event => { if (event.target === event.currentTarget) dialog.current?.close(); }}>
      <div className="flex items-center justify-between gap-4 border-b border-line p-5">
        <h2 id={sidebar ? "sidebar-settings-title" : "public-settings-title"} className="text-lg font-bold">{vi ? "Cài đặt" : "Settings"}</h2>
        <button type="button" className="settings-trigger" aria-label={vi ? "Đóng" : "Close"} onClick={() => dialog.current?.close()}><X size={20} /></button>
      </div>
      <div className="space-y-6 p-5">
        <label className="block font-bold">
          {vi ? "Ngôn ngữ giao diện" : "Interface language"}
          <select className="mt-2 block w-full rounded-lg border p-3" value={locale} onChange={event => setLocale(event.target.value === "en" ? "en" : "vi")}>
            <option value="vi">Tiếng Việt</option><option value="en">English</option>
          </select>
        </label>
        <p className="text-sm text-muted">{vi ? "Đề bài, bài viết, câu hỏi và nội dung luyện tập luôn giữ bằng tiếng Anh." : "Prompts, essays, questions, and practice content remain in English."}</p>
        <div><h3 className="mb-2 font-bold">{vi ? "Giao diện sáng / tối" : "Light / dark appearance"}</h3><ThemeToggle /></div>
      </div>
    </dialog>
  </>;
}
