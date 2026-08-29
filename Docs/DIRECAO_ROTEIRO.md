# The Warehouse Nº 08 — Direção de Roteiro

Avaliação crítica da escrita e da dramaturgia.

**Material lido:** `REFERENCIA/The_Warehouse_N8_Historia_Central.md` (869 linhas),
`Docs/NARRATIVE.md`, `Assets/_Project/Scripts/Editor/TW08NarrativeSetup.cs`
(10 sequências / 73 falas), `Docs/level-specs.json` (40 fases, campo `narrative`),
`Docs/Design/Characters/*.md`, `Assets/_Project/Scripts/Narrative/**`.

**Escopo:** só análise e proposta de texto. Nada foi editado fora deste arquivo.

---

## 0. Veredito

O roteiro implementado é **bom e está sub-utilizado**. Não há nada quebrado, nada
fora de tom, nada melodramático. O problema não é qualidade linha a linha — é
**volume de informação e distribuição**.

Três frases resumem o diagnóstico:

```text
O tom está certo. A dosagem está errada.
Robert está escrito. John está quase. Duda não está.
Elias não existe no jogo.
```

O jogador atravessa 30 fases e recebe **três fatos**: "não confie no painel"
(fase 01), "Robert abriu a porta" (fase 24) e "esconderam nas rotas" (fase 30).
Entre a fase 01 e a fase 24 — 23 fases, a maior parte da campanha — a narrativa
entrega **atmosfera e nenhuma informação nova**. As quatro cenas de setor do meio
dizem a mesma coisa quatro vezes: "o sistema está escondendo algo".

Isso não se resolve escrevendo mais. Resolve-se fazendo cada cena que já existe
**carregar um fato concreto** em vez de uma impressão. A cena do Setor 03 já faz
isso e é, por isso, a melhor cena do meio do jogo. Ela é o modelo.

---

## 1. As seis correções de maior impacto

Em ordem. Se só der para fazer as três primeiras, faça as três primeiras.

### 1.1 Elias precisa ser dito em voz alta — pelo menos uma vez

Elias não aparece em **nenhuma das 73 falas**. Não é nomeado. Não fala. Não é
lembrado.

Isso é a maior infidelidade à bíblia, que dedica a seção 7 inteira a ele e o
define como a prova de que *"o sistema apaga pessoas, não só dados"* (§12).

E o resto do projeto já contou com ele:

- `NarrativeOverlayController.cs:399` — `case "elias": return "Elias";` já existe.
  O falante está implementado e nunca é usado.
- `Docs/AUDIO_DESIGN.md` §16.3 — cinco SFX dele: `story_elias_record_found`,
  `story_elias_data_missing`, `story_elias_static`, `story_elias_last_log`,
  `secret_elias_truth`.
- Duas fases secretas levam o nome dele: **34 — Rota do Elias**,
  **39 — Último Turno do Elias**. Ambas sem uma linha de texto.

Hoje o mistério inteiro do "sistema apaga gente" roda em cima de um terminal que
diz `Operador anterior: inexistente`. Um slot anônimo. Ninguém chora por um slot.

**Correção mínima (uma fala):** nomear Elias no Setor 06. Ver §3.9.

**Correção completa:** dar duas falas a ele nas secretas 34 e 39. Ver §7.3.

### 1.2 O desfecho não revela nada — e pode revelar sem perder o mistério

O jogo é vendido como *"registros apagados e uma funcionária que sumiu da
escala"*. O desfecho entrega `Registro histórico: um arquivo recuperado` e corta.

O jogador termina sem saber **o que estava no arquivo**. Não é ambiguidade
produtiva — é ausência. A bíblia autoriza deixar o destino da Duda em aberto
(§7: *"Ele não precisa estar morto. Isso não precisa ser confirmado"*), mas
ambiguidade sobre o **destino** de uma pessoa exige clareza sobre o **fato**.

A correção é barata e fecha um arco que ainda nem foi aberto. Ver §1.3 e §3.10.

### 1.3 Plantar a escala do turno na abertura

Robert diz, na fase 24: *"Depois o nome dela sumiu da escala do turno."* É a
melhor imagem do roteiro — e chega sem nenhuma preparação. O jogador nunca viu
essa escala, nunca soube quantos nomes tinha.

Plante o objeto na abertura, pague na fase 24, feche no desfecho. Três toques no
mesmo objeto físico, custo total de seis falas, e o jogo inteiro passa a ter uma
espinha.

**Abertura (acrescentar depois de `"Já tá aberto..."`):**

```csharp
John("A escala do turno ainda está pregada na parede?"),
Robert("Está. Com dois nomes."),
Robert("O meu e o seu.")
```

**Fase 24** — já existe, a linha só passa a ter lastro.

**Desfecho** — a escala volta restaurada. Ver §3.10.

Isso também resolve, de graça, o problema apontado em §1.5: a abertura passa a
registrar a **ausência** de Duda sem nomeá-la, que é exatamente o que a bíblia
pede no Ato 1 (*"apresentar a ausência de Duda"*).

### 1.4 Duda repete a mesma frase quatro vezes

`Docs/NARRATIVE.md` afirma que as cenas *"não repetem"* o texto de
`level-specs.json`, *"exceto as três frases âncora"*. Na prática há **quatro
repetições além das âncoras**, e três delas são da Duda:

| Cena | Fala | Também está em |
|---|---|---|
| `narr-setor-s02` | "Se o sistema insiste demais em uma rota, pergunte por quê." | fase 08 (idêntica) |
| `narr-setor-s04` | "Se o sistema insiste em uma rota, pergunte por que ele quer tanto que você vá por ali." | fase 19 (idêntica) — **e é variação da de cima** |
| `narr-setor-s03` | "Nem toda carga parada está esperando destino. Algumas estão segurando caminho." | fase 06 (idêntica) |
| `narr-setor-s06` | "Origem: apagada. Destino: Setor 08. Operador anterior: inexistente." + reação do John | fases 26 e 28 (fatiadas) |

Efeito no jogador: a Duda vira um alerta de sistema que dispara sempre a mesma
mensagem. Some-se a isso que **quatro das nove falas dela usam a mesma estrutura
retórica** ("não X, Y" / "se X, então pergunte Y") e o resultado é uma
personagem sem gente dentro. Ver §5.3.

**Correção:** as cenas de setor ficam com texto exclusivo; as falas curtas
continuam nas fases. Substituições prontas em §3.4 e §3.6.

### 1.5 A confissão do Robert é entregue duas vezes — e a primeira estraga a segunda

A fase 24 (`TW08_Level24_OldGenerator`) tem, em `level-specs.json`:

> Robert: *"Eu abri aquela porta para ela, John. Achei que era só mais uma
> teimosia da Duda. Não era."*

E a cutscene `narr-robert-confissao`, que dispara **ao completar essa mesma
fase**, abre com a mesma confissão.

O jogador recebe a maior revelação do Ato 2 como legenda de briefing, empurra
caixas por cinco minutos, e então assiste à cena em que ela é revelada — já
sabendo. A cena vira redundância.

`NARRATIVE.md` classifica isso como "frase âncora" repetida de propósito. Para as
outras duas âncoras ("não confie no painel", "esconderam nas rotas") a lógica se
sustenta, porque elas são **mote**, não **revelação**. Esta é revelação. Não pode
ser pré-anunciada.

**Correção:** trocar a fala da fase 24 em `level-specs.json` por antecipação, não
por conteúdo:

```json
{ "speaker": "Robert", "line": "Religa esse gerador, John. Depois eu preciso te contar uma coisa." }
```

Agora a fase inteira é espera. E a cutscene entrega.

### 1.6 O conceito central da bíblia nunca é dito

A bíblia insiste, do §1 ao §20, na ideia que casa gênero e história:

```text
As caixas não estavam apenas fora do lugar. Elas formavam um mapa.
Cada puzzle reorganiza uma parte do mapa escondido.
```

**Nenhuma das 73 falas diz isso.** É a razão de o jogo ser um puzzle e não outra
coisa, e ela está ausente do roteiro.

A melhor formulação do projeto inteiro está num briefing, não numa cena —
fase 28: *"A ordem é a mensagem."* Excelente, e enterrada.

**Correção:** uma fala da Duda, na secreta 38 ("O Mapa que Sobrou") ou no Setor
06. Proposta em §7.3.

---

## 2. Fidelidade à bíblia

### 2.1 O que foi bem transposto

- A premissa, integralmente. O lockdown, o chamado noturno, o "só restaurar a
  operação".
- As três frases-âncora, palavra por palavra e nos lugares certos.
- O diálogo de abertura da bíblia (§10) — "Não recomendo elogiar" sobreviveu
  intacto, e é ótimo que tenha sobrevivido.
- O tom do §16, sem exceção: nenhuma fala em 73 escorrega para terror ou
  melodrama. Isso é disciplina real e merece registro.
- **Final 1 — Modo Manual**, escolhido como final único. Decisão correta: dos
  cinco finais da bíblia, é o único que não exige conteúdo que o jogo não tem.
- Robert como coração do elenco (§14: *"Não recomendo matar Robert"*) — respeitado.

### 2.2 O que ficou de fora e faz falta

**1. Elias.** Já tratado em §1.1. Falta crítica.

**2. A ruptura entre John e Duda.** A bíblia §8 é explícita e detalhada: John
gostava dela, demonstrava por atitudes, hesitou quando ela pediu ação, ela leu
como omissão, *"a relação dos dois quebrou antes do lockdown"*. A bíblia chama
isso de **"a ferida emocional do John"**.

No roteiro implementado isso existe em duas falas genéricas de arrependimento
("Tarde, mas estou" / "Demorei demais") **sem objeto**. O jogador não sabe que
houve algo entre os dois. Arrependimento sem causa conhecida não emociona: soa
como educação.

Basta uma fala do Robert — ele é quem pode dizer o que John não diz:

```csharp
Robert("Ela perguntou de você até o fim, sabia."),
Robert("Eu inventei que você estava ocupado. Não sei por que eu inventei.")
```

**3. "As caixas formam um mapa".** §1.6.

**4. O que havia no Setor 08.** A bíblia é concreta: *"cargas irregulares,
documentos internos e registros de falhas graves"*. No jogo o Setor 08 é um
destino sem conteúdo. O jogador chega e não descobre o que foi escondido.

**5. A empresa como antagonista.** Este é o desvio mais sutil e o mais perigoso.
A bíblia diz: *"alguém programou o sistema para esconder esses dados"*. O
antagonista são **pessoas** que usaram a automação. No roteiro implementado o
antagonista virou **o sistema** — ele "não tem coragem", "mente", "esquece",
"apaga". Personificado o tempo todo, sem nunca ter um dono.

Risco: o jogo lê como "IA má", que é exatamente o que a bíblia §16 evita
(*"crítica à automação irresponsável"* — irresponsável por parte de alguém).
Uma fala resolve, e ela já está quase escrita no Setor 04. Ver §3.6.

### 2.3 O que foi inventado

Nada contradiz a bíblia. A disciplina foi boa. E três invenções são **melhores
que o material de origem** — devem ser mantidas e ampliadas:

- **"Três e dez da manhã. Claro que sou eu."** Estabelece hora, cansaço, ironia e
  histórico profissional em oito palavras. Melhor abertura de personagem que a
  bíblia oferece.
- **O compressor rodando seis meses em setor vazio.** Uma pista física,
  verificável, que o jogador entende sozinho. É assim que se escreve mistério
  industrial. É o modelo para as outras cenas de setor.
- **"O sistema esqueceu de me apagar. Eu não reclamei."** Invenção que *melhora* a
  bíblia: coloca Robert no mesmo espectro de Elias e Duda — apagado, só que por
  sorte e não por castigo. Isso amarra o tema sem explicá-lo.

Única invenção que atrapalha: **"Onze minutos"** é excelente, mas a fase 24
acontece no Setor 05 e a bíblia coloca a virada do Ato 2 depois do acesso às
rotas antigas. É desvio menor e não vale corrigir.

---

## 3. Fala por fala

Legenda: **MANTER** (não encoste) · **AJUSTAR** (funciona, pode melhorar) ·
**REESCREVER** (não está funcionando).

---

### 3.1 `narr-abertura` — Turno de Emergência (9 falas)

Melhor sequência de abertura possível com o material disponível. Ritmo de rádio,
piada seca, nenhuma exposição.

> **MANTER** — `John("Três e dez da manhã. Claro que sou eu.")`
> `Robert("Temos algo parecido com energia. Não recomendo elogiar.")`
> `John("Me abre o Recebimento. Eu empurro o resto.")`

A terceira é a tese do personagem em sete palavras. Não encoste.

> **AJUSTAR** — as três falas de `Automacao` seguidas.

Funcionam como abertura fria de máquina, mas três caixas de texto antes de o
jogador tocar em qualquer coisa é o momento de maior risco de *skip* do jogo
inteiro. Funde as duas primeiras:

```csharp
Automacao("Armazém Nº 08. Falha operacional. Rota não autorizada."),
Automacao("Operador manual necessário."),
```

> **ACRESCENTAR** — a escala do turno, ao final. Ver §1.3.

**Falta estrutural:** a abertura não registra a ausência de Duda, que a bíblia
lista como objetivo narrativo do Ato 1. Consequência direta: quando ela fala pela
primeira vez (fase 01, ao completar), John diz `"Duda?"` em tom tenso e o jogador
não faz ideia de quem é nem por que isso é grave. A reação do protagonista fica
maior que o conhecimento do jogador — o defeito clássico. A correção da §1.3
resolve isso sem nomear ninguém.

---

### 3.2 `narr-setor-s01` — Recebimento (6 falas)

> **MANTER** — `John("Remanejando ou procurando?")` seguido de `Robert("...")` e
> da esquiva.

Isto é ofício. A reticência do Robert diz mais que qualquer resposta, e é
coerente com o que o documento da Duda estabelece: *"Ele faz piada quando está
com medo."* A piada que vem depois — *"Pergunta difícil a gente faz depois do
café"* — é o medo dele. Perfeito.

> **REESCREVER** — as duas primeiras falas do John:
> *"Recebimento. Cada caixa aqui devia ter uma etiqueta e um destino."*
> *"Metade não tem nem uma coisa nem outra."*

Dois problemas. O primeiro é nomear o setor em que o jogador está e que o HUD já
mostra — a fala não acrescenta nada. O segundo é que isso é **contabilidade**:
John descreve um estado em vez de ler um sinal. Um operador veterano não conta
etiquetas, ele repara em como a carga foi largada.

```csharp
John("Carga empilhada em pé de porta."),
John("Quem fez isso estava com pressa.", NarrativeTone.Seco),
John("Ou não tinha ninguém aqui pra fazer direito.")
```

Mesma informação, entregue como percepção. E a terceira fala é sinistra sem
sublinhar nada — o sistema fez aquilo, e o jogador chega nisso sozinho.

---

### 3.3 `narr-duda-primeira-mensagem` — Não confie no painel (6 falas)

Boa cena. A melhor virada de prólogo do roteiro.

> **MANTER** — `John("Descartada uma ova.")`

É a primeira vez que John desobedece o sistema, e está escrito como reflexo, não
como decisão. Exatamente certo.

> **AJUSTAR** — `Duda("Ele mostra o armazém que a empresa quer que exista.")`

Ensaístico demais para um recado gravado às pressas. É a **segunda fala da Duda
no jogo inteiro** e ela já está fazendo aforismo. Troca por uma imagem que
pertence ao John:

```csharp
Duda("Ele te mostra o mapa. Não te mostra o chão.")
```

Mais curta, mais dela (registros × realidade física), e planta o "chão" que o
John reivindica no Setor 06 e no desfecho.

> **ACRESCENTAR** — uma fala, entre `"Duda?"` e a descarte do sistema:

```csharp
John("Isso é gravado ou é agora?", NarrativeTone.Tenso),
Automacao("Mensagem sem origem. Descartada."),
```

Agora a fala do sistema é **a recusa de responder a pergunta que importa**. A
mesma linha, sem mudar uma vírgula, ganha uma função dramática. E instala a
dúvida que o jogo precisa carregar até o fim: ela gravou isso, ou ela está viva
em algum lugar do prédio?

---

### 3.4 `narr-setor-s02` — Expedição (7 falas)

> **MANTER** — `Robert("Ninguém, não. Você lê.")`

> **REESCREVER** — o bloco central:
> *"Ela deixou isso num terminal de expedição." / "Ninguém lê terminal de
> expedição." / "Agora eu leio."*

John está explicando a mecânica da trama em voz alta, sozinho, para o jogador.
Três falas para dizer "ela escondeu recado onde ninguém olha". E *"Agora eu
leio"* é tese, não fala.

Divide entre dois personagens e a exposição vira conversa:

```csharp
John("Ela deixou num terminal de expedição."),
Robert("Lugar que ninguém olha."),
John("Ela sabia que eu olho.")
```

Mesma informação, três falas, e a última carrega a relação em vez do tema — ela
conhecia os hábitos dele. É a primeira pista de que houve algo entre os dois, e
custa zero exposição.

> **REESCREVER** — `Duda("Se o sistema insiste demais em uma rota, pergunte por
> quê.")` — repetição idêntica da fase 08 (§1.4).

Esta cena precisa de um **fato**, não de um conselho. A doca B-12 é mencionada
mas nunca investigada. Faça dela a pista:

```csharp
John("B-12."),
John("A B-12 foi lacrada em dois mil e dezenove. Eu ajudei a lacrar.", NarrativeTone.Tenso)
```

Agora o jogador tem uma **anomalia concreta**: o sistema está mandando tudo para
uma doca que não existe mais. E quem sabe disso é o John, porque estava lá — o
que justifica, dentro da ficção, por que a empresa precisava dele e por que ele é
perigoso para ela.

---

### 3.5 `narr-setor-s03` — Câmara Fria (6 falas)

**A melhor cena do meio do jogo.** É o modelo que as outras três deviam seguir:
uma pista física, verificável, dita sem ênfase.

> **MANTER** — `John("Alguém está pagando conta de luz para congelar nada.")`

Melhor fala do roteiro. Raciocínio de quem trabalha, humor seco, e é uma pista.
Note o que ela faz: não diz "isso é estranho", diz um **fato absurdo** e deixa o
jogador concluir. Toda cena de setor devia terminar assim.

> **MANTER** — `Robert("Câmara fria. Veste o casaco e não confia no chão.")`

> **AJUSTAR** — *"Tem uma coisa esquisita aqui, John. O compressor roda em setor
> vazio faz seis meses."*

A primeira metade avisa o jogador de que a segunda metade é estranha. Confia no
fato:

```csharp
Robert("O compressor roda faz seis meses, John."),
Robert("Em setor vazio.")
```

> **REESCREVER** — `Duda("Nem toda carga parada está esperando destino...")` —
> repetição idêntica da fase 06 (§1.4). Ver substituições em §5.3.

---

### 3.6 `narr-setor-s04` — Automação (6 falas)

> **MANTER** — `John("Noventa e nove vírgula quatro. E o armazém inteiro
> travado.")`

O número por extenso é uma boa escolha: é John lendo em voz alta, com desprezo.

> **AJUSTAR** — `John("Rodam vazias para quem olha o mapa.")`

Boa, mas obscura no lugar errado — o jogador ainda não tem contexto para
decodificar. Deixa explícito o que ele viu:

```csharp
John("Vazias no mapa. Alguém devia conferir se estão vazias no chão.")
```

> **REESCREVER** — `Duda("Se o sistema insiste em uma rota, pergunte por que ele
> quer tanto que você vá por ali.")`

Repetição da fase 19 **e** variação da própria fala do Setor 02. É a repetição
mais grave do roteiro. Use canon não aproveitado do documento da Duda, que já é
melhor e é específico deste setor:

```csharp
Duda("Erro não se repete com esse capricho. Isso não é falha, é padrão.")
```

E acrescente aqui a pista física que falta ao setor — está na bíblia §5, não
usada: *"Empilhadeiras autônomas evitavam corredores sem motivo."*

```csharp
Robert("As autônomas nunca entram no corredor C. Contornam. Sempre contornaram."),
John("Máquina não tem medo. Máquina tem instrução.", NarrativeTone.Tenso)
```

A segunda fala resolve, de uma vez, o problema apontado em §2.2 item 5: o
antagonista deixa de ser o sistema e passa a ser **quem escreveu a instrução**.
Uma fala.

> **REESCREVER** — `John("Estou perguntando, Duda. Tarde, mas estou.")`

Antecipa o *"Demorei demais"* do desfecho. Duas declarações do mesmo
arrependimento gastam a segunda, que é a que importa. Aqui John ainda não admite
nada — ele desvia, que é o personagem da bíblia §8. Troque por afeto disfarçado
de observação:

```csharp
John("Padrão. Ela falava essa palavra que nem quem reza.")
```

---

### 3.7 `narr-setor-s05` — Manutenção Pesada (6 falas)

**A melhor sequência do roteiro.** Não há uma linha ruim.

> **MANTER TUDO, especialmente:**
> `Robert("Oficina N-8. Cuidado com o degrau — ele nunca foi automatizado.")`
> `Robert("Desde o lockdown. O sistema esqueceu de me apagar. Eu não reclamei.")`
> `Robert("Pega a chave inglesa, John. A partir daqui a gente abre no braço.")`

A piada do degrau é perfeita: mecânica, seca, e é o tema do jogo inteiro dito
como aviso de segurança. "Eu não reclamei" é devastador por understatement.

> **AJUSTAR** — `John("As máquinas velhas ainda andam?")`

Única fala fraca da cena: existe só para o Robert responder. Dá intenção a ela:

```csharp
John("Sobrou alguma coisa aqui que ande sem pedir licença ao sistema?")
```

> **ACRESCENTAR** — Robert acabou de dizer que está preso ali embaixo desde o
> lockdown e John não reage. O silêncio é bom (são dois homens que não falam
> disso), mas silêncio absoluto lê como indiferença. John cuida por logística —
> é o que a bíblia §8 estabelece. Duas falas:

```csharp
John("Você comeu alguma coisa nesses dias?"),
Robert("Comi. Não pergunta o quê.")
```

Piada, afeto e caracterização dupla em duas linhas. E é a piada que o Robert faz
porque está com medo.

---

### 3.8 `narr-robert-confissao` — A porta que Robert abriu (9 falas)

**A melhor cena dramática do jogo.** Estrutura correta, emoção contida, nenhuma
palavra a mais.

> **MANTER, e não deixe ninguém mexer:**
> `Robert("Desliga o gerador um minuto, John. Eu falo melhor no escuro.")`
> `John("Quanto tempo depois?")`
> `Robert("Onze minutos.")`
> `Robert("Eu carrego peso, John. É o que eu faço.")` / `John("Esse não. Esse a gente divide.")`

A primeira fala dá marcação de cena, motivação e personagem em nove palavras — e
justifica dramaticamente um blackout que o jogo pode usar de verdade.

*"Quanto tempo depois?"* é a melhor escolha de reação do roteiro inteiro: John
não pergunta o que sentiu, pergunta **um horário**. É um operador processando
tragédia como registro de ocorrência. Isso é personagem.

*"Onze minutos"* — a precisão é o que dói. Número redondo teria matado a fala.

> **AJUSTAR** — `Robert("Como se ela nunca tivesse batido ponto aqui.")`

Explica a fala anterior. *"O nome dela sumiu da escala do turno"* já disse tudo,
e disse melhor. Troca a explicação por uma imagem:

```csharp
Robert("Eu fui olhar no quadro. Tinha um espaço em branco onde era o nome dela.")
```

Com a plantação da §1.3, esta fala passa a ser a segunda batida no mesmo objeto,
e o jogador sente o buraco antes de entender.

> **CORRIGIR FORA DA CENA** — a fala duplicada em `level-specs.json`, fase 24.
> Ver §1.5. É o problema mais grave desta cena e ele não está nesta cena.

---

### 3.9 `narr-setor-s06` — Rotas Fantasma (7 falas)

> **MANTER** — `John("O mapa oficial mente desde a primeira caixa.")`

> **REESCREVER** — `John("Ler chão é a única coisa que eu sei fazer direito.")`

Autopiedade. John não se explica e não se diminui — ele é orgulhoso do ofício,
essa é a dignidade do personagem. E a fala também é redundante: o jogador passou
26 fases vendo que ele lê o chão.

```csharp
John("Chão eu leio. Foi pra isso que me chamaram.")
```

Mesma ideia, com orgulho e com ironia — porque chamaram para restaurar a
operação, não para achar a verdade, e ele vai usar exatamente a habilidade
contratada contra quem contratou.

> **REESCREVER** — `John("Inexistente. Eles estão apagando gente agora.")`

A tese dita em voz alta. E "gente" no abstrato não comove ninguém. **Este é o
lugar do Elias** (§1.1):

```csharp
Terminal("Origem: apagada. Destino: Setor 08. Operador anterior: inexistente."),
John("Inexistente."),
John("Tinha um Elias nesse turno. Cauteloso. Anotava tudo à mão.", NarrativeTone.Tenso),
John("Agora ele é um campo vazio.", NarrativeTone.Seco)
```

Quatro falas em vez de duas, e a diferença é que agora existe uma **pessoa** onde
antes havia um conceito. "Anotava tudo à mão" caracteriza Elias exatamente como a
bíblia pede (*"cauteloso, metódico e bom com registros antigos"*) e prepara o
material das secretas 34 e 39 — papel não se apaga remotamente.

> **NOTA** — o Terminal e a reação do John duplicam as falas das fases 26 e 28
> (§1.4). Com a reescrita acima a cena passa a ter texto próprio e a duplicação
> se resolve sozinha.

---

### 3.10 `narr-desfecho-nucleo` — Núcleo Logístico (11 falas)

> **MANTER** — `John("Modo manual.")` / `Automacao("Comando não reconhecido.")` /
> `John("Vai reconhecer.")`

Fecho perfeito. Três falas, um confronto, nenhuma explicação. Não se toca.

> **CORTAR** — `John("Neste armazém, a partir de hoje, quem move carga é gente.")`

É o único momento verdadeiramente melodramático do roteiro: um discurso, para
ninguém, sobre o tema do jogo. E o pior — ele **enfraquece o "Vai reconhecer"**,
porque gasta a retórica antes do golpe.

Corte inteiro. `"Modo manual."` seguido de `"Comando não reconhecido."` seguido
de `"Vai reconhecer."` é mais forte sem intermediário. Confie no que já está
escrito.

> **AJUSTAR** — `Duda("Se você chegou até aqui, é porque leu o armazém inteiro.")`

Parabeniza o jogador por terminar o jogo. Quebra a ficção e é vazio — ela não
teria como saber. Substitua pelo canon do documento dela, que é melhor e tem
farpa:

```csharp
Duda("Eu sabia que você ia demorar."),
Duda("Você nunca deixou um turno pela metade. Nem quando devia.")
```

"Nem quando devia" carrega a briga inteira dos dois em quatro palavras: ele fica,
mas fica tarde e fica calado.

> **ACRESCENTAR — a correção mais importante do desfecho.** Ver §1.2.

O `"arquivo recuperado"` precisa ter conteúdo. E o conteúdo certo é a escala,
fechando o objeto plantado na abertura:

```csharp
Automacao("Núcleo logístico restaurado. Rotas consolidadas."),
Automacao("Registro histórico recuperado: escala do turno da noite."),
Terminal("Hayes, R.  —  Miller, J.  —  Rocha, M. E.  —  Elias."),
John("Quatro nomes."),
John("De manhã tinha dois.", NarrativeTone.Tenso),
```

Isto entrega **um fato concreto** — o sistema apagou pessoas e John as devolveu —
sem afirmar que Duda está viva. A ambiguidade se mantém exatamente onde a bíblia
quer (§7), porque um nome numa escala não é prova de vida. E o jogador sai com
uma imagem, não com um resumo.

O desfecho reordenado fica assim:

```csharp
Automacao("Núcleo logístico restaurado. Rotas consolidadas."),
Automacao("Registro histórico recuperado: escala do turno da noite."),
Terminal("Hayes, R.  —  Miller, J.  —  Rocha, M. E.  —  Elias."),
John("Quatro nomes."),
John("De manhã tinha dois.", NarrativeTone.Tenso),
Duda("Eles não esconderam os dados em arquivos. Esconderam nas rotas."),
Duda("Eu sabia que você ia demorar."),
Duda("Você nunca deixou um turno pela metade. Nem quando devia."),
John("Demorei, Duda. Demorei demais.", NarrativeTone.Tenso),
Robert("A oficina está com energia, John. O que você quiser abrir daqui, abre."),
John("Modo manual."),
Automacao("Comando não reconhecido."),
John("Vai reconhecer.", NarrativeTone.Seco)
```

Treze falas em vez de onze, e o jogo passa a terminar com uma revelação em vez de
uma declaração.

---

## 4. Ritmo

### 4.1 Mapa real de disparo

| Fase | Setor | Sequência | Falas |
|---|---|---|---|
| 01 | S01 | `narr-abertura` + `narr-setor-s01` (enfileiradas) | 15 |
| 01 | S01 | `narr-duda-primeira-mensagem` (ao completar) | 6 |
| 06 | S02 | `narr-setor-s02` | 7 |
| 11 | S03 | `narr-setor-s03` | 6 |
| 16 | S04 | `narr-setor-s04` | 6 |
| 21 | S05 | `narr-setor-s05` | 6 |
| 24 | S05 | `narr-robert-confissao` (ao completar) | 9 |
| 26 | S06 | `narr-setor-s06` | 7 |
| 30 | S06 | `narr-desfecho-nucleo` | 11 |

### 4.2 Diagnóstico

**Concentração excessiva na fase 01.** 21 das 73 falas — **29% do roteiro** —
estão na primeira fase. Quinze delas *antes do primeiro empurrão de caixa*. É o
ponto de maior risco de abandono do jogo, e é onde há mais texto. A correção da
§3.1 (fundir duas falas de sistema) alivia pouco; o alívio real vem de mover a
entrada do Setor 01 para a **fase 02**, o que o sistema já permite sem código
novo: basta o `sectorId` casar com a segunda fase que o jogador abrir. Fica:
abertura na 01, Setor 01 na 02. Nove falas de recepção em vez de quinze.

**A cadência do meio está correta.** Uma cena a cada cinco fases (06, 11, 16, 21)
é bem espaçada. O problema **não é o intervalo**, é o conteúdo — ver §6.

**Três buracos reais**, e todos têm a mesma assinatura: fases sem cena *e* sem
fala curta em `level-specs.json`.

| Buraco | Fases | Gravidade |
|---|---|---|
| Câmara Fria | 12, 13, 14 | Alta — três fases seguidas em silêncio absoluto |
| Automação | 17, 18 e 20 | Média |
| Pré-clímax | 23, 25, **29** | **Alta** — ver abaixo |

**O clímax é mudo.** A fase **29 — Lockdown N-8** tem título dramático, briefing
de urgência (*"portões fecham"*) e **zero texto**. Depois da confissão do Robert
(24), o jogo passa cinco fases sem dizer nada e depois entrega o final. A subida
para o desfecho deveria apertar, não emudecer. É o slot vazio mais caro do
projeto.

**O Ato 3 tem menos cena que o Ato 1, e devia ter mais.** A bíblia separa três
setores no Ato 3 (Setor 06 Arquivo Morto, Setor 07 Rotas Fantasma, Setor 08
Núcleo Logístico). A implementação fundiu os três em **um único S06** rotulado
`"Rotas Fantasma / Setor 08"`, cobrindo as fases 26–30.

Consequência dramatúrgica: o Ato 1 recebe **duas** cenas de setor para 10 fases;
o Ato 3 recebe **uma** para 5 fases — justamente onde toda a revelação acontece.
O ritmo está invertido em relação à curva de tensão.

**Correção sem tocar em código:** o enum `NarrativeTriggerKind.LevelStart` existe
(`NarrativeContext.cs:18`), está testado, e **não é usado por nenhuma sequência**.
Dois `SequenceSpec` novos com `LevelStart` nas fases 28 e 29 preenchem o Ato 3 e
o clímax sem nenhuma linha de sistema nova. Falas prontas em §7.2.

### 4.3 Distribuição recomendada

| Ato | Hoje | Proposto |
|---|---|---|
| Ato 1 (01–10) | 28 falas | 22 (abertura aliviada, S01 movido p/ fase 02) |
| Ato 2 (11–25) | 21 falas | 27 (S03/S04/S05 com fatos + duas falas no S05) |
| Ato 3 (26–30) | 24 falas | 38 (S06 ampliado, LevelStart 28 e 29, desfecho) |

O objetivo não é inflar. É inverter a curva: hoje o jogo fala mais no começo e
menos no fim.

---

## 5. Vozes

Teste aplicado: cobri o nome do falante em cada uma das 73 falas e tentei
identificá-lo pelo texto.

**Resultado: Robert, sempre. John, quase sempre. Duda, nunca — porque ela só tem
um registro.**

### 5.1 Robert — a voz mais bem escrita do jogo

Identificável em 100% das falas. O que o define, concretamente:

- **Fala no imperativo.** "Empurra a caixa", "Veste o casaco", "Pega a chave
  inglesa", "Desliga o gerador", "Corre, John". Ele é o único que dá ordens, e é o
  personagem sem autoridade formal — é o veterano, não o chefe.
- **Só menciona objetos.** Degrau, compressor, chave inglesa, gerador, porta,
  quadro. Nunca menciona um conceito. Nem uma vez em 73 falas. Isso é
  consistência de personagem de altíssimo nível e provavelmente foi deliberado.
- **Usa piada como escudo.** *"Pergunta difícil a gente faz depois do café"* vem
  logo depois do único "..." do roteiro. O documento da Duda já tinha
  diagnosticado: *"Ele faz piada quando está com medo."* O roteiro obedece sem
  nunca explicar.
- **Antropomorfiza máquina, nunca pessoa.** O sistema "não teve coragem", as
  máquinas "não aprenderam a mentir", o degrau "nunca foi automatizado".

Uma única exceção, e é boa: *"São as únicas aqui que não aprenderam a mentir."* É
a permissão poética dele, e funciona porque vem logo depois de uma piada.

### 5.2 John — bem definido, mas invadido pelo autor

Identificável em cerca de 80%. O que o define:

- **Declarativas curtas, sem subordinada.** "Modo manual." "Quatro nomes."
  "Inexistente."
- **Números lidos em voz alta com desprezo.** "Três e dez da manhã", "Noventa e
  nove vírgula quatro", "Quanto tempo depois?"
- **Recusa como reflexo.** "Descartada uma ova." "Vai reconhecer."
- **Pergunta que é acusação.** "Remanejando ou procurando?"

**O defeito:** em cinco momentos ele para de ser operador e vira narrador
temático. *"O mapa oficial mente desde a primeira caixa"*, *"Ler chão é a única
coisa que eu sei fazer direito"*, *"Neste armazém, a partir de hoje, quem move
carga é gente"*, *"Rodam vazias para quem olha o mapa"*, *"Eles estão apagando
gente agora"*.

Todas são frases boas. Nenhuma é dele. **A regra prática:** John descreve o que
vê e recusa o que lhe mandam. Ele **não** formula o tema do jogo — quem formula é
Duda, porque ela é a analista, e essa divisão é justamente o que a bíblia §3
estabelece (*"John entendia as rotas. Duda entendia os registros."*).

Sempre que John explicar o jogo, a fala é da Duda ou não é de ninguém.

### 5.3 Duda — o problema central do roteiro

Identificável em 0% — não porque soe como os outros, mas porque **soa como um
sistema de dicas**.

As nove falas dela, na íntegra:

```text
1. "John, se você está ouvindo isso, não confie no painel principal."
2. "Ele mostra o armazém que a empresa quer que exista."
3. "Se o sistema insiste demais em uma rota, pergunte por quê."
4. "Nem toda carga parada está esperando destino. Algumas estão segurando caminho."
5. "Se o sistema insiste em uma rota, pergunte por que ele quer tanto que você vá por ali."
6. "Não deixei nomes nos arquivos. Deixei nas rotas."
7. "Eles não esconderam os dados em arquivos. Esconderam nas rotas."
8. "Se você chegou até aqui, é porque leu o armazém inteiro."
9. "Eu sabia que você ia demorar. Também sabia que ia chegar."
```

Quatro observações duras:

1. **Quatro das nove usam a mesma figura**: antítese "não X, Y" (4, 6, 7) ou
   condicional-imperativa "se X, pergunte Y" (3, 5, 8). Uma pessoa não fala assim
   nove vezes seguidas. Um oráculo, sim.
2. **6 e 7 são a mesma frase.** "Não deixei nomes nos arquivos, deixei nas rotas"
   e "não esconderam em arquivos, esconderam nas rotas". Mesma estrutura, mesmo
   par de substantivos, com quatro fases de distância.
3. **3 e 5 são a mesma frase**, e ambas se repetem em `level-specs.json` (§1.4).
4. **Ela nunca menciona um objeto concreto.** Nem um. Robert só fala de objetos;
   Duda só fala de abstrações. Nenhum dos dois extremos é humano, mas o do Robert
   funciona porque ele é físico por definição. O da Duda não funciona porque a
   bíblia a chama de **"coração emocional da história"** (§12) — e ela é, no
   estado atual, a personagem menos humana do elenco.

E há um problema de credibilidade além do estilo: as gravações dela são todas
**perfeitamente compostas e perfeitamente cronometradas** à posição do John. Isso
a transforma em oráculo onisciente e apaga o fato dramático mais importante dela
— ela gravou aquilo **com medo, com pressa, sem saber se alguém ouviria**.

**O que caracteriza a Duda (e está no documento dela, sem uso):**

- **Impaciência afetuosa.** *"Não tenta resolver tudo sozinho de novo."*
- **Mágoa direta.** *"Eu não precisava que você dissesse tudo. Só precisava que
  você ficasse."*
- **Veredito técnico, seco.** *"Não é bug. É encobrimento."*
- **Intimidade operacional.** *"Você sempre soube quando uma caixa estava no lugar
  errado. Agora usa isso."*

Nenhuma dessas quatro está no jogo. Todas são melhores que as nove implementadas.

**Substituições prontas** (uma por cena, elimina as repetições da §1.4):

Setor 03 — Câmara Fria, no lugar da fala 4:
```csharp
Duda("Eu contei as caixas dessa câmara três vezes. Sobra uma. Sempre sobrou uma.")
```

Setor 04 — Automação, no lugar da fala 5:
```csharp
Duda("Erro não se repete com esse capricho. Isso não é falha, é padrão.")
```

Setor 06 — Rotas Fantasma, mantendo a fala 6 (é âncora), acrescentar antes:
```csharp
Duda("Eu tenho pouco tempo e a bateria desse terminal é uma piada, então escuta.")
```

E, em qualquer ponto do Ato 2, **uma gravação que soa gravação**:
```csharp
Duda("— não, isso aqui não vai gravar direito. John. John, se isso chegar em você:"),
Duda("não tenta resolver tudo sozinho de novo. Não dessa vez.")
```

Essa última faz mais pela personagem que as outras oito somadas: ela erra, ela se
corrige, ela tem pressa, ela conhece um defeito dele e diz na cara. Vira gente.

### 5.4 Sistema e Terminal — corretos

Boa decisão mantê-los fora do `CharacterRoster` (documentada em `NARRATIVE.md`
§4). São vozes sem rosto e o overlay resolve por *fallback*. O `Sistema` fala em
sujeito oculto e presente do indicativo; o `Terminal` fala em campos rotulados
(`Origem:`, `Destino:`). A distinção é sutil e está sendo respeitada. Manter.

---

## 6. A revelação

### 6.1 Está entregue cedo demais?

**Não.** A contenção é real e correta. Em nenhum momento alguém explica o
encobrimento antes da hora. O Setor 08 é mencionado como destino em terminais
antes de ser lugar, que é a ordem certa. Não há vilão declarado. Não há
monólogo explicativo. Isso é acerto de direção e deve ser preservado em qualquer
mudança.

### 6.2 O jogador entende o suficiente para se importar?

**Não.** E este é o problema, na direção oposta.

Curva de informação real, fase a fase:

```text
Fase 01  →  "não confie no painel"          (mistério aberto)
Fases 06–21  →  nada                        (quatro cenas, zero fatos novos)
Fase 24  →  "Robert abriu a porta"          (primeiro fato de verdade)
Fase 26  →  "apagaram um operador"          (segundo fato)
Fase 30  →  "esconderam nas rotas"          (terceiro fato)
```

**Vinte fases sem uma única informação nova.** Não é reserva narrativa — é
espera. E o jogador não distingue as duas: para ele, "estão me segurando" e "não
tem nada aqui" são a mesma experiência.

O que essas quatro cenas do meio *fazem* é repetir uma impressão — "o sistema
está escondendo algo" — em quatro embalagens. A impressão já tinha sido entregue
na fase 01.

### 6.3 A prescrição: um fato por cena de setor

Não escreva mais cenas. Faça as cenas existentes **carregarem um fato
verificável**, como a do Setor 03 já faz. É uma troca de fala por cena, não uma
adição de cena.

| Cena | Impressão atual | Fato proposto |
|---|---|---|
| S02 Expedição | "o sistema insiste numa rota" | **A doca B-12 foi lacrada em 2019 — e John ajudou a lacrar.** (§3.4) |
| S03 Câmara Fria | *(já tem fato)* | **O compressor roda há seis meses em setor vazio.** Manter. |
| S04 Automação | "o sistema insiste numa rota" | **As empilhadeiras autônomas contornam o corredor C. Sempre contornaram.** (§3.6) |
| S05 Manutenção | *(tem fato pessoal: Robert preso)* | Manter. Acrescentar o mapa de papel: ver abaixo. |
| S06 Rotas Fantasma | "estão apagando gente" | **Elias existiu, anotava tudo à mão, virou campo vazio.** (§3.9) |

Complemento para o S05, usando canon não aproveitado da bíblia (§7, "mapas
manuais"):

```csharp
Robert("Tem um mapa de papel pregado atrás da bancada. Do tempo em que eu entrei."),
Robert("Ele tem um setor a mais que o mapa do computador.", NarrativeTone.Tenso)
```

Com isso o jogador chega na fase 24 tendo acumulado **quatro anomalias
concretas** em vez de quatro impressões. A confissão do Robert deixa de ser a
primeira revelação e passa a ser a **confirmação** de algo que ele já suspeitava.
Essa é a diferença entre um jogador que se importa e um jogador que espera.

### 6.4 O que ainda falta ser respondido

Mesmo com as correções acima, o jogo termina sem responder:

- **O que havia no Setor 08?** A bíblia é concreta (cargas irregulares,
  documentos, registros de falhas graves). O jogo nunca diz. Cabe numa fala de
  Terminal no desfecho ou na secreta 37.
- **Quem apagou os nomes?** §2.2 item 5. Uma fala resolve (§3.6).
- **A Duda está viva?** Deve continuar sem resposta — é a escolha certa. Mas
  precisa ser uma **pergunta formulada**, não uma lacuna. A fala
  `John("Isso é gravado ou é agora?")` proposta em §3.3 formula a pergunta na
  fase 01 e deixa ela aberta por 30 fases. É de graça e muda tudo.

---

## 7. O que falta escrever

### 7.1 Preencher os buracos de fase (`level-specs.json`, campo `narrative`)

Uma fala por fase, no formato que já existe. Prioridade para os três buracos da
§4.2. **Não editei o arquivo** — segue pronto para quem o mantém.

**Câmara Fria — o buraco mais grave (três fases mudas):**

```json
12: { "speaker": "Robert", "line": "Esse corredor nunca teve luz decente. Nem quando tinha orçamento." }
13: { "speaker": "Sistema", "line": "Carga perecível. Prazo de validade: expirado há quatro meses." }
13: { "speaker": "John",    "line": "Estão refrigerando lixo com muito capricho." }
14: { "speaker": "John",    "line": "Sensor congelado. Ou congelaram ele." }
```

**Automação:**

```json
17: { "speaker": "Robert", "line": "Botão de inverter direção. Instalaram e nunca explicaram pra quê." }
18: { "speaker": "John",   "line": "Ele limpa o mesmo corredor a noite inteira. Deve ser o mais honesto daqui." }
20: { "speaker": "Duda",   "line": "A linha nunca para. É assim que ninguém repara no que passa nela." }
```

**Manutenção Pesada e pré-clímax:**

```json
23: { "speaker": "Robert", "line": "N-8 Heavy. Lenta, feia, e nunca me deixou na mão." }
25: { "speaker": "John",   "line": "Peso morto é o que sobra quando ninguém quer assinar a retirada." }
29: { "speaker": "Sistema", "line": "Protocolo de contenção. Portões em quinze minutos." }
29: { "speaker": "Robert",  "line": "Corre, John. Eu seguro o que der deste lado." }
```

### 7.2 Duas sequências novas para o Ato 3 (`LevelStart`, sem código novo)

O gatilho `NarrativeTriggerKind.LevelStart` existe, está testado e nunca foi
usado. Estes dois `SequenceSpec` entram direto em `BuildScript()`.

**Fase 28 — Carga Sem Origem.** Aqui vive o conceito central da bíblia (§1.6),
que hoje não é dito em lugar nenhum:

```csharp
new SequenceSpec(
    "NARR_10_L28_OrdemDaMensagem",
    "narr-ordem-da-mensagem",
    "A ordem é a mensagem",
    NarrativeTriggerKind.LevelStart,
    "S06",
    "TW08_Level28_CargoWithoutOrigin",
    new[]
    {
        John("Quatro caixas, quatro alvos. Qualquer ordem fecha."),
        John("Só que não é qualquer ordem.", NarrativeTone.Tenso),
        Duda("Eu não desenhei mapa nenhum, John."),
        Duda("Eu só fui arrumando as caixas até ele aparecer."),
        John("Ela escreveu no chão."),
        John("Trinta anos empurrando carga e eu nunca li nada aqui.", NarrativeTone.Seco)
    }),
```

**Fase 29 — Lockdown N-8.** O clímax mudo (§4.2):

```csharp
new SequenceSpec(
    "NARR_11_L29_Lockdown",
    "narr-lockdown",
    "Contenção",
    NarrativeTriggerKind.LevelStart,
    "S06",
    "TW08_Level29_LockdownN8",
    new[]
    {
        Automacao("Protocolo de contenção iniciado. Setor 08 em isolamento."),
        Automacao("Todo pessoal deve evacuar."),
        Robert("Que pessoal? Somos dois."),
        John("Quanto tempo até os portões?"),
        Robert("Quinze minutos. Doze, se ele estiver mentindo. E ele mente."),
        John("Fica na oficina, Rob."),
        Robert("Fico. Alguém tem que segurar a porta que eu abri.", NarrativeTone.Tenso)
    },
    priority: 5),
```

A última fala do Robert fecha o arco dele: a porta que ele abriu para Duda é a
mesma que ele agora segura para John. Custo: uma fala.

### 7.3 As dez fases secretas — hoje 100% mudas

As fases 31–40 não têm **uma linha** de narrativa, e quatro delas levam nomes de
personagens: *Sala do Robert*, *Turno da Duda*, *Rota do Elias*, *Último Turno do
Elias*, *O Caminho da Duda*.

Este é o lugar exato dos *"logs opcionais"* e *"terminais antigos"* que a bíblia
pede (§15), e o único lugar onde Elias pode falar sem inchar a campanha
principal. Recompensa narrativa para quem procura — que é precisamente o que uma
fase secreta deve pagar.

```json
31 Caixa Fora do Registro
   { "speaker": "Terminal", "line": "Item sem registro. Sem peso. Sem origem." }
   { "speaker": "John",     "line": "Existe. Está bem na minha frente." }

32 Sala do Robert
   { "speaker": "Robert", "line": "Não mexe em nada aqui. Ou mexe. Já mexeram antes de você." }

33 Turno da Duda
   { "speaker": "Duda", "line": "Se você abriu essa sala, é porque contou os passos. Você sempre contou." }

34 Rota do Elias
   { "speaker": "Elias", "line": "Terceiro turno. Anotei tudo à mão." }
   { "speaker": "Elias", "line": "Papel eles não conseguem apagar de longe." }

35 Oficina Sem Luz
   { "speaker": "Robert", "line": "No escuro eu ando melhor que você. Conheço isso aqui de cor." }

36 Empilhadeira Fantasma
   { "speaker": "Sistema", "line": "Veículo 08 em operação. Operador: não identificado." }
   { "speaker": "John",    "line": "Não tem ninguém nela." }

37 08-B
   { "speaker": "Terminal", "line": "Rota 08-B. Carga sem origem. Status: entregue." }
   { "speaker": "John",     "line": "Entregue para quem?" }

38 O Mapa que Sobrou
   { "speaker": "Duda", "line": "Toda caixa fora do lugar é uma letra. Você só precisa de paciência." }

39 Último Turno do Elias
   { "speaker": "Elias", "line": "Se alguém ler isso: eu estava certo." }
   { "speaker": "Elias", "line": "E isso não me deixa nem um pouco feliz." }

40 O Caminho da Duda
   { "speaker": "Duda", "line": "Sem dica, sem atalho, sem Power Up." }
   { "speaker": "Duda", "line": "Do jeito que eu deixei. Do jeito que você entende." }
```

Note que a 34 e a 39 dão a Elias exatamente **quatro falas no jogo inteiro** — e
elas bastam. A bíblia diz que ele *"não precisa aparecer muito"*. Quatro falas,
duas fases secretas, e um nome dito pelo John no Setor 06 e no desfecho. É o
suficiente para o sumiço dele pesar.

Note também que os SFX correspondentes já estão especificados em
`AUDIO_DESIGN.md` §16.3 (`story_elias_last_log`, `secret_elias_truth`) — o áudio
foi projetado para falas que ninguém escreveu.

---

## 8. Resumo de ações, por impacto

| # | Ação | Onde | Custo | Impacto |
|---|---|---|---|---|
| 1 | Nomear Elias (Setor 06 + secretas 34/39 + desfecho) | `TW08NarrativeSetup.cs`, `level-specs.json` | 8 falas | **Crítico** |
| 2 | Dar conteúdo ao arquivo recuperado no desfecho | `TW08NarrativeSetup.cs` | 4 falas | **Crítico** |
| 3 | Plantar a escala do turno na abertura | `TW08NarrativeSetup.cs` | 3 falas | **Crítico** |
| 4 | Trocar a fala da fase 24 (spoiler da confissão) | `level-specs.json` | 1 fala | Alto |
| 5 | Eliminar as 4 repetições da Duda | `TW08NarrativeSetup.cs` | 4 trocas | Alto |
| 6 | Um fato concreto por cena de setor (S02, S04) | `TW08NarrativeSetup.cs` | 6 falas | Alto |
| 7 | Cortar o discurso do desfecho | `TW08NarrativeSetup.cs` | −1 fala | Alto |
| 8 | Duas sequências `LevelStart` (fases 28 e 29) | `TW08NarrativeSetup.cs` | 13 falas | Alto |
| 9 | Preencher fases 12, 13, 14, 17, 18, 20, 23, 25, 29 | `level-specs.json` | 11 falas | Médio |
| 10 | Escrever as 10 fases secretas | `level-specs.json` | 16 falas | Médio |
| 11 | Reescrever as falas-tese do John (5 ocorrências) | `TW08NarrativeSetup.cs` | 5 trocas | Médio |
| 12 | Mover a entrada do Setor 01 para a fase 02 | `TW08NarrativeSetup.cs` | config | Médio |
| 13 | Uma gravação da Duda que soa gravação | `TW08NarrativeSetup.cs` | 2 falas | Médio |

Total proposto: **73 → ~120 falas**, com a maior parte do crescimento no Ato 3 e
nas secretas, e com o Ato 1 ficando **mais curto** do que é hoje.

---

## 9. O que está bom e não deve ser tocado

Registro explícito, porque revisão que só aponta defeito faz o time reescrever o
que já estava certo.

- **A sequência inteira do Setor 05.** Não há uma linha ruim. É o padrão de
  qualidade do projeto.
- **A confissão do Robert**, exceto pela fala explicativa apontada em §3.8.
  *"Desliga o gerador um minuto, John. Eu falo melhor no escuro."* e *"Onze
  minutos."* são escrita de primeira linha.
- **`"Vai reconhecer."`** como última fala do jogo.
- **`"Alguém está pagando conta de luz para congelar nada."`** — a melhor fala do
  roteiro, e o modelo de como escrever uma pista.
- **`"Remanejando ou procurando?"`** seguido do `"..."` do Robert.
- **A voz do Robert por inteiro**, e a regra tácita de que ele só fala de objetos.
- **A decisão de deixar `sistema` e `terminal` fora do roster.** Vozes sem rosto.
- **A escolha do Final 1 (Modo Manual) como final único.**
- **A contenção geral.** Nenhuma fala em 73 explica o tabuleiro, nenhuma explica o
  tema antes da hora, nenhuma escorrega para melodrama. Isso é difícil e está
  feito.

O roteiro que existe é sólido. Ele só não é suficiente ainda — e o que falta é
menos do que parece.
