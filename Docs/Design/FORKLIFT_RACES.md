# Forklift Shift Races — The Warehouse Nº 08

> Documento de design, arquitetura e produção para o modo de corridas/desafios de empilhadeira do jogo **The Warehouse Nº 08**.

---

## 1. Veredito técnico

[Certeza] O modo **Forklift Shift Races** não deve ser uma corrida arcade solta. Ele precisa funcionar como um **desafio logístico** dentro do Armazém Nº 08.

A fórmula central do modo é:

```text
tempo + precisão + carga + rota + dano + medalhas
```

O jogador não apenas corre. Ele precisa:

- transportar caixas;
- controlar peso da carga;
- evitar colisões;
- escolher rotas;
- usar atalhos com risco;
- estacionar corretamente;
- entregar mercadorias sem dano;
- dominar a empilhadeira.

---

## 2. Conceito

**Forklift Shift Races** é um modo de fases especiais em que o jogador pilota empilhadeiras dentro do Armazém Nº 08, realizando entregas sob pressão de tempo e precisão.

O foco não é velocidade pura. O foco é **operação logística bem executada**.

### Frase de conceito

```text
Corridas de empilhadeira dentro de um armazém industrial, onde vencer exige velocidade, precisão, controle da carga e leitura de rota.
```

### Identidade

```text
Gênero: desafio logístico / corrida técnica / puzzle de rota
Estilo visual: pixel art 16 bits / anos 80 e 90
Ambiente: armazém, docas, câmaras frias, corredores industriais
Tom: divertido, técnico, tenso e recompensador
```

---

## 3. Objetivo do modo

O modo existe para:

- variar o ritmo da campanha;
- aumentar valor de replay;
- dar utilidade às empilhadeiras criadas;
- reforçar o tema de armazém;
- criar desafios de habilidade além do puzzle em grade;
- permitir medalhas, recordes e desbloqueios;
- integrar a economia de **Créditos de Turno**.

---

## 4. Regra de integração com o jogo principal

[Provável] O maior risco é o modo parecer outro jogo dentro do jogo.

Para evitar isso:

```text
Usar a mesma moeda: Créditos de Turno.
Usar a mesma loja: Oficina N-8.
Usar os mesmos assets de caixas e armazém.
Usar a mesma linguagem visual.
Usar corridas para desbloquear setores.
Usar medalhas compatíveis com as fases puzzle.
Usar narrativa de logística, não corrida esportiva.
```

---

## 5. Tipos de fases de corrida

---

# 5.1 Corrida contra o tempo

## Objetivo

Levar uma ou mais cargas do ponto A até o ponto B antes do tempo acabar.

## Exemplo

```text
Fase: Corredor de Expedição
Objetivo: entregar 3 caixas em 2 minutos.
Penalidade: bater perde tempo.
Medalha ouro: entregar sem colisão.
```

## Avaliação

[Certeza] Esse é o melhor tipo para começar, porque é simples de entender, fácil de prototipar e ótimo para validar se a empilhadeira é divertida.

---

# 5.2 Corrida com carga instável

## Objetivo

Transportar carga frágil ou instável sem derrubar ou danificar.

## Regras possíveis

```text
Curva muito rápida aumenta instabilidade.
Freada brusca desloca a carga.
Colisão danifica mercadoria.
Rampa exige velocidade mínima.
Piso molhado reduz aderência.
```

## Exemplo

```text
Carga: Vidros Industriais
Limite de dano: 15%
Tempo máximo: 2min30s
Objetivo: entregar a carga intacta.
```

## Avaliação

[Provável] Excelente para transformar a corrida em desafio técnico, mas não deve entrar no primeiro protótipo.

---

# 5.3 Circuito de precisão

## Objetivo

Completar um circuito sem bater, sem derrubar carga e respeitando rota.

## Pontuação baseada em

```text
Tempo
Colisões
Trajeto correto
Carga preservada
Ré bem executada
Estacionamento final
Uso correto da empilhadeira
```

## Exemplo

```text
Fase: Teste de Operador N-8
Objetivo: completar circuito sem encostar nos cones.
```

## Avaliação

[Provável] Combina muito com estética 16 bits e tema industrial.

---

# 5.4 Entrega em múltiplos pontos

## Objetivo

Escolher a melhor ordem para entregar caixas em diferentes setores.

## Exemplo

```text
Entregar caixas A, B e C nos setores corretos.
Cada rota tem obstáculos diferentes.
A ordem das entregas muda o tempo final.
```

## Camada de puzzle

```text
Qual caixa pegar primeiro?
Qual rota é menor?
Vale pegar atalho com risco?
Vale entregar carga pesada antes?
Vale usar rota segura para carga frágil?
```

## Avaliação

[Certeza] Esse é o melhor tipo para conectar corrida com puzzle.

---

# 5.5 Corrida com portões e sensores

## Objetivo

Atravessar a rota usando mecanismos do armazém.

## Elementos

```text
Portões automáticos
Botões de piso
Esteiras
Barreiras
Semáforos industriais
Robôs patrulhando
Zonas de carga
Elevadores de carga
Sensores de peso
```

## Exemplo

```text
Para abrir o portão 03, o jogador precisa passar pelo sensor carregando a caixa correta.
```

## Avaliação

[Provável] Muito bom para fases intermediárias e avançadas.

---

# 5.6 Corrida de emergência

## Objetivo

Completar entregas ou evacuar cargas antes de bloqueio do setor.

## Elementos

```text
Alarme tocando
Luzes piscando
Tempo curto
Rotas bloqueadas
Portões fechando
Sistema automático com falha
```

## Exemplo

```text
Fase: Evacuação do Setor 08
Objetivo: retirar 4 cargas antes do fechamento automático.
```

## Avaliação

[Certeza] Excelente para clímax de setor e final de mundo.

---

## 6. Estrutura dos modos

### Modo Campanha

Corridas aparecem entre setores como fases especiais.

```text
Setor 01: puzzle tradicional.
Setor 02: primeira corrida curta.
Setor 03: corrida com piso escorregadio.
Setor 04: corrida com robôs e esteiras.
Setor 05: corrida de carga pesada.
Setor 06: corrida de emergência.
```

### Modo Desafio

Corridas independentes com medalhas e recordes.

### Modo Treinamento

Sem tempo. Serve para aprender controles.

### Modo Licença de Operador

Série de testes para desbloquear novas empilhadeiras.

### Modo Diário

Uma corrida nova por dia.

## Recomendação inicial

[Certeza] Começar apenas com:

```text
Campanha + Desafio + Treinamento
```

Não implementar modo diário nem ranking online no MVP.

---

## 7. Mecânicas principais da empilhadeira

---

# 7.1 Movimento

A empilhadeira não deve controlar igual ao personagem em grade. Ela precisa ter sensação própria.

## Teclado

```text
W / S: acelerar e frear/ré
A / D: virar
Espaço: freio de mão leve
E: pegar/soltar carga
Q / R: subir/descer garfo
Shift: buzina ou aceleração leve
Tab: alternar câmera ou mini-mapa
```

## Controle

```text
Analógico esquerdo: direção
RT: acelerar
LT: frear/ré
A: pegar/soltar carga
B: freio
RB/LB: subir/descer garfo
Y: buzina
Start: pausa
```

---

# 7.2 Física controlada

[Certeza] Não use física realista demais para um jogo 16 bits. A empilhadeira precisa ser previsível.

Modelo ideal:

```text
Aceleração suave
Velocidade máxima limitada
Curva com raio previsível
Derrapagem leve em pisos especiais
Colisão com perda de velocidade
Carga com estabilidade simplificada
```

---

# 7.3 Peso da carga

Cada carga pode ter propriedades.

```text
Leve
Média
Pesada
Frágil
Instável
Perigosa
Refrigerada
Urgente
```

## Efeitos

```text
Carga leve:
Aceleração normal.

Carga média:
Curva levemente mais lenta.

Carga pesada:
Aceleração menor e frenagem mais longa.

Carga frágil:
Colisão causa dano alto.

Carga instável:
Curva rápida aumenta risco de queda.

Carga perigosa:
Não pode bater.

Carga refrigerada:
Tem limite de tempo mais severo.

Carga urgente:
Dá bônus maior por entrega rápida.
```

---

# 7.4 Altura do garfo

A altura do garfo pode ser uma mecânica.

```text
Garfo baixo:
Mais estável, mas não passa por alguns obstáculos.

Garfo médio:
Padrão.

Garfo alto:
Permite entregar em plataformas, mas reduz estabilidade.
```

## Recomendação

[Provável] Não colocar altura realista do garfo no MVP. Começar com pegar/soltar simples.

---

## 8. Sistema de pontuação

Cada corrida deve gerar uma nota baseada em vários fatores.

```text
Pontuação base: completar entrega.
Bônus de tempo: quanto mais rápido, melhor.
Bônus de carga intacta: sem dano.
Bônus de rota limpa: sem colisão.
Bônus de precisão: estacionar certo.
Bônus de eficiência: menor distância.
Bônus de risco: usar atalho difícil.
```

## Exemplo de resultado

```text
Tempo: 01:42
Dano da carga: 0%
Colisões: 1
Distância percorrida: 312m
Entregas corretas: 3/3

Resultado:
Medalha Prata
+180 Créditos de Turno
```

---

## 9. Medalhas

```text
Bronze:
Completar a corrida.

Prata:
Completar abaixo do tempo recomendado.

Ouro:
Completar rápido e com pouco dano.

Platina:
Sem colisão, carga intacta e tempo excelente.
```

[Certeza] Medalha platina é ótima para replay value.

---

## 10. Obstáculos e variações

### Obstáculos simples

```text
Cones
Paletes
Caixas paradas
Portões
Corredores estreitos
Rampas
Pisos molhados
Óleo no chão
Curvas fechadas
Empilhadeiras paradas
```

### Obstáculos avançados

```text
Robôs de limpeza
Esteiras móveis
Portas temporizadas
Sensores de peso
Braços mecânicos
Câmeras de segurança
Áreas interditadas
Zonas magnéticas
Piso quebrado
Elevadores industriais
```

### Obstáculos narrativos

```text
Apagão
Alarme de incêndio
Sistema automático com falha
Setor congelado
Setor inundado
Setor em manutenção
Setor com gás/baixa visibilidade
```

---

## 11. Power Ups específicos de corrida

Esses Power Ups devem ser separados dos Power Ups de puzzle.

---

# 11.1 Estabilizador de Carga

```text
Reduz chance de derrubar a caixa em curvas.
```

[Certeza] Melhor Power Up inicial para corrida, porque ajuda sem transformar tudo em velocidade pura.

---

# 11.2 Freio ABS N-8

```text
Permite freada brusca sem derrapar.
```

Bom para circuitos de precisão.

---

# 11.3 Nitro Hidráulico

```text
Acelera por 2 segundos.
Risco: aumenta chance de derrubar carga.
```

[Provável] Deve ser raro, porque pode transformar o modo em corrida arcade.

---

# 11.4 Scanner de Rota

```text
Mostra a melhor rota por alguns segundos.
```

Bom para fases com múltiplos caminhos.

---

# 11.5 Suspensão Reforçada

```text
Reduz dano ao passar por pisos ruins.
```

Boa para setores quebrados ou câmaras frias.

---

# 11.6 Garfo Magnético

```text
Segura melhor a caixa durante curvas.
```

Deve ser limitado por fase.

---

# 11.7 Buzina Industrial

```text
Afasta robôs pequenos ou abre caminho em área movimentada.
```

Boa para fases com obstáculos móveis.

---

## 12. Classes de empilhadeira

---

# 12.1 N-8 Standard

```text
Equilibrada.
Boa para tutorial.
```

## Status sugeridos

```text
Velocidade: 3/5
Aceleração: 3/5
Estabilidade: 3/5
Capacidade: 3/5
Controle: 4/5
```

---

# 12.2 N-8 Heavy

```text
Mais lenta.
Carrega caixas pesadas.
Mais resistente a colisões.
```

## Status sugeridos

```text
Velocidade: 2/5
Aceleração: 2/5
Estabilidade: 4/5
Capacidade: 5/5
Controle: 3/5
```

---

# 12.3 N-8 Light

```text
Mais rápida.
Pior estabilidade.
Boa para corrida contra tempo.
```

## Status sugeridos

```text
Velocidade: 5/5
Aceleração: 4/5
Estabilidade: 2/5
Capacidade: 2/5
Controle: 3/5
```

---

# 12.4 N-8 Cold Storage

```text
Boa em piso escorregadio.
Usada em setores frios.
```

## Status sugeridos

```text
Velocidade: 3/5
Aceleração: 3/5
Estabilidade: 5/5
Capacidade: 3/5
Controle: 4/5
```

---

# 12.5 N-8 Electric

```text
Aceleração suave.
Menor ruído.
Boa para fases de precisão.
```

## Status sugeridos

```text
Velocidade: 3/5
Aceleração: 4/5
Estabilidade: 4/5
Capacidade: 2/5
Controle: 5/5
```

---

# 12.6 N-8 Prototype

```text
Desbloqueável.
Alta velocidade.
Difícil de controlar.
```

## Status sugeridos

```text
Velocidade: 5/5
Aceleração: 5/5
Estabilidade: 2/5
Capacidade: 3/5
Controle: 2/5
```

---

## 13. Integração com a campanha

As corridas podem funcionar como provas para abrir setores.

```text
Setor 01 — Recebimento
Puzzle tradicional.

Setor 02 — Expedição
Primeira corrida curta de empilhadeira.

Setor 03 — Câmara Fria
Puzzle + corrida em piso escorregadio.

Setor 04 — Automação
Corrida com robôs e esteiras.

Setor 05 — Carga Pesada
Empilhadeira Heavy obrigatória.

Setor 06 — Lockdown N-8
Corrida de emergência final.
```

---

## 14. Progressão narrativa

O jogador começa como operador auxiliar e desbloqueia licenças.

```text
Licença Classe C:
Empilhadeira básica.

Licença Classe B:
Cargas pesadas.

Licença Classe A:
Setores perigosos.

Licença Especial N-8:
Protótipo final.
```

## Uso narrativo

```text
A licença funciona como progressão.
Cada licença libera tipos de corrida e empilhadeiras.
A campanha usa corridas como prova de habilidade.
```

---

## 15. Estrutura de fase de corrida

Exemplo de `race_01_warehouse_corridor.json`:

```json
{
  "id": "race_01_warehouse_corridor",
  "name": "Corredor de Expedição",
  "mode": "time_trial",
  "vehicle": "forklift_standard",
  "time_limit": 120,
  "cargo": {
    "type": "wooden_crate",
    "weight": "medium",
    "fragility": 0.4
  },
  "objectives": [
    {
      "type": "deliver",
      "cargo_id": "crate_01",
      "target_zone": "dock_a"
    }
  ],
  "medals": {
    "bronze": {
      "complete": true
    },
    "silver": {
      "time_under": 105,
      "damage_under": 20
    },
    "gold": {
      "time_under": 90,
      "damage_under": 5,
      "collisions_under": 2
    },
    "platinum": {
      "time_under": 75,
      "damage": 0,
      "collisions": 0
    }
  }
}
```

---

## 16. Estrutura de dados de veículos

Exemplo de `vehicles.json`:

```json
{
  "forklift_standard": {
    "name": "N-8 Standard",
    "max_speed": 130,
    "acceleration": 70,
    "brake_force": 90,
    "turn_rate": 80,
    "stability": 0.75,
    "cargo_capacity": "medium",
    "unlock_license": "class_c"
  },
  "forklift_heavy": {
    "name": "N-8 Heavy",
    "max_speed": 95,
    "acceleration": 50,
    "brake_force": 75,
    "turn_rate": 55,
    "stability": 0.9,
    "cargo_capacity": "heavy",
    "unlock_license": "class_b"
  },
  "forklift_light": {
    "name": "N-8 Light",
    "max_speed": 165,
    "acceleration": 95,
    "brake_force": 85,
    "turn_rate": 95,
    "stability": 0.55,
    "cargo_capacity": "light",
    "unlock_license": "class_b"
  }
}
```

---

## 17. Estrutura técnica em Godot

```text
game/
  scripts/
    vehicle/
      ForkliftController.gd
      ForkliftPhysics.gd
      ForkliftCargoSystem.gd
      ForkliftInput.gd
      ForkliftStats.gd
      ForkliftAudio.gd
      ForkliftCollisionFeedback.gd

    racing/
      RaceLevelController.gd
      RaceTimer.gd
      RaceCheckpointSystem.gd
      RaceScoringSystem.gd
      RaceMedalSystem.gd
      RaceObjectiveSystem.gd
      RaceResultController.gd
      RaceRouteSystem.gd

    cargo/
      CargoDamageSystem.gd
      CargoStabilitySystem.gd
      CargoAttachSystem.gd
      CargoWeightProfile.gd

  scenes/
    vehicle/
      Forklift.tscn
      ForkliftStandard.tscn
      ForkliftHeavy.tscn
      ForkliftLight.tscn
      ForkliftColdStorage.tscn
      ForkliftElectric.tscn
      ForkliftPrototype.tscn

    racing/
      RaceLevel.tscn
      Checkpoint.tscn
      DeliveryZone.tscn
      RaceHUD.tscn
      RaceResultScreen.tscn
      TrainingCourse.tscn

  data/
    racing/
      races.json
      vehicles.json
      cargo_types.json
      race_medals.json
```

---

## 18. MVP da corrida

[Certeza] O MVP não deve começar complexo. Faça primeiro uma corrida pequena e muito bem polida.

### MVP obrigatório

```text
1 empilhadeira controlável
1 caixa
1 ponto de coleta
1 ponto de entrega
Timer
Colisão simples
Dano da carga
Medalhas bronze/prata/ouro
HUD de tempo
HUD de dano
Resultado final
```

### Não colocar no MVP

```text
Várias empilhadeiras
Ranking online
Múltiplas cargas
Robôs
Tráfego
Pista gigante
Física realista
Customização visual
Modo diário
```

[Certeza] Primeiro valide se dirigir a empilhadeira é divertido.

---

## 19. UX da corrida

### HUD sugerido

```text
TEMPO: 01:24
CARGA: 92%
COLISÕES: 1
OBJETIVO: Entregar no Doca A
```

### Indicadores visuais

```text
Seta para zona de entrega
Ícone da carga
Barra de estabilidade
Alerta de dano
Linha de rota opcional
Checkpoint brilhando
Zona de entrega marcada no chão
Sinalização de velocidade recomendada
```

### Feedback sonoro

```text
Bip de ré
Motor elétrico/gás
Batida leve
Alarme de carga instável
Som de entrega concluída
Sirene em corrida de emergência
Som de colisão com cone
Som de freio
```

---

## 20. Sistema de dano

Modelo simples:

```text
Colisão leve: -2% integridade
Colisão média: -8% integridade
Colisão forte: -20% integridade
Queda da caixa: -35% integridade
Entrega errada: penalidade de tempo
```

Carga chega a 0%:

```text
Opção 1: falha na entrega.
Opção 2: conclusão com penalidade máxima.
```

## Recomendação

[Provável] Não falhar automaticamente no começo. Melhor permitir concluir com nota ruim, porque reduz frustração.

---

## 21. Sistema de estabilidade

Para carga instável:

```text
Curva leve: seguro.
Curva rápida: aumenta instabilidade.
Freada brusca: aumenta instabilidade.
Colisão: aumenta muito.
Estabilidade chega a 100%: carga cai.
```

HUD:

```text
Estabilidade da carga: 0% a 100%
Quanto menor, melhor.
```

---

## 22. Level design das corridas

Uma fase boa precisa ter:

```text
Rota principal segura
Atalho arriscado
Curvas com leitura clara
Obstáculos posicionados com intenção
Área de recuperação
Ponto de entrega bem visível
Tempo suficiente para bronze
Tempo apertado para ouro
Segredo ou rota alternativa para platina
```

### Exemplo de leitura de medalha

```text
Bronze:
Usa rota segura.

Ouro:
Usa atalho.

Platina:
Usa atalho sem bater e com carga intacta.
```

---

## 23. Primeira fase recomendada

### Nome

```text
Licença de Operador — Classe C
```

### Objetivo

```text
Pegue uma caixa.
Contorne cones.
Entregue na zona marcada.
```

### Condições

```text
Tempo bronze: 2:00
Tempo prata: 1:30
Tempo ouro: 1:10
Platina: 1:00 sem colisão
```

[Certeza] Essa é a fase ideal para validar se a empilhadeira é divertida antes de criar corridas complexas.

---

## 24. Fases sugeridas

---

# 24.1 Licença de Operador — Classe C

```text
Tipo: treinamento
Objetivo: pegar uma caixa e entregar no ponto A
Dificuldade: baixa
Mecânica ensinada: acelerar, frear, virar, pegar carga
```

---

# 24.2 Corredor de Expedição

```text
Tipo: contra o tempo
Objetivo: entregar 3 caixas em docas diferentes
Dificuldade: baixa/média
Mecânica ensinada: escolher rota
```

---

# 24.3 Curva do Palete Perdido

```text
Tipo: precisão
Objetivo: passar por corredor estreito sem bater
Dificuldade: média
Mecânica ensinada: controle de curva
```

---

# 24.4 Câmara Fria

```text
Tipo: piso escorregadio
Objetivo: entregar carga refrigerada antes do tempo
Dificuldade: média
Mecânica ensinada: aderência e frenagem
```

---

# 24.5 Doca em Alerta

```text
Tipo: emergência
Objetivo: retirar cargas antes do fechamento dos portões
Dificuldade: alta
Mecânica ensinada: pressão de tempo
```

---

# 24.6 Carga Pesada

```text
Tipo: carga especial
Objetivo: usar empilhadeira Heavy para levar carga lenta e pesada
Dificuldade: alta
Mecânica ensinada: peso e aceleração
```

---

# 24.7 Lockdown N-8

```text
Tipo: corrida final de setor
Objetivo: entregar múltiplas cargas com rotas bloqueando progressivamente
Dificuldade: alta
Mecânica ensinada: domínio completo do modo
```

---

## 25. Critérios de aceite

### ForkliftController

```text
- Empilhadeira acelera suavemente.
- Empilhadeira freia.
- Empilhadeira dá ré.
- Curva é previsível.
- Controle por teclado funciona.
- Controle por gamepad funciona ou está preparado.
```

### CargoAttachSystem

```text
- Caixa pode ser pega.
- Caixa pode ser solta.
- Caixa acompanha a empilhadeira corretamente.
- Caixa não atravessa parede.
- Caixa não fica presa em estado inválido.
```

### CargoDamageSystem

```text
- Colisão leve aplica dano leve.
- Colisão média aplica dano médio.
- Colisão forte aplica dano alto.
- HUD atualiza integridade da carga.
- Entrega final considera dano.
```

### RaceScoringSystem

```text
- Calcula tempo.
- Calcula colisões.
- Calcula dano.
- Calcula medalha.
- Calcula Créditos de Turno.
```

### RaceHUD

```text
- Mostra tempo.
- Mostra integridade da carga.
- Mostra colisões.
- Mostra objetivo atual.
- Mostra alerta de carga instável.
```

---

## 26. Backlog para Claude + Codex

---

# Sprint Race 1 — Protótipo dirigível

```text
TW08-RACE-001 Criar ForkliftController.gd.
TW08-RACE-002 Criar física simples de aceleração, freio e curva.
TW08-RACE-003 Criar Forklift.tscn.
TW08-RACE-004 Criar pista de teste.
TW08-RACE-005 Criar HUD com tempo.
TW08-RACE-006 Criar colisão simples.
```

---

# Sprint Race 2 — Carga e entrega

```text
TW08-RACE-010 Criar CargoAttachSystem.gd.
TW08-RACE-011 Permitir pegar caixa.
TW08-RACE-012 Permitir soltar caixa.
TW08-RACE-013 Criar DeliveryZone.tscn.
TW08-RACE-014 Detectar entrega correta.
TW08-RACE-015 Criar CargoDamageSystem.gd.
```

---

# Sprint Race 3 — Pontuação e medalhas

```text
TW08-RACE-020 Criar RaceScoringSystem.gd.
TW08-RACE-021 Criar RaceMedalSystem.gd.
TW08-RACE-022 Criar RaceResultScreen.tscn.
TW08-RACE-023 Integrar recompensa em Créditos de Turno.
TW08-RACE-024 Separar resultado limpo e assistido.
```

---

# Sprint Race 4 — Polimento

```text
TW08-RACE-030 Adicionar som de motor.
TW08-RACE-031 Adicionar som de ré.
TW08-RACE-032 Adicionar partículas leves de poeira.
TW08-RACE-033 Adicionar feedback de colisão.
TW08-RACE-034 Adicionar animação de entrega.
TW08-RACE-035 Adicionar alerta visual de dano.
```

---

# Sprint Race 5 — Primeira fase real

```text
TW08-RACE-040 Criar fase Licença de Operador — Classe C.
TW08-RACE-041 Criar layout com cones e rota segura.
TW08-RACE-042 Criar metas bronze/prata/ouro/platina.
TW08-RACE-043 Fazer playtest interno.
TW08-RACE-044 Ajustar aceleração, curva e freio.
```

---

## 27. Prompt para Claude

```text
Você é o arquiteto técnico e game designer sênior do projeto The Warehouse Nº 08.

Objetivo:
Revisar e expandir o modo Forklift Shift Races.

Contexto:
Forklift Shift Races é um modo de desafios de empilhadeira dentro do Armazém Nº 08. Ele não deve ser corrida arcade pura. Deve ser uma mistura de corrida, puzzle, controle de carga, precisão e rota logística.

Regras:
- Manter integração com campanha, Oficina N-8 e Créditos de Turno.
- Não criar escopo grande demais no MVP.
- Priorizar física previsível, não realismo excessivo.
- Criar critérios de aceite para Codex.
- Separar MVP, pós-MVP e futuro.
- Classificar riscos como baixo, médio ou alto.
- Informar como validar.

Entregáveis:
1. Revisão do modo.
2. Ajustes de escopo.
3. Backlog para Codex.
4. Critérios de aceite.
5. Riscos técnicos.
6. Plano de validação.
```

---

## 28. Prompt para Codex

```text
Você é o implementador técnico do projeto The Warehouse Nº 08 em Godot 4.

Objetivo:
Implementar o MVP do modo Forklift Shift Races.

Escopo do MVP:
- 1 empilhadeira controlável.
- 1 caixa.
- 1 ponto de coleta.
- 1 ponto de entrega.
- Timer.
- Colisão simples.
- Dano da carga.
- HUD de tempo e dano.
- Resultado final com medalha.

Arquivos esperados:
- scripts/vehicle/ForkliftController.gd
- scripts/vehicle/ForkliftPhysics.gd
- scripts/vehicle/ForkliftInput.gd
- scripts/cargo/CargoAttachSystem.gd
- scripts/cargo/CargoDamageSystem.gd
- scripts/racing/RaceLevelController.gd
- scripts/racing/RaceTimer.gd
- scripts/racing/RaceScoringSystem.gd
- scripts/racing/RaceMedalSystem.gd
- scenes/vehicle/Forklift.tscn
- scenes/racing/RaceLevel.tscn
- scenes/racing/DeliveryZone.tscn
- scenes/racing/RaceHUD.tscn

Regras:
- Não implementar ranking online.
- Não implementar múltiplas empilhadeiras agora.
- Não implementar modo diário agora.
- Não usar física realista demais.
- Não quebrar sistemas existentes de puzzle.
- Criar uma pista de teste simples.
- Informar arquivos alterados.
- Informar como validar.
- Informar testes executados.
```

---

## 29. Riscos técnicos

### Risco 1 — Física ruim

[Certeza] Se a empilhadeira for ruim de controlar, o modo inteiro falha.

Mitigação:

```text
Testar aceleração, curva e freio antes de criar fases.
Criar pista de teste pequena.
Fazer playtest com teclado e controle.
```

### Risco 2 — Escopo grande demais

[Provável] O modo pode crescer demais com várias empilhadeiras, power ups e obstáculos.

Mitigação:

```text
MVP com 1 empilhadeira, 1 caixa e 1 entrega.
Só expandir depois de validar diversão.
```

### Risco 3 — Parecer outro jogo

[Provável] Se a corrida virar arcade genérico, perde identidade.

Mitigação:

```text
Manter foco em logística.
Pontuar precisão e carga.
Usar medalhas e Créditos de Turno.
Integrar com campanha.
```

### Risco 4 — Bugs de colisão e carga

[Certeza] Sistema de pegar/soltar caixa pode gerar bugs.

Mitigação:

```text
Criar estados claros: livre, acoplada, entregue, danificada.
Nunca permitir dois donos da mesma carga.
Validar colisão antes de soltar.
```

---

## 30. Plano de validação

### Validação do controle

```text
A empilhadeira precisa acelerar, frear, virar e dar ré sem travar.
O jogador precisa conseguir fazer uma curva fechada previsível.
A empilhadeira não pode parecer escorregadia demais.
```

### Validação da entrega

```text
A caixa precisa ser pega.
A caixa precisa acompanhar a empilhadeira.
A caixa precisa ser entregue na zona correta.
O jogo precisa reconhecer entrega concluída.
```

### Validação de pontuação

```text
Tempo final é registrado.
Dano final é registrado.
Colisões são registradas.
Medalha correta é atribuída.
Créditos de Turno são somados.
```

### Validação de UX

```text
O jogador entende onde pegar a caixa.
O jogador entende onde entregar.
O HUD não polui a tela.
Alertas são claros.
A fase termina com feedback satisfatório.
```

---

## 31. Direção final

[Certeza] A melhor versão do modo é:

```text
Corrida de empilhadeira como desafio logístico.
Não corrida arcade pura.
```

A identidade deve ser:

```text
Operar bem é mais importante do que apenas correr.
```

O modo deve recompensar:

```text
velocidade,
precisão,
cuidado com carga,
boa rota,
baixo dano,
domínio da empilhadeira.
```

---

## 32. Status de validação

```text
Tipo de documento: Design técnico inicial.
Validação: Não validado em gameplay ainda.
Base: análise de design, UX, balanceamento e arquitetura para Godot.
Próximo passo: criar MVP dirigível da empilhadeira e testar a primeira fase "Licença de Operador — Classe C".
```
