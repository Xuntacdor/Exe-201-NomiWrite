import { createContext, useContext } from "react";

export type AdminTheme = "light" | "dark";

export interface AdminPreferencesValue {
  theme: AdminTheme;
  setTheme: (theme: AdminTheme) => void;
}

export const AdminPreferencesContext = createContext<AdminPreferencesValue | null>(null);

export function useAdminPreferences() {
  const value = useContext(AdminPreferencesContext);
  if (!value) throw new Error("useAdminPreferences must be used inside AdminPreferencesProvider");
  return value;
}
