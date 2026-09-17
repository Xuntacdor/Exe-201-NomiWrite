import { useState } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
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

const adminNavItems = [
  { icon: BarChart3, label: "Overview", href: "/" },
  { icon: Users, label: "Users", href: "/users" },
  { icon: FileText, label: "Prompts", href: "/prompts" },
  { icon: MessageSquare, label: "Submissions", href: "/submissions" },
  { icon: Settings, label: "Settings", href: "/settings" },
];

export default function AdminSidebar() {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();
  const activePath = location.pathname;

  const handleLogout = () => {
    // Perform logout logic here later
    navigate("/login");
  };

  return (
    <aside
      className={`relative z-20 flex shrink-0 flex-col border-r border-slate-800 bg-slate-900 transition-all duration-300 shadow-[4px_0_24px_rgba(0,0,0,0.2)] ${
        collapsed ? "w-16" : "w-64"
      }`}
    >
      <Link
        to="/"
        className={`flex h-[72px] shrink-0 items-center gap-3 border-b border-slate-800 px-5 ${collapsed ? "justify-center px-0" : ""}`}
      >
        <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br from-indigo-500 to-purple-600 shadow-md shadow-purple-900/50">
          <ShieldAlert className="h-5 w-5 text-white" strokeWidth={2.5} />
        </div>
        {!collapsed && (
          <div className="flex flex-col">
            <span className="text-[15px] font-extrabold text-white tracking-tight">NomiWrite</span>
            <span className="text-[10px] font-bold text-indigo-400 uppercase tracking-wider">Admin Panel</span>
          </div>
        )}
      </Link>

      <button
        onClick={() => setCollapsed(!collapsed)}
        className="absolute -right-3.5 top-20 flex h-7 w-7 items-center justify-center rounded-full border border-slate-700 bg-slate-800 text-slate-400 shadow-sm hover:bg-slate-700 hover:text-white transition-colors z-30"
      >
        {collapsed ? <ChevronRight className="h-3.5 w-3.5" /> : <ChevronLeft className="h-3.5 w-3.5" />}
      </button>

      <nav className="flex-1 space-y-1 overflow-y-auto p-3">
        <div className="mb-2 px-3 text-[10px] font-extrabold uppercase tracking-widest text-slate-500">
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
                  ? "bg-indigo-500/10 text-indigo-400"
                  : "text-slate-400 hover:bg-slate-800 hover:text-slate-200"
              } ${collapsed ? "justify-center" : ""}`}
              title={collapsed ? item.label : undefined}
            >
              <Icon className={`h-5 w-5 shrink-0 ${isActive ? "text-indigo-400" : "text-slate-500 group-hover:text-slate-300"}`} />
              {!collapsed && <span className="text-sm font-semibold">{item.label}</span>}
            </Link>
          );
        })}
      </nav>

      <div className="border-t border-slate-800 p-3">
        <button
          onClick={handleLogout}
          className={`flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-slate-400 hover:bg-red-500/10 hover:text-red-400 transition-all ${
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
