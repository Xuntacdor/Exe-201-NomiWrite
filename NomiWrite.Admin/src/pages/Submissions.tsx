"use client";

import { useState } from "react";
import { Search, Filter, Eye, FileCheck, Target, GraduationCap } from "lucide-react";

interface Submission {
  id: string;
  user: string;
  type: string;
  score: number;
  wordCount: number;
  submittedAt: string;
  status: "Graded" | "Pending";
}

const mockSubmissions: Submission[] = [
  { id: "S-1045", user: "Nguyen Van A", type: "IELTS Task 2", score: 7.5, wordCount: 312, submittedAt: "2023-11-22 14:30", status: "Graded" },
  { id: "S-1044", user: "Tran Thi B", type: "VSTEP", score: 6.0, wordCount: 245, submittedAt: "2023-11-22 13:15", status: "Graded" },
  { id: "S-1043", user: "Le Van C", type: "Email", score: 8.0, wordCount: 150, submittedAt: "2023-11-22 11:45", status: "Graded" },
  { id: "S-1042", user: "Pham D", type: "IELTS Task 1", score: 0, wordCount: 180, submittedAt: "2023-11-22 10:20", status: "Pending" },
  { id: "S-1041", user: "Hoang E", type: "Academic", score: 6.5, wordCount: 420, submittedAt: "2023-11-21 16:50", status: "Graded" },
];

export default function AdminSubmissionsPage() {
  const [searchTerm, setSearchTerm] = useState("");

  return (
    <div className="p-8 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Submissions Review</h1>
          <p className="mt-1.5 text-sm font-medium text-slate-500">Monitor student essays and AI grading results.</p>
        </div>
      </div>

      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl border border-slate-200 shadow-sm">
        <div className="relative w-full sm:w-96">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search by user or ID..."
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

      {/* Submissions Table */}
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 border-b border-slate-200">
              <tr>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">ID / User</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Type</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Score</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Word Count</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider">Submitted</th>
                <th className="px-6 py-4 font-extrabold text-slate-900 tracking-wider text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {mockSubmissions.map((sub) => (
                <tr key={sub.id} className="hover:bg-slate-50/50 transition-colors">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 rounded-xl bg-orange-50 flex items-center justify-center">
                        <FileCheck className="h-5 w-5 text-orange-600" />
                      </div>
                      <div>
                        <div className="font-bold text-slate-900">{sub.id}</div>
                        <div className="text-xs text-slate-500 flex items-center gap-1 mt-0.5">
                          {sub.user}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-md text-[11px] font-extrabold uppercase tracking-widest bg-blue-100 text-blue-700">
                      <GraduationCap className="h-3 w-3" />
                      {sub.type}
                    </span>
                  </td>
                  <td className="px-6 py-4">
                    {sub.status === "Graded" ? (
                      <div className="flex items-center gap-2">
                        <Target className="h-4 w-4 text-emerald-500" />
                        <span className="font-bold text-slate-900">{sub.score}</span>
                      </div>
                    ) : (
                      <span className="inline-flex px-2 py-0.5 rounded-md text-[10px] font-bold uppercase tracking-widest bg-slate-100 text-slate-500">
                        {sub.status}
                      </span>
                    )}
                  </td>
                  <td className="px-6 py-4 text-slate-600 font-medium">
                    {sub.wordCount} words
                  </td>
                  <td className="px-6 py-4 text-slate-500 font-medium text-xs">
                    {sub.submittedAt}
                  </td>
                  <td className="px-6 py-4 text-right">
                    <div className="flex items-center justify-end gap-2">
                      <button className="flex items-center gap-1 px-3 py-1.5 text-xs font-bold text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors border border-indigo-200 bg-white">
                        <Eye className="h-3.5 w-3.5" /> View
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
