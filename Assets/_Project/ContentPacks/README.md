# Content Packs — entrega opcional via Addressables

Estas pastas são as raízes dos grupos Addressables de conteúdo opcional
(criados por `Tools > TW08 > Production > Initialize Content Streaming`):

| Pasta      | Grupo Addressables    | Address                  | Status |
|------------|-----------------------|--------------------------|--------|
| Art/       | TW08-Optional-Art     | tw08/content/art         | vazio — aguardando arte HD opcional |
| Audio/     | TW08-Optional-Audio   | tw08/content/audio       | vazio — aguardando trilha final (ver REFERENCIA/The_Warehouse_N8_Trilha_Sonora_Prompts_Suno.md) |
| Narrative/ | TW08-Narrative-Packs  | tw08/content/narrative   | vazio — aguardando logs/terminais extras |
| Race/      | TW08-Race-Packs       | tw08/content/race        | vazio — aguardando pistas extras |

Cada grupo referencia a PASTA (folder-as-entry): qualquer asset colocado aqui
entra automaticamente no bundle correspondente no próximo build Addressables.

O conteúdo do jogo-base (fases, sprites starter, SFX de prototipagem) NÃO passa
por aqui — fica em `Assets/_Project/` normal e vai embutido no player build.
`ServerData/` (saída de build Addressables) está no .gitignore.
