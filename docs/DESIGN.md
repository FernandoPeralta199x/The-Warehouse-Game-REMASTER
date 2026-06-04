# Design Pillars

## Vision

Um remaster pixel-art HD que preserva a **alma 16-bit** do *Shove It!*
(MD, 1990) com pipeline moderno: input responsivo, undo completo, save
de progresso, editor de fases in-game, e visual nítido em telas 4K.

Não é um port. Não é um clone. É uma **releitura visual** sobre uma
mecânica que continua sendo perfeita há 44 anos.

## Pilares (não-negociáveis)

### P1 — Mecânica é sagrada

Empurrar uma caixa é 1 step. Não pode puxar. Estado da fase é
determinístico. **Sem física, sem inércia, sem "juiciness" que distorce
input.** Sokoban é xadrez — o feeling tátil vem da clareza, não de
animação cheia de easing.

### P2 — Pixel-art HD, não pixel-art fake

Sprites desenhados em resolução nativa **alta** (ex: 64×64 ou 96×96 por
tile) com paleta limitada e shading de 16-bit. Isso **não é** sprite
upscaled. Não é CRT shader em cima de 8×8. É arte nova respeitando
gramática visual do original.

### P3 — Undo infinito

Mecânica Sokoban exige experimentação. Z/Backspace = undo 1 step. Não
existe "morte" — só estados de deadlock que o jogo destaca antes.

### P4 — Editor in-game

Toda fase carregável pelo jogo é editável dentro do próprio jogo. Player
e level designer usam a mesma ferramenta. Output é XSB com frontmatter
YAML (ADR-002).

### P5 — Acessibilidade real

- Daltonismo: paleta tem 3 modos (default / protanopia / deuteranopia)
- Sem som essencial: feedback visual para tudo
- Tamanho de fonte 1.5×/2× toggláveis
- Rebind completo de inputs (teclado + gamepad)
- "Reduced motion" desativa screenshake e fade transitions

### P6 — Sem telemetria, sem conta

Jogo offline. Save local. Zero spy. Hash anônimo opcional de fases
geradas pelo usuário para feature de "share code" (Sprint 5+).

## Anti-pilares (o que NÃO somos)

- **Não somos um Sokoban com twist 3D** — quem quer Patrick's Parabox
  já tem Patrick's Parabox.
- **Não somos um clone retro com glitter** — sem CRT mandatório, sem
  scanlines sobrepostas; quem quiser ativa.
- **Não somos um social/competitivo** — sem leaderboard global no MVP.
- **Não somos free-to-play** — preço fixo, sem microtransação.

## Audience

- Adultos 25-50 anos que jogaram Sokoban em 8/16-bit
- Puzzle enthusiasts que dão valor a polish e level design
- Streamers casuais (jogo é screenshot-friendly, partidas curtas)

## Comparáveis (referências)

| Jogo | O que pegar |
|---|---|
| *Stephen's Sausage Roll* | Profundidade de design de puzzle, ausência de tutorial verboso |
| *Baba Is You* | UI limpa, undo instantâneo, editor in-game |
| *A Monster's Expedition* | Curva de dificuldade orgânica, mundo costurando puzzles |
| *Hex Cells* | Estética minimal, feedback de input cristalino |
| *Picross 3D* | Polidez Nintendo na hora de "snap" das peças |

## Comparáveis (anti-referências)

| Jogo | O que evitar |
|---|---|
| Sokoban mobile shovelware | Anúncio entre fases, energia, paywall, juice fake |
| Ports retro preguiçosos | Emulação direta + filtro CRT só pra dizer "remaster" |

## Resolução e arte

- **Resolução de design (cell):** 96×96px (planejado, ajustar em ADR-004)
- **Resolução nominal de janela:** 1920×1080 (suporta 4K via scale 2×)
- **Aspecto:** 16:9 com letterbox em telas 4:3
- **Paleta:** ~32 cores curadas (não-restrita por hardware, restrita por
  disciplina autoral)

## Som

- Música: 2 trilhas chillout pra menu + gameplay, 1 stinger pra clear
- SFX: empurrar caixa, alvo atingido, undo, level clear, deadlock warning

## Out of scope (MVP)

- Multiplayer
- Modo speedrun com timer global
- Skins/cosméticos
- Localização (pós-MVP — manter strings externalizadas desde dia 1)
- Mobile / touch input (avaliar pós-MVP)
