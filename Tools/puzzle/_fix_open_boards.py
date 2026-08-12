#!/usr/bin/env python3
"""Aperta L21/S02/S05 (tabuleiros abertos demais estouravam o solver)."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
p = ROOT / "Docs/level-layouts.json"
d = json.loads(p.read_text(encoding="utf-8"))
by = {l["id"]: l for l in d["levels"]}

by["TW08_Level21_LockedWorkshop"]["rows"] = [
    "##########",
    "# h ~~.  #",
    "#@$ ~~ . #",
    "# h ~~.  #",
    "# $ ~~ . #",
    "##########",
]
by["TW08_Level21_LockedWorkshop"]["goalRequirements"] = [
    {"x": 6, "y": 4, "kind": "HeavyCrate"},
    {"x": 6, "y": 2, "kind": "HeavyCrate"},
]

by["TW08_Secret02_RobertsRoom"]["rows"] = [
    "##########",
    "# h ~~ . #",
    "#@$ ~~.  #",
    "# h ~~ . #",
    "# $ ~~.  #",
    "##########",
]
by["TW08_Secret02_RobertsRoom"]["goalRequirements"] = [
    {"x": 7, "y": 4, "kind": "HeavyCrate"},
    {"x": 7, "y": 2, "kind": "HeavyCrate"},
]

by["TW08_Secret05_DarkWorkshop"]["rows"] = [
    "#########",
    "#       #",
    "# $  #. #",
    "#@$   . #",
    "# $  #. #",
    "# $   . #",
    "#       #",
    "#########",
]

p.write_text(json.dumps(d, indent=2, ensure_ascii=False), encoding="utf-8")
print("ok")
