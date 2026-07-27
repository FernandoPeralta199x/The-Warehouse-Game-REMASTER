# The Warehouse Nº 08 — SFX Catalog

**Total de sons:** 163

[Certeza] Pacote procedural para prototipo, vertical slice e integracao inicial no Godot.

## Especificacao

- WAV PCM 44.1 kHz, 16-bit.
- SFX mono; ambientes em stereo.
- Catalogo JSON e AudioManager.gd incluidos.
- Loops com crossfade.

## Observacao

Os sons sao procedurais. Antes do lancamento comercial, valide mix, loudness, fadiga auditiva e loops dentro da build real.

## Catalogo

### ambience

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `amb_sector01_receiving` | `audio/sfx/ambience/amb_sector01_receiving_loop.wav` | 12.000s | P1 | Setor 01 - Recebimento. |
| `amb_sector02_expedition` | `audio/sfx/ambience/amb_sector02_expedition_loop.wav` | 12.000s | P1 | Setor 02 - Expedicao. |
| `amb_sector03_cold` | `audio/sfx/ambience/amb_sector03_cold_loop.wav` | 12.000s | P1 | Setor 03 - Camara Fria. |
| `amb_sector04_automation` | `audio/sfx/ambience/amb_sector04_automation_loop.wav` | 12.000s | P1 | Setor 04 - Automacao. |
| `amb_sector05_maintenance` | `audio/sfx/ambience/amb_sector05_maintenance_loop.wav` | 12.000s | P1 | Setor 05 - Manutencao. |
| `amb_sector06_ghost_routes` | `audio/sfx/ambience/amb_sector06_ghost_routes_loop.wav` | 12.000s | P1 | Setor 06 - Rotas Fantasma. |
| `amb_sector08_core` | `audio/sfx/ambience/amb_sector08_core_loop.wav` | 12.000s | P1 | Setor 08 - Nucleo Logistico. |
| `amb_workshop_n8` | `audio/sfx/ambience/amb_workshop_n8_loop.wav` | 12.000s | P1 | Oficina N-8. |

### crates

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `crate_fragile_break` | `audio/sfx/crates/fragile/crate_fragile_break_01.wav` | 0.950s | P2 | Carga fragil quebrando. |
| `crate_fragile_damage` | `audio/sfx/crates/fragile/crate_fragile_damage_01.wav` | 0.300s | P2 | Dano leve em carga fragil. |
| `crate_fragile_rattle` | `audio/sfx/crates/fragile/crate_fragile_rattle_01.wav` | 0.620s | P2 | Conteudo fragil vibrando. |
| `crate_heavy_blocked` | `audio/sfx/crates/heavy/crate_heavy_blocked_01.wav` | 0.520s | P1 | Tentativa falha de empurrar caixa pesada. |
| `crate_heavy_push_01` | `audio/sfx/crates/heavy/crate_heavy_push_01.wav` | 0.480s | P1 | Arrasto de caixa pesada. |
| `crate_heavy_push_02` | `audio/sfx/crates/heavy/crate_heavy_push_02.wav` | 0.480s | P1 | Arrasto de caixa pesada. |
| `crate_heavy_stop` | `audio/sfx/crates/heavy/crate_heavy_stop_01.wav` | 0.350s | P1 | Caixa pesada parando. |
| `crate_hit_metal_01` | `audio/sfx/crates/metal/crate_hit_metal_01.wav` | 0.440s | P1 | Impacto de caixa metalica. |
| `crate_hit_metal_02` | `audio/sfx/crates/metal/crate_hit_metal_02.wav` | 0.440s | P1 | Impacto de caixa metalica. |
| `crate_hit_wall_wood_01` | `audio/sfx/crates/wood/crate_hit_wall_wood_01.wav` | 0.320s | P0 | Caixa de madeira colidindo com parede. |
| `crate_hit_wall_wood_02` | `audio/sfx/crates/wood/crate_hit_wall_wood_02.wav` | 0.320s | P0 | Caixa de madeira colidindo com parede. |
| `crate_ice_stop` | `audio/sfx/crates/ice/crate_ice_stop_01.wav` | 0.360s | P1 | Caixa parando no gelo. |
| `crate_off_target` | `audio/sfx/crates/wood/crate_off_target_01.wav` | 0.330s | P1 | Caixa saindo do alvo. |
| `crate_on_target` | `audio/sfx/crates/wood/crate_on_target_01.wav` | 0.660s | P0 | Caixa encaixando no alvo. |
| `crate_push_metal_01` | `audio/sfx/crates/metal/crate_push_metal_01.wav` | 0.380s | P1 | Caixa metalica arrastando. |
| `crate_push_metal_02` | `audio/sfx/crates/metal/crate_push_metal_02.wav` | 0.380s | P1 | Caixa metalica arrastando. |
| `crate_push_wood_01` | `audio/sfx/crates/wood/crate_push_wood_01.wav` | 0.355s | P0 | Caixa de madeira sendo empurrada. |
| `crate_push_wood_02` | `audio/sfx/crates/wood/crate_push_wood_02.wav` | 0.370s | P0 | Caixa de madeira sendo empurrada. |
| `crate_push_wood_03` | `audio/sfx/crates/wood/crate_push_wood_03.wav` | 0.385s | P0 | Caixa de madeira sendo empurrada. |
| `crate_slide_ice_01` | `audio/sfx/crates/ice/crate_slide_ice_01.wav` | 0.760s | P1 | Caixa deslizando no gelo. |
| `crate_slide_ice_02` | `audio/sfx/crates/ice/crate_slide_ice_02.wav` | 0.800s | P1 | Caixa deslizando no gelo. |
| `crate_stop_wood` | `audio/sfx/crates/wood/crate_stop_wood_01.wav` | 0.180s | P0 | Caixa de madeira parando. |

### forklift

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `forklift_accelerate` | `audio/sfx/forklift/movement/forklift_accelerate_01.wav` | 0.900s | P0 | Empilhadeira acelerando. |
| `forklift_brake` | `audio/sfx/forklift/movement/forklift_brake_01.wav` | 0.380s | P0 | Freio normal. |
| `forklift_cargo_fall` | `audio/sfx/forklift/cargo/forklift_cargo_fall_01.wav` | 0.890s | P2 | Carga caindo. |
| `forklift_cargo_lock` | `audio/sfx/forklift/cargo/forklift_cargo_lock_01.wav` | 0.400s | P1 | Carga travada. |
| `forklift_cargo_unstable` | `audio/sfx/forklift/cargo/forklift_cargo_unstable_01.wav` | 0.790s | P1 | Carga instavel. |
| `forklift_collision_heavy` | `audio/sfx/forklift/collision/forklift_collision_heavy_01.wav` | 0.650s | P1 | Forklift Collision Heavy. |
| `forklift_collision_light` | `audio/sfx/forklift/collision/forklift_collision_light_01.wav` | 0.480s | P0 | Forklift Collision Light. |
| `forklift_collision_medium` | `audio/sfx/forklift/collision/forklift_collision_medium_01.wav` | 0.480s | P0 | Forklift Collision Medium. |
| `forklift_damage_alarm` | `audio/sfx/forklift/alerts/forklift_damage_alarm_01.wav` | 0.680s | P1 | Alerta de dano na carga. |
| `forklift_decelerate` | `audio/sfx/forklift/movement/forklift_decelerate_01.wav` | 0.750s | P1 | Motor reduzindo. |
| `forklift_drop_crate` | `audio/sfx/forklift/cargo/forklift_drop_crate_01.wav` | 0.630s | P0 | Carga solta no piso. |
| `forklift_electric_engine` | `audio/sfx/forklift/engine/forklift_electric_engine_loop.wav` | 4.000s | P2 | Motor eletrico da N-8 Electric. |
| `forklift_engine_high` | `audio/sfx/forklift/engine/forklift_engine_high_loop.wav` | 4.000s | P1 | Motor padrao em alta velocidade. |
| `forklift_engine_idle` | `audio/sfx/forklift/engine/forklift_engine_idle_loop.wav` | 4.000s | P0 | Motor padrao em marcha lenta. |
| `forklift_engine_low` | `audio/sfx/forklift/engine/forklift_engine_low_loop.wav` | 4.000s | P0 | Motor padrao em baixa velocidade. |
| `forklift_fork_lower` | `audio/sfx/forklift/cargo/forklift_fork_lower_01.wav` | 0.820s | P1 | Garfo descendo. |
| `forklift_fork_raise` | `audio/sfx/forklift/cargo/forklift_fork_raise_01.wav` | 0.820s | P1 | Garfo subindo. |
| `forklift_handbrake` | `audio/sfx/forklift/movement/forklift_handbrake_01.wav` | 0.380s | P1 | Freio brusco. |
| `forklift_heavy_engine` | `audio/sfx/forklift/engine/forklift_heavy_engine_loop.wav` | 4.000s | P1 | Motor grave da N-8 Heavy. |
| `forklift_hit_cone` | `audio/sfx/forklift/collision/forklift_hit_cone_01.wav` | 0.560s | P1 | Cone atingido. |
| `forklift_hit_pallet` | `audio/sfx/forklift/collision/forklift_hit_pallet_01.wav` | 0.360s | P1 | Palete atingido. |
| `forklift_ice_skid` | `audio/sfx/forklift/movement/forklift_ice_skid_01.wav` | 0.780s | P2 | Derrapagem no gelo. |
| `forklift_pickup_crate` | `audio/sfx/forklift/cargo/forklift_pickup_crate_01.wav` | 0.660s | P0 | Carga acoplando ao garfo. |
| `forklift_reverse_beep` | `audio/sfx/forklift/alerts/forklift_reverse_beep_loop.wav` | 1.000s | P0 | Bip de re. |
| `forklift_tire_screech` | `audio/sfx/forklift/movement/forklift_tire_screech_01.wav` | 0.550s | P1 | Pneu arrastando. |

### machines

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `button_press` | `audio/sfx/machines/sensors/button_press_01.wav` | 0.120s | P0 | Botao pressionado. |
| `conveyor_belt` | `audio/sfx/machines/conveyors/conveyor_belt_loop.wav` | 4.000s | P1 | Esteira em operacao. |
| `conveyor_jam` | `audio/sfx/machines/conveyors/conveyor_jam_01.wav` | 0.760s | P2 | Esteira travada. |
| `conveyor_reverse` | `audio/sfx/machines/conveyors/conveyor_reverse_01.wav` | 0.610s | P2 | Esteira invertendo. |
| `conveyor_start` | `audio/sfx/machines/conveyors/conveyor_start_01.wav` | 0.950s | P1 | Esteira ligando. |
| `conveyor_stop` | `audio/sfx/machines/conveyors/conveyor_stop_01.wav` | 0.800s | P1 | Esteira parando. |
| `door_close_heavy` | `audio/sfx/machines/doors/door_close_heavy_01.wav` | 1.970s | P0 | Portao fechando. |
| `door_close_light` | `audio/sfx/machines/doors/door_close_light_01.wav` | 1.120s | P0 | Porta fechando. |
| `door_emergency_shut` | `audio/sfx/machines/doors/door_emergency_shut_01.wav` | 1.200s | P1 | Fechamento de emergencia. |
| `door_locked` | `audio/sfx/machines/doors/door_locked_01.wav` | 0.420s | P0 | Porta travada. |
| `door_manual_crank` | `audio/sfx/machines/doors/door_manual_crank_01.wav` | 1.120s | P2 | Manivela manual. |
| `door_open_heavy` | `audio/sfx/machines/doors/door_open_heavy_01.wav` | 1.970s | P0 | Portao abrindo. |
| `door_open_light` | `audio/sfx/machines/doors/door_open_light_01.wav` | 1.120s | P0 | Porta abrindo. |
| `door_unlock` | `audio/sfx/machines/doors/door_unlock_01.wav` | 0.480s | P0 | Trava liberada. |
| `electric_spark_01` | `audio/sfx/machines/generators/electric_spark_01.wav` | 0.180s | P2 | Faisca eletrica. |
| `electric_spark_02` | `audio/sfx/machines/generators/electric_spark_02.wav` | 0.180s | P2 | Faisca eletrica. |
| `generator_fail` | `audio/sfx/machines/generators/generator_fail_01.wav` | 1.460s | P1 | Gerador falhando. |
| `generator_idle` | `audio/sfx/machines/generators/generator_idle_loop.wav` | 6.000s | P1 | Gerador antigo em loop. |
| `generator_start` | `audio/sfx/machines/generators/generator_start_01.wav` | 2.750s | P1 | Gerador ligando. |
| `lever_pull` | `audio/sfx/machines/sensors/lever_pull_01.wav` | 0.520s | P1 | Alavanca acionada. |
| `power_down` | `audio/sfx/machines/generators/power_down_01.wav` | 1.000s | P1 | Energia caindo. |
| `power_up` | `audio/sfx/machines/generators/power_up_01.wav` | 1.200s | P1 | Energia restaurada. |
| `robot_error` | `audio/sfx/machines/robots/robot_error_01.wav` | 0.670s | P2 | Robo em erro. |
| `robot_move` | `audio/sfx/machines/robots/robot_move_loop.wav` | 2.000s | P2 | Robo em movimento. |
| `robot_scan` | `audio/sfx/machines/robots/robot_scan_01.wav` | 0.480s | P2 | Scanner de robo. |
| `robot_shutdown` | `audio/sfx/machines/robots/robot_shutdown_01.wav` | 0.660s | P2 | Robo desligado. |
| `robot_turn` | `audio/sfx/machines/robots/robot_turn_01.wav` | 0.260s | P2 | Robo mudando direcao. |
| `sensor_activate` | `audio/sfx/machines/sensors/sensor_activate_01.wav` | 0.280s | P0 | Sensor ativado. |
| `sensor_deactivate` | `audio/sfx/machines/sensors/sensor_deactivate_01.wav` | 0.260s | P0 | Sensor desativado. |
| `sensor_false` | `audio/sfx/machines/sensors/sensor_false_01.wav` | 0.630s | P1 | Sensor falso. |
| `switch_power` | `audio/sfx/machines/sensors/switch_power_01.wav` | 0.310s | P1 | Chave de energia. |
| `terminal_access_denied` | `audio/sfx/machines/terminals/terminal_access_denied_01.wav` | 0.450s | P0 | Acesso negado. |
| `terminal_access_granted` | `audio/sfx/machines/terminals/terminal_access_granted_01.wav` | 0.280s | P0 | Acesso liberado. |
| `terminal_beep` | `audio/sfx/machines/terminals/terminal_beep_01.wav` | 0.120s | P0 | Bip de terminal. |
| `terminal_boot` | `audio/sfx/machines/terminals/terminal_boot_01.wav` | 0.870s | P0 | Terminal iniciando. |
| `terminal_data_deleted` | `audio/sfx/machines/terminals/terminal_data_deleted_01.wav` | 0.690s | P2 | Registro apagado. |
| `terminal_data_recover` | `audio/sfx/machines/terminals/terminal_data_recover_01.wav` | 0.760s | P1 | Registro recuperado. |
| `terminal_glitch` | `audio/sfx/machines/terminals/terminal_glitch_01.wav` | 0.650s | P1 | Glitch de terminal. |

### player

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `player_blocked` | `audio/sfx/player/movement/player_blocked_01.wav` | 0.200s | P0 | Movimento bloqueado. |
| `player_push_effort_01` | `audio/sfx/player/movement/player_push_effort_01.wav` | 0.240s | P1 | Esforco curto ao mover carga pesada. |
| `player_push_effort_02` | `audio/sfx/player/movement/player_push_effort_02.wav` | 0.240s | P1 | Esforco curto ao mover carga pesada. |
| `step_concrete_01` | `audio/sfx/player/footsteps/step_concrete_01.wav` | 0.150s | P0 | Passo sobre concrete. |
| `step_concrete_02` | `audio/sfx/player/footsteps/step_concrete_02.wav` | 0.150s | P0 | Passo sobre concrete. |
| `step_concrete_03` | `audio/sfx/player/footsteps/step_concrete_03.wav` | 0.150s | P0 | Passo sobre concrete. |
| `step_ice_01` | `audio/sfx/player/footsteps/step_ice_01.wav` | 0.180s | P1 | Passo sobre ice. |
| `step_ice_02` | `audio/sfx/player/footsteps/step_ice_02.wav` | 0.180s | P1 | Passo sobre ice. |
| `step_metal_01` | `audio/sfx/player/footsteps/step_metal_01.wav` | 0.150s | P1 | Passo sobre metal. |
| `step_metal_02` | `audio/sfx/player/footsteps/step_metal_02.wav` | 0.150s | P1 | Passo sobre metal. |
| `step_metal_03` | `audio/sfx/player/footsteps/step_metal_03.wav` | 0.150s | P1 | Passo sobre metal. |
| `step_water_01` | `audio/sfx/player/footsteps/step_water_01.wav` | 0.150s | P2 | Passo sobre water. |
| `step_water_02` | `audio/sfx/player/footsteps/step_water_02.wav` | 0.150s | P2 | Passo sobre water. |

### powerups

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `powerup_battery_reserve` | `audio/sfx/powerups/powerup_battery_reserve_01.wav` | 0.820s | P2 | Bateria Reserva. |
| `powerup_emergency_brake` | `audio/sfx/powerups/powerup_emergency_brake_01.wav` | 0.520s | P2 | Freio de Emergencia. |
| `powerup_ghost_sim` | `audio/sfx/powerups/powerup_ghost_sim_start_01.wav` | 0.800s | P3 | Simulacao Fantasma. |
| `powerup_hint_open` | `audio/sfx/powerups/powerup_hint_open_01.wav` | 0.380s | P0 | Assistente de Turno. |
| `powerup_hydraulic_force` | `audio/sfx/powerups/powerup_hydraulic_force_01.wav` | 0.990s | P2 | Forca Hidraulica. |
| `powerup_jack_pull` | `audio/sfx/powerups/powerup_jack_pull_01.wav` | 1.150s | P2 | Macaco N-8. |
| `powerup_rewind_activate` | `audio/sfx/powerups/powerup_rewind_activate_01.wav` | 0.900s | P0 | Rebobinar Movimento. |
| `powerup_route_marker` | `audio/sfx/powerups/powerup_route_marker_01.wav` | 0.550s | P0 | Marcador de Rota. |
| `powerup_scanner_activate` | `audio/sfx/powerups/powerup_scanner_activate_01.wav` | 0.600s | P0 | Scanner Logistico. |
| `powerup_scanner_ping` | `audio/sfx/powerups/powerup_scanner_ping_01.wav` | 0.220s | P0 | Ping de caixa critica. |

### puzzle_feedback

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `puzzle_complete` | `audio/sfx/puzzle_feedback/puzzle_complete_01.wav` | 2.200s | P0 | Fase concluida. |
| `puzzle_deadlock_warning` | `audio/sfx/puzzle_feedback/puzzle_deadlock_warning_01.wav` | 0.440s | P2 | Aviso de deadlock. |
| `puzzle_invalid_move` | `audio/sfx/puzzle_feedback/puzzle_invalid_move_01.wav` | 0.250s | P0 | Movimento invalido. |
| `puzzle_map_reveal` | `audio/sfx/puzzle_feedback/puzzle_map_reveal_01.wav` | 1.000s | P1 | Mapa revelado. |
| `puzzle_secret_found` | `audio/sfx/puzzle_feedback/puzzle_secret_found_01.wav` | 1.100s | P1 | Segredo encontrado. |
| `puzzle_success` | `audio/sfx/puzzle_feedback/puzzle_success_01.wav` | 0.240s | P0 | Objetivo parcial. |
| `puzzle_target_order_correct` | `audio/sfx/puzzle_feedback/puzzle_target_order_correct_01.wav` | 0.360s | P2 | Ordem correta. |
| `puzzle_target_order_wrong` | `audio/sfx/puzzle_feedback/puzzle_target_order_wrong_01.wav` | 0.320s | P2 | Ordem errada. |
| `puzzle_unlock_path` | `audio/sfx/puzzle_feedback/puzzle_unlock_path_01.wav` | 0.730s | P0 | Rota liberada. |

### secret

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `secret_duda_path` | `audio/sfx/story/secret/secret_duda_path_01.wav` | 2.450s | P2 | Caminho da Duda. |
| `secret_elias_truth` | `audio/sfx/story/secret/secret_elias_truth_01.wav` | 2.470s | P2 | Verdade de Elias. |
| `secret_room_enter` | `audio/sfx/story/secret/secret_room_enter_01.wav` | 1.030s | P1 | Entrada em sala secreta. |
| `secret_unlock` | `audio/sfx/story/secret/secret_unlock_01.wav` | 1.300s | P1 | Fase secreta desbloqueada. |

### story

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `story_duda_clue_reveal` | `audio/sfx/story/characters/story_duda_clue_reveal_01.wav` | 1.120s | P1 | Pista da Duda. |
| `story_duda_log_start` | `audio/sfx/story/characters/story_duda_log_start_01.wav` | 0.740s | P1 | Inicio de log da Duda. |
| `story_duda_stinger` | `audio/sfx/story/characters/story_duda_stinger_01.wav` | 1.300s | P1 | Assinatura de Duda. |
| `story_elias_record_found` | `audio/sfx/story/characters/story_elias_record_found_01.wav` | 1.120s | P1 | Registro de Elias. |
| `story_elias_stinger` | `audio/sfx/story/characters/story_elias_stinger_01.wav` | 1.370s | P2 | Assinatura de Elias. |
| `story_john_stinger` | `audio/sfx/story/characters/story_john_stinger_01.wav` | 1.170s | P2 | Assinatura de John. |
| `story_radio_off` | `audio/sfx/story/radio/story_radio_off_01.wav` | 0.520s | P1 | Radio desligado. |
| `story_radio_on` | `audio/sfx/story/radio/story_radio_on_01.wav` | 0.540s | P1 | Radio ligado. |
| `story_robert_serious` | `audio/sfx/story/characters/story_robert_serious_stinger_01.wav` | 1.180s | P2 | Robert fica serio. |
| `story_robert_stinger` | `audio/sfx/story/characters/story_robert_stinger_01.wav` | 0.990s | P2 | Assinatura de Robert. |

### system

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `system_core_unstable` | `audio/sfx/system/system_core_unstable_01.wav` | 1.380s | P1 | Nucleo instavel. |
| `system_data_corrupt` | `audio/sfx/system/system_data_corrupt_01.wav` | 0.890s | P1 | Dados corrompidos. |
| `system_lockdown_alarm` | `audio/sfx/system/system_lockdown_alarm_loop.wav` | 1.000s | P1 | Alarme de lockdown. |
| `system_lockdown_start` | `audio/sfx/system/system_lockdown_start_01.wav` | 1.130s | P1 | Lockdown iniciado. |
| `system_sector_unlocked` | `audio/sfx/system/system_sector_unlocked_01.wav` | 1.150s | P0 | Setor desbloqueado. |

### ui

| Chave | Arquivo | Duracao | Prioridade | Descricao |
|---|---|---:|---|---|
| `assisted_run_badge` | `audio/sfx/ui/results/assisted_run_badge_01.wav` | 0.260s | P1 | Tentativa assistida. |
| `clean_run_badge` | `audio/sfx/ui/results/clean_run_badge_01.wav` | 0.260s | P1 | Tentativa limpa. |
| `credits_gain` | `audio/sfx/ui/shop/credits_gain_01.wav` | 0.460s | P0 | Creditos recebidos. |
| `medal_bronze` | `audio/sfx/ui/results/medal_bronze_01.wav` | 0.750s | P0 | Medalha Bronze. |
| `medal_gold` | `audio/sfx/ui/results/medal_gold_01.wav` | 1.050s | P0 | Medalha Ouro. |
| `medal_platinum` | `audio/sfx/ui/results/medal_platinum_01.wav` | 1.350s | P1 | Medalha Platina. |
| `medal_silver` | `audio/sfx/ui/results/medal_silver_01.wav` | 0.850s | P0 | Medalha Prata. |
| `result_screen_open` | `audio/sfx/ui/results/result_screen_open_01.wav` | 0.600s | P0 | Tela de resultado. |
| `shop_buy` | `audio/sfx/ui/shop/shop_buy_01.wav` | 0.400s | P1 | Compra concluida. |
| `shop_close` | `audio/sfx/ui/shop/shop_close_01.wav` | 0.460s | P1 | Oficina fechada. |
| `shop_no_credits` | `audio/sfx/ui/shop/shop_no_credits_01.wav` | 0.360s | P1 | Creditos insuficientes. |
| `shop_open` | `audio/sfx/ui/shop/shop_open_01.wav` | 0.600s | P1 | Oficina aberta. |
| `ui_back` | `audio/sfx/ui/menu/ui_back_01.wav` | 0.110s | P0 | Voltar. |
| `ui_confirm` | `audio/sfx/ui/menu/ui_confirm_01.wav` | 0.110s | P0 | Confirmacao. |
| `ui_error` | `audio/sfx/ui/menu/ui_error_01.wav` | 0.200s | P0 | Erro de interface. |
| `ui_pause` | `audio/sfx/ui/menu/ui_pause_01.wav` | 0.180s | P1 | Pausa. |
| `ui_select` | `audio/sfx/ui/menu/ui_select_01.wav` | 0.070s | P0 | Navegacao. |
| `ui_tab_change` | `audio/sfx/ui/menu/ui_tab_change_01.wav` | 0.090s | P1 | Troca de aba. |
| `ui_unpause` | `audio/sfx/ui/menu/ui_unpause_01.wav` | 0.180s | P1 | Retomar. |
