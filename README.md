# The Warehouse Nº 08 — Unity Professional Base

Base inicial avançada e modular para continuar o desenvolvimento de **The Warehouse Nº 08** com **Unity 6.3 LTS e C#**.

## Conteúdo implementado

- puzzle em grade com caixas, objetivos, Undo, Redo, reinício, validação e detecção de deadlock estático;
- corrida 2D de empilhadeira com aceleração, ré, frenagem, drift, mini-boost, aderência, dano, checkpoints, voltas e IA por waypoints;
- Power Ups originais com inventário, distribuição ponderada e estratégia extensível;
- save JSON atômico, backup, checksum e pipeline de migração;
- áudio com pooling de AudioSources e música com crossfade;
- serviços de bootstrap, registro de dependências, estado global e pausa;
- catálogo de fases, requisitos de desbloqueio e contratos de progressão;
- fluxo de telas e contratos narrativos;
- ferramentas do Editor para criação dos protótipos, importação de pixel art e validação;
- testes Edit Mode e Play Mode;
- 163 SFX procedurais de protótipo claramente identificados como não finais;
- documentação de arquitetura, design, testes, propriedade intelectual e publicação.

## Versão recomendada

Abra com a versão mais recente disponível dentro da família **Unity 6.3 LTS**. O arquivo `ProjectSettings/ProjectVersion.txt` usa `6000.3.0f1` como versão-base do projeto.

## Inicialização

1. Extraia o ZIP em uma pasta de desenvolvimento.
2. No Unity Hub, use **Add project from disk**.
3. Aguarde a resolução dos packages e a primeira importação.
4. No Unity, execute:

```text
Tools > TW08 > Create Starter Content and Prototype Scenes
```

5. Execute:

```text
Tools > TW08 > Validate Project
```

6. Abra:

```text
Assets/_Project/Scenes/Tests/PuzzlePrototype.unity
Assets/_Project/Scenes/Tests/RacePrototype.unity
```

7. Abra **Window > General > Test Runner** e execute Edit Mode e Play Mode.

## Limite da validação atual

O projeto foi validado estaticamente fora do Unity. Ele não foi compilado por um Unity Editor neste ambiente. A primeira abertura no Unity é obrigatória para gerar `.meta`, resolver packages, compilar assemblies e executar testes.
