# Direção de Gameplay — diagnóstico crítico

Auditoria de **design de jogo** do estado atual de *The Warehouse Nº 08*, medida com o
solver (`Tools/puzzle/tw08_solver.py`) sobre os 27 layouts de `Docs/level-layouts.json`
**e** sobre os 9 `PuzzleLevelDefinition` das fases 01–09, que não estão no arquivo de
layouts e por isso nunca entraram em nenhum relatório anterior.

Nenhum arquivo do projeto foi alterado além deste. Nenhuma cena foi aberta.

> **Unidade.** Todo número de "custo" é o `MoveCount` do motor: piso `~` custa 2,
> os demais custam 1. Não é contagem de passos. Numa fase como a 08 o jogador dá
> 12 passos e o contador mostra 22.

---

## 1. Resumo executivo — o que importa, em ordem

| # | Achado | Gravidade | Evidência em uma linha |
|---|---|---|---|
| 1 | **A caixa-ferramenta nunca acontece.** Em 19 fases com sensor+porta, os 27 sensores são *também* alvos. Nenhuma carga é estacionada num sensor e depois retirada. | Crítica | `sensorLiberado = false` em 19/19; remover a porta muda o ótimo em 1 fase de 19 (a L14) |
| 2 | **20 das 27 fases reutilizam apenas 7 cascas de sala.** A L19 e a L26 são a mesma sala espelhada, mesmo ótimo (48), mesmo número de empurrões (22). | Crítica | assinatura de paredes idêntica em 7 grupos |
| 3 | **A fase 03 é a mais difícil do primeiro terço do jogo**, empatada com a fase 14. Custo 31 / 19 empurrões — mais empurrões que a L15, a L18, a L21 e a S02. Depois dela a 04 cai para 8. | Alta | 12 → **31** → 8 nas posições 2/3/4 |
| 4 | **O teto de 250 créditos apaga a diferença entre jogar bem e jogar mal.** Bronze e platina pagam exatamente o mesmo numa primeira zerada limpa. | Alta | 350 e 425 brutos, ambos truncados em 250 |
| 5 | **A loja inteira custa 320 créditos e o jogador tem ~2.250 quando ela abre.** Ferramentas são bloqueadas nas fases 01–09; ao chegar na primeira fase que as permite, dá para comprar tudo 7 vezes. | Alta | 40+50+80+150 = 320; 9 fases × teto 250 |
| 6 | **A carga pesada (`h`) não tem regra própria.** Em `PuzzleBoardModel` ela se move igual a uma caixa normal; o único efeito é o `goalRequirement`. Trocá-la por caixa comum não muda o ótimo em 11 das 12 fases. | Alta | `SEM-PESADA` = ótimo em L07, L21, L24, L25, L29, L30, S02, S04, S08, S09 |
| 7 | **Platina = ótimo exato + undo ilimitado e gratuito.** `TryUndo` devolve o custo integralmente e nada marca o turno. Platina virou teste de paciência. | Alta | `MoveCount -= move.MoveCost` sem penalidade |
| 8 | **As secretas são mais fáceis que o último setor principal.** Média 47,1 contra 50,8. A S10 (dificuldade "Extrema") custa 46 — menos que a S05, a S02 e a S07. | Média | tabela §2.3 |
| 9 | **A L17 é 90% caminhada.** Custo 20 com **2 empurrões**: 18 comandos são ida e volta até o botão. | Média | push/comando = 0,10; o segundo pior do jogo é 0,28 |
| 10 | **A campanha é 27 puzzles seguidos.** As fases 10, 13 e 23 (corridas) e a S06 não existem no `TW08_PuzzleCampaign`. A alternância de ritmo que a bíblia exige simplesmente não acontece. | Média | `sceneName` pula de 09 para 11, de 22 para 24 |

### O que está bom, e é bom de verdade

- **A prova de solvabilidade é o maior ativo do projeto.** Ter as 36 fases com ótimo
  provado, replay validado e o solver espelhando o motor linha a linha é raro e
  vale mais que qualquer fase individual. Todo este documento só foi possível por
  causa disso. Não largue essa disciplina.
- **Gelo e esteira estão certos.** São as únicas mecânicas que passam no teste de
  ablação em todas as fases onde aparecem: sem gelo a L11 sobe de 10 para 16, a L15
  de 39 para 43, a S10 de 46 para 51; sem esteira a L16 sobe de 7 para 13 e a L20
  de 46 para 53. Elas mudam a solução, não só a decoração.
- **A L16 é a melhor fase-tutorial do jogo.** Custo 7, 2 empurrões, e a esteira é
  obrigatória (13 sem ela). Ensina em 7 movimentos e não mente.
- **O botão de direção da L17 foi consertado direito.** Sem ele a fase é insolúvel
  — é o único gimmick com dependência dura comprovada.
- **A macro-curva por setor está correta.** 17,4 → 26,3 → 31,4 → 42,5 → 50,8. O
  problema é a micro-curva dentro de cada setor, não o arco geral.
- **A política de ouro (`max(ótimo+2, ⌈ótimo×1,3⌉)`) é mais justa do que parece.**
  Ver §5 — a folga por empurrão fica entre 0,53 e 0,91 em 32 das 36 fases. O
  problema das medalhas está na platina, não no ouro.
- **`allowPowerUps` é lido de verdade** e está distribuído com critério: bloqueado
  no tutorial (01–09) e no clímax (25, 27–30, S07–S10).

---

## 2. Curva de dificuldade

### 2.1 Campanha principal, na ordem real de `TW08_PuzzleCampaign`

| Pos | Fase | Custo ótimo | Empurrões | Δ do anterior |
|---:|---|---:|---:|---:|
| 1 | L01 Primeiro Turno | **3** | 2 | — |
| 2 | L02 Corredor Apertado | 12 | 7 | +9 |
| 3 | L03 Carga Cruzada | **31** | **19** | **+19** |
| 4 | L04 Sensor Split | **8** | 5 | **−23** |
| 5 | L05 Tight Lift | 20 | 9 | +12 |
| 6 | L06 Terminal Route | 13 | 7 | −7 |
| 7 | L07 Dock Sync | 20 | 9 | +7 |
| 8 | L08 Cold Storage | 22 | 8 | +2 |
| 9 | L09 Cross Dispatch | 28 | 11 | +6 |
| 10 | L11 Piso Gelado | **10** | 4 | **−18** |
| 11 | L12 Frio no Corredor | 25 | 7 | +15 |
| 12 | L14 Sensor Congelado | 31 | 11 | +6 |
| 13 | L15 Câmara 08-C | 39 | 18 | +8 |
| 14 | L16 Esteira Ligada | **7** | 2 | **−32** |
| 15 | L17 Rota Automática | 20 | **2** | +13 |
| 16 | L18 Robô de Limpeza | 36 | 17 | **+16** |
| 17 | L19 Sensor Falso | 48 | 22 | +12 |
| 18 | L20 Linha de Produção | 46 | 20 | −2 |
| 19 | L21 Oficina Travada | 47 | 18 | +1 |
| 20 | L22 Macaco N-8 | 33 | 17 | **−14** |
| 21 | L24 Gerador Antigo | 44 | 20 | +11 |
| 22 | L25 Peso Morto | 46 | 21 | +2 |
| 23 | L26 Arquivo Morto | 48 | 22 | +2 |
| 24 | L27 Rota Fantasma | **57** | 26 | +9 |
| 25 | L28 Carga Sem Origem | 45 | 19 | **−12** |
| 26 | L29 Lockdown N-8 | 48 | 22 | +3 |
| 27 | L30 Núcleo Logístico | 56 | 25 | +8 |

### 2.2 Os quatro saltos que quebram a leitura

**a) Posição 3 — a Carga Cruzada é uma fase de meio de jogo no lugar de uma fase
de tutorial.** Ela salta de 12 para 31 e devolve para 8 na fase seguinte. Não é
uma questão de "parecer difícil": com 19 empurrões ela tem *mais* empurrões que a
L15 Câmara 08-C (18), a L21 Oficina Travada (18), a S02 Sala do Robert (18) e a
L18 Robô de Limpeza (17). O terceiro nível do jogo pede mais planejamento que a
fase que a bíblia chama de "primeira fase realmente cerebral". Correção proposta
em §7.1.

**b) Posição 14 — a Esteira Ligada desmonta o setor da câmara fria.** A Câmara
08-C fecha o Setor 03 com custo 39 e a fase seguinte custa 7. Uma queda de 82%.
O problema não é a L16 ser fácil — ela é um tutorial excelente — é ela ser
apresentada como se fosse uma fase de setor. A bíblia prevê exatamente isso ("Fase
16 — Esteira Ligada, Média/Difícil, objetivo: introduzir esteiras"), mas o jogo não
tem nenhum sinal de leitura para o jogador ("novo mecanismo detectado") nem uma
fase de corrida entre os dois setores para absorver a queda. Aqui o buraco da
corrida da fase 13 é sentido em cheio.

**c) Posição 20 — a Macaco N-8 é um degrau para baixo de 14 no meio do Setor 05.**
47 → 33 → 44. É a única fase do Setor 05 que não tem gimmick nenhum (a ablação
devolveu `{}`: sem gelo, esteira, sensor, piso custoso, pesada, robô, botão ou
portão). É uma fase de Sokoban puro com o nome de uma ferramenta que a própria
bíblia excluiu do MVP e que não existe no jogo.

**d) Posição 25 — a Carga Sem Origem esvazia o clímax.** A sequência final é
48 → 57 → **45** → 48 → 56. A fase 27 (Rota Fantasma) é a mais difícil da campanha
e é seguida pela mais fácil das cinco últimas. O arco de fechamento deveria subir
monotonicamente; hoje ele oscila.

### 2.3 Arco secreto — está invertido

| Ordem | Fase | Custo | Empurrões |
|---:|---|---:|---:|
| 1 | S01 Caixa Fora do Registro | 43 | 21 |
| 2 | S02 Sala do Robert | 48 | 18 |
| 3 | S03 Turno da Duda | 46 | 20 |
| 4 | S04 Rota do Elias | 44 | 21 |
| 5 | S05 Oficina Sem Luz | 50 | 24 |
| 6 | S07 08-B | **66** | 26 |
| 7 | S08 O Mapa que Sobrou | **41** | 21 |
| 8 | S09 Último Turno do Elias | **40** | 19 |
| 9 | S10 O Caminho da Duda | 46 | 21 |

Média 47,1 — **abaixo** da média 50,8 do Setor 06 principal. As secretas, que a
bíblia define como "testar domínio real", são em média mais fáceis que as fases
que o jogador já venceu para desbloqueá-las.

Pior: a **S10 O Caminho da Duda** está catalogada como dificuldade *Extrema*, é o
desbloqueio final de toda a campanha, e custa 46 — abaixo da S02 (48), da S05 (50)
e 30% abaixo da S07 (66). O último quarto do arco secreto (41, 40, 46) é o mais
fácil de todos.

**Ordem que a curva medida pede:** S09 (40) → S08 (41) → S01 (43) → S04 (44) →
S03 (46) → S02 (48) → S05 (50) → S10 (?) → S07 (66) como penúltima, com a S10
reconstruída para custar acima de 66. Isso exige remapear os desbloqueios
narrativos — mas os desbloqueios atuais (`unlockedByDefault: 0` em cascata) são
puramente sequenciais, então o custo de remapear é baixo.

---

## 3. Introdução de mecânicas

### 3.1 Onde cada mecânica aparece pela primeira vez

| Mecânica | 1ª aparição | Custo dessa fase | É simples? | Reaparece em |
|---|---|---:|---|---|
| Caixa + alvo | L01 (pos 1) | 3 | ✅ sim | todas |
| Sensor + porta | **L04** (pos 4) | 8 | ⚠️ sim, mas a porta é decorativa | 19 fases |
| Carga pesada `h` + carga frágil | **L07** (pos 7) | 20 | ❌ as duas estreiam juntas, sem ensino e sem regra própria | pesada em 12 fases, frágil em 2 |
| Piso custoso `~` | **L08** (pos 8) | 22 | ❌ o contador pula de 2 em 2 sem explicação | 15 fases |
| Gelo `%` | L11 (pos 10) | 10 | ✅ excelente | 5 fases |
| Esteira | L16 (pos 14) | 7 | ✅ excelente | 3 fases |
| Botão de direção | **L17** (pos 15) | 20 | ❌ sem tutorial, obrigatório | **1 fase** |
| Robô por turno | **L18** (pos 16) | 36 | ❌ 3 cargas, 17 empurrões, sem tutorial | **1 fase** |
| Parede falsa | L26 (pos 23) | 48 | ❌ | 2 fases |
| Névoa (Memory) | L26 (pos 23) | 48 | ❌ | 2 fases |
| Névoa (Lanterna) | S05 secreta | 50 | ❌ só na secreta | 1 fase |
| Portão temporizado | S08 secreta | 41 | ❌ só na secreta | **1 fase** |

### 3.2 As três falhas de progressão

**a) Cinco mecânicas aparecem em 1 ou 2 fases e nunca são combinadas.** Botão de
direção (1), robô (1), portão temporizado (1), parede falsa (2), névoa (3). A
bíblia é explícita: *"ensinar uma ideia; testar uma ideia; combinar duas ideias"*.
Hoje o ciclo para no "ensinar" — e em três casos nem isso, porque a primeira
aparição já é a versão difícil. O robô estreia numa fase de 36 de custo e 17
empurrões; o portão temporizado estreia numa **fase secreta**, ou seja, num
conteúdo opcional que a maior parte dos jogadores nunca vai ver.

**b) O sensor de peso é apresentado como decoração e nunca é ensinado como
ferramenta.** A bíblia dedica a fase 06 inteira a isso ("1 caixa precisa ficar
temporariamente no sensor", "o jogador precisa aceitar que uma caixa não vai
direto ao alvo"). No jogo, a fase 06 se chama *Terminal Route*, tem dois sensores
que já são alvos, e uma porta em (5,3) que dá para contornar — trancá-la
permanentemente ainda deixa a fase solúvel a custo 22 contra 13 do ótimo. Ou seja:
o jogador que nunca entender o que o sensor faz **termina a fase mesmo assim**.

**c) A carga pesada estreia sem existir.** Na L07 Dock Sync a caixa `h` fica no
meio de três corredores idênticos. A solução ótima da L07 é
`RUUUDDDRRUUUDDDRRUUU` — **exatamente a mesma string da L05**, o mesmo motivo de
5 movimentos repetido três vezes. Trocar a pesada por uma caixa comum não muda o
ótimo (20 → 20). O jogador vê um sprite diferente, executa o mesmo gesto, e nada
acontece.

### 3.3 Divergência entre a campanha construída e a bíblia

As nove primeiras fases do jogo são um *vertical slice* legado que não corresponde
ao documento de level design em nome, tema nem função:

| Bíblia | Jogo | Setor na bíblia | Setor no asset |
|---|---|---|---|
| 03 Carga Cruzada | `Level03_CrossLoad` | S01 | S01 ✅ |
| 04 Etiqueta Errada (caixa marcada, terminal, Duda) | `Level04_SensorSplit` | S01 | **S02** |
| 05 Doca Inicial (duas rotas, deadlock leve) | `Level05_TightLift` | S01 | **S02** |
| 06 **Portão de Peso** (tutorial de sensor) | `Level06_TerminalRoute` | S02 | S02 |
| 07 Ordem de Saída (fila, ordem inversa) | `Level07_DockSync` | S02 | **S03** |
| 08 Doca B-12 (rota falsa narrativa) | `Level08_ColdStorage` | S02 | **S03** |
| 09 Três Paletes (área de manobra) | `Level09_CrossDispatch` | S02 | **S03** |

Consequência prática: **o Setor 03 (Câmara Fria) começa na fase 7 e vai até a 13**
— sete fases, 26% da campanha, num setor que a bíblia dimensiona em cinco. E o
Setor 01 e o 02 ficaram com três fases cada. A primeira pista narrativa da Duda
(fase 04) e a primeira rota falsa do sistema (fase 08) não existem no jogo.

---

## 4. Gimmicks decorativos

Método: para cada fase, removi cada mecânica isoladamente e recalculei o ótimo com
o solver. Se o ótimo não muda, a mecânica não aparece na solução ótima. Para
sensor/porta rodei também uma variante *porta trancada* (sensores removidos,
portas viradas parede), que separa "porta que não custa nada" de "porta que não
serve para nada". E instrumentei o replay da solução ótima para ver se alguma
carga entra num sensor e depois sai.

### 4.1 O achado principal: a caixa-ferramenta não existe no jogo

| Métrica | Resultado |
|---|---|
| Fases com sensor+porta | **19** (L04, L06, L09, L14, L15, L19, L20, L24, L25, L26, L28, L29, L30, S03, S04, S07, S08, S09, S10) |
| Células de sensor no jogo | **27** |
| Sensores que **também são alvo** | **27 de 27 (100%)** |
| Fases em que a carga entra num sensor e depois sai | **0 de 19** |
| Fases em que remover a porta muda o ótimo | **1 de 19** (só a L14: 31 → 25) |

Isso é o mesmo sintoma que você já identificou no robô da L18 e no botão da L17,
mas em escala muito maior. Como o sensor é sempre um alvo, o jogador entrega a
carga ali porque *tem* que entregar; a porta abre de graça, como efeito colateral,
e nunca fecha de volta porque nada precisa sair. As fases 06, 14 e 25 — as três que
a bíblia constrói inteiras em cima da caixa-ferramenta — não têm caixa-ferramenta.

Nas fases 04, 06 e 09 é ainda mais frouxo: a porta está no meio de uma sala aberta
e dá para contorná-la.

| Fase | Ótimo | Sem porta | Porta trancada |
|---|---:|---:|---:|
| L04 Sensor Split | 8 | 8 | **16** (ainda solúvel) |
| L06 Terminal Route | 13 | 13 | **22** (ainda solúvel) |
| L09 Cross Dispatch | 28 | 28 | **42** (ainda solúvel) |

Da L14 em diante a porta é um bloqueio real (trancada = impossível), mas continua
custando zero: o jogador nunca precisa planejar em torno dela.

### 4.2 Nenhum tipo de carga tem regra própria

`PuzzleBoardModel.TryMove` trata `HeavyCrate` e `FragileCrate` exatamente como
`Crate`. Não há custo extra, não há empurrão duplo, não há bloqueio, não há dano.
O único efeito de qualquer `PuzzleEntityKind` é o `goalRequirement`, que é uma
restrição de *pareamento* (esta carga vai naquele alvo), não de peso nem de
fragilidade.

A **L07 Dock Sync** é o caso mais nu: três cargas — `crate-standard` (kind 1),
`crate-heavy` (kind 2) e `crate-fragile` (kind 3) — cada uma travada no seu alvo
por `requiredKind`, em três corredores verticais idênticos. A solução ótima é
`RUUUDDDRRUUUDDDRRUUU`: o mesmo motivo de 5 movimentos, três vezes. É um puzzle de
cores vestido de puzzle de física. E é **exatamente a mesma string de comandos da
L05**, duas fases antes.

Substituindo `h` por `$` e removendo os `goalRequirements`:

| Fase | Ótimo | Sem pesada | Δ |
|---|---:|---:|---:|
| L07 Dock Sync | 20 | 20 | **0** |
| L21 Oficina Travada | 47 | 47 | **0** |
| L24 Gerador Antigo | 44 | 44 | **0** |
| L25 Peso Morto | 46 | 46 | **0** |
| L29 Lockdown N-8 | 48 | 48 | **0** |
| L30 Núcleo Logístico | 56 | 56 | **0** |
| S02 Sala do Robert | 48 | 48 | **0** |
| S04 Rota do Elias | 44 | 44 | **0** |
| S08 O Mapa que Sobrou | 41 | 41 | **0** |
| S09 Último Turno do Elias | 40 | 40 | **0** |
| S10 O Caminho da Duda | 46 | 45 | −1 |

Onze fases de doze onde a "carga pesada" — o tema inteiro do Setor 05 Manutenção
Pesada — é intercambiável por uma caixa comum. Vale registrar que a L09 também
tem `h` e dá 28 → 28.

### 4.3 O piso custoso está fazendo o trabalho que os puzzles deviam fazer

O `~` não é decorativo — ele muda o ótimo em todas as fases. Mas ele muda por
**pedágio**, não por raciocínio: são movimentos que custam 2 num caminho que o
jogador seguiria de qualquer forma.

| Fase | Ótimo | Sem piso `~` | Quanto do custo é pedágio |
|---|---:|---:|---:|
| S07 08-B | 66 | 53 | **20%** |
| L21 Oficina Travada | 47 | 35 | **26%** |
| S02 Sala do Robert | 48 | 36 | **25%** |
| L08 Cold Storage | 22 | 12 | **45%** |
| L09 Cross Dispatch | 28 | 20 | **29%** |
| L29 Lockdown N-8 | 48 | 43 | 10% |
| L19 / L26 | 48 | 44 | 8% |

Na **L08 Cold Storage**, 45% do custo é pedágio: o jogador dá 12 passos e o
contador marca 22. A fase é a introdução do `~` no jogo e ninguém explica por que
o número pula de dois em dois — o HUD só diz `MOVIMENTOS 022`
(`HudFormat.MovesValueFormat`). Na S07 08-B — a fase mais difícil do jogo, cuja
etiqueta é `precision` — um quinto do custo é caminhar em cima de azulejo caro.

Isso é dificuldade artificial. Ela infla o ótimo (e portanto os limites de medalha)
sem exigir nenhuma decisão a mais.

### 4.4 Gimmicks que passam no teste

| Gimmick | Fase | Ótimo | Ablado | Veredito |
|---|---|---:|---:|---|
| Gelo | L11 | 10 | 16 | ✅ carrega a fase |
| Gelo | L12 | 25 | 29 | ✅ |
| Gelo | L15 | 39 | 43 | ✅ |
| Gelo | S10 | 46 | 51 | ✅ |
| Esteira | L16 | 7 | 13 | ✅ carrega a fase |
| Esteira | L20 | 46 | 53 | ✅ |
| Botão de direção | L17 | 20 | **insolúvel** | ✅ dependência dura |
| Robô | L18 | 36 | 34 | ⚠️ +2, no limite |
| Portão temporizado | S08 | 41 | 39 | ⚠️ +2, no limite |
| Porta (sensor) | L14 | 31 | 25 | ✅ única do jogo |

Gelo e esteira estão sólidos. O robô e o portão temporizado ficaram exatamente no
limiar de "conta" — +2 de custo cada. Sobrevivem, mas não sustentam uma fase
inteira; ambos precisam de uma segunda aparição que os combine com outra coisa.

### 4.5 Duplicação de layout

Vinte das 27 fases reutilizam sete cascas de sala (mesmas paredes, exatamente):

| Casca | Fases | Custos |
|---|---|---|
| A | **L25, L29, S04, S08, S09** | 46, 48, 44, 41, 40 |
| B | L15, L28, S03 | 39, 45, 46 |
| C | L19, L26 | 48, **48** |
| D | L20, S07 | 46, 66 |
| E | L21, S02 | 47, 48 |
| F | L22, S01 | 33, 43 |
| G | L30, S10 | 56, 46 |

O caso mais grave é a **casca C**: a L19 *Sensor Falso* e a L26 *Arquivo Morto* são
a **mesma sala espelhada verticalmente**. Mesmo ótimo (48), mesmos 22 empurrões,
mesmos 44 comandos, soluções que são o espelho uma da outra. São a fase 17 e a
fase 23 da campanha — o jogador resolve o mesmo puzzle duas vezes com seis fases
de intervalo, uma vez chamada "o sistema mente" e outra chamada "os arquivos foram
apagados".

O segundo caso mais grave é a **casca A**: cinco fases, incluindo o clímax
narrativo do Setor 06 (Lockdown N-8) e três das nove secretas. A "Sala do Robert"
e a "Rota do Elias" — dois momentos narrativos distintos, dois personagens
diferentes — são a mesma planta com as cargas trocadas de lugar.

---

## 5. Balanceamento das medalhas

A política é `platina = ótimo` e `ouro = max(ótimo+2, ⌈ótimo×1,3⌉)`.

### 5.1 O ouro está mais justo do que parece

Sua preocupação era a folga absoluta: 18 de folga numa fase de 60 contra 3 numa
fase de 7. Mas a métrica que importa não é a folga bruta — é **quanto erro a folga
tolera**, e erro em Sokoban se mede em decisões, não em comprimento.

| Fase | Ótimo | Empurrões | Folga do ouro | Folga / empurrão |
|---|---:|---:|---:|---:|
| L17 Rota Automática | 20 | 2 | 6 | **3,00** |
| L16 Esteira Ligada | 7 | 2 | 3 | **1,50** |
| L12 Frio no Corredor | 25 | 7 | 8 | 1,14 |
| L01 Primeiro Turno | 3 | 2 | 2 | 1,00 |
| L14 Sensor Congelado | 31 | 11 | 10 | 0,91 |
| *(mediana das 36 fases)* | — | — | — | **0,68** |
| L27 Rota Fantasma | 57 | 26 | 18 | 0,69 |
| L30 Núcleo Logístico | 56 | 25 | 17 | 0,68 |
| S07 08-B | 66 | 26 | 20 | 0,77 |
| L22 Macaco N-8 | 33 | 17 | 10 | 0,59 |
| L02 Corredor Apertado | 12 | 7 | 4 | 0,57 |
| L03 Carga Cruzada | 31 | 19 | 10 | **0,53** |

Trinta e duas das 36 fases caem entre 0,53 e 0,91. A regra dos 30% já normaliza
sozinha: fases longas têm mais empurrões, e a folga escala junto. **Não mexa no
ouro.** Os únicos desvios reais estão nas fases curtas com poucos empurrões — a
L17 (3,00) e a L16 (1,50) — e ali a folga generosa é apropriada para tutoriais.

O que sobra da sua intuição, e é verdadeiro: nas fases mais **densas** (L03 com
0,53, L02 com 0,57, L22 com 0,59) o ouro é apertado. Mas isso é consequência de
elas serem densas demais para o slot que ocupam, não de a fórmula estar errada.

### 5.2 A platina é o problema

Duas razões, ambas estruturais:

**a) Platina = ótimo exato significa igualar um resultado de busca A\*.** Na S07
08-B o solver expandiu 97.883 estados para achar a sequência de 66. Numa fase como
a L30 foram 1,7 milhão de estados. Pedir ao jogador que reproduza isso não é um
desafio de habilidade — é um desafio de força bruta.

**b) O undo é ilimitado, gratuito, e não marca o turno.**

```csharp
// PuzzleBoardModel.TryUndo
MoveCount = Math.Max(0, MoveCount - Math.Max(1, move.MoveCost));
CommandCount = Math.Max(0, CommandCount - 1);
```

O custo é devolvido integralmente e `PuzzleRunSummary` nem tem campo de undo:
`IsClean => ToolsUsed <= 0 && HintsUsed <= 0`. Ou seja, o jogador pode desfazer
mil vezes até tropeçar no ótimo, e o turno continua "limpo" e vale platina.

A bíblia previa exatamente o contrário — *"Platina: concluir abaixo de 14
movimentos **sem undo**"* na fase 01, *"Platina: sem Power Ups, sem colisão"* na
29, *"Medalha perfeita: sem Power Ups, sem dicas, sem undo extra"* no documento da
loja. O requisito de undo foi perdido na implementação.

**Recomendação (em ordem de esforço):**

1. Adicionar `UndosUsed` ao `PuzzleRunSummary` e exigir `UndosUsed == 0` para a
   platina. É a correção mais barata e restaura a intenção da bíblia. O bronze e
   o ouro continuam permitindo undo à vontade — o jogador casual não sente nada.
2. Alternativamente, afrouxar a platina para `ótimo + ⌈ótimo×0,05⌉` nas fases com
   mais de 20 empurrões, mantendo o ótimo exato como um quarto tier ("Turno
   Perfeito") separado da progressão.

Não faça as duas ao mesmo tempo.

---

## 6. Economia

### 6.1 O teto de 250 apaga o incentivo

`ShiftCredits.Evaluate` soma o extrato e trunca em `MaxPerLevel = 250`. O extrato
possível é: 100 (concluir) + 25/50/100 (medalha) + 50 (sem ferramentas) + 50 (sem
dicas) + 75 (recorde) + 50 (primeira tentativa).

| Cenário | Bruto | Pago |
|---|---:|---:|
| 1ª tentativa, limpo, **bronze** | 100+25+50+50+75+50 = **350** | **250** |
| 1ª tentativa, limpo, **ouro** | 100+50+50+50+75+50 = **375** | **250** |
| 1ª tentativa, limpo, **platina** | 100+100+50+50+75+50 = **425** | **250** |
| Repetindo, limpo, sem recorde, bronze | 100+25+50+50 = 225 | 225 |
| Repetindo, limpo, sem recorde, ouro | 100+50+50+50 = 250 | 250 |

**Numa primeira zerada limpa, jogar perfeitamente e jogar mal pagam exatamente a
mesma coisa.** A diferença entre 350 e 425 brutos é inteiramente comida pelo teto.
O único cenário em que a medalha muda o dinheiro é o replay de uma fase já vencida
sem bater o recorde — que é justamente quando o jogador tem menos motivo para
jogar.

A bíblia dizia *"Fase comum: 100 a 250 créditos possíveis"* — uma **faixa**. A
implementação colapsou a faixa num ponto.

**Correção:** baixar o teto para ~180, ou tirar do teto as linhas que representam
mérito (medalha, recorde) e aplicá-lo só sobre as linhas de participação. Com teto
180 sobre as linhas de participação e a medalha por fora, a diferença bronze →
platina vira 75 créditos reais.

### 6.2 A loja é irrelevante por um fator de sete

| Ferramenta | Preço |
|---|---:|
| Marcador de Rota | 40 |
| Rebobinar Movimento | 50 |
| Scanner Logístico | 80 |
| Assistente de Turno | 150 |
| **Catálogo inteiro** | **320** |

`allowPowerUps: 0` está marcado nas fases 01 a 09. A primeira fase em que uma
ferramenta pode ser usada é a **L11 Piso Gelado**, na posição 10 da campanha.

| Fases concluídas | Créditos (teto) | O que dá para comprar |
|---:|---:|---|
| 1 | 250 | Marcador + Rebobinar + Scanner (170), sobra 80 |
| 2 | 500 | **catálogo inteiro** (320), sobra 180 |
| 9 (quando a loja fica utilizável) | **2.250** | catálogo inteiro **7 vezes** |
| 27 (fim da campanha) | 6.750 | — |

O jogador chega na primeira fase onde pode usar uma ferramenta com dinheiro para
comprar tudo sete vezes. A "regra de progressão" da bíblia — *"se o jogador
conseguir comprar tudo muito cedo, a loja perde sentido"* — é violada por uma
ordem de grandeza.

Três coisas se somam para produzir isso:

1. O teto de 250 é alto demais frente a preços de 40–150.
2. As ferramentas são consumíveis, mas `TryPurchaseTool` só faz
   `AddToolCount(tool.ToolId, 1)` — não há limite de estoque nem de compras, então
   o jogador acumula unidades indefinidamente.
3. Os itens caros da bíblia (Força Hidráulica 120, Reposicionamento Manual 180,
   Macaco N-8 200, Ímã de Carga 220, Chave Mestra 250, Reforço de Empilhadeira
   250) e todos os **upgrades permanentes** (500–1500 na bíblia) ficaram de fora.
   Sem nada acima de 150, não há para onde o dinheiro ir.

**Correções, por ordem de retorno:**

| Ação | Efeito |
|---|---|
| Ligar upgrades permanentes de 500–1500 (Bolso Extra = 3º slot, Manual Avançado, Scanner Melhorado) | dá destino ao dinheiro acumulado sem tocar no núcleo do puzzle; a bíblia já os especifica |
| Baixar o teto de 250 para ~180 e tirar a medalha do teto | restaura a faixa e faz a medalha valer dinheiro |
| Adicionar os itens de Informação de custo médio (Etiqueta de Destino 60, Câmera de Segurança 90, Manual Técnico 25) | são de risco baixo pela própria bíblia e povoam a faixa 25–90 que hoje está vazia |
| Pagar `SectorClearReward` | a constante existe em `ShiftCredits.cs:39` (300 créditos) e **não é chamada em lugar nenhum** — é o único bônus da bíblia com valor implementado e nunca pago |

### 6.3 O bônus "sem ferramentas" é um imposto sobre usar a loja

Usar uma ferramenta custa o preço dela **mais** os 50 créditos do bônus `SEM
FERRAMENTAS` **mais** o ranking limpo. Comprar o Marcador de Rota (40) e usá-lo
uma vez custa efetivamente 90 créditos e tira o turno do ranking. Isso é coerente
com a bíblia, mas combinado com o teto de 250 significa que, para o jogador que
quer maximizar créditos, a loja é estritamente dominada por não usar a loja. O
sistema pune quem interage com ele.

---

## 7. Correções propostas, com layout provado

Rodei o solver em cada proposta abaixo. Todos os custos foram validados com replay
independente (`replay = True`).

### 7.1 Fase 03 — trocar a Carga Cruzada por uma fase de posição 3

Layout atual (custo **31**, 19 empurrões — o custo mais alto das doze primeiras
fases, empatado com a fase 14, na terceira posição):

```
#########
#   #  .#
#    $  #
#@# $ #.#
#  $    #
#.  #   #
#########
```

Proposta (custo **14**, 7 empurrões, ouro 19, platina 14) — cruz de verdade, com a
carga central bloqueando a sala se for movida cedo, exatamente como a bíblia
descreve, e área de manobra à direita:

```
#########
#   .   #
#   $   #
# $@$   #
#  ##   #
# . .   #
#########
```

`ótimo = 14 · empurrões = 7 · solução ULDDURRURDDRDL · replay OK`

A curva das cinco primeiras fases passaria de `3, 12, 31, 8, 20` para
`3, 12, 14, 8, 20`. A queda de 14 para 8 na fase 04 continua, mas cai de −23 para
−6. Se quiser monotonia total, trocar as fases 03 e 04 de posição resolve:
`3, 8, 14, 12*, 20` — mas isso adianta o sensor para a posição 2, o que só vale a
pena se a correção 7.2 for feita antes.

### 7.2 Fase 06 — construir o Portão de Peso que a bíblia pede

Layout atual (`Level06_TerminalRoute`, custo 13; dois sensores que já são alvos,
porta contornável):

```
#########
#       #
# $.    #
# $    .#
# $.    #
#@      #
#########
```

Proposta (custo **17**, 9 empurrões, ouro 23, platina 17):

```
###########
#     #   #
# $   A . #
#@$ 1.#   #
#     #   #
#     #   #
###########
```

- `1` sensor em (4,3) — **não é alvo**, pela primeira vez no jogo
- `A` porta em (6,4) — única passagem entre as duas salas
- 2 cargas, 2 alvos: (8,4) na sala direita e (5,3) na sala esquerda

| Teste | Resultado |
|---|---|
| Ótimo | **17** (9 empurrões, `RRLLURRRRRRLLLLDR`, replay OK) |
| Sem sensor/porta | 13 → **o desvio até o sensor custa 4 movimentos reais** |
| Porta permanentemente trancada | **insolúvel** → a porta é obrigatória |
| Sensor também é alvo? | **não** |

A sequência forçada é exatamente a da bíblia: empurrar a carga A até o sensor →
a porta abre → levar a carga B pela porta até (8,4) → **só então** empurrar a
carga A do sensor até o alvo (5,3), fechando a porta atrás. Quem empurrar a carga
A direto para o alvo antes de tudo fecha a porta com a carga B do lado errado.

E é recuperável sem reiniciar: a carga em (5,3) ainda pode ser empurrada
verticalmente e reposicionada. Numa fase de tutorial isso importa — a bíblia diz
que o jogador deve perder porque pensou errado, não porque o jogo o prendeu.

Essa única fase resolve três problemas de uma vez: ensina o sensor, ensina a
caixa-ferramenta, e dá ao Setor 02 o conteúdo que ele não tem.

### 7.3 Quebrar a casca C (L19 × L26)

As duas são a mesma sala. A correção mínima é reconstruir uma das duas do zero. A
L26 *Arquivo Morto* é a candidata natural porque a mecânica dela (névoa Memory +
parede falsa) pede uma planta com **cantos e ramificações** — a sala atual é um
retângulo aberto onde a névoa não esconde nada relevante: com raio 3 numa sala de
11×7, o jogador enxerga quase tudo o tempo todo. A névoa só é interessante numa
planta em L ou em três salas.

### 7.4 Dar função à carga pesada

Duas opções, do mais barato ao mais caro:

1. **Custo de empurrão 2 para `HeavyCrate`.** Uma linha em
   `PuzzleBoardModel.TryMove` (`moveCost` extra quando `GetCrateKind == HeavyCrate`)
   e o espelho no `solve` do Python. Muda o ótimo de 12 fases — **exige reprova
   completa e recálculo de medalhas**, mas transforma o Setor 05 inteiro numa
   decisão de rota ("vale a pena carregar o pesado por aqui?").
2. **Pesada não desliza no gelo nem na esteira.** Mais barato de provar (afeta só
   a S10) e faz a carga pesada finalmente conversar com outra mecânica, que é o
   que a bíblia pede para o Setor 05 em diante.

Antes de escolher, vale reconhecer que hoje o `goalRequirement` já é uma mecânica
válida — só não é "peso". Se a decisão for não implementar peso, o mínimo honesto
é renomear a coisa (carga marcada / carga etiquetada) e refazer a ficha do Setor
05, porque "Manutenção Pesada" com carga que não pesa nada é uma promessa não
cumprida ao jogador.

---

## 8. O que falta de design, priorizado

| Prioridade | Falta | Impacto |
|---|---|---|
| **P0** | **Caixa-ferramenta.** O conceito central de três fases da bíblia (06, 14, 25) não existe em nenhuma das 36 fases. | Retira do jogo a ideia que separa "Sokoban com sensores" de "Sokoban inteligente" |
| **P0** | **Fase 04 Etiqueta Errada** e **fase 08 Doca B-12** — a primeira pista da Duda e a primeira mentira do sistema. Ambas substituídas por fases genéricas do vertical slice. | O gancho narrativo do jogo só começa a operar depois da metade da campanha |
| **P1** | **Alvos ordenados.** A L28 *Carga Sem Origem* declara `ordered_goals` e a bíblia constrói a fase inteira nisso ("colocar caixas em alvos errados abre caminho falso"). O motor não tem ordem de alvos — só `goalRequirements` por tipo de carga. | A fase 28 hoje é um Sokoban comum com um nome narrativo |
| **P1** | **Segunda aparição** do robô, do botão de direção, do portão temporizado e da parede falsa. Cinco mecânicas com uma única fase cada. | Sem "testar" e "combinar", cada mecânica é um truque de festa |
| **P1** | **Corridas na campanha.** As fases 10, 13, 23 e a secreta 06 não estão em nenhum campaign asset. Os quatro `Track_*.asset` existem e o `TW08_RaceCampaign` os lista, mas são um modo separado. | A campanha é 27 puzzles seguidos; a alternância de ritmo da bíblia não acontece em lugar nenhum |
| **P2** | **Fases híbridas** (20, 24, 29). Estão marcadas com a tag `hybrid` mas são puzzles puros — não há trecho de empilhadeira dentro delas. | Os três clímaxes de setor não têm o clímax |
| **P2** | **Caixa frágil sem fragilidade.** `FragileCrate` (kind 3) é usada na L07 e na L09 e se move exatamente como uma caixa comum. Nas 27 fases de `level-layouts.json` o char `f` não aparece uma única vez — a linguagem de layout suporta a carga frágil e nenhum layout a usa. | Um tipo de carga que promete risco e não entrega nenhum |
| **P2** | **Segurar a porta com o corpo.** `door_state` mantém o grupo aberto enquanto o jogador ou uma carga ocupa a célula da porta — mecânica boa, implementada e testada. Como todos os 27 sensores são alvos, **nenhuma fase foi desenhada em torno disso**. | Material pronto e grátis, sem uso |
| **P2** | **Fase 22 Macaco N-8.** É um tutorial para uma ferramenta que a bíblia excluiu do MVP e que não existe. É a única fase do Setor 05 sem gimmick algum. | Vinte minutos de jogo prometendo algo que não vem |
| **P3** | **Setor 01 e 02 com 3 fases cada**, Setor 03 com 7. A bíblia pede 5 e 5. | Ritmo de setor desbalanceado; o frio dura 26% do jogo |
| **P3** | **Legibilidade do piso `~`.** O HUD mostra `MOVIMENTOS 022` para 12 passos e não explica. | O jogador não consegue entender por que perdeu o ouro |
| **P3** | **`GIMMICKS.md` está desatualizado.** A seção "O que continua sendo só etiqueta" lista `direction_button` e `temporary_block` como não implementados — ambos existem hoje em `PuzzleBoardModel` (`ApplyDirectionButton`, `timedBlocks`) e são usados na L17 e na S08. | Documento de referência que engana quem chegar depois |

---

## 9. Sequência recomendada de trabalho

1. **Fase 06 nova (§7.2).** Uma fase, 17 de custo, já provada. Resolve o P0 da
   caixa-ferramenta e o buraco do tutorial de sensor de uma vez só.
2. **Undo conta para a platina.** Um campo em `PuzzleRunSummary`, uma condição em
   `EvaluateMedal`. Restaura a intenção da bíblia sem reprovar nenhuma fase.
3. **Teto de créditos e upgrades permanentes.** Nenhuma fase muda; a economia
   passa a existir.
4. **Fase 03 nova (§7.1).** Já provada. Endireita o primeiro terço da curva.
5. **Decidir o que é a carga pesada** (§7.4) antes de escrever mais fases do Setor
   05 — a decisão muda 12 layouts.
6. **Reconstruir a L26** e depois quebrar a casca A (cinco fases). É o trabalho
   mais longo e o menos urgente, porque não bloqueia nada.

Os itens 1 a 4 não exigem reprovar a campanha inteira. O item 5 exige.

---

## 10. Reprodutibilidade

Todos os números deste documento saem de dois comandos:

```bash
python Tools/puzzle/tw08_solver.py --layouts Docs/level-layouts.json
python Tools/puzzle/tw08_solver.py --assets <dir com os .asset das fases 01-09>
```

As tabelas de ablação foram produzidas removendo cada mecânica do layout e
resolvendo de novo com o mesmo solver, e as propostas de §7 passaram por
`solve` + `replay` independente. As fases 01–09 vivem em
`Assets/_Project/ScriptableObjects/VerticalSlice/` (01–03) e
`Assets/_Project/ScriptableObjects/Campaign/` (04–09); o parser de `.asset` do
solver **não lê gelo, esteira, patrulha nem parede falsa**, o que é irrelevante
para essas nove fases porque nenhuma delas usa essas mecânicas — mas é uma
armadilha para quem tentar auditar as fases 11+ por esse caminho.
