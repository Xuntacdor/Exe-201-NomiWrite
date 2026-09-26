"use client";

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
import ThemeToggle from "./ThemeToggle";

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
      <Link href="/dashboard" className="sidebar-brand" aria-label="NomiWrite ? Dashboard" title="NomiWrite">
        <span className="sidebar-brand-icon"><PenLine size={22} aria-hidden="true" /></span>
        {!collapsed && <span className="sidebar-label">NomiWrite</span>}
      </Link>
      <nav className="sidebar-nav" aria-label="?i?u h??ng h?c t?p">
        {navItems.map(({ icon: Icon, label, href }, index) => (
          <div key={href}>
            {!collapsed && (index === 0 || index === 1 || index === 6 || index === 7) && (
              <p className="sidebar-group">{index === 0 ? "T?NG QUAN" : index === 1 ? "H?C T?P" : index === 6 ? "THEO D?I" : "C? NH?N"}</p>
            )}
            <Link href={href} className="sidebar-action" aria-current={activePath === href ? "page" : undefined} aria-label={label} title={label}>
              <Icon size={20} aria-hidden="true" />
              {!collapsed && <span className="sidebar-label">{label}</span>}
            </Link>
          </div>
        ))}
      </nav>
      <div className="sidebar-footer">
        {!collapsed && (
          <div className="sidebar-upgrade">
            <span className="text-sm font-bold">Free Plan</span>
            <p className="mb-3 text-xs text-muted">Upgrade for detailed AI grading.</p>
            <Link href="/upgrade" className="block rounded-lg bg-accent px-3 py-2 text-center text-sm font-bold text-ink">Go Premium</Link>
          </div>
        )}
        <ThemeToggle collapsed={collapsed} />
        <button type="button" onClick={handleLogout} className="sidebar-action" aria-label="Logout" title="Logout">
          <LogOut size={20} aria-hidden="true" />
          {!collapsed && <span className="sidebar-label">Logout</span>}
        </button>
      </div>
      <button type="button" onClick={() => setCollapsed(value => !value)} className="sidebar-collapse" aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"} aria-expanded={!collapsed}>
        {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
      </button>
    </aside>
  );
}
