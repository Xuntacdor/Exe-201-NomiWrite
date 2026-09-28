import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { AdminPreferencesContext } from "./adminPreferencesContext";
import type { AdminTheme } from "./adminPreferencesContext";

const storageKey = "nomiwrite-admin-theme";
function initialTheme(): AdminTheme {
  const saved = window.localStorage.getItem(storageKey);
  if (saved === "light" || saved === "dark") return saved;
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

export function AdminPreferencesProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<AdminTheme>(initialTheme);

  useEffect(() => {
    document.documentElement.dataset.adminTheme = theme;
    document.documentElement.style.colorScheme = theme;
    window.localStorage.setItem(storageKey, theme);
  }, [theme]);

  return (
    <AdminPreferencesContext.Provider value={{ theme, setTheme }}>
      {children}
    </AdminPreferencesContext.Provider>
  );
}
