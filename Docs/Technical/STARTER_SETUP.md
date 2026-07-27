# Setup — Unity 6.3 LTS

## 1. Instalação

Instale pelo Unity Hub:

```text
Unity 6.3 LTS
Microsoft Visual Studio ou JetBrains Rider
Windows Build Support (IL2CPP), quando for gerar build
```

## 2. Abrir o projeto

No Unity Hub:

```text
Add project from disk
→ selecionar The_Warehouse_No_08_Unity_Starter
→ abrir com Unity 6.3 LTS
```

A primeira importação pode demorar porque o Unity recria `Library/` e resolve os pacotes.

## 3. Input System

Quando o Unity solicitar ativação do novo Input System, aceite e reinicie o Editor. O projeto usa `com.unity.inputsystem` e cria as ações por C#, reduzindo dependência de assets gerados.

## 4. Gerar conteúdo inicial

Execute:

```text
Tools > TW08 > Create Starter Content and Prototype Scenes
```

A ferramenta cria:

- `GameConfig.asset`;
- fase puzzle de demonstração;
- atributos de empilhadeira;
- cinco Power Ups originais;
- tabela ponderada de Power Ups;
- cena `PuzzlePrototype`;
- cena `RacePrototype`.

## 5. Testes

Abra:

```text
Window > General > Test Runner
```

Execute os testes em **EditMode**.

## 6. Configuração recomendada

```text
Color Space: Linear
Version Control Mode: Visible Meta Files
Asset Serialization: Force Text
Input Handling: Input System Package (New)
```

## 7. Primeiro commit

Não inclua:

```text
Library/
Logs/
Temp/
Obj/
UserSettings/
Builds geradas
```

Inclua sempre arquivos `.meta` criados pelo Unity.
