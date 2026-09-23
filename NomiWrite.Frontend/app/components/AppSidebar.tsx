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
  Sparkles,
  User,
} from "lucide-react";
import { apiClient } from "@/lib/api/client";
import { clearSession } from "@/lib/auth/session";

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
    <aside
      className={`relative z-20 flex shrink-0 flex-col border-r border-slate-100 bg-white transition-all duration-300 shadow-[4px_0_24px_rgba(0,0,0,0.02)] ${
        collapsed ? "w-16" : "w-60"
      }`}
    >
      <Link
        href="/dashboard"
        className={`flex h-[72px] shrink-0 items-center gap-3 border-b border-slate-100 px-5 ${collapsed ? "justify-center px-0" : ""}`}
      >
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-blue-600 to-blue-700 shadow-md shadow-blue-200">
          <PenLine className="h-5 w-5 text-white" strokeWidth={2.5} />
        </div>
        {!collapsed && <span className="text-xl font-extrabold tracking-tight text-slate-900">NomiWrite</span>}
      </Link>

      <nav className="flex-1 space-y-1.5 overflow-hidden px-3 py-6">
        {navItems.map(({ icon: Icon, label, href }) => {
          const active = activePath === href;
          return (
            <Link
              key={href}
              href={href}
              className={`group flex items-center gap-3.5 rounded-2xl px-3.5 py-3 text-[15px] font-semibold transition-all ${
                active 
                  ? "bg-blue-50 text-blue-700" 
                  : "text-slate-500 hover:bg-slate-50 hover:text-slate-900"
              } ${collapsed ? "justify-center px-0" : ""}`}
              title={collapsed ? label : undefined}
            >
              <Icon 
                className={`shrink-0 transition-colors ${active ? "text-blue-600" : "text-slate-400 group-hover:text-slate-600"}`} 
                style={{ width: 20, height: 20 }} 
                strokeWidth={active ? 2.5 : 2}
              />
              {!collapsed && <span>{label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="space-y-3 border-t border-slate-100 px-4 pb-6 pt-5">
        {!collapsed && (
          <div className="rounded-2xl bg-gradient-to-br from-slate-50 to-blue-50/50 p-4 border border-blue-100/50 relative overflow-hidden group">
            <div className="absolute -right-4 -top-4 w-20 h-20 bg-blue-500/10 rounded-full blur-2xl group-hover:bg-blue-500/20 transition-all"></div>
            <div className="relative z-10">
              <div className="mb-2 flex items-center gap-2">
                <div className="flex h-6 w-6 items-center justify-center rounded-full bg-blue-100 text-blue-600">
                  <Sparkles className="h-3 w-3" />
                </div>
                <span className="text-sm font-bold text-slate-800">Free Plan</span>
              </div>
              <p className="text-xs font-medium text-slate-500 leading-relaxed mb-3">Upgrade for detailed AI grading.</p>
              <Link 
                href="/upgrade" 
                className="flex items-center justify-center w-full rounded-xl bg-white px-3 py-2 text-xs font-bold text-blue-600 shadow-sm border border-slate-100 transition-all hover:border-blue-200 hover:shadow-md hover:-translate-y-0.5"
              >
                Go Premium
              </Link>
            </div>
          </div>
        )}

        <button
          type="button"
          onClick={handleLogout}
          className={`flex w-full items-center gap-3.5 rounded-2xl px-3.5 py-3 text-[15px] font-semibold text-slate-500 transition-all hover:bg-red-50 hover:text-red-600 ${
            collapsed ? "justify-center px-0" : ""
          }`}
          title={collapsed ? "Logout" : undefined}
        >
          <LogOut style={{ width: 20, height: 20 }} className="shrink-0 text-slate-400" />
          {!collapsed && <span>Logout</span>}
        </button>
      </div>

      <button
        type="button"
        onClick={() => setCollapsed(value => !value)}
        className="absolute -right-3.5 top-20 z-30 flex h-7 w-7 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-400 shadow-sm transition-all hover:border-blue-200 hover:bg-blue-50 hover:text-blue-600"
        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
      >
        {collapsed ? <ChevronRight className="h-3.5 w-3.5" /> : <ChevronLeft className="h-3.5 w-3.5" />}
      </button>
    </aside>
  );
}
