# Arquitetura técnica

## Princípios

```text
Dados separados de comportamento.
Modelo de puzzle independente de MonoBehaviour.
ScriptableObjects para configuração estática.
Eventos C# para reduzir acoplamento.
Sistemas pequenos e testáveis.
Nenhum Singleton global para cada recurso.
Serviços persistentes apenas quando justificáveis.
Gameplay não depende diretamente de UI.
```

## Camadas

### Common

Tipos utilitários, validação e componentes genéricos.

### Core

Bootstrap, modos do jogo e carregamento de cenas.

### Input

Mapeia dispositivos para intenções de gameplay. O restante do código não consulta teclas diretamente.

### Puzzle

`PuzzleBoardModel` contém a regra pura. `PuzzleRuntime` conecta modelo, cena e eventos. Isso permite testes sem carregar uma Scene.

### Race

Controlador top-down com `Rigidbody2D`, drift, aderência, checkpoints e progresso de corrida.

### PowerUps

Dados em `PowerUpDefinition`, seleção ponderada por posição e execução desacoplada.

### Audio

`AudioEvent` descreve variações; `AudioService` controla reprodução e pooling.

### Save

Persistência JSON versionada, com arquivo temporário e backup.

## Dependências

```text
Common ← todos
Core ← Input, Audio, Save
Puzzle ← Common, Input
Race ← Common, Input
PowerUps ← Race, Input
UI ← eventos dos sistemas
Editor ← Runtime
Tests ← Runtime
```

## Decisões importantes

### Puzzle lógico separado do visual

O modelo usa coordenadas inteiras. Animação, sprites e Tilemap são apenas apresentação. Isso impede que física e precisão decimal alterem regras do puzzle.

### Corrida física separada do puzzle

A empilhadeira usa `Rigidbody2D`. Não reutilizar a grade Sokoban para corrida; são modos com exigências diferentes.

### Power Ups originais

A distribuição por posição e o princípio de “item para recuperação” pertencem ao gênero arcade. Nomes, arte, efeitos, balanceamento e comportamento deste projeto devem ser próprios.

### ScriptableObjects

Usados para fases, atributos, áudio e Power Ups. Save do jogador continua em JSON; ScriptableObject não é arquivo de save em build.
