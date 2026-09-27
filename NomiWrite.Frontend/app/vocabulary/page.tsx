"use client";

import { useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { VocabSuggestion, VocabGroup } from "@/lib/types";
import { BookOpen, CheckCircle2, Circle, Loader2, Search, ArrowLeft, Layers } from "lucide-react";

export default function VocabularyPage() {
  const [groups, setGroups] = useState<VocabGroup[]>([]);
  const [words, setWords] = useState<VocabSuggestion[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [selectedGroup, setSelectedGroup] = useState<VocabGroup | null>(null);

  useEffect(() => {
    let ignore = false;
    Promise.all([apiClient.listVocabGroups(), apiClient.listVocabulary()])
      .then(([g, w]) => {
        if (!ignore) {
          setGroups(g);
          setWords(w);
        }
      })
      .catch(err => {
        if (!ignore) setError(err instanceof Error ? err.message : "Could not load vocabulary data.");
      })
      .finally(() => {
        if (!ignore) setLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, []);

  const filteredGroups = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) return groups;
    return groups.filter(group => group.name.toLowerCase().includes(query));
  }, [search, groups]);

  const activeWords = useMemo(() => {
    if (!selectedGroup) return [];
    return words.filter(word => selectedGroup.vocabularyIds.includes(word.id));
  }, [selectedGroup, words]);

  const masteredCount = activeWords.filter(w => w.isMastered).length;

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
          {selectedGroup ? (
            <button onClick={() => setSelectedGroup(null)} className="mr-2 rounded-full p-1.5 hover:bg-slate-100 transition-colors">
              <ArrowLeft className="h-4 w-4 text-slate-500" />
            </button>
          ) : (
            <Layers className="h-4 w-4 text-blue-500" />
          )}
          <h1 className="text-sm font-extrabold text-slate-900">
            {selectedGroup ? selectedGroup.name : "Vocabulary Groups"}
          </h1>
        </div>
        {selectedGroup && (
          <div className="flex items-center gap-1.5 text-xs font-semibold text-slate-500">
            <CheckCircle2 className="h-3.5 w-3.5 text-emerald-500" />
            {masteredCount}/{activeWords.length} mastered
          </div>
        )}
      </div>

      <div className="w-full space-y-5 p-6">
        {!selectedGroup && (
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
            <input
              value={search}
              onChange={event => setSearch(event.target.value)}
              placeholder="Search groups by topic name..."
              className="w-full rounded-xl border border-slate-200 bg-white py-2.5 pl-10 pr-4 text-sm text-slate-700 placeholder:text-slate-400 focus:border-blue-400 focus:outline-none"
            />
          </div>
        )}

        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-2xl border border-slate-100 bg-white p-8 text-sm font-semibold text-slate-500 shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading vocabulary...
          </div>
        )}

        {error && <p className="rounded-2xl border border-red-100 bg-red-50 p-4 text-sm font-semibold text-red-600">{error}</p>}

        {!loading && !selectedGroup && filteredGroups.length === 0 && (
          <div className="rounded-2xl border border-slate-100 bg-white p-8 text-center shadow-sm">
            <Layers className="mx-auto mb-3 h-8 w-8 text-slate-300" />
            <p className="text-sm font-bold text-slate-800">No vocabulary groups found</p>
            <p className="mt-1 text-xs text-slate-500">Group your vocabulary from the grading result page.</p>
          </div>
        )}

        {!loading && !selectedGroup && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {filteredGroups.map(group => (
              <button
                key={group.id}
                onClick={() => setSelectedGroup(group)}
                className="group text-left flex flex-col rounded-2xl border border-slate-100 bg-white p-5 shadow-sm transition-all hover:border-blue-200 hover:shadow-md"
              >
                <div className="mb-2 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-50 text-blue-600 group-hover:bg-blue-600 group-hover:text-white transition-colors">
                      <BookOpen className="h-4 w-4" />
                    </div>
                    <span className="font-bold text-slate-800">{group.name}</span>
                  </div>
                </div>
                <div className="mt-auto pt-4 flex items-center justify-between text-xs font-semibold text-slate-500 border-t border-slate-50">
                  <span>{group.wordCount} words</span>
                  <span className="text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity">View group &rarr;</span>
                </div>
              </button>
            ))}
          </div>
        )}

        {!loading && selectedGroup && activeWords.length === 0 && (
          <div className="rounded-2xl border border-slate-100 bg-white p-8 text-center shadow-sm">
            <BookOpen className="mx-auto mb-3 h-8 w-8 text-slate-300" />
            <p className="text-sm font-bold text-slate-800">No words in this group</p>
          </div>
        )}

        {!loading && selectedGroup && activeWords.length > 0 && (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {activeWords.map(word => (
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
        )}
      </div>
    </AppShell>
  );
}
