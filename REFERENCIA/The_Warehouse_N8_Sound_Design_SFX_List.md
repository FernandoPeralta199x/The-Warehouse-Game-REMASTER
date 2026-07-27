# The Warehouse Nº 08 — Sound Design e SFX List Completa

> Documento de direção sonora, lista de efeitos sonoros e organização técnica de áudio para **The Warehouse Nº 08**.  
> Inclui SFX de personagem, caixas, empilhadeiras, máquinas, ambientes, Power Ups, UI, fases secretas, Setor 08, feedback de puzzle e integração com Godot.

---

## 1. Veredito técnico

[Certeza] O áudio de **The Warehouse Nº 08** precisa ser funcional antes de ser realista.

O jogo é um puzzle industrial com pixel art, empilhadeiras, caixas, esteiras, sensores, armazém, mistério e narrativa emocional. Portanto, os SFX precisam cumprir três funções:

```text
1. Informar o jogador.
2. Reforçar o peso industrial do mundo.
3. Dar identidade ao Armazém Nº 08.
```

O objetivo não é criar sons cinematográficos longos e realistas demais. O objetivo é criar sons:

```text
curtos;
claros;
legíveis;
repetíveis;
com identidade;
bons para loop;
não cansativos;
facilmente reconhecíveis durante o gameplay.
```

---

## 2. Filosofia sonora do jogo

A trilha musical pode ser industrial, synthwave e chiptune, mas os SFX precisam ficar entre:

```text
foley industrial;
sons retrô;
máquinas reais simplificadas;
bips técnicos;
feedback arcade discreto;
ruídos mecânicos curtos;
texturas de armazém.
```

A identidade sonora geral deve parecer:

```text
um armazém antigo tentando funcionar com tecnologia moderna falhando.
```

---

## 3. Pilares de áudio

## 3.1 Clareza

O jogador precisa entender pelo som:

```text
caixa empurrou;
caixa travou;
caixa encaixou;
sensor ativou;
porta abriu;
Power Up foi usado;
tentativa virou assistida;
empilhadeira bateu;
carga foi danificada;
fase foi concluída.
```

---

## 3.2 Peso

Caixas, máquinas, portas, empilhadeiras e ferramentas precisam soar pesadas.

Mesmo com pixel art, o jogo deve transmitir:

```text
metal;
madeira;
concreto;
borracha;
correntes;
hidráulica;
energia elétrica;
máquina antiga.
```

---

## 3.3 Repetição confortável

Como o jogador vai ouvir passos, empurrões e UI muitas vezes, esses sons precisam ter variações.

Exemplo:

```text
step_concrete_01.wav
step_concrete_02.wav
step_concrete_03.wav

crate_push_wood_01.wav
crate_push_wood_02.wav
crate_push_wood_03.wav
```

[Certeza] Nunca usar apenas um som de passo ou empurrão repetindo igual. Isso causa fadiga auditiva.

---

## 3.4 Narrativa sonora

O Armazém Nº 08 deve contar história pelo som:

```text
Setor 01: sons leves de doca.
Setor 03: freezer, gelo, eco frio.
Setor 04: glitches, esteiras, robôs.
Setor 05: máquinas antigas e geradores.
Setor 06: ruídos apagados, alarmes distantes.
Setor 08: núcleo instável, energia, sinais corrompidos.
```

---

# 4. Estrutura recomendada de pastas

```text
audio/
  sfx/
    player/
      footsteps/
      movement/
      interactions/

    crates/
      push/
      hit/
      slide/
      target/
      heavy/
      fragile/
      metal/

    forklift/
      engine/
      movement/
      cargo/
      collision/
      alerts/
      race/

    machines/
      doors/
      conveyors/
      generators/
      terminals/
      sensors/
      robots/
      hydraulics/

    powerups/
      scanner/
      rewind/
      hints/
      route_marker/
      hydraulic/
      jack/
      emergency/

    ui/
      menu/
      buttons/
      shop/
      medals/
      warnings/
      results/

    ambience/
      sectors/
      loops/
      one_shots/
      secret/

    story/
      radio/
      logs/
      glitches/
      character_stingers/

    puzzle_feedback/
      success/
      error/
      deadlock/
      unlock/
      objective/

  music/
    loops/
    stingers/
    endings/
```

---

# 5. Convenção de nomes

## 5.1 Regra geral

```text
categoria_acao_material_variacao.wav
```

Exemplos:

```text
crate_push_wood_01.wav
crate_hit_metal_02.wav
door_open_heavy_01.wav
forklift_engine_idle_loop.wav
ui_confirm_01.wav
powerup_scanner_activate_01.wav
```

---

## 5.2 Sufixos recomendados

```text
_loop = áudio em loop
_one_shot = som único
_stinger = som curto de transição
_var = variação
_dry = sem efeito
_wet = com reverb/ambiente
```

Exemplos:

```text
conveyor_belt_loop.wav
sector03_freezer_ambience_loop.wav
victory_gold_stinger.wav
terminal_boot_one_shot.wav
```

---

# 6. Configuração técnica recomendada

## 6.1 Formatos

```text
SFX curtos: WAV, 44.1 kHz, 16-bit ou 24-bit
Loops longos: OGG Vorbis
Música: OGG Vorbis
Protótipo: WAV simples
Build final: OGG para loops e música; WAV para SFX críticos
```

---

## 6.2 Duração por tipo

```text
Passos: 0.08s a 0.20s
Empurrar caixa: 0.20s a 0.45s
Impacto de caixa: 0.15s a 0.50s
Sensor: 0.20s a 0.60s
Porta pesada: 0.80s a 2.50s
UI click: 0.04s a 0.12s
Power Up: 0.40s a 1.20s
Vitória: 2s a 5s
Falha: 0.60s a 2s
Ambiente loop: 20s a 60s
Motor loop: 2s a 8s
Esteira loop: 2s a 8s
```

---

## 6.3 Volume relativo sugerido

Escala de referência:

```text
Música: -18 dB a -14 dB
Ambiente: -24 dB a -18 dB
Passos: -20 dB a -16 dB
Caixas: -14 dB a -10 dB
Empilhadeira: -16 dB a -10 dB
UI: -18 dB a -12 dB
Alertas: -12 dB a -8 dB
Vitória/falha: -12 dB a -8 dB
```

[Certeza] Alertas e feedback de puzzle devem ficar acima do ambiente, mas não podem brigar com a música.

---

# 7. Prioridades de implementação

## Prioridade P0 — Obrigatório para protótipo

```text
passos básicos;
empurrar caixa;
caixa bater;
caixa no alvo;
porta abrir/fechar;
sensor ativar/desativar;
UI confirmar/voltar;
vitória;
falha;
empilhadeira motor básico;
empilhadeira ré;
empilhadeira colisão;
terminal ativar.
```

## Prioridade P1 — Vertical slice

```text
variações de passos;
variações de caixas;
Power Ups do MVP;
loops de ambiente por setor;
esteira;
gerador;
alarme de lockdown;
corrida de empilhadeira completa;
medalhas;
loja Oficina N-8.
```

## Prioridade P2 — Campanha completa

```text
SFX por setor;
sons exclusivos de Duda;
sons exclusivos de Robert;
sons de Elias/registros apagados;
Setor 08;
fases secretas;
N-8 Heavy;
carga frágil;
gelo;
robôs.
```

## Prioridade P3 — Polimento

```text
variações extras;
camadas dinâmicas por tensão;
mix adaptativo;
reverb por ambiente;
sons raros;
stingers narrativos;
versões alternativas para skins.
```

---

# 8. SFX do jogador — John Miller

## 8.1 Passos

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `step_concrete_01.wav` | Passo curto em concreto seco | 0.12s | P0 | Setores 01, 02 |
| `step_concrete_02.wav` | Variação de passo em concreto | 0.12s | P0 | Anti-repetição |
| `step_concrete_03.wav` | Variação mais grave | 0.14s | P1 | Anti-repetição |
| `step_metal_01.wav` | Passo em chapa metálica | 0.13s | P1 | Oficina, plataformas |
| `step_metal_02.wav` | Variação metálica | 0.13s | P1 | Oficina |
| `step_ice_01.wav` | Passo leve em gelo | 0.16s | P1 | Câmara Fria |
| `step_ice_slip_01.wav` | Pequeno escorregão | 0.35s | P1 | Piso gelado |
| `step_water_01.wav` | Passo em piso molhado | 0.18s | P2 | Setores especiais |
| `step_heavy_boot_01.wav` | Bota pesada em setor avançado | 0.16s | P2 | Setor 08 |

## 8.2 Movimento e esforço

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `player_push_effort_01.wav` | Esforço curto ao empurrar caixa | 0.20s | P1 | Caixas pesadas |
| `player_push_effort_02.wav` | Variação de esforço | 0.22s | P2 | Anti-repetição |
| `player_blocked_01.wav` | Som seco indicando bloqueio | 0.15s | P0 | Tentou empurrar algo travado |
| `player_turn_gear_01.wav` | Pequeno rangido de roupa/equipamento | 0.10s | P3 | Polimento |
| `player_idle_tool_jingle_01.wav` | Ferramentas no cinto mexendo | 0.40s | P3 | Idle raro |

---

# 9. SFX de caixas

## 9.1 Caixa de madeira normal

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `crate_push_wood_01.wav` | Caixa de madeira arrastando no concreto | 0.30s | P0 | Empurrão básico |
| `crate_push_wood_02.wav` | Variação mais grave | 0.32s | P0 | Anti-repetição |
| `crate_push_wood_03.wav` | Variação com rangido | 0.34s | P1 | Anti-repetição |
| `crate_stop_wood_01.wav` | Caixa parando após empurrão | 0.12s | P0 | Fim do movimento |
| `crate_hit_wall_wood_01.wav` | Madeira batendo na parede | 0.25s | P0 | Colisão |
| `crate_hit_wall_wood_02.wav` | Variação de impacto | 0.28s | P1 | Colisão |
| `crate_on_target_01.wav` | Caixa encaixa no alvo | 0.35s | P0 | Feedback positivo |
| `crate_off_target_01.wav` | Caixa sai do alvo | 0.22s | P1 | Feedback |

## 9.2 Caixa metálica

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `crate_push_metal_01.wav` | Caixa metálica arrastando | 0.35s | P1 | Setor 04/05 |
| `crate_push_metal_02.wav` | Variação metálica | 0.36s | P1 | Anti-repetição |
| `crate_hit_metal_01.wav` | Impacto metálico seco | 0.30s | P1 | Colisão |
| `crate_hit_metal_02.wav` | Impacto com ressonância | 0.45s | P2 | Colisão pesada |

## 9.3 Caixa pesada

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `crate_heavy_push_01.wav` | Arrasto pesado e grave | 0.45s | P1 | Manutenção Pesada |
| `crate_heavy_push_02.wav` | Variação com metal baixo | 0.48s | P2 | Anti-repetição |
| `crate_heavy_stop_01.wav` | Parada forte no chão | 0.25s | P1 | Fim do movimento |
| `crate_heavy_blocked_01.wav` | Tentativa falha de empurrar | 0.25s | P1 | Caixa pesada sem ferramenta |
| `crate_heavy_sensor_drop_01.wav` | Peso ativando sensor | 0.40s | P2 | Sensor pesado |

## 9.4 Caixa frágil

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `crate_fragile_push_01.wav` | Arrasto leve com vidro interno | 0.30s | P2 | Carga frágil |
| `crate_fragile_rattle_01.wav` | Vidro tremendo | 0.50s | P2 | Instabilidade |
| `crate_fragile_damage_01.wav` | Pequeno estalo de dano | 0.35s | P2 | Dano leve |
| `crate_fragile_break_01.wav` | Quebra parcial | 0.80s | P2 | Falha de carga |
| `crate_fragile_success_01.wav` | Entrega delicada concluída | 0.60s | P2 | Entrega perfeita |

## 9.5 Caixa no gelo

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `crate_slide_ice_01.wav` | Caixa deslizando no gelo | 0.60s | P1 | Câmara Fria |
| `crate_slide_ice_02.wav` | Variação de deslizamento | 0.65s | P2 | Anti-repetição |
| `crate_ice_stop_01.wav` | Caixa parando no gelo | 0.22s | P1 | Câmara Fria |
| `crate_ice_hit_01.wav` | Caixa batendo com eco frio | 0.35s | P1 | Colisão no gelo |

---

# 10. SFX de empilhadeiras

## 10.1 Motor e movimento

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `forklift_engine_idle_loop.wav` | Motor parado em loop | 4.00s | P0 | Empilhadeira parada |
| `forklift_engine_low_loop.wav` | Motor em baixa velocidade | 3.00s | P0 | Movimento lento |
| `forklift_engine_high_loop.wav` | Motor em alta velocidade | 3.00s | P1 | Corrida |
| `forklift_accelerate_01.wav` | Aceleração curta | 0.80s | P0 | Início de movimento |
| `forklift_decelerate_01.wav` | Redução de motor | 0.70s | P1 | Freio leve |
| `forklift_reverse_beep_loop.wav` | Bip de ré em loop | 1.00s | P0 | Ré |
| `forklift_brake_01.wav` | Freio curto | 0.35s | P0 | Frenagem |
| `forklift_handbrake_01.wav` | Freio brusco | 0.50s | P1 | Freio forte |
| `forklift_tire_screech_01.wav` | Pneu arrastando | 0.45s | P1 | Curva forte |
| `forklift_ice_skid_01.wav` | Derrapagem no gelo | 0.70s | P2 | Câmara Fria |

## 10.2 Garfo e carga

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `forklift_fork_raise_01.wav` | Garfo subindo | 0.70s | P1 | Pegar carga |
| `forklift_fork_lower_01.wav` | Garfo descendo | 0.70s | P1 | Soltar carga |
| `forklift_pickup_crate_01.wav` | Carga acoplando ao garfo | 0.45s | P0 | Coleta |
| `forklift_drop_crate_01.wav` | Carga solta no chão | 0.40s | P0 | Soltar |
| `forklift_cargo_lock_01.wav` | Carga encaixada corretamente | 0.30s | P1 | Feedback positivo |
| `forklift_cargo_unstable_01.wav` | Carga balançando | 0.60s | P1 | Instabilidade |
| `forklift_cargo_fall_01.wav` | Carga caindo | 1.00s | P2 | Falha grave |

## 10.3 Colisões de empilhadeira

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `forklift_collision_light_01.wav` | Batida leve | 0.25s | P0 | Colisão leve |
| `forklift_collision_medium_01.wav` | Batida média | 0.40s | P0 | Colisão média |
| `forklift_collision_heavy_01.wav` | Batida forte | 0.65s | P1 | Colisão forte |
| `forklift_hit_cone_01.wav` | Cone sendo atingido | 0.25s | P1 | Corrida |
| `forklift_hit_pallet_01.wav` | Palete atingido | 0.35s | P1 | Obstáculo |
| `forklift_damage_alarm_01.wav` | Alerta de dano | 0.80s | P1 | Carga danificada |

## 10.4 Modelos de empilhadeira

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `forklift_standard_engine_loop.wav` | Motor N-8 Standard | 4.00s | P1 | Standard |
| `forklift_heavy_engine_loop.wav` | Motor grave N-8 Heavy | 4.00s | P1 | Heavy |
| `forklift_light_engine_loop.wav` | Motor mais agudo e rápido | 3.00s | P2 | Light |
| `forklift_cold_engine_loop.wav` | Motor abafado em frio | 4.00s | P2 | Cold Storage |
| `forklift_electric_engine_loop.wav` | Motor elétrico suave | 3.00s | P2 | Electric |
| `forklift_prototype_engine_loop.wav` | Motor futurista instável | 3.00s | P3 | Prototype |

---

# 11. SFX de máquinas e ambiente industrial

## 11.1 Esteiras

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `conveyor_belt_loop.wav` | Esteira padrão em loop | 4.00s | P1 | Setor 04 |
| `conveyor_start_01.wav` | Esteira ligando | 0.80s | P1 | Ativação |
| `conveyor_stop_01.wav` | Esteira parando | 0.70s | P1 | Desativação |
| `conveyor_reverse_01.wav` | Esteira invertendo direção | 0.50s | P2 | Botão de direção |
| `conveyor_jam_01.wav` | Esteira travando | 0.90s | P2 | Falha |

## 11.2 Portas e portões

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `door_open_light_01.wav` | Porta pequena abrindo | 0.70s | P0 | Porta simples |
| `door_close_light_01.wav` | Porta pequena fechando | 0.60s | P0 | Porta simples |
| `door_open_heavy_01.wav` | Portão industrial abrindo | 1.80s | P0 | Doca |
| `door_close_heavy_01.wav` | Portão industrial fechando | 1.80s | P0 | Doca |
| `door_locked_01.wav` | Porta travada | 0.30s | P0 | Feedback erro |
| `door_unlock_01.wav` | Trava liberada | 0.50s | P0 | Sensor abriu |
| `door_emergency_shut_01.wav` | Fechamento de emergência | 1.20s | P1 | Lockdown |
| `door_manual_crank_01.wav` | Manivela manual | 1.50s | P2 | Robert/Oficina |

## 11.3 Geradores e energia

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `generator_idle_loop.wav` | Gerador em loop | 6.00s | P1 | Oficina |
| `generator_start_01.wav` | Gerador ligando | 2.50s | P1 | Missão Robert |
| `generator_fail_01.wav` | Gerador falhando | 1.50s | P1 | Falha |
| `power_down_01.wav` | Energia caindo | 1.20s | P1 | Lockdown |
| `power_up_01.wav` | Energia voltando | 1.40s | P1 | Sucesso |
| `electric_spark_01.wav` | Faísca elétrica curta | 0.20s | P2 | Setor 08 |
| `electric_spark_02.wav` | Variação de faísca | 0.22s | P2 | Setor 08 |

## 11.4 Terminais e computadores

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `terminal_boot_01.wav` | Terminal ligando | 1.00s | P0 | Interação |
| `terminal_beep_01.wav` | Bip simples | 0.12s | P0 | UI terminal |
| `terminal_access_granted_01.wav` | Acesso liberado | 0.45s | P0 | Sucesso |
| `terminal_access_denied_01.wav` | Acesso negado | 0.45s | P0 | Erro |
| `terminal_glitch_01.wav` | Glitch de dado | 0.60s | P1 | Duda/Elias |
| `terminal_data_recover_01.wav` | Arquivo recuperado | 0.90s | P1 | Log encontrado |
| `terminal_data_deleted_01.wav` | Arquivo apagado | 0.70s | P2 | Narrativa |

## 11.5 Sensores e botões

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `sensor_activate_01.wav` | Sensor ativado | 0.25s | P0 | Sensor de peso |
| `sensor_deactivate_01.wav` | Sensor desativado | 0.25s | P0 | Sensor |
| `sensor_false_01.wav` | Sensor falso/glitch | 0.45s | P1 | Setor 04 |
| `button_press_01.wav` | Botão pressionado | 0.12s | P0 | Botão |
| `lever_pull_01.wav` | Alavanca puxada | 0.50s | P1 | Portas |
| `switch_power_01.wav` | Chave de energia | 0.35s | P1 | Oficina |

## 11.6 Robôs

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `robot_move_loop.wav` | Robô se movendo | 2.00s | P2 | Setor 04 |
| `robot_turn_01.wav` | Robô virando | 0.20s | P2 | Por turno |
| `robot_scan_01.wav` | Scanner de robô | 0.50s | P2 | Detecção |
| `robot_error_01.wav` | Robô travando | 0.70s | P2 | Falha |
| `robot_shutdown_01.wav` | Robô desligado | 0.90s | P2 | Freio de Emergência |

---

# 12. SFX de Power Ups

## 12.1 Power Ups MVP

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `powerup_rewind_activate_01.wav` | Rebobinar Movimento ativado | 0.80s | P0 | Power Up |
| `powerup_rewind_tick_01.wav` | Passo voltando no tempo | 0.15s | P1 | Cada movimento revertido |
| `powerup_scanner_activate_01.wav` | Scanner Logístico ativado | 0.70s | P0 | Scanner |
| `powerup_scanner_ping_01.wav` | Ping em caixa crítica | 0.35s | P0 | Destaque |
| `powerup_hint_open_01.wav` | Assistente de Turno aberto | 0.40s | P0 | Dica |
| `powerup_route_marker_01.wav` | Marcador de Rota ativado | 0.50s | P0 | Planejamento |
| `powerup_route_draw_01.wav` | Linha sendo desenhada | 0.80s | P1 | Marcador |

## 12.2 Power Ups avançados

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `powerup_hydraulic_force_01.wav` | Força Hidráulica | 0.90s | P2 | Empurrão forte |
| `powerup_jack_pull_01.wav` | Macaco N-8 puxando caixa | 1.00s | P2 | Ferramenta rara |
| `powerup_battery_reserve_01.wav` | Bateria Reserva ativada | 0.80s | P2 | Energia extra |
| `powerup_emergency_brake_01.wav` | Freio de Emergência | 0.60s | P2 | Esteira/robô |
| `powerup_master_key_01.wav` | Chave Mestra | 0.70s | P3 | Porta especial |
| `powerup_ghost_sim_start_01.wav` | Simulação Fantasma inicia | 0.80s | P3 | Pós-MVP |
| `powerup_ghost_sim_cancel_01.wav` | Simulação cancelada | 0.50s | P3 | Pós-MVP |
| `powerup_ghost_sim_confirm_01.wav` | Simulação confirmada | 0.70s | P3 | Pós-MVP |

---

# 13. SFX de UI e HUD

## 13.1 Menu e botões

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `ui_select_01.wav` | Navegar no menu | 0.08s | P0 | UI |
| `ui_confirm_01.wav` | Confirmar | 0.10s | P0 | UI |
| `ui_back_01.wav` | Voltar | 0.10s | P0 | UI |
| `ui_error_01.wav` | Erro | 0.20s | P0 | UI |
| `ui_pause_01.wav` | Pausar | 0.20s | P1 | Pause |
| `ui_unpause_01.wav` | Retomar | 0.20s | P1 | Pause |
| `ui_tab_change_01.wav` | Troca de aba | 0.12s | P1 | Loja |

## 13.2 Oficina N-8 / Loja

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `shop_open_01.wav` | Abrir Oficina N-8 | 0.60s | P1 | Loja |
| `shop_close_01.wav` | Fechar loja | 0.50s | P1 | Loja |
| `shop_buy_01.wav` | Comprar item | 0.45s | P1 | Compra |
| `shop_no_credits_01.wav` | Créditos insuficientes | 0.35s | P1 | Erro |
| `credits_gain_01.wav` | Ganhar créditos | 0.60s | P0 | Resultado |
| `credits_count_loop.wav` | Contagem de créditos | 1.00s | P2 | Tela final |

## 13.3 Medalhas e resultado

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `result_screen_open_01.wav` | Tela de resultado | 0.70s | P0 | Fim da fase |
| `medal_bronze_01.wav` | Medalha bronze | 1.00s | P0 | Resultado |
| `medal_silver_01.wav` | Medalha prata | 1.10s | P0 | Resultado |
| `medal_gold_01.wav` | Medalha ouro | 1.30s | P0 | Resultado |
| `medal_platinum_01.wav` | Medalha platina | 1.60s | P1 | Resultado perfeito |
| `clean_run_badge_01.wav` | Tentativa limpa | 0.90s | P1 | Ranking limpo |
| `assisted_run_badge_01.wav` | Tentativa assistida | 0.70s | P1 | Power Up usado |

---

# 14. Feedback de puzzle

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `puzzle_success_01.wav` | Objetivo parcial concluído | 0.45s | P0 | Caixa no alvo/sensor |
| `puzzle_complete_01.wav` | Fase concluída | 2.50s | P0 | Vitória |
| `puzzle_deadlock_warning_01.wav` | Aviso de possível travamento | 0.50s | P2 | Alarme de Erro |
| `puzzle_invalid_move_01.wav` | Movimento inválido | 0.15s | P0 | Caixa bloqueada |
| `puzzle_unlock_path_01.wav` | Caminho liberado | 0.80s | P0 | Porta/rota |
| `puzzle_secret_found_01.wav` | Segredo encontrado | 1.20s | P1 | Fase secreta/log |
| `puzzle_map_reveal_01.wav` | Parte do mapa revelada | 1.00s | P1 | Rotas Fantasma |
| `puzzle_target_order_wrong_01.wav` | Ordem errada de alvo | 0.40s | P2 | Caixas marcadas |
| `puzzle_target_order_correct_01.wav` | Ordem correta de alvo | 0.45s | P2 | Caixas marcadas |

---

# 15. Ambientes por setor

## 15.1 Setor 01 — Recebimento

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector01_recebimento_loop.ogg` | Doca leve, hum industrial baixo | 40s | P1 |
| `amb_forklift_distant_01.wav` | Empilhadeira distante | 3s | P2 |
| `amb_pallet_shift_01.wav` | Palete mexendo ao longe | 1s | P3 |
| `amb_loading_dock_air_01.wav` | Vento leve de doca | 8s | P3 |

## 15.2 Setor 02 — Expedição

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector02_expedicao_loop.ogg` | Doca ativa, portões, armazém vivo | 45s | P1 |
| `amb_truck_reverse_distant_01.wav` | Caminhão/alarme distante | 4s | P2 |
| `amb_dock_gate_rattle_01.wav` | Portão vibrando | 2s | P2 |

## 15.3 Setor 03 — Câmara Fria

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector03_camara_fria_loop.ogg` | Freezer, vento frio, hum grave | 45s | P1 |
| `amb_ice_crack_01.wav` | Estalo de gelo | 1s | P2 |
| `amb_freezer_pressure_01.wav` | Pressão/ventilação fria | 5s | P2 |

## 15.4 Setor 04 — Automação

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector04_automacao_loop.ogg` | Robôs, esteiras e glitches | 45s | P1 |
| `amb_robot_distant_01.wav` | Robô distante | 3s | P2 |
| `amb_system_glitch_01.wav` | Glitch ocasional | 1s | P2 |
| `amb_conveyor_far_loop.ogg` | Esteira distante | 20s | P2 |

## 15.5 Setor 05 — Manutenção Pesada

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector05_manutencao_loop.ogg` | Geradores, metal, oficina pesada | 50s | P1 |
| `amb_pipe_knock_01.wav` | Cano batendo | 1s | P2 |
| `amb_tool_drop_01.wav` | Ferramenta caindo ao longe | 1s | P2 |
| `amb_generator_far_01.wav` | Gerador distante | 5s | P2 |

## 15.6 Setor 06 — Rotas Fantasma

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector06_rotas_fantasma_loop.ogg` | Setor esquecido, eco e alarmes distantes | 55s | P1 |
| `amb_old_speaker_noise_01.wav` | Alto-falante velho com ruído | 2s | P2 |
| `amb_map_glitch_01.wav` | Mapa falhando | 1s | P2 |
| `amb_distant_alarm_soft_01.wav` | Alarme muito distante | 4s | P2 |

## 15.7 Setor 08 — Núcleo Logístico

| Arquivo | Descrição | Duração | Prioridade |
|---|---|---:|---|
| `amb_sector08_core_loop.ogg` | Núcleo elétrico, tensão e máquina central | 60s | P1 |
| `amb_core_pulse_01.wav` | Pulso grave do núcleo | 2s | P1 |
| `amb_core_warning_01.wav` | Alerta do núcleo | 1s | P1 |
| `amb_data_stream_01.wav` | Dados correndo/glitch | 3s | P2 |

---

# 16. Sons narrativos e personagens

## 16.1 Duda

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `story_duda_log_start_01.wav` | Início de mensagem da Duda | 0.60s | P1 | Logs |
| `story_duda_log_end_01.wav` | Final de mensagem | 0.50s | P1 | Logs |
| `story_duda_signal_glitch_01.wav` | Sinal falhando | 0.80s | P1 | Terminal |
| `story_duda_clue_reveal_01.wav` | Pista importante revelada | 1.00s | P1 | História |
| `story_duda_emotional_stinger_01.wav` | Momento emocional | 2.00s | P2 | Cena |

## 16.2 Robert

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `story_robert_radio_on_01.wav` | Rádio do Robert liga | 0.40s | P1 | Diálogo |
| `story_robert_radio_off_01.wav` | Rádio desliga | 0.30s | P1 | Diálogo |
| `story_robert_tool_rattle_01.wav` | Ferramentas do Robert | 0.50s | P2 | Oficina |
| `story_robert_workshop_stinger_01.wav` | Stinger amigável/oficina | 1.20s | P2 | Robert |
| `story_robert_serious_stinger_01.wav` | Robert fica sério | 1.50s | P2 | Virada |

## 16.3 Elias

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `story_elias_record_found_01.wav` | Registro de Elias encontrado | 1.20s | P1 | Segredo |
| `story_elias_data_missing_01.wav` | Dados apagados | 0.90s | P1 | Terminal |
| `story_elias_static_01.wav` | Estática incompleta | 1.00s | P2 | Suspense |
| `story_elias_last_log_01.wav` | Último log revelado | 2.00s | P2 | Secreta 09 |

---

# 17. Sons de fases secretas

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `secret_unlock_01.wav` | Fase secreta desbloqueada | 1.50s | P1 | Desbloqueio |
| `secret_room_enter_01.wav` | Entrada em sala secreta | 1.20s | P1 | Secreta |
| `secret_clue_pickup_01.wav` | Pista secreta coletada | 0.70s | P1 | Logs |
| `secret_duda_path_01.wav` | Caminho da Duda revelado | 2.00s | P2 | Secreta 10 |
| `secret_elias_truth_01.wav` | Verdade de Elias | 2.00s | P2 | Secreta 09 |
| `secret_final_complete_01.wav` | Final secreto concluído | 3.00s | P2 | Final secreto |

---

# 18. Sons de sistema e alertas

| Arquivo | Descrição | Duração | Prioridade | Uso |
|---|---|---:|---|---|
| `system_warning_01.wav` | Aviso comum | 0.60s | P0 | Alerta |
| `system_error_01.wav` | Erro de sistema | 0.50s | P0 | Erro |
| `system_lockdown_start_01.wav` | Lockdown iniciado | 2.00s | P1 | História |
| `system_lockdown_loop.ogg` | Alarme de lockdown em loop | 10s | P1 | Fase 29 |
| `system_route_denied_01.wav` | Rota negada | 0.50s | P1 | Terminal |
| `system_route_authorized_01.wav` | Rota autorizada | 0.50s | P1 | Porta |
| `system_sector_unlocked_01.wav` | Setor desbloqueado | 1.20s | P0 | Progressão |
| `system_core_unstable_01.wav` | Núcleo instável | 1.50s | P1 | Setor 08 |
| `system_data_corrupt_01.wav` | Dados corrompidos | 0.80s | P1 | Logs |

---

# 19. SFX por fase importante

## 19.1 Fase 01 — Primeiro Turno

```text
step_concrete_01.wav
crate_push_wood_01.wav
crate_on_target_01.wav
terminal_boot_01.wav
puzzle_complete_01.wav
```

## 19.2 Fase 04 — Etiqueta Errada

```text
crate_push_wood_02.wav
terminal_glitch_01.wav
story_duda_log_start_01.wav
puzzle_secret_found_01.wav
```

## 19.3 Fase 10 — Licença de Operador Classe C

```text
forklift_engine_idle_loop.wav
forklift_accelerate_01.wav
forklift_reverse_beep_loop.wav
forklift_pickup_crate_01.wav
forklift_drop_crate_01.wav
forklift_collision_light_01.wav
medal_bronze_01.wav
```

## 19.4 Fase 13 — Carga Refrigerada

```text
amb_sector03_camara_fria_loop.ogg
forklift_cold_engine_loop.wav
forklift_ice_skid_01.wav
crate_fragile_rattle_01.wav
forklift_damage_alarm_01.wav
```

## 19.5 Fase 24 — Gerador Antigo

```text
amb_sector05_manutencao_loop.ogg
generator_start_01.wav
generator_fail_01.wav
door_manual_crank_01.wav
story_robert_serious_stinger_01.wav
power_up_01.wav
```

## 19.6 Fase 29 — Lockdown N-8

```text
system_lockdown_start_01.wav
system_lockdown_loop.ogg
door_emergency_shut_01.wav
forklift_engine_high_loop.wav
forklift_collision_medium_01.wav
system_warning_01.wav
```

## 19.7 Fase 30 — Núcleo Logístico

```text
amb_sector08_core_loop.ogg
amb_core_pulse_01.wav
terminal_data_recover_01.wav
story_duda_clue_reveal_01.wav
system_core_unstable_01.wav
puzzle_complete_01.wav
```

---

# 20. Mix por setor

## Setor 01 — Recebimento

```text
Ambiente baixo.
Caixas em destaque.
Poucos alertas.
Som mais limpo.
```

## Setor 02 — Expedição

```text
Mais ruído de doca.
Portas e sensores mais presentes.
Ritmo de trabalho.
```

## Setor 03 — Câmara Fria

```text
Reverb frio.
Passos mais secos.
Gelo com destaque.
Ambiente com freezer constante.
```

## Setor 04 — Automação

```text
Glitches ocasionais.
Esteiras em loop.
Sensores mais agudos.
Robôs com sons por turno.
```

## Setor 05 — Manutenção Pesada

```text
Baixos mais fortes.
Metal pesado.
Gerador presente.
Sons do Robert/Oficina.
```

## Setor 06 — Rotas Fantasma

```text
Ambiente mais vazio.
Ruídos distantes.
Glitches de mapa.
Sons narrativos em destaque.
```

## Setor 08 — Núcleo Logístico

```text
Core pulse grave.
Alertas controlados.
Energia instável.
Terminal e dados com forte identidade.
```

---

# 21. Integração técnica em Godot

## 21.1 Autoload recomendado

```text
AudioManager.gd
```

Responsabilidades:

```text
tocar SFX por nome;
controlar volume por categoria;
evitar repetição idêntica;
sortear variações;
gerenciar loops;
aplicar bus correto;
pausar/resumir sons;
fazer fade de ambiente.
```

---

## 21.2 Buses de áudio

```text
Master
Music
Ambience
SFX
UI
Vehicle
Puzzle
VoiceRadio
Alerts
```

---

## 21.3 Exemplo de chamada

```gdscript
AudioManager.play_sfx("crate_push_wood")
AudioManager.play_random("step_concrete")
AudioManager.play_loop("forklift_engine_idle_loop", "Vehicle")
AudioManager.stop_loop("forklift_engine_idle_loop")
AudioManager.play_stinger("puzzle_complete")
```

---

## 21.4 Sistema de variações

Exemplo de mapeamento:

```json
{
  "crate_push_wood": [
    "res://audio/sfx/crates/push/crate_push_wood_01.wav",
    "res://audio/sfx/crates/push/crate_push_wood_02.wav",
    "res://audio/sfx/crates/push/crate_push_wood_03.wav"
  ],
  "step_concrete": [
    "res://audio/sfx/player/footsteps/step_concrete_01.wav",
    "res://audio/sfx/player/footsteps/step_concrete_02.wav",
    "res://audio/sfx/player/footsteps/step_concrete_03.wav"
  ]
}
```

---

# 22. JSON sugerido para catálogo de áudio

```json
{
  "crate_push_wood": {
    "category": "crates",
    "bus": "SFX",
    "volume_db": -12,
    "pitch_random": 0.04,
    "files": [
      "res://audio/sfx/crates/push/crate_push_wood_01.wav",
      "res://audio/sfx/crates/push/crate_push_wood_02.wav",
      "res://audio/sfx/crates/push/crate_push_wood_03.wav"
    ]
  },
  "step_concrete": {
    "category": "player",
    "bus": "SFX",
    "volume_db": -18,
    "pitch_random": 0.06,
    "files": [
      "res://audio/sfx/player/footsteps/step_concrete_01.wav",
      "res://audio/sfx/player/footsteps/step_concrete_02.wav"
    ]
  },
  "forklift_engine_idle": {
    "category": "vehicle",
    "bus": "Vehicle",
    "volume_db": -14,
    "loop": true,
    "files": [
      "res://audio/sfx/forklift/engine/forklift_engine_idle_loop.wav"
    ]
  }
}
```

---

# 23. Backlog de implementação

## Sprint SFX-01 — Base de áudio

```text
TW08-SFX-001 Criar estrutura audio/sfx.
TW08-SFX-002 Criar buses de áudio no Godot.
TW08-SFX-003 Criar AudioManager.gd.
TW08-SFX-004 Criar catálogo audio_catalog.json.
TW08-SFX-005 Implementar play_sfx por chave.
TW08-SFX-006 Implementar play_random.
```

## Sprint SFX-02 — Puzzle básico

```text
TW08-SFX-010 Adicionar passos em concreto.
TW08-SFX-011 Adicionar empurrar caixa madeira.
TW08-SFX-012 Adicionar caixa batendo.
TW08-SFX-013 Adicionar caixa no alvo.
TW08-SFX-014 Adicionar movimento inválido.
TW08-SFX-015 Adicionar fase concluída.
```

## Sprint SFX-03 — Máquinas e sensores

```text
TW08-SFX-020 Adicionar sensor ativado/desativado.
TW08-SFX-021 Adicionar portas leves e pesadas.
TW08-SFX-022 Adicionar terminal boot/acesso.
TW08-SFX-023 Adicionar esteira loop.
TW08-SFX-024 Adicionar gerador start/fail.
```

## Sprint SFX-04 — Empilhadeira

```text
TW08-SFX-030 Adicionar motor idle/low/high.
TW08-SFX-031 Adicionar aceleração/freio/ré.
TW08-SFX-032 Adicionar pegar/soltar carga.
TW08-SFX-033 Adicionar colisões leve/média/forte.
TW08-SFX-034 Adicionar dano de carga.
```

## Sprint SFX-05 — UI e Oficina

```text
TW08-SFX-040 Adicionar UI select/confirm/back/error.
TW08-SFX-041 Adicionar loja abrir/fechar.
TW08-SFX-042 Adicionar compra e créditos.
TW08-SFX-043 Adicionar medalhas.
TW08-SFX-044 Adicionar ranking limpo/assistido.
```

## Sprint SFX-06 — Power Ups

```text
TW08-SFX-050 Adicionar Rebobinar Movimento.
TW08-SFX-051 Adicionar Scanner Logístico.
TW08-SFX-052 Adicionar Assistente de Turno.
TW08-SFX-053 Adicionar Marcador de Rota.
TW08-SFX-054 Adicionar Power Ups avançados.
```

## Sprint SFX-07 — Ambientes e narrativa

```text
TW08-SFX-060 Adicionar ambience por setor.
TW08-SFX-061 Adicionar sons de Duda/logs.
TW08-SFX-062 Adicionar sons de Robert/rádio.
TW08-SFX-063 Adicionar sons de Elias/registros.
TW08-SFX-064 Adicionar Setor 08.
```

## Sprint SFX-08 — Polimento

```text
TW08-SFX-070 Adicionar variações.
TW08-SFX-071 Ajustar volumes.
TW08-SFX-072 Aplicar pitch random.
TW08-SFX-073 Fazer teste de fadiga auditiva.
TW08-SFX-074 Validar mix com música.
```

---

# 24. Critérios de aceite

## 24.1 SFX básicos

```text
Passos tocam sem atrasar movimento.
Empurrar caixa toca uma vez por empurrão.
Caixa no alvo tem feedback claro.
Movimento inválido tem som curto e não irritante.
Fase concluída tem stinger satisfatório.
```

## 24.2 Empilhadeira

```text
Motor idle toca em loop.
Motor muda de intensidade conforme velocidade.
Bip de ré toca apenas em ré.
Colisão leve/média/forte tem sons diferentes.
Dano de carga é audível e visível.
Pegar e soltar carga são claros.
```

## 24.3 UI

```text
Botões têm som curto.
Erro de UI não é agressivo demais.
Compra na loja tem feedback satisfatório.
Medalhas têm identidade diferente.
Créditos recebidos são audíveis.
```

## 24.4 Ambientes

```text
Cada setor tem ambiente próprio.
Ambiente não cobre SFX importantes.
Loops não têm corte perceptível.
Setor 08 soa mais tenso que setores anteriores.
Câmara Fria soa diferente da Manutenção Pesada.
```

## 24.5 Narrativa

```text
Sons de Duda indicam dados/logs.
Sons de Robert indicam rádio/oficina.
Sons de Elias indicam mistério/registros apagados.
Glitches não atrapalham leitura de texto.
```

---

# 25. Prompt para Claude

```text
Você é o diretor de áudio e game designer sênior de The Warehouse Nº 08.

Objetivo:
Revisar e expandir o documento de Sound Design / SFX List do jogo.

Contexto:
The Warehouse Nº 08 é um puzzle game original de armazém, com pixel art, caixas, empilhadeiras, Oficina N-8, Power Ups, personagens John, Duda, Robert e Elias, além de mistério industrial envolvendo o Setor 08.

Regras:
- SFX devem ser curtos, legíveis e úteis ao gameplay.
- Não criar sons realistas demais que atrapalhem a clareza.
- Separar prioridades P0, P1, P2 e P3.
- Garantir que cada setor tenha identidade sonora.
- Garantir que empilhadeira tenha feedback completo.
- Garantir que Power Ups tenham identidade própria.
- Criar critérios de aceite para implementação em Godot.

Entregáveis:
1. Revisão da lista de SFX.
2. Lista de lacunas.
3. Sugestões de mix.
4. Sugestões de priorização.
5. Riscos de áudio.
6. Backlog para Codex.
```

---

# 26. Prompt para Codex

```text
Você é o implementador técnico de áudio do projeto The Warehouse Nº 08 em Godot 4.

Objetivo:
Implementar a base técnica de áudio para SFX, loops, UI, empilhadeira, puzzle feedback e ambientes.

Escopo inicial:
- Criar AudioManager.gd.
- Criar buses: Music, Ambience, SFX, UI, Vehicle, Puzzle, VoiceRadio, Alerts.
- Criar audio_catalog.json.
- Implementar play_sfx(key).
- Implementar play_random(key).
- Implementar play_loop(key).
- Implementar stop_loop(key).
- Integrar SFX básicos ao movimento, caixas, sensores e UI.

Regras:
- Não hardcodar caminhos de áudio dentro dos scripts de gameplay.
- Usar catálogo de áudio.
- Permitir pitch random pequeno.
- Não tocar o mesmo SFX idêntico repetidamente se houver variações.
- Garantir que loops possam parar corretamente.
- Não quebrar o core do puzzle.
- Informar arquivos criados/alterados.
- Informar como validar.
```

---

# 27. Riscos principais

## Risco 1 — Fadiga auditiva

[Provável] Passos e empurrões repetidos podem cansar.

Mitigação:

```text
usar 3 variações por som frequente;
pitch random leve;
volume moderado;
sons curtos.
```

## Risco 2 — Mix poluído

[Provável] Música, ambiente, empilhadeira e UI podem competir.

Mitigação:

```text
buses separados;
volumes por categoria;
ducking leve em alertas;
ambience baixo;
SFX de puzzle em destaque.
```

## Risco 3 — Loops mal cortados

[Certeza] Loops ruins quebram imersão.

Mitigação:

```text
testar loops isolados;
usar fades curtos;
preferir OGG para ambientes;
evitar transientes fortes no fim.
```

## Risco 4 — SFX de empilhadeira cansativo

[Provável] Motor em loop pode irritar.

Mitigação:

```text
loop suave;
variação por velocidade;
volume controlado;
filtro em baixa velocidade;
bip de ré não exagerado.
```

## Risco 5 — Sons informativos pouco claros

[Certeza] Se sensor, porta e caixa no alvo soarem parecidos, o jogador se confunde.

Mitigação:

```text
cada feedback deve ter identidade;
sensor = bip curto;
porta = mecânico pesado;
alvo = som positivo;
erro = som seco baixo.
```

---

# 28. Lista mínima para protótipo jogável

[Certeza] Para o primeiro protótipo jogável, criar apenas estes sons:

```text
step_concrete_01.wav
step_concrete_02.wav
crate_push_wood_01.wav
crate_push_wood_02.wav
crate_hit_wall_wood_01.wav
crate_on_target_01.wav
puzzle_invalid_move_01.wav
puzzle_complete_01.wav
door_open_light_01.wav
door_close_light_01.wav
sensor_activate_01.wav
sensor_deactivate_01.wav
terminal_boot_01.wav
ui_select_01.wav
ui_confirm_01.wav
ui_back_01.wav
ui_error_01.wav
forklift_engine_idle_loop.wav
forklift_accelerate_01.wav
forklift_reverse_beep_loop.wav
forklift_collision_light_01.wav
forklift_pickup_crate_01.wav
forklift_drop_crate_01.wav
```

Isso já permite testar:

```text
puzzle básico;
movimento;
feedback de caixa;
UI;
porta/sensor;
primeira fase de empilhadeira.
```

---

# 29. Conclusão

[Certeza] O áudio de **The Warehouse Nº 08** deve ser tratado como parte do design do puzzle, não como decoração.

Os sons precisam informar:

```text
o que aconteceu;
o que mudou;
o que deu certo;
o que deu errado;
o que está em risco;
o que pertence ao mistério.
```

A direção final recomendada é:

```text
SFX curtos, industriais, legíveis, com leve estética retrô.
Ambientes em loop para cada setor.
Empilhadeira com camadas de motor, freio, ré, carga e colisão.
Power Ups com identidade sonora própria.
Duda, Robert e Elias com assinaturas sonoras diferentes.
Setor 08 com áudio mais tenso, elétrico e corrompido.
```

Esse documento deve guiar a produção sonora, a organização de arquivos e a implementação técnica no Godot.

---

## 30. Status de validação

```text
Tipo de documento: Sound Design / SFX List.
Validação: não validado com sons reais ainda.
Uso recomendado: base para produção de SFX, protótipo de áudio e integração no Godot.
Próximo passo: gerar os SFX P0 em WAV e montar audio_catalog.json.
```
