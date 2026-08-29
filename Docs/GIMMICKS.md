# Gimmicks — mecânicas de fase

## O problema que isto resolve

As fases declaravam 48 tags de gimmick, mas o motor só sabia fazer duas coisas:
piso custoso (custo 2) e sensor/porta. `ice_floor` era uma etiqueta sobre células
de custo 2 — o dado dizia "gelo" e o jogo cobrava pedágio.

## Regra de ouro deste trabalho

**O solver e o motor mudam juntos, e as fases são reprovadas antes de qualquer
outra coisa.** As 27 fases só valem enquanto existe prova de que são
solucionáveis; mexer no motor sem reprovar transforma a campanha inteira em
suposição.

Ordem seguida: modelo C# → solver Python → conversão dos layouts → reprova →
recálculo das medalhas → cenas.

## Gelo (`ice_floor`)

Quem entra numa célula de gelo **continua na mesma direção** até pisar em piso
comum ou até a próxima célula estar ocupada. Vale para o jogador e para a carga.

**Custo: 1 por comando, não por célula.** Deslizar é um gesto único. Cobrar por
célula tornaria o gelo um castigo, quando o desenho das fases o usa como atalho.

**Ordem dentro de um empurrão:** a carga desliza primeiro, depois o jogador entra
e desliza. Sem essa ordem o jogador ocuparia a célula que a carga ainda vai
cruzar. O jogador para encostado na carga — nunca a atravessa nem a empurra duas
vezes no mesmo comando.

## Esteira (`conveyor`)

É gelo com direção própria: quem entra é levado na direção da correia, e não na
direção em que vinha. Isso é o que torna a esteira um obstáculo em vez de um
atalho — entrar por um lado errado devolve a carga.

A mesma função resolve as duas mecânicas (`PuzzleBoardModel.Slide`), com um teto
de iterações: um anel fechado de esteiras giraria para sempre sem ele.

## Efeito nas fases

| Fase | Antes | Depois |
|---|---|---|
| L11 Piso Gelado | 21 | **10** |
| L12 Frio no Corredor | 39 | **25** |
| L14 Sensor Congelado | 40 | **31** |
| L15 Câmara 08-C | 46 | **39** |
| L16 Esteira Ligada | 13 | **7** |
| L17 Rota Automática | 23 | **19** |
| L20 Linha de Produção | 62 | **46** |
| S10 O Caminho da Duda | 56 | **46** |

Os custos caíram porque gelo e esteira transportam de graça. **Todas as 27 fases
continuam provadas solucionáveis** e as medalhas foram recalculadas a partir dos
novos ótimos.

## Cuidado que quase passou batido

A poda de células mortas do solver assume empurrão de uma casa. Com deslize a
carga percorre vários passos por comando e alcança células que o fecho reverso
simples marcaria como mortas — a poda estaria **descartando soluções reais**. Ela
fica desligada em fases com gelo ou esteira; custa desempenho e preserva a
correção da prova.

## Linguagem de layout

| Char | Significado |
|---|---|
| `~` | piso custoso (custo 2) — continua existindo, é outra mecânica |
| `%` | gelo |
| `^ v < >` | esteira, na direção da seta |

## Onde fica

| Camada | Arquivo |
|---|---|
| Dados | `Puzzle/PuzzleConveyorDefinition.cs`, campos em `PuzzleLevelDefinition` |
| Regra | `PuzzleBoardModel.Slide` |
| Prova | `Tools/puzzle/tw08_solver.py` (função `slide`) |
| Visual | `Editor/TW08PuzzleSceneBuilder.DrawBoard` |
| Testes | `Tests/EditMode/SlideMechanicsTests.cs` |

O visual distingue as duas camadas: gelo é mais claro e mais opaco que piso frio,
e a esteira mostra uma seta — sem indicador de direção a mecânica só seria
descoberta por tentativa e erro.

## Névoa de guerra (`dark_map`, `limited_vision`, `partial_map`)

Dois modos:

- **Lanterna** (`Flashlight`) — só o raio ao redor do operador aparece; o resto
  volta a escurecer. Usada em S05 Oficina Sem Luz.
- **Memória** (`Memory`) — o que já foi visto continua visível, mais apagado.
  É o mapa parcial de L26 Arquivo Morto e L27 Rota Fantasma.

**Não altera solvabilidade.** Esconder informação muda a dificuldade percebida,
não o que é possível — por isso as fases com névoa continuam valendo as mesmas
provas, sem reprova.

A escuridão é tinta nos renderizadores, e não objetos desligados: carga e alvo
precisam continuar recebendo suas animações mesmo fora do alcance da luz. O raio
usa distância de Chebyshev, e não Manhattan — a lanterna ilumina um quadrado, e
com losango os cantos ficariam escuros e o jogador leria isso como parede.

## Parede falsa (`fake_wall`)

Célula **livre** desenhada como parede até o operador chegar ao lado. A mentira é
só visual: no tabuleiro sempre foi passagem, e é por isso que estas fases também
não precisaram de reprova.

Regra que guiou a escolha das células: parede falsa vai onde o piso **já era
livre**. Transformar parede real em passagem abriria atalho novo e obrigaria a
reprovar a fase — nesse caso seria outro trabalho, não este.

Revela por proximidade, não ao atravessar: o jogador precisa poder descobrir o
segredo olhando, sem ter que tentar andar contra cada parede do setor. Reiniciar
a fase esconde de novo, porque a descoberta faz parte do desafio.

## Robô de limpeza (`turn_robot`, `timing_sync`)

Percorre uma rota fixa, **um passo por comando do jogador**, e volta ao início ao
terminar. É obstáculo sólido: nem o operador nem a carga podem terminar o
movimento onde o robô vai estar.

**A posição é função apenas do número de comandos.** Isso é o que mantém a fase
determinística e permite ao solver prová-la: bastam a rota e um contador, sem
simulação paralela. Toda validação usa a posição **futura** do robô — ele avança
junto com o comando, então validar contra a posição atual deixaria o jogador
terminar o passo dentro dele.

O estado do solver passou a carregar a fase do ciclo. Sem isso ele encontraria
"soluções" que dependem de o robô estar em dois lugares ao mesmo tempo.

Em L18 Robô de Limpeza a rota varre a coluna que liga as cargas aos alvos: vira
uma porta que abre e fecha sozinha. O primeiro traçado que testei patrulhava uma
faixa que a solução ótima nem usava — o custo ficou idêntico ao de antes, prova
de que o robô não estava atrapalhando nada. Movido para o corredor certo, o custo
subiu de 34 para 36.

## O que continua sendo só etiqueta

Estas tags descrevem o desenho da fase e não pedem código: `narrow_corridor`,
`three_rooms`, `false_route`, `move_order`, `precision`, `reverse_planning`,
`isolated_goal`, `marked_crate`, `tool_crate` e afins. Estão corretas como estão.

Só uma tag segue sendo etiqueta de propósito:

- `n8_jack` (L22) — o Macaco N-8 é ferramenta de loja, e a bíblia de design o
  exclui do MVP. A etiqueta está certa como está.

## A caixa-ferramenta, e por que ela não existia

`tool_crate` é o conceito central da bíblia: pôr a carga no sensor para abrir a
porta e depois **tirá-la de lá** para levá-la ao alvo de verdade.

Durante meses ela não aconteceu uma única vez. Ao desenhar as fases, várias specs
pediam mais alvos do que cargas — e o motor exige que os dois números batam. Minha
saída foi pôr um alvo em cima da célula do sensor. O número fechou e a mecânica
morreu: se o sensor também é alvo, a carga estaciona nele e nunca precisa sair.
Eram **23 de 23 sensores** assim.

Medido: remover a porta de todas as 19 fases com sensor muda o custo ótimo de
**uma** delas. Dezoito portas eram decorativas — o mesmo sintoma do robô, do botão
e do portão, em escala dezenove vezes maior.

A fase 06 (Portão de Peso) é a primeira com sensor que **não** é alvo. Trancar a
porta permanentemente a torna insolúvel, e o desvio até o sensor custa 4
movimentos reais. É recuperável sem reiniciar, o que importa num tutorial: o
jogador deve perder porque pensou errado, não porque o jogo o prendeu.

## Botão de direção (`direction_button`)

Pisar no botão **inverte todas as esteiras do tabuleiro**, e não só o trecho sob
os pés. O disparo acontece ao FIM do comando, sobre a célula onde o operador
parou: inverter no meio do deslize mudaria a correia enquanto a carga ainda a
percorre, e o resultado deixaria de ser legível.

Desfazer reaplica a mesma inversão. Como o efeito é derivado de onde o comando
terminou, não foi preciso guardá-lo no movimento — o `PuzzleMove` continua
intacto.

L17 foi redesenhada em torno dele: as correias apontam **para longe** dos alvos,
então empurrar carga no sentido padrão a devolve ao ponto de partida. A solução
ótima começa com sete passos direto ao botão, antes de qualquer empurrão.

## Portão temporizado (`temporary_block`)

Célula fechada até o turno atingir um número de comandos; a partir daí fica
aberta para sempre.

Modelado como **prazo, não como duração**, e é isso que mantém o estado finito: o
solver só precisa saber se o prazo já venceu. O relógio no estado é saturado no
maior prazo da fase — depois dele o tabuleiro não muda mais, então contar além
seria multiplicar estados à toa.

Na secreta 08 o portão fecha o corredor central, por onde tudo passa. É o
"planejamento reverso" que a fase declarava: arrumar o que der antes de o portão
abrir.

## O padrão que se repetiu quatro vezes

Robô, botão e portão tiveram a mesma história: **a primeira colocação não mudou o
custo ótimo**. Quando o número não se mexe, o gimmick está decorando a fase, não
participando dela — ele existe no dado e é irrelevante no jogo.

| Fase | Primeira tentativa | Depois de reposicionar |
|---|---|---|
| L18 robô | 34 (igual ao sem robô) | **36** |
| L17 botão | 19 (igual ao sem botão) | **20** |
| S08 portão | 39 (igual ao sem portão) | **41** |

O solver virou instrumento de crítica de design, e não só gate de
solvabilidade: ele diz quando uma mecânica é enfeite.
