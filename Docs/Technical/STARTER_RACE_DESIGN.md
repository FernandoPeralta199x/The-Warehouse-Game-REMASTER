# Corrida arcade de empilhadeiras

## Direção

O modo corrida usa princípios gerais de kart racing arcade, adaptados para logística industrial:

- controle acessível;
- drift com risco e recompensa;
- mini-boost por execução;
- superfícies com aderência diferente;
- checkpoints sequenciais contra atalhos inválidos;
- Power Ups ponderados pela posição;
- colisão legível;
- velocidade sem perder peso de empilhadeira.

## Identidade original

Não copiar pistas, itens, nomes, personagens, UI, sons, comportamento exato ou identidade visual de franquias conhecidas. O projeto usa equipamentos e riscos do universo N-8.

## Power Ups iniciais

```text
Turbo Compressor: aceleração temporária.
Safety Barrier: absorve uma colisão.
Oil Canister: cria área de baixa aderência.
EMP Signal: reduz temporariamente rivais próximos.
Repair Kit: recupera integridade da empilhadeira.
```

## Catch-up controlado

Jogadores atrás recebem maior chance de ferramentas de recuperação, mas:

- o primeiro colocado ainda recebe itens defensivos;
- não garantir item forte;
- limitar efeitos que removem controle;
- evitar punição artificial ao jogador habilidoso;
- balancear com telemetria.

## Drift

O drift reduz aderência lateral e acumula carga. Ao soltar o botão após carga mínima, o controlador aplica mini-boost. A duração é proporcional ao tempo de drift, com limite definido em `ForkliftStats`.

## Checkpoints

A ordem é obrigatória. A volta só conta após atravessar todos os checkpoints e retornar à linha inicial.
