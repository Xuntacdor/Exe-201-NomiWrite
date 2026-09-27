import fs from "node:fs";
import path from "node:path";
import vm from "node:vm";
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import ts from "typescript";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const catalog = JSON.parse(fs.readFileSync(path.join(root, "lib/i18n/messages.json"), "utf8"));
const code = ts.transpileModule(fs.readFileSync(path.join(root, "lib/i18n/locale.tsx"), "utf8"), {
  compilerOptions: { module: ts.ModuleKind.CommonJS, jsx: ts.JsxEmit.ReactJSX, target: ts.ScriptTarget.ES2020 },
}).outputText;

function environment(saved = null, blocked = false, server = false) {
  let cleanup, selected = "vi";
  const events = new Map();
  const context = {
    exports: {}, document: { documentElement: { lang: "vi" } },
    Event: class { constructor(type) { this.type = type; } },
    localStorage: {
      getItem() { if (blocked) throw Error("blocked"); return saved; },
      setItem(key, value) { assert.equal(key, "nomiwrite_locale"); if (blocked) throw Error("blocked"); saved = value; },
    },
    window: {
      addEventListener(type, callback) { if (!events.has(type)) events.set(type, new Set()); events.get(type).add(callback); },
      removeEventListener(type, callback) { events.get(type)?.delete(callback); },
      dispatchEvent(event) { events.get(event.type)?.forEach(callback => callback(event)); },
    },
    require(name) {
      if (name === "./messages.json") return { default: catalog };
      if (name === "react/jsx-runtime") return { jsx: (tag, props) => ({ tag, props }) };
      if (name === "react") return {
        createContext: () => ({ Provider: "provider" }),
        useContext: () => selected,
        useEffect: effect => { if (!server) effect(); },
        useSyncExternalStore(subscribe, getSnapshot, getServerSnapshot) {
          cleanup?.();
          if (!server) cleanup = subscribe(() => {});
          selected = server ? getServerSnapshot() : getSnapshot();
          return selected;
        },
      };
      throw Error(name);
    },
  };
  vm.runInNewContext(code, context);
  return { ...context, stored: () => saved, setStored: value => { saved = value; }, events, cleanup: () => cleanup?.() };
}
for (const [saved, expected] of [[null, "vi"], ["en", "en"], ["vi", "vi"], ["invalid", "vi"]]) {
  const env = environment(saved);
  const child = { editor: "Unsaved English essay", selection: 12 };
  assert.equal(env.exports.LocaleProvider({ children: child }).props.value, expected);
  env.exports.setLocale("en");
  const tree = env.exports.LocaleProvider({ children: child });
  assert.equal(tree.props.children, child, "Switching language must keep the same child tree");
  assert.equal(env.document.documentElement.lang, "en");
  assert.equal(env.stored(), "en");
  env.setStored("vi");
  env.window.dispatchEvent({ type: "storage", key: "nomiwrite_locale" });
  assert.equal(env.exports.LocaleProvider({ children: child }).props.value, "vi");
  env.cleanup();
  for (const listeners of env.events.values()) assert.equal(listeners.size, 0);
}
const blocked = environment(null, true);
blocked.exports.setLocale("en");
assert.equal(blocked.exports.LocaleProvider({ children: null }).props.value, "en");
assert.equal(environment("en", false, true).exports.LocaleProvider({ children: null }).props.value, "vi", "SSR snapshot stays deterministic");
const { translate } = environment().exports;
assert.equal(translate("Writing Task", "vi"), "Loại bài viết");
assert.equal(translate("Bài viết của bạn", "en"), "Your essay");
assert.equal(translate("Target of {target} words reached!", "vi", { target: 250 }), "Đã đạt mục tiêu 250 từ!");
assert.equal(translate("A learner's original English paragraph.", "vi"), "A learner's original English paragraph.");
assert.equal(translate("constructor", "vi"), "constructor");
assert.equal(translate("toString", "en"), "toString");
assert.equal(translate(" Welcome back, ", "vi"), " Chào mừng trở lại, ");
for (const [key, pair] of Object.entries(catalog)) {
  assert.ok(pair.en && pair.vi, `Missing translation: ${key}`);
  assert.ok(!/[ạảấầẩẫậắằẳẵặẹẻẽếềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳỵỷỹđ]/i.test(pair.en), `Vietnamese in English copy: ${key}`);
  assert.ok(!/[a-z]\?[a-z]|\?\?/.test(pair.vi), `Broken Vietnamese encoding: ${key}`);
  assert.deepEqual(pair.en.match(/\{\w+\}/g)?.sort() ?? [], pair.vi.match(/\{\w+\}/g)?.sort() ?? [], `Placeholder mismatch: ${key}`);
}
// Prevent translation calls from accidentally changing exercise or learner data.
for (const file of ["write/page.tsx", "quiz/page.tsx", "vocabulary/page.tsx", "result/page.tsx", "guide/page.tsx"]) {
  const source = fs.readFileSync(path.join(root, "app", file), "utf8");
  assert.ok(!/translateUi\((?:question\.(?:question|sentence|correctAnswer|explanation)|word\.(?:originalWord|suggestedWord|exampleSentence)|currentPrompt\.(?:topic|prompt)|content)\)/.test(source), `Exercise translated: ${file}`);
}
console.log("PASS: bilingual catalog, interpolation, language persistence, storage sync, blocked storage, SSR snapshot, unchanged children and protected practice content (simulated DOM).");
