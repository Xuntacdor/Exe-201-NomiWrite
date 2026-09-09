"use client";

import { useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import { apiClient, apiMode } from "@/lib/api/client";
import type { VocabSuggestion } from "@/lib/types";
import { BookOpen, CheckCircle2, Circle, Loader2, Search, Sparkles } from "lucide-react";

export default function VocabularyPage() {
  const [words, setWords] = useState<VocabSuggestion[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(apiMode === "mock");
  const [error, setError] = useState("");

  useEffect(() => {
    if (apiMode === "real") {
      return;
    }

    let ignore = false;
    apiClient.listVocabulary()
      .then(items => {
        if (!ignore) setWords(items);
      })
      .catch(err => {
        if (!ignore) setError(err instanceof Error ? err.message : "Could not load vocabulary.");
      })
      .finally(() => {
        if (!ignore) setLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, []);

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) return words;
    return words.filter(word =>
      word.originalWord.toLowerCase().includes(query) ||
      word.suggestedWord.toLowerCase().includes(query) ||
      word.topic.toLowerCase().includes(query),
    );
  }, [search, words]);

  const masteredCount = words.filter(word => word.isMastered).length;

  async function toggleMastered(word: VocabSuggestion) {
    const next = !word.isMastered;
    setWords(current => current.map(item => item.id === word.id ? { ...item, isMastered: next } : item));
    try {
      const updated = await apiClient.updateVocabularyMastered(word.id, { isMastered: next });
      setWords(current => current.map(item => item.id === word.id ? updated : item));
    } catch (err) {
      setWords(current => current.map(item => item.id === word.id ? word : item));
      setError(err instanceof Error ? err.message : "Could not update vocabulary.");
    }
  }

  return (
    <AppShell activePath="/vocabulary">
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-slate-100 bg-white/90 px-6 backdrop-blur">
        <div className="flex items-center gap-2">
          <BookOpen className="h-4 w-4 text-emerald-500" />
          <h1 className="text-sm font-extrabold text-slate-900">Vocabulary</h1>
        </div>
        <div className="flex items-center gap-1.5 text-xs font-semibold text-slate-500">
          <CheckCircle2 className="h-3.5 w-3.5 text-emerald-500" />
          {masteredCount}/{words.length} mastered
        </div>
      </div>

      <div className="w-full space-y-5 p-6">
        {apiMode === "real" ? (
          <div className="rounded-2xl border border-blue-100 bg-blue-50 p-6">
            <div className="mb-3 flex items-center gap-2">
              <Sparkles className="h-5 w-5 text-blue-600" />
              <h2 className="text-base font-extrabold text-blue-900">Vocabulary API pending</h2>
            </div>
            <p className="max-w-2xl text-sm leading-relaxed text-blue-700">
              The frontend page is ready, but the pulled backend does not expose a dedicated vocabulary module yet. Vocabulary suggestions are available inside grading results and this page will switch to real data when the backend adds list/update endpoints.
            </p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-3 gap-3">
              {[
                { label: "Total", value: words.length, color: "text-slate-900" },
                { label: "Learning", value: words.length - masteredCount, color: "text-blue-600" },
                { label: "Mastered", value: masteredCount, color: "text-emerald-600" },
              ].map(item => (
                <div key={item.label} className="rounded-2xl border border-slate-100 bg-white p-4 text-center shadow-sm">
                  <p className={`text-2xl font-extrabold ${item.color}`}>{item.value}</p>
                  <p className="mt-0.5 text-xs text-slate-500">{item.label}</p>
                </div>
              ))}
            </div>

            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
              <input
                value={search}
                onChange={event => setSearch(event.target.value)}
                placeholder="Search original word, suggestion, or topic"
                className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-10 pr-4 text-sm text-slate-700 placeholder:text-slate-400 focus:border-blue-400 focus:outline-none"
              />
            </div>

            {loading && (
              <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
                <Loader2 className="h-4 w-4 animate-spin" />
                Loading vocabulary
              </div>
            )}

            {error && <p className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>}

            {!loading && filtered.length === 0 && (
              <div className="rounded-2xl border border-slate-100 bg-white p-8 text-center shadow-sm">
                <BookOpen className="mx-auto mb-3 h-8 w-8 text-slate-300" />
                <p className="text-sm font-bold text-slate-800">No vocabulary found</p>
                <p className="mt-1 text-xs text-slate-500">Suggestions appear after grading returns vocabulary feedback.</p>
              </div>
            )}

            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {filtered.map(word => (
                <div key={word.id} className={`rounded-2xl border p-5 shadow-sm ${word.isMastered ? "border-emerald-200 bg-emerald-50" : "border-slate-100 bg-white"}`}>
                  <div className="mb-3 flex items-center gap-2">
                    <span className="text-sm font-medium text-slate-400 line-through">{word.originalWord}</span>
                    <span className="text-slate-300">-&gt;</span>
                    <span className="text-base font-extrabold text-slate-900">{word.suggestedWord}</span>
                  </div>
                  <p className="mb-4 text-xs leading-relaxed text-slate-500">{word.exampleSentence}</p>
                  <button
                    type="button"
                    onClick={() => toggleMastered(word)}
                    className={`flex items-center gap-2 text-xs font-bold ${word.isMastered ? "text-emerald-700" : "text-blue-600"}`}
                  >
                    {word.isMastered ? <CheckCircle2 className="h-4 w-4" /> : <Circle className="h-4 w-4" />}
                    {word.isMastered ? "Mastered" : "Mark as mastered"}
                  </button>
                </div>
              ))}
            </div>
          </>
        )}
      </div>
    </AppShell>
  );
}
