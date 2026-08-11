#!/usr/bin/env python3
"""Achata level-layouts.json em coordenadas explícitas + medalhas derivadas do solver.

Entrada:
  Docs/level-layouts.json      (rows ASCII, do level designer)
  Tools/puzzle/report-new.json (optimalCost por id, do solver)
  Docs/level-specs.json        (powerUps/sector/nomes, do game designer)

Saída:
  Docs/level-layouts-enriched.json — consumido pelo TW08CampaignExpansionImporter (C#).
  Formato 100% explícito (sem chars): o C# não precisa entender a linguagem ASCII.

Política de medalhas (movimentos, custo real do engine — convenção do projeto):
  platinum = optimalCost          (jogo perfeito)
  gold     = max(opt+2, ceil(optimalCost * 1.3))
"""
from __future__ import annotations

import json
import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from tw08_solver import parse_layout, KIND_NAMES  # noqa: E402

ROOT = Path(__file__).resolve().parents[2]


def main() -> int:
    layouts = json.loads((ROOT / "Docs/level-layouts.json").read_text(encoding="utf-8"))
    report = json.loads((ROOT / "Tools/puzzle/report-new.json").read_text(encoding="utf-8"))
    specs = json.loads((ROOT / "Docs/level-specs.json").read_text(encoding="utf-8"))

    by_id_report = {e["id"]: e for e in report["levels"]}
    by_id_spec = {e["id"]: e for e in specs["levels"]}

    out_levels = []
    errors = []
    for obj in layouts["levels"]:
        lid = obj["id"]
        rep = by_id_report.get(lid)
        spec = by_id_spec.get(lid, {})
        if rep is None or not rep.get("solvable"):
            errors.append(f"{lid}: sem prova de solvabilidade no report")
            continue
        if not rep.get("replayOk"):
            errors.append(f"{lid}: replay falhou")
            continue

        lv = parse_layout(obj)
        opt = int(rep["optimalCost"])
        # Convenção do projeto (auditada nas 9 fases existentes):
        # platinum = ótimo exato; gold ≈ 30% de folga.
        platinum = opt
        gold = max(opt + 2, math.ceil(opt * 1.3))

        power_ups_raw = str(spec.get("powerUps", "")).lower()
        allow_power_ups = "bloquead" not in power_ups_raw

        # Falas do spec entram no briefing (exibido no HUD da fase).
        briefing = obj.get("briefing") or spec.get("briefing", "")
        narrative = spec.get("narrative") or []
        if narrative:
            falas = "  ".join(
                f"{n['speaker']}: “{n['line']}”" for n in narrative[:2]
            )
            briefing = f"{briefing}\n{falas}" if briefing else falas

        out_levels.append({
            "id": lid,
            "displayName": obj.get("displayName") or spec.get("displayName", lid),
            "sectorId": obj.get("sectorId") or spec.get("sectorId", "S01"),
            "briefing": briefing,
            "gimmickTags": obj.get("gimmickTags") or spec.get("gimmickTags", []),
            "kind": spec.get("kind", "main"),
            "specIndex": spec.get("index", 0),
            "width": lv.width,
            "height": lv.height,
            "player": {"x": lv.player[0], "y": lv.player[1]},
            "walls": [{"x": x, "y": y} for (x, y) in sorted(lv.walls)],
            "goals": [{"x": x, "y": y} for (x, y) in sorted(lv.goals)],
            "costly": [{"x": x, "y": y} for (x, y) in sorted(lv.costly)],
            "crates": [
                {"x": x, "y": y, "kind": KIND_NAMES[k]}
                for (x, y), k in sorted(lv.crates.items())
            ],
            "goalRequirements": [
                {"x": x, "y": y, "kind": KIND_NAMES[k]}
                for (x, y), k in sorted(lv.goal_req.items())
            ],
            "switchGroups": [
                {
                    "id": g.gid,
                    "sensors": [{"x": x, "y": y} for (x, y) in g.sensors],
                    "doors": [{"x": x, "y": y} for (x, y) in g.doors],
                }
                for g in lv.groups
            ],
            "optimalCost": opt,
            "optimalPushes": int(rep.get("pushes", 0)),
            "goldMoveLimit": gold,
            "platinumMoveLimit": platinum,
            "allowPowerUps": allow_power_ups,
        })

    if errors:
        for e in errors:
            print("ERRO:", e, file=sys.stderr)
        return 1

    out = {"levels": out_levels}
    dest = ROOT / "Docs/level-layouts-enriched.json"
    dest.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    mains = sum(1 for e in out_levels if e["kind"] == "main")
    print(f"OK: {len(out_levels)} fases ({mains} main, {len(out_levels)-mains} secret) -> {dest}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
