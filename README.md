# Warehouse Remaster *(working title)*

Pixel-art HD remaster — desktop nativo (Godot 4) — homenagem ao
*Shove It! The Warehouse Game* (Sega Mega Drive, NCS/Masaya, 1990).

## Estrutura

```
.
├── docs/                      ADRs, design doc, roadmap
├── godot/                     Projeto Godot 4 (abrir essa pasta no editor)
│   ├── project.godot
│   ├── scenes/                .tscn
│   ├── scripts/               .gd
│   └── assets/                sprites, audio, fonts, shaders
├── levels/
│   ├── new/                   Pack original do remaster (distribuível)
│   └── reference/             Layouts extraídos do ROM — gitignored
├── tools/                     Scripts Python para R&D (não distribuídos)
└── README.md                  você está aqui
```

## Documentação chave

- [docs/DESIGN.md](docs/DESIGN.md) — pilares e anti-pilares de design
- [docs/ROADMAP.md](docs/ROADMAP.md) — milestones e gates
- [docs/ADR-001-engine-choice.md](docs/ADR-001-engine-choice.md) — Godot 4
- [docs/ADR-002-level-format.md](docs/ADR-002-level-format.md) — XSB
- [docs/ADR-003-ip-strategy.md](docs/ADR-003-ip-strategy.md) — IP / créditos

## Como rodar (dev)

1. Instalar [Godot 4.6+](https://godotengine.org/download) Standard build
2. Abrir o projeto: `Godot > Import > godot/project.godot`
3. F5 pra rodar — verá a tela "WAREHOUSE REMASTER / Sprint 0"; ESC sai.

Gameplay ainda não existe (Sprint 1 entrega o walking skeleton).
Veja [docs/ROADMAP.md](docs/ROADMAP.md) para o plano completo.

## Créditos e IP

Inspirado por *Sokoban* (倉庫番), criado por **Hiroyuki Imabayashi**
para a **Thinking Rabbit** em 1982, e pelo port para Mega Drive
publicado pela **NCS/Masaya** em janeiro de 1990.

Este projeto é uma **homenagem não-oficial** construída do zero. Não
distribui código, arte, áudio ou layouts originais do ROM. O importador
opcional in-game permite ao usuário usar seu próprio ROM (BYO-ROM) para
jogar as 80 fases clássicas — o ROM nunca é incluído nas builds.

"Sokoban" é marca dos respectivos detentores. "Shove It! The Warehouse
Game" é marca da NCS/Masaya. Sega Mega Drive / Genesis são marcas da
Sega Corporation. Este projeto não é afiliado a nenhuma dessas
entidades.

Veja [docs/ADR-003-ip-strategy.md](docs/ADR-003-ip-strategy.md) para a
política completa.

## Licença

- **Código**: MIT (veja [LICENSE](LICENSE))
- **Fases originais criadas pelo dev**: CC BY-SA 4.0
- **Arte / áudio**: a definir (proprietário do dev, possivelmente CC BY-NC)
- **Fases extraídas via tools/**: nenhuma — não são distribuídas

## Status

🟡 **Sprint 0 — Foundation** (em andamento)
