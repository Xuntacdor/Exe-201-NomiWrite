import fs from "node:fs";
import vm from "node:vm";
import assert from "node:assert/strict";
import ts from "typescript";

const source = fs.readFileSync(new URL("../lib/writing-activity.ts", import.meta.url), "utf8");
const context = { exports: {} };
vm.runInNewContext(ts.transpile(source, { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2020 }), context);
const { countWritingDays, localDayKey, monthDays } = context.exports;
const date = new Date(2026, 8, 27, 23, 30);
const row = { id: "one", status: "graded", submittedAt: date.toISOString() };
const result = countWritingDays([
  row, row,
  { ...row, id: "two", status: "submitted" },
  { ...row, id: "draft", status: "draft" },
  { ...row, id: "bad", submittedAt: "invalid" },
]);
assert.equal(result[localDayKey(date)], 2);
assert.equal(Object.keys(result).length, 1);
assert.equal(monthDays(2024, 1).filter(Boolean).length, 29);
assert.equal(monthDays(2025, 1).filter(Boolean).length, 28);
assert.equal(monthDays(2026, 8)[0], null);
assert.equal(monthDays(2026, 8)[1].getDate(), 1);
assert.equal(monthDays(2026, 10).length, 42);
assert.equal(monthDays(2026, 12).find(Boolean).getFullYear(), 2027);
assert.equal(Object.keys(countWritingDays([])).length, 0);
console.log("PASS: activity counts, drafts, duplicates, invalid dates, leap years, Monday alignment and year rollover.");
