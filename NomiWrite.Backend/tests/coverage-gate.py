#!/usr/bin/env python3
"""Coverage gate for the NomiWrite unit-test suites (Application layer only).

Usage:
    python3 coverage-gate.py <merged-cobertura.xml> <coverage-baseline.json>

Computes per-service line coverage for the NomiWrite.<Service>.Application
assemblies from the merged Cobertura report and fails (exit 1) if any service
drops below its committed baseline. Baselines are a ratchet: the gate only
enforces no-regression, it does not enforce the aspirational targets in
TEST_PLAN.md §3.1 (tracked separately, raise baselines here as suites improve).
"""
import json
import sys
import xml.etree.ElementTree as ET

SERVICES = ["Auth", "Payment", "AICoordinator", "Learning",
            "Writing", "Subscription", "User", "Admin", "Notification"]


def main() -> int:
    if len(sys.argv) != 3:
        print(__doc__)
        return 2

    report_path, baseline_path = sys.argv[1], sys.argv[2]

    with open(baseline_path, encoding="utf-8") as fh:
        baseline = {k: float(v) for k, v in json.load(fh).items()}

    root = ET.parse(report_path).getroot()
    per_service = {s: [0, 0] for s in SERVICES}
    for cls in root.iter("class"):
        name = cls.get("name", "")
        bits = name.split(".")
        if len(bits) < 3 or bits[0] != "NomiWrite" or bits[1] not in per_service:
            continue
        if "Application" not in bits or "UnitTests" in bits:
            continue
        covered = valid = 0
        lines = cls.find("lines")
        for line in (list(lines) if lines is not None else []):
            valid += 1
            if int(line.get("hits", "0")) > 0:
                covered += 1
        per_service[bits[1]][0] += covered
        per_service[bits[1]][1] += valid

    failures = []
    print("Service        Covered/Valid   LineCoverage  Baseline  Status")
    for service in SERVICES:
        covered, valid = per_service[service]
        rate = covered / valid * 100 if valid else 0.0
        expect = baseline.get(service)
        if expect is None:
            failures.append(f"{service}: no baseline configured")
            status = "NO-BASELINE"
        elif rate + 1e-9 < expect:  # baselines already include headroom
            failures.append(f"{service}: coverage {rate:.1f}% < baseline {expect:.1f}%")
            status = "FAIL"
        else:
            status = "PASS"
        print(f"{service:14}     {covered:5}/{valid:5}   {rate:8.1f}%   {expect if expect is not None else -1:7.1f}%   {status}")

    if failures:
        print(f"\nFAILED ({len(failures)}):")
        for f in failures:
            print(f"  - {f}")
        return 1

    print("\nPASS")
    return 0


if __name__ == "__main__":
    sys.exit(main())