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
  GraduationCap,
  LayoutDashboard,
  LogOut,
  PenLine,
  Sparkles,
  User,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { clearSession } from "@/lib/auth/session";

const navItems = [
  { icon: LayoutDashboard, label: "Dashboard", href: "/dashboard" },
  { icon: PenLine, label: "Write", href: "/write" },
  { icon: GraduationCap, label: "Guide", href: "/guide" },
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
    <aside
      className={`relative flex shrink-0 flex-col border-r border-slate-800 bg-slate-900 transition-all duration-300 ${
        collapsed ? "w-16" : "w-56"
      }`}
    >
      <Link
        href="/dashboard"
        className={`flex h-16 shrink-0 items-center gap-2.5 border-b border-slate-800 px-4 ${collapsed ? "justify-center px-0" : ""}`}
      >
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-blue-600">
          <PenLine className="h-4 w-4 text-white" strokeWidth={2.5} />
        </div>
        {!collapsed && <span className="text-base font-bold text-white">NomiWrite</span>}
      </Link>

      <nav className="flex-1 space-y-1 overflow-hidden px-2 py-4">
        {navItems.map(({ icon: Icon, label, href }) => {
          const active = activePath === href;
          return (
            <Link
              key={href}
              href={href}
              className={`group flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all ${
                active ? "bg-blue-600 text-white shadow-lg shadow-blue-900/30" : "text-slate-400 hover:bg-slate-800 hover:text-white"
              } ${collapsed ? "justify-center px-0" : ""}`}
              title={collapsed ? label : undefined}
            >
              <Icon className="h-4.5 w-4.5 shrink-0" style={{ width: 18, height: 18 }} />
              {!collapsed && <span>{label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="space-y-2 border-t border-slate-800 px-2 pb-4 pt-3">
        {!collapsed && (
          <div className="mx-1 rounded-xl border border-blue-500/20 bg-gradient-to-r from-blue-600/20 to-violet-600/20 p-2.5">
            <div className="mb-1 flex items-center gap-1.5">
              <Sparkles className="h-3.5 w-3.5 text-blue-400" />
              <span className="text-xs font-bold text-blue-300">Free plan</span>
            </div>
            <p className="text-[11px] text-slate-400">Upgrade when you need more submissions</p>
            <Link href="/upgrade" className="mt-2 block text-[11px] font-semibold text-blue-400 hover:text-blue-300">
              Upgrade Premium
            </Link>
          </div>
        )}

        <button
          type="button"
          onClick={handleLogout}
          className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-slate-500 transition-all hover:bg-slate-800 hover:text-slate-300 ${
            collapsed ? "justify-center px-0" : ""
          }`}
          title={collapsed ? "Logout" : undefined}
        >
          <LogOut style={{ width: 18, height: 18 }} className="shrink-0" />
          {!collapsed && <span>Logout</span>}
        </button>
      </div>

      <button
        type="button"
        onClick={() => setCollapsed(value => !value)}
        className="absolute -right-3 top-20 z-10 flex h-6 w-6 items-center justify-center rounded-full border border-slate-600 bg-slate-700 text-slate-400 shadow-sm transition-all hover:bg-slate-600 hover:text-white"
        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {collapsed ? <ChevronRight className="h-3 w-3" /> : <ChevronLeft className="h-3 w-3" />}
      </button>
    </aside>
  );
}
