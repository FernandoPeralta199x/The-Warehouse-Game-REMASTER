# The Warehouse Nº 08 — Sistema de Narrativa

Estado anterior: `Scripts/Narrative/` tinha três arquivos que não estavam em cena
nenhuma e não guardavam texto — `NarrativeSequence` só tinha `speakerId` e
`localizationKey`. Era código morto.

Estado atual: sistema completo de cutscene, com roteiro real gerado em asset,
disparo por contexto de setor/fase, persistência entre sessões e auto-instalação
nas cenas de puzzle sem tocar nos construtores de cena.

---

## 1. Arquivos

### Runtime — `Assets/_Project/Scripts/Narrative/`

| Arquivo | Papel |
|---|---|
| `NarrativeContext.cs` | `NarrativeTriggerKind`, o struct `NarrativeContext` e `NarrativeMatching` (regra pura de casamento setor/fase). |
| `NarrativeSequence.cs` | **reescrito.** `NarrativeTone`, `NarrativeLine` (com texto real) e `NarrativeSequence` (identidade + gancho de disparo + falas). |
| `NarrativePlayback.cs` | Cursor de falas. Classe pura, sem Unity. |
| `NarrativeCatalog.cs` | Índice da campanha + elenco. Vive em `Resources` para autoconfiguração. |
| `NarrativeProgressStore.cs` | `PlayerPrefs` com prefixo `tw08.narrative.`. |
| `NarrativeService.cs` | **reescrito.** Estado da cutscene, fila de sequências, eventos, persistência. |
| `NarrativeOverlayController.cs` | O player animado. Monta o próprio Canvas em runtime. |
| `NarrativeDirector.cs` | Decide o que a cena diz + `NarrativeBootstrap` (auto-registro). |
| `NarrativeTrigger.cs` | **atualizado.** Gatilho por volume, para fala presa a um lugar. |

### Editor

- `Scripts/Editor/TW08NarrativeSetup.cs` — o roteiro em código, gerador dos assets.
  - Menu `Tools/TW08/Production/Build Narrative`
  - Menu `Tools/TW08/Production/Reset Narrative Progress` (QA)

### Testes

- `Tests/EditMode/NarrativeTests.cs` — 21 testes, namespace `TW08.Tests.EditMode`.

### Assets gerados

```
Assets/_Project/ScriptableObjects/Narrative/
├── Resources/
│   └── TW08_NarrativeCatalog.asset      ← carregado por Resources.Load
└── Sequences/
    ├── NARR_00_Abertura.asset
    ├── NARR_01_S01_Recebimento.asset
    ├── NARR_02_Duda_PrimeiraMensagem.asset
    ├── NARR_03_S02_Expedicao.asset
    ├── NARR_04_S03_CamaraFria.asset
    ├── NARR_05_S04_Automacao.asset
    ├── NARR_06_S05_ManutencaoPesada.asset
    ├── NARR_07_Robert_Confissao.asset
    ├── NARR_08_S06_RotasFantasma.asset
    └── NARR_09_Desfecho_Nucleo.asset
```

---

## 2. O roteiro

Fonte: `REFERENCIA/The_Warehouse_N8_Historia_Central.md`. As falas curtas que já
existem em `Docs/level-specs.json` são **pistas de fase** e continuam lá; as
sequências abaixo são **cenas** e não repetem aquele texto (exceto as três frases
âncora da história, que são de propósito as mesmas: a mensagem do painel, a
confissão do Robert e "esconderam nas rotas").

| Sequência | Disparo | Conteúdo |
|---|---|---|
| `narr-abertura` | `Opening` @ `TW08_Level01_FirstShift` | Chegada de John às 3h10. Sistema repetindo falha, Robert no rádio da Oficina. "Temos algo parecido com energia. Não recomendo elogiar." |
| `narr-setor-s01` | `SectorEntry` @ S01 | Recebimento. Caixas sem etiqueta e sem destino. John pergunta se o sistema estava remanejando ou procurando; Robert desconversa. |
| `narr-duda-primeira-mensagem` | `LevelCompleted` @ `TW08_Level01_FirstShift` | A virada do prólogo: "não confie no painel principal". O sistema descarta a mensagem na cara do John. |
| `narr-setor-s02` | `SectorEntry` @ S02 | Expedição. A insistência do sistema na doca B-12. John admite que passou a vida sem ler os terminais dela. |
| `narr-setor-s03` | `SectorEntry` @ S03 | Câmara Fria. O compressor rodando em setor vazio há seis meses — primeira evidência física de que existe coisa escondida. |
| `narr-setor-s04` | `SectorEntry` @ S04 | Automação. "Eficiência operacional: 99,4%" contra o armazém travado. Esteiras rodando vazias de madrugada. |
| `narr-setor-s05` | `SectorEntry` @ S05 | Manutenção Pesada. Robert está lá embaixo desde o lockdown — o sistema esqueceu de apagá-lo. |
| `narr-robert-confissao` | `LevelCompleted` @ `TW08_Level24_OldGenerator` | Virada do Ato 2. Robert abriu a porta para a Duda. Onze minutos depois o nome dela sumiu da escala. |
| `narr-setor-s06` | `SectorEntry` @ S06 | Rotas Fantasma. Terminal com origem apagada e operador inexistente. "Deixei nas rotas." |
| `narr-desfecho-nucleo` | `Ending` @ `TW08_Level30_LogisticsCore` | Final 1 — Modo Manual. John declara o armazém manual; o sistema não reconhece o comando. "Vai reconhecer." |

Total: **10 sequências, 73 falas.**

Tom seguido: humor seco, mistério industrial, emoção contida. Nenhuma fala
explica o que o jogador acabou de ver no tabuleiro.

---

## 3. Como funciona

### Modelo de dados

`NarrativeLine` carrega `speakerId`, `text` (TextArea, PT-BR), `tone`,
`charactersPerSecond` (0 = usa o padrão da sequência), `minimumDisplaySeconds` e
`voiceEventId` (reservado para o áudio).

`NarrativeSequence` carrega `sequenceId`, `title`, `playOnce`, o ritmo padrão, as
falas e o **gancho de disparo**: `trigger` + `sectorId` + `levelId`. Campo de
filtro vazio é curinga — uma sequência de `SectorEntry` com `sectorId = "S03"` e
`levelId` vazio entra na primeira fase daquele setor que o jogador abrir.

### Resolução

`NarrativeDirector` monta um `NarrativeContext` a partir do `PuzzleRuntime` da
cena (`level.SectorId`, `level.LevelId`) e pergunta ao catálogo. Quando duas
sequências casam, ganha a de maior `priority * 10 + especificidade`, onde filtro
de fase vale 2 e filtro de setor vale 1. Se a preferida já foi vista, a próxima
candidata assume — não fica buraco.

### Fila

Na fase 01 a abertura **e** a entrada do Setor 01 casam ao mesmo tempo. Sem fila,
uma das duas seria descartada em silêncio. O serviço enfileira e emenda: o
overlay não pisca entre elas porque ele só fecha quando `HasPending` é falso.

### Persistência

`playOnce` grava `tw08.narrative.<id>.played = 1` em `PlayerPrefs`, mesmo padrão
de `PuzzleProgressStore`. **`SaveGameData` não foi tocado** — narrativa não pode
forçar migração de save. `Tools/TW08/Production/Reset Narrative Progress` zera
tudo para playtest.

### Overlay

- Escurece a cena com `CanvasGroup` (fade 0.28s).
- Retrato vem de `CharacterProfile.Portrait` via `roster.Find(speakerId)`.
- Troca de falante: **cross-fade real** entre dois `Image` (o retrato anterior sai
  por cima enquanto o novo entra) + deslize vertical.
- Nome do falante entra com `SlideIn` + `Punch`, na cor `CharacterProfile.UiAccent`.
- Texto com `UIMotion.Typewriter`. O tom altera o ritmo: a automação digita 35%
  mais rápido, a memória gravada da Duda arrasta 18%.
- **Primeiro input completa a linha (`handle.Complete()`), o segundo avança.**
- `ESC` / botão leste do gamepad pula a cutscene inteira.
- Input por `UnityEngine.InputSystem` (`Keyboard.current`, `Mouse.current`,
  `Gamepad.current`), todos com guarda de null.

### Degradação

Sem catálogo o diretor se desabilita e não cria nada. Sem elenco, sem perfil ou
sem retrato, o overlay continua exibindo o texto com nome e cor de fallback
(`Sistema N-8`, `Terminal N-8` etc.). Sequência sem falas nunca é resolvida.
Linha em branco é pulada. Nenhum caminho lança por dado ausente.

---

## 4. Decisões

**Auto-instalação por `RuntimeInitializeOnLoadMethod` + `sceneLoaded`.**
As cenas de puzzle são geradas por `TW08PuzzleSceneBuilder`, que não é meu e não
pode depender de narrativa. `NarrativeBootstrap` assina `sceneLoaded` e cria um
`Narrative Director` em toda cena que tenha `PuzzleRuntime` — a menos que o
catálogo tenha `autoInstallInScenes = false`. Isso torna a integração no builder
**opcional**, não obrigatória.

**Catálogo em `Resources`.** É o que permite a autoinstalação sem wiring. A
pasta `ScriptableObjects/Narrative/Resources/` é minha, então nada fora do meu
escopo precisou mudar. O `CharacterRoster` entra como referência dentro do
catálogo, então o elenco carrega junto sem uma segunda busca.

**API tipada de autoria em vez de `SerializedProperty` por nome.**
`TW08ShopSetup` usa `SerializedObject` + `FindProperty("nome")`. Aqui o roteiro
tem arrays aninhados de classes serializáveis, e um nome de campo errado em
`FindProperty` só falha em **runtime**, com `NullReferenceException` dentro do
menu. Como este trabalho foi escrito sem rodar o Unity (o projeto está travado
por outros agentes), preferi `ConfigureAuthoring(...)` — guardado por
`#if UNITY_EDITOR` — que erra na **compilação**. O resto do padrão
(`LoadOrCreate<T>`, `EnsureFolder`, recarregar do `AssetDatabase` depois do
`Refresh`) foi mantido igual ao `TW08ShopSetup`.

**`timeScale = 0` durante a cutscene.** `UIMotion` já roda em tempo
não-escalado, então a animação continua. Como o Input System **não** respeita
`timeScale`, o overlay também desliga o componente `GameInput` da cena (e o
religa depois) — sem isso o jogador empurraria carga por trás do diálogo. O
`timeScale` anterior é restaurado, e também é restaurado em `OnDisable`, para
uma troca de cena no meio da cutscene não deixar o jogo congelado.

**Espera de 0,65s antes da cutscene de conclusão.** A última carga ainda está
deslizando quando `PuzzleRuntime.LevelCompleted` dispara. Entrar na cutscene no
mesmo frame congelaria a peça no meio do movimento, porque o overlay zera o
`timeScale` e a view do puzzle é escalada. O campo `completionDelaySeconds` do
`NarrativeDirector` controla isso.

**Fala do sistema não é personagem do roster.** `sistema` e `terminal` não
existem em `CharacterRoster` de propósito: são vozes sem rosto. O overlay resolve
nome e cor por fallback e simplesmente não mostra retrato.

---

## 5. Testes

`Tests/EditMode/NarrativeTests.cs` cobre lógica pura:

- **Playback**: pula linha vazia e nula, para no fim, `Rewind`, fonte nula.
- **Matching**: curinga, case/whitespace, rejeição por setor/fase/momento,
  `Manual` nunca dispara por contexto, peso da especificidade.
- **Catálogo**: prefere a sequência específica da fase, cai na do setor quando a
  preferida não é elegível, ignora entradas nulas e sequências sem falas, `Find`
  ignora caixa.
- **Serviço**: percorre todas as falas e completa, recusa segunda cutscene
  simultânea, fila emenda abertura → setor sem duplicar, `SkipAll`, entradas
  nulas/vazias sem exceção.
- **Persistência**: `playOnce` sobrevive à troca de instância do serviço; reset
  devolve a sequência à rotação; sequência repetível ignora a marca.

O `TearDown` limpa as chaves de `PlayerPrefs` criadas — sem isso a segunda
execução da suíte encontraria as sequências já marcadas como vistas.

---

## 6. INTEGRAÇÃO PENDENTE

### 6.1 Obrigatório — gerar os assets

O sistema não faz nada até o roteiro existir em disco. Rode uma vez:

```
Tools/TW08/Production/Build Narrative
```

Para deixar isso dentro do pipeline mestre, adicione **uma linha** em
`Assets/_Project/Scripts/Editor/TW08FullProductionExpansionSetup.cs`, dentro de
`BuildFullProductionExpansion()`, **logo depois** de
`TW08ExpansionDataSetup.EnsureAll(); AssetDatabase.SaveAssets();` (hoje linhas
43–44) e **antes** de `ReloadStableExpansionData()` (linha 46):

```csharp
EditorUtility.DisplayProgressBar("The Warehouse Nº 08", "Gerando narrativa...", 0.24f);
TW08NarrativeSetup.EnsureCatalog();
```

Ela precisa vir antes da linha 46 porque `EnsureCatalog` chama
`AssetDatabase.Refresh()`, e o `ReloadStableExpansionData()` seguinte já devolve
referências estáveis.

### 6.2 Opcional — wiring explícito no builder de puzzle

Sem isso o sistema funciona (auto-instalação). Com isso o diretor fica salvo na
cena, com o catálogo, o setor e a fase resolvidos em tempo de build — sem
`Resources.Load` e sem busca em runtime.

Em `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs`, método
`Build(...)`, **logo depois** de `CreateHud(level, runtime, nextScene);` (hoje
linha 219):

```csharp
NarrativeCatalog narrativeCatalog =
    AssetDatabase.LoadAssetAtPath<NarrativeCatalog>(TW08NarrativeSetup.CatalogPath);
if (narrativeCatalog != null)
{
    NarrativeDirector narrativeDirector =
        new GameObject("Narrative Director").AddComponent<NarrativeDirector>();
    narrativeDirector.Configure(narrativeCatalog, level.SectorId, level.LevelId);
}
```

Adicione `using TW08.Narrative;` no topo do arquivo. O `Configure` já marca o
componente e a cena como dirty. O catálogo é carregado ali dentro, e não passado
por parâmetro, pela mesma razão documentada em `TW08ShopSetup.BuildShopScene`:
qualquer `Refresh` anterior invalidaria o wrapper nativo.

Se fizer isso, considere desligar a autoinstalação para não ter os dois caminhos
ativos: marque `autoInstallInScenes = false` no
`TW08_NarrativeCatalog.asset` (ou troque o `true` final da chamada
`catalog.ConfigureAuthoring(roster, sequences, true)` em `TW08NarrativeSetup`).
Não é um bug ter os dois — o bootstrap não instala nada se já houver um
`NarrativeDirector` na cena — mas é ruído a menos.

### 6.3 Opcional — áudio

`NarrativeLine.VoiceEventId` existe e está vazio em todas as falas. Quando o
sistema de áudio tiver eventos de diálogo, o gancho é em
`NarrativeOverlayController.ShowLine(...)`: disparar o evento junto com o
`Typewriter`. Não fiz porque `Scripts/Audio/**` não é meu.

### 6.4 Nada além disso

Não editei `Scripts/UI/**`, `Scripts/Motion/**`, `Scripts/Puzzle/**`,
`Scripts/Save/**`, `Scripts/Economy/**`, nem nenhum dos builders de cena.
`SaveGameData` não foi tocado.
