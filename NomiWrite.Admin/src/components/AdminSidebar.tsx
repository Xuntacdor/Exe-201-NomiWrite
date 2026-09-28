import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import {
  BarChart3,
  Users,
  FileText,
  MessageSquare,
  ChevronLeft,
  ChevronRight,
  LogOut,
} from "lucide-react";
import { clearAdminSession } from "../lib/authSession";
import AdminSettingsButton from "./AdminSettingsButton";

const frontendLoginUrl = `${import.meta.env.VITE_FRONTEND_APP_URL ?? "http://localhost:3000"}/login`;

const adminNavItems = [
  { icon: BarChart3, label: "Overview", href: "/" },
  { icon: Users, label: "Users", href: "/users" },
  { icon: FileText, label: "Prompts", href: "/prompts" },
  { icon: MessageSquare, label: "Submissions", href: "/submissions" },
];

export default function AdminSidebar() {
  const [collapsed, setCollapsed] = useState(false);
  const location = useLocation();
  const activePath = location.pathname;

  const handleLogout = () => {
    clearAdminSession();
    window.location.assign(frontendLoginUrl);
  };

  return (
    <aside
      className={`admin-sidebar relative z-20 flex shrink-0 flex-col border-r transition-all duration-300 ${
        collapsed ? "w-16" : "w-64"
      }`}
    >
      <Link
        to="/"
        className={`flex h-[72px] shrink-0 items-center gap-3 border-b border-[var(--admin-line)] px-5 ${collapsed ? "justify-center px-0" : ""}`}
      >
        <img src="/nomiwrite-mark.svg" alt="" aria-hidden="true" className="h-10 w-10 shrink-0" />
        {!collapsed && (
          <div className="flex flex-col">
            <span className="text-[15px] font-extrabold text-[var(--admin-ink)] tracking-tight">NomiWrite</span>
            <span className="text-[10px] font-bold text-[var(--admin-brand)] uppercase tracking-wider">Admin Panel</span>
          </div>
        )}
      </Link>

      <button
        onClick={() => setCollapsed(!collapsed)}
        className="admin-icon-button absolute -right-3.5 top-20 z-30 flex h-7 w-7 items-center justify-center rounded-full border shadow-sm transition-colors"
      >
        {collapsed ? <ChevronRight className="h-3.5 w-3.5" /> : <ChevronLeft className="h-3.5 w-3.5" />}
      </button>

      <nav className="flex-1 space-y-1 overflow-y-auto p-3">
        <div className="mb-2 px-3 text-[10px] font-extrabold uppercase tracking-widest text-slate-400">
          {!collapsed && "Management"}
        </div>
        {adminNavItems.map((item) => {
          const Icon = item.icon;
          const isActive = activePath === item.href || (item.href !== "/" && activePath.startsWith(item.href));

          return (
            <Link
              key={item.href}
              to={item.href}
              className={`group flex items-center gap-3 rounded-xl px-3 py-2.5 transition-all ${
                isActive
                  ? "bg-pink-100 text-rose-800"
                  : "text-slate-600 hover:bg-pink-50 hover:text-slate-900"
              } ${collapsed ? "justify-center" : ""}`}
              title={collapsed ? item.label : undefined}
            >
              <Icon className={`h-5 w-5 shrink-0 ${isActive ? "text-rose-700" : "text-slate-400 group-hover:text-slate-600"}`} />
              {!collapsed && <span className="text-sm font-semibold">{item.label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="space-y-1 border-t border-[var(--admin-line)] p-3">
        <AdminSettingsButton collapsed={collapsed} />
        <button
          onClick={handleLogout}
          className={`admin-sidebar-action text-slate-600 hover:bg-red-50 hover:text-red-600 ${
            collapsed ? "justify-center" : ""
          }`}
          title={collapsed ? "Logout" : undefined}
        >
          <LogOut className="h-5 w-5 shrink-0" />
          {!collapsed && <span className="text-sm font-semibold">Logout</span>}
        </button>
      </div>
    </aside>
  );
}
