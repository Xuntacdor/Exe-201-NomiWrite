"use client";

import { useState } from "react";
import {
  PenLine,
  LayoutDashboard,
  BookOpen,
  BrainCircuit,
  Clock,
  User,
  GraduationCap,
  ChevronLeft,
  ChevronRight,
  LogOut,
  Sparkles,
} from "lucide-react";

const navItems = [
  { icon: LayoutDashboard, label: "Dashboard",   href: "/dashboard" },
  { icon: PenLine,         label: "Viết bài",    href: "/write" },
  { icon: GraduationCap,   label: "Hướng dẫn",   href: "/guide" },
  { icon: BrainCircuit,    label: "Quiz",         href: "/quiz" },
  { icon: BookOpen,        label: "Từ vựng",     href: "/vocabulary" },
  { icon: Clock,           label: "Lịch sử",     href: "/history" },
  { icon: User,            label: "Hồ sơ",       href: "/profile" },
];

interface AppSidebarProps {
  activePath?: string;
}

export default function AppSidebar({ activePath = "/dashboard" }: AppSidebarProps) {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <aside
      className={`relative flex flex-col bg-slate-900 border-r border-slate-800 transition-all duration-300 shrink-0 ${
        collapsed ? "w-16" : "w-56"
      }`}
    >
      {/* Logo */}
      <div className={`flex items-center gap-2.5 px-4 h-16 border-b border-slate-800 shrink-0 ${collapsed ? "justify-center px-0" : ""}`}>
        <div className="w-8 h-8 rounded-lg bg-blue-600 flex items-center justify-center shrink-0">
          <PenLine className="w-4 h-4 text-white" strokeWidth={2.5} />
        </div>
        {!collapsed && (
          <span className="text-base font-bold text-white">NomiWrite</span>
        )}
      </div>

      {/* Nav */}
      <nav className="flex-1 py-4 space-y-1 px-2 overflow-hidden">
        {navItems.map(({ icon: Icon, label, href }) => {
          const active = activePath === href;
          return (
            <a
              key={href}
              href={href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium transition-all group ${
                active
                  ? "bg-blue-600 text-white shadow-lg shadow-blue-900/30"
                  : "text-slate-400 hover:text-white hover:bg-slate-800"
              } ${collapsed ? "justify-center px-0" : ""}`}
              title={collapsed ? label : undefined}
            >
              <Icon className="w-4.5 h-4.5 shrink-0" style={{ width: 18, height: 18 }} />
              {!collapsed && <span>{label}</span>}
            </a>
          );
        })}
      </nav>

      {/* Bottom: plan badge + logout */}
      <div className="px-2 pb-4 space-y-2 border-t border-slate-800 pt-3">
        {!collapsed && (
          <div className="mx-1 p-2.5 rounded-xl bg-gradient-to-r from-blue-600/20 to-violet-600/20 border border-blue-500/20">
            <div className="flex items-center gap-1.5 mb-1">
              <Sparkles className="w-3.5 h-3.5 text-blue-400" />
              <span className="text-xs font-bold text-blue-300">Gói miễn phí</span>
            </div>
            <p className="text-[11px] text-slate-400">3/5 bài viết tháng này</p>
            <div className="mt-1.5 h-1 bg-slate-700 rounded-full overflow-hidden">
              <div className="h-full w-3/5 bg-blue-500 rounded-full" />
            </div>
            <a href="/upgrade" className="block mt-2 text-[11px] font-semibold text-blue-400 hover:text-blue-300">
              Nâng cấp Premium →
            </a>
          </div>
        )}

        <a
          href="/login"
          className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-slate-500 hover:text-slate-300 hover:bg-slate-800 transition-all ${
            collapsed ? "justify-center px-0" : ""
          }`}
          title={collapsed ? "Đăng xuất" : undefined}
        >
          <LogOut style={{ width: 18, height: 18 }} className="shrink-0" />
          {!collapsed && <span>Đăng xuất</span>}
        </a>
      </div>

      {/* Collapse toggle */}
      <button
        onClick={() => setCollapsed(!collapsed)}
        className="absolute -right-3 top-20 w-6 h-6 rounded-full bg-slate-700 border border-slate-600 flex items-center justify-center text-slate-400 hover:text-white hover:bg-slate-600 transition-all z-10 shadow-sm"
      >
        {collapsed ? <ChevronRight className="w-3 h-3" /> : <ChevronLeft className="w-3 h-3" />}
      </button>
    </aside>
  );
}
