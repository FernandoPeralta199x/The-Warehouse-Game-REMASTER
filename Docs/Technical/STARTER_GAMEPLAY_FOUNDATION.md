# Fundação de gameplay

## Puzzle

A base implementa:

- movimento cardinal;
- paredes;
- caixas;
- caixas pesadas como tipo de dado;
- objetivos;
- detecção de conclusão;
- Undo por comando;
- reinício;
- eventos de movimento e conclusão;
- modelo testável sem Scene.

## Evolução prevista

```text
sensores de peso;
portas;
esteiras;
piso de gelo;
caixas frágeis;
caixas marcadas;
ordem de alvos;
robôs por turno;
mapa incompleto;
Power Up Rebobinar;
replay e ghost.
```

## Regra de qualidade

Toda mecânica deve existir primeiro no modelo lógico, depois receber visual, áudio e animação.

## Dados da fase

`PuzzleLevelDefinition` armazena:

```text
ID;
nome;
largura e altura;
posição inicial;
paredes;
objetivos;
caixas;
limites de medalha;
permissão de Power Ups.
```

## Undo

Cada movimento registra:

```text
posição anterior e nova do jogador;
se uma caixa foi movida;
ID da caixa;
posição anterior e nova da caixa.
```

Essa mesma base atende Undo, Rebobinar Movimento, replay e validação de ranking.
