# The Warehouse Nº 08 — Direção Técnica e de Game Design

**Status:** baseline de produção para o vertical slice
**Data:** 2026-08-11
**Engine:** Unity 6.3 LTS (`6000.3.0f1`)
**Linguagem:** C# (baseline compatível com C# 9 do Unity 6)

## 1. Decisão de produção

Unity 6.3 LTS + C# passa a ser a linha-base de produção. O histórico Godot permanece apenas como audit trail.

O objetivo imediato não é implementar toda a design bible. É provar um vertical slice pequeno, original, reproduzível e testável antes de liberar racing, power-ups, narrativa complexa e conteúdo de campanha.

## 2. Fontes técnicas consultadas

- Unity Manual — C# Compiler / Roslyn / C# 9:
  https://docs.unity3d.com/6000.0/Documentation/Manual/csharp-compiler.html
- Unity Manual — Input System 1.17.0 para Unity 6:
  https://docs.unity3d.com/6000.0/Manual/com.unity.inputsystem.html
- Unity Manual — 2D Pixel Perfect:
  https://docs.unity3d.com/6000.0/Manual/com.unity.2d.pixel-perfect.html
- Unity Manual — Sprite Atlas:
  https://docs.unity3d.com/6000.0/Manual/sprite/atlas/atlas-landing.html

## 3. Princípios de arquitetura

1. **Core lógico determinístico primeiro.** O puzzle não depende de física 2D para decidir movimentos.
2. **Visual é projeção do estado.** Sprites, animação, câmera e áudio reagem ao `PuzzleBoardModel`; não definem regras.
3. **Input é uma borda.** Teclado, gamepad e UI convertem intenção em comandos do runtime.
4. **Dados de fase são separados da cena.** `PuzzleLevelDefinition` continua sendo a fonte tipada dentro do Unity; export JSON pode ser adicionado depois para revisão/portabilidade.
5. **Uma feature só entra após teste.** Racing/narrativa/power-ups ficam fora do primeiro gate.
6. **Sem conteúdo contaminado.** Nenhum mapa, sprite, som, texto ou dado extraído de Shove It! entra no produto.

## 4. Pipeline retro/pixel-art

### Importação

- Sprite import com `FilterMode.Point`.
- Sem mipmaps para sprites 2D de pixel-art.
- Compressão desativada enquanto a direção visual está sendo validada.
- `Pixels Per Unit` deve ser padronizado quando o tamanho final do tile for fechado.

### Câmera

Adicionar `com.unity.2d.pixel-perfect` após a primeira compilação limpa do vertical slice e configurar `PixelPerfectCamera` com resolução interna de referência.

Baseline proposto para prototipagem visual:

- resolução lógica: **320 × 180** ou **640 × 360**;
- apresentação: escala inteira para 1280×720 / 1920×1080 quando possível;
- movimento do puzzle permanece em coordenadas inteiras de grid;
- animações de transição não podem deixar sprites repousarem em subpixel.

A escolha final 320×180 vs 640×360 depende do tamanho dos sprites autorais e legibilidade do HUD.

### Atlas

Quando os sprites finais começarem a substituir placeholders, agrupar por domínio:

- `Atlas_Warehouse_Environment`
- `Atlas_Characters`
- `Atlas_Crates`
- `Atlas_UI`
- `Atlas_Forklifts` (fase posterior)

Não criar um atlas gigante global.

## 5. Direção visual

O estilo deve parecer um sistema industrial retro-futurista, não uma imitação direta de console/jogo existente.

### Linguagem visual

- carvão/preto azulado para fundo;
- verde de terminal como cor operacional;
- âmbar para aviso/ação crítica;
- vermelho apenas para falha/perigo;
- volumes simples e silhuetas claras;
- detalhes industriais: faixas de segurança, etiquetas, LEDs, placas de doca, números de setor;
- pixel-art com leitura primeiro, textura depois.

### Regra de legibilidade

Cada tile precisa comunicar função antes de decoração. Jogador, caixa, parede, alvo, sensor e perigo devem continuar distinguíveis sem texto e sem depender apenas de cor.

## 6. UX/UI

### Menu principal

Hierarquia:

1. marca `THE WAREHOUSE Nº 08`;
2. estado operacional do terminal;
3. ação principal `INICIAR NOVO TURNO`;
4. continuar apenas quando save/progress estiver validado;
5. opções após termos configurações reais;
6. sair.

Não exibir botão funcional para sistema inexistente.

### HUD de puzzle

Mostrar somente:

- nome da rota/fase;
- movimentos;
- disponibilidade de Undo/Redo;
- status operacional;
- dica contextual curta;
- botões de Undo/Redo/Restart para mouse/touch, sem substituir atalhos.

Feedback de deadlock deve orientar `Undo`, não punir com tela de game over.

## 7. Level design — método

Cada fase deve ter uma **tese**.

Exemplos:

- Fase 01: `empurrar e concluir`;
- Fase 02: `não posso puxar; preciso planejar espaço`;
- Fase 03: `ordem de movimentação importa`.

Uma fase inicial não deve introduzir duas regras novas ao mesmo tempo.

### Estrutura de ensino

1. **Apresentar:** situação segura que demonstra a regra.
2. **Confirmar:** pequena decisão usando a regra.
3. **Testar:** consequência visível para decisão errada.
4. **Combinar:** somente em fase posterior.

### Regras contra dificuldade artificial

- não esconder informação necessária;
- não exigir tentativa e erro por falta de feedback;
- não usar corredor/canto mortal sem antes ensinar deadlock;
- evitar excesso de caixas como substituto de design;
- medir solução ótima ou uma referência próxima antes de definir medalhas;
- registrar solução conhecida para toda fase publicada.

## 8. Primeiras três fases do vertical slice

As três salas abaixo são originais e foram verificadas com busca BFS externa ao runtime antes de serem codificadas no gerador.

| Fase | Tese | Solução mínima encontrada | Gold | Platinum |
|---|---|---:|---:|---:|
| Primeiro Turno | empurrar / objetivo | 3 | 5 | 3 |
| Corredor Apertado | espaço e irreversibilidade | 12 | 16 | 12 |
| Carga Cruzada | ordem e preparação | 31 | 40 | 31 |

Esses valores ainda precisam ser confirmados por playtest humano. Solver ótimo não equivale a dificuldade percebida.

## 9. Gate do vertical slice

O vertical slice só é considerado aprovado quando:

- projeto abre sem erro na versão definida do Unity;
- packages resolvem sem erro;
- EditMode tests passam;
- PlayMode smoke tests passam;
- `Tools > TW08 > Create Professional Vertical Slice` gera todas as cenas;
- menu funciona com teclado, gamepad e mouse;
- três fases podem ser concluídas;
- Undo/Redo/Restart funcionam em todas;
- `Undo -> Redo` do movimento vencedor dispara conclusão corretamente;
- não existem `Missing Script`, referências nulas críticas ou exceptions no Console;
- build Windows development inicia pelo menu;
- frame pacing e input não apresentam comportamento perceptivelmente instável.

## 10. Próxima sequência de produção

### Gate A — Compilação

Corrigir qualquer erro de package/API/assembly até Unity compilar limpo.

### Gate B — Puzzle vertical slice

Validar menu, HUD, três fases, save mínimo e progressão 01→02→03.

### Gate C — Pixel-art real

Definir tile size, PPU, resolução lógica, Pixel Perfect Camera e primeiro tileset autoral do Setor 01.

### Gate D — UX/acessibilidade

Remapeamento, volume, fullscreen, escala de UI, contraste e alternativa para sinais dependentes de cor.

### Gate E — Conteúdo

Expandir Setor 01 para cinco fases somente após os gates anteriores.

### Depois

Racing, Oficina N-8, narrativa sistêmica e Power Ups retornam ao roadmap somente após o puzzle loop provar qualidade e estabilidade.

## 11. Dívidas conhecidas nesta branch

- `Packages/packages-lock.json` será re-resolvido pelo Unity após adicionar uGUI.
- Pixel Perfect package ainda não foi adicionado de propósito; primeiro queremos compilação limpa da fatia atual.
- cenas são geradas por ferramenta de Editor, portanto ainda não existem no Git até executar o menu no Unity e revisar o resultado.
- não existe validação runtime neste ambiente; toda validação atual é estática.
