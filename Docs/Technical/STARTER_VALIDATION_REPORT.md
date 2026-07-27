# Relatório de validação do pacote

## Validado automaticamente

```text
estrutura de diretórios;
JSON de Packages/manifest.json;
JSON dos assembly definitions;
presença dos documentos obrigatórios;
balanceamento de chaves e delimitadores C# em verificação estática simples;
namespace raiz consistente;
ZIP criado e reaberto para conferência.
```

## Não validado neste ambiente

```text
compilação pelo Unity Editor;
importação dos pacotes;
execução das cenas geradas;
comportamento de Rigidbody2D;
Test Runner real;
build Windows;
performance em hardware alvo.
```

A validação definitiva deve ocorrer no Unity 6.3 LTS.
