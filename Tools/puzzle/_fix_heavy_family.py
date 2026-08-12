#!/usr/bin/env python3
"""Encolhe a família L25 (fases de sensor com 5-6 caixas) de w12 para w10."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
p = ROOT / "Docs/level-layouts.json"
d = json.loads(p.read_text(encoding="utf-8"))
by = {l["id"]: l for l in d["levels"]}


def setrows(lid, rows, extra_goals, reqs):
    lv = by[lid]
    lv["rows"] = rows
    lv["extraGoals"] = extra_goals
    lv["goalRequirements"] = reqs


H = "HeavyCrate"

setrows("TW08_Level25_DeadWeight", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$$~ A..#",
    "# h ~1  ##",
    "# $  .  ##",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Level29_LockdownN8", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$$~AA..#",
    "# h ~1  ##",
    "# $   . ##",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Level30_LogisticsCore", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$$~ A..#",
    "# h ~1  ##",
    "# $ $ .. #",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Secret04_EliasRoute", [
    "##########",
    "#   ######",
    "# h  1####",
    "#@$$~ A..#",
    "# h  1  ##",
    "# $  .  ##",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Secret08_LeftoverMap", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$$$ A..#",
    "# h ~1  ##",
    "#    .  ##",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Secret09_EliasLastShift", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$ $ AA.#",
    "# h ~1  ##",
    "# $  .. ##",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

setrows("TW08_Secret10_DudasPath", [
    "##########",
    "#   ######",
    "# h ~1####",
    "#@$$~AA..#",
    "# h ~1  ##",
    "# $ $ .. #",
    "##########",
], [{"x": 5, "y": 4}, {"x": 5, "y": 2}],
   [{"x": 5, "y": 4, "kind": H}, {"x": 5, "y": 2, "kind": H}])

for l in d["levels"]:
    w = len(l["rows"][0])
    bad = [r for r in l["rows"] if len(r) != w]
    if bad:
        raise SystemExit(f"{l['id']}: largura inconsistente: {bad}")

p.write_text(json.dumps(d, indent=2, ensure_ascii=False), encoding="utf-8")
print("ok")
