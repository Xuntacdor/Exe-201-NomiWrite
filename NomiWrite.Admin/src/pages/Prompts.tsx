"use client";

import { useEffect, useMemo, useState } from "react";
import { Plus, Search, Filter, MoreVertical, FileText, Edit2, Trash2, Loader2 } from "lucide-react";
import { adminService } from "../services/adminService";
import type { AdminPromptListItemDto } from "../services/adminService";

function normalizeEnum(value: number | string) {
  return String(value).trim().toLowerCase();
}

function getDifficultyLabel(difficulty: number | string) {
  const normalized = normalizeEnum(difficulty);
  if (normalized === "0" || normalized === "beginner") return "Beginner";
  if (normalized === "2" || normalized === "advanced") return "Advanced";
  return "Intermediate";
}

export default function AdminPromptsPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [prompts, setPrompts] = useState<AdminPromptListItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    const fetchPrompts = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await adminService.getPrompts(1, 50);
        setPrompts(data.items);
        setTotalCount(data.totalCount);
      } catch (err: any) {
        setError(err.message || "Failed to fetch prompts");
      } finally {
        setLoading(false);
      }
    };

    fetchPrompts();
  }, []);

  const filteredPrompts = useMemo(() => {
    const query = searchTerm.trim().toLowerCase();
    if (!query) return prompts;

    return prompts.filter((prompt) => {
      const typeName = prompt.writingTypeName || prompt.writingTypeId;
      return (
        prompt.title.toLowerCase().includes(query) ||
        typeName.toLowerCase().includes(query) ||
        getDifficultyLabel(prompt.difficulty).toLowerCase().includes(query)
      );
    });
  }, [prompts, searchTerm]);

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Prompts Management</h1>
          <p className="mt-1.5 text-sm font-medium text-slate-500">Create, edit, and publish writing prompts.</p>
        </div>
        <button className="flex items-center justify-center gap-2 px-5 py-2.5 bg-indigo-600 rounded-xl text-sm font-bold text-white hover:bg-indigo-700 transition-colors shadow-sm shadow-indigo-200">
          <Plus className="h-4 w-4" /> Create Prompt
        </button>
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl border border-slate-200 shadow-sm">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search prompts..."
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

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-600 text-sm font-bold flex items-center justify-between">
          <span>Warning: Could not fetch prompts from the real backend API ({error}). Are you sure the backend is running?</span>
        </div>
      )}

      {/* Prompts Table */}
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
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Title</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Type</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Difficulty</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Status</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Created</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filteredPrompts.length === 0 && !loading && !error && (
                <tr>
                  <td colSpan={6} className="px-6 py-12 text-center text-slate-500 font-semibold">No prompts found.</td>
                </tr>
              )}
              {filteredPrompts.map((prompt) => {
                const difficultyLabel = getDifficultyLabel(prompt.difficulty);
                const typeName = prompt.writingTypeName || prompt.writingTypeId.slice(0, 8);

                return (
                <tr key={prompt.id} className="hover:bg-slate-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 rounded-xl bg-indigo-50 flex items-center justify-center">
                        <FileText className="h-5 w-5 text-indigo-600" />
                      </div>
                      <div className="font-bold text-slate-900">{prompt.title}</div>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-slate-600 font-semibold">
                    {typeName}
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      difficultyLabel === "Beginner" ? "bg-emerald-100 text-emerald-700" :
                      difficultyLabel === "Intermediate" ? "bg-amber-100 text-amber-700" :
                      "bg-rose-100 text-rose-700"
                    }`}>
                      {difficultyLabel}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      prompt.isActive ? "bg-blue-100 text-blue-700" : "bg-slate-200 text-slate-600"
                    }`}>
                      {prompt.isActive ? "Published" : "Draft"}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-500 font-medium">
                    {new Date(prompt.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <button className="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors">
                        <Edit2 className="h-4 w-4" />
                      </button>
                      <button className="p-2 text-slate-400 hover:text-red-600 hover:bg-red-50 rounded-lg transition-colors">
                        <Trash2 className="h-4 w-4" />
                      </button>
                      <button className="p-2 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors">
                        <MoreVertical className="h-4 w-4" />
                      </button>
                    </div>
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
            {totalCount > 0 ? `Showing 1 to ${Math.min(50, totalCount)} of ${totalCount} entries` : "Showing 0 entries"}
          </p>
          <div className="flex gap-2">
            <button className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold text-slate-400 bg-white cursor-not-allowed">Previous</button>
            <button className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold text-slate-400 bg-white cursor-not-allowed">Next</button>
          </div>
        </div>
      </div>
    </div>
  );
}
