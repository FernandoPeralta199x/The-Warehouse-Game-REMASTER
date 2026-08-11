#!/usr/bin/env python3
"""TW08 Sokoban solver — espelha exatamente a semântica de PuzzleBoardModel.cs.

Regras espelhadas (fonte: Assets/_Project/Scripts/Puzzle/):
- TryMove: passo ortogonal; custo = 2 se célula de DESTINO do jogador é costly, senão 1.
  Empurrão: caixa precisa cair em célula livre (dentro, sem parede/porta/caixa).
- Portas (switch groups, PuzzleRuntime.ApplySwitchGroups): grupo "aberto" se TODOS os
  sensores têm caixa em cima OU se alguma porta do grupo está ocupada (jogador/caixa)
  no momento do fechamento — transição atômica. Estado derivado de (player, crates).
- Completion (EvaluateCompletion): len(goals) == len(crates), toda goal ocupada,
  goalRequirements exigem kind específico da caixa.

Uso:
  python tw08_solver.py --assets  <dir com *.asset PuzzleLevelDefinition>
  python tw08_solver.py --layouts <layouts.json>
  Saída: relatório JSON em stdout (ou --out arquivo).
"""
from __future__ import annotations

import argparse
import heapq
import json
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path

DIRS = {"U": (0, 1), "D": (0, -1), "L": (-1, 0), "R": (1, 0)}

KIND_NAMES = {0: "Player", 1: "Crate", 2: "HeavyCrate", 3: "FragileCrate"}
KIND_IDS = {v: k for k, v in KIND_NAMES.items()}


@dataclass
class SwitchGroup:
    gid: str
    sensors: tuple
    doors: tuple


@dataclass
class Level:
    level_id: str
    width: int
    height: int
    player: tuple
    walls: frozenset
    goals: frozenset
    crates: dict          # (x,y) -> kind:int
    costly: frozenset
    goal_req: dict        # (x,y) -> kind:int
    groups: list = field(default_factory=list)
    display_name: str = ""


# ---------------------------------------------------------------- Unity YAML --

_COORD_RE = re.compile(r"^\s*-?\s*x:\s*(-?\d+)\s*$")


def _parse_coord_list(lines, i):
    """Parseia lista de GridCoordinate no YAML da Unity a partir da linha i."""
    coords = []
    n = len(lines)
    while i < n:
        line = lines[i]
        m = re.match(r"^\s*-\s*x:\s*(-?\d+)", line)
        if m:
            x = int(m.group(1))
            m2 = re.match(r"^\s*y:\s*(-?\d+)", lines[i + 1])
            if not m2:
                break
            coords.append((x, int(m2.group(1))))
            i += 2
            continue
        break
    return coords, i


def parse_unity_asset(path: Path) -> Level | None:
    text = path.read_text(encoding="utf-8", errors="replace")
    if "PuzzleLevelDefinition" not in text:
        return None
    lines = text.splitlines()
    n = len(lines)

    def find_scalar(key, cast=str, default=None):
        for ln in lines:
            m = re.match(rf"^\s*{key}:\s*(.+?)\s*$", ln)
            if m:
                v = m.group(1).strip().strip("'\"")
                try:
                    return cast(v)
                except ValueError:
                    return default
        return default

    def find_block(key):
        for idx, ln in enumerate(lines):
            if re.match(rf"^\s*{key}:\s*$", ln) or re.match(rf"^\s*{key}:\s*\[\]", ln):
                return idx
        return -1

    width = find_scalar("width", int, 8)
    height = find_scalar("height", int, 6)
    level_id = find_scalar("levelId", str, path.stem)
    display = find_scalar("displayName", str, "") or ""

    # playerStart
    px = py = 1
    for idx, ln in enumerate(lines):
        if re.match(r"^\s*playerStart:\s*$", ln):
            mx = re.match(r"^\s*x:\s*(-?\d+)", lines[idx + 1])
            my = re.match(r"^\s*y:\s*(-?\d+)", lines[idx + 2])
            if mx and my:
                px, py = int(mx.group(1)), int(my.group(1))
            break

    def coord_list(key):
        idx = find_block(key)
        if idx < 0 or "[]" in lines[idx]:
            return []
        coords, _ = _parse_coord_list(lines, idx + 1)
        return coords

    walls = coord_list("walls")
    goals = coord_list("goals")
    costly = coord_list("costlyCells")

    # crates: lista de objetos {id, kind, position:{x,y}}
    crates = {}
    idx = find_block("crates")
    if idx >= 0 and "[]" not in lines[idx]:
        i = idx + 1
        cur_kind = 1
        while i < n:
            ln = lines[i]
            if re.match(r"^\s*-\s*id:", ln) or re.match(r"^\s*-\s*crateId:", ln):
                cur_kind = 1
                i += 1
                continue
            mk = re.match(r"^\s*kind:\s*(\d+)", ln)
            if mk:
                cur_kind = int(mk.group(1))
                i += 1
                continue
            if re.match(r"^\s*position:\s*$", ln):
                mx = re.match(r"^\s*x:\s*(-?\d+)", lines[i + 1])
                my = re.match(r"^\s*y:\s*(-?\d+)", lines[i + 2])
                if mx and my:
                    crates[(int(mx.group(1)), int(my.group(1)))] = cur_kind
                i += 3
                continue
            if re.match(r"^\s*(costlyCells|switchGroups|goldMoveLimit|goalRequirements):", ln):
                break
            i += 1

    # goalRequirements: {position:{x,y}, requiredKind}
    goal_req = {}
    idx = find_block("goalRequirements")
    if idx >= 0 and "[]" not in lines[idx]:
        i = idx + 1
        pos = None
        while i < n:
            ln = lines[i]
            if re.match(r"^\s*position:\s*$", ln):
                mx = re.match(r"^\s*x:\s*(-?\d+)", lines[i + 1])
                my = re.match(r"^\s*y:\s*(-?\d+)", lines[i + 2])
                if mx and my:
                    pos = (int(mx.group(1)), int(my.group(1)))
                i += 3
                continue
            mk = re.match(r"^\s*requiredKind:\s*(\d+)", ln)
            if mk and pos is not None:
                goal_req[pos] = int(mk.group(1))
                pos = None
                i += 1
                continue
            if re.match(r"^\s*(crates|costlyCells|switchGroups|goldMoveLimit):", ln):
                break
            i += 1

    # switchGroups: {id, sensors:[...], doors:[...]}
    groups = []
    idx = find_block("switchGroups")
    if idx >= 0 and "[]" not in lines[idx]:
        i = idx + 1
        cur = None
        while i < n:
            ln = lines[i]
            mid = re.match(r"^\s*-\s*id:\s*(.+?)\s*$", ln)
            if mid:
                if cur:
                    groups.append(cur)
                cur = {"id": mid.group(1).strip("'\""), "sensors": [], "doors": []}
                i += 1
                continue
            if cur is not None and re.match(r"^\s*sensors:\s*$", ln):
                coords, i2 = _parse_coord_list(lines, i + 1)
                cur["sensors"] = coords
                i = i2
                continue
            if cur is not None and re.match(r"^\s*doors:\s*$", ln):
                coords, i2 = _parse_coord_list(lines, i + 1)
                cur["doors"] = coords
                i = i2
                continue
            if re.match(r"^\s*(goldMoveLimit|platinumMoveLimit|allowPowerUps):", ln):
                break
            i += 1
        if cur:
            groups.append(cur)

    return Level(
        level_id=level_id,
        width=width,
        height=height,
        player=(px, py),
        walls=frozenset(walls),
        goals=frozenset(goals),
        crates=dict(crates),
        costly=frozenset(costly),
        goal_req=dict(goal_req),
        groups=[SwitchGroup(g["id"], tuple(g["sensors"]), tuple(g["doors"])) for g in groups],
        display_name=display,
    )


# -------------------------------------------------------------- Layout JSON --

CHAR_KIND = {"$": 1, "*": 1, "h": 2, "H": 2, "f": 3, "F": 3}


def parse_layout(obj: dict) -> Level:
    rows = obj["rows"]
    height = len(rows)
    width = max(len(r) for r in rows)
    walls, goals, costly = set(), set(), set()
    crates: dict = {}
    player = None
    sensors: dict = {}
    doors: dict = {}

    for ri, row in enumerate(rows):
        y = height - 1 - ri  # rows[0] = topo visual
        for x, ch in enumerate(row):
            if ch == "#":
                walls.add((x, y))
            elif ch == "@":
                player = (x, y)
            elif ch == "+":
                player = (x, y)
                goals.add((x, y))
            elif ch == ".":
                goals.add((x, y))
            elif ch == "~":
                costly.add((x, y))
            elif ch == ":":
                costly.add((x, y))
                goals.add((x, y))
            elif ch in "$hf":
                crates[(x, y)] = CHAR_KIND[ch]
            elif ch in "*HF":
                crates[(x, y)] = CHAR_KIND[ch]
                goals.add((x, y))
            elif ch in "1234":
                sensors.setdefault(ch, []).append((x, y))
            elif ch in "ABCD":
                doors.setdefault(ch, []).append((x, y))

    groups = []
    for digit, letter in zip("1234", "ABCD"):
        if digit in sensors or letter in doors:
            groups.append(SwitchGroup(
                f"sg-{letter.lower()}",
                tuple(sensors.get(digit, [])),
                tuple(doors.get(letter, [])),
            ))

    for g in obj.get("switchGroups", []):
        groups.append(SwitchGroup(
            g["id"], tuple(map(tuple, g["sensors"])), tuple(map(tuple, g["doors"]))))

    goal_req = {}
    for r in obj.get("goalRequirements", []):
        goal_req[(r["x"], r["y"])] = KIND_IDS[r["kind"]] if isinstance(r["kind"], str) else int(r["kind"])

    for c in obj.get("extraCrates", []):
        crates[(c["x"], c["y"])] = KIND_IDS.get(c.get("kind", "Crate"), 1)
    for c in obj.get("extraCostly", []):
        costly.add((c["x"], c["y"]))

    if player is None:
        raise ValueError(f"{obj.get('id')}: layout sem jogador (@)")

    return Level(
        level_id=obj["id"],
        width=width,
        height=height,
        player=player,
        walls=frozenset(walls),
        goals=frozenset(goals),
        crates=crates,
        costly=frozenset(costly),
        goal_req=goal_req,
        groups=groups,
        display_name=obj.get("displayName", ""),
    )


# ------------------------------------------------------------------- Solver --

def door_state(level: Level, player, crates):
    """Retorna frozenset de células de porta FECHADAS (bloqueadas).

    Espelha PuzzleRuntime.ApplySwitchGroups: grupo abre se todos os sensores têm
    caixa; se deveria fechar mas alguma porta está ocupada (player/caixa), o grupo
    inteiro permanece aberto (transição atômica).
    """
    closed = set()
    for g in level.groups:
        requested_open = len(g.sensors) > 0 and all(s in crates for s in g.sensors)
        if requested_open:
            continue
        if any(d == player or d in crates for d in g.doors):
            continue  # grupo fica aberto
        closed.update(g.doors)
    return frozenset(closed)


def is_complete(level: Level, crates):
    if len(level.goals) == 0 or len(level.goals) != len(crates):
        return False
    for goal in level.goals:
        kind = crates.get(goal)
        if kind is None:
            return False
        req = level.goal_req.get(goal)
        if req is not None and kind != req:
            return False
    return True


def corner_deadlock(level: Level, crates):
    """Deadlock estático: caixa em canto de paredes fora de goal compatível."""
    for pos, kind in crates.items():
        if pos in level.goals:
            req = level.goal_req.get(pos)
            if req is None or req == kind:
                continue
        x, y = pos

        def solid(cx, cy):
            return (cx, cy) in level.walls or not (0 <= cx < level.width and 0 <= cy < level.height)

        if (solid(x - 1, y) or solid(x + 1, y)) and (solid(x, y - 1) or solid(x, y + 1)):
            return True
    return False


def solve(level: Level, max_states=2_000_000):
    start_crates = dict(level.crates)
    if is_complete(level, start_crates):
        return {"solvable": True, "optimalCost": 0, "pushes": 0, "solution": "", "states": 0}
    if len(level.goals) != len(start_crates):
        return {"solvable": False, "reason": f"goals={len(level.goals)} != crates={len(start_crates)}", "states": 0}

    def key(player, crates):
        return (player, tuple(sorted((p, k) for p, k in crates.items())))

    start_key = key(level.player, start_crates)
    dist = {start_key: 0}
    prev = {}
    heap = [(0, 0, start_key, level.player, tuple(sorted((p, k) for p, k in start_crates.items())))]
    counter = 0
    expanded = 0

    while heap:
        cost, _, k, player, crates_t = heapq.heappop(heap)
        if dist.get(k, 1 << 60) < cost:
            continue
        crates = dict(crates_t)
        expanded += 1
        if expanded > max_states:
            return {"solvable": False, "reason": "state-limit", "states": expanded}

        closed_doors = door_state(level, player, crates)

        for dname, (dx, dy) in DIRS.items():
            nx, ny = player[0] + dx, player[1] + dy
            npos = (nx, ny)
            if not (0 <= nx < level.width and 0 <= ny < level.height):
                continue
            if npos in level.walls or npos in closed_doors:
                continue
            step_cost = 2 if npos in level.costly else 1
            pushed = False
            new_crates = crates
            if npos in crates:
                cx, cy = nx + dx, ny + dy
                cpos = (cx, cy)
                if not (0 <= cx < level.width and 0 <= cy < level.height):
                    continue
                if cpos in level.walls or cpos in closed_doors or cpos in crates:
                    continue
                new_crates = dict(crates)
                new_crates[cpos] = new_crates.pop(npos)
                pushed = True
                if corner_deadlock(level, new_crates):
                    continue
            nk = key(npos, new_crates)
            ncost = cost + step_cost
            if ncost < dist.get(nk, 1 << 60):
                dist[nk] = ncost
                prev[nk] = (k, dname, pushed)
                if is_complete(level, new_crates):
                    # reconstruir
                    moves = []
                    pushes = 0
                    cur = nk
                    while cur != start_key:
                        pk, dn, pu = prev[cur]
                        moves.append(dn)
                        pushes += 1 if pu else 0
                        cur = pk
                    moves.reverse()
                    return {
                        "solvable": True,
                        "optimalCost": ncost,
                        "pushes": pushes,
                        "solution": "".join(moves),
                        "states": expanded,
                    }
                counter += 1
                heapq.heappush(heap, (ncost, counter, nk, npos,
                                      tuple(sorted((p, kk) for p, kk in new_crates.items()))))

    return {"solvable": False, "reason": "exhausted", "states": expanded}


def replay(level: Level, solution: str):
    """Reexecuta a solução validando cada passo — prova independente."""
    player = level.player
    crates = dict(level.crates)
    cost = 0
    for ch in solution:
        dx, dy = DIRS[ch]
        closed = door_state(level, player, crates)
        npos = (player[0] + dx, player[1] + dy)
        if not (0 <= npos[0] < level.width and 0 <= npos[1] < level.height):
            return False, f"fora do tabuleiro em {npos}"
        if npos in level.walls or npos in closed:
            return False, f"bloqueado em {npos}"
        if npos in crates:
            cpos = (npos[0] + dx, npos[1] + dy)
            if (cpos in level.walls or cpos in closed or cpos in crates
                    or not (0 <= cpos[0] < level.width and 0 <= cpos[1] < level.height)):
                return False, f"empurrão inválido para {cpos}"
            crates[cpos] = crates.pop(npos)
        cost += 2 if npos in level.costly else 1
        player = npos
    if not is_complete(level, crates):
        return False, "solução não completa o nível"
    return True, cost


# --------------------------------------------------------------------- Main --

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--assets", help="diretório com .asset PuzzleLevelDefinition")
    ap.add_argument("--layouts", help="arquivo layouts.json")
    ap.add_argument("--out", help="arquivo de saída (JSON)")
    args = ap.parse_args()

    levels: list[Level] = []
    if args.assets:
        for p in sorted(Path(args.assets).rglob("*.asset")):
            lv = parse_unity_asset(p)
            if lv:
                levels.append(lv)
    if args.layouts:
        data = json.loads(Path(args.layouts).read_text(encoding="utf-8"))
        for obj in data["levels"]:
            levels.append(parse_layout(obj))

    report = []
    for lv in levels:
        res = solve(lv)
        entry = {
            "id": lv.level_id,
            "displayName": lv.display_name,
            "board": f"{lv.width}x{lv.height}",
            "crates": len(lv.crates),
            "goals": len(lv.goals),
            **res,
        }
        if res.get("solvable") and res.get("solution"):
            ok, replay_cost = replay(lv, res["solution"])
            entry["replayOk"] = ok
            entry["replayCost"] = replay_cost
        report.append(entry)

    out = json.dumps({"levels": report}, indent=2, ensure_ascii=False)
    if args.out:
        Path(args.out).write_text(out, encoding="utf-8")
        solvable = sum(1 for e in report if e.get("solvable"))
        print(f"{solvable}/{len(report)} solucionáveis -> {args.out}")
    else:
        print(out)
    return 0 if all(e.get("solvable") for e in report) else 1


if __name__ == "__main__":
    sys.exit(main())
