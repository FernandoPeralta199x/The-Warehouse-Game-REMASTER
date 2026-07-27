# The Warehouse Nº 08 — Sistema de Loja e Power Ups

> Documento de design e implementação inicial para o sistema de **Loja de Equipamentos N-8**, economia interna e Power Ups do jogo **The Warehouse Nº 08**.

---

## 1. Veredito técnico

[Certeza] O sistema de Power Ups precisa ajudar o jogador sem destruir o núcleo do puzzle.

O jogo é baseado em lógica espacial: empurrar caixas, planejar rotas, corrigir erros e resolver fases com eficiência. Portanto, os Power Ups não devem funcionar como “pular desafio” ou “comprar solução”. Eles devem ser tratados como **ferramentas de armazém**, com usos limitados e impacto controlado.

**Regra central:**

```text
Power Ups ajudam o jogador a pensar melhor, corrigir erro ou reduzir frustração.
Power Ups não devem resolver a fase sozinhos.
```

---

## 2. Nome do sistema

### Nome principal

**Loja de Equipamentos N-8**

### Alternativas de nome

- **Oficina N-8**
- **N-8 Tool Shop**
- **Central de Ferramentas**
- **Oficina do Armazém Nº 8**
- **Sala de Manutenção N-8**
- **Setor de Suprimentos**
- **Depósito de Ferramentas**

### Recomendação

[Provável] O melhor nome para o jogo é:

```text
Oficina N-8
```

Motivo: combina com o tema de armazém, empilhadeira, caixa, ferramenta e manutenção. Também soa bem em português e pode ser traduzido facilmente.

---

## 3. Moeda do jogo

O jogador ganha uma moeda interna chamada:

```text
Créditos de Turno
```

Nome em inglês:

```text
Shift Credits
```

Também pode ser usado:

```text
Warehouse Credits
```

### Recomendação

[Certeza] Para manter identidade própria, use:

```text
Créditos de Turno
```

Porque o nome conversa com a ideia de trabalhar em turnos dentro do armazém.

---

## 4. Como ganhar Créditos de Turno

O jogador ganha créditos ao jogar bem, não apenas ao terminar fases.

### Fontes de ganho

```text
Completar fase: +100
Medalha bronze: +25
Medalha prata: +50
Medalha ouro: +100
Sem usar dica: +50
Sem usar Power Up: +50
Novo recorde pessoal: +75
Completar desafio diário: +150
Completar mundo/setor: +300
Resolver fase na primeira tentativa: +50
Resolver abaixo do limite de empurrões: +75
Resolver abaixo do limite de movimentos: +75
```

### Regra de progressão

[Certeza] O jogo não deve dar crédito demais. Se o jogador conseguir comprar tudo muito cedo, a loja perde sentido.

Modelo recomendado:

```text
Fase comum: 100 a 250 créditos possíveis.
Power Up comum: 50 a 150 créditos.
Power Up forte: 180 a 300 créditos.
Upgrade permanente: 500 a 1500 créditos.
```

---

## 5. Sistema de medalhas

Cada fase pode ter 3 metas:

```text
Bronze: completar a fase.
Prata: completar abaixo do limite médio de movimentos.
Ouro: completar perto da solução otimizada.
```

Critérios adicionais:

```text
Medalha limpa: sem Power Ups.
Medalha assistida: com Power Ups.
Medalha perfeita: sem Power Ups, sem dicas, sem undo extra.
```

### Exemplo de resultado

```text
Fase concluída!

Movimentos: 42
Empurrões: 12
Power Ups usados: 1
Dicas usadas: 0

Resultado: Conclusão Assistida
Ranking competitivo: desativado
Créditos recebidos: +125
```

---

## 6. Ranking limpo e ranking assistido

[Certeza] Power Ups não devem contar para o ranking principal.

### Ranking limpo

Sem Power Ups.

Conta para:

- leaderboard competitivo;
- medalhas especiais;
- conquistas avançadas;
- recorde oficial da fase;
- desafio semanal.

### Ranking assistido

Com Power Ups.

Conta para:

- progressão casual;
- conclusão da campanha;
- moedas básicas;
- desbloqueio de fases;
- acessibilidade.

### Regra de interface

Sempre que o jogador usar Power Up, mostrar:

```text
Modo Assistido ativado.
Esta tentativa não entrará no ranking competitivo.
```

---

## 7. Tipos de Power Ups

---

# 7.1 Rebobinar Movimento

## Nome

```text
Rebobinar Movimento
```

## Função

Permite desfazer movimentos extras.

## Uso

```text
Desfaz os últimos 3 movimentos.
Uso: 1 vez por fase.
Preço: 50 créditos.
```

## Avaliação

[Certeza] É o Power Up mais seguro do jogo, porque reduz frustração sem alterar a lógica principal.

## Risco

Baixo.

## Recomendação

Entrar no MVP.

---

# 7.2 Scanner Logístico

## Nome

```text
Scanner Logístico
```

## Função

Destaca uma caixa importante ou mostra qual caixa está em posição crítica.

## Uso

```text
Destaca uma caixa recomendada por alguns segundos.
Uso: 1 vez por fase.
Preço: 80 créditos.
```

## Variações

```text
Scanner de Caixa Crítica
Scanner de Rota
Scanner de Alvo
Scanner de Congestionamento
```

## Avaliação

[Certeza] É excelente para UX porque ajuda o jogador a entender o problema sem entregar a solução completa.

## Risco

Baixo a médio.

## Recomendação

Entrar no MVP.

---

# 7.3 Assistente de Turno

## Nome

```text
Assistente de Turno
```

## Função

Dá dicas progressivas em camadas.

## Exemplo

```text
Dica 1: Observe a caixa perto da parede norte.
Dica 2: Leve essa caixa antes de mover a caixa central.
Dica 3: Primeiro movimento recomendado: direita, cima, cima.
```

## Uso

```text
Cada fase pode ter até 3 dicas.
Preço: 150 créditos por pacote.
```

## Avaliação

[Certeza] Melhor do que mostrar uma solução completa, porque mantém o raciocínio do jogador.

## Risco

Médio se a terceira dica for muito direta.

## Recomendação

Entrar no MVP, mas com dicas escritas manualmente por fase.

---

# 7.4 Força Hidráulica

## Nome

```text
Força Hidráulica
```

## Função

Permite empurrar uma caixa por 2 casas, se o caminho estiver livre.

## Uso

```text
Empurra uma caixa 2 espaços em linha reta.
Uso: 1 vez por fase.
Preço: 120 créditos.
```

## Avaliação

[Provável] É interessante para fases avançadas, mas pode quebrar puzzles se não for controlado.

## Risco

Alto.

## Recomendação

Não entrar no MVP. Testar apenas depois da vertical slice.

---

# 7.5 Macaco N-8

## Nome

```text
Macaco N-8
```

## Função

Permite puxar uma caixa uma única vez.

## Uso

```text
Puxa uma caixa uma casa.
Uso: 1 vez por fase.
Preço: 200 créditos.
```

## Avaliação

[Provável] É um item forte demais para o núcleo Sokoban. Deve ser tratado como ferramenta rara.

## Risco

Alto.

## Recomendação

Bloquear no ranking competitivo.

---

# 7.6 Reposicionamento Manual

## Nome

```text
Reposicionamento Manual
```

## Função

Retorna uma caixa para sua posição inicial sem reiniciar a fase inteira.

## Uso

```text
Retorna uma caixa para sua posição inicial.
Uso: 1 vez por fase.
Preço: 180 créditos.
```

## Avaliação

[Provável] Bom para jogadores casuais, mas pode quebrar a progressão se for barato.

## Risco

Médio a alto.

## Recomendação

Adicionar apenas depois do playtest.

---

## 8. Novas ideias de Power Ups

---

# 8.1 Trava de Segurança

## Função

Marca uma caixa como “travada” por alguns movimentos, impedindo o jogador de empurrá-la por engano.

## Uso

```text
Trava uma caixa por 5 movimentos.
Preço: 70 créditos.
Uso: 1 vez por fase.
```

## Por que é bom

[Provável] Ajuda em fases onde o jogador empurra uma caixa errada sem perceber.

## Risco

Baixo.

---

# 8.2 Piso Antiderrapante

## Função

Em fases com gelo ou piso escorregadio, bloqueia o deslizamento de uma caixa ou jogador por um turno.

## Uso

```text
Cancela o efeito de piso escorregadio uma vez.
Preço: 90 créditos.
```

## Risco

Médio.

## Observação

Só faz sentido se o jogo tiver mecânica de gelo.

---

# 8.3 Ímã de Carga

## Função

Atrai uma caixa próxima uma casa em direção ao jogador.

## Uso

```text
Atrai uma caixa em linha reta por 1 casa.
Preço: 220 créditos.
Uso: 1 vez por fase.
```

## Avaliação

[Provável] Muito forte. Deve ser usado somente em modo assistido.

## Risco

Alto.

---

# 8.4 Bateria Reserva

## Função

Permite ativar uma máquina, porta, esteira ou sensor uma vez extra.

## Uso

```text
Reativa um mecanismo sem energia.
Preço: 130 créditos.
```

## Melhor uso

Fases com:

- esteiras;
- sensores;
- portas;
- plataformas;
- máquinas;
- robôs de rota.

## Risco

Médio.

---

# 8.5 Chave Mestra

## Função

Abre uma porta bloqueada sem precisar ativar o botão correto.

## Uso

```text
Abre uma porta uma única vez.
Preço: 250 créditos.
```

## Avaliação

[Provável] Pode quebrar fases. Deve ser raro e bloqueado em ranking.

## Risco

Alto.

---

# 8.6 Freio de Emergência

## Função

Para uma esteira ou robô por alguns turnos.

## Uso

```text
Congela esteiras ou robôs por 3 turnos.
Preço: 100 créditos.
```

## Risco

Médio.

## Boa aplicação

Fases com movimento automático.

---

# 8.7 Marcador de Rota

## Função

Permite o jogador desenhar uma rota planejada no chão antes de mover.

## Uso

```text
Mostra uma linha de planejamento por 15 segundos.
Preço: 40 créditos.
```

## Avaliação

[Certeza] Excelente para puzzle, porque não resolve nada sozinho; só ajuda planejamento.

## Risco

Baixo.

## Recomendação

Boa opção para MVP ou pós-MVP.

---

# 8.8 Simulação Fantasma

## Função

Permite testar alguns movimentos em modo “fantasma”, sem alterar a fase real.

## Uso

```text
Permite simular até 8 movimentos e depois confirmar ou cancelar.
Preço: 150 créditos.
```

## Avaliação

[Provável] Muito bom para puzzle avançado.

## Risco

Médio.

## Implementação

Exige snapshot temporário do estado da fase.

---

# 8.9 Câmera de Segurança

## Função

Mostra uma visão geral da fase ou destaca becos sem saída.

## Uso

```text
Revela áreas críticas do mapa por alguns segundos.
Preço: 90 créditos.
```

## Risco

Baixo.

---

# 8.10 Alarme de Erro

## Função

Avisa quando o jogador acabou de colocar uma caixa em uma posição irreversível.

## Uso

```text
Detecta possível deadlock uma vez.
Preço: 110 créditos.
```

## Avaliação

[Provável] Muito útil, mas tecnicamente mais difícil.

## Risco técnico

Alto, porque detectar deadlock corretamente pode ser complexo.

---

# 8.11 Reforço de Empilhadeira

## Função

Permite ao jogador usar a empilhadeira em uma fase específica para mover uma caixa pesada.

## Uso

```text
Ativa empilhadeira por 1 ação.
Preço: 250 créditos.
```

## Observação

Esse Power Up pode virar uma mecânica especial de mundo, não apenas item de loja.

## Risco

Alto.

---

# 8.12 Etiqueta de Destino

## Função

Coloca uma etiqueta colorida indicando qual caixa combina melhor com qual alvo.

## Uso

```text
Mostra par caixa/alvo por 10 segundos.
Preço: 60 créditos.
```

## Risco

Baixo.

## Recomendação

Ótimo para fases com caixas coloridas.

---

# 8.13 Café do Turno

## Função

Não altera a fase. Apenas aumenta a velocidade de animação e movimentação do personagem por alguns segundos.

## Uso

```text
Acelera animações por 20 segundos.
Preço: 30 créditos.
```

## Avaliação

[Provável] Bom para qualidade de vida, não para puzzle.

## Risco

Baixo.

---

# 8.14 Seguro de Carga

## Função

Permite errar uma vez sem perder a medalha “sem erro”.

## Uso

```text
Protege uma medalha de precisão uma vez.
Preço: 140 créditos.
```

## Risco

Médio.

---

# 8.15 Manual Técnico

## Função

Desbloqueia uma explicação da mecânica atual do mundo.

## Uso

```text
Mostra tutorial avançado da mecânica.
Preço: 25 créditos.
```

## Avaliação

[Certeza] É seguro porque educa o jogador.

## Risco

Baixo.

---

## 9. Categorias de Power Ups

Para organizar a loja, use categorias.

### Categoria 1 — Correção

Itens para corrigir erro:

```text
Rebobinar Movimento
Reposicionamento Manual
Macaco N-8
Seguro de Carga
```

### Categoria 2 — Informação

Itens que ajudam a entender a fase:

```text
Scanner Logístico
Assistente de Turno
Câmera de Segurança
Etiqueta de Destino
Alarme de Erro
Manual Técnico
```

### Categoria 3 — Ação

Itens que alteram fisicamente a fase:

```text
Força Hidráulica
Ímã de Carga
Bateria Reserva
Chave Mestra
Freio de Emergência
Reforço de Empilhadeira
```

### Categoria 4 — Qualidade de vida

Itens que melhoram experiência:

```text
Café do Turno
Marcador de Rota
Simulação Fantasma
```

---

## 10. Power Ups recomendados para o MVP

[Certeza] O MVP deve começar simples.

### Entram no MVP

```text
1. Rebobinar Movimento
2. Scanner Logístico
3. Assistente de Turno
4. Marcador de Rota
```

### Não entram no MVP

```text
Força Hidráulica
Macaco N-8
Reposicionamento Manual
Ímã de Carga
Chave Mestra
Reforço de Empilhadeira
Alarme de Erro avançado
```

### Motivo

Os Power Ups do MVP ajudam o jogador sem alterar radicalmente as regras da fase.

---

## 11. Power Ups por raridade

```text
Comum:
- Rebobinar Movimento
- Marcador de Rota
- Manual Técnico
- Café do Turno

Incomum:
- Scanner Logístico
- Etiqueta de Destino
- Câmera de Segurança
- Freio de Emergência

Raro:
- Assistente de Turno
- Simulação Fantasma
- Reposicionamento Manual
- Bateria Reserva

Épico:
- Macaco N-8
- Força Hidráulica
- Ímã de Carga
- Chave Mestra
- Reforço de Empilhadeira
```

---

## 12. Modelo de preços recomendado

```text
Rebobinar Movimento: 50
Marcador de Rota: 40
Manual Técnico: 25
Café do Turno: 30

Scanner Logístico: 80
Etiqueta de Destino: 60
Câmera de Segurança: 90
Freio de Emergência: 100

Assistente de Turno: 150
Simulação Fantasma: 150
Reposicionamento Manual: 180
Bateria Reserva: 130

Macaco N-8: 200
Força Hidráulica: 120
Ímã de Carga: 220
Chave Mestra: 250
Reforço de Empilhadeira: 250
```

---

## 13. Sistema de estoque da loja

Para evitar abuso, a loja pode ter estoque limitado por ciclo.

### Exemplo

```text
Oficina N-8 — Estoque do Dia

Rebobinar Movimento: 3 unidades
Scanner Logístico: 2 unidades
Assistente de Turno: 1 unidade
Macaco N-8: 1 unidade
```

### Reset

```text
O estoque renova ao completar um setor ou após desafio diário.
```

[Provável] Isso torna a loja mais interessante, mas aumenta complexidade. Não usar no MVP.

---

## 14. Upgrades permanentes

Além de Power Ups consumíveis, o jogador pode comprar melhorias permanentes.

### Exemplos

```text
Bolso Extra:
Aumenta o limite de Power Ups equipados por fase.

Manual Avançado:
Libera dicas de nível 2 em fases mais cedo.

Scanner Melhorado:
Scanner dura 2 segundos a mais.

Plano de Turno:
Permite salvar uma simulação fantasma.

Treinamento N-8:
Reduz o custo de Rebobinar Movimento em 10%.
```

### Cuidado

[Certeza] Upgrades permanentes podem quebrar balanceamento. Devem ser poucos e caros.

---

## 15. Equipamento antes da fase

Antes de iniciar uma fase, o jogador escolhe quais ferramentas levar.

### Exemplo de tela

```text
PREPARAR TURNO

Fase: Setor 02 - Correia Congelada
Créditos: 450

Slots de ferramenta: 2

[ ] Rebobinar Movimento
[ ] Scanner Logístico
[ ] Assistente de Turno
[ ] Marcador de Rota
[ ] Freio de Emergência

Iniciar Turno
```

### Regras

```text
MVP: 2 slots por fase.
Campanha avançada: até 3 slots.
Modo competitivo: 0 slots.
Modo assistido: até 3 slots.
```

---

## 16. Power Up Bar durante a fase

Durante a fase, mostrar uma barra simples:

```text
[Undo Extra x1] [Scanner x1] [Dica x2]
```

Estados visuais:

```text
Disponível
Usado
Bloqueado
Indisponível por modo competitivo
Sem créditos
```

---

## 17. Sistema de restrições por fase

Cada fase pode bloquear certos Power Ups.

### Exemplo

```json
{
  "level_id": "w02_010",
  "blocked_powerups": [
    "hydraulic_force",
    "jack_pull",
    "master_key"
  ]
}
```

### Motivo

[Certeza] Alguns Power Ups podem quebrar certas fases. O Level Designer precisa controlar isso por fase.

---

## 18. Sistema de penalidade

Usar Power Up não deve impedir o jogador de terminar o jogo, mas deve afetar ranking.

### Penalidades sugeridas

```text
Usou 1 Power Up:
- Ranking competitivo desativado.
- Medalha ouro limpa bloqueada.

Usou dica nível 1:
- Ranking ainda pode continuar, se o jogo quiser ser mais flexível.

Usou dica nível 3:
- Ranking competitivo desativado.

Usou Macaco N-8:
- Ranking competitivo desativado.
- Medalha perfeita bloqueada.
```

---

## 19. Conquistas relacionadas

Ideias de conquistas:

```text
Turno Limpo
Complete 10 fases sem usar Power Ups.

Sem Atalho
Complete um setor inteiro sem usar dicas.

Técnico Preparado
Compre seu primeiro Power Up.

Ferramenta Certa
Use o Scanner Logístico e conclua a fase em seguida.

Gerente de Turno
Acumule 5.000 Créditos de Turno.

Mão Pesada
Use Força Hidráulica pela primeira vez.

Sem Pânico
Use Rebobinar Movimento depois de prender uma caixa.

Planejador
Complete uma fase usando Marcador de Rota.

Modo Raiz
Complete o jogo sem Power Ups no modo competitivo.
```

---

## 20. Interface da loja

### Tela principal

```text
OFICINA N-8

Créditos disponíveis: 450

Correção
[ Rebobinar Movimento ]       50 créditos
[ Reposicionamento Manual ]   180 créditos
[ Macaco N-8 ]                200 créditos

Informação
[ Scanner Logístico ]         80 créditos
[ Assistente de Turno ]       150 créditos
[ Etiqueta de Destino ]       60 créditos

Ação
[ Força Hidráulica ]          120 créditos
[ Freio de Emergência ]       100 créditos
[ Bateria Reserva ]           130 créditos
```

### Tela de confirmação

```text
Comprar Scanner Logístico?

Preço: 80 Créditos de Turno
Você possui: 450

Descrição:
Destaca uma caixa crítica por alguns segundos.

[Comprar] [Cancelar]
```

---

## 21. Estrutura técnica em Godot

```text
game/
  scripts/
    systems/
      EconomySystem.gd
      PowerUpSystem.gd
      InventorySystem.gd
      ShopSystem.gd
      RankingRules.gd

    gameplay/
      LevelController.gd
      MoveCommand.gd
      UndoStack.gd
      HintController.gd
      RouteMarker.gd
      ScannerController.gd

  data/
    powerups/
      powerups.json

    economy/
      reward_rules.json

    shop/
      shop_catalog.json

  ui/
    ShopScreen.tscn
    PowerUpBar.tscn
    PreLevelLoadoutScreen.tscn
    RewardSummaryScreen.tscn
```

---

## 22. Exemplo de powerups.json

```json
{
  "undo_extra": {
    "name": "Rebobinar Movimento",
    "category": "correction",
    "price": 50,
    "rarity": "common",
    "max_per_level": 1,
    "ranking_allowed": false,
    "mvp": true
  },
  "scanner": {
    "name": "Scanner Logístico",
    "category": "information",
    "price": 80,
    "rarity": "uncommon",
    "max_per_level": 1,
    "ranking_allowed": false,
    "mvp": true
  },
  "hint_pack": {
    "name": "Assistente de Turno",
    "category": "information",
    "price": 150,
    "rarity": "rare",
    "max_per_level": 3,
    "ranking_allowed": false,
    "mvp": true
  },
  "route_marker": {
    "name": "Marcador de Rota",
    "category": "quality_of_life",
    "price": 40,
    "rarity": "common",
    "max_per_level": 1,
    "ranking_allowed": true,
    "mvp": true
  },
  "hydraulic_force": {
    "name": "Força Hidráulica",
    "category": "action",
    "price": 120,
    "rarity": "epic",
    "max_per_level": 1,
    "ranking_allowed": false,
    "mvp": false
  },
  "jack_pull": {
    "name": "Macaco N-8",
    "category": "correction",
    "price": 200,
    "rarity": "epic",
    "max_per_level": 1,
    "ranking_allowed": false,
    "mvp": false
  }
}
```

---

## 23. Exemplo de reward_rules.json

```json
{
  "base_complete": 100,
  "bronze_medal": 25,
  "silver_medal": 50,
  "gold_medal": 100,
  "no_hint_bonus": 50,
  "no_powerup_bonus": 50,
  "new_personal_record": 75,
  "daily_challenge_complete": 150,
  "sector_complete": 300,
  "first_try_bonus": 50,
  "under_push_limit_bonus": 75,
  "under_move_limit_bonus": 75
}
```

---

## 24. Estados que precisam ser salvos

O Save System precisa guardar:

```text
Créditos totais
Power Ups comprados
Power Ups consumidos
Upgrades permanentes
Fases concluídas
Medalhas
Recordes pessoais
Tentativas limpas
Tentativas assistidas
```

Exemplo:

```json
{
  "credits": 450,
  "inventory": {
    "undo_extra": 3,
    "scanner": 2,
    "hint_pack": 1
  },
  "upgrades": {
    "extra_tool_slot": false,
    "scanner_plus": false
  },
  "records": {
    "w01_001": {
      "best_moves_clean": 22,
      "best_pushes_clean": 7,
      "best_moves_assisted": 20,
      "completed": true
    }
  }
}
```

---

## 25. Critérios de aceite para implementação

### EconomySystem

```text
- Soma créditos ao concluir fase.
- Aplica bônus corretamente.
- Não permite saldo negativo.
- Persiste créditos no save.
```

### PowerUpSystem

```text
- Carrega powerups.json.
- Valida se Power Up está disponível.
- Valida limite por fase.
- Aplica efeito correto.
- Marca tentativa como assistida quando necessário.
```

### InventorySystem

```text
- Compra item.
- Consome item.
- Impede uso sem estoque.
- Salva inventário.
```

### ShopSystem

```text
- Lista itens da loja.
- Mostra preço.
- Mostra descrição.
- Bloqueia item sem crédito suficiente.
- Confirma compra antes de gastar.
```

### RankingRules

```text
- Desativa ranking competitivo ao usar Power Up proibido.
- Mantém ranking limpo se o item for permitido.
- Registra tentativa assistida separadamente.
```

---

## 26. Backlog para Claude + Codex

### Sprint 1 — Design e dados

```text
TW08-PU-001 Criar documento da Loja de Equipamentos N-8.
TW08-PU-002 Criar powerups.json.
TW08-PU-003 Criar reward_rules.json.
TW08-PU-004 Definir categorias e raridades.
TW08-PU-005 Definir quais Power Ups entram no MVP.
```

### Sprint 2 — Economia

```text
TW08-PU-010 Criar EconomySystem.gd.
TW08-PU-011 Implementar cálculo de recompensa por fase.
TW08-PU-012 Implementar bônus por medalha.
TW08-PU-013 Implementar bônus sem dica.
TW08-PU-014 Implementar bônus sem Power Up.
TW08-PU-015 Persistir créditos no SaveSystem.
```

### Sprint 3 — Inventário

```text
TW08-PU-020 Criar InventorySystem.gd.
TW08-PU-021 Implementar compra de item.
TW08-PU-022 Implementar consumo de item.
TW08-PU-023 Implementar limite por fase.
TW08-PU-024 Salvar inventário.
```

### Sprint 4 — Power Ups MVP

```text
TW08-PU-030 Implementar Rebobinar Movimento.
TW08-PU-031 Implementar Scanner Logístico.
TW08-PU-032 Implementar Assistente de Turno.
TW08-PU-033 Implementar Marcador de Rota.
```

### Sprint 5 — Interface

```text
TW08-PU-040 Criar ShopScreen.tscn.
TW08-PU-041 Criar PowerUpBar.tscn.
TW08-PU-042 Criar PreLevelLoadoutScreen.tscn.
TW08-PU-043 Criar RewardSummaryScreen.tscn.
```

### Sprint 6 — Ranking

```text
TW08-PU-050 Criar RankingRules.gd.
TW08-PU-051 Separar tentativa limpa e assistida.
TW08-PU-052 Bloquear ranking competitivo após Power Up.
TW08-PU-053 Exibir aviso de modo assistido.
```

---

## 27. Prompt para Claude

```text
Você é o arquiteto técnico e game designer sênior do projeto The Warehouse Nº 08.

Objetivo:
Revisar, expandir e proteger o design do sistema de Loja de Equipamentos N-8, economia interna e Power Ups.

Contexto:
The Warehouse Nº 08 é um puzzle game original de logística/armazém inspirado no gênero Sokoban. O sistema de Power Ups deve ajudar o jogador sem quebrar o puzzle.

Regras:
- Não propor microtransações no MVP.
- Não criar Power Ups que resolvam a fase automaticamente.
- Separar ranking limpo e ranking assistido.
- Classificar riscos como baixo, médio ou alto.
- Priorizar UX, balanceamento, acessibilidade e implementação em Godot 4.
- Criar tarefas pequenas para Codex.
- Informar como validar cada sistema.

Entregáveis:
1. Revisão do design de Power Ups.
2. Lista de riscos.
3. Ajustes de balanceamento.
4. Backlog para Codex.
5. Critérios de aceite.
6. Sugestões de UX para loja e barra de Power Ups.
```

---

## 28. Prompt para Codex

```text
Você é o implementador técnico do projeto The Warehouse Nº 08 em Godot 4.

Objetivo:
Implementar o sistema inicial da Loja de Equipamentos N-8, economia interna, inventário e Power Ups do MVP.

Arquivos esperados:
- scripts/systems/EconomySystem.gd
- scripts/systems/InventorySystem.gd
- scripts/systems/PowerUpSystem.gd
- scripts/systems/ShopSystem.gd
- scripts/systems/RankingRules.gd
- data/powerups/powerups.json
- data/economy/reward_rules.json
- ui/ShopScreen.tscn
- ui/PowerUpBar.tscn

Regras:
- Não implementar microtransação.
- Não implementar ranking online agora.
- Não quebrar o core de movimento do puzzle.
- Separar tentativa limpa e assistida.
- Criar testes ou cenários manuais de validação.
- Não alterar arquivos fora do escopo sem justificar.

Power Ups do MVP:
1. Rebobinar Movimento.
2. Scanner Logístico.
3. Assistente de Turno.
4. Marcador de Rota.

Depois de implementar:
- Liste arquivos alterados.
- Explique cada mudança.
- Informe testes executados.
- Informe se foi validado por execução ou leitura estática.
- Informe riscos restantes.
```

---

## 29. Riscos principais

### Risco 1 — Quebrar o puzzle

[Certeza] Itens como puxar caixa, empurrar 2 casas e reposicionar caixa podem invalidar o design das fases.

Mitigação:

```text
Bloquear esses itens no ranking competitivo.
Permitir bloqueio por fase.
Testar fase com e sem Power Ups.
```

### Risco 2 — Economia fácil demais

[Provável] Se o jogador ganhar muitos créditos, a loja vira infinita.

Mitigação:

```text
Controlar recompensas.
Limitar estoque.
Aumentar custo de itens fortes.
Separar consumíveis de upgrades permanentes.
```

### Risco 3 — Loja cansativa

[Provável] Se a loja tiver itens demais no começo, o jogador se perde.

Mitigação:

```text
MVP com poucos itens.
Categorias simples.
Descrições curtas.
Ícones claros.
```

### Risco 4 — Ranking injusto

[Certeza] Jogador com Power Up não pode competir diretamente com jogador sem Power Up.

Mitigação:

```text
Ranking limpo separado de ranking assistido.
Tentativas com Power Up marcadas claramente.
```

---

## 30. Conclusão

A melhor direção para **The Warehouse Nº 08** é tratar Power Ups como ferramentas de armazém, não como poderes mágicos.

Modelo recomendado:

```text
Power Ups = ferramentas de armazém
Moeda = Créditos de Turno
Loja = Oficina N-8
Ranking competitivo = sem Power Ups
Ranking casual = com Power Ups permitido
MVP = Rebobinar Movimento + Scanner Logístico + Assistente de Turno + Marcador de Rota
```

[Certeza] O primeiro Power Up a implementar deve ser **Rebobinar Movimento**.

[Certeza] O segundo deve ser **Scanner Logístico**.

[Provável] O melhor diferencial moderno é **Simulação Fantasma**, mas ela deve vir depois do MVP porque exige mais estrutura técnica.

---

## 31. Status de validação

```text
Tipo de documento: Design técnico inicial.
Validação: Não validado em gameplay ainda.
Base: Análise de design, UX, balanceamento e arquitetura para Godot.
Próximo passo: prototipar Power Ups do MVP e testar em 10 fases originais.
```
