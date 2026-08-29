# QUALIDADE — The Warehouse Nº 08

> Auditoria da rede de segurança: testes, cobertura, validação e processo.
> Data: 2026-08-29. Base: 129 testes EditMode + 3 PlayMode passando
> (`TestResults-EditMode-v6.xml`, `TestResults-PlayMode-v6.xml`).
>
> Nada neste relatório foi executado dentro do Unity — o editor está travado por
> outros agentes. Tudo que afirmo sobre o motor foi verificado por leitura do C#
> e por reimplementação independente em Python, executada contra os `.asset`
> realmente enviados. Onde não pude provar, digo que não pude.

---

## 0. Resumo — os sete achados, em ordem de gravidade

| # | Achado | Onde | Gravidade |
|---|--------|------|-----------|
| 1 | **`PuzzleRuntime.Redo()` está quebrado em toda fase com gelo ou esteira** (8 de 36 fases). Ele recalcula a direção como `PlayerTo - PlayerFrom`, que após um deslize não é um vetor unitário; `TryMove` recusa de imediato. | `PuzzleRuntime.cs:187` | **Bug ao vivo** |
| 2 | **A medalha de corrida gravada não é a medalha mostrada ao jogador.** Os dois pontos de persistência são chamados sem o dano de carga e caem no `cargoDamage = 0f` do parâmetro opcional. Um turno que a HUD chama de PRATA é salvo como PLATINA. | `RaceSessionController.cs:149` e `:154` | **Bug ao vivo** |
| 3 | **A paridade entre o solver Python e o motor C# é confiança pura.** Nenhum teste, nenhum script, nenhuma verificação automática. É o achado de processo mais importante. | `Tools/puzzle/tw08_solver.py` | **Processo** |
| 4 | **O caminho `--assets` do solver ignora silenciosamente cinco mecânicas** (gelo, esteira, robô, botão, portão). Rodar o gate desse jeito devolve `solvable: true` e `replayOk: true` provando um jogo que não é o jogo. | `tw08_solver.py:105` | **Armadilha** |
| 5 | **Sensores e portas — a mecânica mais usada da campanha (19 de 36 fases) — têm ZERO teste.** A regra de transição atômica de `ApplySwitchGroups` nunca foi exercitada. | `PuzzleRuntime.cs:223-290` | **Cobertura** |
| 6 | **`PuzzleLevelValidator` não valida nenhuma das cinco mecânicas novas.** Esteira apontando para parede, rota de robô que teleporta, portão fechado em cima do jogador: tudo passa. | `PuzzleLevelValidator.cs` | **Cobertura** |
| 7 | **Nenhum teste carrega uma cena.** São 48 cenas `.unity` geradas por ~5.000 linhas de pipeline de editor sem nenhuma verificação. `BootstrapSmokeTests` não faz smoke de bootstrap: testa uma máquina de estados em memória. | `Tests/PlayMode/` | **Cobertura** |

E o que está bom está na seção 7 — é bastante coisa.

---

## 1. Mapa de cobertura honesto

Contagem mecânica de tipos públicos citados por algum teste (não é cobertura de
linha; é "existe algum teste que sequer menciona este tipo"):

| Área | Tipos citados / total | Leitura |
|------|----------------------|---------|
| Puzzle | 11 / 26 | Núcleo bem coberto, periferia nova descoberta |
| Save | 7 / 13 | Migrações cobertas, I/O não |
| Narrative | 10 / 14 | Lógica pura coberta, apresentação não |
| UI | 10 / 37 | Formatação e regras puras cobertas, controllers não |
| Economy | 4 / 9 | `ShiftCredits`/`PuzzleAdvisor` cobertos, `PuzzleToolService` não |
| Forklift (corrida) | 5 / 32 | **Confirmado: a área mais descoberta** |
| Core | 3 / 15 | |
| Presentation | 2 / 16 | |
| Motion | 0 / 7 | |
| Levels | 0 / 6 | |
| Player | 0 / 1 | |
| Audio | 0 / 9 | |
| Editor | 0 / 38 | **Estruturalmente impossível de testar** (ver 1.6) |
| **Total** | **52 / 223** | |

### 1.1 Regras do tabuleiro (`PuzzleBoardModel`)

| Mecânica | Fases que usam | Estado |
|---|---|---|
| Empurrão básico, parede, undo | 36 | **COBERTO** — `PuzzleBoardModelTests` |
| Gelo (deslize) | 5 | **COBERTO** — `SlideMechanicsTests`, 6 casos, inclusive "o jogador nunca atravessa a carga que empurrou" |
| Esteira | 3 | **PARCIAL** — coberta só pelo construtor cru `Dictionary<GridCoordinate,GridCoordinate>`. Nenhum teste passa por `PuzzleConveyorDefinition.Step`, que é o que o dado real usa (ver 4.2) |
| Robô de patrulha | 1 | **COBERTO** — `PatrolMechanicsTests`, 7 casos, inclusive passo negativo no undo |
| Botão de direção | 1 | **DESCOBERTO** — `directionButtons` não aparece em nenhum teste. `ApplyDirectionButton` nunca é executado |
| Portão temporizado | 1 | **DESCOBERTO** — `PuzzleTimedBlockDefinition` não é citado em nenhum teste. `IsClosedAt` nunca é executado |
| Sensores e portas | **19** | **DESCOBERTO** — `PuzzleSwitchGroupDefinition` não é citado em nenhum teste. `ApplySwitchGroups` nunca é executado |
| Carga tipada (pesada/frágil) | 12 | **COBERTO** — `PuzzleAdvancedMechanicsTests`, doca aceita e recusa |
| Piso custoso | 17 | **COBERTO** — custo 2 e devolução no undo |
| Porta dinâmica crua (`SetDynamicBlocked`) | — | **PARCIAL** — o caso feliz existe; os três `return false` do método (célula ocupada, fora do tabuleiro, em cima de parede) não |
| Undo | 36 | **COBERTO** para movimento, empurrão, custo, deslize e relógio do robô. **Descoberto** para botão e portão |
| Redo | 36 | **PARCIAL e com bug** — só existe teste em corredor liso; ver achado 1 |
| Névoa | 3 | **DESCOBERTO** — `PuzzleFogOfWar` (206 linhas) não é citado. É apresentação pura, risco baixo, mas nada garante que continue sendo |
| Parede falsa | 2 | **DESCOBERTO** — `PuzzleFakeWallView` não é citado. Também apresentação pura |

O ponto mais desconfortável desta tabela: **a mecânica mais usada da campanha
(sensor/porta, 19 fases) é a única grande sem nenhum teste.** Não é acidente —
é consequência direta de um defeito de testabilidade descrito em 5.3.

### 1.2 Economia (`Scripts/Economy/`)

- **COBERTO**: `ShiftCredits.Evaluate` e `BuildStatement` (7 casos entre
  `ShopEconomyTests` e `HudTests`), `PuzzleRunSummary.IsClean`,
  `PuzzleAdvisor.TryFindCriticalCrate` / `FindOpenGoals` / `BuildHint`,
  estoque de ferramentas em `SaveGameData`.
- **DESCOBERTO**: `PuzzleToolService` (221 linhas), que é quem *executa* as
  ferramentas. Em particular:
  - `CanUse` gatilha em `Level.AllowPowerUps` — o interruptor com que o level
    designer proíbe ferramenta numa fase que ela quebraria. Se isso regredir, o
    jogador usa Rebobinar numa fase desenhada sem ele e, pior, o turno passa a
    contar como assistido e sai do ranking limpo. Nada testa.
  - `usesThisLevel` é indexado por `PuzzleToolKind`, não por `ToolId`: duas
    ferramentas do mesmo tipo com limites diferentes compartilham o contador.
  - `TryUse` cobra o estoque *depois* de confirmar efeito (correto, e é uma
    regra de dinheiro do jogador) — sem teste.
- **DESCOBERTO**: `PuzzleProgressStore.EvaluateMedal`, que é quem decide a
  medalha do puzzle. Nunca devolve 0: qualquer conclusão vale bronze. Isso pode
  ser intencional, mas não está escrito em lugar nenhum nem travado por teste.
  E `SaveManager.CommitPuzzleShift` usa esse valor para pagar créditos.

### 1.3 Save e migrações

- **COBERTO**: `SaveMigrationV1ToV2` e `SaveMigrationV2ToV3` (3 casos),
  `SaveIntegrity.ComputeChecksum`, `SaveGameData.EnsureDefaults`.
- **DESCOBERTO e arriscado**: `JsonSaveService`.
  - `Load()` faz `data.version = currentVersion` incondicionalmente **depois**
    da migração. Um save de versão **maior** que a atual (playtester que voltou
    de um branch beta) é carimbado com a versão velha sem migração e sem aviso:
    corrupção silenciosa do progresso.
  - `SaveMigrationPipeline.MigrateTo` lança `InvalidOperationException` quando
    não há migração registrada, e `Load()` **não captura**. No dia em que
    existir v4 e alguém esquecer de registrar v3→v4, o `Awake` do `SaveManager`
    estoura e o jogo não abre. Nenhum teste passa por esse caminho.
  - O fallback para `.backup` nunca foi exercitado.
- **DESCOBERTO**: `SaveManager` inteiro (203 linhas), inclusive
  `CommitPuzzleShift`, que é onde o dinheiro do jogador é creditado.

### 1.4 Narrativa, HUD, Menus, Motion

- **Narrativa — o melhor conjunto do projeto.** 21 testes cobrindo playback,
  casamento por contexto, precedência de fase sobre setor, fila, `SkipAll`,
  `PlayOnce` atravessando instâncias e sequências sem linhas. Sem ressalvas.
- **HUD — muito bom.** 24 testes de formatação com strings literais de verdade,
  incluindo separador decimal invariante no cronômetro e "nunca mostrar número
  negativo". Descoberto: `PuzzleHudController` (569 linhas) e
  `RaceHudController` (605 linhas) — mas eles são fiação de cena, e a decisão de
  extrair `HudFormat`/`PuzzleHudStatusResolver`/`ShiftReportPresenter` como
  regras puras foi a decisão de testabilidade mais acertada do projeto.
- **Menus — bom.** 15 testes cobrindo rótulo do cartão de fase, tinta por
  estado, enquadramento do scroll, loop dos créditos e as regras da vitrine.
- **Motion — zero.** `Easing` (106 linhas), `UIMotion` (529 linhas),
  `UIEntranceAnimator`. `Easing` é matemática pura e testável em cinco minutos:
  monotonicidade, `f(0)=0`, `f(1)=1`. Hoje nada impede uma curva devolver NaN.
  Risco de jogo: baixo. Custo de cobrir: quase nada. É fruta madura.

### 1.5 Corrida (`Scripts/Forklift/`) — confirmado, é a área mais descoberta

5 de 32 tipos citados, e os 3 testes existentes (`RaceProductionSystemsTests`)
cobrem ranking por checkpoint, leitura de posição de checkpoint e a estabilidade
dos valores serializados do enum de power-up. Fora isso:

- **`RaceProgressStore` — descoberto, e é onde está o bug 2.** É quem grava
  medalha e recorde em `PlayerPrefs`.
- **`SaveManager.RecordRaceCompletion` — descoberto**, mesmo bug.
- **`RaceDefinition.EvaluateMedal` — parcialmente coberto por um teste que não
  cobre o que importa.** `ExpansionProgressionTests.RaceDefinitionAwardsExpectedMedals`
  prova a escala de tempo, mas a fronteira de dano que separa platina de ouro
  (`cargoDamage <= 0f` vs `<= maximumCargoDamageForGold`) não é testada em
  isolado — e é exatamente essa fronteira que o bug 2 atropela.
- **`RaceManager.NotifyCheckpoint` — descoberto no que decide a corrida**:
  a rejeição de checkpoint fora de ordem (`checkpointIndex != racer.NextCheckpointIndex`,
  que é a única defesa contra corte de percurso), o encerramento da corrida
  quando todos terminam, e `GetProgressScore` na volta do último checkpoint para
  o primeiro (`NextCheckpointIndex == 0` vira `checkpoints.Count - 1`) — a linha
  mais fácil de errar do arquivo, porque decide a posição na tela do jogador
  toda vez que alguém cruza a linha de largada.
- **`RaceCargoController` — descoberto.** É quem calcula o dano que decide a
  medalha. `DamagePercent`, o limiar lateral, o multiplicador de derrapagem, o
  `ApplyStabilityProtection` que expira por `Time.time`: nada.
- **`PowerUpExecutor`, `WeightedPowerUpTable`, `PowerUpInventory` — descobertos.**
  A tabela ponderada é sorteio: se a soma dos pesos ou o índice estiver errado,
  um power-up nunca sai e ninguém percebe.
- **`ArcadeForkliftController2D` (262 linhas) — descoberto.** É física; testar
  em EditMode é caro. Aceitável ficar de fora, mas então precisa de checklist de
  playtest, que hoje também não existe.
- **`RaceProgressStore` grava em `PlayerPrefs` e `SaveManager` grava no JSON.**
  Duas fontes de verdade para o mesmo progresso de corrida, escritas na mesma
  linha de código, sem nada garantindo que concordem.

### 1.6 Pipeline de editor — descoberto por construção, não por descuido

`Assets/_Project/Tests/EditMode/TW08.Tests.EditMode.asmdef` referencia apenas
`TW08.Runtime`. **Nenhum teste pode sequer compilar contra `TW08.Editor`.** São
38 tipos e ~9.000 linhas de pipeline (importadores de campanha, construtores de
cena, setups de arte e áudio) fora de alcance.

Isso importa porque é o pipeline que **gera os 36 `.asset` de fase**. Ou seja: a
única coisa que o gate do solver prova está sendo produzida por código que nada
testa. E dentro dele:

- `TW08ProjectValidator.ValidateProject` é o único lugar que chama
  `PuzzleLevelValidator` sobre os assets enviados — e é um `[MenuItem]`. Ele não
  falha build, não falha CI, não falha nada. Só falha se um humano clicar.
- `TW08RuntimeSceneListGuard` e `TW08RaceCampaignIntegrity` são
  `[InitializeOnLoad]` que *consertam* dados sozinhos ao abrir o editor. Reparo
  automático sem teste é a pior combinação possível: quando ele consertar errado,
  o diff aparece como se um humano tivesse feito.

**Correção barata:** adicionar `"TW08.Editor"` às `references` do asmdef de
EditMode. Uma linha, e destrava a testabilidade de 9.000 linhas.

---

## 2. O gate do solver — o achado mais importante

`Tools/puzzle/tw08_solver.py` é a única prova de que a campanha é jogável. As
perguntas eram: o replay é mesmo independente, e a paridade com o C# é
verificada ou é confiança?

### 2.1 O replay é semi-independente — e a metade que falta é a que importa

`replay()` (linha 637) é independente do **buscador**: não usa a heurística, nem
a poda de células mortas, nem a poda de canto, nem a reconstrução do caminho por
`prev`. Se o A* devolvesse uma sequência corrompida, o replay pegaria. Isso é
valor real.

Mas `replay()` **chama as mesmas funções de regra que o `solve()` chamou**:
`slide()`, `door_state()`, `is_complete()`, `closed_timed()`, `patrol_cells()`.
Um erro em `slide()` — por exemplo, esteira invertida aplicando o passo errado —
produz uma solução errada **e** um replay que concorda com ela. Os dois erram
juntos, `replayOk: true`, e o relatório diz que a fase está provada.

Ou seja: o replay prova que a busca não mentiu. **Não prova que as regras estão
certas.** E as regras são justamente o que precisa espelhar o C#.

### 2.2 A paridade com o motor C# é confiança. Confirmado.

Não existe nenhum teste, script, hook ou passo de CI que compare o solver ao
`PuzzleBoardModel`. A única coisa que amarra os dois é o comentário no topo do
`tw08_solver.py` e o comentário em `SlideMechanicsTests.cs` ("estes testes são o
contrato que o solver Python espelha"). São dois documentos apontando um para o
outro, sem execução no meio.

Fui atrás de saber se a confiança está justificada hoje. Reimplementei as regras
em Python **lendo o C#, não o solver** (`PuzzleBoardModel.TryMove`, `Slide`,
`IsBlocked`, `IsSlideTargetFree`, `EvaluateCompletion` e
`PuzzleRuntime.ApplySwitchGroups`), e reproduzi as 36 soluções provadas contra
os `.asset` realmente enviados:

```
OK=36  FALHA=0  SEM_PROVA=0
```

Todas as 36 completam, e o custo bate exatamente com o `optimalCost` do
relatório em todas elas. **A paridade está correta hoje.** O problema não é o
estado; é que nada segura esse estado. A próxima mudança de regra no C# quebra a
prova em silêncio.

### 2.3 A armadilha do `--assets`

`parse_unity_asset()` lê `walls`, `goals`, `costlyCells`, `crates`,
`goalRequirements` e `switchGroups`. **Não lê `iceCells`, `conveyors`,
`patrols`, `directionButtons` nem `timedBlocks`** — os cinco mecanismos novos.
Não avisa; devolve um `Level` com esses campos vazios.

Rodei o gate pelos dois caminhos nas mesmas fases:

| Fase | `--layouts` (usado no relatório) | `--assets` (silenciosamente cego) |
|---|---|---|
| Level11_IcedFloor | 10 | **16** (ignorou 10 células de gelo) |
| Level16_ConveyorOnline | 7 | **13** (ignorou 4 esteiras) |
| Level17_AutomaticRoute | 20 | **18** (ignorou esteiras e o botão) |
| Level18_CleaningRobot | 36 | **34** (ignorou o robô) |
| Secret08_LeftoverMap | 41 | **39** (ignorou o portão) |

Nos dois casos o relatório diz `solvable: true` e `replayOk: true`. O replay
concorda com a busca porque os dois leem o mesmo `Level` vazio. Quem rodar
`--assets` amanhã vai receber uma prova bem-formatada de um jogo que não existe,
e limites de medalha errados junto.

Isso já é perigoso hoje, e vira sério na hora em que alguém decidir "vamos rodar
o gate direto nos assets, que é o dado de verdade" — que é a decisão *certa* de
processo, tomada sobre um parser que não a suporta.

### 2.4 A prova é de um documento, não do dado enviado

`report-new.json` foi produzido a partir de `Docs/level-layouts.json`. O jogo
carrega `Assets/_Project/ScriptableObjects/Campaign/*.asset`, gerados
separadamente pelo importador C#. **Nada verifica que os dois concordam.**

Comparei os dois campo a campo (paredes, alvos, gelo, esteiras com direção,
custosas, botões, portões com prazo, cargas com tipo, requisitos de doca, grupos
de sensor/porta, paredes falsas, névoa, início do jogador, dimensões e limites
de medalha):

```
assets comparados: 27   com divergência: 0
```

**Concordam hoje.** De novo: o estado está bom, a garantia não existe.

### 2.5 Os limites de platina são o ótimo exato — e isso tem consequência

Em todas as 36 fases, `platinumMoveLimit == optimalCost` do solver e
`goldMoveLimit == ceil(optimalCost * 1,3)`. Platina exige a partida perfeita,
sem uma casa de folga.

Isso levanta uma dúvida legítima: a heurística Manhattan do A* é admissível
enquanto cada comando move uma carga no máximo uma casa, mas **com gelo ou
esteira uma carga percorre várias casas por comando** e a heurística pode
superestimar — o que tornaria o `optimalCost` reportado não-ótimo e a platina
mais frouxa do que se pretende. Rodei Dijkstra puro (h=0, ótimo garantido) nas
8 fases com deslize:

| Fase | A* | Dijkstra |
|---|---|---|
| Level11, Level12, Level14, Level15, Level16, Level17, Level20 | igual | **confirmado ótimo** |
| Secret10_DudasPath | 46 | inconclusivo (estourou 4M estados) |

Sete das oito estão provadas ótimas. **Secret10 é a única fase do jogo cujo
limite de platina não está provado.** Não é um bug — a platina continua
alcançável, porque o custo reportado corresponde a uma solução que eu reproduzi.
É só a única com asterisco, e merece registro.

### 2.6 Como automatizar a paridade (proposta concreta)

Três peças, em ordem de valor:

1. **Teste de paridade em EditMode** (código pronto em 5.1). O solver publica
   `Tools/puzzle/proven-solutions.json` com `id → solution + optimalCost`; o
   teste carrega cada `PuzzleLevelDefinition` enviado, reproduz a sequência pelo
   `PuzzleRuntime` real e exige `IsComplete` e `MoveCount == optimalCost`. Isso
   fecha os buracos 2.1, 2.2 e 2.4 de uma vez: se o C# mudar, o teste vermelha;
   se o asset divergir do layout, o teste vermelha; se `slide()` estiver errado
   no Python, o custo não bate. **Falha se alguma fase enviada não tiver solução
   provada** — é assim que o gate passa a cobrir fases novas sem ninguém lembrar.
2. **Consertar `parse_unity_asset`** para ler as cinco mecânicas, e então
   **apontar o gate para os `.asset`** em vez do JSON. O layout JSON vira fonte
   de autoria; o asset vira fonte de verdade. Enquanto o parser não for
   consertado, ele deve **falhar alto** ao encontrar `iceCells:`/`conveyors:`/
   `patrols:`/`directionButtons:`/`timedBlocks:` não vazios, em vez de ignorar.
   São seis linhas e eliminam a armadilha 2.3 imediatamente.
3. **Separar as regras do buscador no Python.** Mover `slide`, `door_state`,
   `is_complete` e o passo de movimento para `tw08_rules.py`, e escrever o
   `replay` a partir do C# de novo, sem importar `tw08_rules`. Aí o replay vira
   independente de verdade. É a peça mais cara e a menos urgente das três,
   porque o item 1 já cobre a maior parte do risco.

---

## 3. Interações não testadas — o que existe de verdade nas 36 fases

Contagem sobre os `.asset` enviados (não sobre o documento de design):

| Mecânica | Fases |
|---|---|
| Sensor/porta | **19** — L04, L06, L09, L14, L15, L19, L20, L24, L25, L26, L28, L29, L30, S03, S04, S07, S08, S09, S10 |
| Piso custoso | 17 |
| Doca tipada | 12 |
| Gelo | 5 — L11, L12, L14, L15, S10 |
| Esteira | 3 — L16, L17, L20 |
| Névoa | 3 — L26, L27, S05 |
| Parede falsa | 2 — L26, S01 |
| Robô | **1** — L18 |
| Botão de direção | **1** — L17 |
| Portão temporizado | **1** — S08 |

### 3.1 Combinações que EXISTEM na campanha e não têm teste

| Combinação | Fases | Por que dói |
|---|---|---|
| **Gelo × sensor/porta** | L14, L15, S10 | A carga desliza e a porta abre/fecha *depois* do deslize. `Slide` avalia `IsBlocked` com o estado de porta anterior ao comando; `ApplySwitchGroups` roda depois. Uma carga pode deslizar por cima de uma porta que fecharia atrás dela. Ninguém verifica que esse encadeamento é o pretendido — e é exatamente onde uma fase vira insolúvel sem aviso |
| **Esteira × sensor/porta** | L20 | Mesmo encadeamento, com o agravante de a esteira poder levar a carga para *fora* do sensor no comando seguinte |
| **Esteira × botão de direção** | L17 | A única fase do jogo com botão. Se o botão inverter no momento errado, L17 fica insolúvel e nada avisa |
| **Portão temporizado × sensor/porta** | S08 | Duas fontes de bloqueio na mesma célula-vizinhança, uma dirigida por `CommandCount` e outra por posição de carga. `IsBlocked` soma as duas; o undo rebobina só uma delas |
| **Névoa × sensor × custo × parede falsa** | L26 | Quatro mecânicas empilhadas na fase mais complexa do jogo, e as duas visuais (névoa, parede falsa) não têm nenhum teste que garanta que continuam sendo *só* visuais |
| **Gelo × doca tipada** | S10 | A carga certa precisa parar na doca certa depois de um deslize. É o único lugar onde "onde a carga para" e "que carga é" se cruzam |

### 3.2 Combinações que NÃO existem em fase nenhuma — e por isso são armadilha

O motor suporta, ninguém usa, ninguém testa. O próximo designer que tentar cai
em terreno não provado:

- **Gelo × esteira.** `Slide` trata a transição (o gelo mantém a direção de
  entrada, a esteira impõe a sua, e `direction = step` no fim do laço propaga a
  direção da esteira para o gelo seguinte). É código deliberado, sofisticado, e
  sem uma única execução em teste ou em fase.
- **Robô × deslize.** `Slide` recebe `robots` e para antes deles em três pontos
  distintos. `SlideMechanicsTests` não tem robô; `PatrolMechanicsTests` não tem
  gelo. O parâmetro existe e nunca foi exercitado.
- **Robô × sensor/porta.** L18 é a única fase com robô e não tem porta.
- **Botão × qualquer coisa que não seja esteira** (robô, portão, gelo).
- **Portão temporizado × deslize.** A carga desliza para dentro do prazo?

Custo de cobrir: um teste cada, todos de dez linhas, todos no estilo de
`SlideMechanicsTests`. Valor: transformar "achamos que funciona" em "funciona".

---

## 4. Qualidade dos testes existentes

O conjunto é acima da média. As ressalvas abaixo são cirúrgicas, não um recado
geral.

### 4.1 Teste que passa sem provar nada

**`ShopEconomyTests.PerfectCleanRun_IsCappedAtLevelMaximum`**

```csharp
Assert.AreEqual(ShiftCredits.MaxPerLevel, ShiftCredits.Evaluate(summary));
```

Compara a constante do código com a saída do código. Se alguém trocar
`MaxPerLevel` de 250 para 1, o teste continua verde e a loja quebra. O
comentário do teste diz o valor certo ("somando tudo daria 425; o teto por fase
segura em 250") mas o `Assert` não o afirma. Correção de uma linha:

```csharp
Assert.AreEqual(425, ShiftCredits.BuildStatement(summary).Sum(e => e.Amount));
Assert.AreEqual(250, ShiftCredits.Evaluate(summary));
```

O mesmo padrão auto-referencial aparece em `BareCompletion_PaysOnlyCompletionAndBronze`
e em `UsingATool_RemovesTheCleanBonusAndClearsCompetitiveFlag`, mas nesses dois
o valor da asserção está na *estrutura* (só estas duas linhas apareceram; a
diferença é exatamente um bônus), e isso é legítimo.

### 4.2 Teste que testa o mock em vez do código

**`SlideMechanicsTests` inteiro, no que diz respeito a esteira.**

Todos os testes de esteira montam o tabuleiro pelo construtor cru:

```csharp
conveyors: new Dictionary<GridCoordinate, GridCoordinate> {
    [new GridCoordinate(3, 1)] = GridCoordinate.Right
}
```

O dado real do jogo não é isso. É `PuzzleConveyorDefinition` com um
`ConveyorDirection`, convertido por:

```csharp
public GridCoordinate Step => direction switch {
    ConveyorDirection.Up => GridCoordinate.Up,
    ConveyorDirection.Down => GridCoordinate.Down,
    ConveyorDirection.Left => GridCoordinate.Left,
    _ => GridCoordinate.Right   // <- Right é o default do switch
};
```

**Nenhum teste executa esse `switch`.** Se `Up` mapeasse para `Down`, os 129
testes continuariam verdes e as três fases com esteira quebrariam. O `_ =>`
como default agrava: um valor de enum novo cairia silenciosamente em `Right`.

O mesmo vale, em menor grau, para gelo, robô, botão e portão: os testes
constroem `PuzzleBoardModel` direto, e o construtor
`PuzzleBoardModel(PuzzleLevelDefinition)` — que faz o `ToDictionary` de tudo e é
o único usado em produção — só é exercitado por níveis de teste simples, sem
nenhuma das cinco mecânicas novas. **A fiação entre o dado e o modelo não tem
teste.** É o teste 5.5.

### 4.3 Testes que quebrariam pelo motivo errado

1. **`ShopEconomyTests.Advisor_HintsGetMoreDirectWithEachTier`**
   ```csharp
   StringAssert.Contains("passo", tier3.ToLowerInvariant());
   ```
   Afirma sobre o texto de UI. Uma revisão de copy trocando "Primeiro passo
   sugerido" por "Sugestão de movimento" vermelha o teste sem nenhuma regressão
   de lógica. E o ramo alternativo do tier 3 (`"Abra caminho até a carga
   destacada: não há rota livre agora."`, que não contém "passo") nunca é
   exercitado — então o teste é ao mesmo tempo frágil e incompleto.

2. **`PuzzleEntityViewPlayModeTests.AnimatedMoveReachesLogicalTarget`**
   ```csharp
   view.MoveTo(new GridCoordinate(1, 0), 1f, true);
   yield return new WaitForSeconds(0.15f);
   ```
   `moveDuration` é `0.10f`. A margem é de 50 ms. Um ajuste de *feel* para 0,2 s
   — mudança legítima e provável — quebra o teste. O certo é ler `moveDuration`
   ou esperar a corrotina terminar, não um número mágico.

3. **`HudTests` de formatação** afirmam strings literais em português
   (`"ROTA LIBERADA // MEDALHA 2 // LIMPO"`). Isso é intencional e correto
   enquanto o jogo for só em pt-BR. **No dia da localização, 15 testes viram
   dívida de uma vez.** Vale registrar agora, não descobrir depois.

### 4.4 Testes fora do namespace combinado

`PuzzleBoardModelTests` e `PuzzleRuntimeTests` estão em `namespace TW08.Tests`;
todo o resto está em `TW08.Tests.EditMode`. Cosmético, mas confunde filtro por
namespace em linha de comando.

### 4.5 `BootstrapSmokeTests` não faz smoke de bootstrap

O nome promete verificar que o jogo sobe. O teste instancia um
`GameStateMachine` em memória e verifica uma transição. Não carrega cena, não
toca `GameBootstrap`, não toca `SceneLoader`. **São 48 cenas `.unity` e nenhuma
é carregada por teste nenhum.** O primeiro sintoma de uma cena quebrada vai ser
um humano abrindo o jogo.

---

## 5. Os testes que faltam, em ordem de valor

| # | Teste | Por que, concretamente |
|---|---|---|
| 1 | **Paridade solver ↔ motor** | É a única prova de que a campanha é jogável, e hoje ela é um PDF. Fecha os buracos 2.1, 2.2 e 2.4 juntos |
| 2 | **Redo depois de deslize** | Bug ao vivo em 8 de 36 fases. O botão de refazer não faz nada em L11, L12, L14, L15, L16, L17, L20 e S10, e o contador da HUD nunca baixa |
| 3 | **Sensor e porta (grupo de comutação)** | 19 de 36 fases. A regra de transição atômica ("se um painel está ocupado, o grupo inteiro fica aberto") é a mais sutil do motor e nunca foi executada por teste |
| 4 | **Botão de direção e portão temporizado** | Duas mecânicas com zero execução. O botão dispara *na célula onde o comando terminou* — deslizar por cima dele não pode inverter nada. Se isso inverter, L17 fica insolúvel |
| 5 | **Medalha de corrida persistida** | Bug ao vivo: a medalha salva ≠ a medalha mostrada. Afeta recompensa do jogador |
| 6 | **Fiação `PuzzleLevelDefinition` → `PuzzleBoardModel`** | Nenhum teste passa pelo `switch` de `ConveyorDirection`; um mapeamento invertido passaria pelos 129 testes |
| 7 | **`JsonSaveService.Load` com save de versão futura e sem migração registrada** | Corrupção silenciosa de progresso e falha de boot, respectivamente |
| 8 | **`RaceManager.NotifyCheckpoint` fora de ordem e volta de pista** | Única defesa contra corte de percurso; `GetProgressScore` na virada de volta decide a posição na tela |
| 9 | **`PuzzleToolService.CanUse` com `AllowPowerUps = false`** | Interruptor do level designer; se regredir, contamina o ranking limpo |
| 10 | **`PuzzleLevelValidator` para as cinco mecânicas novas** | Ver 6.3 — o validador não olha nada disso hoje |
| 11 | **Combinações não usadas** (gelo × esteira, robô × deslize) | Código deliberado e sofisticado, zero execução. Dez linhas cada |
| 12 | **`Easing`** | Matemática pura, cinco minutos, protege contra NaN em curva de animação |
| 13 | **Smoke de carregamento de cena (PlayMode)** | 48 cenas, nenhuma verificada |

Abaixo, os cinco primeiros, prontos. Todos em `namespace TW08.Tests.EditMode`,
no estilo das suítes existentes.

> **Nota de honestidade:** não pude executar nenhum destes no Unity. Os valores
> esperados de 5.2, 5.3 e 5.4 foram todos verificados na reimplementação Python
> das regras do C#, a mesma que reproduz as 36 fases enviadas com 36/36. O 5.5
> é o único que **falha hoje de propósito** — ele é o bug.

### 5.1 `SolverParityTests` — a prova vira teste

Pré-requisito: o solver passa a publicar `Tools/puzzle/proven-solutions.json`:

```bash
python Tools/puzzle/tw08_solver.py \
  --layouts Docs/level-layouts.json \
  --assets  Assets/_Project/ScriptableObjects/VerticalSlice \
  --out     Tools/puzzle/proven-solutions.json
```

```csharp
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEditor;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// O gate do solver deixa de ser confiança.
    ///
    /// Para cada fase enviada, reproduz no motor de verdade a solução que o
    /// solver Python provou e exige que ela conclua a fase pelo custo exato.
    /// Se a semântica do C# mudar, se o .asset divergir do layout que foi
    /// provado, ou se o solver espelhar a regra errada, este teste vermelha.
    ///
    /// A ausência de solução provada também é falha: fase nova sem prova não
    /// entra no jogo.
    /// </summary>
    public sealed class SolverParityTests
    {
        private const string ReportPath = "Tools/puzzle/proven-solutions.json";

        [System.Serializable]
        private sealed class ProvenLevel
        {
            public string id;
            public bool solvable;
            public string solution;
            public int optimalCost;
        }

        [System.Serializable]
        private sealed class ProvenReport
        {
            public ProvenLevel[] levels;
        }

        private static Dictionary<string, ProvenLevel> LoadProofs()
        {
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ReportPath));
            Assert.IsTrue(
                File.Exists(path),
                $"Prova do solver ausente em '{ReportPath}'. Rode o solver antes de commitar fase.");

            ProvenReport report = JsonUtility.FromJson<ProvenReport>(File.ReadAllText(path));
            Assert.IsNotNull(report?.levels, "Relatório do solver ilegível.");

            return report.levels
                .Where(entry => !string.IsNullOrWhiteSpace(entry.id))
                .ToDictionary(entry => entry.id, entry => entry);
        }

        private static IEnumerable<PuzzleLevelDefinition> ShippedLevels()
        {
            return AssetDatabase.FindAssets("t:PuzzleLevelDefinition")
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<PuzzleLevelDefinition>)
                .Where(level => level != null);
        }

        private static GridCoordinate ToDirection(char command)
        {
            switch (command)
            {
                case 'U': return GridCoordinate.Up;
                case 'D': return GridCoordinate.Down;
                case 'L': return GridCoordinate.Left;
                case 'R': return GridCoordinate.Right;
                default: throw new AssertionException($"Comando desconhecido '{command}'.");
            }
        }

        [Test]
        public void EveryShippedLevelHasAProvenSolution()
        {
            Dictionary<string, ProvenLevel> proofs = LoadProofs();
            List<string> missing = ShippedLevels()
                .Select(level => level.LevelId)
                .Where(id => !proofs.TryGetValue(id, out ProvenLevel proof)
                             || !proof.solvable
                             || string.IsNullOrEmpty(proof.solution))
                .ToList();

            CollectionAssert.IsEmpty(
                missing,
                "Fases enviadas sem prova de solubilidade: " + string.Join(", ", missing));
        }

        [Test]
        public void TheEngineReplaysEveryProvenSolutionForTheExactCost()
        {
            Dictionary<string, ProvenLevel> proofs = LoadProofs();
            List<string> failures = new();

            foreach (PuzzleLevelDefinition level in ShippedLevels())
            {
                if (!proofs.TryGetValue(level.LevelId, out ProvenLevel proof)
                    || !proof.solvable
                    || string.IsNullOrEmpty(proof.solution))
                {
                    continue; // coberto pelo teste acima
                }

                GameObject host = new("Parity " + level.LevelId);
                try
                {
                    PuzzleRuntime runtime = host.AddComponent<PuzzleRuntime>();
                    runtime.Configure(level, null, System.Array.Empty<PuzzleEntityView>());
                    runtime.Initialize();

                    if (runtime.Board == null)
                    {
                        failures.Add($"{level.LevelId}: PuzzleLevelValidator recusou a fase.");
                        continue;
                    }

                    bool broke = false;
                    for (int i = 0; i < proof.solution.Length; i++)
                    {
                        if (!runtime.TryMove(ToDirection(proof.solution[i])))
                        {
                            failures.Add(
                                $"{level.LevelId}: o motor recusou o comando {i + 1} " +
                                $"('{proof.solution[i]}') que o solver provou válido.");
                            broke = true;
                            break;
                        }
                    }

                    if (broke) continue;

                    if (!runtime.Board.IsComplete)
                    {
                        failures.Add($"{level.LevelId}: a solução provada não conclui a fase no motor.");
                        continue;
                    }

                    if (runtime.Board.MoveCount != proof.optimalCost)
                    {
                        failures.Add(
                            $"{level.LevelId}: custo divergente — motor={runtime.Board.MoveCount}, " +
                            $"solver={proof.optimalCost}. As duas semânticas saíram de sincronia.");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(host);
                }
            }

            CollectionAssert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void PlatinumIsAlwaysReachableByTheProvenSolution()
        {
            // O limite de platina é o custo ótimo. Se um ajuste de dificuldade
            // baixar o limite abaixo do que o solver conseguiu, a medalha vira
            // inalcançável e ninguém descobre até um jogador reclamar.
            Dictionary<string, ProvenLevel> proofs = LoadProofs();
            List<string> impossible = new();

            foreach (PuzzleLevelDefinition level in ShippedLevels())
            {
                if (!proofs.TryGetValue(level.LevelId, out ProvenLevel proof) || !proof.solvable)
                {
                    continue;
                }

                if (level.PlatinumMoveLimit > 0 && proof.optimalCost > level.PlatinumMoveLimit)
                {
                    impossible.Add(
                        $"{level.LevelId}: platina exige <= {level.PlatinumMoveLimit}, " +
                        $"mas o melhor conhecido custa {proof.optimalCost}.");
                }

                if (level.GoldMoveLimit > 0 && proof.optimalCost > level.GoldMoveLimit)
                {
                    impossible.Add(
                        $"{level.LevelId}: ouro exige <= {level.GoldMoveLimit}, " +
                        $"mas o melhor conhecido custa {proof.optimalCost}.");
                }
            }

            CollectionAssert.IsEmpty(impossible, string.Join("\n", impossible));
        }
    }
}
```

### 5.2 `SlideRedoTests` — o bug do refazer

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Refazer depois de um deslize.
    ///
    /// PuzzleRuntime.Redo reconstrói a direção do comando como
    /// PlayerTo - PlayerFrom. Num passo comum isso é um vetor unitário e
    /// funciona. Depois de um deslize em gelo ou esteira, o jogador andou várias
    /// células num comando só: a subtração devolve (4,0), TryMove recusa por
    /// ManhattanLength != 1 e o refazer não acontece.
    ///
    /// São 8 das 36 fases enviadas (L11, L12, L14, L15, L16, L17, L20, S10).
    /// O teste de refazer existente roda em corredor liso e por isso passa.
    /// </summary>
    public sealed class SlideRedoTests
    {
        private GameObject host;
        private PuzzleLevelDefinition level;

        [TearDown]
        public void TearDown()
        {
            if (host != null) UnityEngine.Object.DestroyImmediate(host);
            if (level != null) UnityEngine.Object.DestroyImmediate(level);
        }

        [Test]
        public void RedoRestoresThePlayerAfterAnIceSlide()
        {
            // Corredor 8x3. Gelo em 2..4: o comando leva o jogador de (1,1) a (5,1).
            PuzzleRuntime runtime = BuildRuntime(
                ice: new List<GridCoordinate> { new(2, 1), new(3, 1), new(4, 1) });

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.AreEqual(new GridCoordinate(5, 1), runtime.Board.PlayerPosition,
                "O deslize precisa levar o operador até o fim do gelo.");

            Assert.IsTrue(runtime.Undo());
            Assert.AreEqual(new GridCoordinate(1, 1), runtime.Board.PlayerPosition);
            Assert.AreEqual(1, runtime.RedoCount);

            Assert.IsTrue(runtime.Redo(), "Refazer precisa reexecutar o comando, não recusá-lo.");
            Assert.AreEqual(new GridCoordinate(5, 1), runtime.Board.PlayerPosition);
            Assert.AreEqual(0, runtime.RedoCount, "O contador de refazer da HUD tem que baixar.");
        }

        [Test]
        public void RedoRestoresThePlayerAfterAConveyorCarriesHimBack()
        {
            // Esteira em (3,1) apontando para a esquerda: quem entra indo à
            // direita volta para (2,1). O comando termina onde começou, e a
            // subtração PlayerTo - PlayerFrom vira (0,0).
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(2, 1),
                conveyors: new List<PuzzleConveyorDefinition>
                {
                    new(new GridCoordinate(3, 1), ConveyorDirection.Left)
                });

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.AreEqual(new GridCoordinate(2, 1), runtime.Board.PlayerPosition);
            Assert.AreEqual(1, runtime.Board.CommandCount);

            Assert.IsTrue(runtime.Undo());
            Assert.AreEqual(0, runtime.Board.CommandCount);

            Assert.IsTrue(runtime.Redo(), "Um comando que termina onde começou continua sendo um comando.");
            Assert.AreEqual(1, runtime.Board.CommandCount);
        }

        [Test]
        public void RedoStillWorksOnPlainFloor()
        {
            // Regressão do caminho que já funciona, para o conserto não quebrá-lo.
            PuzzleRuntime runtime = BuildRuntime();

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.IsTrue(runtime.Undo());
            Assert.IsTrue(runtime.Redo());
            Assert.AreEqual(new GridCoordinate(2, 1), runtime.Board.PlayerPosition);
        }

        private PuzzleRuntime BuildRuntime(
            GridCoordinate? player = null,
            List<GridCoordinate> ice = null,
            List<PuzzleConveyorDefinition> conveyors = null)
        {
            const int width = 8;
            List<GridCoordinate> walls = new();
            for (int x = 0; x < width; x++)
            {
                walls.Add(new GridCoordinate(x, 0));
                walls.Add(new GridCoordinate(x, 2));
            }

            walls.Add(new GridCoordinate(0, 1));
            walls.Add(new GridCoordinate(width - 1, 1));

            level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
            SetField(level, "levelId", "test-slide-redo");
            SetField(level, "displayName", "Slide Redo");
            SetField(level, "width", width);
            SetField(level, "height", 3);
            SetField(level, "cellSize", 1f);
            SetField(level, "playerStart", player ?? new GridCoordinate(1, 1));
            SetField(level, "walls", walls);
            SetField(level, "goals", new List<GridCoordinate> { new(6, 1) });
            SetField(level, "crates", new List<PuzzleCrateDefinition>
            {
                new("crate-a", PuzzleEntityKind.Crate, new GridCoordinate(6, 1))
            });
            SetField(level, "iceCells", ice ?? new List<GridCoordinate>());
            SetField(level, "conveyors", conveyors ?? new List<PuzzleConveyorDefinition>());

            host = new GameObject("Slide Redo Runtime");
            PuzzleRuntime runtime = host.AddComponent<PuzzleRuntime>();
            runtime.Configure(level, null, Array.Empty<PuzzleEntityView>());
            runtime.Initialize();
            Assert.IsNotNull(runtime.Board, "O nível de teste precisa passar no validador.");
            return runtime;
        }

        private static void SetField<T>(PuzzleLevelDefinition target, string fieldName, T value)
        {
            FieldInfo field = typeof(PuzzleLevelDefinition)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
```

> **Conserto sugerido** (uma linha, em `PuzzleRuntime.Redo`): guardar a direção
> no `PuzzleMove` no momento do comando, em vez de reconstruí-la por subtração.
> `PuzzleMove` já é um `readonly struct`; adicionar `public GridCoordinate Direction { get; }`
> preenchido em `TryMove` resolve os dois casos (deslize e comando que termina
> onde começou) sem tocar em mais nada.

### 5.3 `SwitchGroupDoorTests` — a mecânica de 19 fases

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TW08.Puzzle;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// Sensores e portas — a mecânica presente em 19 das 36 fases enviadas.
    ///
    /// A regra difícil é a transição atômica: quando o grupo deveria fechar mas
    /// algum painel está ocupado pelo operador ou por uma carga, o grupo INTEIRO
    /// permanece aberto. Sem isso a porta fecharia em cima de quem está nela.
    /// É a regra que o solver espelha em door_state() e a única do motor que
    /// nunca foi executada por teste.
    ///
    /// Tabuleiro 7x5 com borda de parede; o interior é x 1..5, y 1..3.
    /// </summary>
    public sealed class SwitchGroupDoorTests
    {
        private GameObject host;
        private PuzzleLevelDefinition level;

        [TearDown]
        public void TearDown()
        {
            if (host != null) UnityEngine.Object.DestroyImmediate(host);
            if (level != null) UnityEngine.Object.DestroyImmediate(level);
        }

        [Test]
        public void DoorStartsClosedWhileTheSensorIsEmpty()
        {
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(1, 1),
                crates: new[] { new GridCoordinate(2, 1) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            Assert.IsFalse(runtime.IsSwitchGroupOpen("sg-a"));
            Assert.IsTrue(runtime.Board.IsBlocked(new GridCoordinate(4, 1)));
        }

        [Test]
        public void ClosedDoorAlsoRefusesACargoPush()
        {
            // O operador empurra a carga de (2,1) para (3,1) e depois tenta
            // empurrá-la para dentro da porta fechada em (4,1).
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(1, 1),
                crates: new[] { new GridCoordinate(2, 1) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.IsFalse(runtime.TryMove(GridCoordinate.Right),
                "Carga não atravessa porta fechada.");
            Assert.IsTrue(runtime.Board.Crates.ContainsKey(new GridCoordinate(3, 1)),
                "Comando recusado não pode ter mexido a carga.");
        }

        [Test]
        public void SensorOccupiedAtStartLeavesTheDoorOpen()
        {
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(1, 1),
                crates: new[] { new GridCoordinate(3, 3) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            Assert.IsTrue(runtime.IsSwitchGroupOpen("sg-a"));
            Assert.IsFalse(runtime.Board.IsBlocked(new GridCoordinate(4, 1)));

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.IsTrue(runtime.TryMove(GridCoordinate.Right));
            Assert.AreEqual(new GridCoordinate(4, 1), runtime.Board.PlayerPosition,
                "Com a porta aberta o operador atravessa o painel.");
        }

        [Test]
        public void TheGroupStaysOpenWhileTheOperatorStandsOnAPanel()
        {
            // Dois sensores, só um com carga: o grupo DEVERIA fechar. Mas o
            // operador começa em cima do painel, então o grupo inteiro fica
            // aberto — é a transição atômica.
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(4, 1),
                crates: new[] { new GridCoordinate(3, 3) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3), new GridCoordinate(5, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            Assert.IsTrue(runtime.IsSwitchGroupOpen("sg-a"),
                "A porta não pode fechar em cima do operador.");
            Assert.IsFalse(runtime.Board.IsBlocked(new GridCoordinate(4, 1)));
        }

        [Test]
        public void TheDoorClosesTheMomentThePanelIsVacated()
        {
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(4, 1),
                crates: new[] { new GridCoordinate(3, 3) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3), new GridCoordinate(5, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            Assert.IsTrue(runtime.TryMove(GridCoordinate.Up));
            Assert.AreEqual(new GridCoordinate(4, 2), runtime.Board.PlayerPosition);

            Assert.IsFalse(runtime.IsSwitchGroupOpen("sg-a"));
            Assert.IsTrue(runtime.Board.IsBlocked(new GridCoordinate(4, 1)));
            Assert.IsFalse(runtime.TryMove(GridCoordinate.Down),
                "A porta fechou atrás do operador; ele não volta por ali.");
        }

        [Test]
        public void UndoReopensTheDoorThatClosedBehindTheOperator()
        {
            PuzzleRuntime runtime = BuildRuntime(
                player: new GridCoordinate(4, 1),
                crates: new[] { new GridCoordinate(3, 3) },
                goals: new[] { new GridCoordinate(5, 1) },
                sensors: new[] { new GridCoordinate(3, 3), new GridCoordinate(5, 3) },
                doors: new[] { new GridCoordinate(4, 1) });

            runtime.TryMove(GridCoordinate.Up);
            Assert.IsTrue(runtime.Board.IsBlocked(new GridCoordinate(4, 1)));

            Assert.IsTrue(runtime.Undo());
            Assert.AreEqual(new GridCoordinate(4, 1), runtime.Board.PlayerPosition);
            Assert.IsFalse(runtime.Board.IsBlocked(new GridCoordinate(4, 1)),
                "Desfazer devolve o operador ao painel; a porta tem que reabrir junto.");
            Assert.IsTrue(runtime.IsSwitchGroupOpen("sg-a"));
        }

        // --------------------------------------------------------- Apoio --

        private PuzzleRuntime BuildRuntime(
            GridCoordinate player,
            GridCoordinate[] crates,
            GridCoordinate[] goals,
            GridCoordinate[] sensors,
            GridCoordinate[] doors)
        {
            const int width = 7;
            const int height = 5;

            List<GridCoordinate> walls = new();
            for (int x = 0; x < width; x++)
            {
                walls.Add(new GridCoordinate(x, 0));
                walls.Add(new GridCoordinate(x, height - 1));
            }

            for (int y = 1; y < height - 1; y++)
            {
                walls.Add(new GridCoordinate(0, y));
                walls.Add(new GridCoordinate(width - 1, y));
            }

            List<PuzzleCrateDefinition> crateDefinitions = new();
            for (int i = 0; i < crates.Length; i++)
            {
                crateDefinitions.Add(new PuzzleCrateDefinition(
                    "crate-" + i, PuzzleEntityKind.Crate, crates[i]));
            }

            level = ScriptableObject.CreateInstance<PuzzleLevelDefinition>();
            SetField(level, "levelId", "test-switch-group");
            SetField(level, "displayName", "Switch Group");
            SetField(level, "width", width);
            SetField(level, "height", height);
            SetField(level, "cellSize", 1f);
            SetField(level, "playerStart", player);
            SetField(level, "walls", walls);
            SetField(level, "goals", new List<GridCoordinate>(goals));
            SetField(level, "crates", crateDefinitions);
            SetField(level, "switchGroups", new List<PuzzleSwitchGroupDefinition>
            {
                BuildGroup("sg-a", sensors, doors)
            });

            host = new GameObject("Switch Group Runtime");
            PuzzleRuntime runtime = host.AddComponent<PuzzleRuntime>();
            runtime.Configure(level, null, Array.Empty<PuzzleEntityView>());
            runtime.Initialize();
            Assert.IsNotNull(runtime.Board, "O nível de teste precisa passar no validador.");
            return runtime;
        }

        /// <summary>
        /// PuzzleSwitchGroupDefinition não expõe construtor nem setter: só dá
        /// para montá-lo por reflexão. Isso é um defeito de testabilidade e é a
        /// razão de a mecânica de 19 fases nunca ter sido testada.
        /// </summary>
        private static PuzzleSwitchGroupDefinition BuildGroup(
            string id, GridCoordinate[] sensors, GridCoordinate[] doors)
        {
            PuzzleSwitchGroupDefinition group = new();
            SetPrivate(group, "id", id);
            SetPrivate(group, "sensors", new List<GridCoordinate>(sensors));
            SetPrivate(group, "doors", new List<GridCoordinate>(doors));
            return group;
        }

        private static void SetPrivate<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }

        private static void SetField<T>(PuzzleLevelDefinition target, string fieldName, T value)
        {
            FieldInfo field = typeof(PuzzleLevelDefinition)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field '{fieldName}'.");
            field.SetValue(target, value);
        }
    }
}
```

### 5.4 `ClockMechanicsTests` — botão de direção e portão temporizado

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using TW08.Puzzle;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// As duas mecânicas movidas pelo relógio de comandos, ambas hoje sem
    /// nenhuma execução em teste.
    ///
    /// Botão de direção (só em L17): inverte TODAS as esteiras do tabuleiro, e
    /// dispara na célula onde o comando TERMINOU. Deslizar por cima dele não
    /// pode inverter nada — se invertesse, a esteira mudaria de sentido com a
    /// carga ainda em cima, e L17 deixaria de ser legível e de ser solúvel.
    ///
    /// Portão temporizado (só em S08): fica fechado enquanto CommandCount for
    /// menor que o prazo, e a partir daí abre para sempre. Desfazer rebobina o
    /// relógio e pode fechá-lo de novo.
    ///
    /// Corredor 8x3 com borda de parede; a faixa central (y=1) é o que varia.
    /// </summary>
    public sealed class ClockMechanicsTests
    {
        private static PuzzleBoardModel Build(
            GridCoordinate? player = null,
            IEnumerable<GridCoordinate> ice = null,
            IReadOnlyDictionary<GridCoordinate, GridCoordinate> conveyors = null,
            IEnumerable<GridCoordinate> buttons = null,
            IEnumerable<PuzzleTimedBlockDefinition> timed = null,
            IReadOnlyDictionary<string, GridCoordinate> crates = null)
        {
            const int width = 8;
            List<GridCoordinate> walls = new();
            for (int x = 0; x < width; x++)
            {
                walls.Add(new GridCoordinate(x, 0));
                walls.Add(new GridCoordinate(x, 2));
            }

            walls.Add(new GridCoordinate(0, 1));
            walls.Add(new GridCoordinate(width - 1, 1));

            return new PuzzleBoardModel(
                width, 3, walls,
                new[] { new GridCoordinate(width - 2, 1) },
                player ?? new GridCoordinate(1, 1),
                crates ?? new Dictionary<string, GridCoordinate>(),
                null, null, null,
                ice, conveyors, null, buttons, timed);
        }

        // --------------------------------------------- Botão de direção --

        [Test]
        public void SteppingOnTheButtonFlipsEveryConveyorOnTheBoard()
        {
            // Botão em (2,1); esteira em (4,1) apontando para a direita.
            PuzzleBoardModel board = Build(
                conveyors: new Dictionary<GridCoordinate, GridCoordinate>
                {
                    [new GridCoordinate(4, 1)] = GridCoordinate.Right
                },
                buttons: new[] { new GridCoordinate(2, 1) });

            Assert.IsFalse(board.ConveyorsInverted);
            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.IsTrue(board.ConveyorsInverted, "Pisar no botão inverte a correia.");

            board.TryMove(GridCoordinate.Right, out _);           // anda até (3,1)
            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));

            // Entra na esteira invertida em (4,1) e é devolvido a (3,1).
            Assert.AreEqual(new GridCoordinate(3, 1), board.PlayerPosition,
                "Com o botão acionado, a correia devolve quem entra nela.");
        }

        [Test]
        public void WithoutTheButtonTheSameConveyorCarriesForward()
        {
            // Contraprova: sem acionar o botão, a mesma esteira leva adiante.
            PuzzleBoardModel board = Build(
                conveyors: new Dictionary<GridCoordinate, GridCoordinate>
                {
                    [new GridCoordinate(4, 1)] = GridCoordinate.Right
                });

            board.TryMove(GridCoordinate.Right, out _);
            board.TryMove(GridCoordinate.Right, out _);
            board.TryMove(GridCoordinate.Right, out _);

            Assert.AreEqual(new GridCoordinate(5, 1), board.PlayerPosition);
            Assert.IsFalse(board.ConveyorsInverted);
        }

        [Test]
        public void SlidingOverTheButtonDoesNotPressIt()
        {
            // Gelo em 3..5 com o botão em (4,1): o operador atravessa o botão
            // sem parar e vai até (6,1). Inverter aqui mudaria a correia com a
            // carga ainda percorrendo o gelo.
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(2, 1),
                ice: new[] { new GridCoordinate(3, 1), new GridCoordinate(4, 1), new GridCoordinate(5, 1) },
                buttons: new[] { new GridCoordinate(4, 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(6, 1), board.PlayerPosition);
            Assert.IsFalse(board.ConveyorsInverted,
                "O botão dispara onde o comando termina, não onde ele passa.");
        }

        [Test]
        public void StoppingOnTheButtonAtTheEndOfASlideDoesPressIt()
        {
            // Mesmo gelo, botão na célula onde o deslize PARA.
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(2, 1),
                ice: new[] { new GridCoordinate(3, 1), new GridCoordinate(4, 1), new GridCoordinate(5, 1) },
                buttons: new[] { new GridCoordinate(6, 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(6, 1), board.PlayerPosition);
            Assert.IsTrue(board.ConveyorsInverted);
        }

        [Test]
        public void UndoUnflipsTheConveyors()
        {
            PuzzleBoardModel board = Build(
                conveyors: new Dictionary<GridCoordinate, GridCoordinate>
                {
                    [new GridCoordinate(4, 1)] = GridCoordinate.Right
                },
                buttons: new[] { new GridCoordinate(2, 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out PuzzleMove move));
            Assert.IsTrue(board.ConveyorsInverted);

            Assert.IsTrue(board.TryUndo(move));
            Assert.IsFalse(board.ConveyorsInverted, "Desfazer devolve a correia ao sentido original.");
        }

        // ----------------------------------------- Portão temporizado --

        [Test]
        public void TimedGateIsClosedBeforeItsDeadline()
        {
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(2, 1),
                timed: new[] { new PuzzleTimedBlockDefinition(new GridCoordinate(3, 1), 1) });

            Assert.IsTrue(board.IsBlocked(new GridCoordinate(3, 1)));
            Assert.IsFalse(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(0, board.CommandCount, "Comando recusado não adianta o relógio.");
        }

        [Test]
        public void TimedGateOpensOnceTheShiftReachesTheDeadline()
        {
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(2, 1),
                timed: new[] { new PuzzleTimedBlockDefinition(new GridCoordinate(3, 1), 1) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Left, out _));   // gasta 1 comando
            Assert.IsFalse(board.IsBlocked(new GridCoordinate(3, 1)));

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));  // volta a (2,1)
            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));  // atravessa
            Assert.AreEqual(new GridCoordinate(3, 1), board.PlayerPosition);
        }

        [Test]
        public void UndoRewindsTheClockAndClosesTheGateAgain()
        {
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(2, 1),
                timed: new[] { new PuzzleTimedBlockDefinition(new GridCoordinate(3, 1), 1) });

            board.TryMove(GridCoordinate.Left, out PuzzleMove first);
            board.TryMove(GridCoordinate.Right, out PuzzleMove second);
            board.TryMove(GridCoordinate.Right, out PuzzleMove third);
            Assert.AreEqual(new GridCoordinate(3, 1), board.PlayerPosition);

            Assert.IsTrue(board.TryUndo(third));
            Assert.IsTrue(board.TryUndo(second));
            Assert.IsTrue(board.TryUndo(first));

            Assert.AreEqual(0, board.CommandCount);
            Assert.IsTrue(board.IsBlocked(new GridCoordinate(3, 1)),
                "Rebobinar o turno fecha o portão de novo — o prazo é função do relógio.");
        }

        [Test]
        public void ClosedGateAlsoRefusesACargoPush()
        {
            PuzzleBoardModel board = Build(
                player: new GridCoordinate(1, 1),
                timed: new[] { new PuzzleTimedBlockDefinition(new GridCoordinate(3, 1), 5) },
                crates: new Dictionary<string, GridCoordinate> { ["c1"] = new(2, 1) });

            Assert.IsFalse(board.TryMove(GridCoordinate.Right, out _));
            Assert.IsTrue(board.Crates.ContainsKey(new GridCoordinate(2, 1)),
                "Comando recusado não pode ter mexido a carga.");
        }

        [Test]
        public void SlidingStopsInFrontOfAClosedGate()
        {
            // Gelo em 2..3, portão fechado em (4,1): o deslize precisa parar em
            // (3,1). Se o portão fosse ignorado durante o deslize, a carga
            // atravessaria uma parede que a fase considera fechada.
            PuzzleBoardModel board = Build(
                ice: new[] { new GridCoordinate(2, 1), new GridCoordinate(3, 1) },
                timed: new[] { new PuzzleTimedBlockDefinition(new GridCoordinate(4, 1), 9) });

            Assert.IsTrue(board.TryMove(GridCoordinate.Right, out _));
            Assert.AreEqual(new GridCoordinate(3, 1), board.PlayerPosition);
        }
    }
}
```

### 5.5 `RaceMedalPersistenceTests` — o bug da medalha

> **Este teste falha hoje. Ele é o bug.** Passa depois de corrigir as duas
> chamadas em `RaceSessionController.OnRacerFinished` (ver conserto abaixo).

```csharp
using System.Reflection;
using NUnit.Framework;
using TW08.Race;
using UnityEngine;

namespace TW08.Tests.EditMode
{
    /// <summary>
    /// A medalha que o jogo GRAVA tem que ser a mesma que o jogo MOSTRA.
    ///
    /// RaceSessionController.OnRacerFinished calcula a medalha exibida com o
    /// dano de carga real e depois persiste em dois lugares — RaceProgressStore
    /// e SaveManager — sem passar esse dano. Os dois caem no parâmetro opcional
    /// cargoDamage = 0f. Um turno que a HUD chama de PRATA é salvo como PLATINA.
    /// </summary>
    public sealed class RaceMedalPersistenceTests
    {
        private const string TrackId = "test-medal-track";
        private RaceDefinition rules;
        private RaceTrackDefinition track;

        [SetUp]
        public void SetUp()
        {
            rules = ScriptableObject.CreateInstance<RaceDefinition>();
            SetField(rules, "raceId", "test-medal-rules");
            SetField(rules, "bronzeTime", 75f);
            SetField(rules, "silverTime", 65f);
            SetField(rules, "goldTime", 58f);
            SetField(rules, "platinumTime", 52f);
            SetField(rules, "maximumCargoDamageForGold", 5f);

            track = ScriptableObject.CreateInstance<RaceTrackDefinition>();
            SetField(track, "trackId", TrackId);
            SetField(track, "displayName", "Medal Track");
            SetField(track, "raceRules", rules);

            ClearPrefs();
        }

        [TearDown]
        public void TearDown()
        {
            ClearPrefs();
            Object.DestroyImmediate(track);
            Object.DestroyImmediate(rules);
        }

        [Test]
        public void DamageDowngradesTheMedalTheDriverIsShown()
        {
            // Tempo de platina, mas a carga chegou destruída: a régua manda prata.
            Assert.AreEqual(4, track.GetMedal(51f, 0f));
            Assert.AreEqual(2, track.GetMedal(51f, 40f),
                "Platina exige carga intacta e ouro exige dano <= 5.");
        }

        [Test]
        public void TheStoredMedalIsTheOneTheDriverWasShown()
        {
            // ESTE É O BUG. RaceSessionController grava sem o dano; a régua
            // então devolve 4 para uma corrida que o jogador viu valer 2.
            RaceProgressStore.Record(track, 51f, 40f);

            Assert.AreEqual(
                track.GetMedal(51f, 40f),
                RaceProgressStore.GetMedal(TrackId),
                "A medalha gravada tem que ser a mesma que a HUD mostrou.");
        }

        [Test]
        public void DroppingTheDamageArgumentInflatesTheMedal()
        {
            // Documenta o mecanismo exato da falha para que ninguém reintroduza
            // a chamada sem o dano por descuido.
            RaceProgressStore.Record(track, 51f);          // como o jogo chama hoje
            int stored = RaceProgressStore.GetMedal(TrackId);

            Assert.AreEqual(4, stored);
            Assert.AreNotEqual(
                track.GetMedal(51f, 40f), stored,
                "Chamar Record sem o dano promove a medalha em dois degraus.");
        }

        [Test]
        public void ABetterMedalNeverOverwritesAWorseOneDownwards()
        {
            // Regressão da regra que já existe e não pode se perder no conserto.
            RaceProgressStore.Record(track, 51f, 0f);      // platina
            RaceProgressStore.Record(track, 70f, 30f);     // corrida ruim depois

            Assert.AreEqual(4, RaceProgressStore.GetMedal(TrackId),
                "O recorde de medalha só sobe.");
            Assert.AreEqual(51f, RaceProgressStore.GetBestTime(TrackId), 0.001f);
        }

        private static void ClearPrefs()
        {
            PlayerPrefs.DeleteKey("tw08.race." + TrackId + ".completed");
            PlayerPrefs.DeleteKey("tw08.race." + TrackId + ".best");
            PlayerPrefs.DeleteKey("tw08.race." + TrackId + ".medal");
            PlayerPrefs.Save();
        }

        private static void SetField<T>(object target, string name, T value)
        {
            FieldInfo field = target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing field '{name}'.");
            field.SetValue(target, value);
        }
    }
}
```

> **Conserto** — `RaceSessionController.OnRacerFinished`, duas linhas:
> ```csharp
> RaceProgressStore.Record(track, finishTime, cargoDamage);
> // ...
> saveManager.RecordRaceCompletion(track, finishTime, cargoDamage);
> ```
> E, para o erro não voltar: **remover o valor padrão `= 0f`** de
> `RaceProgressStore.Record` e `SaveManager.RecordRaceCompletion`. Um parâmetro
> opcional que silenciosamente muda a medalha do jogador não deveria ser
> opcional.

### 5.6 Bônus barato — `LevelDefinitionWiringTests`

Não está entre os cinco, mas custa vinte minutos e fecha o buraco de 4.2:

```csharp
[Test]
public void ConveyorDirectionMapsToTheRightStep()
{
    Assert.AreEqual(GridCoordinate.Up,
        new PuzzleConveyorDefinition(new GridCoordinate(1, 1), ConveyorDirection.Up).Step);
    Assert.AreEqual(GridCoordinate.Down,
        new PuzzleConveyorDefinition(new GridCoordinate(1, 1), ConveyorDirection.Down).Step);
    Assert.AreEqual(GridCoordinate.Left,
        new PuzzleConveyorDefinition(new GridCoordinate(1, 1), ConveyorDirection.Left).Step);
    Assert.AreEqual(GridCoordinate.Right,
        new PuzzleConveyorDefinition(new GridCoordinate(1, 1), ConveyorDirection.Right).Step);
}

[Test]
public void TheBoardReceivesEveryMechanicDeclaredByTheLevel()
{
    // Monta um PuzzleLevelDefinition com as cinco mecânicas novas e verifica
    // que o construtor PuzzleBoardModel(level) copiou todas. Hoje esse
    // construtor — o único usado em produção — só é exercitado por níveis sem
    // nenhuma delas: um campo esquecido no ToDictionary passaria despercebido.
    PuzzleBoardModel board = new(level);

    Assert.IsTrue(board.IsIce(new GridCoordinate(3, 1)));
    Assert.IsTrue(board.TryGetConveyor(new GridCoordinate(4, 1), out GridCoordinate step));
    Assert.AreEqual(GridCoordinate.Up, step);
    Assert.Contains(new GridCoordinate(2, 1), (System.Collections.ICollection)board.DirectionButtons);
    Assert.AreEqual(1, board.TimedBlocks.Count);
    Assert.AreEqual(4, board.TimedBlocks[0].OpensAfterCommands);
    Assert.AreEqual(1, board.Patrols.Count);
    Assert.AreEqual(new GridCoordinate(5, 1), board.Patrols[0].PositionAt(1));
}
```

---

## 6. Processo

### 6.1 O que deveria rodar antes de cada commit e não roda

Não há hook, não há script, não há `.github/`. **Não existe CI.** O contrato do
`AGENTS.md` diz que todo commit deve "passar validação estática
(`static_validation.json` quando aplicável)" — mas esse arquivo é um artefato
morto: registra `"csharp_files": 75` quando `Assets/` tem 211 arquivos `.cs`, e
`"unity_editor_compilation_executed": false`. **A única checagem
obrigatória do contrato é um JSON desatualizado que ninguém regenera.**

Pré-commit mínimo, em ordem de custo crescente:

| Passo | Custo | Bloqueia o quê |
|---|---|---|
| 1. `python Tools/puzzle/tw08_solver.py --layouts Docs/level-layouts.json --out Tools/puzzle/proven-solutions.json` | segundos | Fase nova insolúvel |
| 2. Diff de `proven-solutions.json` limpo, ou justificativa no commit | zero | Mudança de regra que altera o ótimo sem ninguém notar |
| 3. Unity EditMode em batchmode | ~1 min | Regressão de lógica (129 → ~160 testes) |
| 4. Unity PlayMode em batchmode | ~1 min | Regressão de apresentação |
| 5. `TW08ProjectValidator.ValidateProject` via `-executeMethod` | ~30 s | Asset de fase inválido |

Os passos 1 e 2 já são possíveis hoje sem tocar em nada. Os 3 e 4 exigem só a
linha de comando do Unity. O 5 exige transformar o `[MenuItem]` num método
público chamável por `-executeMethod` que retorne código de saída — vinte
minutos de trabalho.

### 6.2 O que um CI deveria executar

Ordem deliberada: o mais barato e mais informativo primeiro.

1. **Gate do solver** (Python, sem Unity, segundos). Falha se alguma fase for
   insolúvel, se `replayOk` for falso, **ou se alguma fase enviada não tiver
   entrada no relatório**.
2. **Paridade asset ↔ prova** (Python, segundos). O script que escrevi para
   esta auditoria compara os 27 layouts com os 27 assets campo a campo; vale
   commitar em `Tools/puzzle/check_asset_parity.py` e rodá-lo aqui. Hoje dá
   0 divergências; amanhã, sem isso, ninguém sabe.
3. **EditMode**, incluindo os novos `SolverParityTests`.
4. **PlayMode**.
5. **`TW08ProjectValidator`** sobre todos os assets.
6. **Build Windows** (`TW08BuildPipeline.BuildWindowsFromCommandLine`, que já
   devolve código de saída correto — está pronto para CI e ninguém usa).
7. **Guarda de contaminação**: `git diff --name-only` contra a lista de
   extensões proibidas do `AGENTS.md` (`.bin`, `.smd`, `.gen`, `.mdx`, ROMs).
   O `.gitignore` cobre, mas `.gitignore` não é gate — um `git add -f` passa.

Comandos batchmode:

```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode \
      -testResults TestResults-EditMode.xml -quit
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode \
      -testResults TestResults-PlayMode.xml -quit
```

### 6.3 Higiene que dá para arrumar hoje

1. **`TestResults-*.xml` estão versionados** — 20 arquivos na raiz do repo,
   artefatos de execução. Não estão no `.gitignore`. Adicionar
   `TestResults*.xml` e remover os antigos com `git rm --cached`.
2. **Adicionar `"TW08.Editor"` às `references` do asmdef de EditMode** — uma
   linha destrava 9.000 linhas de pipeline para teste.
3. **Dar construtor público a `PuzzleSwitchGroupDefinition` e
   `PuzzleGoalRequirementDefinition`**, como `PuzzleCrateDefinition`,
   `PuzzleConveyorDefinition`, `PuzzleTimedBlockDefinition` e
   `PuzzlePatrolDefinition` já têm. É a razão mecânica de a mecânica de 19 fases
   nunca ter sido testada: para testá-la é preciso reflexão, e ninguém encarou.
4. **Estender `PuzzleLevelValidator` para as cinco mecânicas novas.** Hoje ele
   não olha `iceCells`, `conveyors`, `patrols`, `directionButtons`,
   `timedBlocks` nem `fakeWalls`. Passam sem reclamação:
   - esteira em cima de parede, ou apontando direto para uma parede (armadilha
     que o jogador lê como bug);
   - célula de gelo em cima de parede;
   - rota de robô com células não adjacentes (o robô teleporta) ou atravessando
     parede;
   - botão de direção em cima de parede;
   - portão temporizado sobre a célula inicial do jogador — o construtor de
     `PuzzleBoardModel` só verifica parede e carga, então o operador **começa a
     fase dentro de um portão fechado**;
   - duplicatas em qualquer uma dessas listas.
5. **Regenerar ou aposentar `static_validation.json`.** Um gate obrigatório que
   está errado é pior que gate nenhum: ensina a equipe a ignorar gates.
6. **`SimpleDeadlockDetector` diverge do solver.** O detector do jogo trata
   qualquer carga sobre alvo como segura (`if (board.IsGoal(crate)) continue;`),
   enquanto o solver checa o **tipo exigido** pela doca. Consequência concreta:
   nas 12 fases com doca tipada, uma carga pesada encravada num canto que também
   é doca de frágil está morta, e a HUD não acende "CARGA TRAVADA // USE UNDO".
   O jogador fica preso sem aviso. Alinhar o detector ao solver é meia dúzia de
   linhas, e o teste cabe em `HudTests` ao lado dos dois que já existem.

---

## 7. O que está bom

Não é um projeto com uma rede de segurança improvisada. É um projeto com uma
rede boa e com buracos identificáveis — o que é bem diferente e bem melhor.

- **129 testes rodam em 0,17 s.** Suíte instantânea é suíte que a equipe roda.
  Isso não acontece por sorte: acontece porque a lógica foi separada da cena.
- **A decisão de extrair regras puras da UI foi acertada e paga todo dia.**
  `HudFormat`, `PuzzleHudStatusResolver`, `ShiftReportPresenter`, `ShiftCredits`,
  `PuzzleAdvisor`, `NarrativeCatalog`: tudo testável sem `GameObject`. É por
  isso que HUD e menus têm cobertura de verdade enquanto controllers de 600
  linhas não precisam.
- **`NarrativeTests` é exemplar.** 21 casos, incluindo os degenerados
  (sequência nula, sem linhas, `PlayOnce` atravessando instâncias). É o padrão
  que as outras áreas deveriam copiar.
- **`SlideMechanicsTests` e `PatrolMechanicsTests` são bem escritos** — cada
  teste explica em comentário *por que* a regra é aquela, não só o que ela faz.
  `PlayerNeverSlidesThroughTheCrateItJustPushed` e
  `ClosedConveyorLoopTerminatesInsteadOfHanging` cobrem exatamente os dois casos
  em que essa mecânica costuma morrer.
- **O motor foi projetado para ser provável.** Robô como função do contador de
  comandos, portão como prazo e não como duração, botão disparando ao fim do
  comando: são três decisões que existem para manter o espaço de estados finito
  e o solver capaz de fechar a prova. Isso é raro e é o que torna o gate do
  solver possível.
- **A paridade solver ↔ motor está correta hoje.** Reproduzi as 36 soluções
  provadas no modelo do C# contra os assets enviados: 36/36, custo exato.
- **Os assets batem com os layouts provados.** 27/27, zero divergência em 15
  campos comparados.
- **Sete das oito fases com deslize têm o limite de platina provado ótimo** por
  Dijkstra puro.
- **Os comentários de código são de verdade.** Explicam decisão, não sintaxe:
  "os robôs avançam junto com este comando, então tudo é validado contra onde
  eles VÃO estar", "o teto de iterações protege contra esteiras em circuito
  fechado". Isso é o que permitiu esta auditoria em um dia.
- **`TW08BuildPipeline` já devolve código de saída correto** e está pronto para
  CI. Falta só o CI.

---

## Apêndice — como reproduzir o que está neste relatório

Tudo abaixo roda em Python, sem Unity.

```bash
# Gate atual (layouts) — 27/27
python Tools/puzzle/tw08_solver.py --layouts Docs/level-layouts.json

# A armadilha do --assets: mesmas fases, custos diferentes
python Tools/puzzle/tw08_solver.py --assets Assets/_Project/ScriptableObjects/Campaign \
       --only Level11,Level16,Level17,Level18,Secret08

# Fases 01-03, que não estão em relatório nenhum
python Tools/puzzle/tw08_solver.py --assets Assets/_Project/ScriptableObjects/VerticalSlice
```

Os três scripts de verificação escritos para esta auditoria — paridade
asset ↔ layout, replay independente das 36 fases contra os assets, e o Dijkstra
de otimalidade — não foram commitados (o escopo deste agente é só este arquivo).
Os dois primeiros merecem virar `Tools/puzzle/check_asset_parity.py` e
`Tools/puzzle/check_engine_replay.py`, e entrar no CI conforme 6.2.
