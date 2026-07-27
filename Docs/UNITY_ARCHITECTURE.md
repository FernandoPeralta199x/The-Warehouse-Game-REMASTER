# Arquitetura Unity

## Decisões principais

- regras do puzzle permanecem separadas da apresentação;
- ScriptableObjects armazenam configuração estática, nunca o save do jogador;
- JSON versionado é usado para persistência;
- Input System centraliza teclado e gamepad;
- serviços globais são limitados ao bootstrap e registrados explicitamente;
- UI, gameplay e narrativa comunicam-se por eventos e contratos;
- carregamento de cena é assíncrono;
- código de Editor fica em assembly exclusivo;
- testes são separados em Edit Mode e Play Mode.

## Módulos

```text
Core        bootstrap, serviços, estado, pausa e cenas
Player      entrada e intenção do usuário
Puzzle      modelo lógico, apresentação, histórico e validação
Forklift    física arcade, corrida, IA, carga e Power Ups
Audio       SFX, pooling, música e crossfade
Save        persistência, integridade, backup e migração
UI          telas e fluxo de navegação
Narrative   sequências, gatilhos e contratos de apresentação
Levels      catálogo, metadados e desbloqueios
Editor      setup, validação e importação de pixel art
```

## Dependências

O runtime está em uma assembly própria (`TW08.Runtime`). Editor e testes possuem assemblies separadas. A próxima divisão recomendada, quando o projeto crescer, é extrair o modelo puro de puzzle e os contratos de save para assemblies sem referência ao UnityEngine.

## Não adotado nesta fase

- Addressables: adiado até existir volume real de conteúdo que justifique o custo operacional;
- framework de injeção de dependência externo: o registro interno é suficiente para a escala inicial;
- ECS/DOTS: inadequado para o núcleo atual e adicionaria complexidade sem benefício comprovado;
- multiplayer: fora do escopo atual.
