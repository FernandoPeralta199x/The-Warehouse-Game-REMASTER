# Direção de Imagem — The Warehouse Nº 08

Diagnóstico crítico da direção visual. Auditoria feita por leitura de arquivos e
inspeção das imagens; nada foi executado no Unity e nenhum arquivo de arte ou de
código foi alterado.

**Método.** As cores citadas não são impressão: foram calculadas a partir dos
valores em código, aplicando a mesma multiplicação de tinta que o
`SpriteRenderer` faz, e convertidas para L\* (luminância perceptual). Os sprites
de referência foram medidos pixel a pixel. Onde há número, ele é reproduzível.

---

## 1. Veredito em uma página

O jogo tem **duas artes que não são só de estilos diferentes — são de mídias
diferentes**. Personagens e empilhadeiras são ilustrações com 12 mil cores e
sombreado contínuo. Piso, parede, caixa e alvo são pixel art real de 4 a 9 cores.
Isso é resolvível e eu digo como.

Mas o problema mais grave não é a costura. É este:

> **O tabuleiro comunica a estrutura pela decoração e esconde as mecânicas.**

Três achados que valem mais do que todo o resto somado:

1. **A parede tem contraste zero com o piso.** Parede `#232B2B` (L\* 16,8) contra
   piso `#252B2E` (L\* 17,1). Diferença: **−0,3 de L\***. O que faz a parede ser
   vista é a faixa de zebrado âmbar de 4 px na base dela — um enfeite. Num
   Sokoban, a parede é a regra do jogo. Ela está sendo comunicada por um adesivo.

2. **A seta da esteira não aponta para lugar nenhum.** `DrawGoal`
   (`TW08ProductionArtSetup.cs:296`) desenha quatro cantos e uma cruz centrada —
   uma figura com simetria de 4 dobras. `DrawBoard` rotaciona esse sprite em
   0°/90°/180°/−90° para indicar a direção da correia
   (`TW08PuzzleSceneBuilder.cs:427-433`). **Rotacionar uma figura de simetria
   quádrupla em múltiplos de 90° não muda um único pixel.** As quatro direções
   são visualmente idênticas. O `GIMMICKS.md:85` afirma que "a esteira mostra uma
   seta — sem indicador de direção a mecânica só seria descoberta por tentativa e
   erro". A mecânica está, hoje, sendo descoberta por tentativa e erro.

3. **As quatro mecânicas que mudam de estado são desenhadas mais escuras que o
   piso.** Robô L\* 5,4 · portão temporizado 7,0 · porta 9,2 · esteira 9,0 —
   contra piso 17,1. Elas são **buracos**, não objetos. O robô, que mata a jogada,
   é a coisa mais escura da tela inteira.

A regra que o tabuleiro segue hoje é: **decoração estática brilha, mecânica
dinâmica afunda.** É exatamente o inverso do necessário.

---

## 2. O que está bom — e é bastante

Antes da lista de problemas, o que não deve ser mexido:

- **A arte de referência é excelente e é o maior ativo do projeto.** John, Duda e
  Robert têm silhueta legível, linguagem corporal, identidade de uniforme
  (âmbar N-8) e o boné com o logo funciona como marca de leitura à distância. As
  seis empilhadeiras (`REFERENCIA/Sprites/2.png`) têm variação real de chassi e
  cor, não recolorações.
- **As pranchas de referência são um bíblia de arte completa, não um moodboard.**
  Há tiles de piso, portas, sensores, esteiras direcionais, ícones de power-up
  que batem um a um com a loja, seis cenários de ambiente prontos e uma paleta
  organizada em rampas. **A direção de arte já foi feita. Ela só não foi
  extraída.** Isso muda a natureza do trabalho que falta: não é "desenhar", é
  "recortar e normalizar".
- **A intenção do `DrawBoard` está certa em vários pontos.** A ordem de camadas
  respeita a lógica de um Sokoban (alvo abaixo da carga, carga abaixo do
  operador). Gelo e piso custoso foram conscientemente separados por alfa e
  brilho — a intenção documentada em `GIMMICKS.md:84-86` é a correta, a execução
  é que não entregou.
- **O alvo (`DrawGoal`) é o melhor sprite procedural do jogo.** Cantos de
  enquadramento + cruz central é um vocabulário clássico e correto de "deposite
  aqui". Ele é legível, tem L\* 77,6 e é a coisa mais brilhante do tabuleiro —
  o que, num Sokoban, é a hierarquia certa. **Ele só não podia ter virado a base
  de mais três coisas** (ver §4).
- **A linguagem de movimento da interface é madura.** `Motion/Easing.cs` tem 11
  curvas, `UIMotion.cs` tem `Typewriter`, `CountTo`, `Punch`, `Shake`, `PopIn`
  com durações bem escolhidas (0,3–0,4 s) e o `UIEntranceAnimator` faz cascata
  escalonada com atraso de 0,06 s por filho. Isso é bom trabalho e dá ao jogo uma
  personalidade de terminal que a arte do tabuleiro ainda não tem. **O HUD está
  mais adiantado que o tabuleiro.**
- A cortina de transição (`ScreenFader`, 0,42 s) e a névoa de guerra por tinta em
  vez de desligar objetos (`PuzzleFogOfWar`, `hiddenAlpha 0.06` /
  `rememberedAlpha 0.34`) são decisões tecnicamente corretas.

---

## 3. Coerência — onde a costura aparece, medida

### 3.1 O número que explica a costura

| | densidade da fonte | arquivo |
|---|---|---|
| Piso, parede, caixa, alvo | **32 px por unidade de mundo** | `TW08ProductionArtSetup.cs:19` |
| Personagens | **160 px por unidade** | `TW08ReferenceGameArt.cs:21` |
| Empilhadeiras | **200 px por unidade** | `TW08ReferenceGameArt.cs:22` |

O personagem carrega **5×** mais informação de textura por unidade de mundo que o
chão que ele pisa; a empilhadeira, **6,25×**. Em qualquer zoom, o operador parece
um adesivo de alta resolução colado sobre um cenário de baixa.

### 3.2 O agravante: a referência não é pixel art

Medi `John_IdleDown.png` (137×235):

- **12.272 cores únicas.** Pixel art de verdade usa 8 a 64. O piso procedural usa
  **6**; a parede, 9; a caixa, 8; o gelo, 4.
- **0% de alfa parcial** — as bordas são duras, mas o interior é ruído contínuo.
- O "pixel aparente" mede **3 a 4 téxeis** e **não está alinhado a uma grade**.

Ou seja: é uma pixel art de aproximadamente **40×60** que foi ampliada ~3,4× e
recebeu ruído. Ampliada e suja. A empilhadeira é pior: em zoom, regiões inteiras
de `Car1_Front.png` são pintura borrada, sem estrutura de pixel nenhuma.

**Consequência prática:** com `filterMode = Point` a 160 PPU, esse ruído não é
suavizado — ele é exibido. O personagem não aparece "mais detalhado", aparece
**sujo e trêmulo** ao lado de um piso perfeitamente limpo.

### 3.3 Onde a costura incomoda mais

Em ordem de quanto machuca:

1. **Contato pé/chão.** O personagem tem sombra de contato pintada e gradiente; o
   piso é chapado. O operador flutua.
2. **Borda do sprite.** John tem contorno escuro de espessura variável (2–4
   téxeis, porque veio de ampliação); a caixa tem contorno de exatamente 2 px
   chapado. Lado a lado, um parece desenhado e o outro parece vetor.
3. **Empilhadeira sobre tile.** É o pior caso — 6,25× de diferença e a
   empilhadeira nem tem estrutura de pixel para defender.

### 3.4 A correção — e ela é barata

O tamanho de mundo está certo; o que está errado é a densidade. Portanto:
**reamostrar a fonte para baixo e baixar o PPU na mesma proporção mantém o
tamanho em tela idêntico e unifica a grade de pixel.**

| | hoje | proposto | tamanho em mundo |
|---|---|---|---|
| Tiles | 32×32 @ 32 PPU | **48×48 @ 48 PPU** | 1,00 × 1,00 (igual) |
| John idle | 137×235 @ 160 PPU | **41×70 @ 48 PPU** | 0,86 × 1,47 (igual) |
| Empilhadeira frontal | 224×322 @ 200 PPU | **54×77 @ 48 PPU** | 1,12 × 1,61 (igual) |

Escolhi 48 px/célula porque é a densidade **nativa das próprias pranchas**: medi
os separadores do painel "TILES DE AMBIENTE (PISO)" da prancha 2 e as células têm
**~47×48 px**. Os tiles saem da prancha já na resolução certa. Não é um número
inventado — é o número que o material de origem já usa.

A reamostragem precisa ser por **quantização para a paleta**, não por média
simples: reduzir 12.272 cores para ~24 por sprite é o que transforma a ilustração
ampliada em pixel art de verdade. Feito isso, a costura não é atenuada — ela
deixa de existir, porque as duas artes passam a ser a mesma coisa.

---

## 4. Legibilidade das mecânicas — a seção mais grave

### 4.1 Duas formas para onze significados

O tabuleiro inteiro é montado com **dois sprites**, variando tinta e escala:

**`catalog.Goal` — 4 significados, 1 forma:**

| elemento | escala | tinta | ordem |
|---|---|---|---|
| Alvo | 0,90 | por tipo exigido | 4 |
| Seta de esteira | 0,45 | âmbar | 5 |
| Sensor | 0,72 | ciano | 6 |
| Botão de direção | 0,55 | âmbar | 6 |

**`catalog.Wall` — 5 significados, 1 forma:**

| elemento | escala | tinta | ordem |
|---|---|---|---|
| Parede | 1,00 | branco | 10 |
| Parede falsa | 1,00 | branco | 9 |
| Portão temporizado | 1,00 | vermelho | 9 |
| Porta | 1,00 | âmbar | 12 |
| Robô de limpeza | 0,62 | vermelho | 8 |

O jogador vê **o mesmo retículo em quatro tamanhos** e **o mesmo bloco em cinco
cores**. Não existe vocabulário de forma. Tudo é diferenciado por matiz — e o
matiz, como mostro em §4.3, também falha.

### 4.2 O que um jogador confunde — lista direta

| confunde | com | por quê |
|---|---|---|
| **Sensor** | **Botão de direção** | Mesma forma, escalas 0,72 e 0,55. Só a cor separa. |
| **Sensor** | **Alvo** | Mesma forma. Um jogador tenta entregar carga no sensor. |
| **Seta de esteira** | **Alvo pequeno** | Mesma forma a 0,45. E não indica direção nenhuma. |
| **Esteira ↑** | **Esteira ↓ ← →** | **Idênticas.** Simetria quádrupla anula a rotação. |
| **Robô** | **Portão temporizado** | Ambos são o sprite de parede em vermelho. Matiz H5 vs H6 — 1 grau. |
| **Robô** | **Parede** | Robô em ordem 8, parede em 10: **o robô é desenhado atrás da parede.** |
| **Parede falsa** | **Parede** | Correto por design. Mas empata em ordem 9 com o portão temporizado. |
| **Piso custoso** | **Gelo** | Mesmo sprite `Ice.png`, mesmo matiz. Diferença só de brilho. |
| **Caixa pesada** | **Caixa comum suja** | A tinta azul sobre madeira laranja dá **oliva**, não azul (§4.3). |
| **Alvo frágil** | **Alvo desativado** | A tinta produz verde **escuro** — lê-se como "apagado". |

### 4.3 A falha de tinta multiplicativa — com os números

`SpriteRenderer.color` **multiplica**. Aplicar uma tinta fria sobre um sprite
quente não o esfria: escurece e enlameia. Calculei o resultado real:

**Caixas** — sprite de madeira `#C9701C`:

| tipo | tinta | **cor real na tela** | lê-se como |
|---|---|---|---|
| Comum | — | `#C9701C` | madeira ✔ |
| **Pesada** | (0,46 · 0,69 · 0,92) | **`#5C4D1A`** | **oliva sujo ✘** |
| Frágil | (1 · 0,52 · 0,36) | `#C93A0A` | vermelho ✔ |

A caixa pesada deveria ser azul. Ela é **verde-oliva escuro**. A intenção não
chega ao jogador.

**Alvos** — sprite verde `#31DD74`:

| exigência | tinta | **cor real** | lê-se como |
|---|---|---|---|
| Qualquer | (0,58 · 1 · 0,72) | `#1CDD54` | verde |
| **Pesada** | (0,45 · 0,78 · 1) | **`#16AC74`** | **verde-azulado** |
| **Frágil** | (1 · 0,48 · 0,34) | **`#316A27`** | **verde escuro** |

**Os três alvos são verdes.** O sistema de alvo-com-exigência-de-tipo, que é uma
mecânica de design real, **não existe visualmente**. O alvo frágil, em L\* 39,8
contra 77,6 do comum, parece um alvo **desligado**.

**Sprite de parede tingido** — miolo `#232B2B`, faixa âmbar `#DE8B16`:

| elemento | **miolo real** | L\* | faixa |
|---|---|---|---|
| Parede | `#23282B` | 16,8 | `#DE8B16` |
| Porta | **`#231807`** | 9,2 | `#DE5504` |
| Portão temporizado | **`#22110F`** | 7,0 | `#D53A08` |
| Robô | **`#220B09`** | **5,4** | `#D52705` |

Porta, portão e robô são **blocos quase pretos**. A única coisa colorida neles é
a faixa de zebrado de 4 px herdada do desenho da parede. Ou seja: **três
mecânicas distintas são comunicadas pela cor de uma listra decorativa na base do
tile.** É por isso que a mecânica não se lê de relance — não há o que ler.

**Gelo vs piso custoso** — mesmo `Ice.png`, só alfa e tinta diferentes:

| | cor real composta sobre o piso | L\* |
|---|---|---|
| Piso custoso (α 0,62) | `#30677C` | 40,9 |
| Gelo (α 0,92) | `#4089A1` | 53,6 |

Mesmo matiz (H≈195), mesma textura, mesma forma. **Separados só por 12,7 de
L\*.** Duas mecânicas opostas — "andar aqui custa caro" e "aqui você não
controla onde para" — usando o mesmo signo. Um jogador que aprendeu que azul
significa deslizar vai entrar no piso custoso esperando deslizar.

### 4.4 Correções concretas

**Prioridade 1 — a seta da esteira.** Não é ajuste, é defeito. Ou se desenha um
sprite de seta assimétrico, ou se extrai o tile de seta âmbar que **já existe**
na prancha 2 (painel "SENSORES E MECÂNICAS", 2ª linha, 1ª célula: seta âmbar
sobre moldura tracejada âmbar). É a correção de maior impacto por menor esforço
do documento inteiro.

**Prioridade 2 — parar de tingir sprites escuros.** Substituir tinta
multiplicativa por sprite próprio, para: caixa pesada, alvo pesado, alvo frágil,
porta, portão temporizado, robô. Onde não houver sprite ainda, usar tinta
**aditiva/aclarante** (multiplicar por valor > 1 em um canal só) ou trocar o
sprite base por um cinza neutro `#8A9296`, sobre o qual a tinta multiplicativa
funciona como esperado. Esta segunda opção é de uma linha e resolve seis
elementos.

**Prioridade 3 — vocabulário de forma.** Cada mecânica precisa de silhueta
própria. Proposta, toda ela extraível das pranchas:

| mecânica | forma | onde já existe |
|---|---|---|
| Alvo | cantos + cruz (manter) | `DrawGoal`, ou prancha 1 "SINAIS", suporte de cantos âmbar |
| Esteira | **seta assimétrica** sobre correia | prancha 2, "SENSORES E MECÂNICAS" |
| Sensor | **olho / lente circular** | prancha 2, mesmo painel (2 variantes de olho) |
| Botão de direção | **botão redondo com haste** | prancha 1, "BOTÕES / ALAVANCAS" |
| Porta | **porta com trilho e batente** | prancha 2, "PORTAS E PORTÕES" (6 variantes) |
| Portão temporizado | **barreira a laser vermelha** | prancha 2, 3ª linha do painel de sensores |
| Robô | **silhueta redonda com farol** | prancha 1, "OUTROS", farol vermelho |
| Gelo | **tile de gelo azul-claro riscado** | prancha 2, "TILES DE AMBIENTE" (2 tiles) |
| Piso custoso | **tile de zebrado âmbar** | prancha 2, mesmo painel |
| Parede falsa | parede idêntica (correto) | — |

Note que gelo e piso custoso passam a se separar **por matiz** (azul frio vs
âmbar quente), não por brilho. Essa é a separação que sobrevive a um jogador
daltônico, a um monitor mal calibrado e a uma olhada rápida.

---

## 5. Paleta

### 5.1 Os valores em código

`TW08ProductionSceneUtility.cs:14-22`:

| nome | hex | H | S | L |
|---|---|---|---|---|
| `Background` | `#030405` | 202° | 22% | 2% |
| `Panel` | `#080C0E` | 195° | 22% | 4% |
| `PanelLight` | `#0E1314` | 192° | 19% | 7% |
| `Green` | `#3FF293` | 148° | 87% | 60% |
| `Amber` | `#FFA01E` | 35° | 100% | 56% |
| `Cyan` | `#42D6EA` | 187° | 80% | 59% |
| `Red` | `#F44738` | 5° | 90% | 59% |
| `TextPrimary` | `#DDF4E8` | 147° | 53% | 92% |
| `TextMuted` | `#77A391` | 155° | 19% | 55% |

### 5.2 Funciona como sistema? Não — e o motivo é estrutural

**Falta a faixa média.** O sistema tem três tons quase pretos (L\* 2–7) e cinco
acentos quase neon (L\* 55–92). **Não há nada entre 8 e 54.** Um tabuleiro
construído assim é um vazio preto com pontos fluorescentes: sem tons médios, todo
acento vira destaque máximo e nenhum vence. É a causa raiz de "onze elementos
gritando no mesmo volume" (§6).

**Há cor demais no sentido errado.** Não há acentos demais — cinco é razoável. O
problema é que **cada acento carrega significados demais**:

| cor | significados que carrega |
|---|---|
| **Âmbar** | esteira, botão de direção, porta, faixa de zebrado da parede, rótulo OPERADOR, botão RESET, medalha de ouro, subtítulo do menu, botões CORRIDA e OFICINA, barra de acento, narrativa, ARQUIVO SECRETO |
| **Ciano** | sensor, piso custoso, gelo, caixa pesada, alvo pesado, UNDO, REDO, botão OPERADORES, medalha de platina |
| **Verde** | alvo, status OK, `TextPrimary`, `TextMuted`, botão CAMPANHA, créditos, TURNO LIMPO |
| **Vermelho** | robô, portão temporizado, caixa frágil, alvo frágil, erro |

**Sim, o âmbar está sobrecarregado — 48 usos em 13 arquivos.** Mas ele não é o
pior caso. O pior é o **ciano, que carrega três mecânicas de tabuleiro
mutuamente exclusivas** (sensor, gelo, piso custoso) mais dois tipos de carga.
Quando tudo que é azul pode ser cinco coisas, o azul deixou de informar.

**O verde é o mais corrompido.** `TextPrimary` (`#DDF4E8`) e `TextMuted`
(`#77A391`) são **brancos e cinzas esverdeados** — matiz 147° e 155°, os mesmos
do `Green` de sinalização (148°). Como todo o texto do HUD já é verde, o verde
perdeu valor de sinal: quando o alvo acende em verde, ele acende na cor do
mobiliário. **Correção de uma linha:** neutralizar o texto para
`TextPrimary #E4EAED` (H 200°, S 15%) e `TextMuted #7C8A91` (H 197°, S 8%). O
texto vira cinza-frio de terminal, e o verde volta a significar exclusivamente
"objetivo cumprido".

**Colisões de matiz medidas** (dentro de 5°, significados diferentes):

- `Red` H5 (robô) × `TimedGate` H6 (portão) — **1°**
- `Amber` H35 × `Door` H32 × medalha de bronze H31 — **4°**
- `Cyan` H187 × `Sensor` H189 × `IceTint` H189 — **2°**
- `GoalHeavy` H204 × medalha de platina H203 × `HeavyCrate` H210 — **7°**

### 5.3 A paleta que a referência propõe — e que o código ignorou

Amostrei o painel "PALETA DE CORES" da prancha 2 (10×5 amostras):

| linha | conteúdo |
|---|---|
| 1 — cinzas frios | `#626469` `#4E5157` `#4A4B4F` `#4B423B` `#3D352E` `#322E2C` `#2C2929` `#22232B` `#20252F` `#191D25` |
| 2 — creme → âmbar → ferrugem | `#7A7572` `#807B78` `#9F8240` `#9D6F02` `#945F01` `#965C03` `#7B4104` `#6E310B` `#541C0B` `#390D0A` |
| 3 — teal → azul → roxo | `#223529` `#13251B` `#102D27` `#0F3136` `#24424F` `#1D4360` `#1F335D` `#222358` `#251543` `#2B0F39` |
| 4 — marrons quase pretos | `#1A1817` … `#0E0B0A` |
| 5 — fundo | `#06090E` … `#05090D` |

Duas diferenças estruturais em relação ao código:

1. **A referência trabalha em rampas de 10 passos; o código tem acentos
   chapados.** É por isso que a arte procedural parece plana e a de referência
   parece iluminada. Cada material precisa de 3 tons no mínimo (sombra, base,
   luz) — hoje o piso tem 6 cores para o tile inteiro.
2. **A referência é escura e dessaturada.** Seu âmbar mais forte é `#9D6F02`
   (L\* ≈ 31). O `Amber` do código é `#FFA01E` (L\* 74). Os acentos do código
   estão **~25 pontos de L\* mais claros e 2 a 3× mais saturados** que qualquer
   coisa na referência. O jogo em código é um HUD de terminal brilhante; a
   referência é um armazém noturno. **A referência está certa** — e o
   `Background #030405` do código é o único valor que já concorda com ela.

### 5.4 Paleta proposta — três camadas, com validação

A regra é: **saturação plena é um recurso escasso, reservado à sinalização.**

**Camada 0 — cenário. Nunca saturado, L\* ≤ 35.**

| papel | hex | L\* | ΔL\* vs piso |
|---|---|---|---|
| Fundo / vinheta | `#06090E` | 2,4 | −9,9 |
| Piso A | `#1B2124` | 12,3 | 0 |
| Piso B (xadrez) | `#262E33` | 18,4 | **+6,1** |
| Parede — face | `#2E383D` | 22,8 | **+10,5** |
| Parede — topo iluminado | `#46545B` | 34,8 | **+22,5** |
| Parede — sombra de contato | `#0D1114` | 4,9 | −7,4 |

Isto sozinho conserta o achado nº 1: a parede passa de **ΔL\* −0,3 para +22,5**.

**Camada 1 — modificadores de piso. Separados por MATIZ, não por brilho.**

| papel | hex | L\* | matiz |
|---|---|---|---|
| Gelo — base | `#3E6E80` | 43,8 | **azul frio 197°** |
| Gelo — risco de brilho | `#8FC4D6` | 76,2 | azul frio |
| Piso custoso — base | `#3A3222` | 21,2 | **âmbar quente 42°** |
| Piso custoso — zebrado | `#6E5A2E` | 39,3 | âmbar quente |
| Esteira — correia | `#333C41` | 24,7 | cinza neutro |

**Camada 2 — carga. Matiz próprio, L\* na faixa 39–55, sprites separados.**

| papel | hex | L\* |
|---|---|---|
| Caixa comum (madeira) | `#B4741F` | 54,3 |
| Caixa pesada (aço) | `#4E7391` | 46,9 |
| Caixa frágil | `#A8352A` | 39,6 |

**Camada 3 — sinalização. Único lugar onde saturação plena é permitida.**

| papel | hex | significado — **e só ele** |
|---|---|---|
| `Green` `#3FF293` | L\* 85 | objetivo / entrega concluída |
| `Cyan` `#42D6EA` | L\* 79 | vínculo sensor↔porta |
| `Amber` `#FFA01E` | L\* 74 | maquinário que você pode alterar |
| `Red` `#F44738` | L\* 56 | perigo que se move |

Com isso o ciano deixa de ser gelo, piso custoso e caixa pesada; o âmbar deixa de
ser porta, listra de parede e metade do menu.

**Duas observações de manutenção:**

- `UI/Hud/HudPalette.cs` **duplica os nove valores** de
  `TW08ProductionSceneUtility.cs`. O comentário no arquivo reconhece e justifica
  (a utility é de editor e não existe no player), mas o efeito é que **qualquer
  mudança de paleta precisa ser feita em dois lugares ou o HUD anima de volta
  para a cor antiga**. Vale extrair para um `ScriptableObject` de paleta ou uma
  classe em assembly compartilhada.
- No menu (`TW08MenuSceneBuilder.cs:127-130`), CAMPANHA é verde, CORRIDA é âmbar,
  OFICINA é âmbar e OPERADORES é ciano. As cores são **variação decorativa, não
  categoria**. Se a paleta vira sistema no tabuleiro, o menu precisa segui-lo.

---

## 6. Hierarquia visual do tabuleiro

### 6.1 O que o olho encontra primeiro — medido

L\* de cada elemento contra o piso (`#252B2E`, L\* 17,1):

| elemento | L\* | ΔL\* |
|---|---|---|
| Alvo comum | 77,6 | +60,6 |
| Sensor | 68,6 | +51,5 |
| **Faixa âmbar da parede** | **65,0** | **+47,9** |
| Seta de esteira / botão de direção | 64,2 | +47,1 |
| John (camisa) | 63,6 | +46,5 |
| Alvo pesado | 62,5 | +45,4 |
| Caixa comum | 56,2 | +39,1 |
| Gelo | 53,6 | +36,5 |
| Faixa do robô | 46,5 | +29,4 |
| Caixa frágil | 46,3 | +29,2 |
| Piso custoso | 40,9 | +23,8 |
| Alvo frágil | 39,8 | +22,7 |
| Caixa pesada | 33,3 | +16,2 |
| Piso secundário | 20,3 | **+3,2** |
| **Parede (miolo)** | **16,8** | **−0,3** |
| Porta | 9,2 | −7,9 |
| Esteira | 9,0 | −8,1 |
| Portão temporizado | 7,0 | −10,1 |
| **Robô** | **5,4** | **−11,7** |

Leitura desta tabela:

- **Onze elementos entre L\* 46 e 78.** Isso não é hierarquia, é um platô. Tudo
  que importa e tudo que não importa gritam no mesmo volume.
- **O terceiro elemento mais brilhante do jogo é um enfeite** — a faixa de
  zebrado da parede, mais forte que o próprio operador.
- **O xadrez do piso é invisível** (ΔL\* 3,2). O jogador não tem referência de
  grade, o que num jogo de contar movimentos é um custo real de usabilidade.
- **A parede desaparece** (ΔL\* −0,3).
- **As quatro mecânicas de estado são as quatro coisas mais escuras da tela.**

A hierarquia correta para um Sokoban, em ordem: **alvo > carga > operador >
perigo móvel > parede > modificador de piso > piso**. A hierarquia atual é:
**alvo > enfeite de parede > operador > carga > modificador de piso > piso >
perigo móvel**. Perigo móvel está em último.

### 6.2 Ordem de camadas — dois defeitos e uma reorganização

Valores atuais (`TW08PuzzleSceneBuilder.cs`):

```
-20 piso      -10 custoso   -9 gelo      -8 esteira
  4 alvo        5 seta       6 sensor     6 botão de direção
  8 ROBÔ        9 parede falsa  9 portão temporizado
 10 parede     12 porta      20 caixa     30 operador
```

**Defeito A — o robô é ocluído pelo cenário.** Ordem 8, contra parede 10 e porta
12. O robô patrulha corredores; ao passar por uma célula adjacente a parede ou
porta com qualquer sobreposição de sprite, **ele é desenhado por trás**. É uma
entidade dinâmica com ordem de cenário estático.

**Defeito B — empate em 9.** Parede falsa e portão temporizado compartilham a
ordem 9. Numa célula onde os dois coexistam, quem fica na frente é
indeterminado (resolvido por ordem de criação, não por intenção).

**Observação C — a esteira é um buraco.** A correia é `FloorSecondary` tingido
`(0,30 · 0,34 · 0,36)`, resultando `#171A1B` — **mais escura que o piso**. Somada
à seta que não aponta, a esteira é hoje um quadrado preto com uma cruz âmbar
fraca. É a mecânica pior lida do jogo.

**Reorganização proposta**, em bandas de 10 para permitir inserção futura:

| ordem | banda |
|---|---|
| −100 | fundo / vinheta |
| −50 | piso base |
| −40 | modificadores de piso (gelo, custoso, correia) |
| −30 | pintura de piso (setas de esteira, botão de direção, sensor) |
| −20 | alvo |
| 0 | estrutura estática (parede, parede falsa) |
| 10 | maquinário com estado (porta, portão temporizado) |
| 20 | carga |
| **25** | **robô** ← hoje 8 |
| 30 | operador |
| 40 | VFX / poeira |
| 50 | névoa de guerra |

O princípio: **tudo que é pintado no chão fica abaixo de tudo que é sólido; tudo
que se move fica acima de tudo que é fixo.** A ordem atual mistura as duas
famílias.

---

## 7. Fatiamento pendente — o que há nas pranchas

As duas pranchas são 1536×1024, RGB, sem alfa, sobre fundo preto de painel. Não
são moodboards: são folhas de sprite organizadas em painéis rotulados.

### 7.1 Conteúdo — `REFERENCIA/Sprites/1.png`

| painel | região aprox. | conteúdo |
|---|---|---|
| PERSONAGENS | x 10–500, y 5–465 | John, Duda, Robert — idle, caminhada, empurrando caixa; linha de emotes |
| EMPILHADEIRAS | x 505–920 | N-8 Padrão / Heavy / Elite / Prototype × 5 vistas |
| CAIXAS E OBJETOS | x 925–1268, y 5–468 | **7 linhas**: ~15 caixas de madeira e metal (2 com estêncil N-8), tambores, paletes, cone, gaiola de arame, caixa de ferramentas, macacos hidráulicos, galões, cofre |
| ELEMENTOS INTERATIVOS | x 1268–1532 | 5 sensores azulados · 4 botões redondos + 8 alavancas · 3 portas · 5 terminais CRT verdes · farol, sirene, câmera, painel |
| AMBIENTES (TILES) | x 8–480, y 478–815 | **grade 9×3 de tiles de piso, passo ~52 px** + fila de grades/corrimãos + fila de props (caixas em palete, tubulação, tanques) |
| CENÁRIOS E DECORAÇÕES | x 485–895 | estantes, armários, quadros de aviso, caixas de fusível, ar-condicionado, bancadas, carrinhos |
| PADRÕES DE FASES | x 905–1530, y 478–815 | **8 layouts de fase montados** — o melhor guia de como o tabuleiro deveria ler |
| AÇÃO / VFX / UI / SINAIS | y 820–1020 | empurrar, pular, usar terminal; poeira, faísca, sucesso, alerta; corações, moedas, medalhas, estrelas; **setas cinza ← →**, zebrados, placa N-8, triângulo de alerta, placa PARE, **cantos de alvo em âmbar** |

### 7.2 Conteúdo — `REFERENCIA/Sprites/2.png`

| painel | região aprox. | conteúdo |
|---|---|---|
| PERSONAGENS PRINCIPAIS | x 10–450, y 20–450 | os 3 + **"ELIAS", um quarto personagem que não está no elenco do jogo** |
| EMPILHADEIRAS | x 455–950, y 85–450 | **6 variantes nomeadas** (Standard, Heavy, Electric, Pro, Cold, Race) + 6 estados de animação |
| POWER UPS | x 340–630, y 465–630 | **10 ícones** — Rebobinar, Scanner, Dica, Macaco N-8, Empurrão, Mapa, Congelar, Extensor, Teleportar, Proteção. Batem 1:1 com a loja |
| **TILES DE AMBIENTE (PISO)** | x 638–952, y 462–635 | **grade ~8×3, célula medida em ~47×48 px**: concreto, zebrado âmbar, chapa xadrez, **2 tiles de GELO azul-claro riscado**, tábuas, grelha, tijolo enferrujado |
| CAIXAS E PALETES | x 955–1230, y 60–345 | 4×5 caixas, incluindo estênceis N-8 e variantes coloridas |
| **SENSORES E MECÂNICAS** | x 1005–1235, y 348–642 | olhos-sensor, anel scanner âmbar, **SETA ÂMBAR DIRECIONAL sobre moldura tracejada**, cruz, X, moldura vazia; **3 barreiras a laser vermelhas** |
| PORTAS E PORTÕES | x 1238–1530, y 348–642 | 6 portas (gradeada, dupla rebitada, de enrolar, elevador, N-8, vão com luz vermelha), cada uma com peça de trilho de piso |
| **CENÁRIOS E AMBIENTES** | largura total, y 660–820 | **6 cenas prontas**: Doca de Recebimento, Corredor Industrial, Câmara Fria, Área de Expedição, Oficina N-8, Setor 08 (Bloqueado) |
| DECORATIVOS / UI / SINAIS / VFX / PALETA | y 835–1020 | tubulação, luminárias; HUD com corações e cronômetro; zebrados; poeira e faíscas; **paleta 10×5** |

### 7.3 Prioridade de extração

Ordenado por **impacto no jogador ÷ dificuldade de recorte**.

| # | o que | de onde | itens | dificuldade | por que primeiro |
|---|---|---|---|---|---|
| **1** | **Tiles de piso** | pr. 2, "TILES DE AMBIENTE" | ~24 | **Baixa** — grade regular, célula ~47×48, sulcos pretos uniformes; fatiável por grade automática | Resolve piso, xadrez, **gelo** e **piso custoso** de uma vez. Já vem na densidade de 48 px que unifica o projeto (§3.4) |
| **2** | **Seta direcional + sensores + lasers** | pr. 2, "SENSORES E MECÂNICAS" | ~15 | **Baixa–Média** — as duas primeiras linhas são grade regular; a linha dos lasers é irregular (os feixes ligam as peças) e pede corte manual | Conserta o defeito nº 2 do documento. A seta assimétrica existe pronta |
| **3** | **Caixas e tambores** | pr. 1, "CAIXAS E OBJETOS" | ~40 | **Baixa** — objetos bem separados sobre preto chapado; extração por componente conexo funciona | A caixa é o objeto que o jogador mais toca. Dá caixa comum, pesada (metal) e frágil como **sprites distintos**, encerrando a falha de tinta (§4.3) |
| **4** | **Sinais e marcações** | pr. 1, "SINAIS E MARCAÇÕES" | ~16 | **Baixa** | Setas cinza, cantos de alvo, zebrados, PARE — vocabulário de sinalização de graça |
| **5** | **Power-ups** | pr. 2, "POWER UPS" | 10 | **Baixa** — grade regular | Batem 1:1 com a loja; hoje a barra de ferramentas não tem ícone |
| **6** | **Portas e portões** | pr. 2, "PORTAS E PORTÕES" | 6+6 | **Média** — cada porta vem com uma peça de trilho de piso colada embaixo que precisa ser separada; são mais altas que uma célula | Porta e portão temporizado são hoje blocos pretos |
| **7** | Tiles do segundo conjunto | pr. 1, "AMBIENTES (TILES)" | 27 | **Baixa** — passo ~52 px | Parcialmente redundante com o item 1; usar para variação e para as paredes |
| **8** | Terminais, botões, alavancas | pr. 1, "ELEMENTOS INTERATIVOS" | ~25 | **Baixa–Média** | Botão de direção e decoração de cenário |
| **9** | Cenários de ambiente | pr. 2, "CENÁRIOS E AMBIENTES" | 6 | **Baixa** para recortar | Não são assets de tabuleiro — são **o melhor guia de iluminação existente** e servem de fundo para seleção de fase e telas de carregamento |
| **10** | Estantes e decoração | pr. 1, "CENÁRIOS E DECORAÇÕES" | ~30 | **Média** — tamanhos variados, alguns objetos se tocam | Preenchimento de cenário; último |

### 7.4 O alerta que precede todo o fatiamento

**Recortar não basta.** Todo pixel dessas pranchas tem o mesmo defeito medido em
§3.2: milhares de cores, blocos de pixel irregulares de 3–4 téxeis, bordas
borradas. Um tile extraído cru e importado a 48 PPU **traz o ruído junto** — a
costura apenas migra dos personagens para o chão.

O fatiamento tem dois passos, e o segundo é o que importa:

1. **Recortar** na grade nativa (~47×48 para os tiles da prancha 2).
2. **Normalizar**: reamostrar para 48×48 exatos e **quantizar para a paleta de
   §5.4** (ordem de 16–24 cores por tile). É este passo que converte ilustração
   ampliada em pixel art, e é ele que decide se o resultado é coerente ou se o
   projeto passa a ter três artes em vez de duas.

Vale automatizar o passo 2 num script Python de pipeline (Pillow + quantização
com paleta fixa) e versionar a paleta como arquivo, para que toda arte futura
passe pelo mesmo funil.

---

## 8. Direção para o cenário desenhado

### 8.1 Referências de estilo — concretas

- **Into the Breach** — *a* referência obrigatória. É um jogo de grade tática onde
  cada ameaça é legível numa captura estática. A regra deles: **a forma carrega o
  significado, a cor carrega o estado.** Se o Warehouse adotar só isso, resolve
  §4 inteiro.
- **Signalis** — o clima exato: industrial, noturno, CRT, âmbar de alerta contra
  ciano frio, pretos profundos com neblina. É o que as seis cenas de ambiente da
  prancha 2 já estão perseguindo.
- **Duskers** — terminal monocromático como interface diegética. Referência para
  o HUD, que já está no caminho certo.
- **Papers, Please** — paleta restrita, suja e burocrática; prova que 20 cores
  bem escolhidas bastam para um jogo inteiro.
- **Katana ZERO** — tratamento de luz: fontes coloridas fortes sobre base quase
  preta, sem meio-tom desperdiçado.
- **Death's Door / Blasphemous** — renderização de material (metal, madeira,
  ferrugem) em poucos tons.

O eixo: **Into the Breach para a legibilidade da grade, Signalis para a
atmosfera.** Os dois convivem porque o primeiro governa forma e o segundo governa
luz.

### 8.2 Escala do pixel — decidida

- **Célula: 48×48 px, 48 PPU, 1 célula = 1 unidade de mundo.** Não é escolha
  estética: é a densidade nativa medida nas pranchas (§3.4).
- **Operador: ~41×70 px**, pivô nos pés, ≈1,45 célula de altura.
- **Empilhadeira: ~54×77 px.**
- **Câmera ortográfica em múltiplo inteiro** do PPU, para nunca haver meio pixel.
  `orthographicSize = Mathf.Max(4.2f, level.Height * 0.67f)`
  (`TW08PuzzleSceneBuilder.cs:477`) produz valores fracionários e deve passar a
  arredondar para a grade.
- **Contorno de 1 px, nunca preto puro.** Sobre um fundo `#06090E`, contorno preto
  desaparece. Usar o tom escuro do próprio material: `#12171A` para aço,
  `#4A2C0E` para madeira, `#1D3742` para gelo.

### 8.3 Tratamento de luz

As seis cenas da prancha 2 já definem o modelo; basta transpô-lo para a vista de
cima:

- **Uma fonte quente, alta e fora de quadro** (as luminárias de galpão). Todo
  objeto recebe **1 px de luz âmbar dessaturada `#6E5A2E` no topo**.
- **Preenchimento frio pelo chão** — 1 px de `#24424F` na face inferior dos
  objetos altos.
- **Sombra de contato obrigatória:** 1–2 px de `#0D1114` na base de todo objeto
  que ocupa célula. É o que ancora a carga no piso e o que hoje falta — o motivo
  de o operador parecer flutuar (§3.3).
- **O piso não recebe luz.** Ele é a camada mais escura legível (L\* 12–18) e
  precisa continuar sendo, para que carga e sinalização tenham contra o que
  brilhar.
- **Vinheta sutil nas bordas do tabuleiro** com `#06090E` — reforça o armazém
  noturno e ajuda a névoa de guerra a parecer intencional em vez de bug.

### 8.4 Regras de forma que devem virar lei do projeto

1. **Uma mecânica, uma silhueta.** Nenhum sprite serve a dois significados. Hoje
   dois sprites servem a onze.
2. **Cor indica estado, não identidade.** A porta é uma porta pela forma; o âmbar
   diz que está fechada e o verde que abriu.
3. **Nada que muda de estado pode ser mais escuro que o piso.** Regra derivada
   diretamente da tabela de §6.1.
4. **Saturação plena é orçamento fixo:** quatro cores, quatro significados, e
   nenhuma delas aparece em cenário ou decoração.
5. **Tinta multiplicativa só sobre base cinza neutra.** Sobre sprite colorido,
   ela enlameia — está provado em §4.3.
6. **Perigo se anuncia por movimento**, não por brilho estático: o robô ganha
   farol pulsante (o sprite existe na prancha 1, painel "OUTROS"), aproveitando o
   `UIMotion` que o projeto já tem.

---

## 9. Ordem de trabalho recomendada

Ordenada por impacto no jogador por hora de trabalho.

**Correções de código, sem arte nova — resolvem a maior parte do dano:**

1. **Seta de esteira assimétrica.** Defeito funcional, não estético.
2. **Robô da ordem 8 para 25.** Uma linha; hoje ele fica atrás das paredes.
3. **Desempatar parede falsa e portão temporizado** (ambos em 9).
4. **Neutralizar `TextPrimary` e `TextMuted`** para cinza-frio, devolvendo ao
   verde o valor de sinal. Dois valores, em dois arquivos (lembrar do
   `HudPalette.cs`).
5. **Separar parede de piso** em L\*: parede face `#2E383D`, topo `#46545B`;
   piso `#1B2124` / `#262E33`. Corrige o achado nº 1.
6. **Trocar a base tingida por cinza neutro** (`#8A9296`) para porta, portão,
   robô, caixa pesada e alvos — faz a tinta multiplicativa voltar a funcionar em
   seis elementos de uma vez.
7. **Clarear a correia da esteira** para `#333C41` (hoje é mais escura que o
   piso).

**Arte, na ordem de §7.3:**

8. Fatiar e normalizar os tiles de piso da prancha 2 (inclui gelo e zebrado).
9. Fatiar seta direcional, sensores e barreiras a laser.
10. Fatiar as caixas — comum, metálica e frágil como sprites distintos.
11. Reamostrar personagens e empilhadeiras para 48 PPU (§3.4), encerrando a
    costura.

**Estrutural, quando houver fôlego:**

12. Extrair a paleta para um `ScriptableObject` único, eliminando a duplicação
    entre `TW08ProductionSceneUtility.cs` e `HudPalette.cs`.
13. Script de pipeline (recorte → reamostragem → quantização) para que toda arte
    futura entre pelo mesmo funil.

---

## Anexo — arquivos citados

| assunto | arquivo |
|---|---|
| Paleta em código | `Assets/_Project/Scripts/Editor/TW08ProductionSceneUtility.cs:14-22` |
| Tinta das caixas | `Assets/_Project/Scripts/Editor/TW08ProductionSceneUtility.cs:202-210` |
| Montagem do tabuleiro | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:341-456` |
| Piso, custoso e gelo | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:358-374` |
| Esteira e seta | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:413-434` |
| Sensor e porta | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:436-455` |
| Botão de direção | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:238-243` |
| Portão temporizado | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:246-269` |
| Robô | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:273-296` |
| Tinta dos alvos | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:458-469` |
| Câmera | `Assets/_Project/Scripts/Editor/TW08PuzzleSceneBuilder.cs:471-479` |
| Arte procedural (piso, parede, caixa, alvo) | `Assets/_Project/Scripts/Editor/TW08ProductionArtSetup.cs:250-332` |
| `DrawGoal` — a figura simétrica | `Assets/_Project/Scripts/Editor/TW08ProductionArtSetup.cs:296-311` |
| `DrawWall` — a faixa âmbar | `Assets/_Project/Scripts/Editor/TW08ProductionArtSetup.cs:277-294` |
| `DrawIce` | `Assets/_Project/Scripts/Editor/TW08ExpansionStarterArt.cs:212-219` |
| PPU de personagem e empilhadeira | `Assets/_Project/Scripts/Editor/TW08ReferenceGameArt.cs:21-22` |
| Paleta duplicada do HUD | `Assets/_Project/Scripts/UI/Hud/HudPalette.cs` |
| Cores do menu | `Assets/_Project/Scripts/Editor/TW08MenuSceneBuilder.cs:127-130` |
| Linguagem de movimento | `Assets/_Project/Scripts/Motion/UIMotion.cs`, `Easing.cs` |
| Névoa de guerra | `Assets/_Project/Scripts/Puzzle/PuzzleFogOfWar.cs:37-38` |
| Afirmação sobre a seta | `Docs/GIMMICKS.md:84-86` |
| Pranchas de referência | `REFERENCIA/Sprites/1.png`, `REFERENCIA/Sprites/2.png` (1536×1024) |
