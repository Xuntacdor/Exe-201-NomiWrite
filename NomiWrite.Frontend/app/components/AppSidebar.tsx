"use client";

import { useLocale } from "@/lib/i18n/locale";


import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  BookOpen,
  BrainCircuit,
  ChevronLeft,
  ChevronRight,
  Clock,
  Compass,
  GraduationCap,
  LayoutDashboard,
  LogOut,
  PenLine,
  User,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { clearSession } from "@/lib/auth/session";
import SettingsButton from "./SettingsButton";
import BrandMark from "./BrandMark";

const navItems = [
  { icon: LayoutDashboard, label: "Dashboard", href: "/dashboard" },
  { icon: PenLine, label: "Write", href: "/write" },
  { icon: GraduationCap, label: "Guide", href: "/guide" },
  { icon: Compass, label: "Study Plan", href: "/study-guide" },
  { icon: BrainCircuit, label: "Quiz", href: "/quiz" },
  { icon: BookOpen, label: "Vocabulary", href: "/vocabulary" },
  { icon: Clock, label: "History", href: "/history" },
  { icon: User, label: "Profile", href: "/profile" },
];

interface AppSidebarProps {
  activePath?: string;
}

export default function AppSidebar({ activePath = "/dashboard" }: AppSidebarProps) {
  const { t: translateUi } = useLocale();
  const [collapsed, setCollapsed] = useState(false);
  const router = useRouter();

  const handleLogout = async () => {
    try {
      await apiClient.logout();
    } catch {}
    clearSession();
    router.push("/login");
  };

  return (
    <aside className={`app-sidebar ${collapsed ? "is-collapsed" : ""}`}>
      <Link href="/dashboard" className="sidebar-brand" aria-label={translateUi("NomiWrite - Dashboard")} title={translateUi("NomiWrite")}>
        <span className="sidebar-brand-icon">
          <BrandMark size={40} />
        </span>
        {!collapsed && <span className="sidebar-label">{translateUi("NomiWrite")}</span>}
      </Link>
      <nav className="sidebar-nav" aria-label={translateUi("Điều hướng học tập")}>
        {navItems.map(({ icon: Icon, label, href }, index) => (
          <div key={href}>
            {!collapsed && (index === 0 || index === 1 || index === 6 || index === 7) && (
              <p className="sidebar-group">{translateUi(index === 0 ? "TỔNG QUAN" : index === 1 ? "HỌC TẬP" : index === 6 ? "THEO DÕI" : "CÁ NHÂN")}</p>
            )}
            <Link href={href} className="sidebar-action" aria-current={activePath === href ? "page" : undefined} aria-label={translateUi(label)} title={translateUi(label)}>
              <Icon size={20} aria-hidden="true" />
              {!collapsed && <span className="sidebar-label">{translateUi(label)}</span>}
            </Link>
          </div>
        ))}
      </nav>
      <div className="sidebar-footer">
        {!collapsed && (
          <div className="sidebar-upgrade">
            <span className="text-sm font-bold">{translateUi("Free Plan")}</span>
            <p className="mb-3 text-xs text-muted">{translateUi("Upgrade for detailed AI grading.")}</p>
            <Link href="/upgrade" className="block rounded-lg bg-accent px-3 py-2 text-center text-sm font-bold text-ink">{translateUi("Go Premium")}</Link>
          </div>
        )}
        <SettingsButton sidebar collapsed={collapsed} />
        <button type="button" onClick={handleLogout} className="sidebar-action" aria-label={translateUi("Logout")} title={translateUi("Logout")}>
          <LogOut size={20} aria-hidden="true" />
          {!collapsed && <span className="sidebar-label">{translateUi("Logout")}</span>}
        </button>
      </div>
      <button type="button" onClick={() => setCollapsed(value => !value)} className="sidebar-collapse" aria-label={translateUi(collapsed ? "Expand sidebar" : "Collapse sidebar")} aria-expanded={!collapsed}>
        {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
      </button>
    </aside>
  );
}
