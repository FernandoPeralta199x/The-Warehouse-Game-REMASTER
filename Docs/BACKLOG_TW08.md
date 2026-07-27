# Backlog TW08 — mapeamento Workflow §16 ↔ scaffold Unity atual

Base normativa: [REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md](../REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md)
§16 (Backlog inicial). Este documento é o **single source of truth** para
status de cada TW08-XXX. Atualizar aqui quando fechar item.

## Convenções

- **Status:** ⬜ pending · 🟡 in progress · ✅ done · ⚠️ done com desvio · ❌ blocked
- **Fonte:** arquivo/pasta principal que resolve o item
- **Desvio:** se `⚠️`, explicar diferença vs Workflow

## Sprint 0 — Blindagem jurídica e técnica

| ID | Descrição (Workflow) | Status | Fonte / Desvio |
|---|---|---|---|
| TW08-001 | Criar `AGENTS.md` | ✅ | `AGENTS.md` |
| TW08-002 | Criar `docs/04_CLEAN_ROOM_RULES.md` | ⬜ | Conteúdo coberto por AGENTS.md + ADR-005; migrar para arquivo dedicado se quisermos manter esquema numérico |
| TW08-003 | Criar `docs/06_IP_AND_LICENSES.md` | ⚠️ | Existe `Docs/LEGAL_IP_CHECKLIST.md` (do scaffold Unity) — renomear ou consolidar |
| TW08-004 | Configurar `.gitignore` contra ROMs/dumps | ✅ | `.gitignore` (linhas §9) |
| TW08-005 | Criar `README` com identidade original | ⚠️ | `README.md` existe (do scaffold) — precisa revisão pra alinhar com AGENTS.md |
| TW08-006 | Criar `docs/08_CLAUDE_CODEX_WORKFLOW.md` | ⚠️ | Existe em `REFERENCIA/`, deve ser movido pra `Docs/08_CLAUDE_CODEX_WORKFLOW.md` |

**Critério de aceite Sprint 0:**
- [x] Nenhum arquivo de ROM no repo
- [x] Nenhum mapa original no repo
- [x] Nenhuma senha original no repo
- [x] Nenhum nome/personagem/sprite/música de Shove It!
- [x] Documentação declara sucessor espiritual original

## Sprint 1 — Core lógico (puzzle)

| ID | Descrição | Status | Fonte / Desvio |
|---|---|---|---|
| TW08-010 | Criar modelo de grid | ✅ | `Assets/_Project/Scripts/Puzzle/GridCoordinate.cs` + `PuzzleBoardModel.cs` |
| TW08-011 | Criar modelo de célula | ✅ | `PuzzleEntityKind.cs` (enum), tiles implícitos no BoardModel |
| TW08-012 | Movimento do jogador | ✅ | `PuzzlePlayerController.cs` + `PuzzleBoardModel.TryMove` |
| TW08-013 | Empurrar caixa | ✅ | `PuzzleMove.cs` + `PuzzleBoardModel` |
| TW08-014 | Colisão | ✅ | `PuzzleBoardModel` (walls + crates + goals) |
| TW08-015 | Vitória | ✅ | `PuzzleBoardModel.IsComplete` |
| TW08-016 | Undo | ✅ | `PuzzleHistory.cs` |
| TW08-017 | Restart | ✅ | `PuzzleRuntime.cs` (assumido — verificar) |
| TW08-018 | Testes do core | ⚠️ | Assembly `TW08.Tests.EditMode.asmdef` existe mas testes precisam ser rodados no Unity Editor |

**Critério de aceite Sprint 1:**
- [ ] Testes passam (bloqueado: Unity Editor precisa abrir pela primeira vez)
- [ ] Movimento é determinístico (validar via testes)
- [ ] Estado da fase serializa/desserializa (assumido; validar)
- [ ] Undo funciona (código existe, precisa test suite rodando)
- [ ] VictoryChecker vence quando todos os alvos estão ocupados

## Sprint 2 — Formato de fase e loader

| ID | Descrição | Status | Fonte / Desvio |
|---|---|---|---|
| TW08-020 | Schema JSON de fase | ⚠️ | Scaffold usa `PuzzleLevelDefinition` ScriptableObject em vez de JSON. **Decisão pendente:** manter SO ou adicionar JSON schema por cima |
| TW08-021 | `LevelLoader` | ⚠️ | `LevelCatalog.cs` + `LevelDefinition.cs` — cobre parcialmente |
| TW08-022 | `LevelValidator` | ✅ | `PuzzleLevelValidator.cs` |
| TW08-023 | Criar 10 fases originais de MVP | ⬜ | Sem `.asset` de fase criados ainda; scaffold só tem código |
| TW08-024 | Testes de carregamento | ⬜ | Depende de fases + Unity Editor |

**Desvio principal:** Workflow §16 pede JSON puro (`tiles: ["###...","#.P.#"]`).
O scaffold usa ScriptableObject com listas de coordenadas. Trade-off:
- SO ganha em editor visual do Unity e forte tipagem
- JSON ganha em portabilidade, review de PR, editabilidade externa

Recomendação: **manter SO como formato runtime + gerar JSON canônico como
export/backup**. Documentar em ADR-006 se avançarmos.

## Sprint 3 — Protótipo visual

| ID | Descrição | Status | Fonte |
|---|---|---|---|
| TW08-030 | Cena de jogo | ⬜ | Sem `.unity` files ainda — `Tools > TW08 > Create Starter Content` |
| TW08-031 | Player placeholder | ⚠️ | `PrototypeSpriteRenderer.cs` existe (utilitário), mas sem prefab |
| TW08-032 | Crate placeholder | ⬜ | idem |
| TW08-033 | Wall/floor/target placeholders | ⬜ | idem |
| TW08-034 | HUD de movimentos | ⚠️ | `LoadingScreenPresenter`, `UIFlowController`, `UIScreen` — precisa HUD dedicada |
| TW08-035 | Botões undo/restart | ⬜ | UI a construir |
| TW08-036 | Seleção simples de fases | ⚠️ | `LevelCatalog` + `LevelProgressionService` — base pronta, UI faltando |

**Crítico:** este sprint só destrava com Unity aberto (cenas `.unity` são
binário-ish e não posso criar/editar via Write tool).

## Sprint 4 — UX moderna e controle

| ID | Descrição | Status | Fonte |
|---|---|---|---|
| TW08-040 | Suporte a controle | ⚠️ | `GameInput.cs` existe — validar mapping |
| TW08-041 | Remapeamento de teclas | ⬜ | |
| TW08-042 | Animação suave | ⬜ | Depende de prefabs |
| TW08-043 | Tela de pausa | ⚠️ | `PauseService.cs` existe (serviço), tela UI faltando |
| TW08-044 | Configurações | ⬜ | |
| TW08-045 | Acessibilidade básica | ⬜ | |

## Sprint 5 — Vertical slice

| ID | Descrição | Status | Fonte |
|---|---|---|---|
| TW08-050 | Direção de arte final do mundo 1 | ⬜ | |
| TW08-051 | Música original do mundo 1 | ⬜ | 163 SFX WAV existem em `Assets/.../Prototype/` — são placeholders, não finais |
| TW08-052 | SFX originais | ⚠️ | Prototype SFX prontos, finais pendentes |
| TW08-053 | Sistema de medalhas | ⬜ | (SFX de medals existe, sistema não) |
| TW08-054 | Save local | ✅ | `Save/JsonSaveService.cs` + `SaveIntegrity` + `SaveMigrationPipeline` (atomic, checksum, migration) |
| TW08-055 | 20 fases originais | ⬜ | 0 fases criadas ainda |
| TW08-056 | Build Windows | ⬜ | |
| TW08-057 | Playtest fechado | ⬜ | |

## Fora do Workflow §16 — escopo adicional do scaffold Unity

O scaffold gerou infraestrutura que **vai além** do Sprint 0-5 do Workflow.
Estes itens **não estão no backlog original** e precisam de decisão:

| Feature | Fonte | Decisão pendente |
|---|---|---|
| Racing arcade de empilhadeira | `Scripts/Forklift/*.cs` (13 scripts) | Manter, deprecar, ou mover pra Sprint 6+? |
| Power-ups (7 tipos, weighted table) | `Scripts/Forklift/PowerUps/*.cs` | idem |
| Narrativa com triggers | `Scripts/Narrative/*.cs` | idem |
| 4 personagens (John/Duda/Big Rob/Elias) | `REFERENCIA/Personagens/` | idem — já roteirizado em `Historia_Central.md` |
| 6 setores de progressão | Referenciado em SFX naming | idem |
| Shop com credits | SFX `ui/shop/` existe | idem |
| Sistema de medalhas bronze/prata/ouro/platinum | SFX `ui/results/medal_*` | Alinha com TW08-053 |

**Recomendação:** Sprint 0.5 = auditoria de escopo. Ou você mantém tudo
(vira 6+ meses de solo dev), ou aceita reduzir o scaffold pra Sokoban-first
(deleta `Forklift/` e `Narrative/` até serem necessários).

## Estado agregado

- **Sprint 0:** 4/6 ✅ (2 desvios menores) — **pronto para fechar**
- **Sprint 1:** 8/9 ✅ (testes bloqueados por Unity Editor)
- **Sprint 2:** 0/5 ✅ (1 desvio de formato pendente)
- **Sprint 3-5:** dependem de Unity Editor aberto e fases criadas
