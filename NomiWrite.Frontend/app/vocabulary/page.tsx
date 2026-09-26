"use client";

import { useEffect, useMemo, useState } from "react";
import AppShell from "../components/AppShell";
import { apiClient } from "@/lib/api/client";
import type { VocabSuggestion } from "@/lib/types";
import { BookOpen, CheckCircle2, Circle, Loader2, Search } from "lucide-react";

export default function VocabularyPage() {
  const [words, setWords] = useState<VocabSuggestion[]>([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
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
      <div className="sticky top-0 z-10 flex h-14 items-center justify-between border-b border-line bg-surface px-6">
        <div className="flex items-center gap-2">
          <BookOpen className="h-4 w-4 text-success-ink" />
          <h1 className="text-sm font-bold text-ink">Vocabulary</h1>
        </div>
        <div className="flex items-center gap-1.5 text-xs font-semibold text-muted">
          <CheckCircle2 className="h-3.5 w-3.5 text-success-ink" />
          {masteredCount}/{words.length} mastered
        </div>
      </div>

      <div className="w-full space-y-5 p-6">
        <div className="grid grid-cols-3 gap-3">
          {[
            { label: "Total", value: words.length, color: "text-ink" },
            { label: "Learning", value: words.length - masteredCount, color: "text-accent-ink" },
            { label: "Mastered", value: masteredCount, color: "text-success-ink" },
          ].map(item => (
            <div key={item.label} className="rounded-xl border border-line bg-surface p-4 text-center shadow-sm">
              <p className={`text-2xl font-bold ${item.color}`}>{item.value}</p>
              <p className="mt-0.5 text-xs text-muted">{item.label}</p>
            </div>
          ))}
        </div>

        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" />
          <input
            value={search}
            onChange={event => setSearch(event.target.value)}
            placeholder="Search original word, suggestion, or topic"
            className="w-full rounded-xl border border-line bg-surface py-2.5 pl-10 pr-4 text-sm text-ink placeholder:text-muted focus:border-focus focus:outline-none"
          />
        </div>

        {loading && (
          <div className="flex items-center justify-center gap-2 rounded-xl border border-line bg-surface p-8 text-sm font-semibold text-muted shadow-sm">
            <Loader2 className="h-4 w-4 animate-spin" />
            Loading vocabulary
          </div>
        )}

        {error && <p className="rounded-xl border border-line bg-danger p-4 text-sm font-semibold text-danger-ink">{error}</p>}

        {!loading && filtered.length === 0 && (
          <div className="rounded-xl border border-line bg-surface p-8 text-center shadow-sm">
            <BookOpen className="mx-auto mb-3 h-8 w-8 text-muted" />
            <p className="text-sm font-bold text-ink">No vocabulary found</p>
            <p className="mt-1 text-xs text-muted">Suggestions appear after grading returns vocabulary feedback.</p>
          </div>
        )}

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          {filtered.map(word => (
            <div key={word.id} className={`rounded-xl border p-5 shadow-sm ${word.isMastered ? "border-line bg-success" : "border-line bg-surface"}`}>
              <div className="mb-3 flex items-center gap-2">
                <span className="text-sm font-medium text-muted line-through">{word.originalWord}</span>
                <span className="text-muted">-&gt;</span>
                <span className="text-base font-bold text-ink">{word.suggestedWord}</span>
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
      </div>
    </AppShell>
  );
}
