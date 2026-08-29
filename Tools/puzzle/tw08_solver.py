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
import math
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
    ice: frozenset = frozenset()          # células de gelo
    conveyors: dict = field(default_factory=dict)  # (x,y) -> (dx,dy)
    patrols: tuple = ()   # tupla de rotas; cada rota é uma tupla de células
    buttons: frozenset = frozenset()   # botões que invertem as esteiras
    timed: tuple = ()     # ((x,y), abre_apos_comandos), ...)
    display_name: str = ""

    @property
    def patrol_period(self):
        """Passos até todos os robôs voltarem à posição inicial (mmc das rotas)."""
        period = 1
        for route in self.patrols:
            period = period * len(route) // math.gcd(period, len(route))
        return period

    def patrol_cells(self, step):
        return frozenset(route[step % len(route)] for route in self.patrols)

    @property
    def timed_horizon(self):
        """Maior prazo. Depois dele o relógio não muda mais nada no tabuleiro."""
        return max((deadline for _, deadline in self.timed), default=0)

    def closed_timed(self, commands):
        return frozenset(cell for cell, deadline in self.timed if commands < deadline)


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

# Gelo: "%". Esteiras: seta apontando para onde levam.
CONVEYOR_CHARS = {"^": (0, 1), "v": (0, -1), "<": (-1, 0), ">": (1, 0)}
CONVEYOR_NAMES = {"up": (0, 1), "down": (0, -1), "left": (-1, 0), "right": (1, 0)}


def parse_layout(obj: dict) -> Level:
    rows = obj["rows"]
    height = len(rows)
    width = max(len(r) for r in rows)
    walls, goals, costly, ice = set(), set(), set(), set()
    conveyors: dict = {}
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
            elif ch == "%":
                ice.add((x, y))
            elif ch == "?":
                # Parede falsa: o tabuleiro sempre a tratou como passagem livre,
                # a mentira é só visual. Nada a registrar aqui.
                pass
            elif ch in CONVEYOR_CHARS:
                conveyors[(x, y)] = CONVEYOR_CHARS[ch]
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
    for c in obj.get("extraGoals", []):
        # Permite alvo sobre célula de sensor/porta (chars não sobrepõem).
        goals.add((c["x"], c["y"]))
    for c in obj.get("extraIce", []):
        ice.add((c["x"], c["y"]))
    for c in obj.get("extraConveyors", []):
        conveyors[(c["x"], c["y"])] = CONVEYOR_NAMES[c["dir"]]

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
        ice=frozenset(ice),
        conveyors=conveyors,
        patrols=tuple(
            tuple((c["x"], c["y"]) for c in patrol["route"])
            for patrol in obj.get("patrols", []) if patrol.get("route")
        ),
        buttons=frozenset((b["x"], b["y"]) for b in obj.get("directionButtons", [])),
        timed=tuple(((b["x"], b["y"]), b["opensAfter"]) for b in obj.get("timedBlocks", [])),
        display_name=obj.get("displayName", ""),
    )


# ------------------------------------------------------------------- Solver --

def slide(level: Level, start, direction, crates, closed_doors, vacated=None,
          robots=frozenset(), inverted=False):
    """Espelha PuzzleBoardModel.Slide.

    Gelo mantém a direção de entrada; esteira impõe a própria. Segue até pisar
    em piso comum ou até a próxima célula estar ocupada. O teto de iterações
    protege contra esteiras em circuito fechado.
    """
    current = start
    guard = level.width * level.height

    while guard > 0:
        guard -= 1
        if current in level.conveyors:
            step = level.conveyors[current]
            if inverted:
                step = (-step[0], -step[1])
        elif current in level.ice:
            step = direction
        else:
            return current

        nxt = (current[0] + step[0], current[1] + step[1])
        if not (0 <= nxt[0] < level.width and 0 <= nxt[1] < level.height):
            return current
        if nxt in level.walls or nxt in closed_doors:
            return current
        if nxt != vacated and nxt in crates:
            return current
        if nxt in robots:
            return current

        current = nxt
        direction = step

    return current


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


class AllCells:
    """Aceita qualquer célula. Usado quando a poda estática não se aplica."""

    def __init__(self, level):
        self.level = level

    def __contains__(self, cell):
        return True


def compute_live_cells(level: Level) -> frozenset:
    """Células onde uma caixa ainda pode alcançar ALGUM goal (análise estática).

    Fecho reverso de empurrões ignorando outras caixas; portas tratadas como
    abertas (podem abrir). Sobre-aproximação segura: caixa em célula morta
    jamais termina em goal, e toda caixa precisa terminar em goal
    (EvaluateCompletion exige len(goals) == len(crates)).
    """
    def floor(c):
        x, y = c
        return 0 <= x < level.width and 0 <= y < level.height and c not in level.walls

    live = {g for g in level.goals if floor(g)}
    frontier = list(live)
    while frontier:
        x, y = frontier.pop()
        for dx, dy in DIRS.values():
            src = (x - dx, y - dy)        # caixa vinha daqui
            psrc = (x - 2 * dx, y - 2 * dy)  # jogador empurrava daqui
            if src not in live and floor(src) and floor(psrc):
                live.add(src)
                frontier.append(src)
    return frozenset(live)


def solve(level: Level, max_states=3_000_000):
    start_crates = dict(level.crates)
    if is_complete(level, start_crates):
        return {"solvable": True, "optimalCost": 0, "pushes": 0, "solution": "", "states": 0}
    if len(level.goals) != len(start_crates):
        return {"solvable": False, "reason": f"goals={len(level.goals)} != crates={len(start_crates)}", "states": 0}

    # A poda de células mortas assume empurrão de uma casa. Com gelo ou esteira
    # a carga percorre vários passos por comando e alcança células que o fecho
    # reverso simples marcaria como mortas — a poda descartaria soluções reais.
    if level.ice or level.conveyors:
        live = AllCells(level)
    else:
        live = compute_live_cells(level)
        dead_start = [p for p in start_crates if p not in live]
        if dead_start:
            return {"solvable": False, "reason": f"caixa inicial em célula morta: {dead_start}", "states": 0}

    # A*: h = soma das distâncias Manhattan de cada caixa ao goal compatível
    # mais próximo. Admissível (cada empurrão custa >= 1 e aproxima 1 caixa em
    # <= 1 célula) e consistente -> primeiro pop de estado completo é o ótimo.
    goals_by_kind: dict = {}
    for kind in set(start_crates.values()):
        compat = [g for g in level.goals
                  if level.goal_req.get(g) is None or level.goal_req.get(g) == kind]
        if not compat:
            return {"solvable": False, "reason": f"nenhum goal compatível com kind={kind}", "states": 0}
        goals_by_kind[kind] = compat

    def heuristic(crates):
        total = 0
        for (cx, cy), kind in crates.items():
            total += min(abs(cx - gx) + abs(cy - gy) for gx, gy in goals_by_kind[kind])
        return total

    # Com robôs, a posição deles é função do número de comandos: o estado
    # precisa carregar a fase, senão o solver acharia soluções que dependem de
    # o robô estar em dois lugares ao mesmo tempo.
    period = level.patrol_period if level.patrols else 1

    # O relógio dos prazos só importa até o último deles: depois disso o
    # tabuleiro não muda mais, e saturar o contador mantém o estado finito.
    horizon = level.timed_horizon

    def clock(commands):
        return commands if commands < horizon else horizon

    def key(player, crates, phase=0, inverted=False, commands=0):
        return (player, tuple(sorted((p, k) for p, k in crates.items())),
                phase, inverted, clock(commands))

    start_key = key(level.player, start_crates, 0, False, 0)
    dist = {start_key: 0}
    prev = {}
    heap = [(heuristic(start_crates), 0, 0, start_key, level.player,
             tuple(sorted((p, k) for p, k in start_crates.items())), 0, False, 0)]
    counter = 0
    expanded = 0

    while heap:
        _, cost, _, k, player, crates_t, phase, inverted, commands = heapq.heappop(heap)
        if dist.get(k, 1 << 60) < cost:
            continue
        crates = dict(crates_t)
        if is_complete(level, crates):
            moves = []
            pushes = 0
            cur = k
            while cur != start_key:
                pk, dn, pu = prev[cur]
                moves.append(dn)
                pushes += 1 if pu else 0
                cur = pk
            moves.reverse()
            return {
                "solvable": True,
                "optimalCost": cost,
                "pushes": pushes,
                "solution": "".join(moves),
                "states": expanded,
            }
        expanded += 1
        if expanded > max_states:
            return {"solvable": False, "reason": "state-limit", "states": expanded}

        closed_doors = door_state(level, player, crates)
        next_phase = (phase + 1) % period
        robots = level.patrol_cells(phase + 1) if level.patrols else frozenset()
        next_commands = commands + 1

        # Prazos são avaliados com o relógio de ANTES do comando: a célula só
        # libera no comando seguinte ao vencimento, igual ao IsBlocked do motor.
        if level.timed:
            closed_doors = closed_doors | level.closed_timed(commands)

        for dname, (dx, dy) in DIRS.items():
            nx, ny = player[0] + dx, player[1] + dy
            npos = (nx, ny)
            if not (0 <= nx < level.width and 0 <= ny < level.height):
                continue
            if npos in level.walls or npos in closed_doors or npos in robots:
                continue
            step_cost = 2 if npos in level.costly else 1
            pushed = False
            cfinal = None
            new_crates = crates
            if npos in crates:
                cx, cy = nx + dx, ny + dy
                cpos = (cx, cy)
                if not (0 <= cx < level.width and 0 <= cy < level.height):
                    continue
                if cpos in level.walls or cpos in closed_doors or cpos in crates or cpos in robots:
                    continue
                # A carga desliza antes de o jogador entrar; a célula que ela
                # deixou conta como livre durante o próprio deslize.
                cfinal = slide(level, cpos, (dx, dy), crates, closed_doors, vacated=npos,
                               robots=robots, inverted=inverted)
                if cfinal in robots:
                    continue
                if cfinal not in live:
                    continue  # célula morta: caixa nunca mais alcança goal
                new_crates = dict(crates)
                del new_crates[npos]
                new_crates[cfinal] = crates[npos]
                pushed = True
                if corner_deadlock(level, new_crates):
                    continue
                pfinal = slide(level, npos, (dx, dy), new_crates, closed_doors,
                               robots=robots, inverted=inverted)
            else:
                pfinal = slide(level, npos, (dx, dy), crates, closed_doors,
                               robots=robots, inverted=inverted)

            # Espelha PuzzleBoardModel: esteira invertida pode devolver a carga
            # para a célula onde o jogador vai parar. Comando recusado.
            if pfinal in robots or (pushed and pfinal == cfinal):
                continue

            npos = pfinal
            next_inverted = inverted != (npos in level.buttons)
            nk = key(npos, new_crates, next_phase, next_inverted, next_commands)
            ncost = cost + step_cost
            if ncost < dist.get(nk, 1 << 60):
                dist[nk] = ncost
                prev[nk] = (k, dname, pushed)
                counter += 1
                heapq.heappush(heap, (ncost + heuristic(new_crates), ncost, counter, nk, npos,
                                      tuple(sorted((p, kk) for p, kk in new_crates.items())),
                                      next_phase, next_inverted, next_commands))

    return {"solvable": False, "reason": "exhausted", "states": expanded}


def replay(level: Level, solution: str):
    """Reexecuta a solução validando cada passo — prova independente."""
    player = level.player
    crates = dict(level.crates)
    cost = 0
    commands = 0
    inverted = False
    for ch in solution:
        dx, dy = DIRS[ch]
        closed = door_state(level, player, crates)
        if level.timed:
            closed = closed | level.closed_timed(commands)
        commands += 1
        robots = level.patrol_cells(commands) if level.patrols else frozenset()
        npos = (player[0] + dx, player[1] + dy)
        if not (0 <= npos[0] < level.width and 0 <= npos[1] < level.height):
            return False, f"fora do tabuleiro em {npos}"
        if npos in level.walls or npos in closed:
            return False, f"bloqueado em {npos}"
        if npos in robots:
            return False, f"robô ocupa {npos}"
        if npos in crates:
            cpos = (npos[0] + dx, npos[1] + dy)
            if (cpos in level.walls or cpos in closed or cpos in crates
                    or not (0 <= cpos[0] < level.width and 0 <= cpos[1] < level.height)):
                return False, f"empurrão inválido para {cpos}"
            if cpos in robots:
                return False, f"robô bloqueia a carga em {cpos}"
            cfinal = slide(level, cpos, (dx, dy), crates, closed, vacated=npos,
                           robots=robots, inverted=inverted)
            if cfinal in robots:
                return False, f"carga pararia sobre robô em {cfinal}"
            crate_kind = crates.pop(npos)
            crates[cfinal] = crate_kind
            cost += 2 if npos in level.costly else 1
            player = slide(level, npos, (dx, dy), crates, closed, robots=robots, inverted=inverted)
            if player in robots:
                return False, f"jogador pararia sobre robô em {player}"
            if player == cfinal:
                return False, f"jogador e carga parariam juntos em {player}"
            if player in level.buttons:
                inverted = not inverted
            continue

        cost += 2 if npos in level.costly else 1
        player = slide(level, npos, (dx, dy), crates, closed, robots=robots, inverted=inverted)
        if player in robots:
            return False, f"jogador pararia sobre robô em {player}"
        if player in level.buttons:
            inverted = not inverted
    if not is_complete(level, crates):
        return False, "solução não completa o nível"
    return True, cost


# --------------------------------------------------------------------- Main --

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--assets", help="diretório com .asset PuzzleLevelDefinition")
    ap.add_argument("--layouts", help="arquivo layouts.json")
    ap.add_argument("--out", help="arquivo de saída (JSON)")
    ap.add_argument("--only", help="filtra por substring do id (separar múltiplos por vírgula)")
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

    if args.only:
        keys = [k.strip() for k in args.only.split(",") if k.strip()]
        levels = [lv for lv in levels if any(k in lv.level_id for k in keys)]

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
