import fs from "node:fs";
import path from "node:path";
import vm from "node:vm";
import assert from "node:assert/strict";
import { fileURLToPath } from "node:url";
import ts from "typescript";

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const source = ts.createSourceFile("write.tsx", fs.readFileSync(path.join(root, "app/write/page.tsx"), "utf8"), ts.ScriptTarget.Latest, true, ts.ScriptKind.TSX);
let loadEffect, saveEffect;
function visit(node) {
  if (ts.isCallExpression(node) && node.expression.getText(source) === "useEffect") {
    const callback = node.arguments[0]?.getText(source);
    if (callback?.includes("localStorage.getItem(draftKey)")) loadEffect = callback;
    if (callback?.includes("localStorage.setItem(draftKey, content)")) saveEffect = callback;
  }
  ts.forEachChild(node, visit);
}
visit(source);
assert.ok(loadEffect && saveEffect);
const data = new Map([["prompt-a", "Saved essay A"], ["prompt-b", "Saved essay B"]]);
const timers = new Map();
let timerId = 0, blocked = false;
const state = {
  draftKey: "prompt-a", loadedDraftKey: null, content: "", hasCustomTopic: false,
  customPrompt: "", customHints: [], customVocab: "",
  setContent(value) { state.content = value; },
  setLoadedDraftKey(key) { state.loadedDraftKey = key; },
  setTimeout(callback) { timers.set(++timerId, callback); return timerId; },
  clearTimeout(id) { timers.delete(id); },
  localStorage: {
    getItem(key) { if (blocked) throw Error("blocked"); return data.get(key) ?? null; },
    setItem(key, value) { if (blocked) throw Error("blocked"); data.set(key, value); },
  },
};
vm.createContext(state);
const run = expression => vm.runInContext(`(${expression})()`, state);
function flushTimers() { for (const [id, callback] of timers) { timers.delete(id); callback(); } }

// React runs both effects before the deferred load. Saving must not erase storage.
run(loadEffect); run(saveEffect);
assert.equal(data.get("prompt-a"), "Saved essay A");
flushTimers(); run(saveEffect);
assert.equal(state.content, "Saved essay A");
state.content = "Updated essay A"; run(saveEffect);
assert.equal(data.get("prompt-a"), "Updated essay A");
state.draftKey = "prompt-b";
run(loadEffect); run(saveEffect);
assert.equal(data.get("prompt-b"), "Saved essay B", "Switching prompts must not copy essay A into B");
flushTimers(); run(saveEffect);
assert.equal(state.content, "Saved essay B");
assert.equal(data.get("prompt-a"), "Updated essay A");
state.draftKey = "prompt-c";
run(loadEffect); run(saveEffect); flushTimers(); run(saveEffect);
assert.equal(state.content, "");
assert.equal(data.get("prompt-c"), "");
const cancel = run(loadEffect); cancel();
assert.equal(timers.size, 0);
blocked = true; state.draftKey = "blocked";
run(loadEffect); run(saveEffect); flushTimers();
assert.equal(state.loadedDraftKey, "blocked");
assert.doesNotThrow(() => run(saveEffect));
console.log("PASS: saved draft restoration, prompt isolation, edits, empty drafts, cleanup and blocked storage (actual page effects, simulated scheduler).");
