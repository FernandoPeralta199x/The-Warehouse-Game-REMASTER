# Áudio — implementação

Implementa as prioridades **P0** e **P1** de `REFERENCIA/The_Warehouse_N8_Sound_Design_SFX_List.md`.

Antes disto o jogo tinha 7 efeitos e nenhum ligado à maior parte do gameplay:
portas, sensores, carga pesada, impacto, ferramentas, medalhas e empilhadeira
eram silenciosos.

## Como os sons são feitos

O projeto é clean-room e não pode usar samples de terceiros, então **todo efeito
é sintetizado** (`Editor/TW08AudioSynth.cs`): tom com varredura, ruído filtrado
por passa-baixa de um polo, onda quadrada suavizada, envelope ADSR, mixagem e
normalização de pico.

Isto não substitui gravação — é a camada que deixa o jogo audível e coerente
enquanto não existe banco de áudio próprio. Trocar por samples reais depois é só
substituir os WAVs: os `AudioEvent` e o código não mudam.

Geração determinística: a semente vem do nome do arquivo, então rodar o pipeline
duas vezes produz bytes idênticos e o git não vê ruído.

## Cobertura

**P0** — passos (3 variações), empurrar carga (3), carga pesada (2), impacto (3),
carga no alvo, porta abrir/fechar, sensor ligar/desligar, UI confirmar/voltar/
foco/recusa, terminal ligando, vitória, falha, empilhadeira (motor em loop, ré,
colisão).

**P1** — as 4 ferramentas da Oficina N-8 com assinatura própria, stingers de
bronze/ouro/platina, compra na loja, tique de créditos, alarme de lockdown,
esteira, ambiência de armazém e de câmara fria, marcadores de fala de John, Duda
e Robert.

Total: **40 WAVs, 39 eventos de áudio**, catálogo sem nenhum campo vazio.

## Decisões

**Variação em som de repetição alta.** Passo, empurrão e impacto têm três
amostras cada, sorteadas a cada disparo. O pilar de "repetição confortável" do
documento existe porque o jogador ouve esses sons centenas de vezes por sessão —
uma amostra única vira tique nervoso em dez minutos.

**Peso audível.** Carga pesada tem som próprio, mais grave e mais longo que a
comum. O jogador deve ouvir a diferença de esforço antes de olhar para a tela.

**Desfazer não reusa o som de empurrar.** Usa o som de voltar, para o jogador
distinguir "avancei" de "voltei" sem conferir o contador.

**Impacto é decidido pelo estado do tabuleiro**, não pelo tipo de movimento: a
carga que chega ao alvo confirma, a que encosta em algo sólido bate.

**Loops são costurados.** `MakeSeamless` faz crossfade do fim no começo — motor,
alarme, esteira e ambiência tocam continuamente e um corte seco clica.

**Normalização obrigatória.** Somar camadas estoura em 1.0 e o WAV grava
distorcido; toda mixagem normaliza o pico em 0,92.

**Alarme de travamento toca uma vez por travamento.** Repetir a cada frame
viraria ruído contínuo.

## Arquitetura

| Camada | Arquivo |
|---|---|
| Síntese | `Editor/TW08AudioSynth.cs` |
| Banco de sons | `Editor/TW08SoundBank.cs` |
| Catálogo | `Audio/TW08AudioCatalog.cs` |
| Ligação ao gameplay | `Audio/PuzzleAudioDirector.cs` |
| Reprodução | `Audio/AudioService.cs` (pool, já existia) |

`PuzzleAudioDirector` fica separado do `PuzzleRuntime` de propósito: a regra do
tabuleiro não deve saber que áudio existe. O diretor assina os eventos que o
runtime já publica e decide o que tocar. É instalado nas cenas de fase pelo
`TW08PuzzleSceneBuilder`.

## Segunda passada: ligar o que estava mudo

O diretor de som mediu os 40 WAVs e apontou que **13 dos 39 eventos não tinham
nenhum call site**. O banco existia inteiro e metade dele nunca tocava.

Ligados agora:

- **Menus** — som ao mudar o foco (`MenuNavigationAudio`, que escuta o
  EventSystem em vez de cada botão, porque o foco muda por teclado, gamepad e
  mouse) e ao confirmar ou recusar.
- **Oficina N-8** — compra confirmada.
- **Narrativa** — o marcador de fala por personagem. `VoiceFor` existia, estava
  correto e nunca era chamado.
- **Corrida** — motor em loop só com o veículo em movimento, bipe de ré detectado
  pelo sentido real da velocidade, e impacto na colisão. Antes só a contagem
  regressiva e a chegada soavam: a empilhadeira era muda enquanto o jogador
  dirigia, que é o que ele faz o tempo todo.

Para isso o catálogo passou a morar em `Resources`: menu, loja e narrativa não
têm campo serializado para ele, e exigir um obrigaria a refazer a fiação de todas
as cenas. `GameAudio` carrega uma vez e nunca lança quando o catálogo falta — som
é decoração, e um projeto sem áudio gerado precisa continuar jogável.

### Correções de qualidade na mesma passada

**Envelope que gravava silêncio.** `Percussive` combina decay de 0,18 com sustain
zero: o sinal zerava em 18,4% do arquivo. O impacto de caixa declarava 130 ms e
tinha 24 ms audíveis. `Impact`, com decaimento exponencial, subiu isso para 78%.

**Sensor e porta tocavam no mesmo frame** e o ouvido lia os dois como um evento
só. A porta agora entra 110 ms depois, e a relação de causa fica audível.

**Recusa de movimento usa o impacto de carga**, não o bipe de recusa do menu: o
jogador esbarra em parede dezenas de vezes por fase, e um bipe de erro nessa
frequência vira irritação. Encostar soa como encostar.

**Ambiência de 3,9 s para 22 s.** Uma volta a cada quatro segundos era percebida
como repetição, não como ambiente.

## O que falta

**P2 e P3** — SFX exclusivos por personagem e por setor, camadas dinâmicas por
tensão, mix adaptativo, reverb por ambiente.

**Trilha sonora** — `REFERENCIA/The_Warehouse_N8_Trilha_Sonora_Prompts_Suno.md`
descreve faixas para serem geradas em ferramenta externa. As três músicas atuais
são procedurais e servem de espaço reservado.

**Voz** — os marcadores de fala são bipes por personagem, que é o que o documento
pede. Voz gravada não está no escopo.
