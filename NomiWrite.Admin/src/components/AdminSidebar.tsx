import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import {
  BarChart3,
  Users,
  FileText,
  MessageSquare,
  Settings,
  ChevronLeft,
  ChevronRight,
  LogOut,
  ShieldAlert,
} from "lucide-react";
import { clearAdminSession } from "../lib/authSession";

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
      className={`relative z-20 flex shrink-0 flex-col border-r border-slate-200 bg-white transition-all duration-300 shadow-xl ${
        collapsed ? "w-16" : "w-64"
      }`}
    >
      <Link
        to="/"
        className={`flex h-[72px] shrink-0 items-center gap-3 border-b border-slate-200 px-5 ${collapsed ? "justify-center px-0" : ""}`}
      >
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-500 to-purple-600 shadow-md shadow-indigo-200">
          <ShieldAlert className="h-5 w-5 text-white" strokeWidth={2.5} />
        </div>
        {!collapsed && (
          <div className="flex flex-col">
            <span className="text-[15px] font-extrabold text-slate-900 tracking-tight">NomiWrite</span>
            <span className="text-[10px] font-bold text-indigo-600 uppercase tracking-wider">Admin Panel</span>
          </div>
        )}
      </Link>

      <button
        onClick={() => setCollapsed(!collapsed)}
        className="absolute -right-3.5 top-20 flex h-7 w-7 items-center justify-center rounded-full border border-slate-200 bg-white text-slate-500 shadow-sm hover:bg-slate-50 hover:text-slate-900 transition-colors z-30"
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
                  ? "bg-indigo-50 text-indigo-700"
                  : "text-slate-600 hover:bg-slate-50 hover:text-slate-900"
              } ${collapsed ? "justify-center" : ""}`}
              title={collapsed ? item.label : undefined}
            >
              <Icon className={`h-5 w-5 shrink-0 ${isActive ? "text-indigo-600" : "text-slate-400 group-hover:text-slate-600"}`} />
              {!collapsed && <span className="text-sm font-semibold">{item.label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-slate-200 p-3">
        <button
          onClick={handleLogout}
          className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-slate-600 hover:bg-red-50 hover:text-red-600 transition-all ${
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
