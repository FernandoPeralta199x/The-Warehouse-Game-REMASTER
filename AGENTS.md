# AGENTS.md — The Warehouse Nº 08

> Contrato operacional obrigatório para todos os agentes (Claude, Codex,
> ferramentas de scaffold, LLMs de suporte) que trabalham neste repositório.
> Base normativa: `REFERENCIA/The_Warehouse_No_08_Claude_Codex_Workflow.md`
> (versão de 2026-07-09).

## Identidade do projeto

The Warehouse Nº 08 é um puzzle game **original** de logística/armazém,
inspirado no gênero Sokoban e em clássicos de puzzle dos anos 90.

Este projeto **NÃO É** remake, port, decompilação, clone de conteúdo,
recriação de mapas ou reimplementação de *Shove It! The Warehouse Game*.

- Nome de produção: **The Warehouse Nº 08**
- Nome internacional: **The Warehouse No. 08**
- Slug: `the-warehouse-no-08`
- Repositório: `The-Warehouse-Game-REMASTER` (nome histórico do repo)

## Regras obrigatórias (proibições absolutas)

1. Não usar ROMs.
2. Não usar dumps binários (`.bin`, `.smd`, `.gen`, `.mdx`, etc).
3. Não usar sprites extraídos do jogo original.
4. Não usar músicas ou sons extraídos.
5. Não usar mapas/fases extraídas.
6. Não usar senhas, textos, nomes internos ou strings do jogo original.
7. Não usar personagem Stevedore.
8. Não usar nome "Shove It!" no produto ou marketing.
9. Não copiar HUD, capa, manual ou identidade visual do original.
10. Não criar scripts de extração para uso no jogo final (BYO-ROM importer
    é **proibido**, contrariamente ao ADR-003 legacy).
11. Não adicionar assets sem licença.
12. Não alterar múltiplos módulos sem justificativa.
13. Não fazer scaffold de larga escala sem primeiro fechar o Sprint atual.

## Permitido

- Implementar mecânica abstrata de puzzle em grade.
- Empurrar caixas (uma por vez, sem puxar, sem empurrar duas).
- Alvos, undo, restart, editor de fases próprio.
- Criar fases originais autorais.
- Criar arte, áudio, música próprios ou com licença clara.
- Criar formato JSON próprio para fases (schema em `docs/`).
- Nomes de personagens originais (John Miller, Duda Rocha, Big Rob, Elias
  — todos criados pelo dev, ver `REFERENCIA/The_Warehouse_N8_Historia_Central.md`).

## Zonas de trabalho

- **Zona limpa (autorizada):** GDD original, mecânicas abstratas, level
  design novo, arte nova, áudio novo, UI própria, código próprio, formato
  JSON próprio, editor próprio, documentação de licenças, playtests,
  builds originais.
- **Zona contaminada (proibida):** ROMs, dumps, offsets, mapas extraídos,
  senhas originais, sprites/sons/músicas/textos originais, screenshots
  diretas do jogo original, strings internas, dados binários convertidos.
- **Isolamento:** se estudo de engenharia reversa histórica existir, deve
  ficar **fora** deste repositório, sem virar input de IA e sem output
  reaproveitado no produto.

## Responsabilidade do Claude (arquiteto/revisor)

- Manter a visão do projeto e o GDD alinhado a `REFERENCIA/`.
- Criar e revisar arquitetura, documentação, plano de produção.
- Quebrar tarefas em issues pequenas para Codex (formato `TW08-XXX`).
- Revisar riscos jurídicos, técnicos, UX e escopo.
- Revisar diffs e apontar riscos antes de aprovar merge.
- **Rejeitar** qualquer conteúdo contaminado, mesmo indiretamente.
- **Não** pedir ROM, ler mapas extraídos, mandar Codex criar extractor,
  ou aceitar commits com arquivos proibidos.

## Responsabilidade do Codex (implementador)

- Implementar **uma tarefa por vez** com critério de aceite claro.
- Criar testes junto com implementação.
- Manter separação **core lógico ↔ camada visual**.
- Validar comportamento por execução, não só por leitura.
- Entregar: diff + arquivos alterados + testes rodados + riscos restantes.
- **Não** sair do escopo, criar extractor, usar dados originais, gerar
  assets sem licença, ou misturar features em um único commit.

## Critério de qualidade universal

Todo commit deve:

- Ter escopo pequeno e reversível.
- Não introduzir arquivos da zona contaminada.
- Passar validação estática (`static_validation.json` quando aplicável).
- Ter mensagem descritiva com `[tipo]: descrição` (chore/feat/fix/docs/refactor).
- Preservar audit trail — usar `git mv` para renames, evitar rewrite de history.

## Conflitos conhecidos entre este AGENTS.md e o estado atual

Ver [docs/ADR-004-engine-pivot-godot-to-unity.md](docs/ADR-004-engine-pivot-godot-to-unity.md).

O Workflow doc de 2026-07-09 recomenda **Godot 4**; o scaffold gerado em
2026-07-26 usa **Unity 6.3 LTS**. Essa contradição está documentada em
ADR-004 e permanece **em aberto** até decisão explícita do owner do projeto.
