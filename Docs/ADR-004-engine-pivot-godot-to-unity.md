# ADR-004: Engine pivot — Godot 4 → Unity 6.3 LTS

**Status:** Provisional (aguarda ratificação do owner)
**Date:** 2026-07-26
**Deciders:** fernando.augusto.peralta@gmail.com
**Supersedes (implicitly):** ADR-001 (Godot 4)

## Context

Entre 2026-06-04 (data original de ADR-001) e 2026-07-26 (esta ADR):

- 2026-06-04 → ADR-001 escolheu **Godot 4** como engine, com análise de
  trade-offs formal (Godot 109 pts, Unity 80 pts). Sprints 0 e 1 foram
  implementados em Godot 4.6.3 (commits `e657803`, `13bec05`, `6a97ab0`
  no `origin/main`, ainda preservados).
- 2026-07-09 → O documento operacional
  `REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md` foi escrito e
  na Seção 7.1 **reforça Godot 4** como escolha recomendada.
- 2026-07-26 → Um scaffold **Unity 6.3 LTS** (75 scripts C#, 163 SFX
  procedurais, `Assets/_Project/`, assemblies split, save system com
  atomic writes/migração, docs em `Docs/`) foi gerado no diretório do
  projeto. O `godot/` original foi removido do working tree. O Git local
  foi reinicializado (sem commits).

Owner confirmou (2026-07-26) que o pivot foi decisão consciente dele.

## Decision (provisional)

**Adotar Unity 6.3 LTS como engine de produção**, contrariando ADR-001 e a
recomendação do Workflow doc (§7.1). Manter o histórico Godot preservado
em uma branch dedicada (`godot-legacy`) para audit trail.

## Reasoning

Motivações declaradas ou inferidas para o pivot:

- **Escopo real do jogo** (per `REFERENCIA/`): puzzle + racing arcade de
  empilhadeira + narrativa com 4 personagens + power-ups + 30 fases + 10
  secretas + 6 setores. Isso é grande demais para o roadmap Godot solo
  de 16 semanas projetado em ADR-001.
- **C# vs GDScript** para um projeto desse porte: tooling mais maduro
  (Rider, Visual Studio), refactoring mais seguro, mercado maior de
  bibliotecas .NET, e onboarding potencial de colaboradores futuros mais
  amplo em C#.
- **ScriptableObject como content pipeline**: para 40 fases + power-ups
  paramétricos + rotas de corrida + waypoints, editor visual de Unity
  ganha do editor de Godot 4.
- **Unity 6 URP 2D + 2D Renderer**: para pixel-art HD (pilar P2 de
  `docs/DESIGN.md` legacy) com iluminação 2D, oclusão e post-processing,
  Unity oferece pipeline mais maduro.
- **Ecosystem Steam**: Steamworks.NET, DLC pipeline, Unity Cloud Build.

## Conflict with Workflow doc

O Workflow doc (§7.1) diz:

> [Provável] Use **Godot 4**.
> Motivos: Excelente para 2D. Leve. Boa para indie. Boa produtividade.
> Evita o peso da Unreal.

Note que o marcador é `[Provável]`, não `[Certeza]` — o próprio doc admite
que a escolha da engine tem espaço para reavaliação. Este ADR-004 documenta
essa reavaliação.

**Ação de sincronização:** o Workflow doc deve ser atualizado (ou uma nota
de errata anexada) para refletir a decisão de Unity. Alternativa: reverter
para Godot conforme o Workflow original.

## Consequences

**Herda:**
- Estrutura `Assets/_Project/` (Unity padrão) em vez de `game/scenes/`
  (Workflow §8).
- Namespaces `TW08.*` em C# em vez de scripts GDScript planos.
- Formato de fase: `PuzzleLevelDefinition` ScriptableObject em vez de JSON
  puro (Workflow §16 sprint 2 dizia `id, name, tiles, par`; migrar).

**Perde de ADR-001:**
- Custo zero de licença Godot (Unity 6 tem free tier, mas com futuras
  restrições possíveis — risco histórico).
- Simplicidade de `.tscn` texto vs `.unity` YAML para merges Git.
- Editor 150MB vs 5+GB.

**Perde de Workflow doc:**
- Estrutura de repo padronizada (game/scenes/, data/levels/, tools/).
- Backlog TW08-XXX mapeado para arquivos GDScript.

**Ganha:**
- Melhor suporte a assets 2D avançados (Sprite Atlas, 2D Lights).
- Test runner integrado (Edit/Play mode assemblies).
- C# tooling superior (async/await, LINQ, Rider, resharper).
- Roadmap acomoda escopo racing+narrativa+puzzle sem reescrever engine.

## Migration plan

1. **Preservar Godot como histórico** (branch `godot-legacy` local +
   remoto). Feito neste sprint.
2. **Commit atômico Unity** substituindo `godot/` por `Assets/`, no topo
   dos commits Godot (fast-forward de `origin/main`).
3. **Deprecar formalmente** ADR-001 (adicionar seção "Deprecated in favor
   of ADR-004" ao topo).
4. **Reescrever ADR-002** (formato de fase XSB → JSON schema per Workflow
   §16 + ScriptableObject wrapper Unity).
5. **Reescrever ADR-003** (remover Camada 3 BYO-ROM importer — Workflow
   §3 proíbe). Manter Camadas 1 e 2.
6. **Refazer ROADMAP.md** com os IDs TW08-XXX do Workflow §16.
7. **Sincronizar Workflow doc** com decisão Unity, ou reverter para Godot.
8. **Primeira compilação Unity obrigatória** — o scaffold nunca abriu no
   Unity Editor (VALIDATION_REPORT.md diz "validado estaticamente").

## Action Items

1. [x] Criar `AGENTS.md` (Workflow §10)
2. [x] Escrever ADR-004 (este arquivo)
3. [ ] Owner ratifica ou reverte o pivot (Workflow doc precisa alinhar)
4. [ ] Marcar `docs/ADR-001*.md` como Deprecated (após pull do godot-legacy)
5. [ ] Reescrever ADR-002 e ADR-003 (idem)
6. [ ] Instalar Unity 6.3 LTS e abrir projeto pela primeira vez
7. [ ] Rodar `Tools > TW08 > Create Starter Content and Prototype Scenes`
8. [ ] Rodar `Tools > TW08 > Validate Project`
9. [ ] Rodar Test Runner (Edit Mode + Play Mode) e capturar resultado

## Riscos abertos

- **Scaffold nunca compilou no Unity** — VALIDATION_REPORT.md declara
  "validação estática, sem Unity Editor". Podem existir bugs de compilação.
- **`.meta` files potencialmente inconsistentes** — Unity re-gera na
  primeira abertura, pode causar duplicações ou conflitos.
- **Escopo do scaffold** (puzzle + racing + narrativa + power-ups) já
  cobre Sprints 6+ do Workflow original — pode indicar over-scaffold que
  torna manutenção difícil antes de qualquer sprint fechar.
- **Conflict com Workflow doc** — se não ratificado, ADR-004 fica em
  contradição permanente com o normativo do projeto.
