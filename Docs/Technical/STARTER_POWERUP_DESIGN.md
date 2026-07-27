# Power Ups

## Arquitetura

```text
PowerUpDefinition: dados do item.
WeightedPowerUpTable: distribuição por posição.
PowerUpPickup: coleta e respawn.
PowerUpInventory: armazena um item.
PowerUpExecutor: executa efeito.
ForkliftDamage: integridade e escudo.
```

## Regras

- um item por vez no MVP;
- efeito visual e sonoro obrigatório;
- cooldown ou duração explícita;
- efeitos de controle devem ser curtos;
- evitar aleatoriedade impossível de entender;
- toda distribuição deve aceitar semente determinística para testes.

## Posição normalizada

```text
0.0 = liderança
1.0 = última posição
```

Cada entrada define faixa de posição e peso. O seletor sorteia apenas itens válidos para aquela faixa.
