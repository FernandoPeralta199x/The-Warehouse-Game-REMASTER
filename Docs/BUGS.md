# BUGS — The Warehouse Nº 08

Caça de defeitos por leitura de código e raciocínio adversarial.
Data: 2026-08-29. Nenhum arquivo do projeto foi alterado; nenhuma instância do Unity foi aberta.

**Método.** Além da leitura, foi construída uma transcrição fiel de `PuzzleBoardModel.TryMove/Slide/TryUndo` +
`PuzzleRuntime.ApplySwitchGroups` em Python e rodada contra as 27 fases de `Docs/level-layouts.json`.
Todas as sequências de comandos citadas abaixo foram executadas nessa transcrição e são reproduzíveis
tecla a tecla no jogo (W/A/S/D ou setas; `U`=cima, `D`=baixo, `L`=esquerda, `R`=direita).

---

## ÍNDICE POR SEVERIDADE

| # | Severidade | Título | Arquivo |
|---|---|---|---|
| 1 | CRÍTICO | Esteira invertida coloca jogador e carga na mesma célula | `PuzzleBoardModel.cs` |
| 2 | CRÍTICO | Undo devolve carga para dentro de porta fechada — fase invencível | `PuzzleRuntime.cs` |
| 3 | CRÍTICO | Undo + Redo depois de concluir paga Créditos de Turno infinitamente | `PuzzleRuntime.cs` / `PuzzleHudController.cs` |
| 4 | ALTO | Carga do tipo errado encalha em canto sobre alvo — fase invencível sem alarme | `SimpleDeadlockDetector.cs` |
| 5 | ALTO | Redo é permanentemente impossível em fases com gelo ou esteira | `PuzzleRuntime.cs` |
| 6 | ALTO | Névoa de guerra apaga o próprio jogador e as cargas | `PuzzleFogOfWar.cs` |
| 7 | ALTO | Fake-null no importador de campanha grava fases nulas | `TW08CampaignExpansionImporter.cs` |
| 8 | ALTO | Fake-null na geração de arte starter | `TW08ProductionArtSetup.cs` / `TW08ProductionSceneUpgrader.cs` |
| 9 | MÉDIO | Cutscene captura `timeScale` já zerado e congela o jogo | `NarrativeOverlayController.cs` |
| 10 | MÉDIO | Validador ignora gelo, esteira, robô, botão, portão e parede falsa | `PuzzleLevelValidator.cs` |
| 11 | MÉDIO | Migração de save lança exceção não tratada e o jogo roda sem salvar | `SaveMigrationPipeline.cs` / `SaveManager.cs` |
| 12 | MÉDIO | Progresso vive em dois lugares que não conversam | `PuzzleProgressStore.cs` / `SaveManager.cs` |
| 13 | MÉDIO | `UIMotion.Sequence.Play()` não tem a guarda de Play Mode que o resto tem | `UIMotion.cs` |
| 14 | BAIXO | Migração v1→v2 apaga as configurações de áudio do jogador | `SaveMigrationV1ToV2.cs` |
| 15 | BAIXO | Ferramenta sem estoque continua ocupando slot equipado | `SaveGameData.cs` / `ShopController.cs` |
| 16 | BAIXO | `EvaluateMedal` nunca devolve 0 — "SEM MEDALHA" é código morto | `PuzzleProgressStore.cs` |

---

### [CRÍTICO] Esteira invertida coloca jogador e carga na mesma célula

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleBoardModel.cs:184-208` (e `Slide` em `:251-308`, `IsSlideTargetFree` em `:292-308`)

**Cenário:** `TW08_Level17_AutomaticRoute`. Jogador começa em `(1,3)`. Duas teclas:

1. `U` — jogador vai para `(1,4)`.
2. `R` — empurra a carga de `(2,4)`.

Passo a passo do que o motor faz no comando 2:
- `destination = (2,4)` tem carga; `crateDestination = (3,4)`, que é esteira apontando para a **esquerda**.
- `Slide((3,4), dir=Right, vacated=(2,4))`: a esteira impõe `step = (-1,0)`; `next = (2,4)` é a célula
  vacada, portanto tratada como livre → a carga volta para `(2,4)`. `(2,4)` é piso comum → `crateFinal = (2,4)`.
- `crateByPosition.Remove((2,4))` seguido de `Add((2,4))` — a carga não saiu do lugar.
- `playerFinal = Slide((2,4), Right, null)`: `(2,4)` é piso comum, devolve `(2,4)` na hora.
- `PlayerPosition = (2,4)`.

**Esperado:** o comando ser recusado (a carga não tem para onde ir), ou o jogador parar antes dela.

**Observado:** jogador e carga ocupam `(2,4)`. O sprite do John fica em cima da caixa. A partir daí o jogador
está "dentro" de uma carga: `IsFree((2,4))` é falso, o motor deixa de enxergar aquela célula como livre para
qualquer outra carga, e o jogador pode sair andando de dentro dela e voltar a empurrá-la por um lado que
seria fisicamente impossível.

Também reproduz, com sequências curtas, em:
- `TW08_Level16_ConveyorOnline` — `URRRURRDL` (9 comandos; sobreposição em `(6,4)`).
- `TW08_Level20_ProductionLine` — `UURRDRRRDL` (10 comandos; sobreposição em `(5,3)`).

**Variante da mesma causa (carga atravessa o jogador):** se a célula de origem do empurrão também for
esteira contrária, a carga desliza *através* do jogador e para na célula de onde ele saiu, enquanto ele
avança para a célula que ela ocupava — os dois trocam de lugar atravessando um ao outro. Tabuleiro 5x3,
jogador `(1,2)`, carga `(2,2)`, esteiras `<` em `(2,2)` e `(3,2)`, tecla `R`: resultado jogador `(2,2)`,
carga `(1,2)`.

**Causa:** `Slide` nunca considera a posição do jogador como obstáculo (`IsSlideTargetFree` só olha paredes,
bloqueios dinâmicos e `crateByPosition`), e `TryMove` nunca verifica `crateFinal != playerFinal` — a única
checagem de colisão pós-deslize é contra robôs (`:186` e `:197`).

**Correção sugerida:** em `TryMove`, depois de `crateFinal` e `playerFinal` estarem calculados, recusar o
comando inteiro quando `crateFinal == playerFinal` ou quando `crateFinal == previousPlayer` (fazendo o
mesmo rollback já usado no caso do robô, `:200-202`). Alternativamente, passar a posição atual do jogador
para `Slide` como célula ocupada durante o deslize da carga.

**Nota de paridade:** `Tools/puzzle/tw08_solver.py` reproduz o mesmo defeito (`slide()` em `:366-401` e
`solve()` em `:601-614` também não comparam `cfinal` com `pfinal`), então a prova de solvabilidade **não**
diverge do motor aqui. Verifiquei que nenhuma das 27 soluções ótimas provadas passa por um estado
sobreposto — mas qualquer correção precisa ser aplicada nos **dois** arquivos ao mesmo tempo, senão a
paridade quebra e as fases 16, 17 e 20 precisam ser reprovadas.

---

### [CRÍTICO] Undo devolve carga para dentro de porta fechada — fase fica invencível

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleRuntime.cs:258-266` (a condição de `allClosed`),
com `PuzzleBoardModel.TryUndo` em `PuzzleBoardModel.cs:315-341` e `SetDynamicBlocked` em `:343-361`.

**Cenário:** `TW08_Level26_DeadArchive`. Sensor em `(6,2)`, porta em `(7,3)`. Sequência exata verificada:

```
D D R U R R R L L L L U R R R R R L D R U R U
```

Marcos importantes:
- comando 7: uma carga chega ao sensor `(6,2)` → a porta `(7,3)` abre.
- comando 17: outra carga é empurrada para dentro da porta, parando em `(7,3)`.
- comando 20: a carga do sensor sai de `(6,2)` → o grupo pede fechamento, mas `(7,3)` está ocupada pela
  carga, então a transição atômica mantém o grupo aberto (correto).
- comando 22: a carga de `(7,3)` é empurrada para `(8,3)`; o **jogador** para em cima de `(7,3)`, e o grupo
  continua aberto porque a porta está ocupada por ele (correto).
- comando 23: o jogador sai de `(7,3)`. Agora nada ocupa a porta → `dynamicBlocked = {(7,3)}`.

Agora o jogador aperta **Z (Undo) duas vezes**:
- Undo #1 devolve o jogador para `(7,3)`. `ApplySwitchGroups` roda: `SetDynamicBlocked((7,3), true)` devolve
  `false` (o jogador está lá), mas `DynamicBlockedCells.Contains((7,3))` é `true`, então a linha 261 conta
  isso como "fechou com sucesso". **O jogador fica dentro de uma porta fechada.**
- Undo #2 devolve a carga para `(7,3)` (`move.CrateFrom`), sem checar que a célula está bloqueada.
  `ApplySwitchGroups` roda de novo e comete o mesmo engano. **A carga fica dentro da porta fechada.**

**Esperado:** desfazer devolve o tabuleiro a um estado legal; a porta reabre porque está ocupada.

**Observado:** estado final `player=(6,3)`, `crates=[(2,4),(3,4),(7,2),(7,3)]`, `dynamicBlocked=[(7,3)]`.
A carga em `(7,3)` está imóvel para sempre — testei os quatro lados na transcrição do motor:

```
empurrar de (7,4) para baixo  -> RECUSADA (IsBlocked)
empurrar de (8,3) para esquerda -> RECUSADA (IsBlocked)
empurrar de (6,3) para direita  -> RECUSADA (IsBlocked)
empurrar de (7,2) para cima     -> impossível, (7,2) tem outra carga
```

Como `(7,3)` é o único caminho para os alvos `(8,3)` e `(9,3)`, a fase se torna invencível. Não há aviso:
o alarme de carga travada não dispara (a célula não é canto de paredes) e o único caminho é Reiniciar.

**Causa:** `PuzzleRuntime.cs:261`
```csharp
if (!Board.SetDynamicBlocked(door, true) && !Board.DynamicBlockedCells.Contains(door))
```
A intenção de `&& !Contains(door)` é "porta já fechada também conta como sucesso". Mas `SetDynamicBlocked`
devolve `false` por **dois** motivos diferentes — "já estava no conjunto" e "tem jogador/carga em cima" — e
a condição não os distingue. Depois de um Undo, os dois acontecem juntos e a porta fica fechada com algo
dentro. Segundo fator: `PuzzleBoardModel.TryUndo:329-330` reinsere a carga em `move.CrateFrom` sem
verificar `IsFree`.

**Correção sugerida:** trocar a condição por uma pergunta direta ao estado, não ao valor de retorno:
```csharp
bool occupied = Board.PlayerPosition == door || Board.Crates.ContainsKey(door);
if (occupied || !Board.DynamicBlockedCells.Contains(door)) { allClosed = false; }
```
(com `SetDynamicBlocked(door, true)` chamado antes, só pelo efeito). E, por defesa em profundidade, fazer
`TryUndo` recusar quando `move.CrateFrom` não estiver livre, devolvendo `false` para o histórico restaurar
o movimento (`PuzzleRuntime.Undo` já trata esse retorno em `:162-166`).

---

### [CRÍTICO] Undo + Redo depois de concluir paga Créditos de Turno infinitamente

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleRuntime.cs:155-178` (Undo sem guarda de conclusão) e
`Assets/_Project/Scripts/UI/PuzzleHudController.cs:264-286` (`OnCompleted` sem guarda de uma-vez-só)

**Cenário:** qualquer fase. Conclua normalmente. O turno fecha, `LevelCompleted` dispara,
`CommitPuzzleShift` credita (ex.: 100 concluído + 100 platina + 50 sem ferramentas + 50 sem dicas +
50 primeira tentativa = 350, cortado pelo teto em **250 créditos**). Agora, com a tela de resultado no ar:

1. Aperte **Z** (Undo). `PuzzleRuntime.Undo()` não consulta `Board.IsComplete`, então o desfazer é aceito e
   o tabuleiro deixa de estar completo.
2. Aperte **Y** (Redo). `PuzzleRuntime.Redo()` chama `Board.TryMove`, o tabuleiro completa de novo e
   `EvaluateBoardState()` (`:203`) dispara `LevelCompleted` outra vez.
3. `PuzzleHudController.OnCompleted` chama `saveManager.CommitPuzzleShift(...)` de novo — não existe nenhum
   `if (hasReport) return;`. `record.attempts` continua 1, então o bônus de PRIMEIRA TENTATIVA é pago de
   novo; o total volta a bater o teto.

Repita Z+Y: **+250 créditos por par de teclas**, sem limite. Os botões da HUD também servem: `undoButton`
fica `interactable` porque `runtime.UndoCount > 0` (`PuzzleHudController.cs:318`), e o `redoButton` acende
logo depois (`:322`).

**Esperado:** o turno é fechado e pago exatamente uma vez por entrada na fase.

**Observado:** a Oficina N-8 inteira é comprável em menos de um minuto; a economia descrita em
`Docs/SHOP_ECONOMY.md` (100–250 créditos por fase) deixa de existir.

**Causa:** `PuzzleRuntime.TryMove` protege contra jogar depois do fim (`:107`, `Board.IsComplete`), mas
`Undo` e `Redo` não têm a mesma trava, e o consumidor do evento não é idempotente.

**Correção sugerida:** duas travas independentes, porque cada uma sozinha ainda deixa buraco:
1. Em `PuzzleHudController.OnCompleted`, sair cedo quando `hasReport` já for `true`.
2. Em `PuzzleRuntime`, guardar um `bool shiftCommitted` zerado só em `Initialize()`, e não redisparar
   `LevelCompleted` enquanto ele estiver marcado (ou bloquear `Undo`/`Redo` quando `Board.IsComplete`, o
   que também resolve o problema de o jogador continuar mexendo no tabuleiro atrás da tela de resultado).

---

### [ALTO] Carga do tipo errado encalha em canto sobre alvo — fase invencível sem alarme

**Arquivo:** `Assets/_Project/Scripts/Puzzle/SimpleDeadlockDetector.cs:12-17`

**Cenário:** `TW08_Secret08_LeftoverMap`. `(5,4)` é alvo com `goalRequirement = HeavyCrate`, tem parede em
cima (`(5,5)`) e parede à direita (`(6,4)`). Sequência de **8 comandos** a partir do início:

```
D R R U L U R R
```

Isso leva uma carga **comum** (`Crate`, kind 1) para `(5,4)`.

**Esperado:** o jogo avisar que o turno virou impossível (o alarme de carga travada existe justamente para
isso — `PuzzleRuntime.StaticDeadlockDetected` → `PuzzleHudController.TriggerDeadlockAlert`).

**Observado:** nenhum alarme. A carga é intocável — para tirá-la de `(5,4)` seria preciso empurrá-la para
baixo (jogador precisaria estar em `(5,5)`, parede) ou para a esquerda (jogador em `(6,4)`, parede). E
`EvaluateCompletion` nunca aceitará uma `Crate` num alvo que exige `HeavyCrate`. A fase está perdida e o
jogador só descobre por exaustão.

Reproduzido também, com sequências de 7 a 18 comandos, em:
`TW08_Level24_OldGenerator` (`DDRULURRRRURDLDRRR`, carga em `(9,2)`), `TW08_Level25_DeadWeight`,
`TW08_Level29_LockdownN8`, `TW08_Level30_LogisticsCore`, `TW08_Secret04_EliasRoute` (todos `DRRULURR`),
`TW08_Secret09_EliasLastShift` (`URRDRDRU`) e `TW08_Secret10_DudasPath` (`DRRULUR`).

**Causa:** `SimpleDeadlockDetector.cs:14`
```csharp
if (board.IsGoal(crate)) { continue; }
```
Estar sobre um alvo é tratado como "está resolvido", mas o modelo tem tipos de carga: um alvo com
`GoalRequirement` incompatível não resolve nada. O solver Python faz isso certo — `corner_deadlock`
(`tw08_solver.py:435-449`) consulta `level.goal_req` antes de pular.

**Correção sugerida:** espelhar a regra do solver:
```csharp
if (board.IsGoal(crate)
    && (!board.TryGetGoalRequirement(crate, out PuzzleEntityKind req)
        || board.GetCrateKind(board.Crates[crate]) == req))
{
    continue;
}
```
`TryGetGoalRequirement` e `GetCrateKind` já são públicos em `PuzzleBoardModel` (`:406` e `:436`).

---

### [ALTO] Redo é permanentemente impossível em fases com gelo ou esteira

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleRuntime.cs:187`

**Cenário:** `TW08_Level12_ColdCorridor`, jogando a solução ótima provada. No **comando 7** (tecla `R`) o
jogador entra no gelo em `(2,3)` e desliza até `(3,3)`: `PlayerFrom = (1,3)`, `PlayerTo = (3,3)`.

Aperte `Z` (Undo) e depois `Y` (Redo).

**Esperado:** o movimento é refeito.

**Observado:** Redo falha em silêncio. `PuzzleRuntime.Redo` reconstrói a direção como
`move.PlayerTo - move.PlayerFrom` = `(2,0)`; `PuzzleBoardModel.TryMove` recusa de cara porque
`direction.ManhattanLength != 1` (`PuzzleBoardModel.cs:135-138`). O movimento volta para a pilha de redo
(`:191`), então o botão REFAZER continua aceso e continua não fazendo nada para sempre — a partir daquele
ponto o histórico de redo fica permanentemente travado.

Primeiros comandos afetados, medidos ao longo das soluções provadas:
- `TW08_Level12_ColdCorridor` — comando 7, `R`, `(1,3)→(3,3)`
- `TW08_Level14_FrozenSensor` — comando 5, `U`, `(3,1)→(3,4)`
- `TW08_Level15_Chamber08C` — comando 8, `R`, `(2,3)→(4,3)`
- `TW08_Secret10_DudasPath` — comando 16, `U`, `(4,1)→(4,4)`

O caso da esteira é pior porque o vetor pode até virar `(0,0)` (esteira que devolve o jogador para a
célula de onde ele saiu), e aí `ManhattanLength == 0`.

**Causa:** `PuzzleMove` guarda a posição **final** do jogador, já depois do deslize, e `Redo` assume que a
diferença entre origem e destino é o comando digitado. Isso só é verdade em piso comum.

**Correção sugerida:** guardar a direção do comando no próprio `PuzzleMove` (um campo
`GridCoordinate Direction`, preenchido em `TryMove` nas duas construções, `PuzzleBoardModel.cs:171` e
`:209`) e usá-la em `Redo`. Como efeito colateral bom, isso também torna o `Redo` correto em fases com
robô, onde a direção reconstruída poderia ser unitária mas apontar para o lado errado.

---

### [ALTO] Névoa de guerra apaga o próprio jogador e as cargas

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleFogOfWar.cs:99-128` (`IndexScene`) e `:135-165` (`LateUpdate`)

**Cenário:** `TW08_Level26_DeadArchive` (`fogMode: Memory`, `fogRadius: 3`), `TW08_Level27_GhostRoute` ou
`TW08_Secret05_DarkWorkshop`. Jogador começa em `(1,3)`. Ande até a coluna 8 (por exemplo `DDRRRRRRR`).

**Esperado:** o operador e as cargas próximas sempre visíveis; escurece só o que está longe **dele**.

**Observado:** o sprite do John desaparece (alfa 0.06) assim que ele passa de 3 células de distância da
célula **onde a fase começou**. Cargas empurradas somem do mesmo jeito, e cargas paradas perto do ponto
inicial continuam acesas mesmo com o jogador do outro lado do mapa.

**Causa:** `IndexScene` roda uma vez em `OnEnable` e monta `cellRenderers[célula] = [renderers]` usando a
posição do transform **naquele instante**. `LateUpdate` calcula o alfa por chave do dicionário
(`TargetAlphaFor(entry.Key, player)`) e aplica a todos os renderizadores daquele balde. O John e as cargas
são `SpriteRenderer` criados pelo builder (`TW08PuzzleSceneBuilder.cs:190` e `:217`), então entram no
índice congelados na célula inicial e nunca são reindexados. O comentário em `:96-97` diz "carga e jogador
são tratados à parte por posição a cada frame" — esse tratamento não existe no arquivo.

**Correção sugerida:** em `LateUpdate`, calcular o alfa pela posição **atual** do renderizador
(`WorldToCell(renderer.transform.position)`) em vez da chave do dicionário, ou manter uma lista separada
de renderizadores móveis (o `playerView` e os `crateViews` do `PuzzleRuntime`) tratados por posição a cada
frame, deixando o índice por célula só para cenário estático.

---

### [ALTO] Fake-null no importador de campanha grava fases nulas na campanha

**Arquivo:** `Assets/_Project/Scripts/Editor/TW08CampaignExpansionImporter.cs:132-145`

**Cenário:** rodar `Tools > TW08 > Production > Import Campaign Levels From JSON` (ou a entrada de
batchmode `ImportFromCommandLine`) num projeto onde a importação vá reimportar assets — que é o caso normal
quando as 27 fases são criadas pela primeira vez.

```csharp
foreach (LevelDto dto in root.levels)
{
    createdAssets[dto.id] = CreateOrUpdateLevelAsset(dto);   // wrappers nativos capturados aqui
}

AssetDatabase.SaveAssets();
AssetDatabase.Refresh();                                      // <- pode substituir os wrappers

RegisterMainCampaign(root.levels, createdAssets);             // usa as referências antigas
RegisterSecretCampaign(root.levels, createdAssets);
```

**Esperado:** a campanha principal e a secreta apontam para os 27 `PuzzleLevelDefinition`.

**Observado:** as entradas gravadas em `el.FindPropertyRelative("level").objectReferenceValue = assets[dto.id]`
(`:335` e `:371`) recebem `None`, porque `AssetDatabase.Refresh()` invalida os wrappers de `UnityEngine.Object`
criados antes dele mesmo com o asset íntegro em disco. Consequências em cadeia: `PuzzleLevelSelectController`
mostra entradas vazias, `PuzzleProgressStore.IsUnlocked` (`PuzzleProgressStore.cs:65`) devolve `false` para
todas as fases seguintes, e `TW08PuzzleSceneBuilder.ResolveEntrySceneName` lança
`"Puzzle campaign entry NN is invalid"`. Pior: na **segunda** execução do importador, `RegisterMainCampaign`
monta `existingIds` lendo `lvl.LevelId` das entradas já gravadas (`:316-323`); como as entradas estão
fake-null, o `if (lvl != null)` falha, nenhum id entra no conjunto e as 27 fases são **anexadas de novo**,
duplicando a campanha.

**Causa:** o padrão fake-null do Unity — referência a `UnityEngine.Object` guardada através de
`AssetDatabase.Refresh()`. O projeto já conhece o padrão e o trata corretamente em
`TW08NarrativeSetup.EnsureCatalog` (`TW08NarrativeSetup.cs:141-145`, com comentário explícito) e em
`TW08FullProductionExpansionSetup.ReloadStableExpansionData`. Este arquivo ficou de fora.

**Correção sugerida:** não guardar os objetos. Depois do `Refresh()`, recarregar por caminho, exatamente
como `EnsureCatalog` faz:
```csharp
PuzzleLevelDefinition asset = AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>(
    $"{TW08ExpansionDataSetup.CampaignRoot}/{dto.id}.asset");
```
dentro de `RegisterMainCampaign`/`RegisterSecretCampaign`, e trocar `createdAssets` por um dicionário de
caminhos (`string → string`).

---

### [ALTO] Fake-null na geração de arte starter

**Arquivo:** `Assets/_Project/Scripts/Editor/TW08ProductionArtSetup.cs:145-165` e
`Assets/_Project/Scripts/Editor/TW08ProductionSceneUpgrader.cs:26-55`

**Cenário A — `TW08ProductionArtSetup.EnsureStarterPixelArt`.** `EnsureProductionArtAssets` (`:39`) carrega
`catalog` e `john`, e passa os dois para `EnsureStarterPixelArt(catalog, john)`. Lá dentro:

```csharp
AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);  // :145

AssignCatalogSpriteIfEmpty(catalog, "floorPrimary", floorPrimary);   // :147  usa wrapper de antes
...
SerializedObject serializedJohn = new(john);                          // :154  idem
```

Numa importação limpa (pasta `GeneratedStarter` vazia, ou seja, o primeiro run do pipeline), as ~20 PNGs
são escritas e importadas, o `Refresh` reconstrói os wrappers, e `catalog`/`john` viram fake-null.
`new SerializedObject(john)` sobre um objeto inválido lança; e `AssignCatalogSpriteIfEmpty(catalog, ...)`
escreve num wrapper morto — o catálogo fica com os campos de sprite vazios e o jogo abre com os quadrados
do protótipo em vez da arte starter.

**Cenário B — `TW08ProductionSceneUpgrader.UpgradeVerticalSlicePresentation`.** Pior, porque acumula três
invalidadores sobre a mesma referência:

```csharp
TW08ArtCatalog catalog = TW08ProductionArtSetup.EnsureProductionArtAssets();  // :26 — já devolvido depois de um Refresh interno
TW08StarterArtRefinement.Regenerate();                                        // :27 — faz outro Refresh (TW08StarterArtRefinement.cs:50)
...
Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);  // :41 — troca de cena
...
ApplyEnvironmentSkin(catalog);                                                // :53
ConfigureJohn(john, runtime, catalog);                                        // :54
```

Rodar `Tools > TW08 > Production > Upgrade Vertical Slice Presentation` com a pasta starter zerada
resulta em `NullReferenceException` no primeiro `ApplyEnvironmentSkin`, ou — se o wrapper sobreviver
parcialmente — nas três cenas salvas com sprites nulos.

**Esperado:** o upgrade aplicar a skin em `TW08_Level01/02/03`.

**Observado:** exceção ou cenas gravadas sem sprite, e as cenas ficam salvas assim (`SaveScene` em `:57`
roda mesmo depois do erro nas duas primeiras).

**Causa:** mesmo padrão fake-null do bug 7.

**Correção sugerida:** em `EnsureProductionArtAssets`, mover o `AssetDatabase.Refresh` de `:145` para
**antes** de qualquer uso de `catalog`/`john`, recarregando os dois por caminho logo em seguida
(`AssetDatabase.LoadAssetAtPath<TW08ArtCatalog>(CatalogPath)` e `...<DirectionalSpriteSet>(JohnSpriteSetPath)`).
Em `TW08ProductionSceneUpgrader`, não guardar o retorno: chamar `EnsureProductionArtAssets()` e
`Regenerate()` pelo efeito, e recarregar o catálogo por caminho **dentro** do laço, depois de cada
`OpenScene`.

---

### [MÉDIO] Cutscene captura `timeScale` já zerado e congela o jogo

**Arquivo:** `Assets/_Project/Scripts/Narrative/NarrativeOverlayController.cs:487-509` (linha `:491`)

**Cenário:** conclua uma fase. `NarrativeDirector.OnLevelCompleted` agenda a cutscene de conclusão com
`UIMotion.Chain().Wait(0.65f)` (`NarrativeDirector.cs:169`), e esse encadeamento roda em **tempo não
escalado** (`UIMotion.RunSequence` usa `WaitForSecondsRealtime`). Dentro dessa janela de 0,65 s, aperte
**ESC**: `PauseService.SetPaused(true)` põe `Time.timeScale = 0`.

Quando a espera vence, o overlay abre e executa:
```csharp
previousTimeScale = Time.timeScale;   // 0
Time.timeScale = 0f;
```
Ao fim da cutscene, `RestoreGameplay()` faz `Time.timeScale = previousTimeScale` — devolve **0**.

**Esperado:** o overlay não pode "aprender" que o tempo normal do jogo é zero.

**Observado:** a cutscene termina e o jogo fica congelado. É recuperável (`PauseService` guardou o valor
certo e um segundo ESC despausa, e `SceneLoader.TryLoadImmediate` faz `Time.timeScale = 1f` em `:29`), mas
enquanto isso o jogador vê o jogo travado com a tela de resultado inerte.

**Causa:** `PauseService.SetPaused` já protege contra isso (`PauseService.cs:40`:
`Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale`); `NarrativeOverlayController.SuspendGameplay`
não replicou a guarda.

**Correção sugerida:** copiar a mesma linha:
```csharp
previousTimeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : Time.timeScale;
```

---

### [MÉDIO] Validador ignora gelo, esteira, robô, botão, portão temporizado e parede falsa

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleLevelValidator.cs:127-129`

**Cenário:** `Validate` só olha `walls`, `goals`, `goalRequirements`, `crates`, `costlyCells` e
`switchGroups`. Todas as mecânicas novas passam sem checagem alguma. Consequências concretas de dados que
o validador aceita hoje:

1. `PuzzlePatrolDefinition.Route` com uma célula que também está em `walls`: o robô atravessa a parede e
   passa a bloquear uma célula que o jogador vê como livre. `GetPatrolCells` (`PuzzleBoardModel.cs:227`)
   não filtra nada.
2. Rota de robô que passa pela célula inicial do jogador no passo 0: `PuzzleBoardModel` valida
   `playerStart` contra paredes e cargas (`:125-128`) mas não contra robôs — a fase abre com os dois
   sobrepostos.
3. `timedBlocks` sobre a célula inicial do jogador ou sobre uma carga inicial: `IsBlocked` passa a ser
   verdadeiro para uma célula ocupada, e a carga fica intocável até o prazo vencer.
4. Botão de direção sobre uma parede: nunca dispara — a fase é publicada com um gimmick morto.
5. `fakeWalls` que também estejam em `walls`: `PuzzleBoardModel` só lê `walls`, então a célula é uma parede
   de verdade, enquanto `PuzzleFakeWallView` a apresenta como mentira revelável. O jogador anda até ela e
   bate. (Nas 27 fases atuais os `fakeWalls` caem em piso livre, então isso ainda não acontece — é a
   próxima fase que quebra.)
6. Esteira e gelo sobrepostos na mesma célula: `Slide` dá precedência à esteira (`PuzzleBoardModel.cs:263`),
   o que é uma regra invisível que o level designer não tem como saber.

**Esperado:** o mesmo rigor aplicado a paredes e sensores.

**Observado:** o único gate real é o solver, que não roda contra o `.asset` final — ver a nota de
paridade abaixo.

**Correção sugerida:** acrescentar em `Validate` checagens de `IsInside` + não-sobreposição-com-parede para
`iceCells`, `conveyors[].Position`, `directionButtons`, `timedBlocks[].Position`, `fakeWalls` e cada célula
de cada `Patrols[].Route`; recusar rota vazia, rota com salto não-ortogonal entre passos consecutivos,
célula que seja simultaneamente gelo e esteira, e `timedBlocks`/`patrols[0]` coincidindo com `playerStart`
ou com uma carga inicial.

---

### [MÉDIO] Migração de save lança exceção não tratada e o jogo passa a rodar sem salvar

**Arquivo:** `Assets/_Project/Scripts/Save/SaveMigrationPipeline.cs:31-42` e
`Assets/_Project/Scripts/Save/SaveManager.cs:39-48`

**Cenário A:** alguém sobe `GameConfig.saveVersion` para 4 (é um campo `[SerializeField]`, editável no
Inspector) sem registrar uma `SaveMigrationV3ToV4`. No próximo boot com um save v3 em disco:
`JsonSaveService.Load` (`:31-34`) vê `3 < 4` e chama `MigrateTo`, que não acha migração para a versão 3 e
lança `InvalidOperationException`. A exceção sobe por `SaveManager.Awake` sem nenhum `try`.

**Cenário B:** save cujo `payload` traga `"version": 0` (arquivo truncado e reescrito, ou save de uma build
pré-versionamento). Mesmo caminho, mesma exceção.

**Esperado:** save ilegível cai para o padrão (`new SaveGameData()`), como já acontece quando o checksum
falha (`JsonSaveService.cs:80-83`).

**Observado:** o Unity engole a exceção de `Awake` e loga; `Data` fica `null` e o resto de `Awake` nunca
roda (nem `CharacterSelectionState.Select`, nem `ApplyAudioSettingsToRuntime`). Como todo o resto do jogo
testa `saveManager?.Data == null` e sai em silêncio (`PuzzleToolService.cs:85`, `:135`;
`SaveManager.CommitPuzzleShift:88`; `ShopController.Buy:151`), o jogo **parece** funcionar: dá para jogar,
concluir fases e "comprar" na loja sem que nada seja gravado. O jogador só descobre ao reabrir o jogo.

**Causa:** `MigrateTo` lança em vez de degradar, e ninguém captura.

**Correção sugerida:** envolver a migração num `try/catch` dentro de `JsonSaveService.Load`, caindo para
`new SaveGameData()` com `Debug.LogWarning`, do mesmo jeito que `TryLoad` já faz. E, em `SaveManager.Awake`,
garantir `Data ??= new SaveGameData()` depois do `Load()`. Bônus barato: tratar
`data.version > currentVersion` (save de build mais nova) como save incompatível em vez de carimbar a
versão por cima (`JsonSaveService.cs:36`).

---

### [MÉDIO] Progresso de campanha vive em dois lugares que não conversam

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleProgressStore.cs:10-49` vs
`Assets/_Project/Scripts/Save/SaveManager.cs:59-126`

**Cenário:** ao concluir uma fase, `PuzzleHudController.OnCompleted` grava nos **dois**: `PuzzleProgressStore`
(PlayerPrefs, chaves `tw08.puzzle.*`) e `SaveManager` (JSON com checksum + backup em `persistentDataPath`).
Mas o desbloqueio da campanha lê **só** PlayerPrefs — `PuzzleProgressStore.IsUnlocked:65` chama
`IsCompleted(previous.Level.LevelId)`, que é PlayerPrefs. `SaveGameData.lastUnlockedLevel` e
`LevelProgressRecord.completed` não são consultados por nenhum caminho de desbloqueio.

**Esperado:** uma fonte de verdade para progresso.

**Observado:** consequências assimétricas e confusas para o jogador:
- Apagar o `tw08-save.json` não reseta nada da campanha (as fases continuam abertas).
- Limpar o registro/PlayerPrefs (troca de máquina, perfil, `PlayerPrefs.DeleteAll` de um menu de debug)
  tranca a campanha de volta na fase 1 mesmo com o save íntegro, com medalhas e créditos preservados.
- Um eventual sync de save em nuvem levaria os créditos e as medalhas, mas não o desbloqueio.

**Causa:** `PuzzleProgressStore` nasceu como store de protótipo e continuou vivo depois de o `SaveManager`
assumir o progresso.

**Correção sugerida:** fazer `PuzzleProgressStore.IsCompleted`/`GetMedal`/`GetBestMoves` consultarem o
`SaveManager` quando ele existir na cena, mantendo PlayerPrefs só como fallback de cena isolada; ou remover
a duplicidade e ler `SaveGameData.levels` direto no `PuzzleLevelSelectController`.

---

### [MÉDIO] `UIMotion.Sequence.Play()` não tem a guarda de Play Mode que o resto do arquivo tem

**Arquivo:** `Assets/_Project/Scripts/Motion/UIMotion.cs:430-444`

**Cenário:** `UIMotion.Play` (`:456-473`) trata explicitamente o fora-de-Play-Mode aplicando o estado final
na hora e comentando o porquê. `Sequence.Play()` não faz isso: chama `Host.StartCoroutine` direto. Fora do
Play Mode, `Host` cria um `GameObject` com `HideFlags.HideAndDontSave` (que nem recebe `DontDestroyOnLoad`,
`:86`) e `StartCoroutine` não executa — o passo agendado nunca roda e o objeto oculto fica pendurado na
cena aberta do editor.

**Esperado:** mesma degradação do `Play`.

**Observado:** hoje ninguém chama `Sequence.Play()` de código de editor — `HudFx.Delayed` (`HudFx.cs:157-163`)
tem a guarda antes de encadear, e os outros dois chamadores
(`NarrativeOverlayController.cs:252` e `NarrativeDirector.cs:169`) são de runtime. É uma armadilha
carregada: o primeiro builder de cena que usar `UIMotion.Chain()` perde os passos em silêncio.

**Correção sugerida:** replicar a guarda dentro de `Sequence.Play()`:
```csharp
if (!Application.isPlaying)
{
    foreach ((float _, Action step) in steps) step?.Invoke();
    return new MotionHandle { Finished = true };
}
```

---

### [BAIXO] Migração v1→v2 apaga as configurações de áudio do jogador

**Arquivo:** `Assets/_Project/Scripts/Save/SaveMigrationV1ToV2.cs:19-21`

**Cenário:** jogador com save v1 e volumes ajustados (por exemplo música em 0,2). Ao atualizar o jogo,
`Migrate` executa incondicionalmente `masterVolume = 1f; musicVolume = 0.8f; sfxVolume = 1f;`.

**Esperado:** preservar os valores existentes e só preencher os que faltam.

**Observado:** as configurações voltam ao padrão, e `SaveManager.ApplyAudioSettingsToRuntime` (`:194-201`)
aplica isso ao `AudioListener` e ao PlayerPrefs no mesmo boot. O jogo abre alto na cara de quem tinha
baixado o volume.

**Causa:** a migração trata os três campos como novos, mas `SaveGameData` já os tinha em v1 (o
inicializador de campo garante o padrão para saves que realmente não os traziam).

**Correção sugerida:** deixar `EnsureDefaults()` (`SaveGameData.cs:145-147`, que já faz `Clamp01`) cuidar
disso e remover as três atribuições.

---

### [BAIXO] Ferramenta sem estoque continua ocupando slot equipado

**Arquivo:** `Assets/_Project/Scripts/Save/SaveGameData.cs:131-148` (`EnsureDefaults`) e
`Assets/_Project/Scripts/UI/ShopController.cs:195`

**Cenário:** com 1 slot de equipamento: compre 1 "Rebobinar Movimento", equipe, entre numa fase e use.
`PuzzleToolService.TryUse` consome (`PuzzleToolService.cs:164`), o estoque cai a 0 e `EnsureDefaults`
remove o `ToolStackRecord` (`:141`), mas o id continua em `equippedTools` — `EnsureDefaults` só limpa
entradas em branco (`:142`).

**Esperado:** o slot volta a ficar livre quando o estoque zera.

**Observado:** de volta à Oficina, o mostrador diz `SLOTS DE FERRAMENTA // 1/1` e equipar qualquer outra
ferramenta é recusado com "Slots cheios". A barra da fase mostra "SLOT VAZIO" (porque
`GetEquippedTools` filtra por estoque, `PuzzleToolService.cs:92`), então a HUD e a loja discordam.
É recuperável — o botão EQUIPAR da linha vazia continua clicável e desequipa —, mas nada indica isso.

**Correção sugerida:** em `SaveGameData.EnsureDefaults`, depois de podar `ownedTools`, remover de
`equippedTools` todo id sem estoque.

---

### [BAIXO] `EvaluateMedal` nunca devolve 0 — "SEM MEDALHA" é código inalcançável

**Arquivo:** `Assets/_Project/Scripts/Puzzle/PuzzleProgressStore.cs:68-86`

**Cenário:** conclua qualquer fase com um número absurdo de movimentos (300 numa fase de platina 10 / ouro 13).

**Esperado, pela existência de `ShiftReportPresenter.EmptyMedalLabel = "SEM MEDALHA"`
(`ShiftReportPresenter.cs:36`) e do `_ => EmptyMedalLabel` em `:109`:** algum resultado sem medalha.

**Observado:** `EvaluateMedal` cai sempre no `return 1` final — bronze garantido. `ShiftCredits` paga
`BronzeReward` (25) em todo turno concluído (`ShiftCredits.cs:72`), e o rótulo "SEM MEDALHA" nunca aparece.
Ou a escala de bronze precisa de um teto, ou o rótulo e o `case _` devem sair.

**Correção sugerida:** decidir a regra de design e alinhar os dois lados — por exemplo, um
`bronzeMoveLimit` no `PuzzleLevelDefinition` (ou `gold * 2`) abaixo do qual `EvaluateMedal` devolve 0.

---

## PARIDADE MOTOR C# × SOLVER PYTHON — RESULTADO

Área examinada com prioridade máxima, conforme pedido. **Não encontrei divergência de regra entre
`PuzzleBoardModel.cs` e `tw08_solver.py` nas 27 fases.** O que foi verificado:

1. **Replay das 27 soluções provadas no motor C#.** Transcrevi `TryMove`/`Slide`/`IsBlocked`/`IsFree` e
   `ApplySwitchGroups` linha a linha e reexecutei as soluções de `Tools/puzzle/report-new.json`.
   Resultado: **27/27 aceitas, com custo em movimentos idêntico ao `optimalCost`** do relatório e
   `EvaluateCompletion` verdadeiro no fim. As medalhas derivadas (`platinum = optimalCost`) são atingíveis.

2. **Comparação transição a transição.** BFS sobre os estados alcançáveis de cada fase (até 60.000 estados
   por fase, ~1,4 milhão no total), aplicando as quatro direções nos dois modelos e comparando
   `(posição do jogador, cargas, esteiras invertidas, custo)`. **Zero divergências.** Confirmei em
   particular que coincidem: ordem das operações no empurrão (carga desliza primeiro, depois o jogador,
   com a carga já reposicionada no dicionário); avanço do robô em `CommandCount + 1` para validar destino,
   destino da carga, pouso da carga e pouso do jogador; prazo do portão avaliado com o relógio de **antes**
   do comando; inversão da esteira aplicada **depois** dos dois deslizes, sobre a célula final do jogador;
   `vacated` tratada como livre durante o deslize da carga; custo do movimento cobrado pela célula
   **adjacente**, não pela de pouso; e a transição atômica dos grupos de porta.

3. **Teto de iterações do `Slide`.** `guard = Width * Height` nos dois lados, com corpo de laço idêntico.
   `Docs/level-layouts-enriched.json` deriva `width`/`height` do mesmo `parse_layout` que alimenta o
   solver, então os dois orçamentos são iguais por construção. Nenhuma das 27 fases tem esteira em
   circuito fechado, mas se uma tiver, o comportamento continuará idêntico.

**Ressalvas reais que valem como risco de processo (não são divergência de regra hoje):**

- **`parse_unity_asset` ignora todas as mecânicas novas.** `tw08_solver.py:105-251` lê `walls`, `goals`,
  `crates`, `costlyCells`, `goalRequirements` e `switchGroups`. Não lê `iceCells`, `conveyors`, `patrols`,
  `directionButtons`, `timedBlocks` nem `fakeWalls`. Portanto rodar `python tw08_solver.py --assets <dir>`
  contra os `.asset` gerados trata gelo e esteira como piso comum, ignora robôs e considera portões
  temporizados sempre abertos — e devolve "solucionável" com custo ótimo errado. A prova só é válida pelo
  caminho `--layouts Docs/level-layouts.json`. **Recomendação:** fazer `parse_unity_asset` lançar
  explicitamente quando o `.asset` contiver qualquer bloco de mecânica que ele não sabe ler, para o modo
  `--assets` não produzir uma prova falsa em silêncio.

- **A poda de canto do solver herda o mesmo tipo de erro do bug 4, mas para o lado seguro.**
  `corner_deadlock` (`tw08_solver.py:435-449`) consulta `goal_req` corretamente; é o C# que não consulta.
  Ou seja, o solver é o lado certo e o motor é o lado errado — corrigir o item 4 aproxima os dois.

- **A heurística do A\* não é admissível em fases com gelo ou esteira.** `heuristic` (`:516-520`) soma
  distâncias de Manhattan supondo que cada comando aproxima uma carga em no máximo uma célula; com
  deslizamento um único comando move a carga várias casas. Isso **não** afeta a solvabilidade nem a
  validade do `replay`, mas o `optimalCost` reportado para as fases 11, 12, 14, 15, 16, 17, 20 e
  Secret10 é um limite **superior**, não necessariamente o ótimo. Como `platinum = optimalCost`, o efeito é
  apenas uma platina possivelmente mais generosa do que o pretendido — nenhuma fase fica impossível.
  Vale documentar a ressalva ou trocar por Dijkstra puro (`h = 0`) nessas fases se a platina precisar ser
  exata.

- **Divergência teórica em porta sobre parede.** `PuzzleRuntime.ApplySwitchGroups` deixa o grupo aberto
  para sempre se alguma porta estiver fora do tabuleiro ou sobre parede (`SetDynamicBlocked` devolve
  `false` e `DynamicBlockedCells.Contains` também), enquanto `door_state` do solver fecharia as demais.
  Hoje é inalcançável porque `PuzzleLevelValidator` recusa porta sobre parede (`:253`). Fica registrado
  porque a correção do bug 2 mexe exatamente nessa condição.

---

## ÁREAS EXAMINADAS SEM ACHADO

Registro explícito do que foi olhado e saiu limpo, para o silêncio não ser confundido com omissão:

- **`Slide` com esteiras em ciclo fechado.** O teto `Width * Height` protege, e o resultado é determinístico
  e idêntico ao do solver. Nenhuma das 27 fases contém ciclo.
- **Carga empurrada por cima de botão de direção.** Não ativa — `ApplyDirectionButton` só é chamada com a
  célula final do **jogador** (`PuzzleBoardModel.cs:170` e `:208`). Comportamento correto e igual ao do
  solver (`tw08_solver.py:623`).
- **Botão disparando no meio do deslize.** Não dispara: os dois `Slide` do comando terminam antes de
  `ApplyDirectionButton`. Documentado e correto.
- **Undo do toggle de esteira.** `TryUndo` reaplica `ApplyDirectionButton(move.PlayerTo)`, que é uma
  involução — correto, e não precisa de campo no `PuzzleMove`.
- **Undo no primeiro comando.** `CommandCount` é clampado por `Math.Max(0, ...)` (`:339`) e `MoveCount`
  também (`:338`); além disso o Undo só é alcançável com histórico não vazio. Não vira −1.
- **Undo desalinhando robô ou portão temporizado.** Verifiquei formalmente: cada movimento é validado
  contra `robots(CommandCount + 1)` e o estado restaurado por Undo é exatamente o que era válido em
  `CommandCount - 1`. A cadeia é consistente, e as vistas leem `board.CommandCount` em vez de manterem
  relógio próprio (`PuzzlePatrolView.cs:80`, `PuzzleTimedGateView.cs:96`) — está certo.
- **Portão temporizado fechando com o jogador em cima.** Não pode acontecer: o prazo é modelado como
  "fechado até `opensAfterCommands`, aberto para sempre depois" (`PuzzleTimedBlockDefinition.cs:33`), então
  a célula nunca refecha. Não existe o caso.
- **Robô chegando numa célula com carga parada.** O modelo permite (`GetPatrolCells` não filtra cargas), e
  o solver permite igual — é paridade, não bug. Vale só como decisão de design a confirmar.
- **Robô encurralando o jogador (estado sem movimento legal).** Varri 300.000 estados alcançáveis de
  `TW08_Level18_CleaningRobot` procurando um estado sem nenhuma direção aceita: **não existe** nessa fase.
  (A geometria protege; o item 10 cobre o risco de uma fase futura.)
- **Chave duplicada em `crateByPosition`.** Testei `crateByPosition.Add` com detecção de colisão em toda a
  BFS das 27 fases: nunca colide. `IsSlideTargetFree` só libera célula vazia ou a `vacated`, e o `Remove`
  precede o `Add`.
- **Rollback do empurrão quando o robô bloqueia o pouso do jogador** (`PuzzleBoardModel.cs:197-203`):
  correto — desfaz a carga, não incrementa `MoveCount`/`CommandCount` e não toca no toggle.
- **`ShiftCredits.Evaluate` e `ShiftReportPresenter`.** O teto de 250 e a linha "TETO DA FASE" fecham:
  `VisibleTotal(BuildLines(s))` == `CappedTotal(s)` == `Evaluate(s)` para todas as combinações de bônus.
- **`RewindMove` desfazendo 3 movimentos.** O laço `for (int i = 0; i < 3 && runtime.Undo(); i++)`
  (`PuzzleToolService.cs:178`) desfaz no máximo 3 por curto-circuito, e o estoque só é cobrado depois de
  `Execute` confirmar efeito (`:157-164`). Correto.
- **Assinaturas de evento em `PuzzleHudController`, `PuzzleToolBarController`, `PuzzleTimedGateView`,
  `PuzzlePatrolView`, `PuzzleFogOfWar`, `NarrativeOverlayController`, `NarrativeDirector`.** Todos
  desassinam simetricamente em `OnDisable`/`OnDestroy` e cancelam os `MotionHandle` guardados. Não achei
  vazamento nem callback em objeto destruído. `UIMotion` revalida o alvo a cada frame e os encadeamentos
  que sobrevivem à cena checam `this == null` (`NarrativeOverlayController.cs:264`,
  `NarrativeDirector.cs:179`).
- **`UIMotion` fora do Play Mode.** `Play` está protegido (`:463`); o único buraco é o `Sequence.Play()`,
  reportado como item 13.
- **Escrita atômica do save.** `JsonSaveService.Save` (`:56-66`) escreve em `.tmp`, copia o atual para
  `.backup` e só então move. A janela entre `Delete` e `Move` é coberta pelo backup no `Load` (`:30`).
  Checksum SHA-256 correto e verificado antes de desserializar.
- **`ShiftCredits` com valores negativos.** `Data.credits = Mathf.Max(0, Data.credits + earned)`
  (`SaveManager.cs:116`) e `credits = Math.Max(0, credits)` em `EnsureDefaults` (`SaveGameData.cs:140`)
  impedem saldo negativo. `AddToolCount` também clampa (`:127`). `TryPurchaseTool` valida saldo antes de
  debitar (`SaveManager.cs:131`). Sem furo.
- **`SaveMigrationV2ToV3` com dados parciais.** Trata `null` em `data`, nas três listas e em cada
  `LevelProgressRecord` (`:20-30`). Correto.
- **Falso alarme do detector de deadlock.** Rodei o detector ao longo das 27 soluções ótimas: **nenhum
  falso positivo**. O problema é o oposto — falso negativo, item 4.
- **`fakeWalls` no solver.** `parse_layout` ignora a chave `fakeWalls`, mas nas 27 fases as coordenadas
  listadas já caem em piso livre nas `rows`, então motor e solver enxergam o mesmo tabuleiro. Coberto
  preventivamente pelo item 10.
- **Perímetro e linhas irregulares nos layouts.** Todas as 27 fases têm linhas do mesmo comprimento e
  perímetro fechado de paredes. Nenhuma fuga do tabuleiro por linha curta.
