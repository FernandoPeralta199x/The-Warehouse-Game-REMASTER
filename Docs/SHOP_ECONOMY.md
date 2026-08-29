# Oficina N-8 — Economia e Ferramentas

Implementação do documento de referência `REFERENCIA/The_Warehouse_N8_Loja_PowerUps.md`.

## Moeda

**Créditos de Turno**, campo `credits` em `SaveGameData`. Só sobe ao concluir uma fase.

## Ganhos por turno (`ShiftCredits`)

| Fonte | Valor |
|---|---|
| Turno concluído | +100 |
| Medalha bronze / ouro / platina | +25 / +50 / +100 |
| Sem ferramentas | +50 |
| Sem dicas | +50 |
| Novo recorde pessoal | +75 |
| Primeira tentativa | +50 |

**Teto de 250 créditos por fase.** A bíblia de design fixa a faixa de 100–250 por fase
comum e trata como requisito que o jogador não compre a loja inteira cedo demais. Sem o
teto, um turno perfeito pagaria 425.

A escala de medalhas do jogo é bronze/ouro/platina (1/2/3) e recebe a curva 25/50/100 da
tabela original, que falava em bronze/prata/ouro.

**Fora do escopo por falta de dados:** o bônus "abaixo do limite de empurrões" precisa de
um limite de empurrões por fase, que os `PuzzleLevelDefinition` não têm — só limites de
movimento (ouro e platina), já cobertos pelas medalhas.

## Ranking limpo x assistido

`PuzzleRunSummary.IsClean` é verdadeiro quando o turno não usou ferramenta nem dica.

- **Limpo** → grava `bestCleanMoves` e `cleanMedal`; é o único que vale para ranking competitivo.
- **Assistido** → grava só `bestMoves`/`medal`, conclui a campanha e desbloqueia fases normalmente.

A HUD avisa em tempo real (`TURNO LIMPO // RANKING ATIVO` ou `MODO ASSISTIDO // FORA DO RANKING`),
como exige a regra de interface do documento.

## Ferramentas do MVP

Só entram ferramentas que ajudam o jogador a pensar. Força Hidráulica, Macaco N-8,
Chave Mestra e afins ficaram de fora — a própria bíblia as exclui do MVP por alterarem as
regras da fase.

| Ferramenta | Raridade | Preço | Usos/fase | Efeito |
|---|---|---|---|---|
| Rebobinar Movimento | Comum | 50 | 1 | Desfaz 3 movimentos |
| Marcador de Rota | Comum | 40 | 2 | Destaca alvos descobertos |
| Scanner Logístico | Incomum | 80 | 1 | Aponta a carga mais crítica |
| Assistente de Turno | Raro | 150 | 3 | Dicas em 3 camadas |

**Slots por turno: 2.** Comprar e equipar são passos separados — ter no estoque não coloca
em campo.

### Como o conselho é gerado (`PuzzleAdvisor`)

Puro e determinístico, sem resolver a fase:

- **Carga crítica**: primeiro as travadas em canto de parede, depois a mais distante de um
  alvo livre. Empates resolvem por posição para o destaque não oscilar entre frames.
- **Dica 1**: região do setor onde está o problema.
- **Dica 2**: em que direção levar a carga.
- **Dica 3**: primeiro passo de uma rota livre até a carga (BFS). Continua sem entregar a
  sequência da solução.

## Bloqueio por fase

`PuzzleLevelDefinition.allowPowerUps` — antes deste sistema o campo existia mas ninguém o
lia. Agora `PuzzleToolService` recusa qualquer ferramenta em fases marcadas como bloqueadas,
que é o controle do level designer previsto no documento.

O estoque só é debitado depois que a ferramenta confirma efeito: acionar o Marcador de Rota
sem alvos descobertos não gasta a unidade.

## Save

Schema **v3**. `SaveMigrationV2ToV3` promove os recordes existentes a recordes limpos —
saves v2 foram jogados antes de existirem ferramentas, então todo resultado antigo é
necessariamente limpo.

## Onde fica no código

| Camada | Arquivos |
|---|---|
| Domínio (puro, testável) | `Scripts/Economy/ShiftCredits.cs`, `PuzzleRunSummary.cs`, `PuzzleAdvisor.cs` |
| Dados | `PuzzleToolDefinition.cs`, `PuzzleToolCatalog.cs` |
| Runtime | `PuzzleToolService.cs`, contadores em `PuzzleRuntime.cs` |
| Persistência | `SaveGameData.cs`, `SaveMigrationV2ToV3.cs`, `SaveManager.CommitPuzzleShift` |
| UI | `UI/ShopController.cs`, `UI/PuzzleToolBarController.cs` |
| Geração | `Editor/TW08ShopSetup.cs` (assets + cena `TW08_ShopN8`) |

Testes em `Tests/EditMode/ShopEconomyTests.cs` e `PuzzleShiftTrackingTests.cs`.
