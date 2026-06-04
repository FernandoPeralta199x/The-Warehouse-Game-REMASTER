# ADR-001: Engine — Godot 4

**Status:** Accepted
**Date:** 2026-06-04
**Deciders:** fernando.augusto.peralta@gmail.com (solo dev)

## Context

Estamos iniciando um remaster pixel-art HD de *Shove It! The Warehouse Game*
(Mega Drive, 1990, NCS/Masaya) como projeto desktop nativo. Características
do jogo que importam para a escolha de engine:

- Gameplay em grid (tile-based) — empurrar caixas para alvos
- Top-down 2D, sem física complexa
- Lógica determinística e turn-based (entrada do jogador = 1 step)
- ~80 fases originais a serem recriadas + pack novo + editor de fases
- Sem rede, sem multiplayer, sem 3D
- Target: Windows primário, Linux/Mac secundário, Web opcional
- Solo dev, escopo médio (3-6 meses)

## Decision

**Adotar Godot 4.x como engine.**
GDScript como linguagem primária; C# como fallback se algum subsistema
exigir performance ou bibliotecas .NET.

## Options Considered

### Option A: Godot 4 (escolhida)

| Dimensão | Avaliação |
|----------|-----------|
| Custo | $0 (MIT, sem royalty, sem runtime fee) |
| Tamanho do editor | ~150 MB |
| Pipeline 2D | Excelente — `TileMap`, `AStarGrid2D`, `AnimatedSprite2D` nativos |
| Linguagem | GDScript (rápido pra prototipar) ou C# |
| Build targets | Win/Linux/Mac/Web/Android/iOS do mesmo projeto |
| Familiaridade | Curva curta; documentação oficial muito boa |
| Comunidade | Crescimento forte pós-Unity-fiasco (2023+) |
| Source control | Cenas/scripts são texto puro (.tscn/.gd) — diff legível |
| Risco vendor lock-in | Zero (open source, fork-able) |

**Pros:**
- Tile-based 2D é o "happy path" do Godot 4 — exatamente nosso caso
- `TileMap` + `AStarGrid2D` + sinais resolvem ~70% do código de gameplay
- Editor portátil (1 executável, sem Hub)
- `.tscn` é texto → diff/merge no Git funciona de verdade
- Sem licença de runtime ameaçando virar paywall
- Web build (HTML5) funciona pra demo

**Cons:**
- Mercado de trabalho menor que Unity (irrelevante pro projeto)
- C# em Godot ainda menos polido que GDScript
- Menos asset store comparado a Unity (irrelevante — arte custom)
- Debugging de shaders menos maduro que Unity

### Option B: Unity 6

| Dimensão | Avaliação |
|----------|-----------|
| Custo | Grátis até $200k receita; runtime fee saga gerou desconfiança |
| Tamanho do editor | 5+ GB (Editor + módulos + Hub obrigatório) |
| Pipeline 2D | Bom, mas claramente secundário ao 3D |
| Linguagem | C# (maduro, performance OK) |
| Build targets | Idem ao Godot |
| Familiaridade | Alta no mercado |
| Source control | YAML para cenas (mergeable mas verboso) |
| Risco vendor lock-in | Alto (decisões de licença unilaterais) |

**Pros:**
- Empregabilidade C# (irrelevante neste projeto)
- Asset Store rico (irrelevante — arte custom)
- IDE Visual Studio / Rider maduros

**Cons:**
- Excesso de features pra um jogo grid-based 2D
- Hub + login obrigatórios
- Histórico de mudanças hostis de TOS (2023 runtime fee, depois revogado)
- `.unity` em YAML tem merge conflicts feios em prefabs aninhados

### Option C: Stack custom (SDL2 + C/C++)

| Dimensão | Avaliação |
|----------|-----------|
| Custo | $0 |
| Controle | Total |
| Tempo até MVP | 3-5x maior que engine |
| Tooling | Tudo manual (editor de fases, loop, asset pipeline) |

**Pros:**
- Performance, controle absoluto
- Honra ao espírito retro (matching a vibe MD original)

**Cons:**
- Reinventar editor de fases, asset pipeline, audio, save system
- ROI ruim pra projeto solo de ~6 meses
- Web build via Emscripten é viável mas chato

## Trade-off Analysis

| Critério | Peso | Godot | Unity | SDL2 |
|---|---|---|---|---|
| Tempo até jogável | 5 | 5 | 4 | 2 |
| Custo/risco licença | 5 | 5 | 2 | 5 |
| Pipeline 2D | 5 | 5 | 3 | 1 |
| Source control friendly | 4 | 5 | 3 | 5 |
| Web build trivial | 2 | 4 | 4 | 2 |
| Maturidade ecosystem | 3 | 4 | 5 | 3 |
| **Total (ponderado)** | | **109** | **80** | **66** |

Godot vence por margem clara em todos os critérios que importam neste
projeto específico. Unity perderia mesmo se o runtime-fee não tivesse
existido — para grid-based 2D solo, é overkill.

## Consequences

**Becomes easier:**
- Prototipagem rápida em GDScript (sem compilar)
- Editor de fases custom usando próprias scenes do Godot
- Hot-reload de scripts durante dev
- Build pra Win/Linux/Mac do mesmo projeto

**Becomes harder:**
- Eventual portabilidade pra Switch/console exige tradução (Godot 4 ainda
  não tem suporte first-party a consoles fechados)
- Onboarding de devs C# experientes (GDScript é diferente)

**To revisit:**
- Se decidirmos lançar em Switch/PS5: reavaliar com W4 Games (consultoria
  paga que faz porting de Godot pra console)
- Se gameplay exigir física 2D pesada: Godot 4 Physics tem limitações
  conhecidas; reavaliar Box2D direto

## Action Items

1. [x] Instalar Godot 4.6.3 Standard (não C#) ✓
2. [x] Criar `godot/project.godot` com config pixel-perfect ✓
3. [ ] Decidir nominal resolution (ver ADR-004 quando vier)
4. [x] Setup `.gitignore` Godot-aware ✓
5. [ ] Versão mínima alvo: **Godot 4.6.x** (instalada pelo dev em 2026-06)
