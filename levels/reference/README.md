# `levels/reference/` — material de referência interno

**NÃO COMMITAR LAYOUTS DESTA PASTA.**

Esta pasta é destinada a guardar saídas das ferramentas em `tools/` que
decodificam as 80 fases do ROM original *Shove It!* (NCS/Masaya, 1990)
para **estudo interno** de design (curva de dificuldade, padrões
recorrentes, par moves esperado).

O `.gitignore` do projeto raiz exclui `levels/reference/*.xsb` e
`levels/reference/*.json`. Este `README.md` é o único arquivo desta
pasta que entra no Git.

Veja [docs/ADR-003-ip-strategy.md](../../docs/ADR-003-ip-strategy.md)
para a política completa.

## Como popular esta pasta (uso pessoal)

```
cd tools/
python extract_levels.py \
    --rom "../../../Shove It - The Warehouse Game (U) [!].bin" \
    --out ../levels/reference/
```

Os arquivos gerados ficam SÓ no seu disco. Nunca dão `git add`.

Se você for outra pessoa lendo este repo: você não precisa desta pasta
para rodar o jogo. As fases distribuíveis ficam em `levels/new/`.
