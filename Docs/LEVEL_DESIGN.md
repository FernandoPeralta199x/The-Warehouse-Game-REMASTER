# The Warehouse Nº 08 — Plano Robusto de Fases, Fases Secretas e Forklift Shift Races

> Documento de level design para **The Warehouse Nº 08**.  
> Estrutura com **30 fases principais** e **10 fases secretas**, integrando puzzle em grade, narrativa, Oficina N-8, Power Ups e o modo **Forklift Shift Races**.

---

## 1. Veredito de design

[Certeza] As fases não podem ser apenas “mais caixas em salas menores”. Para o puzzle ser atrativo, cada fase precisa ter uma intenção clara:

```text
ensinar uma ideia;
testar uma ideia;
combinar duas ideias;
enganar o jogador com uma rota falsa;
exigir planejamento;
recompensar leitura inteligente.
```

A campanha deve alternar:

```text
Puzzle puro
Puzzle com ferramentas
Puzzle narrativo
Corrida de empilhadeira
Entrega logística
Fase híbrida puzzle + empilhadeira
Fase secreta de raciocínio extremo
```

A regra central do jogo deve ser:

```text
O jogador deve perder porque pensou errado, não porque o jogo foi injusto.
```

---

## 2. Direção geral das fases

A campanha principal terá:

```text
30 fases principais
10 fases secretas
6 setores principais
8 fases com empilhadeira na campanha principal
4 fases híbridas puzzle + empilhadeira
3 corridas secretas de empilhadeira
5 fases mega difíceis
1 fase secreta extrema final
```

A experiência ideal:

```text
Fases 01–05: ensinar fundamentos.
Fases 06–10: introduzir sensores, docas e primeira empilhadeira.
Fases 11–15: adicionar gelo, carga frágil e controle de risco.
Fases 16–20: adicionar automação, esteiras e rotas falsas.
Fases 21–25: adicionar manutenção pesada, ferramentas e N-8 Heavy.
Fases 26–30: domínio total, rotas fantasma, Setor 08 e final.
Secretas 01–10: desafios especiais, difíceis e narrativos.
```

---

## 3. Tipos de fase

## 3.1 Puzzle puro

Fase focada em empurrar caixas, alvos, paredes, sensores, esteiras e ordem correta.

Uso:

```text
ensinar lógica base;
criar raciocínio espacial;
preparar o jogador para fases mais complexas.
```

---

## 3.2 Puzzle narrativo

Fase onde as posições das caixas, terminais e mensagens da Duda revelam parte da história.

Uso:

```text
ligar gameplay com mistério;
mostrar que as caixas formam mensagens;
dar contexto ao Setor 08.
```

---

## 3.3 Forklift Shift Race

Fase de empilhadeira com tempo, rota, carga, dano e medalhas.

Uso:

```text
variar o ritmo;
dar utilidade às empilhadeiras;
recompensar precisão e domínio de direção.
```

---

## 3.4 Fase híbrida

Combina puzzle em grade com empilhadeira.

Exemplo:

```text
John resolve um puzzle para abrir a garagem.
Depois pilota a empilhadeira para transportar uma carga.
Depois volta ao puzzle para ativar sensores finais.
```

Uso:

```text
clímax de setor;
missões especiais;
desbloqueio da Oficina N-8;
final do jogo.
```

---

## 4. Mecânicas principais

## 4.1 Mecânicas de puzzle

```text
Caixa normal
Caixa pesada
Caixa frágil
Caixa marcada
Caixa falsa
Caixa de bloqueio
Alvo simples
Alvo colorido
Alvo ordenado
Sensor de peso
Porta automática
Porta manual
Esteira
Piso escorregadio
Botão temporizado
Terminal
Rota falsa
Mapa incompleto
Setor escuro
Robô por turno
Área sem retorno
Caixa usada como ferramenta
```

---

## 4.2 Mecânicas de empilhadeira

```text
Aceleração controlada
Freio/ré
Curva previsível
Carga com peso
Carga frágil
Carga instável
Carga refrigerada
Carga pesada
Dano por colisão
Estabilidade de carga
Zona de coleta
Zona de entrega
Checkpoints
Atalhos arriscados
Portões temporizados
Sensores de carga
Piso escorregadio
Rota de emergência
Estacionamento final
```

---

## 5. Escala de dificuldade

```text
Fácil:
ensina mecânica e quase não pune.

Média:
exige ordem simples e atenção.

Difícil:
pune empurrão errado e exige planejamento.

Muito difícil:
combina mecânicas e exige leitura completa da sala.

Mega difícil:
exige ordem específica, uso de caixas como ferramentas, rotas falsas e previsão de vários passos.

Extrema:
fase secreta, sem Power Ups, sem dicas diretas, feita para jogadores avançados.
```

---

## 6. Regras para fases difíceis e mega difíceis

[Certeza] Dificuldade boa não é excesso de elemento. É consequência lógica.

Use estas regras:

## 6.1 Ordem obrigatória

A fase tem várias caixas, mas apenas uma ordem resolve.

Exemplo:

```text
Caixa C abre caminho.
Caixa A vira bloqueio.
Caixa D precisa esperar.
Caixa B vai para o alvo só no final.
```

---

## 6.2 Caixa-ferramenta

Uma caixa não serve apenas para alvo. Ela pode:

```text
segurar porta;
bloquear esteira;
parar outra caixa no gelo;
ativar sensor;
criar parede temporária;
abrir rota de retorno;
bloquear robô;
liberar corredor.
```

---

## 6.3 Rota falsa

O caminho mais óbvio leva a deadlock.

O jogador precisa desconfiar antes.

---

## 6.4 Reversão planejada

O jogador precisa mover uma caixa para longe do alvo para depois conseguir posicioná-la corretamente.

---

## 6.5 Informação narrativa como pista

Uma fala de Duda ou Robert pode sugerir a lógica.

Exemplo:

```text
Duda:
“Nem toda carga parada está esperando destino. Algumas estão segurando caminho.”
```

Isso sugere que uma caixa deve ficar em sensor ou bloqueio temporário.

---

## 7. Estrutura da campanha

```text
Setor 01 — Recebimento
Setor 02 — Expedição
Setor 03 — Câmara Fria
Setor 04 — Automação
Setor 05 — Manutenção Pesada
Setor 06 — Rotas Fantasma / Setor 08
```

Cada setor terá 5 fases principais:

```text
4 fases de puzzle/híbridas
1 fase de empilhadeira ou clímax especial
```

Nem todo setor segue a mesma fórmula, para evitar monotonia.

---

# 8. Fases principais — 30 fases

---

# SETOR 01 — RECEBIMENTO

Tema: fundamentos, movimentação, primeira pista de Duda.  
Tom: início do turno, luzes de emergência, armazém ainda “compreensível”.

---

## Fase 01 — Primeiro Turno

```text
Tipo: Puzzle puro
Dificuldade: Fácil
Mecânicas: caixa normal, alvo simples, parede
Personagem em foco: John
Power Ups: bloqueados
```

### Objetivo

Ensinar o jogador a empurrar caixas para alvos.

### Modelo de fase

```text
1 caixa
1 alvo
corredores largos
sem risco real de travamento
terminal no final
```

### Ideia de puzzle

O jogador aprende:

```text
andar;
empurrar;
não puxar;
colocar caixa no alvo;
concluir fase.
```

### Narrativa

```text
Sistema: “Operador manual detectado.”
John: “Ótimo. Até o sistema sabe que precisa de gente.”
```

### Medalhas

```text
Bronze: concluir
Prata: concluir abaixo de 25 movimentos
Ouro: concluir abaixo de 18 movimentos
Platina: concluir abaixo de 14 movimentos sem undo
```

---

## Fase 02 — Corredor Apertado

```text
Tipo: Puzzle puro
Dificuldade: Fácil/Média
Mecânicas: corredor estreito, caixa normal, alvo simples
Personagem em foco: John
```

### Objetivo

Ensinar que caixa não pode ser puxada.

### Modelo de fase

```text
2 caixas
2 alvos
corredor em L
uma caixa pode ser empurrada para canto sem volta
```

### Ideia de puzzle

O jogador precisa mover primeiro a caixa mais distante.

### Erro comum

Empurrar a caixa da entrada cedo demais e bloquear o corredor.

### Dica indireta

```text
John: “Se entrou apertado, sai pior. Melhor pensar antes.”
```

---

## Fase 03 — Carga Cruzada

```text
Tipo: Puzzle puro
Dificuldade: Média
Mecânicas: 3 caixas, layout em cruz, área de manobra
```

### Objetivo

Ensinar ordem de movimentação.

### Modelo de fase

```text
3 caixas
3 alvos
layout em cruz
a caixa central bloqueia a sala se for movida cedo
```

### Solução conceitual

```text
1. Criar espaço lateral.
2. Mover caixas externas.
3. Resolver caixa central por último.
```

### Critério de qualidade

A fase deve parecer simples, mas punir pressa.

---

## Fase 04 — Etiqueta Errada

```text
Tipo: Puzzle narrativo
Dificuldade: Média
Mecânicas: caixa marcada, alvo especial, terminal
Personagem em foco: Duda
```

### Objetivo

Introduzir pistas de Duda.

### Modelo de fase

```text
2 caixas normais
1 caixa marcada N-8
1 alvo especial
1 terminal bloqueado atrás de caixas
```

### Lógica

A caixa marcada não deve ir para o alvo mais próximo.

Ela deve primeiro abrir passagem para o terminal.

### Mensagem de Duda

```text
“Se uma etiqueta parece fora do lugar, talvez ela esteja exatamente onde deveria.”
```

### Recompensa

Primeiro log parcial de Duda.

---

## Fase 05 — Doca Inicial

```text
Tipo: Puzzle de setor
Dificuldade: Média/Difícil
Mecânicas: 4 caixas, 4 alvos, duas rotas, deadlock leve
```

### Objetivo

Primeira fase com planejamento real.

### Modelo de fase

```text
4 caixas
4 alvos
duas rotas aparentes
rota curta prende uma caixa
rota longa resolve
```

### Conceito

Nem sempre o caminho mais curto é o correto.

### Narrativa

```text
Robert: “Doca limpa demais sempre me deixa desconfiado.”
```

---

# SETOR 02 — EXPEDIÇÃO

Tema: sensores, docas, ordem de saída, primeira empilhadeira.  
Tom: armazém começa a mostrar que o sistema está conduzindo o jogador para rotas erradas.

---

## Fase 06 — Portão de Peso

```text
Tipo: Puzzle com sensor
Dificuldade: Média
Mecânicas: sensor de peso, porta, caixa-ferramenta
```

### Objetivo

Ensinar sensor de peso.

### Modelo de fase

```text
1 sensor
1 porta
3 caixas
2 alvos finais
1 caixa precisa ficar temporariamente no sensor
```

### Ideia de puzzle

O jogador precisa aceitar que uma caixa não vai direto ao alvo.

Ela primeiro serve como ferramenta.

### Dica de Duda

```text
“Nem toda carga parada está esperando destino. Algumas estão segurando caminho.”
```

---

## Fase 07 — Ordem de Saída

```text
Tipo: Puzzle puro
Dificuldade: Difícil
Mecânicas: fila de caixas, corredor único, alvos laterais
```

### Objetivo

Ensinar ordem inversa.

### Modelo de fase

```text
3 caixas em linha
3 alvos laterais
corredor único
sem espaço para retorno se errar
```

### Solução conceitual

```text
1. Tirar a última caixa da fila.
2. Abrir área de manobra.
3. Entregar as caixas da frente por último.
```

### Erro comum

Empurrar a primeira caixa direto para frente.

---

## Fase 08 — Doca B-12

```text
Tipo: Puzzle narrativo
Dificuldade: Difícil
Mecânicas: rota falsa, terminal enganoso, caixas normais
Personagem em foco: Duda
```

### Objetivo

Introduzir sistema mentindo.

### Modelo de fase

```text
1 rota aberta e óbvia
1 rota longa e desconfortável
rota óbvia leva a deadlock
terminal do sistema sugere rota errada
mensagem de Duda contradiz sistema
```

### Mensagem do sistema

```text
“Rota ideal: B-12.”
```

### Mensagem de Duda

```text
“Se o sistema insiste demais em uma rota, pergunte por quê.”
```

---

## Fase 09 — Três Paletes

```text
Tipo: Puzzle espacial
Dificuldade: Difícil
Mecânicas: 3 caixas, alvos em canto, espaço pequeno
```

### Objetivo

Ensinar criação de área de manobra.

### Modelo de fase

```text
3 caixas
3 alvos no canto superior
área central pequena
um corredor lateral serve como estacionamento temporário
```

### Conceito

O jogador precisa preparar a sala antes de resolver.

---

## Fase 10 — Licença de Operador Classe C

```text
Tipo: Forklift Shift Race
Dificuldade: Média
Mecânicas: empilhadeira Standard, caixa média, cones, doca A
Personagem em foco: Robert
```

### Objetivo

Primeira fase de empilhadeira.

### Modelo de fase

```text
pegar 1 caixa
contornar cones
frear em zona marcada
entregar na doca A
```

### Critério de design

[Certeza] Essa fase não deve ser difícil. Ela deve validar se dirigir a empilhadeira é divertido.

### Falas

```text
Robert: “Antes de correr, aprende a parar. Metade dos operadores ruins só descobre isso na parede.”
```

### Medalhas

```text
Bronze: completar
Prata: abaixo de 1:30
Ouro: abaixo de 1:10
Platina: abaixo de 1:00 sem colisão
```

---

# SETOR 03 — CÂMARA FRIA

Tema: gelo, carga refrigerada, escorregamento, precisão.  
Tom: ambiente frio, luz azulada, falhas nos sensores.

---

## Fase 11 — Piso Gelado

```text
Tipo: Puzzle com gelo
Dificuldade: Média/Difícil
Mecânicas: piso escorregadio, caixa normal, parede-freio
```

### Objetivo

Ensinar deslizamento.

### Modelo de fase

```text
2 caixas
2 alvos
áreas de gelo em linha reta
paredes usadas como freio
```

### Conceito

O jogador precisa pensar onde a caixa vai parar, não só para onde ela vai.

---

## Fase 12 — Frio no Corredor

```text
Tipo: Puzzle com gelo
Dificuldade: Difícil
Mecânicas: gelo, corredor estreito, alvo lateral
```

### Objetivo

Combinar gelo e falta de espaço.

### Modelo de fase

```text
2 caixas em piso gelado
2 alvos fora do gelo
1 corredor lateral para reposicionamento
```

### Erro comum

Empurrar caixa direto para parede errada e perder espaço de manobra.

---

## Fase 13 — Carga Refrigerada

```text
Tipo: Forklift Shift Race
Dificuldade: Difícil
Mecânicas: empilhadeira Cold Storage, carga refrigerada, piso escorregadio, tempo
```

### Objetivo

Entregar carga refrigerada antes do tempo.

### Modelo de fase

```text
1 carga refrigerada
piso escorregadio parcial
curvas amplas
zona de entrega fria
atalho arriscado
```

### Pontuação

```text
Tempo
Dano da carga
Colisões
Estabilidade
```

### Medalhas

```text
Bronze: entregar
Prata: entregar abaixo de 2:00
Ouro: entregar abaixo de 1:35 com dano abaixo de 10%
Platina: entregar abaixo de 1:20 sem colisão
```

---

## Fase 14 — Sensor Congelado

```text
Tipo: Puzzle com sensor e gelo
Dificuldade: Muito difícil
Mecânicas: sensor de peso, gelo, porta, caixa-ferramenta
```

### Objetivo

Usar caixa como bloqueio antes de usar como entrega.

### Modelo de fase

```text
3 caixas
2 alvos
1 sensor
1 porta
gelo entre sensor e porta
```

### Solução conceitual

```text
1. Usar caixa A para parar caixa B no gelo.
2. Colocar caixa C no sensor.
3. Abrir porta.
4. Retirar caixa A do bloqueio.
5. Entregar caixas finais.
```

---

## Fase 15 — Câmara 08-C

```text
Tipo: Puzzle de setor
Dificuldade: Muito difícil
Mecânicas: gelo, sensor, porta, 4 caixas, alvo isolado
Personagem em foco: Duda
```

### Objetivo

Primeira fase realmente cerebral.

### Modelo de fase

```text
4 caixas
4 alvos
1 sensor
1 porta
1 alvo isolado atrás de porta
gelo em duas linhas
```

### Conceito

Resolver em três camadas:

```text
abrir porta;
criar área de manobra;
entregar caixas na ordem correta.
```

### Mensagem de Duda

```text
“Se o caminho escorrega demais, pare de empurrar força. Use o que já está parado.”
```

---

# SETOR 04 — AUTOMAÇÃO

Tema: esteiras, robôs, sensores falsos, sistema enganando o jogador.  
Tom: máquinas repetindo rotas, luzes de alerta, sistema insistindo em caminhos errados.

---

## Fase 16 — Esteira Ligada

```text
Tipo: Puzzle com esteira
Dificuldade: Média/Difícil
Mecânicas: esteira, caixa-ferramenta, alvo simples
```

### Objetivo

Introduzir esteiras.

### Modelo de fase

```text
2 caixas
2 alvos
1 esteira
1 caixa precisa bloquear fim da esteira
```

### Conceito

Usar movimento automático a favor.

---

## Fase 17 — Rota Automática

```text
Tipo: Puzzle com esteira múltipla
Dificuldade: Difícil
Mecânicas: duas esteiras, botão de direção, 3 caixas
```

### Objetivo

Alternar esteira na hora certa.

### Modelo de fase

```text
3 caixas
3 alvos
2 esteiras
1 botão alterna direção
1 corredor de retorno
```

### Erro comum

Alternar direção cedo demais e perder uma caixa.

---

## Fase 18 — Robô de Limpeza

```text
Tipo: Puzzle com obstáculo por turno
Dificuldade: Difícil
Mecânicas: robô por turno, caixas, sincronização
```

### Objetivo

Usar timing sem virar jogo de reflexo.

### Modelo de fase

```text
1 robô com rota fixa
3 caixas
3 alvos
robô bloqueia corredor a cada 3 turnos
```

### Regra

O robô se move por turno, não em tempo real.

### Conceito

Planejar sequência considerando posição futura do robô.

---

## Fase 19 — Sensor Falso

```text
Tipo: Puzzle narrativo
Dificuldade: Muito difícil
Mecânicas: sensor falso, porta correta, rota enganosa, terminal
```

### Objetivo

Mostrar que o sistema tenta enganar.

### Modelo de fase

```text
2 sensores
1 sensor abre porta correta
1 sensor fecha passagem crítica
terminal recomenda sensor errado
```

### Mensagem de Duda

```text
“Se o sistema insiste em uma rota, pergunte por que ele quer tanto que você vá por ali.”
```

---

## Fase 20 — Linha de Produção

```text
Tipo: Fase híbrida leve
Dificuldade: Muito difícil
Mecânicas: esteiras, sensor, porta temporizada, empilhadeira curta
```

### Objetivo

Combinar puzzle com uma micro-entrega de empilhadeira.

### Modelo de fase

```text
5 caixas
4 alvos
1 caixa deve ficar no sensor
2 esteiras
1 porta temporizada
1 empilhadeira usada para mover carga entre duas docas
```

### Conceito

Aceitar que nem toda caixa vai para alvo. Algumas viram ferramentas.

### Medalhas

```text
Bronze: concluir
Prata: concluir sem reiniciar
Ouro: concluir sem Power Ups
Platina: concluir sem Power Ups e sem dano na carga
```

---

# SETOR 05 — MANUTENÇÃO PESADA

Tema: Oficina N-8, Robert, caixas pesadas, N-8 Heavy, ferramentas e culpa.  
Tom: máquinas antigas, geradores, caminhos manuais esquecidos.

---

## Fase 21 — Oficina Travada

```text
Tipo: Puzzle de desbloqueio
Dificuldade: Difícil
Mecânicas: caixa pesada, porta manual, corredor bloqueado
Personagem em foco: Robert
```

### Objetivo

Desbloquear parcialmente a Oficina N-8.

### Modelo de fase

```text
1 caixa pesada
3 caixas normais
2 alvos
1 porta manual
área central bloqueada
```

### Falas

```text
Robert: “Essa porta sempre travou. A diferença é que antes ela tinha vergonha.”
```

---

## Fase 22 — Macaco N-8

```text
Tipo: Puzzle tutorial de ferramenta
Dificuldade: Difícil
Mecânicas: Macaco N-8, caixa presa, ranking assistido
```

### Objetivo

Ensinar uso do Macaco N-8.

### Modelo de fase

```text
1 caixa obrigatoriamente presa
1 uso controlado do Macaco N-8
2 caixas normais
2 alvos
```

### Regra

Usar Macaco N-8 desativa ranking limpo.

### Mensagem de tutorial

```text
Ferramenta pesada usada.
Tentativa marcada como Assistida.
```

---

## Fase 23 — Carga Pesada

```text
Tipo: Forklift Shift Race
Dificuldade: Muito difícil
Mecânicas: N-8 Heavy, carga pesada, rampa, dano, curva lenta
Personagem em foco: Robert
```

### Objetivo

Usar empilhadeira pesada para entregar carga lenta e crítica.

### Modelo de fase

```text
1 carga pesada
1 rampa
2 curvas fechadas
1 portão temporizado
1 zona de entrega elevada
```

### Medalhas

```text
Bronze: entregar
Prata: entregar abaixo de 2:20
Ouro: entregar abaixo de 1:55 com dano abaixo de 5%
Platina: entregar abaixo de 1:40 sem colisão
```

---

## Fase 24 — Gerador Antigo

```text
Tipo: Fase híbrida puzzle + empilhadeira
Dificuldade: Muito difícil
Mecânicas: bateria pesada, painéis manuais, porta travada, empilhadeira Heavy
Personagem em foco: Robert
```

### Objetivo

Religar gerador antigo da Oficina N-8.

### Modelo de fase

```text
mover caixas para liberar acesso
levar bateria pesada com empilhadeira
ativar 3 painéis manuais
usar caixa para travar porta
proteger Robert enquanto ele conserta o gerador
```

### Virada narrativa

Robert confessa:

```text
“Eu abri aquela porta para ela, John. Achei que era só mais uma teimosia da Duda. Não era.”
```

### Recompensas

```text
Oficina N-8 completa
N-8 Heavy
Macaco N-8
Rotas manuais antigas
```

---

## Fase 25 — Peso Morto

```text
Tipo: Puzzle mega difícil
Dificuldade: Mega difícil
Mecânicas: caixas pesadas, sensores, corredor central, caixa-ferramenta
```

### Objetivo

Resolver um puzzle pesado de ordem e espaço.

### Modelo de fase

```text
5 caixas
2 caixas pesadas
3 alvos comuns
2 sensores
1 corredor central
1 área de manobra limitada
```

### Conceito

Usar caixas pesadas como âncoras temporárias.

### Solução conceitual

```text
1. Caixa pesada A segura sensor 1.
2. Caixa normal B abre rota lateral.
3. Caixa pesada C deve sair do corredor antes do alvo final.
4. Caixa normal D trava a porta por 3 movimentos.
5. Caixa A sai do sensor apenas depois da entrega 3.
```

### Restrição recomendada

```text
Power Ups fortes bloqueados.
Scanner permitido.
Macaco N-8 bloqueado para medalha ouro.
```

---

# SETOR 06 — ROTAS FANTASMA / SETOR 08

Tema: mapa incompleto, rotas falsas, final, Duda, Setor 08.  
Tom: tensão, mistério, conclusão emocional.

---

## Fase 26 — Arquivo Morto

```text
Tipo: Puzzle narrativo
Dificuldade: Muito difícil
Mecânicas: mapa parcial, terminal, paredes falsas, caixas marcadas
```

### Objetivo

Introduzir setores apagados.

### Modelo de fase

```text
partes do mapa começam ocultas
terminais revelam áreas
algumas paredes oficiais não existem fisicamente
2 caixas marcadas indicam rota
```

### Mensagem

```text
Terminal:
“Operador anterior: inexistente.”

John:
“Inexistente, claro. Eles estão apagando gente agora.”
```

---

## Fase 27 — Rota Fantasma

```text
Tipo: Puzzle mega difícil
Dificuldade: Mega difícil
Mecânicas: mapa incompleto, memória espacial, caixas marcadas
Personagem em foco: Duda
```

### Objetivo

Resolver com informação incompleta.

### Modelo de fase

```text
jogador vê apenas parte da fase
algumas áreas aparecem após ativar sensores
caixas marcadas indicam caminho
```

### Mensagem de Duda

```text
“Se o mapa falhar, lembra do chão.”
```

### Regra

A fase deve ser difícil, mas não injusta. A iluminação e marcas no chão precisam guiar o jogador.

---

## Fase 28 — Carga Sem Origem

```text
Tipo: Puzzle mega difícil narrativo
Dificuldade: Mega difícil
Mecânicas: caixas marcadas, alvos ordenados, rota falsa, terminal final
```

### Objetivo

Descobrir que a ordem das caixas importa.

### Modelo de fase

```text
4 caixas marcadas
4 alvos
ordem correta importa
colocar caixas em alvos errados abre caminho falso
```

### Conceito

Não basta colocar qualquer caixa em qualquer alvo.

### Mensagem de Duda

```text
“Não deixei nomes nos arquivos. Deixei nas rotas.”
```

---

## Fase 29 — Lockdown N-8

```text
Tipo: Fase híbrida final
Dificuldade: Mega difícil
Mecânicas: puzzle + empilhadeira + sensores + corrida curta
```

### Objetivo

Abrir caminho para o Setor 08.

### Modelo de fase

```text
resolver puzzle para abrir garagem
usar empilhadeira para transportar carga pesada
voltar ao puzzle para ativar sensores
entrega de emergência com timer
evitar portões fechando
```

### Conceito

Clímax mecânico antes do final.

### Medalhas

```text
Bronze: concluir
Prata: concluir com menos de 3 colisões
Ouro: concluir sem Power Ups e com carga acima de 80%
Platina: concluir sem Power Ups, sem colisão e abaixo do tempo ideal
```

---

## Fase 30 — Núcleo Logístico

```text
Tipo: Puzzle final
Dificuldade: Mega difícil/final
Mecânicas: caixas, sensores, portas, esteiras, rota falsa, terminal final
Personagem em foco: John, Duda, Robert
```

### Objetivo

Revelar a verdade do Setor 08.

### Modelo de fase

```text
6 caixas
6 alvos
2 sensores
2 portas
2 esteiras
1 rota falsa
1 terminal final
1 área de retorno limitada
```

### Conceito

Todas as mecânicas aparecem, mas organizadas em blocos, não em bagunça.

### Mensagem final de Duda

```text
“Eles não esconderam os dados em arquivos. Esconderam nas rotas.”
```

### Resultado

Libera final conforme:

```text
logs encontrados;
fases secretas concluídas;
Robert salvo na Oficina;
corridas completadas;
uso de Power Ups no Setor 08;
registros de Elias recuperados.
```

---

# 9. Fases secretas — 10 fases

As fases secretas devem ter três funções:

```text
testar domínio real;
entregar história opcional;
desbloquear finais, skins ou recursos.
```

---

## Secreta 01 — Caixa Fora do Registro

```text
Tipo: Puzzle secreto
Dificuldade: Difícil
Desbloqueio: encontrar 3 etiquetas N-8 escondidas
```

### Modelo

```text
1 caixa extra sem alvo aparente
3 caixas normais
parede falsa
terminal opcional
```

### Conceito

A caixa extra não é erro. Ela abre uma parede falsa.

### Recompensa

Log oculto de Duda.

---

## Secreta 02 — Sala do Robert

```text
Tipo: Puzzle secreto de manutenção
Dificuldade: Difícil
Desbloqueio: concluir Gerador Antigo
```

### Modelo

```text
caixas pesadas
portas manuais
ferramentas antigas
corredor apertado
```

### Recompensa

```text
Skin Robert “Velha Guarda”
Upgrade: Bateria Reserva
```

---

## Secreta 03 — Turno da Duda

```text
Tipo: Puzzle lógico narrativo
Dificuldade: Muito difícil
Desbloqueio: ler todos os logs do Setor 03
```

### Modelo

```text
poucas caixas
muitas mensagens
ordem deduzida por texto
alvos não óbvios
```

### Regra

Resolver pela interpretação das mensagens.

### Recompensa

Mensagem emocional de Duda para John.

---

## Secreta 04 — Rota do Elias

```text
Tipo: Puzzle de caminho único
Dificuldade: Muito difícil
Desbloqueio: encontrar 4 registros apagados
```

### Modelo

```text
3 salas conectadas
erro na sala 1 bloqueia sala 3
caixas usadas como pontes lógicas
```

### Recompensa

Último acesso de Elias.

---

## Secreta 05 — Oficina Sem Luz

```text
Tipo: Puzzle em baixa visibilidade
Dificuldade: Difícil
Desbloqueio: completar uma fase de setor sem usar Scanner
```

### Modelo

```text
visão limitada
mapa escuro
caixas aparecem apenas próximas ao jogador
som/ícones ajudam localização
```

### Conceito

Memória e cautela.

### Recompensa

Skin John “Operador Noturno”.

---

## Secreta 06 — Empilhadeira Fantasma

```text
Tipo: Forklift Shift Race secreta
Dificuldade: Muito difícil
Desbloqueio: ouro em todas as corridas normais
```

### Modelo

```text
pista escura
rota muda durante a corrida
carga frágil
sem colisão para platina
```

### Recompensa

Skin N-8 Prototype.

---

## Secreta 07 — 08-B

```text
Tipo: Puzzle compacto extremo
Dificuldade: Mega difícil
Desbloqueio: colocar caixas marcadas em ordem correta em fases anteriores
```

### Modelo

```text
fase pequena
4 caixas
4 alvos
área de manobra mínima
cada movimento importa
```

### Conceito

Precisão absoluta.

### Recompensa

Código parcial do Setor 08.

---

## Secreta 08 — O Mapa que Sobrou

```text
Tipo: Puzzle mega difícil
Dificuldade: Mega difícil
Desbloqueio: completar Setor 06 sem Power Ups
```

### Modelo

```text
layout parece impossível
caixa deve virar bloqueio temporário
depois precisa ser libertada por outra rota
```

### Recompensa

Final alternativo parcial.

---

## Secreta 09 — Último Turno do Elias

```text
Tipo: Puzzle longo de três salas
Dificuldade: Mega difícil
Desbloqueio: encontrar todos os registros de Elias
```

### Modelo

```text
3 salas conectadas
erro na sala 1 só aparece na sala 3
2 caixas marcadas
1 sensor permanente
1 rota falsa
```

### Conceito

Planejamento longo.

### Recompensa

Verdade sobre Elias.

---

## Secreta 10 — O Caminho da Duda

```text
Tipo: Fase secreta extrema
Dificuldade: Extrema
Desbloqueio: concluir todas as fases secretas anteriores
```

### Modelo

```text
sem dicas
sem Power Ups
caixas marcadas
sensores
esteiras
gelo
mapa incompleto
terminal final
trecho curto de empilhadeira
```

### Objetivo

Reconstruir a rota deixada por Duda.

### Conceito

A fase final secreta testa:

```text
raciocínio espacial;
memória;
leitura narrativa;
domínio de esteiras;
uso de caixa-ferramenta;
controle de empilhadeira;
planejamento de ordem.
```

### Recompensa

Final secreto ou cena emocional com John e Duda.

---

# 10. Fases de empilhadeira — distribuição final

## Fases de empilhadeira na campanha principal

```text
Fase 10 — Licença de Operador Classe C
Fase 13 — Carga Refrigerada
Fase 20 — Linha de Produção
Fase 23 — Carga Pesada
Fase 24 — Gerador Antigo
Fase 29 — Lockdown N-8
```

## Fases com empilhadeira sugeridas como variação dentro de fases híbridas

```text
Fase 20 — micro-entrega entre esteiras
Fase 24 — transporte de bateria pesada
Fase 29 — entrega de emergência
Fase 30 — possível trecho curto para abrir núcleo
```

## Fases secretas com empilhadeira

```text
Secreta 06 — Empilhadeira Fantasma
Secreta 10 — O Caminho da Duda
```

## Recomendação

[Provável] Para aumentar presença da empilhadeira sem quebrar o ritmo, a campanha deve ter:

```text
6 fases principais com empilhadeira ou híbridas
2 fases secretas com empilhadeira
1 modo desafio separado com corridas extras
```

---

# 11. Modelos de fases de empilhadeira adicionais para modo desafio

Essas fases não precisam entrar na campanha principal, mas podem compor um menu extra chamado:

```text
Forklift Shift Challenges
```

---

## Desafio 01 — Doca Limpa

```text
Tipo: time trial
Dificuldade: Média
Objetivo: entregar 3 caixas em docas diferentes
Mecânica: escolher melhor ordem
```

---

## Desafio 02 — Corredor de Cones

```text
Tipo: precisão
Dificuldade: Média
Objetivo: completar rota sem bater
Mecânica: curvas apertadas
```

---

## Desafio 03 — Vidro Industrial

```text
Tipo: carga frágil
Dificuldade: Difícil
Objetivo: entregar com menos de 5% de dano
Mecânica: frenagem suave
```

---

## Desafio 04 — Câmara Fria Pro

```text
Tipo: piso escorregadio
Dificuldade: Difícil
Objetivo: entregar carga refrigerada em rota com gelo
Mecânica: controle de aderência
```

---

## Desafio 05 — Turno Pesado

```text
Tipo: N-8 Heavy
Dificuldade: Muito difícil
Objetivo: transportar carga pesada em rota curta e apertada
Mecânica: antecipar frenagem
```

---

## Desafio 06 — Portões Temporizados

```text
Tipo: rota com sensores
Dificuldade: Muito difícil
Objetivo: atravessar portões antes do fechamento
Mecânica: ritmo e precisão
```

---

## Desafio 07 — Evacuação N-8

```text
Tipo: emergência
Dificuldade: Mega difícil
Objetivo: retirar 4 cargas antes do lockdown
Mecânica: ordem de entregas e risco
```

---

# 12. Modelo de dados para fase

```json
{
  "id": "s06_028_carga_sem_origem",
  "name": "Carga Sem Origem",
  "sector": 6,
  "difficulty": "mega_hard",
  "type": "puzzle",
  "mechanics": [
    "marked_crates",
    "false_route",
    "ordered_targets",
    "terminal_hint"
  ],
  "size": {
    "width": 16,
    "height": 12
  },
  "ranking": {
    "clean_required_for_gold": true,
    "powerups_allowed": false
  },
  "narrative": {
    "terminal_message": "Origem: apagada. Destino: Setor 08.",
    "duda_hint": "Não deixei nomes nos arquivos. Deixei nas rotas."
  },
  "medals": {
    "bronze": {
      "complete": true
    },
    "silver": {
      "moves_under": 120
    },
    "gold": {
      "moves_under": 85,
      "no_powerups": true
    },
    "platinum": {
      "moves_under": 70,
      "no_undo": true,
      "no_powerups": true
    }
  }
}
```

---

# 13. Modelo de dados para corrida

```json
{
  "id": "race_23_carga_pesada",
  "name": "Carga Pesada",
  "sector": 5,
  "difficulty": "very_hard",
  "type": "forklift_race",
  "vehicle": "forklift_heavy",
  "time_limit": 160,
  "cargo": {
    "type": "industrial_crate",
    "weight": "heavy",
    "fragility": 0.3,
    "stability": 0.65
  },
  "objectives": [
    {
      "type": "pickup",
      "cargo_id": "crate_heavy_01",
      "zone": "maintenance_bay"
    },
    {
      "type": "deliver",
      "cargo_id": "crate_heavy_01",
      "zone": "generator_dock"
    }
  ],
  "hazards": [
    "narrow_turn",
    "ramp",
    "timed_gate"
  ],
  "medals": {
    "bronze": {
      "complete": true
    },
    "silver": {
      "time_under": 140,
      "damage_under": 20
    },
    "gold": {
      "time_under": 115,
      "damage_under": 5,
      "collisions_under": 2
    },
    "platinum": {
      "time_under": 100,
      "damage": 0,
      "collisions": 0
    }
  }
}
```

---

# 14. Estrutura recomendada no projeto

```text
data/
  levels/
    main/
      sector_01_recebimento/
        s01_001_primeiro_turno.json
        s01_002_corredor_apertado.json
        s01_003_carga_cruzada.json
        s01_004_etiqueta_errada.json
        s01_005_doca_inicial.json

      sector_02_expedicao/
        s02_006_portao_de_peso.json
        s02_007_ordem_de_saida.json
        s02_008_doca_b12.json
        s02_009_tres_paletes.json
        s02_010_licenca_classe_c.json

      sector_03_camara_fria/
      sector_04_automacao/
      sector_05_manutencao_pesada/
      sector_06_rotas_fantasma/

    secret/
      secret_01_caixa_fora_do_registro.json
      secret_02_sala_do_robert.json
      secret_03_turno_da_duda.json
      secret_04_rota_do_elias.json
      secret_05_oficina_sem_luz.json
      secret_06_empilhadeira_fantasma.json
      secret_07_08_b.json
      secret_08_o_mapa_que_sobrou.json
      secret_09_ultimo_turno_do_elias.json
      secret_10_o_caminho_da_duda.json

    challenges/
      forklift/
        challenge_01_doca_limpa.json
        challenge_02_corredor_de_cones.json
        challenge_03_vidro_industrial.json
        challenge_04_camara_fria_pro.json
        challenge_05_turno_pesado.json
        challenge_06_portoes_temporizados.json
        challenge_07_evacuacao_n8.json
```

---

# 15. Critérios de aceite de level design

Cada fase precisa passar por estes critérios:

```text
A fase tem objetivo claro.
A fase ensina ou testa uma ideia específica.
A solução não depende de sorte.
A solução não depende de reflexo injusto.
O deadlock é evitável por leitura lógica.
O jogador consegue entender por que errou.
A fase tem restart rápido.
A fase tem medalhas possíveis.
A fase tem pelo menos uma solução limpa.
Power Ups fortes não quebram a fase ou são bloqueados.
A narrativa não atrapalha o raciocínio.
```

---

# 16. Critérios especiais para fases mega difíceis

Fases mega difíceis devem ter:

```text
ordem obrigatória;
pelo menos uma rota falsa;
uso de caixa-ferramenta;
espaço de manobra limitado;
pista narrativa indireta;
solução lógica e auditável;
bloqueio de Power Ups fortes;
medalha platina sem Power Ups.
```

Não devem ter:

```text
movimentos aleatórios;
tempo apertado em puzzle puro;
punição invisível;
solução por tentativa cega;
elementos demais sem função.
```

---

# 17. Backlog para Claude + Codex

## Sprint LD-01 — Estrutura de dados

```text
TW08-LD-001 Criar schema JSON para puzzle.
TW08-LD-002 Criar schema JSON para corrida.
TW08-LD-003 Criar enum de mecânicas.
TW08-LD-004 Criar enum de dificuldade.
TW08-LD-005 Criar estrutura de pastas de fases.
```

## Sprint LD-02 — Fases iniciais

```text
TW08-LD-010 Implementar Fase 01 — Primeiro Turno.
TW08-LD-011 Implementar Fase 02 — Corredor Apertado.
TW08-LD-012 Implementar Fase 03 — Carga Cruzada.
TW08-LD-013 Implementar Fase 04 — Etiqueta Errada.
TW08-LD-014 Implementar Fase 05 — Doca Inicial.
```

## Sprint LD-03 — Sensores e docas

```text
TW08-LD-020 Implementar sensor de peso.
TW08-LD-021 Implementar porta por sensor.
TW08-LD-022 Implementar Fases 06 a 09.
TW08-LD-023 Implementar primeira corrida Classe C.
```

## Sprint LD-04 — Gelo e câmara fria

```text
TW08-LD-030 Implementar piso escorregadio.
TW08-LD-031 Implementar caixa refrigerada.
TW08-LD-032 Implementar Fases 11 a 15.
TW08-LD-033 Implementar corrida Carga Refrigerada.
```

## Sprint LD-05 — Automação

```text
TW08-LD-040 Implementar esteiras.
TW08-LD-041 Implementar robô por turno.
TW08-LD-042 Implementar sensor falso.
TW08-LD-043 Implementar Fases 16 a 20.
```

## Sprint LD-06 — Manutenção pesada

```text
TW08-LD-050 Implementar caixa pesada.
TW08-LD-051 Implementar portas manuais.
TW08-LD-052 Implementar N-8 Heavy.
TW08-LD-053 Implementar Fases 21 a 25.
```

## Sprint LD-07 — Rotas fantasma e final

```text
TW08-LD-060 Implementar mapa incompleto.
TW08-LD-061 Implementar caixas marcadas.
TW08-LD-062 Implementar alvos ordenados.
TW08-LD-063 Implementar Fases 26 a 30.
```

## Sprint LD-08 — Secretas

```text
TW08-LD-070 Implementar sistema de desbloqueio de fases secretas.
TW08-LD-071 Implementar fases secretas 01 a 05.
TW08-LD-072 Implementar fases secretas 06 a 10.
TW08-LD-073 Implementar recompensas secretas.
```

---

# 18. Prompt para Claude

```text
Você é o game designer sênior de The Warehouse Nº 08.

Objetivo:
Revisar e expandir o plano de 30 fases principais + 10 fases secretas, garantindo que os puzzles sejam inteligentes, justos, originais e integrados à história central.

Contexto:
O jogo mistura puzzle em grade, narrativa industrial, Oficina N-8, Power Ups e Forklift Shift Races. A história central envolve John, Duda, Robert, Elias e o Setor 08.

Regras:
- Não criar fases copiadas de jogos existentes.
- Não usar mapas de jogos antigos.
- Cada fase precisa ter intenção de design.
- Fases difíceis devem exigir raciocínio, não tentativa cega.
- Power Ups não podem quebrar ranking limpo.
- Corridas de empilhadeira devem parecer desafios logísticos, não arcade solto.
- Classificar riscos como baixo, médio ou alto.
- Definir critérios de aceite para Codex.

Entregáveis:
1. Revisão das 30 fases principais.
2. Revisão das 10 secretas.
3. Sugestões de ajustes de dificuldade.
4. Lista de mecânicas por setor.
5. Critérios de aceite.
6. Tarefas para Codex.
```

---

# 19. Prompt para Codex

```text
Você é o implementador técnico de The Warehouse Nº 08 em Godot 4.

Objetivo:
Implementar progressivamente as fases da campanha e secretas usando os schemas JSON definidos.

Escopo inicial:
- Criar estrutura data/levels.
- Criar schema para puzzle.
- Criar schema para corrida.
- Implementar as 5 primeiras fases do Setor 01.
- Implementar validação básica de fase.
- Garantir que nenhuma fase dependa de asset externo sem licença.

Regras:
- Não implementar todas as fases de uma vez.
- Não alterar core do movimento sem teste.
- Não criar mapas copiados de jogos existentes.
- Não usar ROM, dumps ou mapas antigos.
- Cada fase deve ter pelo menos uma solução limpa.
- Informar arquivos alterados.
- Informar como validar.
- Informar testes executados.
```

---

# 20. Conclusão

[Certeza] A estrutura ideal para **The Warehouse Nº 08** é:

```text
30 fases principais
10 fases secretas
6 setores narrativos
6 fases principais com empilhadeira/híbridas
2 fases secretas com empilhadeira
7 desafios extras opcionais de empilhadeira
5 fases mega difíceis
1 fase secreta extrema final
```

O jogo deve crescer assim:

```text
ensinar;
enganar de forma justa;
combinar mecânicas;
exigir planejamento;
usar narrativa como pista;
testar domínio no Setor 08;
recompensar quem pensa.
```

A fase boa é aquela em que o jogador pensa:

```text
“Agora eu entendi. O erro foi meu. Vou tentar de novo.”
```

Essa deve ser a base de todos os puzzles de **The Warehouse Nº 08**.

---

## Status de validação

```text
Tipo de documento: plano de level design.
Validação: não validado em gameplay ainda.
Base: documentos atuais do projeto, história central, personagens, Oficina N-8, Power Ups e Forklift Shift Races.
Próximo passo: criar schema real de fase e prototipar as 5 primeiras fases no Godot.
```
