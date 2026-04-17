# Engenharia Reversa e Reestruturação de Sistema de Pedidos

Este projeto demonstra o processo de modernização de um sistema legado, passando pela análise, redocumentação e reestruturação completa utilizando princípios de Engenharia de Software modernos.

## 1. Engenharia Reversa (Parte 01 e 02)

O sistema original (`SistemaLegadoPedidos.cs`) foi identificado como um **God Method** (Método Deus), onde uma única função era responsável por todas as etapas do processo.

### Regras de Negócio Identificadas:
- **Financeiro:** Cálculo de subtotal com taxas por categoria (Alimento, Importado), descontos progressivos por tipo de cliente (VIP, Premium, etc.) e juros fixos para parcelamento no cartão.
- **Logística:** Cálculo de frete dinâmico baseado no peso e destino (Nacional BR vs Internacional), com suporte a frete expresso e cupons de isenção.
- **Segurança:** Bloqueios de clientes restritos e alertas automáticos para pedidos de alto valor (>1000) ou comportamentos suspeitos (Clientes novos > 5000).
- **Infraestrutura:** Persistência de logs e notificações via e-mail condicionais.

## 2. Redocumentação (Parte 02)

A primeira etapa de melhoria focou em:
- **Semântica:** Renomeação de variáveis de nomes genéricos (`temErro`, `itens`) para nomes descritivos (`possuiViolacoesDeRegra`, `itensDoPedido`).
- **Documentação:** Inclusão de comentários XML para facilitar o IntelliSense e manutenção futura.

## 3. Reestruturação Arquitetural (Parte 03)

A nova versão (`SistemaPedidosModerno/`) foi desenvolvida seguindo os princípios **SOLID** e **Clean Architecture**.

### Melhorias Implementadas:
- **Princípio de Responsabilidade Única (SRP):** Cada regra (frete, desconto, juros) agora possui sua própria classe.
- **Pattern Strategy:** As calculadoras financeiras foram desacopladas do serviço principal, permitindo que novas regras de frete ou desconto sejam adicionadas sem alterar o orquestrador.
- **Encapsulamento:** Remoção de campos públicos em favor de propriedades e métodos de comportamento nas classes de domínio (`Pedido`, `ItemPedido`).
- **Desacoplamento de Infraestrutura:** Uso de interfaces (`ILogger`, `INotificador`) que permitem trocar a forma de log ou envio de e-mail sem afetar a lógica de negócio.
- **Tipagem Forte:** Substituição de strings mágicas por `Enums` para evitar erros de digitação e ambiguidades.

## Estrutura do Projeto Moderno
- `Core/Enums`: Definições globais de tipos.
- `Core/Models`: Entidades de domínio ricas.
- `Core/Interfaces`: Contratos para inversão de dependência.
- `Services`: Lógica de orquestração e calculadoras concretas.

