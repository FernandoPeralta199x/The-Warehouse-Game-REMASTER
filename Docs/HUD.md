# HUD — The Warehouse Nº 08

Documento do serviço de HUD: o que existe, como está animado e o que ainda
depende de mudança em código que não pertence a esta frente.

Toda animação usa `Assets/_Project/Scripts/Motion/UIMotion.cs`
(namespace `TW08.Motion`), que roda em tempo **não escalado** e aplica o estado
final imediatamente fora do Play Mode — por isso os builders de cena podem
chamar `Configure(...)` sem deixar elementos a meio caminho.

**Princípio:** animação é decorativa. Texto, número e gravação de progresso já
estão aplicados antes de qualquer tween começar. Se um movimento for
interrompido (troca de cena, `OnDisable`), o valor final continua correto.

---

## 1. Arquivos

### Novos — `Assets/_Project/Scripts/UI/Hud/`

| Arquivo | Papel |
| --- | --- |
| `HudPalette.cs` | Paleta do HUD em runtime. Repete os tons de `TW08ProductionSceneUtility`, que é editor-only e não existe no player. |
| `HudFormat.cs` | Toda formatação numérica e de rótulo. Expõe as constantes de formato consumidas por `UIMotion.CountTo`. **Lógica pura, testada.** |
| `PuzzleHudStatus.cs` | `PuzzleHudStatus` + `PuzzleHudStatusResolver`: decide estado, texto e cor da faixa de status. **Lógica pura, testada.** |
| `ShiftReportPresenter.cs` | `ShiftReportLine` + montagem do extrato de fim de turno, incluindo a linha de teto por fase. **Lógica pura, testada.** |
| `AnimatedCounter.cs` | Rótulo numérico que conta do valor antigo até o novo. Guarda o valor lógico separado do texto pintado. |
| `HudFx.cs` | Efeitos curtos (`Flash`, `FadeInFrom`, `Punch`, `PopIn`, `Shake`, `Delayed`) com gerência de handle por efeito. |
| `ScreenFader.cs` | Cortina de transição entre cenas. |
| `PuzzleShiftReportPanel.cs` | Tela de conclusão de turno do puzzle. |
| `RaceResultPanel.cs` | Tela de resultado da corrida. |

### Alterados

- `Assets/_Project/Scripts/UI/PuzzleHudController.cs`
- `Assets/_Project/Scripts/UI/PuzzleToolBarController.cs`
- `Assets/_Project/Scripts/UI/RaceHudController.cs`
- `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs` — apenas `CreateHud`,
  `CreateToolBar` e os novos auxiliares de HUD
- `Assets/_Project/Scripts/Editor/TW08RaceSceneBuilder.cs` — apenas `CreateHud` e
  os novos auxiliares de HUD
- `Assets/_Project/Tests/EditMode/HudTests.cs` (novo)

---

## 2. HUD de puzzle

### Entrada da fase
`UIEntranceAnimator` em **HUD Top** (`SlideDown`, atraso 0,05 s) e **HUD Bottom**
(`SlideUp`, atraso 0,16 s), ambos com cascata de filhos a cada 0,06 s. A barra de
ferramentas entra em `SlideLeft` com atraso 0,28 s. O resultado é o painel
subindo linha a linha, no ritmo de terminal que o menu já usa.

### Contador de movimentos
O rótulo composto antigo (`MOVIMENTOS 000   UNDO 00   REDO 00`) foi dividido:

- **Moves Value** — só o número, animado com `UIMotion.CountTo` no formato
  `HudFormat.MovesValueFormat`, mais `Punch` a cada mudança.
- **Moves** — `UNDO nn   REDO nn`, estático.

A divisão é necessária porque `CountTo` reescreve o rótulo inteiro a cada frame;
undo/redo no mesmo texto seriam apagados pelo tween.

**Compatibilidade:** se `movesValueText` não for configurado (cenas antigas, como
as de `TW08VerticalSliceSetup`), o controller volta ao rótulo composto de antes.

### Empurrão de carga
`MoveApplied` com `move.CrateMoved` faz o `Punch` do contador ser mais forte
(0,20 contra 0,10) e acende um `Flash` ciano que volta ao verde.

### Alerta de carga travada
Ao entrar em `PuzzleHudStatus.Deadlock`:

- `UIMotion.Shake` no painel inferior inteiro (13 px, 0,46 s)
- `Punch` na faixa de status
- **piscar contínuo** vermelho ↔ âmbar, feito no `Update` com `Mathf.PingPong` em
  tempo não escalado, e não com um flash único — um flash apagaria antes de o
  jogador desviar o olhar do tabuleiro
- texto: `ALERTA: CARGA TRAVADA // USE UNDO`

Ao sair do estado, a cor volta por `ColorTo`.

### Porta abrindo/fechando
`SwitchGroupStateChanged` escreve `PORTA <ID> ABERTA/FECHADA` na faixa de status
com `Flash` (ciano ao abrir, âmbar ao fechar) e `Punch`. O alarme de travamento
tem prioridade e o aviso é suprimido enquanto ele estiver ativo. O próximo
movimento devolve a faixa a `ROTA ATIVA`.

### Tela de conclusão de turno
`PuzzleShiftReportPanel`, alimentado pelo `PuzzleShiftReport` que o
`SaveManager.CommitPuzzleShift` já devolvia. Sequência:

1. Painel entra com fade (`CanvasGroup`) + `SlideIn` de baixo.
2. Medalha faz `PopIn` (0,42 s) e leva um `Punch` 0,58 s depois; cor por medalha
   (platina/ouro/bronze).
3. Indicador limpo × assistido entra por fade, verde ou âmbar.
4. **Extrato em cascata**: uma linha a cada 0,085 s, cada uma com fade + `PopIn`.
5. Total sobe com `CountTo` no formato `+{0} CRÉDITOS`.
6. Saldo sobe com `CountTo` a partir do valor anterior ao turno, para o jogador
   ver quanto este turno somou.

**Linha de teto.** `ShiftCredits.BuildStatement` devolve os bônus brutos, sem o
teto por fase; quem aplica o teto é `ShiftCredits.Evaluate`. Um turno perfeito
soma 425 em bônus e paga 250. Para a soma na tela fechar com o crédito recebido,
`ShiftReportPresenter.BuildLines` acrescenta uma linha explícita
`TETO DA FASE -175`. Isso é coberto por teste.

A cena reserva **7 rótulos** de extrato: os seis bônus possíveis mais o corte.

Sem `SaveManager` na cena (fase aberta isolada), a tela ainda aparece — só sem
extrato e com total zero.

### Transição de saída
`ScreenFader` cobre o Canvas inteiro, é sempre o último filho, e só passa a
bloquear cliques durante a saída — assim o jogador não dispara a próxima fase
duas vezes no meio do fade. Ao entrar na cena faz fade de preto. Se a cortina não
existir, a troca de cena acontece direto: o progresso nunca depende da animação.

### Indicador limpo/assistido
Chip `TURNO LIMPO` / `TURNO ASSISTIDO` no painel superior. Só anima na troca de
estado (`ColorTo` + `Punch`), ligado a `PuzzleRuntime.AssistanceUsed`. A linha
longa (`MODO ASSISTIDO // FORA DO RANKING`) continua na barra de ferramentas,
onde já estava — são dois avisos com finalidades diferentes, não duplicação.

---

## 3. Barra de ferramentas (Oficina N-8)

- **Slot acende** ao passar de indisponível para disponível: `PopIn` no botão e
  `Flash` branco → ciano no rótulo. Só na transição — repintar a cada `Refresh`
  faria a barra piscar a cada movimento do tabuleiro.
- **`Punch`** no slot ao usar a ferramenta.
- **`Shake`** (9 px, 0,36 s) no slot quando `TryUse` recusa, junto da mensagem
  de recusa.
- **Contador de usos animado**: cada slot ganhou um rótulo `Uses` próprio,
  animado com `CountTo` no formato `x{0}`. Sem os rótulos dedicados a barra volta
  ao formato antigo (`REBOBINAR x2` num rótulo só).
- **Mensagem** entra com fade + `SlideIn` de 20 px e sai com fade após 3,5 s
  (agora em `WaitForSecondsRealtime`, para casar com o tempo não escalado do
  `UIMotion`).
- **Modo de ranking** anima cor e `Punch` só na troca limpo ↔ assistido.

---

## 4. HUD de corrida

- **Velocímetro** novo (`VEL 000`), com suavização exponencial no `Update` para
  acompanhar a inércia da empilhadeira em vez de saltar a cada frame de física, e
  cor interpolando ciano → âmbar conforme `NormalizedSpeed`.
- **Volta**: `Punch` + `Flash` no contador e `Punch` no cronômetro a cada volta.
- **Última volta**: rótulo `ÚLTIMA VOLTA` com `PopIn` + fade, piscando âmbar ↔
  vermelho até a chegada.
- **Checkpoint**: rótulo `CHECKPOINT` com `PopIn` e fade automático, mais `Punch`
  no indicador de posição.
- **Contagem regressiva**: cada número entra com `PopIn` e fade; o `GO` usa
  `PopIn` mais amplo e verde, e a faixa volta a `ROTA ATIVA` depois de 1,15 s.
- **Tela de resultado** (`RaceResultPanel`): painel com fade + `SlideIn`, medalha
  com `PopIn` + `Punch`, tempo final por **máquina de escrever** e melhor tempo e
  carga entrando em cascata.
  O tempo usa `Typewriter` em vez de `CountTo` porque `CountTo` interpola um
  inteiro — um cronômetro interpolado passaria por tempos que nunca existiram.
- **Saída** (reiniciar/sair) passa pelo `ScreenFader`.

Volta e checkpoint são lidos por **polling** de `RacerProgress` no `Update`:
`RacerProgress` não publica eventos, e a HUD não pode alterar a camada de corrida
só para se avisar de uma passagem de checkpoint.

---

## 5. Compatibilidade de API

Nenhuma assinatura existente mudou. Os elementos animados entram por métodos
novos e **opcionais**:

| Controller | Método existente (intocado) | Método novo |
| --- | --- | --- |
| `PuzzleHudController` | `Configure`, `ConfigureExtendedLabels`, `ConfigureCampaignFlow` | `ConfigureMotion(movesValue, ranking, bottomBar, report, fader)` |
| `PuzzleToolBarController` | `Configure` | `ConfigureSlotDisplays(labels, counters)` |
| `RaceHudController` | `Configure`, `ConfigureArcadeOverlay` | `ConfigureMotion(vehicle, speed, lastLap, checkpoint, result, fader)` |

Chamadores fora desta frente que continuam funcionando sem alteração:

- `Editor/TW08VerticalSliceSetup.cs:340` — `PuzzleHudController.Configure`
- `Editor/TW08MegaSceneUpgrade.cs:440` — `RaceHudController.ConfigureArcadeOverlay`

A única assinatura ajustada é interna ao builder de corrida:
`TW08RaceSceneBuilder.CreateHud` passou a receber o
`ArcadeForkliftController2D` do jogador (é `private static`, chamado só em
`Build`).

---

## 6. Testes

`Assets/_Project/Tests/EditMode/HudTests.cs`, namespace `TW08.Tests.EditMode`.

Cobre só o que dá para julgar sem cena, e de propósito **não toca em
`UnityEngine.UI`**: a assembly de teste referencia apenas `TW08.Runtime`.

- Formatação: contadores, tempo em cultura invariante, volta, posição, carga,
  velocidade, sinal do extrato, textos de ranking e de porta, e a garantia de que
  contadores negativos nunca chegam à tela.
- Status: conclusão vence travamento; rótulos idênticos aos do terminal; só
  travamento alarma; a faixa de conclusão só mostra créditos depois do turno
  fechado. Dois testes cruzam o resolvedor com o `SimpleDeadlockDetector` real
  sobre um `PuzzleBoardModel` montado à mão.
- Extrato: linha de teto quando os bônus estouram; o total exibido bate com
  `ShiftCredits.Evaluate` em quatro perfis de turno; extrato ausente não quebra;
  rótulos de medalha e ranking; quantidade de linhas cabe nos slots da cena.

---

## 7. Notas de manutenção

- **Um handle por efeito.** `UIMotion.Punch` e `PopIn` capturam a escala de
  repouso no início. Reiniciar um pulso sem encerrar o anterior faz a escala ser
  capturada com o elemento inflado, e ele nunca volta ao tamanho certo. Por isso
  `HudFx.Punch`/`PopIn`/`Shake` recebem o handle por `ref` e concluem o anterior.
- **`SlideIn` grava o destino na chamada.** Se um deslize for interrompido, a
  posição atual vira o novo destino e o elemento migra. Onde há `SlideIn`
  repetido (mensagem da barra, painéis de resultado) a posição de origem é
  guardada e restaurada antes de animar.
- **`Complete()` numa `Chain` executa os passos pendentes.** Em `OnDisable`, os
  handles de sequência são encerrados com `Kill()` (`HudFx.Abort`), não com
  `Complete()`, para não disparar uma animação nova enquanto a tela sai.
- **Arquivos `.meta`.** Os novos `.cs` e a pasta `Scripts/UI/Hud/` ainda não têm
  `.meta`; o Unity os gera no primeiro import.

---

## INTEGRAÇÃO PENDENTE

Nada aqui é bloqueante — a entrega funciona sem estes itens. São melhorias que
exigem tocar em arquivos fora desta frente.

### 1. Evento de checkpoint no `RacerProgress` *(baixa prioridade)*

**Arquivo:** `Assets/_Project/Scripts/Forklift/RacerProgress.cs`

Hoje o HUD descobre a passagem por checkpoint comparando `NextCheckpointIndex` a
cada `Update`. Funciona, mas gasta um `Update` e perde a informação de *qual*
checkpoint foi cruzado.

Adicionar ao lado dos membros públicos:

```csharp
/// <summary>Checkpoint recém-cruzado e a volta em que isso aconteceu.</summary>
public event System.Action<int, int> CheckpointPassed;
```

E ao final de `AdvanceCheckpoint`, imediatamente antes de cada `return` que não
seja o de `Finished`:

```csharp
CheckpointPassed?.Invoke(NextCheckpointIndex, CurrentLap);
```

Feito isso, eu troco o polling de `RaceHudController.RefreshPosition` por uma
assinatura do evento.

### 2. Evento de volta completada no `RacerProgress` *(baixa prioridade)*

**Arquivo:** o mesmo.

```csharp
/// <summary>Volta concluída, já com o número da volta nova.</summary>
public event System.Action<int> LapCompleted;
```

Disparar dentro do bloco `if (NextCheckpointIndex == 0)` de `AdvanceCheckpoint`,
logo depois de `CurrentLap++` e da checagem de `CurrentLap > totalLaps`.

Com isso o aviso de última volta deixa de depender de comparação por frame.

### 3. Áudio de conclusão de turno *(média prioridade)*

**Arquivo:** `Assets/_Project/Scripts/Audio/PuzzleAudioFeedback.cs`

A tela de conclusão é o momento mais forte do HUD e hoje sobe em silêncio. O
ideal é um som por linha do extrato, acompanhando a cascata de 0,085 s.

O gancho já existe: `PuzzleShiftReportPanel` anima cada linha com um atraso
conhecido. Se `PuzzleAudioFeedback` expuser algo como

```csharp
public void PlayStatementTick(int lineIndex);
```

eu chamo dentro de `PuzzleShiftReportPanel.AnimateStatement`, no mesmo
`HudFx.Delayed` que já agenda cada linha.

### 4. Pausa durante a tela de conclusão *(a decidir)*

**Arquivo:** `Assets/_Project/Scripts/Core/PauseService.cs`

Hoje o tabuleiro continua aceitando entrada enquanto a tela de conclusão está na
frente. Como `PuzzleBoardModel.TryMove` já recusa movimento com o tabuleiro
completo, isso não causa problema funcional — é só uma decisão de design se o
input deve ficar travado. Se for para travar, o ponto é `PuzzleHudController.OnCompleted`,
com a chamada correspondente em `OnInitialized`/`OnRestarted` para destravar.
