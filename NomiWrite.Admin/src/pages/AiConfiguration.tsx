import { useEffect, useState } from "react";
import type { FormEvent, ReactNode } from "react";
import { Bot, CheckCircle2, Eye, EyeOff, KeyRound, Loader2, RefreshCw, Save, ShieldCheck, TriangleAlert } from "lucide-react";
import { adminService } from "../services/adminService";
import type { AiGradingConfigDto, UpdateAiGradingConfigRequest } from "../services/adminService";

const geminiModels = [
  { id: "gemini-3.8-flash", label: "Gemini 3.8 Flash" },
  { id: "gemini-3.7-flash", label: "Gemini 3.7 Flash" },
  { id: "gemini-3.6-flash", label: "Gemini 3.6 Flash" },
  { id: "gemini-3.5-flash", label: "Gemini 3.5 Flash" },
  { id: "gemini-3.5-flash-lite", label: "Gemini 3.5 Flash-Lite" },
  { id: "gemini-3.1-flash-lite", label: "Gemini 3.1 Flash-Lite" },
  { id: "gemini-3.1-pro-preview", label: "Gemini 3.1 Pro Preview" },
  { id: "gemini-3-flash-preview", label: "Gemini 3 Flash Preview" },
  { id: "gemini-2.5-pro", label: "Gemini 2.5 Pro" },
  { id: "gemini-2.5-flash", label: "Gemini 2.5 Flash" },
  { id: "gemini-2.5-flash-lite", label: "Gemini 2.5 Flash-Lite" },
] as const;

const customModelValue = "__custom__";

type FormState = {
  providerName: string;
  modelName: string;
  fallbackModelName: string;
  apiKey: string;
  clearApiKey: boolean;
  temperature: string;
  maxOutputTokens: string;
  systemPromptTemplate: string;
};

const emptyForm: FormState = {
  providerName: "Gemini",
  modelName: "gemini-2.5-flash",
  fallbackModelName: "gemini-2.5-flash-lite",
  apiKey: "",
  clearApiKey: false,
  temperature: "",
  maxOutputTokens: "",
  systemPromptTemplate: "",
};

function toForm(config: AiGradingConfigDto): FormState {
  return {
    providerName: config.providerName,
    modelName: config.modelName,
    fallbackModelName: config.fallbackModelName ?? "",
    apiKey: "",
    clearApiKey: false,
    temperature: config.temperature?.toString() ?? "",
    maxOutputTokens: config.maxOutputTokens?.toString() ?? "",
    systemPromptTemplate: config.systemPromptTemplate ?? "",
  };
}

function errorMessage(error: unknown) {
  const candidate = error as { response?: { data?: { message?: string; errors?: string[] } }; message?: string };
  return candidate.response?.data?.errors?.join(" ") || candidate.response?.data?.message || candidate.message || "Could not update AI configuration.";
}

export default function AiConfiguration() {
  const [config, setConfig] = useState<AiGradingConfigDto | null>(null);
  const [form, setForm] = useState<FormState>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [showKey, setShowKey] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const load = async () => {
    try {
      setLoading(true);
      setError(null);
      const value = await adminService.getAiConfig();
      setConfig(value);
      setForm(toForm(value));
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let active = true;
    void adminService.getAiConfig()
      .then((value) => {
        if (!active) return;
        setConfig(value);
        setForm(toForm(value));
      })
      .catch((err: unknown) => {
        if (active) setError(errorMessage(err));
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => { active = false; };
  }, []);

  const update = (field: keyof FormState, value: string | boolean) => {
    setForm((current) => ({ ...current, [field]: value }));
    setSuccess(null);
  };

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (!form.modelName.trim()) {
      setError("Primary model is required.");
      return;
    }

    const request: UpdateAiGradingConfigRequest = {
      providerName: form.providerName,
      modelName: form.modelName.trim(),
      fallbackModelName: form.fallbackModelName.trim() || null,
      clearApiKey: form.clearApiKey,
      temperature: form.temperature === "" ? null : Number(form.temperature),
      maxOutputTokens: form.maxOutputTokens === "" ? null : Number(form.maxOutputTokens),
      systemPromptTemplate: form.systemPromptTemplate.trim() || null,
    };
    if (form.apiKey.trim()) request.apiKey = form.apiKey.trim();

    try {
      setSaving(true);
      setError(null);
      setSuccess(null);
      const updated = await adminService.updateAiConfig(request);
      setConfig(updated);
      setForm(toForm(updated));
      setSuccess("Configuration activated. New grading requests will use it immediately.");
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="mx-auto max-w-5xl space-y-7 p-8">
      <header className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <div className="mb-2 flex items-center gap-2 text-xs font-extrabold uppercase tracking-[0.18em] text-rose-700"><Bot className="h-4 w-4" /> Runtime controls</div>
          <h1 className="text-3xl font-extrabold tracking-tight text-slate-900">AI Configuration</h1>
          <p className="mt-1.5 max-w-2xl text-sm font-medium text-slate-500">Switch Gemini credentials and models without rebuilding or redeploying the service.</p>
        </div>
        <button type="button" onClick={() => void load()} disabled={loading || saving} className="flex items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-50">
          <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} /> Reload
        </button>
      </header>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatusCard label="Provider" value={config?.providerName ?? "—"} icon={<Bot className="h-5 w-5" />} />
        <StatusCard label="Active model" value={config?.modelName ?? "—"} icon={<CheckCircle2 className="h-5 w-5" />} />
        <StatusCard label="Admin-managed key" value={config?.hasStoredApiKey ? config.apiKeyHint || "Configured" : "Environment fallback"} icon={<KeyRound className="h-5 w-5" />} />
      </div>

      {error && <div className="flex items-start gap-3 rounded-xl border border-red-200 bg-red-50 p-4 text-sm font-semibold text-red-700"><TriangleAlert className="mt-0.5 h-5 w-5 shrink-0" /><span>{error}</span></div>}
      {success && <div className="flex items-start gap-3 rounded-xl border border-emerald-200 bg-emerald-50 p-4 text-sm font-semibold text-emerald-700"><CheckCircle2 className="mt-0.5 h-5 w-5 shrink-0" /><span>{success}</span></div>}

      <form onSubmit={submit} className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        {loading && <div className="flex min-h-80 items-center justify-center"><Loader2 className="h-8 w-8 animate-spin text-rose-700" /></div>}
        {!loading && <>
          <section className="space-y-5 border-b border-slate-100 p-6">
            <div><h2 className="text-lg font-extrabold text-slate-900">Provider and models</h2><p className="mt-1 text-sm text-slate-500">Model values are sent directly to Gemini, so preview or future model IDs can also be entered.</p></div>
            <div className="grid gap-5 md:grid-cols-2">
              <Field label="Provider"><select value={form.providerName} onChange={(e) => update("providerName", e.target.value)} className="admin-input w-full rounded-xl border px-3 py-2.5 text-sm" disabled><option>Gemini</option></select></Field>
              <ModelPicker label="Primary model" required value={form.modelName} onChange={(value) => update("modelName", value)} />
              <ModelPicker label="Fallback model" allowEmpty value={form.fallbackModelName} onChange={(value) => update("fallbackModelName", value)} />
              <div className="grid grid-cols-2 gap-3">
                <Field label="Temperature"><input type="number" min="0" max="2" step="0.1" value={form.temperature} onChange={(e) => update("temperature", e.target.value)} className="admin-input w-full rounded-xl border px-3 py-2.5 text-sm" placeholder="Default" /></Field>
                <Field label="Max tokens"><input type="number" min="1" step="1" value={form.maxOutputTokens} onChange={(e) => update("maxOutputTokens", e.target.value)} className="admin-input w-full rounded-xl border px-3 py-2.5 text-sm" placeholder="Default" /></Field>
              </div>
            </div>
          </section>

          <section className="space-y-5 border-b border-slate-100 p-6">
            <div className="flex items-start gap-3"><div className="rounded-xl bg-pink-50 p-2.5 text-rose-700"><ShieldCheck className="h-5 w-5" /></div><div><h2 className="text-lg font-extrabold text-slate-900">API credential</h2><p className="mt-1 text-sm text-slate-500">The key is encrypted by the backend. It is never returned to this page after saving.</p></div></div>
            <Field label={config?.hasStoredApiKey ? `Replace key (${config.apiKeyHint || "configured"})` : "Set admin-managed API key"}>
              <div className="relative"><input type={showKey ? "text" : "password"} autoComplete="new-password" value={form.apiKey} disabled={form.clearApiKey} onChange={(e) => update("apiKey", e.target.value)} className="admin-input w-full rounded-xl border px-3 py-2.5 pr-11 text-sm disabled:opacity-50" placeholder={config?.hasStoredApiKey ? "Leave blank to keep the current key" : "Leave blank to use the environment key"} /><button type="button" onClick={() => setShowKey((value) => !value)} className="absolute right-2 top-1/2 -translate-y-1/2 rounded-lg p-2 text-slate-400 hover:bg-slate-100 hover:text-slate-700" aria-label={showKey ? "Hide API key" : "Show API key"}>{showKey ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}</button></div>
            </Field>
            {config?.hasStoredApiKey && <label className="flex cursor-pointer items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-3 text-sm text-amber-900"><input type="checkbox" checked={form.clearApiKey} onChange={(e) => { update("clearApiKey", e.target.checked); if (e.target.checked) update("apiKey", ""); }} className="mt-0.5 h-4 w-4" /><span><strong>Remove the stored key</strong><br /><span className="text-xs text-amber-700">The service will fall back to `GeminiSettings__ApiKey` from its environment.</span></span></label>}
          </section>

          <section className="space-y-3 p-6"><Field label="System prompt override"><textarea rows={8} value={form.systemPromptTemplate} onChange={(e) => update("systemPromptTemplate", e.target.value)} className="admin-input w-full resize-y rounded-xl border px-3 py-2.5 font-mono text-xs leading-5" placeholder="Leave blank to use the built-in IELTS grading prompt." /></Field></section>
          <footer className="flex flex-col items-start justify-between gap-3 border-t border-slate-100 bg-slate-50/50 px-6 py-4 sm:flex-row sm:items-center"><p className="text-xs font-medium text-slate-500">Saving creates an audit-history row and activates it immediately.</p><button type="submit" disabled={saving} className="admin-primary-button flex min-w-40 items-center justify-center gap-2 rounded-xl px-5 py-2.5 text-sm font-bold disabled:opacity-50">{saving ? <Loader2 className="h-4 w-4 animate-spin" /> : <Save className="h-4 w-4" />}{saving ? "Activating…" : "Save and activate"}</button></footer>
        </>}
      </form>
    </div>
  );
}

function Field({ label, required, children }: { label: string; required?: boolean; children: ReactNode }) {
  return <label className="block"><span className="mb-1.5 block text-xs font-extrabold uppercase tracking-wider text-slate-600">{label}{required && <span className="ml-1 text-red-500">*</span>}</span>{children}</label>;
}

function ModelPicker({
  label,
  value,
  onChange,
  required = false,
  allowEmpty = false,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  required?: boolean;
  allowEmpty?: boolean;
}) {
  const isKnownModel = geminiModels.some((model) => model.id === value);
  const selectedValue = value === "" && allowEmpty
    ? ""
    : isKnownModel
      ? value
      : customModelValue;

  const selectModel = (nextValue: string) => {
    if (nextValue === customModelValue) {
      if (isKnownModel || (allowEmpty && value === "")) onChange("");
      return;
    }
    onChange(nextValue);
  };

  return (
    <Field label={label} required={required}>
      <div className="space-y-2">
        <select
          value={selectedValue}
          onChange={(event) => selectModel(event.target.value)}
          className="admin-input w-full rounded-xl border px-3 py-2.5 text-sm"
          required={required && selectedValue !== customModelValue}
        >
          {allowEmpty && <option value="">No fallback</option>}
          {geminiModels.map((model) => (
            <option key={model.id} value={model.id}>{model.label} — {model.id}</option>
          ))}
          <option value={customModelValue}>Custom model ID…</option>
        </select>
        {selectedValue === customModelValue && (
          <input
            required={required}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            className="admin-input w-full rounded-xl border px-3 py-2.5 font-mono text-sm"
            placeholder="gemini-model-id"
            aria-label={`${label} custom model ID`}
          />
        )}
      </div>
    </Field>
  );
}

function StatusCard({ label, value, icon }: { label: string; value: string; icon: ReactNode }) {
  return <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"><div className="flex items-center gap-3"><div className="rounded-xl bg-pink-50 p-2.5 text-rose-700">{icon}</div><div className="min-w-0"><p className="text-[10px] font-extrabold uppercase tracking-widest text-slate-400">{label}</p><p className="truncate text-sm font-extrabold text-slate-900" title={value}>{value}</p></div></div></div>;
}
