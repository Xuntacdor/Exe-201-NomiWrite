"use client";

import { usePathname } from "next/navigation";
import SettingsButton from "./SettingsButton";

const appRoutes = new Set(["/dashboard", "/write", "/guide", "/study-guide", "/quiz", "/vocabulary", "/history", "/profile", "/result"]);
export default function PublicSettings() {
  const pathname = usePathname();
  if (appRoutes.has(pathname)) return null;
  return <div className="public-settings"><SettingsButton /></div>;
}
