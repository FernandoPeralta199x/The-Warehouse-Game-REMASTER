# The Warehouse Nº 08 — Production Mega Update

Data: 2026-08-11
Branch: `feat/tw08-production-mega-update`
Base: `feat/tw08-vertical-slice-foundation`

## 1. Objetivo

Transformar o vertical slice em uma base de produção mais próxima de um jogo comercial sem destruir o core puzzle-first.

Esta tranche expande quatro pilares:

1. apresentação gráfica e câmera;
2. N-8 Logistics Rush como corrida arcade logística original;
3. menus/UI com comportamento de produto;
4. pipeline para conteúdo grande e opcional.

A meta não é aumentar o tamanho do repositório artificialmente. Tamanho em disco só é aceitável quando corresponde a conteúdo real: arte, áudio, vídeo, mapas, localização ou pacotes opcionais.

## 2. Regra de originalidade

O modo corrida pode estudar convenções de kart-racing arcade — grid, rivais, posição, itens, catch-up moderado, leitura clara da pista e feedback forte — mas não pode copiar identidade, personagens, nomes, layouts, assets, sons, UI ou conteúdo proprietário de Mario Kart/Nintendo ou de qualquer outro jogo.

A identidade TW08 continua sendo logística industrial:

`tempo + precisão + carga + rota + dano + medalhas`

Isso segue `REFERENCIA/Forklift_Shift_Races.md`.

## 3. Pesquisa técnica aplicada

### Addressables

Unity 6 documenta Addressables como sistema de assets por endereço, com carregamento assíncrono e suporte a conteúdo local ou remoto. O projeto fixa `com.unity.addressables` em 2.7.6, versão documentada como released para Unity 6000.0.

Uso TW08:

- `TW08-Optional-Art`
- `TW08-Optional-Audio`
- `TW08-Race-Packs`
- `TW08-Narrative-Packs`

Os grupos começam com paths locais. CDN/remote build path só deve ser configurado quando existir infraestrutura de distribuição real.

### Scene loading

A documentação do Unity recomenda `SceneManager.LoadSceneAsync` na maioria dos casos para evitar pausas e stutter. O `SceneLoader` central foi atualizado para carregar cenas de runtime de forma assíncrona, mantendo o fallback de Editor usado para diagnóstico.

### Camera

Cinemachine 3.1.5 está documentado para Unity 6000.0, mas não foi introduzido nesta tranche. O projeto já precisava de uma câmera 2D pixel-aware específica e de baixo risco. Foi criada uma camada própria pequena e substituível. Cinemachine continua opção futura para cutscenes/track cameras, não uma dependência obrigatória do core.

### Render pipeline

URP é o pipeline prebuilt da Unity para workflows otimizados e possui caminho 2D/iluminação 2D. A migração para URP não foi feita nesta tranche porque trocar render pipeline durante estabilização de cenas amplia muito o raio de regressão. `TW08GraphicsProfile` e `TW08GraphicsDirector` isolam decisões de qualidade/câmera para permitir uma migração posterior.

### Pooling

Unity 6 oferece `UnityEngine.Pool.ObjectPool<T>`. Itens, partículas e hazards que passarem a criar/destruir objetos com alta frequência devem migrar para pooling quando profiling mostrar pressão de GC/CPU. Não foi introduzido pooling prematuro em objetos que ainda são poucos e estáticos.

## 4. TW08 Graphics Layer

Arquivos principais:

- `Presentation/Rendering/TW08GraphicsProfile.cs`
- `Presentation/Rendering/TW08GraphicsDirector.cs`
- `Presentation/Rendering/TW08CameraRig2D.cs`
- `Presentation/Rendering/RaceImpactFeedback.cs`

Responsabilidades:

- frame pacing;
- vSync/AA policy;
- pixel snapping;
- camera smoothing;
- velocity look-ahead;
- speed zoom-out;
- camera shake centralizado.

Não é uma nova game engine. É a camada de apresentação TW08 sobre Unity, com responsabilidade estreita e substituível.

## 5. Logistics Rush — nova direção

### Pelotão

Cada pista passa a suportar:

- jogador;
- três rivais IA;
- posição `01/04`;
- ranking por volta/checkpoint/distância ao próximo gate.

A IA usa os mesmos componentes físicos do jogador, não teleporta e não usa um modelo de movimento paralelo.

### Catch-up

O catch-up é leve e transparente:

- líder recebe pequena redução de agressividade/ritmo;
- último colocado recebe pequeno auxílio de ritmo;
- IA continua obedecendo física, checkpoints e superfície.

O objetivo é manter disputa, não falsificar resultado.

### Drift

IA escolhe drift a partir de:

- ângulo para o próximo checkpoint;
- velocidade normalizada;
- agressividade do piloto.

Jogador mantém drift charge + boost na saída.

### Carga

`RaceCargoController` transforma o modo em logística competitiva:

- impacto danifica carga;
- velocidade lateral excessiva causa stress;
- drift agressivo aumenta risco;
- integridade chega ao HUD;
- dano da carga participa do cálculo de medalha.

### Itens

O grid inicial usa 8 ferramentas logísticas:

- Nitro Hidráulico;
- Barreira de Segurança;
- Estabilizador de Carga;
- Freio ABS N-8;
- Scanner de Rota;
- Buzina Industrial;
- Kit de Reparo;
- Suspensão Reforçada.

O sistema reutiliza `PowerUpDefinition`, `WeightedPowerUpTable`, `PowerUpInventory` e `PowerUpExecutor` já existentes.

Item distribution é ponderada por posição, permitindo oferecer ferramentas de recuperação com maior frequência a quem está atrás sem tornar o líder impotente.

## 6. Menus profissionais

Componentes:

- `ProfessionalMenuPresenter`
- `MenuFocusAnimator`

Comportamento:

- entrada curta por fade/slide/scale;
- interação bloqueada durante a introdução;
- foco inequívoco para teclado/gamepad;
- microescala do item selecionado;
- animação em unscaled time.

Princípio: movimento serve hierarquia e legibilidade, não decoração gratuita.

## 7. Conteúdo grande: política de GB

### O que NÃO fazer

Nunca adicionar arquivos aleatórios, zeros, duplicatas ou mídia sem uso para atingir uma meta de tamanho. Isso piora:

- clone/fetch;
- CI;
- cache;
- backup;
- build incremental;
- revisão de código;
- custo de armazenamento.

### O que fazer

Conteúdo real deve crescer por packs:

- arte final/alta resolução;
- stems e masters de áudio;
- vídeo/cinematics;
- novos setores e pistas;
- voice-over/localização;
- content packs opcionais.

Binários de produção usam Git LFS via `.gitattributes`.

Addressables separa conteúdo opcional do core. Quando houver CDN real, os grupos opcionais poderão usar remote build/load path sem mudar a lógica do jogo.

### Meta de orçamento

Orçamento é medido por conteúdo útil, não por tamanho alvo arbitrário.

Sugestão futura para uma versão rica:

- core Windows: 1–3 GB;
- optional art pack: 1–4 GB;
- audio/OST/VO: 0.5–3 GB;
- race/sector packs: 0.25–2 GB por pack;
- cinematics: conforme bitrate/duração.

Esses números são planejamento, não requisito de qualidade.

## 8. Pipeline recomendado de arte

`ReferenceSource` continua sendo direção/conceito.

Arte que chega ao runtime deve ser curada em `Art/Production` ou em um Content Pack. Boards de referência 1536x1024 não viram spritesheets automaticamente.

Quando assets finais estiverem prontos:

1. limpar/silhuetar frames;
2. padronizar pivô e PPU;
3. importar Point/sem compressão quando pixel art exigir;
4. criar sprite sets/atlases;
5. migrar refs do starter para production;
6. profile de memória/draw calls;
7. somente depois apagar starters sem referência.

## 9. Gates obrigatórios

Antes de mergear esta tranche:

1. Package Manager resolve Addressables;
2. zero erros de compilação;
3. `Repair Runtime Scene Registration` confirma 19/19;
4. `Build Mega Production Update` completa;
5. EditMode completo;
6. PlayMode completo;
7. Player tests quando aplicável;
8. playtest das três pistas;
9. validar IA 4/4;
10. validar item pickup/use;
11. validar carga/dano/medalha;
12. validar menu com teclado + gamepad;
13. validar save/reload;
14. Windows Development Build;
15. profiler sem spikes graves antes de polish final.

## 10. Próximas tranches

Somente depois do gate:

- novas geometrias de pista e atalhos reais;
- ghost/time trial;
- classes de empilhadeira Standard/Heavy/Light;
- ranking local expandido;
- tela de resultados com splits/dano/medalhas;
- pooling de VFX/hazards quando profiling justificar;
- URP 2D renderer + 2D lights em branch isolada;
- Cinemachine para cinematics/câmeras especiais se trouxer ganho real;
- Addressables remote profile quando existir storage/CDN;
- ingestão dos assets finais da art bible.

## 11. Fontes públicas consultadas

- Unity Manual — Addressables, Unity 6: `https://docs.unity3d.com/6000.0/Manual/com.unity.addressables.html`
- Unity Scripting API — `SceneManager.LoadSceneAsync`: `https://docs.unity3d.com/6000.0/ScriptReference/SceneManagement.SceneManager.LoadSceneAsync.html`
- Unity Manual — Cinemachine, Unity 6: `https://docs.unity3d.com/6000.0/Manual/com.unity.cinemachine.html`
- Unity Manual — Universal Render Pipeline, Unity 6: `https://docs.unity3d.com/6000.0/Manual/universal-render-pipeline.html`
- Unity Scripting API — `ObjectPool<T>`: `https://docs.unity3d.com/6000.0/ScriptReference/Pool.ObjectPool_1.html`
- Unity Learn — Karting Microgame: `https://learn.unity.com/project/karting-template`

A pesquisa serve para decisões de arquitetura. Nenhum asset/conteúdo do Karting Microgame foi importado para o projeto.
