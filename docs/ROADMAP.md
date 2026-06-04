# Roadmap

Estimativas em **semanas-pessoa** (1 dev solo, ~15h/semana). Marcos
gateados por demonstração funcional, não por porcentagem de "feito".

## Sprint 0 — Foundation (semana 1)

**Status:** EM ANDAMENTO

- [x] Scaffold de pastas
- [x] ADR-001 (engine)
- [x] ADR-002 (level format)
- [x] ADR-003 (IP)
- [x] DESIGN.md, ROADMAP.md, README
- [x] `project.godot` minimamente configurado
- [x] Instalar Godot 4.6.3
- [x] Placeholder `main_menu.tscn` (tela "Sprint 0", ESC sai)
- [ ] Repositório Git iniciado, primeiro commit

**Gate:** abrir Godot, ver `main_menu.tscn` rodando sem erros. ✅

## Sprint 1 — Walking skeleton (semanas 2-3)

Objetivo: jogador anda em grid 16×12, empurra 1 caixa, alvo registra clear.

- [ ] `level.gd` — TileMap simples com paredes/piso/alvo
- [ ] `player.gd` — movimento step-by-step com input WASD/setas
- [ ] `box.gd` — caixa empurrável, detecta sobre alvo
- [ ] Parser XSB básico (sem frontmatter ainda) em `level_loader.gd`
- [ ] 1 fase hardcoded (tutorial dummy)
- [ ] Win condition: todas caixas em alvo → tela "FASE COMPLETA"

**Gate:** terminar uma fase do começo ao fim.

**Risco:** subestimar pixel-perfect rendering no Godot 4 (camera, snap).

## Sprint 2 — Polimento de input (semana 4)

- [ ] Undo stack (Z = 1 step, Shift+Z = 10 steps)
- [ ] Reset de fase (R)
- [ ] Gamepad support (D-pad)
- [ ] Animação de empurrão (~120ms tween, sem easing exagerado)
- [ ] SFX placeholder (4-5 sons gerados em sfxr/jsfxr)

**Gate:** controle se sente "tight" em playtests rápidos.

## Sprint 3 — Editor de fases v1 (semanas 5-6)

- [ ] Cena `level_editor.tscn`
- [ ] Paleta de tiles (parede, alvo, caixa, jogador)
- [ ] Brush + erase + bucket fill
- [ ] Save/load XSB local
- [ ] Validador: BFS de solvability antes de salvar

**Gate:** desenhar 1 fase no editor, salvar, jogar a partir do menu.

## Sprint 4 — BYO-ROM importer (semana 7)

- [ ] Tela "import original levels"
- [ ] Parser de ROM: localiza tabela de fases (RE preliminar já feita)
- [ ] Validação SHA-1 contra ROM `[!]` (rejeitar outros)
- [ ] Converte 80 fases pra XSB em memória (não disco)
- [ ] Disponibiliza "Original Pack" no level select se importado

**Gate:** usuário com ROM próprio joga as 80 originais.

## Sprint 5 — Arte HD inicial (semanas 8-10)

- [ ] Sprites tile-set base (parede, piso, alvo, caixa, jogador) 96×96
- [ ] Animação de idle do jogador (3 frames)
- [ ] Animação de walk (4 frames × 4 direções)
- [ ] Paleta final + tile variants (anti-tiling)
- [ ] Modos daltonismo (3 paletas)

**Gate:** screenshot do jogo parece um produto, não um protótipo.

## Sprint 6 — Pack original do remaster (semanas 11-13)

- [ ] Desenhar 30 fases novas com curva de dificuldade
- [ ] Playtest com 3+ pessoas
- [ ] Ajustar com base em telemetria local (heatmap de undo/reset)

**Gate:** 30 fases jogáveis em ordem, com curva validada.

## Sprint 7 — Áudio e final polish (semanas 14-15)

- [ ] 2 trilhas (menu + gameplay)
- [ ] SFX finais
- [ ] Tela de título com idle
- [ ] Save/load progresso (slot único + cloud opcional via Steam Cloud)
- [ ] Settings (volume, paleta, rebind, scale)

**Gate:** build pronto para release.

## Sprint 8 — Release prep (semana 16)

- [ ] Itch.io page
- [ ] Trailer 60s
- [ ] Build Windows, Linux, Mac
- [ ] Smoke test em 3 máquinas diferentes

**Gate:** v1.0.0 publicado.

## Pós-MVP

- Localização (PT-BR garantida; ES, EN, JP target)
- Modo "daily puzzle" (procedural seedado em data)
- Steam release (se itch der tração)
- Editor de pack/sharing por código
- Mobile (avaliar)

## Riscos altos

| Risco | Mitigação |
|---|---|
| Pixel-perfect render quebra no scale 4K | Resolver em Sprint 1, não adiar |
| Arte HD demora 3× o estimado | Plano B: contratar artista freelance se passar de Sprint 6 |
| BYO-ROM importer falha em ROMs `[h1]`/`[T+Por]` | Aceitar SHA-1 das 3 versões "boas"; rejeitar `[b1]`/`[o1]` |
| Scope creep em editor | Time-box rígido em Sprint 3 |
| Demotivação solo após Sprint 5 | Aceitar pause de 2 semanas como parte do plano |
