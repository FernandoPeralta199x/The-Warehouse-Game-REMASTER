# Direção de som — The Warehouse Nº 08

Diagnóstico crítico do áudio implementado, medido nos WAVs reais do banco
(`Assets/_Project/Audio/GeneratedStarter/`), no sintetizador
(`Editor/TW08AudioSynth.cs`), no banco (`Editor/TW08SoundBank.cs`) e no diretor
(`Audio/PuzzleAudioDirector.cs`), comparado com
`REFERENCIA/The_Warehouse_N8_Sound_Design_SFX_List.md` (a bíblia) e
`REFERENCIA/The_Warehouse_N8_Trilha_Sonora_Prompts_Suno.md`.

Todas as medições foram feitas com análise direta dos arquivos (duração, pico,
RMS, fator de crista, envelope em 12 janelas, DFT de quarto de tom até 8 kHz,
energia por banda de oitava, descontinuidade de costura de loop). Onde aparece
**LoudST** significa "RMS máximo em janela de 50 ms multiplicado pelo `volume` do
`AudioEvent`" — o proxy de loudness percebida que uso para comparar com a escala
de volume relativo da bíblia (§6.3). Comparar por pico não serve: no banco atual
o impacto de caixa e a ambiência de armazém têm **exatamente o mesmo pico**
(-0,72 dBFS) e 8,8 dB de diferença de loudness, porque o fator de crista vai de
6,8 dB (loop) a 18,8 dB (transiente).

---

## 0. Sumário executivo

**O que está construído é uma boa fundação com uma camada sonora ainda não
calibrada.** A arquitetura está certa, a convenção de nomes foi seguida à risca,
a decisão de gerar variações está correta e as costuras de loop funcionam. O que
não está entregue é o áudio em si: metade do banco não toca no jogo, a hierarquia
de mixagem declarada no código não é a que sai do alto-falante, e um bug de
envelope faz **todo som percussivo durar 18% do que o código pede**.

Os seis problemas em ordem de impacto:

| # | Problema | Evidência medida | Custo de correção |
|---|---|---|---|
| 1 | `Envelope.Percussive` zera o sinal em 18,4% da duração. Passo, impacto, sensor, blip de voz e clique de UI gravam 82% de silêncio digital. | `crate_hit_wood_01`: arquivo 130 ms, som audível **24 ms**, cauda muda 81,9%. Idêntico em 20 arquivos. | 1 linha em `Evaluate` |
| 2 | 27 das 36 cenas de fase têm `PuzzleAudioFeedback` **e** `PuzzleAudioDirector` ativos ao mesmo tempo. Cada passo e cada empurrão toca duas vezes. | GUID `485d2e3f…` e `cba7ebf0…` coexistem em 27 `.unity` | remover um componente |
| 3 | 13 dos 39 eventos do catálogo nunca são tocados por ninguém. Menu, loja, terminal, diálogo, esteira, alarme de lockdown e toda a empilhadeira são mudos. | `grep` das chamadas: só 24 eventos têm call site | ligação nos sistemas |
| 4 | Normalização inconsistente: só o que passa por `Mix`/`Sequence` é normalizado. A hierarquia de `volume` (0,30–0,86) é ficção. | Sensor de ativação sai a **-27,7 dB** LoudST onde a bíblia pede -12 a -8. Espalhamento real: 37 dB. | recalibrar + `NormalizeLoudness` |
| 5 | Movimento bloqueado é **silêncio absoluto**. `TryMove` retorna `false` sem publicar evento. | `PuzzleRuntime.cs:106-121` — não há `MoveBlocked` | evento novo + som P0 |
| 6 | Ambiência de setor com 3,88 s de loop = **15,5 voltas por minuto**. A bíblia pede 20–60 s. | medido; o release do envelope ainda faz cair 6,3 dB no fim de cada volta | duração + `Envelope.Looping` |

Corrigir 1, 2 e 4 muda a percepção do jogo inteiro e custa pouco código.

---

## 1. Os pilares estão cumpridos?

### 1.1 Clareza — **parcial, 4 de 10**

A bíblia (§3.1) lista dez coisas que o jogador precisa entender pelo som. Estado
medido:

| O jogador entende que… | Som existe? | Toca? | Legível? |
|---|---|---|---|
| caixa empurrou | sim | sim | **não** — duplicado em 27 cenas |
| caixa travou | **não existe** | — | **não** |
| caixa encaixou | sim | sim | fraco — -15,0 dB, 99 ms, mascarado pelo empurrão simultâneo |
| sensor ativou | sim | sim | **não** — -27,7 dB e 18 ms audíveis; a porta toca no mesmo frame e cobre |
| porta abriu | sim | sim | sim, mas 0,55 s onde a bíblia pede 1,80 s |
| Power Up foi usado | sim (4) | sim | parcial — 15 dB de espalhamento entre as quatro |
| tentativa virou assistida | **não existe** | — | **não** |
| empilhadeira bateu | sim | **não** | **não** |
| carga foi danificada | **não existe** | — | **não** |
| fase concluída | sim | sim | fraco — o stinger de sucesso (-14,6 dB) toca junto com a medalha (-4,5 dB) e some |

O Risco 5 da bíblia ("se sensor, porta e caixa no alvo soarem parecidos, o
jogador se confunde") está mitigado no timbre — sensor é quadrada aguda, porta é
senoide grave, alvo é intervalo de quinta — mas quebrado no nível: o sensor está
15 dB abaixo da porta que dispara junto com ele.

### 1.2 Peso — **reprovado na entrega, apesar de a intenção estar certa**

O código faz o certo (carga pesada tem som próprio, mais grave e mais longo),
mas o resultado inverte o pilar. Medido:

```
crate_push_heavy_01   pico espectral 52 Hz   93% da energia abaixo de 80 Hz
crate_push_wood_01    pico espectral 82 Hz   94% da energia em 80–250 Hz
```

Em alto-falante de notebook (corte típico entre 150 e 250 Hz) a caixa **pesada
soa mais fraca que a comum**, não mais pesada — 93% do sinal simplesmente não é
reproduzido. O `Tone(0.30f, 116f, 88f, 0.16f, Env.Soft)` que deveria carregar o
peso para sistemas pequenos está 10 dB abaixo do fundamental e não dá conta.

A solução é psicoacústica, não de volume: **fundamental ausente**. Ouvindo 116 e
174 Hz o cérebro reconstrói 58 Hz mesmo num alto-falante que corta em 200 Hz. A
segunda e a terceira harmônica precisam ser fortes.

Além disso, a bíblia (§3.2) lista oito materiais (metal, madeira, concreto,
borracha, correntes, hidráulica, energia elétrica, máquina antiga). O banco tem
um material — e ele não soa como madeira (ver §2.1).

### 1.3 Repetição confortável — **parcial**

**Bom:** as três variações existem em passo, empurrão e impacto, exatamente como
a bíblia exige em §3.3, e o pitch aleatório está aplicado (±8% no passo, ±10% no
impacto). Isso já coloca o projeto à frente da maioria.

**Três problemas:**

1. `AudioEvent.GetRandomClip()` usa `Random.Range` sem memória. Com três
   variações, a chance de repetir o mesmo clipe imediatamente é 1/3 e de repetir
   três vezes seguidas é 1/9. A bíblia (§26) diz literalmente "não tocar o mesmo
   SFX idêntico repetidamente se houver variações".

2. As variações são fracas. Distância espectral média medida entre as três
   amostras (0 = idênticas, escala normalizada):

   ```
   step_concrete    0,109
   crate_hit_wood   0,056
   crate_push_wood  0,053
   ```

   Os três passos têm centroide 452/481/439 Hz e pico em 117–127 Hz. A variação é
   de semente de ruído, não de caráter. Sob um envelope de 13 ms audíveis, o
   ouvido não distingue. Variação real precisa mudar a frequência das parciais e a
   duração, não só o ruído.

3. O pior: **as ambiências de 3,88 s**. Isso é 15,5 voltas por minuto. Numa fase
   de cinco minutos são 77 repetições do mesmo padrão de ruído. A bíblia §6.2 pede
   20–60 s. É a violação mais séria deste pilar — mais séria que a falta de
   variação de passo, porque a ambiência toca ininterruptamente.

### 1.4 Narrativa sonora — **reprovado**

A bíblia (§3.4 e §15) pede sete identidades de setor. Existem duas:

- `sector_warehouse_ambience_loop` — 99% da energia abaixo de 80 Hz, pico em 48 Hz
- `sector03_freezer_ambience_loop` — 86% em 80–250 Hz, pico em 117 Hz

São a **mesma textura em nota diferente**. Nenhuma tem os agudos que criam sensação
de espaço (ventilação, ar, metal distante): ambas caem a zero acima de 800 Hz.
Um armazém noturno vazio é definido tanto pelo hum quanto pelo silêncio com ar em
cima dele.

E `PuzzleAudioDirector.StartAmbience` resolve o setor com
`runtime.Level.SectorId.Contains("03")` — os setores S01, S02, S04, S05, S06 e as
fases secretas (25 das 30 fases da campanha, mais 10 secretas) caem todas na
mesma ambiência genérica. Setor 04 tem cinco fases e não toca esteira; Setor 05
não tem gerador; Setor 06 não tem alarme distante; Setor 08 não tem nada.

Os três marcadores de fala (John/Duda/Robert) foram construídos com alturas
distintas e corretas (185 / 330 / 147 Hz — grave, médio-agudo, muito grave) mas
**nenhum é tocado no jogo** e todos saem a -37 dB, ou seja, inaudíveis mesmo se
fossem.

---

## 2. Qualidade som a som

### 2.0 O bug que afeta 20 arquivos: `Envelope.Percussive`

```csharp
public static Envelope Percussive => new(0.004f, 0.18f, 0f, 0.30f);
```

`Attack + Decay = 0,184`. Com `Sustain = 0`, o `Evaluate` devolve zero para todo
`t > 0,184`, e o trecho de release faz `Lerp(0f, 0f, k)`, que também é zero. O
som existe apenas nos primeiros **18,4%** da duração pedida.

Cauda muda medida (fração do arquivo após o último sinal acima de -45 dB do pico):

```
step_concrete_01           82,2%   (62 ms de silêncio em 75 ms de arquivo)
crate_hit_wood_01          81,9%   (106 ms de 130 ms)
forklift_impact_01         81,9%   (180 ms de 220 ms)
sensor_activate_01         81,7%   ( 82 ms de 100 ms)
voice_john_blip_01         82,6%   ( 37 ms de 45 ms)
ui_credit_tick_01          81,9%   ( 29 ms de 35 ms)
```

Consequências: (a) os sons percussivos duram muito menos do que o código declara
e do que a bíblia pede; (b) todos ficam sem cauda, o que soa como clique digital,
não como objeto físico ressoando; (c) o `AudioService.ReturnAfter` segura uma
`AudioSource` do pool por 130 ms para tocar 24 ms.

**Correção — uma linha em `Evaluate` mais um envelope novo:**

```csharp
public Envelope(float attack, float decay, float sustain, float release, float decayShape = 1f)
{
    ...
    DecayShape = Mathf.Max(0.2f, decayShape);
}

public float DecayShape { get; }

/// <summary>
/// Impacto: decaimento exponencial ao longo de toda a duração. O Percussive
/// antigo (decay 0,18 com sustain 0) zerava o sinal em 18,4% do arquivo e
/// gravava 82% de silêncio — todo transiente saía cinco vezes mais curto que o
/// pedido e sem cauda.
/// </summary>
public static Envelope Impact => new(0.0015f, 0.998f, 0f, 0.0015f, 2.6f);

/// <summary>
/// Loop: platô com micro-fades. O Sustained antigo tinha release em 20% da
/// duração, o que fazia a ambiência cair 6,3 dB no fim de cada volta — uma
/// respiração audível a cada 3,9 s.
/// </summary>
public static Envelope Looping => new(0.004f, 0.004f, 1f, 0.004f);

// dentro de Evaluate, o trecho de decay:
if (afterAttack < Decay)
{
    float k = afterAttack / Decay;
    return Sustain + (1f - Sustain) * Mathf.Pow(1f - k, DecayShape);
}
```

Com `DecayShape = 1f` a expressão é idêntica ao `Mathf.Lerp(1f, Sustain, k)`
atual, então nada existente quebra. Com 2,6 o decaimento passa por -2,4 dB em 10%
da duração, -8,2 dB em 30%, -15,6 dB em 50% e -34,7 dB em 80% — a curva de um
corpo físico ressoando.

### 2.1 Impacto de caixa de madeira — **não soa como madeira**

```csharp
seed => TW08AudioSynth.Mix(
    TW08AudioSynth.Noise(0.13f, 0.70f, Env.Percussive, seed, 0.30f),
    TW08AudioSynth.Tone(0.13f, 190f, 70f, 0.55f, Env.Percussive, harmonics: 0.20f)),
```

Medido: 96% da energia em 80–250 Hz, pico espectral em 180 Hz, centroide 449 Hz,
duração audível 24 ms.

Isso é um **bumbo eletrônico**, não madeira. Três razões técnicas:

1. **A varredura 190→70 Hz em 130 ms é a assinatura literal de um kick 808.** Um
   impacto em madeira não tem *pitch drop*: as parciais são fixas, o que muda é a
   amplitude. O sweep é o que faz o ouvido classificar o som como percussão
   eletrônica.

2. **`harmonics: 0.20f` soma o terceiro harmônico exato (3f).** Uma relação
   harmônica inteira é a assinatura de tubo ou corda (clarinete, órgão). Uma caixa
   de madeira tem parciais **inarmônicas** — modos de flexão da tampa e das
   laterais em razões como 1 : 2,4 : 4,1. É essa inarmonicidade que o ouvido lê
   como "madeira".

3. **O ruído está com passa-baixa em `0.30f`.** Num filtro de um polo,
   fc = -ln(1-a)·fs/(2π), então `0.30` a 22050 Hz corta em **1252 Hz** com queda
   de apenas 6 dB/oitava. O transiente de madeira vive em 2–8 kHz — e a taxa de
   22,05 kHz nem permite chegar lá. O que sobra é ruído grave, que o tom de 190 Hz
   mascara completamente (daí os 96% em 80–250 Hz).

**Proposta:**

```csharp
// Impacto de madeira: transiente largo, corpo curto e três parciais fixas em
// razões inarmônicas (1 : 2,42 : 4,10). Sem varredura de frequência: um
// pitch drop é a assinatura de bumbo eletrônico, não de caixa batendo.
events["crateHit"] = MultiEvent(
    "Crate_Hit_Wood",
    new[] { "crate_hit_wood_01", "crate_hit_wood_02", "crate_hit_wood_03" },
    seed => TW08AudioSynth.Mix(
        // Estalo: 8 ms de ruído aberto. É este pedaço que diz "madeira".
        TW08AudioSynth.Noise(0.030f, 0.85f, Env.Impact, seed + "_tick", 0.80f),
        // Corpo do ruído, banda média.
        TW08AudioSynth.Noise(0.16f, 0.42f, Env.Impact, seed, 0.34f),
        // Modo fundamental da tábua.
        TW08AudioSynth.Tone(0.26f, 196f, 194f, 0.44f, Env.Impact),
        // Modos superiores: morrem antes, que é o que faz o som "escurecer".
        TW08AudioSynth.Tone(0.13f, 474f, 470f, 0.26f, Env.Impact),
        TW08AudioSynth.Tone(0.065f, 803f, 798f, 0.15f, Env.Impact)),
    volume: 0.70f, pitchMin: 0.90f, pitchMax: 1.10f);
```

Duração 0,26 s (bíblia §9.1 pede 0,25–0,28 s para `crate_hit_wall_wood`). As três
variações devem também mudar a **frequência do modo fundamental** (188 / 196 /
207 Hz) e não só a semente, para que a distância espectral saia de 0,056 para
algo perceptível.

### 2.2 Empurrar caixa de madeira — **soa como motor, não como arrasto**

```csharp
TW08AudioSynth.Noise(0.20f, 0.62f, new Env(0.02f, 0.10f, 0.60f, 0.30f), seed, 0.14f),
TW08AudioSynth.Tone(0.20f, 92f, 74f, 0.34f, new Env(0.03f, 0.12f, 0.55f, 0.30f))
```

Medido: pico em 82 Hz, 94% da energia em 80–250 Hz, centroide 343 Hz.

O passa-baixa em `0.14f` corta em **529 Hz**. Todo o atrito — que é o que
identifica o material e vive entre 400 Hz e 3 kHz — foi filtrado fora. Sobra o
rumble, que soa como um motor pequeno ligando.

Duração 0,20 s também está no piso: a bíblia pede 0,30–0,34 s.

**Proposta:**

```csharp
// Arrasto: a informação de material vive em 400–2000 Hz. O grave é peso, não
// identidade — se ele domina, o som vira motor.
seed => TW08AudioSynth.Mix(
    TW08AudioSynth.Noise(0.30f, 0.52f, new Env(0.025f, 0.10f, 0.72f, 0.34f), seed, 0.42f),
    TW08AudioSynth.Noise(0.30f, 0.30f, new Env(0.03f, 0.12f, 0.62f, 0.34f), seed + "_grain", 0.16f),
    TW08AudioSynth.Tone(0.30f, 104f, 88f, 0.22f, new Env(0.03f, 0.12f, 0.60f, 0.34f)))
```

`0.42f` corta em 1912 Hz e deixa o atrito passar; a camada `_grain` em `0.16f`
(608 Hz) dá a irregularidade do arrasto sem virar rumble; o tom cai de 0,34 para
0,22 de amplitude para sair da frente.

A bíblia (§9.1) pede também `crate_stop_wood_01` (0,12 s, **P0**) — a batida seca
de parada no fim do empurrão. Hoje o empurrão simplesmente desaparece.

### 2.3 Carga pesada — **grave demais para ser audível**

93% abaixo de 80 Hz, pico em 52 Hz. Ver §1.2.

**Proposta:**

```csharp
// Peso audível em hardware pequeno: a segunda e a terceira harmônica carregam
// a impressão de grave mesmo num alto-falante que corta em 200 Hz (fundamental
// ausente). Sem elas, 93% do sinal não é reproduzido e a carga pesada soa
// MAIS FRACA que a comum.
seed => TW08AudioSynth.Mix(
    TW08AudioSynth.Noise(0.45f, 0.44f, new Env(0.03f, 0.14f, 0.68f, 0.32f), seed, 0.30f),
    TW08AudioSynth.Tone(0.45f, 58f, 46f, 0.50f, new Env(0.04f, 0.16f, 0.62f, 0.32f)),
    TW08AudioSynth.Tone(0.45f, 116f, 92f, 0.34f, new Env(0.04f, 0.16f, 0.60f, 0.32f)),
    TW08AudioSynth.Tone(0.45f, 174f, 138f, 0.20f, new Env(0.05f, 0.18f, 0.55f, 0.32f)))
```

Duração 0,45 s conforme §9.3 (`crate_heavy_push_01`). O passa-baixa sobe de
`0.10f` (370 Hz) para `0.30f` (1252 Hz) para que o arrasto exista.

### 2.4 A onda quadrada injeta DC — **afeta 6 sons**

```csharp
float value = phase < duty ? 1f : -1f;
```

Com `duty ≠ 0.5` o valor médio da onda é `2·duty - 1`, ou seja um offset DC
constante, multiplicado pelo envelope. O resultado é um pulso de sub-grave a cada
disparo, mais perda de headroom.

Medido:

```
sensor_activate_01     duty 0.35   DC -0,0059   81% da energia abaixo de 80 Hz
powerup_scanner_01     duty 0.25   DC -0,0051   60% da energia abaixo de 80 Hz
terminal_boot          duty 0.30   DC -0,0086   67% da energia abaixo de 80 Hz
```

O bipe do sensor tem **81% da sua energia num thump inaudível de sub-grave**.
Isso explica por que ele sai a -27,7 dB: quase todo o headroom do arquivo foi
gasto no offset, não no bipe.

Perda de headroom por duty: 0,35 → -3,1 dB; 0,30 → -4,4 dB; 0,25 → -6,0 dB.

**Correção:**

```csharp
// Onda quadrada centrada. Com duty != 0,5 a forma crua tem valor médio
// 2*duty-1: um offset DC modulado pelo envelope, que vira thump de sub-grave e
// come o headroom do bipe. Medido: com duty 0,35 o sensor gastava 81% da
// energia abaixo de 80 Hz.
float scale = 1f / (2f * Mathf.Max(duty, 1f - duty));
float value = (phase < duty ? 2f * (1f - duty) : -2f * duty) * scale;
samples[i] = value * 0.7f * amplitude * envelope.Evaluate(t);
```

Verificação: com `duty = 0.35`, o valor alto vira +1,0 e o baixo -0,538; a média
é `0,35·1,0 + 0,65·(-0,538) = 0`. Com `duty = 0.5` continua ±1,0 exatamente como
hoje.

### 2.5 Bip de ré da empilhadeira — **ritmo quebrado e primeiro bip mutilado**

Medido no `forklift_reverse_beep_loop.wav`:

```
inícios de bip:      6,0 ms  e  550,1 ms
duração do loop:     680 ms
intervalo interno:   544 ms
intervalo na volta:  136 ms    ->  RITMO IRREGULAR
pico do bip 1:      -16,1 dBFS
pico do bip 2:       -0,7 dBFS   ->  15,4 dB de diferença
```

Dois defeitos somados. O `MakeSeamless(…, 0.05f)` fez o crossfade **por cima do
primeiro bip**, derrubando-o 15 dB. E o `Sequence` posiciona os bipes em 0 e
0,55 s num arquivo de 0,68 s, então na volta do loop o segundo intervalo é 136 ms
— o alarme sai "bip … bip-bip … bip … bip-bip".

Além disso, cada bip dura 33 ms audíveis (por causa do `Percussive`), quando um
alarme de ré real emite 250–400 ms. Soa como tique, não como bipe.

**Proposta:**

```csharp
// Alarme de ré: 1,0 s de ciclo (bíblia §10.1), bipe de 300 ms, silêncio de
// 700 ms. O crossfade cai no silêncio, não em cima do bipe.
events["forkliftReverse"] = LoopEvent(
    "Forklift_Reverse",
    "forklift_reverse_beep_loop",
    TW08AudioSynth.MakeSeamless(TW08AudioSynth.Sequence(
        (0.02f, TW08AudioSynth.Square(0.30f, 988f, 0.42f, new Env(0.01f, 0.02f, 0.95f, 0.06f), 0.5f)),
        (1.02f, new float[TW08AudioSynth.SampleCount(0.02f)])),
        0.02f),
    volume: 0.46f);
```

A segunda entrada é um bloco de silêncio que fixa o comprimento total do arquivo
em 1,04 s; o crossfade de 20 ms cai dentro dele. Envelope quase plano para o bipe
soar como bipe e não como clique.

### 2.6 Alarme de lockdown — **audiometria, não alarme**

```csharp
TW08AudioSynth.Tone(0.55f, 440f, 620f, 0.34f, Env.Soft),
TW08AudioSynth.Tone(0.55f, 620f, 440f, 0.34f, Env.Soft)
```

Medido: 100% da energia em 250–800 Hz, um único pico espectral em 494 Hz, o
segundo pico 62 dB abaixo. É uma senoide pura varrendo. Um alarme industrial é um
buzzer piezo ou uma sirene eletromecânica: harmônicos ímpares fortes até 4 kHz,
que é o que faz o som "cortar" o ambiente.

O envelope também gagueja: a sexta das doze janelas cai para -8,6 dB, porque o
release de 35% do primeiro `Tone` termina antes de o segundo entrar.

E a duração: 1,07 s onde a bíblia (§18) pede `system_lockdown_loop.ogg` com 10 s.

**Proposta:**

```csharp
// Alarme: quadrada, não senoide. Os harmônicos ímpares são o que faz o alarme
// cortar o ambiente; uma senoide pura soa como teste de audiometria. Ciclo de
// 2 s repetido 5 vezes = 10 s de loop (bíblia §18).
events["lockdownAlarm"] = LoopEvent(
    "Lockdown_Alarm",
    "alarm_lockdown_loop",
    TW08AudioSynth.MakeSeamless(TW08AudioSynth.Sequence(
        (0.00f, TW08AudioSynth.Square(0.62f, 660f, 0.30f, new Env(0.05f, 0.06f, 0.90f, 0.14f), 0.5f)),
        (0.00f, TW08AudioSynth.Tone(0.62f, 330f, 330f, 0.14f, new Env(0.05f, 0.06f, 0.90f, 0.14f))),
        (0.70f, TW08AudioSynth.Square(0.62f, 880f, 0.30f, new Env(0.05f, 0.06f, 0.90f, 0.14f), 0.5f)),
        (0.70f, TW08AudioSynth.Tone(0.62f, 440f, 440f, 0.14f, new Env(0.05f, 0.06f, 0.90f, 0.14f)))
        /* … repetir o par a 1.40, 2.10, 2.80 … até 10 s … */),
        0.10f),
    volume: 0.34f);
```

A camada de oitava abaixo (330/440 Hz) dá corpo sem tirar o corte. Padrão de dois
tons alternados a 660/880 Hz é o intervalo de quinta justa, que soa "de sistema"
sem ficar dissonante contra a trilha em Dó menor.

### 2.7 Portas — **curtas demais e sem articulação**

```
door_open_heavy_01    550 ms   pico 90 Hz    99% em 80–250 Hz
door_close_heavy_01   540 ms   pico 101 Hz   99% em 80–250 Hz
```

A bíblia (§11.2) pede **1,80 s** para portão industrial. E um portão tem três
partes: destravamento (clunk metálico), corpo (motor/servo + trilho rangendo,
com variação de velocidade) e chegada ao batente. Hoje o `doorOpen` é um único
sweep 70→130 Hz — soa como um zumbido subindo.

O `doorClose` acertou ao ter um clunk final separado; é a construção mais bem
articulada do banco e serve de modelo para o resto.

**Proposta para `doorOpen`** (espelhando o `doorClose`, que está certo):

```csharp
events["doorOpen"] = SingleEvent(
    "Door_Open_Heavy",
    "door_open_heavy_01",
    TW08AudioSynth.Sequence(
        // Destrava: relé + ferrolho.
        (0.00f, TW08AudioSynth.Mix(
            TW08AudioSynth.Noise(0.09f, 0.55f, Env.Impact, "door_unlock", 0.55f),
            TW08AudioSynth.Tone(0.12f, 320f, 300f, 0.24f, Env.Impact))),
        // Corpo: motor sobe, trilho range em cima.
        (0.12f, TW08AudioSynth.Mix(
            TW08AudioSynth.Tone(1.40f, 64f, 118f, 0.44f, Env.Soft, harmonics: 0.16f),
            TW08AudioSynth.Tone(1.40f, 128f, 236f, 0.16f, Env.Soft),
            TW08AudioSynth.Noise(1.40f, 0.24f, Env.Soft, "door_rail", 0.26f))),
        // Chegada ao fim de curso.
        (1.50f, TW08AudioSynth.Mix(
            TW08AudioSynth.Noise(0.16f, 0.48f, Env.Impact, "door_stop", 0.34f),
            TW08AudioSynth.Tone(0.20f, 132f, 96f, 0.34f, Env.Impact)))),
    volume: 0.42f, pitchMin: 0.98f, pitchMax: 1.02f);
```

`0.26f` corta em 1104 Hz — o rangido do trilho precisa desses agudos.

### 2.8 Passos — **curtos demais, sem o clique da sola**

75 ms de arquivo, **13 ms audíveis**. A bíblia pede 0,12–0,14 s. E um passo em
concreto tem duas partes: o clique do salto/sola em 1,5–4 kHz e o corpo em
100–200 Hz. Hoje só existe o corpo, filtrado em `0.22f` (872 Hz).

```csharp
seed => TW08AudioSynth.Mix(
    // Sola: 6 ms de ruído aberto. É o que diz "concreto duro".
    TW08AudioSynth.Noise(0.022f, 0.62f, Env.Impact, seed + "_tap", 0.75f),
    // Corpo do passo.
    TW08AudioSynth.Noise(0.13f, 0.40f, Env.Impact, seed, 0.28f),
    TW08AudioSynth.Tone(0.13f, 128f, 118f, 0.26f, Env.Impact)),
volume: 0.92f, pitchMin: 0.92f, pitchMax: 1.08f);
```

Sem varredura de frequência (o `132f → 96f` atual é outro *pitch drop* de
percussão eletrônica). As três variações devem usar 120 / 128 / 141 Hz.

### 2.9 Marcadores de fala — **senoides puras, inaudíveis**

```
voice_robert_blip_01   pico 147 Hz, segundo pico 72 dB abaixo    9 ms audíveis   -36,8 dB
```

Um seno puro de 9 ms é um tique, não um marcador de fala. Blips de fala legíveis
(a família Undertale/Animal Crossing) têm três coisas: um formante (não uma
senoide), 40–90 ms de duração, e ruído de sopro. E precisam ser **derivados dos
motivos musicais dos personagens** (ver §6.6), não de frequências arbitrárias.

```csharp
events["voiceRobert"] = SingleEvent(
    "Voice_Robert",
    "voice_robert_blip_01",
    TW08AudioSynth.Mix(
        // Fundamental grave de Robert.
        TW08AudioSynth.Square(0.070f, 147f, 0.24f, new Env(0.10f, 0.18f, 0.55f, 0.35f), 0.42f),
        // Formante: é a segunda ressonância que faz o bipe soar como voz.
        TW08AudioSynth.Tone(0.070f, 441f, 415f, 0.12f, new Env(0.12f, 0.20f, 0.50f, 0.35f)),
        TW08AudioSynth.Noise(0.070f, 0.08f, new Env(0.10f, 0.20f, 0.45f, 0.35f), "robert", 0.50f)),
    volume: 0.32f, pitchMin: 0.96f, pitchMax: 1.05f);
```

John em 185 Hz com formante em 555 Hz; Duda em 330 Hz com formante em 990 Hz e
uma pitada de glitch (`Square` com duty 0,18) para a identidade digital que a
bíblia dá a ela.

### 2.10 O que está bem construído

**Não mexer:**

- **`crate_place_goal`** — o intervalo Dó5 (523 Hz) → Sol5 (784 Hz) é uma quinta
  justa ascendente, curta, positiva e não vira fanfarra. É a escolha certa para um
  som que toca dez vezes por fase. Só precisa de nível e de ligar as duas notas
  (hoje há 54 ms de silêncio entre elas por causa do `Percussive`).
- **`door_close_heavy`** — corpo + clunk separados no `Sequence`. É a única
  construção do banco com articulação real, e é o modelo para porta, esteira,
  gerador e garfo.
- **A escada das medalhas** — Sol-Dó / Dó-Mi-Sol / Dó-Mi-Sol-Dó8. Mais notas,
  mais brilho, mais alto. A ideia está certa; falta duração (0,50/0,72/0,98 s
  contra 1,00/1,30/1,60 s da bíblia) e falta baixar o nível.
- **`MakeSeamless`** — funciona. Descontinuidade medida na costura, em % do pico:
  motor 0,21%, armazém 0,32%, alarme 0,69%, freezer 1,44%, esteira 2,17%. Para
  comparação, as três músicas (que **não** passam por `MakeSeamless`) medem
  8,7% a 9,7% — clique audível a cada volta. A ferramenta resolve o problema; só
  falta aplicá-la à música.
- **A determinismo por semente** e o `WriteWaveIfChanged` por comparação de bytes.
  Rodar o pipeline duas vezes não suja o git. Isso é correto e raro.
- **A convenção de nomes** foi seguida literalmente: `crate_push_wood_01.wav`,
  `forklift_engine_idle_loop.wav`, `victory_gold_stinger.wav`,
  `sector03_freezer_ambience_loop.wav`, `terminal_boot_one_shot.wav` batem com os
  exemplos do §5 da bíblia, sufixos incluídos.

### 2.11 Taxa de amostragem

`SampleRate = 22050` dá Nyquist em 11 kHz. A bíblia (§6.1) pede **44,1 kHz** para
SFX. Hoje isso não é o gargalo, porque nenhum som tem conteúdo acima de 2,5 kHz —
mas passa a ser assim que as correções de §2.1, §2.8 e §2.7 forem aplicadas: o
estalo de madeira e o clique da sola vivem em 3–8 kHz e não cabem em 22 kHz.

O banco inteiro ocupa hoje ~1,1 MB. A 44,1 kHz seria ~2,2 MB. Subir é gratuito e
destrava metade das correções.

---

## 3. Mixagem

### 3.1 A hierarquia declarada não é a que sai

Medição completa, ordenada por loudness real, contra a escala da bíblia §6.3:

| Evento | vol | pico do arquivo | **LoudST** | Alvo bíblia | Erro |
|---|---:|---:|---:|---|---:|
| medalPlatinum | 0,86 | -0,72 | **-4,5** | Vitória -12…-8 | **+3,5** |
| medalGold | 0,80 | -0,72 | **-6,1** | Vitória -12…-8 | +1,9 |
| doorOpen | 0,74 | -0,72 | **-7,5** | — | — |
| medalBronze | 0,74 | -0,72 | **-8,0** | Vitória -12…-8 | ok |
| cratePushHeavy | 0,86 | -0,72 | **-8,2** | Caixas -14…-10 | **+1,8** |
| doorClose | 0,78 | -0,72 | **-8,2** | — | — |
| shopPurchase | 0,68 | -0,72 | **-8,5** | UI -18…-12 | **+3,5** |
| toolScanner | 0,62 | -0,72 | **-9,3** | UI -18…-12 | **+2,7** |
| terminalBoot | 0,58 | -0,72 | **-10,0** | UI -18…-12 | **+2,0** |
| toolAssistant | 0,60 | -0,72 | **-10,2** | UI -18…-12 | **+1,8** |
| puzzlePush | 0,78 | -0,72 | **-11,5** | Caixas -14…-10 | ok |
| lockdownAlarm | 0,44 | -0,77 | **-11,8** | Alertas -12…-8 | ok |
| forkliftEngine | 0,44 | -1,18 | **-12,6** | Empilh. -16…-10 | ok |
| forkliftReverse | 0,50 | -0,72 | **-13,3** | Empilh. -16…-10 | ok |
| forkliftImpact | 0,84 | -0,72 | **-13,4** | Empilh. -16…-10 | ok |
| uiDenied | 0,62 | -0,72 | **-13,6** | UI -18…-12 | ok |
| puzzleSuccess | 0,88 | -10,46 | **-14,6** | Vitória -12…-8 | **-2,6** |
| raceFinish | 0,88 | -10,46 | **-14,6** | Vitória -12…-8 | **-2,6** |
| puzzleError | 0,80 | -8,66 | **-14,7** | Vitória -12…-8 | **-2,7** |
| crateOnGoal | 0,72 | -0,72 | **-15,0** | Caixas -14…-10 | **-5,0** |
| crateHit | 0,80 | -0,72 | **-15,5** | Caixas -14…-10 | **-5,5** |
| conveyorLoop | 0,36 | -0,72 | **-15,7** | Ambiente -24…-18 | **+2,3** |
| raceCountdown | 0,82 | -11,06 | **-15,8** | UI -18…-12 | ok |
| uiConfirm | 0,75 | -11,06 | **-16,6** | UI -18…-12 | ok |
| toolRewind | 0,66 | -10,30 | **-17,3** | UI -18…-12 | ok |
| warehouseAmbience | 0,26 | -0,72 | **-17,7** | Ambiente -24…-18 | **+0,3** |
| freezerAmbience | 0,30 | -0,72 | **-19,3** | Ambiente -24…-18 | ok |
| puzzleStep | 0,52 | -0,72 | **-23,0** | Passos -20…-16 | **-7,0** |
| toolMarker | 0,58 | -11,20 | **-24,1** | UI -18…-12 | **-12,1** |
| uiBack | 0,60 | -10,52 | **-27,4** | UI -18…-12 | **-15,4** |
| sensorOn | 0,56 | -13,56 | **-27,7** | Alertas -12…-8 | **-19,7** |
| sensorOff | 0,52 | -14,16 | **-28,2** | Alertas -12…-8 | **-20,2** |
| voiceRobert | 0,34 | -13,93 | **-36,8** | — | — |
| voiceJohn | 0,32 | -14,35 | **-38,0** | — | — |
| uiFocus | 0,34 | -14,05 | **-38,4** | UI -18…-12 | **-26,4** |
| voiceDuda | 0,32 | -14,61 | **-38,8** | — | — |
| creditsTick | 0,30 | -16,22 | **-41,5** | UI -18…-12 | **-29,5** |

**Espalhamento real: 37 dB.** A escala de `volume` no código vai de 0,30 a 0,86,
que são 9,2 dB. Os outros 28 dB vêm de um lugar não intencional: **a coluna "pico
do arquivo"**.

Causa raiz: `Mix()` e `Sequence()` chamam `Normalize(…, 0.92f)`; `Tone()` e
`Square()` chamados diretamente não chamam nada. Doze sons do banco são camada
única e saem no seu próprio nível — de -10,3 a -16,2 dBFS de pico, contra -0,72
dos demais. O `volume` do `AudioEvent` está multiplicando picos que diferem em
15 dB entre si.

### 3.2 O que mascara o quê

1. **Sensor + porta.** `OnSwitchChanged` dispara os dois no mesmo frame. O sensor
   está 20 dB abaixo da porta, e 81% da energia dele está num sub-grave que colide
   exatamente com o fundamental da porta (90–101 Hz). Resultado prático: o jogador
   ouve a porta e não sabe que um sensor foi acionado — que é justamente a
   informação de causa.

2. **Sucesso + medalha.** `OnCompleted` toca `PuzzleSuccess` (-14,6) e a medalha
   (até -4,5) no mesmo frame. A medalha está 10 dB acima e ocupa a mesma banda
   (250–800 Hz). O stinger de fase concluída é inaudível.

3. **Empurrão + impacto / empurrão + alvo.** `OnMoveApplied` toca `PuzzlePush`
   (-11,5) e depois `CrateHit` (-15,5) ou `CrateOnGoal` (-15,0). O empurrão está
   4–5 dB acima do evento informativo que vem junto. Está invertido: a informação
   é o impacto, não o gesto.

4. **Ambiência de armazém sobre tudo.** -17,7 dB é o teto da faixa de ambiente da
   bíblia, e ela é 99% sub-80 Hz, exatamente onde vivem o empurrão pesado (93%
   abaixo de 80 Hz) e as portas. O critério de aceite 24.4 ("ambiente não cobre
   SFX importantes") não está atendido.

5. **Esteira a -15,7 dB** está 2,3 dB acima do teto de ambiente e mais alta que o
   impacto de caixa. Se ela chegar a tocar (hoje não toca), cobre o gameplay.

### 3.3 Não existe AudioMixer

`Assets/_Project/Audio/Mixers/` está **vazio**. Nenhum `AudioEvent` gerado tem
`mixerGroup` atribuído (o `EnsureEvent` não seta o campo), então todo som vai
direto ao master do Unity. A bíblia (§21.2) especifica nove buses:

```
Master / Music / Ambience / SFX / UI / Vehicle / Puzzle / VoiceRadio / Alerts
```

Sem eles não há: ducking de alertas (mitigação do Risco 2), filtro por ambiente
(o abafado da Câmara Fria), controle de categoria além dos dois `PlayerPrefs`
aplicados por script, nem limitador no master. Com quatro ou cinco sons
simultâneos a -6 dB o master satura na saída do dispositivo.

**Ação (asset, não código):** criar `TW08_Master.mixer` com os nove grupos,
limitador no Master com teto em -1 dBFS, e um `Duck Volume` no bus `Music`
alimentado por `Alerts` (-4 dB, ataque 20 ms, release 400 ms) e por `Puzzle`
(-2 dB, release 250 ms). Depois atribuir `mixerGroup` no `EnsureEvent`.

### 3.4 Correção de mixagem — dois níveis

**Nível 1 — imediato, sem tocar no sintetizador.** Recalibrar o `volume` de cada
`AudioEvent` por `volume_novo = volume_atual × 10^((alvo − LoudST)/20)`, com
teto em 1,0:

| Evento | vol atual | **vol novo** | Observação |
|---|---:|---:|---|
| puzzleStep | 0,52 | **0,92** | |
| puzzlePush | 0,78 | **0,74** | |
| cratePushHeavy | 0,86 | **0,56** | |
| crateHit | 0,80 | **1,00** | ainda 0,6 dB abaixo — precisa reconstruir |
| crateOnGoal | 0,72 | **1,00** | ainda 2,0 dB abaixo |
| doorOpen | 0,74 | **0,39** | |
| doorClose | 0,78 | **0,45** | |
| sensorOn | 0,56 | **1,00** | **faltam 12,6 dB — impossível sem reconstruir** |
| sensorOff | 0,52 | **1,00** | **faltam 13,3 dB** |
| uiBack | 0,60 | **1,00** | faltam 7,9 dB |
| uiFocus | 0,34 | **1,00** | **faltam 18,0 dB** |
| uiDenied | 0,62 | **0,53** | |
| terminalBoot | 0,58 | **0,33** | |
| forkliftEngine | 0,44 | **0,30** | alvo -16 (motor de fundo) |
| forkliftReverse | 0,50 | **0,46** | |
| forkliftImpact | 0,84 | **1,00** | faltam 1,5 dB |
| toolRewind | 0,66 | **0,86** | |
| toolScanner | 0,62 | **0,32** | |
| toolAssistant | 0,60 | **0,35** | |
| toolMarker | 0,58 | **1,00** | faltam 4,7 dB |
| medalBronze | 0,74 | **0,52** | alvo -11 |
| medalGold | 0,80 | **0,51** | alvo -10 |
| medalPlatinum | 0,86 | **0,51** | alvo -9 |
| shopPurchase | 0,68 | **0,36** | |
| creditsTick | 0,30 | **1,00** | **faltam 11,0 dB** |
| lockdownAlarm | 0,44 | **0,34** | alvo -14 (loop, não one-shot) |
| conveyorLoop | 0,36 | **0,20** | |
| warehouseAmbience | 0,26 | **0,16** | |
| freezerAmbience | 0,30 | **0,22** | |
| uiConfirm | 0,75 | **0,90** | |
| puzzleSuccess | 0,88 | **1,00** | faltam 2,3 dB |
| puzzleError | 0,80 | **1,00** | |
| raceCountdown | 0,82 | **1,00** | |
| raceFinish | 0,88 | **1,00** | faltam 3,4 dB |
| voiceJohn / Duda / Robert | 0,32–0,34 | **1,00** | **faltam 11–13 dB cada** |

Dez eventos batem no teto de 1,0 e ainda ficam abaixo do alvo — sete deles por
5 a 18 dB. Isso demonstra que ajuste de volume sozinho não resolve: **os WAVs
desses dez precisam ser regravados com normalização.**

**Nível 2 — estrutural.** Normalizar por loudness, não por pico, e derivar o
volume do evento a partir da escala da bíblia:

```csharp
/// <summary>
/// Ajusta o ganho para que o RMS máximo em janela de 50 ms atinja
/// <paramref name="targetDbfs"/>, respeitando um teto de pico.
///
/// Normalizar por pico não serve para SFX: no banco anterior o impacto de caixa
/// e a ambiência de armazém tinham o mesmo pico (-0,72 dBFS) e 8,8 dB de
/// diferença de loudness, porque o fator de crista vai de 6,8 dB num loop a
/// 18,8 dB num transiente.
/// </summary>
internal static float[] NormalizeLoudness(float[] samples, float targetDbfs, float peakCeiling = 0.94f)
{
    int window = Mathf.Min(SampleCount(0.05f), samples.Length);
    double best = 0d, acc = 0d;
    for (int i = 0; i < samples.Length; i++)
    {
        acc += samples[i] * (double)samples[i];
        if (i >= window) acc -= samples[i - window] * (double)samples[i - window];
        if (i >= window - 1 && acc > best) best = acc;
    }

    float loudness = (float)Math.Sqrt(best / window);
    if (loudness <= 0.0001f) return samples;

    float peak = 0f;
    foreach (float sample in samples) peak = Mathf.Max(peak, Mathf.Abs(sample));

    float gain = Mathf.Min(
        Mathf.Pow(10f, targetDbfs / 20f) / loudness,
        peakCeiling / Mathf.Max(peak, 0.0001f));

    for (int i = 0; i < samples.Length; i++) samples[i] *= gain;
    return samples;
}
```

E os alvos como constantes nomeadas em `TW08SoundBank`, citando a bíblia:

```csharp
// Escala de volume relativo da bíblia (§6.3), em dBFS de loudness de curto prazo.
private const float Alert   = -10f;  // biblia: -12 a  -8
private const float Victory = -10f;  // biblia: -12 a  -8
private const float Crate   = -12f;  // biblia: -14 a -10
private const float Vehicle = -13f;  // biblia: -16 a -10
private const float Ui      = -15f;  // biblia: -18 a -12
private const float Voice   = -16f;
private const float Step    = -18f;  // biblia: -20 a -16
private const float Ambient = -21f;  // biblia: -24 a -18
```

Com isso o `volume` do `AudioEvent` volta a ser o que deveria ser — um trim de
ajuste fino perto de 1,0 — e a hierarquia mora nos arquivos.

---

## 4. Cobertura

### 4.1 P0 que ainda falta

| Item da bíblia | Status | Impacto |
|---|---|---|
| `puzzle_invalid_move_01` / `player_blocked_01` | **não existe e não toca** | **crítico** — o jogador empurra contra a parede e o jogo fica mudo. Critério de aceite 24.1 não atendido |
| `crate_stop_wood_01` (0,12 s) | não existe | o empurrão desaparece sem parada |
| `terminal_boot_01` | existe, **não toca** | P0 e listado em "lista mínima para protótipo" (§28) |
| `ui_select_01` (navegar) | existe como `uiFocus`, **não toca** | menu inteiro mudo |
| `ui_confirm_01` | existe, **não toca** | menu inteiro mudo |
| `ui_error_01` | existe como `uiDenied`, **não toca** | |
| `door_open_light` / `door_close_light` | não existem — só a versão pesada | toda porta soa como portão de doca |
| `door_locked_01` / `door_unlock_01` | não existem | |
| `forklift_engine_idle_loop` | existe, **não toca** | critério 24.2 inteiro não atendido |
| `forklift_reverse_beep_loop` | existe, **não toca** e com ritmo quebrado | |
| `forklift_collision_light/medium` | existe um só (`forkliftImpact`), **não toca** | a bíblia pede três intensidades |
| `forklift_accelerate_01`, `forklift_brake_01` | não existem | |
| `forklift_pickup_crate` / `drop_crate` | não existem | listados na lista mínima (§28) |
| `terminal_access_granted/denied` | não existem | |
| `credits_gain_01` | existe como `creditsTick` (-41,5 dB), **não toca** | |
| `result_screen_open_01` | não existe | |
| `medal_silver_01` | não existe — o catálogo tem 3 medalhas, a bíblia tem 4 | |
| `system_warning_01` / `system_error_01` | não existem | |
| `system_sector_unlocked_01` | não existe | |
| `puzzle_complete_01` (2,50 s) | existe como `puzzleSuccess` de **0,42 s** | 6× curto |

### 4.2 P1 que ainda falta

- **Ambiências de setor 01, 02, 04, 05, 06 e 08** — a bíblia pede seis loops de
  40–60 s. Existem duas de 3,88 s. Este é o item de P1 com maior retorno: destrava
  o pilar 3.4 inteiro.
- **Esteira** — existe, não toca. Setor 04 tem cinco fases.
- **Gerador** (`generator_idle_loop`, `generator_start`, `power_down`, `power_up`)
  — não existe. Setor 05 e a fase 24 dependem dele.
- **`crate_off_target_01`** — caixa saindo do alvo. Feedback negativo essencial em
  Sokoban; hoje é silêncio.
- **`player_push_effort_01`** — esforço ao empurrar carga pesada. Camada humana
  barata que vende o peso.
- **`crate_slide_ice` / `crate_ice_stop` / `step_ice`** — a Câmara Fria tem cinco
  fases e usa exatamente os mesmos sons do Recebimento.
- **Sons de Duda / Robert / Elias** — os blips existem mas não tocam, e a bíblia
  pede rádio ligando/desligando, glitch de sinal e revelação de pista.
- **Loja Oficina N-8** — `shopPurchase` existe e não toca; abrir/fechar loja e
  "créditos insuficientes" não existem.
- **`assisted_run_badge_01`** — o runtime já publica `AssistanceUsed` e ninguém
  escuta.

### 4.3 P2 que vale antecipar

Três itens de P2 rendem mais que metade do P1 restante:

1. **`crate_hit_metal` e `crate_push_metal`.** Um segundo material muda a
   percepção do jogo inteiro — hoje toda carga soa igual, e a bíblia lista
   quatro tipos de caixa. Custo: uma variação da construção de §2.1 com parciais
   mais altas (240 / 1180 / 2650 Hz), decaimento mais longo e passa-baixa aberto.

2. **`amb_ice_crack`, `amb_pipe_knock`, `amb_system_glitch`, `amb_tool_drop`** —
   *one-shots* esparsos de ambiente, disparados a cada 15–40 s com posição
   aleatória. É o truque mais barato que existe para fazer uma ambiência curta
   parecer longa, e ataca diretamente o problema de fadiga do §1.3. Quatro
   arquivos de 1 s resolvem mais que dobrar a duração dos loops.

3. **`reverb por ambiente` (formalmente P3).** Depende do AudioMixer que já
   precisa existir por outros motivos. Um `SFX Reverb Zone` por setor —
   Câmara Fria com *decay* 2,2 s, Manutenção Pesada com 1,4 s e mais grave,
   Rotas Fantasma com 3,5 s — dá identidade de setor a custo zero de arquivos.

---

## 5. Quando o som toca

### 5.1 Bug crítico: som duplicado em 27 das 36 cenas de fase

`TW08AudioSceneUpgrade.AttachPuzzle` (linha 97) adiciona `PuzzleAudioFeedback` ao
GameObject do `PuzzleRuntime`. `TW08PuzzleSceneBuilder` (linha 716) cria um
`PuzzleAudioDirector`. Os dois assinam `MoveApplied`, `LevelCompleted` e
`StaticDeadlockDetected`.

Verificação por GUID nos arquivos `.unity`:

```
PuzzleAudioFeedback (485d2e3f…)   27 cenas
PuzzleAudioDirector (cba7ebf0…)   36 cenas
AMBOS                             27 cenas
```

Nessas 27 fases (toda a `VerticalSlice`), cada passo dispara `PuzzleStep` duas
vezes, com clipe e pitch aleatórios independentes — o que soa como *flanging* /
eco curto, não como um passo. Cada vitória toca dois stingers. Cada deadlock toca
dois alarmes.

E há um efeito pior. O `Director` escolhe entre `CratePushHeavy` e `PuzzlePush`
conforme o peso; o `Feedback` toca sempre `PuzzlePush`. Então empurrar uma carga
pesada dispara **pesada + comum simultaneamente** — a distinção de peso que o
pilar 3.2 exige fica borrada em "o mesmo som, só que mais alto".

**Correção:** `PuzzleAudioFeedback` foi substituído pelo `Director` e deve ser
removido das cenas (e o `AttachPuzzle` do `TW08AudioSceneUpgrade` deve parar de
adicioná-lo). É a correção de maior retorno por linha de código do projeto.

### 5.2 Eventos importantes em silêncio

**Movimento bloqueado é o buraco mais grave.** `PuzzleRuntime.TryMove` retorna
`false` sem publicar evento algum:

```csharp
if (Board == null || Board.IsComplete || !Board.TryMove(direction, out PuzzleMove move))
{
    return false;
}
```

O jogador aperta a seta contra uma parede e **o jogo não responde de forma
alguma**. A bíblia lista `puzzle_invalid_move_01` e `player_blocked_01` como P0,
e o critério de aceite 24.1 exige "movimento inválido tem som curto e não
irritante". O `uiDenied` já existe, calibrado, e não é usado em lugar nenhum.

Correção: `public event Action MoveBlocked;` no runtime, disparado no `return
false`, e no diretor:

```csharp
private void OnMoveBlocked()
{
    // Curto e não irritante (bíblia §24.1): um "toc" seco, nunca um bipe de erro
    // de menu. O jogador esbarra dezenas de vezes por fase e o som não pode
    // acusar erro — só confirmar que a entrada foi recebida.
    Play(catalog.MoveBlocked);
}
```

O som deve ser **diegético e discreto** (-20 dB, 90 ms, ruído seco em 200–800 Hz),
não o `uiDenied` de menu: um bipe de recusa dezenas de vezes por fase é
exatamente o "irritante" que o critério proíbe.

Outros eventos mudos, por sistema:

- **Menus:** `uiFocus`, `uiConfirm`, `uiDenied` e `terminalBoot` não têm call site
  em lugar nenhum. Navegar e confirmar no menu é silêncio.
- **Loja:** `shopPurchase` e `creditsTick` não tocam. Critério 24.3 não atendido.
- **Narrativa:** os três blips não tocam. `TW08AudioCatalog.VoiceFor()` existe,
  está correto e **nunca é chamado**. Critério 24.5 não atendido.
- **Empilhadeira:** `forkliftEngine`, `forkliftReverse` e `forkliftImpact` não
  tocam. O `RaceAudioFeedback` só liga contagem regressiva e chegada. Critério
  24.2 inteiro não atendido.
- **Esteira e alarme de lockdown:** não tocam. Note que o `AUDIO_IMPLEMENTATION.md`
  afirma "alarme de travamento toca uma vez por travamento", mas `OnDeadlock` toca
  `catalog.PuzzleError` — o som do protótipo, não o `lockdownAlarm`. Ou a
  documentação está errada ou o mapeamento está.
- **`AssistanceUsed`:** o runtime publica, ninguém escuta.
- **Caixa saindo do alvo:** não há evento nem som.

Contagem: **13 dos 39 eventos do catálogo não têm nenhum call site.**

### 5.3 Som demais e som simultâneo

- **`OnSwitchChanged` toca dois sons por comutação** (sensor + porta) no mesmo
  frame. A relação está semanticamente certa — o sensor causa a porta — mas
  precisa de **escalonamento temporal** para o cérebro ler causa e efeito em vez
  de um evento só. Sensor imediato, porta com 90–120 ms de atraso. E se uma fase
  comuta três grupos no mesmo movimento, são seis sons num frame, somados
  coerentemente.

- **`OnCompleted` toca sucesso e medalha juntos.** Devem ser sequenciados: o
  stinger de fase concluída, e a medalha ~600 ms depois, quando o painel de
  resultado aparecer.

- **Passo em cada movimento sem empurrão.** Em Sokoban o jogador faz longas
  sequências de reposicionamento. Com 13 ms audíveis e -23 dB é discreto demais
  hoje, mas depois da correção de §2.8 (130 ms) convém pisar em `-18 dB` e
  **variar a duração** entre as três amostras (110/128/141 ms), senão vira
  metrônomo.

- **Empurrão + impacto na mesma jogada.** Está certo tocar os dois, mas os níveis
  estão invertidos (ver §3.2). Depois de recalibrar, o impacto fica 3 dB acima do
  empurrão.

### 5.4 O som de "voltar" ao desfazer — **intenção certa, execução errada**

A decisão de não reusar o som de empurrar está **correta**: o jogador precisa
distinguir "avancei" de "voltei" sem olhar o contador, e reusar o mesmo som
destruiria essa leitura. O comentário no código está certo.

Mas a execução tem três problemas:

1. **`uiBack` sai a -27,4 dB**, 16 dB abaixo do empurrão. O feedback de "voltei" é
   quase inaudível, quando deveria ser tão legível quanto o de "avancei". Um
   desfazer que não se ouve é pior do que um desfazer com o som errado.

2. **`OnRestarted` toca o mesmo `uiBack`.** Desfazer um movimento e jogar a fase
   inteira fora soam **idênticos**. São ações de peso completamente diferente e
   precisam de sons diferentes — reiniciar é mais próximo de um `power_down` do
   que de um "voltar".

3. **É um som de UI numa ação do tabuleiro.** O jogador aprende "esse bipe =
   menu". Desfazer acontece no mundo: a caixa volta, o operador volta. Emprestar
   um som de interface quebra a separação diegética que o resto do banco respeita.
   E hoje desfazer um passo e desfazer um empurrão de carga pesada tocam
   exatamente o mesmo som.

**Proposta — manter a decisão, trocar o som:**

```csharp
private void OnMoveUndone(PuzzleMove move)
{
    // Desfazer continua sem reusar o som de empurrar — mas o gesto acontece no
    // tabuleiro, não no menu. A varredura invertida (grave -> agudo) e o ataque
    // longo dão a leitura de "sendo puxado de volta" sem sair do mundo do jogo.
    Play(move.CrateMoved ? catalog.CrateUndo : catalog.StepUndo);
}
```

```csharp
// Arrasto invertido: o mesmo material do empurrão, com a varredura ao contrário
// (88 -> 104 Hz) e ataque de 10% da duração. É o que o ouvido lê como "rebobinou".
events["crateUndo"] = SingleEvent(
    "Crate_Undo",
    "crate_push_reverse_01",
    TW08AudioSynth.Mix(
        TW08AudioSynth.Noise(0.26f, 0.46f, new Env(0.30f, 0.06f, 0.80f, 0.24f), "crate_undo", 0.40f),
        TW08AudioSynth.Tone(0.26f, 88f, 104f, 0.28f, new Env(0.30f, 0.06f, 0.80f, 0.24f))),
    volume: 0.62f, pitchMin: 0.98f, pitchMax: 1.02f);

// Passo invertido: versão curta, para desfazer um movimento sem carga.
events["stepUndo"] = SingleEvent(
    "Step_Undo",
    "step_reverse_01",
    TW08AudioSynth.Mix(
        TW08AudioSynth.Noise(0.11f, 0.36f, new Env(0.35f, 0.08f, 0.75f, 0.22f), "step_undo", 0.34f),
        TW08AudioSynth.Tone(0.11f, 112f, 132f, 0.22f, new Env(0.35f, 0.08f, 0.75f, 0.22f))),
    volume: 0.48f, pitchMin: 0.97f, pitchMax: 1.03f);
```

E para reiniciar, um som próprio:

```csharp
private void OnRestarted()
{
    OnInitialized();
    // Reiniciar não é desfazer: descarta a fase inteira. Merece um som de
    // desligamento, não o mesmo clique de voltar um movimento.
    Play(catalog.LevelReset);
}
```

```csharp
// Reset: queda de energia. Grave descendente com o ruído fechando junto —
// o oposto exato do terminal ligando.
events["levelReset"] = SingleEvent(
    "Level_Reset",
    "system_power_down_01",
    TW08AudioSynth.Mix(
        TW08AudioSynth.Tone(0.70f, 220f, 62f, 0.40f, new Env(0.03f, 0.10f, 0.70f, 0.55f), harmonics: 0.14f),
        TW08AudioSynth.Noise(0.70f, 0.26f, new Env(0.03f, 0.10f, 0.65f, 0.55f), "power_down", 0.30f)),
    volume: 0.52f, pitchMin: 1f, pitchMax: 1f);
```

**Sobre refazer:** `MoveRedone` está mapeado para `OnMoveApplied`, então refazer
soa idêntico a um movimento novo. Isso é aceitável e até defensável (refazer *é*
avançar), mas fica assimétrico com o desfazer, que tem som próprio. Se o custo
for baixo, um `pitchMin/Max` levemente acima (1,04–1,06) no redo dá a distinção
sem exigir arquivo novo.

---

## 6. Trilha

### 6.1 Estado das três faixas procedurais

| Faixa | Dur. | Fundamental | BPM | Pico | Centroide | Salto na costura |
|---|---:|---:|---:|---:|---:|---:|
| `music_menu` | 8,00 s | 55 Hz | 72 | -17,1 dBFS | 76 Hz | **8,8% do pico** |
| `music_puzzle` | 8,00 s | 44 Hz | 60 | -18,7 dBFS | 60 Hz | **8,7%** |
| `music_race` | 8,00 s | 110 Hz | 140 | -16,7 dBFS | 126 Hz | **9,7%** |

Cada uma é fundamental + quinta (×0,35) + oitava (×0,18) + quadrada em sub-oitava
(×0,08), com um pulso de amplitude no andamento. Não há melodia, não há
percussão, não há nenhuma das características que a bíblia pede (chiptune,
arpejo, percussão metálica, pads).

**Dois problemas que valem correção mesmo sendo espaço reservado:**

1. **A costura clica.** As músicas não passam por `MakeSeamless` (só o banco de
   SFX passa) — 8,7 a 9,7% de descontinuidade contra 0,2–2,2% dos loops de SFX.
   Um clique audível a cada 8 segundos durante um puzzle é ativamente pior do que
   silêncio para concentração.

2. **`music_puzzle` tem fundamental em 44 Hz.** Está uma oitava e meia abaixo do
   que um laptop reproduz. Em prática o jogador ouve uma pulsação sem nota.

**Correção provisória de duas linhas:** aplicar `MakeSeamless` nas três e subir a
duração de 8 s para 32 s (quatro variações do ciclo harmônico em vez de uma). Ou,
se isso não couber agora, **desligar a música de puzzle por padrão** até haver
faixa definitiva — silêncio com ambiência boa é melhor que um loop de 8 s com
clique.

### 6.2 Sistema: música em camadas, não faixa única

A trilha definitiva deve ser produzida em **stems**, não em faixas monolíticas.
Quatro camadas por faixa, todas com exatamente o mesmo número de amostras, mesma
grade de tempo e mesmo ponto de emenda:

| Stem | Conteúdo | Nível de referência | Quando toca |
|---|---|---:|---|
| `bed` | pad/drone + hum do setor | -22 LUFS | sempre |
| `pulse` | baixo + kick + percussão | -18 LUFS | a partir do primeiro movimento |
| `motif` | melodia chiptune | -19 LUFS | fase já resolvida, ou última caixa faltando |
| `tension` | arpejo + alarme filtrado | -20 LUFS | risco de deadlock, ou fase de lockdown |

O `MusicService` atual já faz crossfade com duas fontes; precisa evoluir para N
fontes **sincronizadas por `AudioSettings.dspTime`** com ganho independente por
stem. Sem sincronia por dsp o *phasing* entre stems é audível em segundos.

### 6.3 Reação ao estado do jogo

Este é o ponto onde a trilha deixa de ser decoração e vira design:

| Gatilho | Reação | Tempo |
|---|---|---|
| Entrada na fase | só `bed` | — |
| Primeiro movimento do jogador | `pulse` entra | fade 1,5 s |
| **Ocioso > 20 s** | `pulse` cai -8 dB, `bed` sobe +2 dB | fade 3,0 s |
| Movimento retomado | `pulse` volta | fade 1,2 s |
| Falta uma caixa para o alvo final | `motif` entra | fade 2,0 s |
| `StaticDeadlockDetected` | `tension` entra, `pulse` sai | fade 0,8 s |
| `LevelCompleted` | tudo cai, stinger toca, `bed` volta na tela de resultado | corte 250 ms |
| Undo / Redo | **nada muda** | — |

A linha do ocioso é a mais importante. Num puzzle, o jogador passa metade do
tempo parado pensando, e é exatamente nesse momento que uma trilha rítmica vira
pressão indesejada. Fazer a música **recuar quando o jogador para de agir** é o
que separa uma trilha de puzzle de uma trilha genérica. É também a mitigação real
do "trilha que atrapalhe raciocínio" que o documento de trilha lista em "Evitar".

Undo não deve pontuar nada: se cada desfazer mexesse na música, uma sequência de
dez undos viraria uma montanha-russa.

### 6.4 Instrumentação, andamento e tonalidade por setor

Todas em 4/4, exceto onde indicado. Duração 1:30–2:00 (documento de trilha §11.2),
sem vocal (§11.3).

| Contexto | Faixa | BPM | Tom | `bed` | `pulse` | `motif` / identidade |
|---|---|---:|---|---|---|---|
| Menu | *Turno Iniciado* | 72 | Lá menor | pad analógico lento + hum de 60 Hz | kick suave a cada 2 tempos | triangular com eco de fita, melancólica |
| S01 Recebimento | *Primeiro Turno* | 88 | Ré menor | pad claro, filtro aberto | dente-de-serra filtrado, bumbo simples | bipes de empilhadeira ao longe como percussão melódica |
| S02 Expedição | *Doca B-12* | 104 | Sol menor | pad médio com movimento | baixo sincopado, kick em 1 e 3 | chapa e palete metálicos marcando 2 e 4 |
| Oficina N-8 | *Velho Motor* | 92 | Fá maior | órgão analógico quente | baixo redondo, groove leve | acordeão/órgão brincalhão, chave de fenda como *shaker* |
| S03 Câmara Fria | *Câmara 08-C* | 68 | Si menor | pad cristalino, *reverb* 3 s | pulso lento, sem kick pesado | glockenspiel digital, respiração de ventilação |
| S04 Automação | *Rota Automática* | 120 | Mi menor (frígio) | drone estático | sequência travada de 16 semicolcheias | *bit-crush*, repetição de buffer, esteira como *hi-hat* |
| S05 Manutenção | *Peso Morto* | 84 | Dó menor | drone grave distorcido | baixo pesado, kick em cada tempo | batidas de metal em contratempo, gerador |
| S06 Rotas Fantasma | *Mapa Incompleto* | 60 (5/4) | Lá menor | drone com convolução longa | quase ausente, só um pulso | melodia **com notas faltando**, alarme distante filtrado |
| S08 Núcleo | *Núcleo Logístico* | 132 | Ré menor | pad tenso em movimento | percussão cinemática + kick | arpejo de 8 notas subindo, lead em oitavas |
| Corrida padrão | *Forklift Shift Race* | 150 | Mi menor | pad mínimo | baixo chiptune em colcheias | motor como base rítmica, freios como percussão |
| Corrida fria | *Câmara Fria Pro* | 144 | Si menor | pads gelados | idem | derrapagem como transiente |
| Corrida lockdown | *Lockdown N-8* | 160 | Dó menor | drone de alarme | idem + pulso de alarme em cada tempo | contagem regressiva integrada ao ritmo |

O compasso 5/4 nas Rotas Fantasma é deliberado: o setor é sobre mapas
incompletos, e um compasso que "falta um tempo" comunica isso antes de qualquer
diálogo. É a aplicação mais direta do pilar de narrativa sonora à trilha.

### 6.5 Regra de tonalidade compartilhada entre SFX e trilha

Hoje os SFX afinados estão espalhados: `crateOnGoal` em Dó5/Sol5, medalhas em
Sol4/Dó5/Mi5/Sol5/Dó6, sensor em **1180 Hz** (≈Ré6, fora de qualquer escala),
`toolMarker` terminando em **1100 Hz** (≈Dó♯6). Com faixas em Lá menor, Ré menor,
Sol menor, Mi menor, Si menor, Dó menor e Fá maior, esses SFX vão brigar.

**Regra:** todo SFX melódico usa a **pentatônica de Dó (C-D-E-G-A)** em qualquer
oitava. É consonante contra os sete tons acima.

```
C4 261,6   D4 293,7   E4 329,6   G4 392,0   A4 440,0
C5 523,3   D5 587,3   E5 659,3   G5 784,0   A5 880,0
C6 1046,5  D6 1174,7  E6 1318,5  G6 1568,0  A6 1760,0
```

Correções pontuais:

- `sensorOn`: 1180 Hz → **1046,5 Hz** (Dó6)
- `sensorOff`: 620 Hz → **784 Hz** (Sol5) — mantém a leitura "desce" contra o Dó6
- `toolMarker`: 440 → 1100 Hz vira 440 → **1046,5 Hz**
- `toolRewind`: 880 → 220 Hz já está na escala (Lá5 → Lá3) — manter
- `creditsTick`: 1480 Hz → **1568 Hz** (Sol6)
- `terminalBoot`: 440/660/880/1320 Hz vira **440 / 659,3 / 880 / 1318,5**
- `shopPurchase`: 880 → 1318 Hz vira **880 → 1318,5** (Lá5 → Mi6, sexta maior)

### 6.6 Motivos de personagem

O documento de trilha (§11.4) define quatro identidades. Em vez de quatro faixas
completas, o retorno maior vem de **quatro motivos de quatro notas**, usados como
camada `motif` nas fases do personagem e como stinger de 2 s nos diálogos — e
como base dos blips de fala do §2.9.

| Personagem | Motivo | Timbre |
|---|---|---|
| John Miller | Dó5 – Sol4 – Mi4 – Dó4 (descendente, resolvido) | synth quente, baixo constante |
| Duda | Mi5 – Sol5 – Lá5 – Dó6 (ascendente, aberto) | digital delicado, *glitch* na última nota |
| Robert | Dó3 – Dó3 – Sol3 (grave, teimoso) | baixo pesado, percussão de oficina |
| Elias | Lá4 – (pausa) – Mi4 – (pausa) – Ré♭4 | notas espaçadas, ruído digital, **sem resolver** |

O Ré♭ do Elias está fora da pentatônica de propósito: é a única exceção, e é o
que faz o tema dele soar "errado" contra tudo o mais no jogo — que é exatamente a
função narrativa dele.

### 6.7 Produção e formato

- **Formato:** OGG Vorbis q6 para música e loops longos, WAV 44,1 kHz/16 bits para
  SFX (bíblia §6.1). Hoje tudo é WAV 22,05 kHz.
- **Stems:** mesmo comprimento em amostras, exatamente. Um stem com uma amostra a
  mais desalinha o loop progressivamente.
- **Loop:** ponto de emenda no *downbeat*, com um compasso de crossfade. Nunca
  final dramático (documento de trilha §11.1).
- **Jingles** (§6.1–6.3 do documento de trilha): vitória 8–12 s, medalha ouro/
  platina 8 s, falha 5–8 s. Hoje: 0,42 / 0,98 / 0,25 s. Devem ser `MusicTrack`
  tocados pelo `MusicService` com *duck* da faixa de fase — não `AudioEvent` no
  bus de SFX, senão empilham com o SFX de gameplay.
- **Prompts:** os prompts do documento de trilha estão prontos e são bons.
  A recomendação de produção é gerar 3 variações por faixa, escolher uma, e depois
  **re-produzir os stems separadamente** a partir da faixa escolhida — ferramenta
  generativa não entrega stems, então a camada `pulse` e a `tension` precisam ser
  reconstruídas em DAW por cima do resultado.

---

## 7. Plano priorizado

### Bloco 1 — correções de alto impacto e baixo custo

1. Remover `PuzzleAudioFeedback` das 27 cenas e do `TW08AudioSceneUpgrade`
   (§5.1). Elimina duplicação e restaura a distinção de peso.
2. Corrigir `Envelope.Evaluate` + adicionar `Envelope.Impact` e
   `Envelope.Looping`; migrar todo `Env.Percussive` para `Env.Impact` e todo
   `Env.Sustained` de loop para `Env.Looping` (§2.0). Desbloqueia 20 arquivos.
3. Centrar a onda quadrada (§2.4). Recupera 3–6 dB e mata o thump de sub-grave.
4. Aplicar a tabela de volumes recalibrados (§3.4, nível 1).
5. Subir `SampleRate` para 44100 (§2.11).
6. Aplicar `MakeSeamless` nas três músicas e subir de 8 s para 32 s (§6.1).

### Bloco 2 — o que o jogador percebe imediatamente

7. Evento `MoveBlocked` no runtime + som diegético de bloqueio (§5.2). **P0 da
   bíblia, hoje silêncio total.**
8. Ligar menu, loja, terminal e narrativa aos eventos que já existem (§5.2).
   13 eventos deixam de ser código morto.
9. Reconstruir impacto de madeira, empurrão, carga pesada e passo (§2.1–2.3, 2.8).
10. Sons próprios para desfazer e reiniciar (§5.4).
11. Escalonar sensor→porta em 90–120 ms e sucesso→medalha em 600 ms (§5.3).
12. Ambiências de setor de 30–45 s, com *one-shots* esparsos (§4.3 item 2).

### Bloco 3 — estrutura

13. Criar o `AudioMixer` com os nove buses, limitador e *ducking* (§3.3).
14. `NormalizeLoudness` + alvos nomeados da bíblia (§3.4, nível 2).
15. `GetRandomClip` sem repetição imediata (§1.3).
16. Alarme de lockdown de 10 s e bip de ré com ritmo correto (§2.5–2.6).
17. Portas com articulação de três partes e 1,8 s (§2.7).

### Bloco 4 — trilha e campanha

18. Sistema de stems sincronizados por `dspTime` e reação ao estado (§6.2–6.3).
19. Produzir as faixas definitivas conforme §6.4, com a regra de tonalidade §6.5.
20. Motivos de personagem e blips derivados deles (§6.6, §2.9).
21. Segundo material de caixa (metal) e sons de setor faltantes (§4.2–4.3).

---

## 8. Critérios de aceite — status atual

Contra §24 da bíblia, o que passa e o que não passa hoje:

| Critério | Status |
|---|---|
| Passos tocam sem atrasar movimento | **passa** |
| Empurrar caixa toca uma vez por empurrão | **falha** — toca duas vezes em 27 cenas |
| Caixa no alvo tem feedback claro | **falha** — 5 dB abaixo do alvo e mascarado |
| Movimento inválido tem som curto e não irritante | **falha** — não existe |
| Fase concluída tem stinger satisfatório | **falha** — 0,42 s contra 2,50 s, e esmagado pela medalha |
| Motor idle toca em loop | **falha** — não toca |
| Motor muda de intensidade conforme velocidade | **falha** |
| Bip de ré toca apenas em ré | **falha** — não toca, e o arquivo tem ritmo quebrado |
| Colisão leve/média/forte tem sons diferentes | **falha** — existe uma só e não toca |
| Pegar e soltar carga são claros | **falha** — não existem |
| Botões têm som curto | **falha** — existem e não tocam |
| Erro de UI não é agressivo demais | **passa** (quando tocar) |
| Compra na loja tem feedback satisfatório | **falha** — existe e não toca |
| Medalhas têm identidade diferente | **passa** — a escada melódica está certa |
| Créditos recebidos são audíveis | **falha** — -41,5 dB e não toca |
| Cada setor tem ambiente próprio | **falha** — 2 de 7, e as duas são a mesma textura |
| Ambiente não cobre SFX importantes | **falha** — no teto da faixa e na mesma banda das caixas |
| Loops não têm corte perceptível | **passa** nos SFX (0,2–2,2%), **falha** na música (8,7–9,7%) |
| Setor 08 soa mais tenso que os anteriores | **falha** — não existe |
| Câmara Fria soa diferente da Manutenção Pesada | **falha** — Manutenção usa a ambiência genérica |
| Sons de Duda / Robert / Elias | **falha** — dois existem e não tocam, Elias não existe |
| Glitches não atrapalham leitura de texto | não aplicável ainda |

**4 de 22.** Depois dos Blocos 1 e 2 acima, 14 de 22 passam sem produção de áudio
nova além do que já está descrito.

---

## 9. Nota sobre o método

Todo som deste banco é sintetizado por código porque o projeto é clean-room. Isso
é uma restrição legítima e a implementação a respeita bem. O que este documento
não faz é fingir que síntese procedural substitui gravação: ela não substitui.
As correções acima levam o banco de "audível" para "legível e calibrado", que é
o teto realista desta abordagem.

O caminho para o teto seguinte é foley próprio — gravar uma caixa de madeira
sendo arrastada num piso de concreto rende, em uma tarde, mais do que qualquer
refinamento de síntese. A arquitetura já está pronta para isso: os `AudioEvent`
referenciam clipes por asset, e trocar `crate_push_wood_01.wav` por uma gravação
não exige mudar uma linha de `PuzzleAudioDirector`. Essa foi a decisão de
arquitetura mais acertada do trabalho até aqui, e vale protegê-la.
