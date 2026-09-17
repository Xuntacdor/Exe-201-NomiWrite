import { useEffect, useState } from "react";
import { Users, FileText, CheckCircle, TrendingUp, Activity, CreditCard, Loader2 } from "lucide-react";
import { adminService } from "../services/adminService";
import type { UserAnalyticsDto } from "../services/adminService";

export default function Dashboard() {
  const [analytics, setAnalytics] = useState<UserAnalyticsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const data = await adminService.getAnalytics();
        setAnalytics(data);
      } catch (err: any) {
        // If the backend isn't running or returns an error, we can handle it here.
        setError(err.message || "Failed to fetch analytics");
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  const stats = [
    { 
      title: "Total Users", 
      value: analytics ? analytics.totalUsers.toLocaleString() : "0", 
      change: "+12.5%", 
      isPositive: true, 
      icon: Users, 
      color: "text-blue-600", 
      bg: "bg-blue-100" 
    },
    { 
      title: "Active Subscriptions", 
      value: "842", // Placeholder until Subscription backend provides this
      change: "+5.2%", 
      isPositive: true, 
      icon: CreditCard, 
      color: "text-emerald-600", 
      bg: "bg-emerald-100" 
    },
    { 
      title: "Total Prompts", 
      value: "156", // Placeholder until Prompts backend provides this
      change: "+2", 
      isPositive: true, 
      icon: FileText, 
      color: "text-purple-600", 
      bg: "bg-purple-100" 
    },
    { 
      title: "Total Submissions", 
      value: "12,450", // Placeholder until Submissions backend provides this
      change: "+450", 
      isPositive: true, 
      icon: CheckCircle, 
      color: "text-orange-600", 
      bg: "bg-orange-100" 
    },
  ];

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Dashboard Overview</h1>
          <p className="mt-1.5 text-sm font-medium text-slate-500">Welcome back! Here's what's happening on NomiWrite today.</p>
        </div>
        <div className="flex items-center gap-2 rounded-xl bg-white px-4 py-2 shadow-sm border border-slate-200">
          <Activity className="h-4 w-4 text-emerald-500" />
          <span className="text-xs font-bold text-slate-700">System Status: All Systems Operational</span>
        </div>
      </div>

      {/* Error State */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm font-bold flex items-center justify-between">
          <span>Warning: Could not connect to the real backend API ({error}). Check if Docker is running!</span>
        </div>
      )}

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, i) => {
          const Icon = stat.icon;
          return (
            <div key={i} className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm transition-all hover:shadow-md hover:border-indigo-200 group">
              <div className="flex items-center justify-between mb-4">
                <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${stat.bg}`}>
                  <Icon className={`h-6 w-6 ${stat.color}`} strokeWidth={2} />
                </div>
                <div className={`flex items-center gap-1 text-xs font-bold ${stat.isPositive ? 'text-emerald-600' : 'text-red-600'}`}>
                  {stat.change} <TrendingUp className="h-3 w-3" />
                </div>
              </div>
              <p className="text-sm font-bold text-slate-500">{stat.title}</p>
              {loading ? (
                <div className="flex items-center h-9 mt-1">
                  <Loader2 className="h-5 w-5 animate-spin text-indigo-500" />
                </div>
              ) : (
                <h3 className="text-3xl font-extrabold text-slate-900 mt-1">{stat.value}</h3>
              )}
            </div>
          );
        })}
      </div>

      {/* Main Content Area */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Recent Activity */}
        <div className="lg:col-span-2 rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
          <div className="border-b border-slate-100 p-6 flex justify-between items-center bg-slate-50/50">
            <h2 className="text-lg font-extrabold text-slate-900">Recent Activity</h2>
            <button className="text-xs font-bold text-indigo-600 hover:text-indigo-700">View All</button>
          </div>
          <div className="p-6">
            <div className="flex flex-col items-center justify-center py-12 text-center space-y-3">
              <div className="h-12 w-12 rounded-full bg-slate-100 flex items-center justify-center">
                <Activity className="h-5 w-5 text-slate-400" />
              </div>
              <div>
                <p className="text-sm font-bold text-slate-900">No recent activity</p>
                <p className="text-xs text-slate-500 mt-1">Activity logs will appear here once connected to the backend.</p>
              </div>
            </div>
          </div>
        </div>

        {/* Quick Actions / System Health */}
        <div className="space-y-6">
          <div className="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
            <div className="border-b border-slate-100 p-5 bg-slate-50/50">
              <h2 className="text-base font-extrabold text-slate-900">System Health</h2>
            </div>
            <div className="p-5 space-y-4">
              {[
                { label: "API Gateway", status: error ? "Offline" : "Operational", color: error ? "bg-red-500" : "bg-emerald-500" },
                { label: "Database", status: error ? "Offline" : "Operational", color: error ? "bg-red-500" : "bg-emerald-500" },
                { label: "AI Service", status: "Operational", color: "bg-emerald-500" },
                { label: "Payment Gateway", status: "Operational", color: "bg-emerald-500" },
              ].map((service, i) => (
                <div key={i} className="flex items-center justify-between">
                  <span className="text-sm font-semibold text-slate-700">{service.label}</span>
                  <div className="flex items-center gap-2">
                    <span className="text-xs font-bold text-slate-500">{service.status}</span>
                    <div className={`h-2 w-2 rounded-full ${service.color}`} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
