# Menus — movimento e estrutura

Todas as telas de menu do jogo passaram a usar o serviço de movimento
(`Scripts/Motion/UIMotion.cs`). Nenhuma tela troca de estado em corte seco.

## Linguagem de movimento

O efeito assinatura é o **terminal ligando linha por linha**: cabeçalho digitado
com máquina de escrever, depois os controles entrando em cascata. Isso é o
`MenuScreenAnimator`, aplicado em todas as telas.

| Situação | Resposta |
|---|---|
| Tela abre | cabeçalho digita, controles entram em cascata |
| Item recebe foco | escala, brilho e um marcador que desliza até ele |
| Clique / confirmar | pulso (`MenuPressFeedback`) |
| Ação indisponível | tremor (`MenuFeedback.Denied`) |
| Tela fecha | saída animada e só então a troca de cena (`MenuTransition`) |
| Fundo | grade derivando e varredura de CRT (`MenuBackdropAnimator`) |

## Componentes

| Arquivo | Papel |
|---|---|
| `UI/Menus/MenuScreenAnimator.cs` | entrada em cascata + cabeçalho digitado |
| `UI/Menus/MenuTransition.cs` | saída animada seguida de troca de cena |
| `UI/Menus/MenuPressFeedback.cs` | pulso de confirmação em botão |
| `UI/Menus/MenuFeedback.cs` | pulso e tremor sob demanda |
| `UI/Menus/MenuBackdropAnimator.cs` | movimento contínuo do fundo |
| `UI/Menus/CreditsScreenController.cs` | rolagem em loop dos créditos |
| `UI/MenuFocusAnimator.cs` | foco expressivo + marcador deslizante |
| `UI/ScrollToSelected.cs` | mantém o item selecionado enquadrado |

## Decisões que valem registro

**A navegação nunca depende da animação.** `MenuTransition` conta o tempo até o
carregamento no próprio `Update`, não numa corrotina do serviço de movimento. Se
a animação for interrompida — `Kill`, objeto destruído, runner perdido — o
jogador ainda navega. Nenhum menu pode prender o jogador numa tela que não avança.

**Uma propriedade, um dono.** `MenuFocusAnimator` escreve `localScale` todo frame;
por isso `MenuFeedback.Click` delega o pulso a ele quando existe um na hierarquia,
em vez de abrir um tween paralelo na mesma propriedade. O tremor mexe em
`anchoredPosition`, que ninguém mais controla, e vai direto pelo serviço.

**`MenuPressFeedback` é componente próprio, e não um listener.** O EventSystem
entrega `submit` ao objeto selecionado e `pointerDown` ao alvo do raycast — um
componente no pai não recebe os dois. E `onClick.AddListener` seria apagado: os
controladores de seleção chamam `RemoveAllListeners` ao religar a tela.

**`MenuScreenAnimator` não toca no `CanvasGroup` do shell.** Esse pertence ao
`ProfessionalMenuPresenter`, que o pipeline de produção instala no mesmo objeto.
Os dois se somam: o painel acende, depois as linhas entram.

**Fundo não usa tween.** `MenuBackdropAnimator` é um loop de `Update` em tempo
não-escalado porque não tem fim. A cor só é reescrita quando muda de verdade — a
grade gera mais de cem quads e remontar essa malha todo frame seria desperdício
numa tela parada.

**Créditos rolam dentro de uma máscara**, e a posição de origem é capturada no
`Awake` para que a cena salva nunca guarde um quadro intermediário da rolagem.

## Telas

Menu principal, Central de Operações, Operadores, Campanha (grade de 27 fases com
scroll), Arquivo Secreto, Corrida, Oficina N-8, Configurações e Créditos.

Na grade de fases o cartão mostra o melhor resultado e a medalha, e a cor indica o
estado: medalha conquistada tem precedência sobre o realce de "próxima fase" —
perder o registro de platina na grade seria pior do que perder a indicação de
onde continuar.

## Testes

`Tests/EditMode/MenuTests.cs` — regras puras: rótulo e cor do cartão de fase,
enquadramento do scroll por teclado, rolagem em loop dos créditos e as regras de
vitrine da Oficina (saldo, estoque, limite de slots).

## Integração pendente

Nenhuma. As telas são geradas por `Editor/TW08MenuSceneBuilder.cs` e
`Editor/TW08ShopSetup.cs`, ambos já ligados ao pipeline de produção.
