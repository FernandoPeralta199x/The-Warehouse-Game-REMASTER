# ADR-005: Estratégia de IP (Unity) — sucessor espiritual original

**Status:** Accepted
**Date:** 2026-07-26
**Deciders:** fernando.augusto.peralta@gmail.com
**Supersedes:** ADR-003 (godot-legacy) — em especial elimina a "Camada 3
(BYO-ROM importer)" que violava a regra jurídica central do Workflow doc §3.

## Context

The Warehouse Nº 08 é um **sucessor espiritual original** do gênero
Sokoban, com identidade autoral própria em nomes, personagens, narrativa,
arte, música, SFX, fases e código. Referência normativa:
[REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md](../REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md)
§3 (Regra jurídica central) e §4 (Zonas de trabalho).

## Decision

**Zero dependência de conteúdo de terceiros.** Todo asset, fase, texto e
código do produto final é original ou licenciado explicitamente. Nenhuma
ferramenta que leia ROM/dump é distribuída, incluída em build, ou existe
sequer como opção de menu.

### O que muda vs ADR-003 legacy

| Aspecto | ADR-003 (godot-legacy) | ADR-005 (aqui) |
|---|---|---|
| Camada 1 — arte/audio/fases distribuídas | Original | **Original** (mantém) |
| Camada 2 — tools/ internas para R&D | Permitido | **Permitido, mas fora do repo** |
| Camada 3 — BYO-ROM importer no jogo | Módulo opcional | **REMOVIDO** — proibido pelo Workflow §3 |
| Naming | "Warehouse Remaster" (WIP) | **"The Warehouse Nº 08"** (definitivo) |
| Referência a "Shove It!" no marketing | Vago | **Proibido** |

### Regras práticas

1. **Zero arquivos binários de ROM no repositório.** `.gitignore` bloqueia
   `*.bin`, `*.smd`, `*.gen`, `*.iso`, `*.mdx` etc.
2. **Zero importador BYO-ROM no código.** Não existe menu "Import Original
   Levels". Não existe parser de dump. Se um usuário quiser jogar as fases
   originais, ele usa um emulador com o cartucho dele — não é problema
   nosso.
3. **Zero referências textuais a IPs de terceiros no produto.**
   - Créditos podem dizer *"inspired by classic warehouse puzzles"* mas
     não *"remake of Shove It!"*.
   - Nenhum nome de personagem do original (Stevedore, etc.) aparece.
   - Nenhum layout de fase reproduz 1:1 uma fase do original.
4. **Nenhum agente (Claude, Codex, outros) pode:**
   - Pedir para ler uma ROM.
   - Gerar fases baseadas em layouts do original.
   - Usar senhas, textos, ou strings extraídas.
   - Sugerir "importar do jogo original" como feature.
5. **Estudo histórico privado permitido, mas segregado.** Se o dev quiser
   estudar a ROM para entender o gênero, os arquivos ficam em uma pasta
   externa ao repo (ex: `X:\Shove-It-The-Warehouse\*.bin` no disco pessoal).
   O output dessa análise **não entra** como input de agente nem como
   dado do produto.

### Sobre a análise técnica da ROM já feita (Sprint 0 Godot)

O Sprint 0 do godot-legacy incluiu análise binária das 5 ROMs `[!]`, `[b1]`,
`[h1]`, `[o1]`, `[T+Por]`. Essa análise:

- **Continua permitida como estudo histórico privado.**
- **NÃO deve alimentar** o game design, level design, prompts de arte,
  código de produção ou marketing.
- Os relatórios técnicos daquele sprint estão preservados no branch
  `godot-legacy` como audit trail, mas **não devem ser referenciados**
  em prompts para geração de fases, arte, ou strings do produto.
- Os arquivos `.bin` das ROMs ficam **fora** do repositório (pasta
  irmã `X:\Shove-It-The-Warehouse\` do disco pessoal — não commitáveis).

## Consequences

**Fica mais fácil:**
- Aprovação Steam sem risco de takedown.
- Aceitar contribuições sem viral de licença.
- Marketing honesto sem misrepresentation.
- Auditoria: qualquer arquivo do repo é 100% autoral ou licenciado.

**Fica mais difícil:**
- Não podemos oferecer "as 80 fases clássicas" como feature — usuários
  querendo isso ficam com o emulador+cartucho deles.
- Naming exige criatividade (feito: "The Warehouse Nº 08" + narrativa
  autoral com John Miller/Duda/Big Rob/Elias).

**Torna-se obrigatório:**
- Todo PR passa por lint contra padrões da zona contaminada (grep contra
  `.bin`, `Stevedore`, `Shove It!`, `shove_it_offsets`, etc.).
- Nova feature relacionada a "importar/converter/extrair" precisa de
  aprovação explícita neste ADR (ou emenda) antes de sair de review.
- README/páginas Steam citam inspiração no gênero (Sokoban 1982, Thinking
  Rabbit) mas **nunca** o port Mega Drive específico.

## Action Items

1. [x] `.gitignore` bloqueia binários de ROM (Workflow §9)
2. [x] AGENTS.md documenta a proibição
3. [x] ADR-005 (este arquivo) supersedes ADR-003 legacy
4. [ ] Configurar hook pre-commit que rejeita `.bin`/`.smd`/`.gen`
       (deferred para Sprint 0.1 do Unity)
5. [ ] Auditar `REFERENCIA/` — confirmar que nenhum arquivo lá dentro é
       cópia direta de asset do original (parece OK: são referências
       autorais + docs)
6. [ ] Créditos no README + página Steam alinhados com "inspired by
       Sokoban genre, not Shove It!"
