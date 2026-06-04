# ADR-002: Formato de fase — XSB estendido

**Status:** Accepted
**Date:** 2026-06-04
**Deciders:** fernando.augusto.peralta@gmail.com

## Context

Precisamos persistir fases (grid 2D com paredes, caixas, alvos, jogador) em
disco. Decisões:

1. Reaproveitar formato padrão da comunidade Sokoban OU criar formato custom?
2. Texto ou binário?
3. Suportar metadados (nome, autor, dificuldade, par)?

Cenário de uso:
- Editor in-game escreve fases criadas pelo usuário
- Importador opcional traduz layouts extraídos do ROM original
- Distribuir pack do remaster
- Eventual import de packs da comunidade (David Skinner, Microban, etc.)

## Decision

**Adotar XSB (eXtended Soko Ban) como formato base + frontmatter YAML para
metadados.**

Arquivo `.xsb` contém uma ou mais fases separadas por linha em branco. Cada
caractere é uma célula:

| Char | Significado |
|---|---|
| `#` | Parede |
| ` ` | Piso |
| `.` | Alvo (goal) |
| `$` | Caixa |
| `*` | Caixa em cima de alvo |
| `@` | Jogador |
| `+` | Jogador em cima de alvo |

Metadados em bloco YAML opcional antes do grid:

```
---
title: "Stage 1"
author: "fguta"
par: 23
difficulty: 1
---
########
#@ $  .#
########
```

## Options Considered

### Option A: XSB padrão + frontmatter (escolhida)

**Pros:**
- Compatível com 30+ anos de packs comunitários (literalmente milhares de
  fases já catalogadas)
- Editores externos (YASC, Sokoban+, JSoko) abrem direto
- Texto puro → diff/merge no Git, edição rápida em qualquer text editor
- Frontmatter resolve metadados sem quebrar parser legado (parser que não
  entende frontmatter ignora as linhas até o grid)

**Cons:**
- Caracteres ambíguos visualmente (`#` e `*` podem confundir)
- Não suporta nativamente mecânicas extras (botões, portas, tile-types)

### Option B: JSON custom

```json
{ "grid": [[0,1,...],...], "boxes": [...], "player": [3,2] }
```

**Pros:**
- Estruturado, fácil de validar com schema
- Extensível pra mecânicas novas
- Compatível com tooling moderno

**Cons:**
- Quebra interop com tooling Sokoban existente
- Verboso (cada célula = número), edição manual pior
- Diff Git é um inferno em arquivos JSON grandes

### Option C: Resource Godot (.tres)

**Pros:**
- Editor visual no Godot grátis
- Tipagem forte (classes Godot)

**Cons:**
- Lock-in total no Godot (impossível usar em editor externo)
- Diff verboso
- Não funciona pra import de packs externos

## Trade-off Analysis

| Critério | XSB+meta | JSON | .tres |
|---|---|---|---|
| Interop com packs externos | ✅ | ❌ | ❌ |
| Editável em text editor | ✅ | △ | ❌ |
| Git-friendly | ✅ | △ | △ |
| Extensível p/ mecânicas novas | △ (precisa convenção) | ✅ | ✅ |
| Validação automática | ❌ | ✅ | ✅ |

Decisivo: **interop**. Conseguir importar dos 5000+ packs comunitários
(LocalSearchSpace, Microban, Mas Sasquatch, David Skinner, etc.) já é
valor enorme antes de escrevermos qualquer fase.

## Extensões customizadas (futuro)

Reservar caracteres não-padrão XSB para mecânicas exclusivas do remaster:

| Char | Reservado para |
|---|---|
| `B` | Botão de pressão |
| `D` | Porta (controlada por botão) |
| `/` | Esteira esquerda |
| `\` | Esteira direita |
| `^` | Esteira cima |
| `v` | Esteira baixo |
| `T` | Teleportador |

Validar antes de comprometer: começar só com XSB-puro, adicionar mecânicas
quando ADR-005+ definir gameplay extra.

## Consequences

**Becomes easier:**
- Onboarding de level designers (formato conhecido)
- Importar packs comunitários como conteúdo extra (com créditos)
- Compartilhar fases do editor in-game como texto colável em Discord/forum

**Becomes harder:**
- Parser precisa lidar com frontmatter opcional + grid + packs
  multi-fase (precisa state machine simples)
- Mecânicas custom (botões, esteiras) precisam doc própria

## Action Items

1. [ ] Implementar parser XSB em `godot/scripts/level_loader.gd`
2. [ ] Suportar frontmatter YAML simples (só title/author/par/difficulty)
3. [ ] Serializer para o editor in-game produzir XSB válido
4. [ ] Validador de fase (BFS check de solvability) em `solver.gd`
5. [ ] Tools/Python: conversor XSB → resource Godot pra preload
