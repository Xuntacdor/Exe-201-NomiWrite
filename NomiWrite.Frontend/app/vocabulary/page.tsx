"use client";

import { useLocale } from "@/lib/i18n/locale";


import { useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { VocabSuggestion, VocabGroup } from "@/lib/types";
import { BookOpen, CheckCircle2, Circle, Loader2, Search, ArrowLeft, Layers } from "lucide-react";

export default function VocabularyPage() {
  const { t: translateUi, errorText } = useLocale();
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
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          {selectedGroup ? (
            <button onClick={() => setSelectedGroup(null)} className="mr-2 rounded-full p-1.5 hover:bg-surface-muted transition-colors">
              <ArrowLeft className="h-4 w-4 text-muted" />
            </button>
          ) : (
            <Layers className="h-4 w-4 text-accent-ink" />
          )}
          <h1 className="text-sm font-extrabold text-ink">
            {selectedGroup ? selectedGroup.name : "Vocabulary Groups"}
          </h1>
        </div>
        {selectedGroup && (
          <div className="flex items-center gap-1.5 text-xs font-semibold text-muted">
            <CheckCircle2 className="h-3.5 w-3.5 text-success-ink" />
            {masteredCount}/{activeWords.length} mastered
          </div>
        )}
      </div>

      <div className="w-full space-y-5 p-6">
        {!selectedGroup && (
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
            <input
              value={search}
              onChange={event => setSearch(event.target.value)}
              placeholder="Search groups by topic name..."
              className="w-full rounded-xl border border-line bg-surface py-2.5 pl-10 pr-4 text-sm text-ink placeholder:text-muted focus:border-focus focus:outline-none"
            />
          </div>
        )}

        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            {translateUi("Loading vocabulary")}</div>
        )}

        {error && <p className="rounded-xl border border-line bg-danger p-4 text-sm font-semibold text-danger-ink">{errorText(error)}</p>}

        {!loading && !error && !selectedGroup && filteredGroups.length === 0 && (
          <div className="rounded-2xl border border-line bg-surface p-8 text-center shadow-sm">
            <Layers className="mx-auto mb-3 h-8 w-8 text-muted" />
            <p className="text-sm font-bold text-ink">No vocabulary groups found</p>
            <p className="mt-1 text-xs text-muted">Group your vocabulary from the grading result page.</p>
          </div>
        )}

        {!loading && !error && !selectedGroup && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {filteredGroups.map(group => (
              <button
                key={group.id}
                onClick={() => setSelectedGroup(group)}
                className="group text-left flex flex-col rounded-2xl border border-line bg-surface p-5 shadow-sm transition-all hover:border-line hover:shadow-md"
              >
                <div className="mb-2 flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-accent text-accent-ink group-hover:bg-accent group-hover:text-ink transition-colors">
                      <BookOpen className="h-4 w-4" />
                    </div>
                    <span className="font-bold text-ink">{group.name}</span>
                  </div>
                </div>
                <div className="mt-auto pt-4 flex items-center justify-between text-xs font-semibold text-muted border-t border-line">
                  <span>{group.wordCount} words</span>
                  <span className="text-accent-ink opacity-0 group-hover:opacity-100 transition-opacity">View group &rarr;</span>
                </div>
              </button>
            ))}
          </div>
        )}

        {!loading && !error && selectedGroup && activeWords.length === 0 && (
          <div className="rounded-2xl border border-line bg-surface p-8 text-center shadow-sm">
            <BookOpen className="mx-auto mb-3 h-8 w-8 text-muted" />
            <p className="text-sm font-bold text-ink">No words in this group</p>
          </div>
        )}

        {!loading && !error && selectedGroup && activeWords.length > 0 && (
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            {activeWords.map(word => (
              <div key={word.id} className={`rounded-2xl border p-5 shadow-sm ${word.isMastered ? "border-line bg-success" : "border-line bg-surface"}`}>
                <div className="mb-3 flex items-center gap-2">
                  <span className="text-sm font-medium text-muted line-through">{word.originalWord}</span>
                  <span className="text-muted">-&gt;</span>
                  <span className="text-base font-extrabold text-ink">{word.suggestedWord}</span>
                </div>
                <p className="mb-4 text-xs leading-relaxed text-muted">{word.exampleSentence}</p>
                <button
                  type="button"
                  onClick={() => toggleMastered(word)}
                  className={`flex items-center gap-2 text-xs font-bold ${word.isMastered ? "text-success-ink" : "text-accent-ink"}`}
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
