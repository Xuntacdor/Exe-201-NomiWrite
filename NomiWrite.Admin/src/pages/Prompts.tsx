"use client";

import { useState } from "react";
import { Plus, Search, Filter, MoreVertical, FileText, Edit2, Trash2 } from "lucide-react";

interface Prompt {
  id: string;
  title: string;
  type: string;
  difficulty: "Easy" | "Medium" | "Hard";
  status: "Published" | "Draft";
  createdAt: string;
}

const mockPrompts: Prompt[] = [
  { id: "1", title: "Technology in modern society", type: "IELTS Task 2", difficulty: "Medium", status: "Published", createdAt: "2023-11-10" },
  { id: "2", title: "Describe a memorable journey", type: "IELTS Task 2", difficulty: "Easy", status: "Published", createdAt: "2023-11-12" },
  { id: "3", title: "Line graph: renewable energy 2000-2020", type: "IELTS Task 1", difficulty: "Hard", status: "Draft", createdAt: "2023-11-15" },
  { id: "4", title: "VSTEP Writing Task 2: Environment", type: "VSTEP", difficulty: "Medium", status: "Published", createdAt: "2023-11-18" },
  { id: "5", title: "Write an email to a manager", type: "Email", difficulty: "Easy", status: "Published", createdAt: "2023-11-20" },
];

export default function AdminPromptsPage() {
  const [searchTerm, setSearchTerm] = useState("");

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

      {/* Prompts Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
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
              {mockPrompts.map((prompt) => (
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
                    {prompt.type}
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      prompt.difficulty === "Easy" ? "bg-emerald-100 text-emerald-700" :
                      prompt.difficulty === "Medium" ? "bg-amber-100 text-amber-700" :
                      "bg-rose-100 text-rose-700"
                    }`}>
                      {prompt.difficulty}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest ${
                      prompt.status === "Published" ? "bg-blue-100 text-blue-700" : "bg-slate-200 text-slate-600"
                    }`}>
                      {prompt.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-slate-500 font-medium">
                    {prompt.createdAt}
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
              ))}
            </tbody>
          </table>
        </div>
        {/* Pagination */}
        <div className="px-6 py-4 border-t border-slate-100 flex items-center justify-between bg-slate-50/50">
          <p className="text-xs font-semibold text-slate-500">Showing 1 to 5 of 5 entries</p>
          <div className="flex gap-2">
            <button className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold text-slate-400 bg-white cursor-not-allowed">Previous</button>
            <button className="px-3 py-1.5 border border-slate-200 rounded-lg text-xs font-bold text-slate-400 bg-white cursor-not-allowed">Next</button>
          </div>
        </div>
      </div>
    </div>
  );
}
