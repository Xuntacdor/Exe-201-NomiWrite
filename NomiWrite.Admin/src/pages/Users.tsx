import { useState, useEffect } from "react";
import { Search, Filter, MoreVertical, Shield, UserX, UserCheck, Mail, Loader2 } from "lucide-react";
import { adminService } from "../services/adminService";
import type { AdminUserListItemDto } from "../services/adminService";

function normalizeEnum(value: number | string) {
  return String(value).trim().toLowerCase();
}

function getRoleLabel(role: number | string) {
  const normalized = normalizeEnum(role);
  if (normalized === "1" || normalized === "admin") return "Admin";
  if (normalized === "2" || normalized === "moderator") return "Moderator";
  return "Student";
}

function getStatusLabel(status: number | string) {
  const normalized = normalizeEnum(status);
  if (normalized === "0" || normalized === "active") return "Active";
  if (normalized === "1" || normalized === "deactivated") return "Deactivated";
  return "Blocked";
}

export default function Users() {
  const [searchTerm, setSearchTerm] = useState("");
  const [users, setUsers] = useState<AdminUserListItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        setLoading(true);
        const data = await adminService.getUsers(currentPage, 10);
        setUsers(data.items);
        setTotalCount(data.totalCount);
      } catch (err: any) {
        setError(err.message || "Failed to fetch users");
      } finally {
        setLoading(false);
      }
    };

    fetchUsers();
  }, [currentPage]);
  
  const handlePrevious = () => setCurrentPage((prev) => Math.max(1, prev - 1));
  const handleNext = () => setCurrentPage((prev) => (prev * 10 < totalCount ? prev + 1 : prev));
  const startEntry = totalCount === 0 ? 0 : (currentPage - 1) * 10 + 1;
  const endEntry = Math.min(currentPage * 10, totalCount);

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Users Management</h1>
          <p className="mt-1.5 text-sm font-medium text-slate-500">Manage students, admins, and platform access.</p>
        </div>
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl border border-slate-200 shadow-sm">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all"
          />
        </div>
        <div className="flex items-center gap-3 w-full sm:w-auto">
          <button className="flex items-center justify-center gap-2 px-4 py-2.5 border border-slate-200 bg-white rounded-xl text-sm font-bold text-slate-700 hover:bg-slate-50 transition-colors w-full sm:w-auto">
            <Filter className="h-4 w-4" /> Filter
          </button>
        </div>
      </div>

      {/* Error State */}
      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm font-bold flex items-center justify-between">
          <span>Warning: Could not fetch users from the real backend API ({error}). Are you sure the backend is running?</span>
        </div>
      )}

      {/* Users Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto min-h-[400px] relative">
          {loading && (
            <div className="absolute inset-0 bg-white/50 backdrop-blur-sm z-10 flex items-center justify-center">
              <Loader2 className="h-8 w-8 animate-spin text-indigo-500" />
            </div>
          )}
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">User</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Role</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Status</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Joined Date</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {users.length === 0 && !loading && !error && (
                 <tr>
                  <td colSpan={5} className="px-6 py-12 text-center text-slate-500 font-semibold">No users found.</td>
                 </tr>
              )}
              {users.map((user) => {
                const roleLabel = getRoleLabel(user.role);
                const statusLabel = getStatusLabel(user.accountStatus);
                const isElevatedRole = roleLabel !== "Student";
                const isActive = statusLabel === "Active";

                return (
                <tr key={user.id} className="hover:bg-slate-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 rounded-full bg-gradient-to-br from-indigo-100 to-purple-100 flex items-center justify-center text-indigo-700 font-bold uppercase">
                        {user.fullName ? user.fullName.charAt(0) : '?'}
                      </div>
                      <div>
                        <div className="font-bold text-slate-900">{user.fullName || "Unknown"}</div>
                        <div className="text-xs text-slate-500 flex items-center gap-1 mt-0.5">
                          <Mail className="h-3 w-3" /> {user.email}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      isElevatedRole ? "bg-purple-100 text-purple-700" : "bg-slate-100 text-slate-600"
                    }`}>
                      {isElevatedRole && <Shield className="h-3 w-3" />}
                      {roleLabel}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      isActive ? "bg-emerald-100 text-emerald-700" : "bg-red-100 text-red-700"
                    }`}>
                      {isActive ? <UserCheck className="h-3 w-3" /> : <UserX className="h-3 w-3" />}
                      {statusLabel}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-500 font-medium">
                    {new Date(user.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <button className="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors">
                      <MoreVertical className="h-4 w-4" />
                    </button>
                  </td>
                </tr>
                );
              })}
            </tbody>
          </table>
        </div>
        {/* Pagination */}
        <div className="px-6 py-4 border-t border-slate-100 flex items-center justify-between bg-slate-50/50">
          <p className="text-xs font-semibold text-slate-500">
            {totalCount > 0 ? `Showing ${startEntry} to ${endEntry} of ${totalCount} entries` : "Showing 0 entries"}
          </p>
          <div className="flex gap-2">
            <button 
              onClick={handlePrevious} 
              disabled={currentPage === 1} 
              className={`px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold ${currentPage === 1 ? 'text-slate-400 bg-white cursor-not-allowed' : 'text-slate-700 bg-white hover:bg-slate-50 transition-colors'}`}
            >
              Previous
            </button>
            <button 
              onClick={handleNext} 
              disabled={currentPage * 10 >= totalCount} 
              className={`px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold ${currentPage * 10 >= totalCount ? 'text-slate-400 bg-white cursor-not-allowed' : 'text-slate-700 bg-white hover:bg-slate-50 transition-colors'}`}
            >
              Next
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
