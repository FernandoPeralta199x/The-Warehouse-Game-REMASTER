# Padrões de código C#

- namespaces por domínio;
- uma responsabilidade principal por componente;
- campos serializados privados e API pública mínima;
- eventos em vez de referências bidirecionais;
- ausência de `FindObjectOfType` em gameplay recorrente;
- ausência de caminhos de assets hardcoded em sistemas de produção;
- validação em `Awake`, `OnValidate` ou ferramentas de Editor;
- lógica testável fora de `Update` sempre que possível;
- corrotinas canceladas no desligamento;
- nenhuma exceção silenciada sem log e estratégia de recuperação;
- comentários explicam decisão, não repetem o código.
