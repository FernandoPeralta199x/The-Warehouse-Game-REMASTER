#!/usr/bin/env python3
"""Adiciona Lote B (S05/S06) e Lote C (secretas) ao level-layouts.json."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
p = ROOT / "Docs/level-layouts.json"
d = json.loads(p.read_text(encoding="utf-8"))
ids = {l["id"] for l in d["levels"]}

new = [
 {"id":"TW08_Level21_LockedWorkshop","displayName":"Oficina Travada","sectorId":"S05",
  "gimmickTags":["heavy_crate","manual_door","blocked_corridor"],
  "rows":["###########","#         #","# h  ~~.  #","#@$  ~~ . #","# h  ~~.  #","# $  ~~ . #","#         #","###########"],
  "goalRequirements":[{"x":7,"y":5,"kind":"HeavyCrate"},{"x":7,"y":3,"kind":"HeavyCrate"}]},
 {"id":"TW08_Level22_JackN8","displayName":"Macaco N-8","sectorId":"S05",
  "gimmickTags":["n8_jack","stuck_crate","tutorial"],
  "rows":["##########","#    #   #","# $  #.  #","#@$    . #","# #$   . #","#        #","##########"]},
 {"id":"TW08_Level24_OldGenerator","displayName":"Gerador Antigo","sectorId":"S05",
  "gimmickTags":["hybrid","heavy_battery","locked_door","tool_crate","narrative"],
  "rows":["###########","#   #######","# h $ 1####","#@ ~~  A .#","# $ h  . .#","#   #######","###########"],
  "extraGoals":[{"x":6,"y":4}],
  "goalRequirements":[{"x":9,"y":3,"kind":"HeavyCrate"},{"x":9,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Level25_DeadWeight","displayName":"Peso Morto","sectorId":"S05",
  "gimmickTags":["heavy_crate","weight_sensor","tool_crate","move_order"],
  "rows":["############","#    #######","# h  ~~1####","#@$$ ~~ A..#","# h  ~~1  ##","# $   .  ###","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Level26_DeadArchive","displayName":"Arquivo Morto","sectorId":"S06",
  "gimmickTags":["partial_map","terminal","fake_wall","marked_crate","narrative"],
  "rows":["###########","#    ######","# $$   .  #","#@ ~~  A..#","# $$  1   #","#    ######","###########"],
  "extraGoals":[{"x":6,"y":2}]},
 {"id":"TW08_Level27_GhostRoute","displayName":"Rota Fantasma","sectorId":"S06",
  "gimmickTags":["partial_map","spatial_memory","marked_crate"],
  "rows":["############","#@   #######","# $$$ ...###","#  ~~    ###","# $$   .. ##","#  #########","############"]},
 {"id":"TW08_Level28_CargoWithoutOrigin","displayName":"Carga Sem Origem","sectorId":"S06",
  "gimmickTags":["marked_crate","ordered_goals","false_route","final_terminal","narrative"],
  "rows":["###########","#  # ######","# $ $ 1   #","#@~~   A. #","# $ $  . .#","#   #######","###########"],
  "extraGoals":[{"x":6,"y":4}]},
 {"id":"TW08_Level29_LockdownN8","displayName":"Lockdown N-8","sectorId":"S06",
  "gimmickTags":["hybrid","weight_sensor","timed_gate"],
  "rows":["############","#    #######","# h  ~~1####","#@$$ ~AA..##","# h  ~~1  ##","# $   .  ###","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Level30_LogisticsCore","displayName":"Nucleo Logistico","sectorId":"S06",
  "gimmickTags":["weight_sensor","door","false_route","final_terminal","narrative","finale"],
  "rows":["############","#    #######","# h  ~~1####","#@$$ ~~ A..#","# h  ~~1  ##","# $ $ ..####","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Secret01_OffRecordCrate","displayName":"Caixa Fora do Registro","sectorId":"SEC",
  "gimmickTags":["secret","fake_wall","extra_crate"],
  "rows":["##########","#    #   #","# $  #.  #","#@$    . #","# #$   . #","#  $   . #","##########"]},
 {"id":"TW08_Secret02_RobertsRoom","displayName":"Sala do Robert","sectorId":"SEC",
  "gimmickTags":["secret","heavy_crate","old_tools","narrow_corridor"],
  "rows":["###########","#         #","# h  ~~ . #","#@$  ~~.  #","# h  ~~ . #","# $  ~~.  #","#         #","###########"],
  "goalRequirements":[{"x":8,"y":5,"kind":"HeavyCrate"},{"x":8,"y":3,"kind":"HeavyCrate"}]},
 {"id":"TW08_Secret03_DudasShift","displayName":"Turno da Duda","sectorId":"SEC",
  "gimmickTags":["secret","narrative","hidden_goals"],
  "rows":["###########","#  # ######","# $ $ 1   #","#@ ~~ AA .#","# $ $  . .#","#   #######","###########"],
  "extraGoals":[{"x":6,"y":4}]},
 {"id":"TW08_Secret04_EliasRoute","displayName":"Rota do Elias","sectorId":"SEC",
  "gimmickTags":["secret","three_rooms","single_path"],
  "rows":["############","#    #######","# h  ~ 1####","#@$$ ~~ A..#","# h   ~1  ##","# $   .  ###","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Secret05_DarkWorkshop","displayName":"Oficina Sem Luz","sectorId":"SEC",
  "gimmickTags":["secret","dark_map","limited_vision","spatial_memory"],
  "rows":["###########","#         #","# $     . #","#@$    .  #","# $   #.  #","# $    .  #","#         #","###########"]},
 {"id":"TW08_Secret07_Sector08B","displayName":"08-B","sectorId":"SEC",
  "gimmickTags":["secret","tight_space","precision","move_order"],
  "rows":["############","#   ########","# $$  1#####","#@~~~   A..#","# $$ ~~~ .##","#   ########","############"],
  "extraGoals":[{"x":6,"y":4}]},
 {"id":"TW08_Secret08_LeftoverMap","displayName":"O Mapa que Sobrou","sectorId":"SEC",
  "gimmickTags":["secret","tool_crate","temporary_block","reverse_planning"],
  "rows":["############","#    #######","# h  ~~1####","#@$$ ~AA..##","# h  ~~1  ##","# $    . ###","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Secret09_EliasLastShift","displayName":"Ultimo Turno do Elias","sectorId":"SEC",
  "gimmickTags":["secret","three_rooms","marked_crate","weight_sensor","false_route"],
  "rows":["############","#    #######","# h  ~~1####","#@ $ ~~ A. #","# h  ~~1  ##","# $ $  .. ##","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
 {"id":"TW08_Secret10_DudasPath","displayName":"O Caminho da Duda","sectorId":"SEC",
  "gimmickTags":["secret","no_hints","no_powerups","weight_sensor","ice_floor","finale"],
  "rows":["############","#    #######","# h ~~ 1####","#@$$ ~~AA..#","# h ~~ 1  ##","# $ $  ..###","############"],
  "extraGoals":[{"x":7,"y":4},{"x":7,"y":2}],
  "goalRequirements":[{"x":7,"y":4,"kind":"HeavyCrate"},{"x":7,"y":2,"kind":"HeavyCrate"}]},
]

for n in new:
    if n["id"] in ids:
        for i, l in enumerate(d["levels"]):
            if l["id"] == n["id"]:
                d["levels"][i] = n
                break
    else:
        d["levels"].append(n)

# valida larguras consistentes
for l in d["levels"]:
    w = len(l["rows"][0])
    bad = [r for r in l["rows"] if len(r) != w]
    if bad:
        raise SystemExit(f"{l['id']}: linhas com largura inconsistente: {bad}")

p.write_text(json.dumps(d, indent=2, ensure_ascii=False), encoding="utf-8")
print("total:", len(d["levels"]))
