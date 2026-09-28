import { useRef } from "react";
import { Check, Moon, Settings, Sun, X } from "lucide-react";
import { useAdminPreferences } from "../lib/adminPreferencesContext";

export default function AdminSettingsButton({ collapsed = false, floating = false }: { collapsed?: boolean; floating?: boolean }) {
  const dialog = useRef<HTMLDialogElement>(null);
  const { theme, setTheme } = useAdminPreferences();

  return (
    <>
      <button
        type="button"
        className={floating ? "admin-settings-trigger admin-settings-floating" : `admin-sidebar-action ${collapsed ? "justify-center" : ""}`}
        onClick={() => dialog.current?.showModal()}
        aria-label="Settings"
        title="Settings"
      >
        <Settings className="h-5 w-5 shrink-0" />
        {!floating && !collapsed && <span className="text-sm font-semibold">Settings</span>}
      </button>

      <dialog
        ref={dialog}
        className="admin-settings-dialog"
        aria-labelledby="admin-settings-title"
        onClick={(event) => { if (event.target === event.currentTarget) dialog.current?.close(); }}
      >
        <div className="flex items-center justify-between border-b border-[var(--admin-line)] p-5">
          <div>
            <h2 id="admin-settings-title" className="text-lg font-extrabold">Settings</h2>
            <p className="mt-1 text-xs text-[var(--admin-muted)]">Admin appearance</p>
          </div>
          <button type="button" className="admin-icon-button" onClick={() => dialog.current?.close()} aria-label="Close settings">
            <X className="h-5 w-5" />
          </button>
        </div>
        <div className="p-5">
          <p className="mb-3 text-sm font-bold">Color mode</p>
          <div className="grid grid-cols-2 gap-3">
            {([
              { value: "light" as const, label: "Light", Icon: Sun },
              { value: "dark" as const, label: "Dark", Icon: Moon },
            ]).map(({ value, label, Icon }) => (
              <button
                key={value}
                type="button"
                className={`admin-theme-option ${theme === value ? "is-selected" : ""}`}
                onClick={() => setTheme(value)}
              >
                <Icon className="h-5 w-5" />
                <span>{label}</span>
                {theme === value && <Check className="ml-auto h-4 w-4" />}
              </button>
            ))}
          </div>
        </div>
      </dialog>
    </>
  );
}
