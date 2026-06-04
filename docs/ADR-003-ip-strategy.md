# ADR-003: Estratégia de Propriedade Intelectual

**Status:** Accepted
**Date:** 2026-06-04
**Deciders:** fernando.augusto.peralta@gmail.com

## Context

O projeto é um remaster pixel-art HD de *Shove It! The Warehouse Game*
(Mega Drive, NCS/Masaya, 1990). Histórico relevante:

- **Sokoban** (倉庫番) foi criado por Hiroyuki Imabayashi para Thinking
  Rabbit em 1982
- A *mecânica* de empurrar caixas em grid não é patenteável (matéria
  excluída) e está em uso livre desde 1982
- A **palavra-marca "Sokoban"** foi marca registrada da Thinking Rabbit/
  Falcon Co. — status atual incerto em 2026
- **Layouts específicos das 80 fases** publicadas pela NCS no port MD são
  protegidos por copyright (obra autoral)
- **Sprites, audio, fontes** do ROM original são copyright NCS/Masaya
- **Sega** detém marca do hardware (irrelevante — não estamos rodando no
  hardware da Sega)

Para um remaster solo distribuível, três áreas de risco:

1. **Nome do produto** — usar "Shove It" ou "Sokoban" no título
2. **Layouts de fases** — recriar 1:1 as 80 originais
3. **Arte/audio** — extrair e modernizar sprites originais

## Decision

**Estratégia em três camadas:**

### Camada 1 — Distribuível (binário público)

- **Título do produto:** novo, neutro. Não usa "Shove It" nem "Sokoban".
  Sugestão de trabalho: *Warehouse Remaster* ou *Shove* (a definir).
  Subtítulo pode reconhecer inspiração: "*inspired by classic
  box-pushing puzzles*". Decisão final em ADR-004 (branding).
- **Fases:** apenas pack original criado pelo dev + packs comunitários
  com licença explícita (CC0/CC-BY).
- **Arte:** 100% original (sprites HD criados do zero).
- **Audio:** 100% original (música nova, SFX próprios).
- **Fonte:** open-source (ex: PixelOperator, Jersey-Mini, ou própria).

### Camada 2 — Ferramentas internas (NÃO distribuídas)

- `tools/extract_levels.py` — decodifica os 80 layouts do ROM original
  para estudo de design (par, dificuldade progressiva, padrões).
  **Output não comitado ao Git.**
- Notas de engenharia reversa em `tools/rom_disassembly/` — uso pessoal,
  fica fora do release.

### Camada 3 — Opt-in do usuário (importador BYO-ROM)

- Implementar importador opcional **dentro do jogo distribuído** que aceita
  um arquivo ROM do *próprio usuário* (que ele legalmente possui) e extrai
  os layouts para a sessão de jogo dele.
- O ROM **nunca é incluído** no instalador.
- Funciona pelo mesmo princípio "dump your own cartridge" usado por
  emuladores (Dolphin, RetroArch, etc.) — legalmente defensável quando o
  software *não* distribui conteúdo proprietário.
- UI clara: "Forneça seu próprio ROM Shove It! para jogar as 80 fases
  originais. Nós não distribuímos esse arquivo."

## Options Considered

### Option A: Estratégia em 3 camadas (escolhida)

| Dimensão | Avaliação |
|----------|-----------|
| Risco legal | Mínimo |
| Esforço técnico | Médio (importador BYO-ROM) |
| Experiência do usuário | Boa (fases originais opt-in, novas no core) |
| Honestidade autoral | Alta (créditos claros) |

### Option B: Tudo original, ignorar fases originais

**Pros:** Zero risco. Mais simples.
**Cons:** Perde a oportunidade de oferecer "as 80 fases que você lembra"
para fãs nostálgicos. Marketing fraco ("é só mais um Sokoban").

### Option C: Distribuir tudo, assumir o risco

**Pros:** Simplicidade máxima.
**Cons:** Em 2026, NCS não existe mais com mesmo nome, mas a IP foi
sucedida (provavelmente Falcom ou subsidiária). Risco real é baixo *na
prática* (cease-and-desist é o pior caso, sem dano material), mas é
tecnicamente infringente e fecha portas pra eventualmente colocar na
Steam (Valve exige declaração de IP).

### Option D: Pedir licença formal

**Pros:** Cobertura legal completa.
**Cons:** NCS não existe; rastrear sucessor leva meses e provavelmente
não responde a indie solo. ROI ruim.

## Trade-off Analysis

A camada 3 (BYO-ROM importer) é a chave: dá ao usuário o que ele quer
(jogar as 80 fases clássicas) sem nós distribuirmos nada protegido. É o
mesmo modelo que tornou emuladores legais. Combina autoria limpa
(camada 1) com fidelidade ao original (camada 3).

## Marcas e atribuições

No README, créditos do jogo e documentação:

> *This game is inspired by Sokoban (倉庫番), originally created by
> Hiroyuki Imabayashi for Thinking Rabbit in 1982, and the Mega Drive
> port published by NCS/Masaya in 1990. This is an unaffiliated
> homage — not a derivative work — built from scratch. "Sokoban" is a
> trademark of its respective owners.*

## Consequences

**Becomes easier:**
- Eventual lançamento Steam/itch.io (nenhuma IP de terceiros distribuída)
- Aceitar contribuições de fases sem risco viral de licença
- Marketing honesto ("homage to a classic")

**Becomes harder:**
- Naming do produto exige criatividade (sem usar marcas conhecidas)
- BYO-ROM importer adiciona scope (mas isolado em módulo opcional)
- Devem ser implementados warnings claros se importação detectar ROM
  modificado/distribuído (não vamos rodar dumps suspeitos)

**Becomes mandatory:**
- `tools/` precisa ficar em `.gitignore` quando contiver outputs do ROM
- README/about screen com créditos honestos a Imabayashi e ao port NCS

## Action Items

1. [ ] `.gitignore`: excluir `tools/extract_levels_output/`, `*.bin`,
   `levels/reference/*.xsb`
2. [ ] Definir título de trabalho (ADR-004 — branding)
3. [ ] Escrever créditos em `docs/CREDITS.md` quando arte/audio chegarem
4. [ ] Importador BYO-ROM como módulo opcional ativado por flag (sprint 4+)
5. [ ] Antes do release público: revisar com leitura de outro humano
   (não é parecer jurídico, mas reduz pontos cegos)
