import fs from 'node:fs';
import vm from 'node:vm';
import assert from 'node:assert/strict';
import ts from 'typescript';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
const frontend = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const compile = file => ts.transpileModule(fs.readFileSync(file, 'utf8'), {
  compilerOptions: { module: ts.ModuleKind.CommonJS, jsx: ts.JsxEmit.ReactJSX, target: ts.ScriptTarget.ES2020 }
}).outputText;
const boot = { exports: {} };
vm.runInNewContext(compile(`${frontend}/lib/theme-script.ts`), boot);

function environment({ system = false, saved = null, blocked = false } = {}) {
  const document = { documentElement: { dataset: {} }, activeElement: null };
  const media = { matches: system, listeners: new Set(), addEventListener(_, cb) { this.listeners.add(cb); }, removeEventListener(_, cb) { this.listeners.delete(cb); } };
  const events = new Map();
  const window = { matchMedia: () => media,
    addEventListener(type, cb) { if (!events.has(type)) events.set(type, new Set()); events.get(type).add(cb); },
    removeEventListener(type, cb) { events.get(type)?.delete(cb); },
    dispatchEvent(event) { events.get(event.type)?.forEach(cb => cb(event)); }
  };
  const localStorage = { getItem() { if (blocked) throw Error('blocked'); return saved; }, setItem(_, value) { if (blocked) throw Error('blocked'); saved = value; } };
  const state = { document, window, localStorage, Event: class { constructor(type) { this.type = type; } }, exports: {} };
  let cleanup;
  state.require = name => {
    if (name === 'react') return { useSyncExternalStore(subscribe, snapshot) { cleanup?.(); cleanup = subscribe(() => {}); return snapshot(); } };
    if (name === 'lucide-react') return { Sun: 'sun', Moon: 'moon' };
    if (name === 'react/jsx-runtime') return { jsx: (tag, props) => ({ tag, props }), jsxs: (tag, props) => ({ tag, props }) };
    throw Error(name);
  };
  vm.runInNewContext(boot.exports.themeScript, state);
  vm.runInNewContext(compile(`${frontend}/app/components/ThemeToggle.tsx`), state);
  return { ...state, media, saved: () => saved, setSaved: value => { saved = value; }, cleanup: () => cleanup?.() };
}
for (const [args, expected] of [[{system:true},'dark'], [{system:false},'light'], [{system:true,saved:'light'},'light'], [{system:false,saved:'dark'},'dark'], [{system:true,saved:'invalid'},'dark'], [{system:true,blocked:true},'dark']]) {
  assert.equal(environment(args).document.documentElement.dataset.theme, expected);
}
const env = environment();
let button = env.exports.default({});
env.media.matches = true; env.media.listeners.forEach(cb => cb());
assert.equal(env.document.documentElement.dataset.theme, 'dark');
button = env.exports.default({});
button.props.onClick();
assert.equal(env.saved(), 'light');
assert.equal(env.document.documentElement.dataset.theme, 'light');
assert.equal(environment({system:true,saved:env.saved()}).document.documentElement.dataset.theme, 'light');
env.media.listeners.forEach(cb => cb());
assert.equal(env.document.documentElement.dataset.theme, 'light');
env.setSaved('dark'); env.window.dispatchEvent({type:'storage',key:'nomiwrite_theme'});
assert.equal(env.document.documentElement.dataset.theme, 'dark');
const editor = { value:'Draft remains untouched', selectionStart:7, selectionEnd:7, matches: () => true };
env.document.activeElement = editor;
button = env.exports.default({collapsed:true});
let prevented = false;
button.props.onPointerDown({button:0,preventDefault(){prevented=true;}});
button.props.onClick();
assert.equal(prevented,true);
assert.equal(editor.value,'Draft remains untouched');
assert.equal(editor.selectionStart,7);
assert.equal(env.document.activeElement,editor);
assert.ok(button.props['aria-label']);
env.cleanup(); assert.equal(env.media.listeners.size,0);
const blocked = environment({blocked:true});
blocked.exports.default({}).props.onClick();
assert.equal(blocked.document.documentElement.dataset.theme,'dark');
assert.equal(blocked.exports.default({}).props['aria-pressed'],true);
console.log('PASS: theme boot, explicit preference, system changes, storage events, blocked storage, listener cleanup, editor pointer preservation (simulated DOM).');
