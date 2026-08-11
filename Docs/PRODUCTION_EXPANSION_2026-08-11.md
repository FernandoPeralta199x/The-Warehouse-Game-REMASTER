# The Warehouse Nº 08 — Full Production Expansion

Data: 2026-08-11  
Engine: Unity 6.3 LTS  
Linguagem: C#  
Branch: `feat/tw08-vertical-slice-foundation`

## Objetivo

Expandir o vertical slice puzzle-first sem diluir o core do projeto. Puzzle permanece o modo principal; `N-8 Logistics Rush` entra como modo arcade/logístico complementar, reutilizando o subsistema de empilhadeira/corrida já existente.

## Conteúdo implementado

### Operadores

- John Miller — jogável em puzzle e corrida.
- Maria Eduarda “Duda” — jogável em puzzle e corrida.
- Robert “Big Rob” Hayes — perfil/NPC da oficina; ainda não jogável.
- Seleção persistente de operador.
- `CharacterProfile`, `CharacterRoster` e apresentação selecionável por dados.

### Campanha puzzle

A campanha gerada contém 9 fases:

1. Primeiro Turno
2. Corredor Apertado
3. Carga Cruzada
4. Sensor Split
5. Tight Lift
6. Terminal Route
7. Dock Sync
8. Cold Storage
9. Cross Dispatch

As fases 04–09 introduzem regras reais no domínio:

- sensores que controlam grupos de portas;
- transições atômicas de portas, sem esmagar player/carga;
- piso de câmara fria com custo de movimento adicional;
- docas tipadas para carga normal, heavy e fragile;
- combinações dessas regras na Fase 09.

### Verificação lógica independente das novas fases

Um solver BFS independente foi executado contra o modelo das novas regras antes da materialização das fases. Resultados mínimos encontrados:

| Fase | Solução mínima encontrada |
|---|---:|
| 04 — Sensor Split | 8 |
| 05 — Tight Lift | 20 |
| 06 — Terminal Route | 13 |
| 07 — Dock Sync | 20 |
| 08 — Cold Storage | 12 movimentos / custo 22 |
| 09 — Cross Dispatch | 20 movimentos / custo 28 |

Isso prova solucionabilidade sob as regras modeladas. Não prova diversão, pacing ou dificuldade percebida; playtest humano continua obrigatório.

### N-8 Logistics Rush

Três pistas são geradas:

1. Receiving Loop — tutorial/entrada.
2. Industrial Corridor — chicanes, obstáculos e zona de baixa aderência.
3. Frozen Route — baixa aderência como gimmick principal.

O modo reutiliza:

- `ArcadeForkliftController2D`;
- `RaceManager`;
- `RacerProgress`;
- `RaceCheckpoint`;
- `RaceCountdown`;
- `BoostPad`;
- `SurfaceGripZone`;
- `ForkliftDamage`.

A expansão adiciona metadata/campaign, sessão, HUD, persistência de tempos/medalhas, seleção de piloto, VFX e geração das três cenas.

## UI/UX

O fluxo de produção passa a ser:

`Main Menu -> Hub -> Campaign / Race / Operators / Settings / Credits`

O hub gera:

- seleção de 9 fases com lock/unlock, medalha e best moves;
- seleção de 3 pistas com lock/unlock, medalha e best time;
- tela de operadores com John, Duda e Robert;
- configurações de Master/Music/SFX;
- créditos;
- HUD puzzle ampliado;
- HUD de corrida com timer, volta, best time, piloto e countdown.

## Save v2

O save agora persiste:

- operador selecionado;
- operadores desbloqueados;
- progresso e medalhas de puzzle;
- best moves;
- progresso, medalhas e best times de corrida;
- créditos;
- volumes Master/Music/SFX.

Existe migração explícita de save v1 para v2, incluindo mapeamento do antigo `prototype-001` para a primeira rota de produção.

## Arte e apresentação

A pasta `ReferenceSource` continua como direção visual. Assets starter gerados pelo Editor ficam separados da arte manual/final.

A expansão gera:

- Duda em quatro direções com Idle + Walk 1/2/3;
- retratos starter de John, Duda e Robert;
- empilhadeiras John/Duda;
- piso, barreira, checkpoint, boost, gelo e óleo para corrida;
- VFX de push/sucesso/deadlock no puzzle;
- VFX de trail/drift/finish na corrida.

Esses assets são substituíveis sem mudar o domínio de gameplay.

## Áudio

Como o repositório ainda não contém música final suficiente para todos os novos contextos, o Editor pode gerar áudio starter local:

- UI confirm;
- passo;
- push;
- sucesso;
- erro/deadlock;
- countdown;
- finish;
- loops starter de menu, puzzle e corrida.

`AudioService` e `MusicService` respeitam os volumes persistidos. O material starter deve ser substituído pelos assets finais sem trocar a arquitetura.

## One-click authoring

Após compilação limpa, executar:

`Tools > TW08 > Production > Build Full Production Expansion`

O comando é responsável por:

1. recuperar/normalizar fases base 01–03;
2. criar dados de personagens/campanhas/save;
3. criar arte e áudio starter locais;
4. criar menu/hub/seletores/configurações/créditos;
5. criar as 9 cenas puzzle;
6. criar as 3 cenas de corrida;
7. injetar VFX e áudio;
8. validar estrutura/dados;
9. atualizar Build Settings;
10. abrir o Main Menu.

## Testes adicionados

- custo de movimento em piso frio + Undo;
- portas dinâmicas;
- doca tipada correta/incorreta;
- migração save v1 -> v2;
- thresholds de medalha de corrida.

Os testes existentes de puzzle, validator, redo, save, apresentação e bootstrap permanecem parte do gate.

## Gate de validação

A expansão não deve ser considerada verde até passar por:

1. Unity 6.3.0f1 sem erros de compilação.
2. `Build Full Production Expansion` sem exceção.
3. EditMode Test Runner completo.
4. PlayMode Test Runner completo.
5. Menu/hub navegável por teclado/mouse/gamepad.
6. John e Duda testados em puzzle.
7. Fases 04–09 playtestadas manualmente.
8. Três pistas testadas com checkpoint/lap/timer/finish.
9. Save/reload de operador, puzzle, corrida e áudio.
10. Windows Development Build.

## Estado atual

- Implementação e authoring publicados na branch.
- Novas fases verificadas por solver independente.
- Revisão estática realizada sobre APIs internas reutilizadas.
- **Compilação/runtime desta expansão ainda não executados no ambiente que produziu os commits.**
- O próximo gate real é o Unity do owner.
