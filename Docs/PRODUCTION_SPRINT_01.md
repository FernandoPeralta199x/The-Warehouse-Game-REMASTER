# The Warehouse Nº 08 — Production Sprint 01

Data de início: 2026-08-11  
Engine: Unity 6.3 LTS  
Linguagem: C#  
Branch: `feat/tw08-vertical-slice-foundation`

## Objetivo

Transformar o vertical slice técnico em um primeiro loop de produção que preserve regras determinísticas de puzzle e permita substituir placeholders por pixel-art sem reescrever gameplay.

## Princípios de engenharia

- O estado do puzzle é instantâneo e baseado em grade.
- Movimento visual, tween e animação são apresentação e não podem alterar a regra.
- Arte em `ReferenceSource` é referência; somente `Art/Production` entra como arte promovida para gameplay.
- Assets de personagem são configurados por `DirectionalSpriteSet`.
- O jogo deve continuar funcional quando o catálogo de arte estiver incompleto, usando fallback provisório.
- Toda fase precisa validar antes de entrar em runtime.
- Toda regressão encontrada pelo Unity deve ganhar correção ou teste quando aplicável.

## Escopo do Sprint 01

### Core puzzle

- Movimento cardinal em grade.
- Push de uma caixa.
- Undo / redo / restart.
- Detecção de conclusão.
- Detecção simples de deadlock estático.
- Três fases originais solucionáveis.

### Apresentação

- Interpolação curta entre células sem física no domínio.
- Direção visual Down / Up / Left / Right.
- `DirectionalSpriteSet` para John.
- Animação de passo ligada aos eventos do `PuzzleRuntime`.
- Catálogo central de arte de produção.

### Pipeline de arte

- `Assets/_Project/Art/ReferenceSource`: material de referência importado.
- `Assets/_Project/Art/Production/Characters/John`: sprites finais/limpos do John.
- `Assets/_Project/Art/Production/Environment`: pisos e paredes.
- `Assets/_Project/Art/Production/Props`: caixas, pallets e props.
- `Assets/_Project/Art/Production/Interactive`: sensores, portas e terminais.
- `Assets/_Project/Art/Production/UI`: HUD e elementos de interface.
- `Assets/_Project/Art/Production/VFX`: efeitos de movimento, sucesso e alerta.

## Direção visual derivada das referências

- Industrial retro-futurista N-8.
- Preto/grafite como base.
- Âmbar/amarelo para operador, carga, aviso e identidade industrial.
- Verde terminal para sucesso, alvo e estado operacional.
- Azul/ciano reservado para tecnologia, frio e sistemas especiais.
- Personagem com footprint lógico 1x1; sprite pode exceder a célula visualmente.
- Pivô de personagem nos pés.
- Movimento legível acima de suavidade excessiva.

## John — contrato inicial de animação

Quatro direções:

- Down / Front
- Up / Back
- Left
- Right

Estados mínimos:

- Idle
- Walk 1
- Walk 2
- Walk 3

Playback recomendado inicial: 8 FPS. O domínio continua movendo uma célula por comando; a animação apenas apresenta o passo.

## Gate de conclusão

Sprint 01 só é considerado concluído quando:

1. Projeto abre em Unity 6.3 LTS sem erros de compilação.
2. EditMode tests passam.
3. PlayMode tests passam.
4. As três fases carregam sem Missing Script.
5. John move com apresentação interpolada sem alterar o resultado lógico do puzzle.
6. Undo e redo mantêm posição lógica e apresentação sincronizadas.
7. `DirectionalSpriteSet` troca direção de John corretamente quando sprites de produção são atribuídos.
8. Fase 01 possui primeira passagem de arte de produção para John, piso, parede, caixa e alvo.
9. Menu → Fase 01 → Fase 02 → Fase 03 → Menu funciona.
10. Development Build Windows é gerada sem erro bloqueante.

## Fora do Sprint 01

- Corridas de empilhadeira completas.
- Power-ups de campanha.
- Câmara fria e gelo.
- Esteiras/robôs avançados.
- Loja.
- Narrativa completa de Duda/Robert/Elias.
- Backend online, telemetria ou conta de usuário.

Esses sistemas ficam bloqueados até o core puzzle e a apresentação da primeira fase atingirem o gate acima.
