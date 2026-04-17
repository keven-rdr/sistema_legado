using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaLegadoPedidos
{
    /// <summary>
    /// Classe responsável pelo processamento central de pedidos do sistema legado.
    /// Contém regras de negócio para validação, cálculos financeiros e notificações.
    /// </summary>
    public class ProcessadorDePedidos
    {
        private List<string> _logsDoSistema = new List<string>();

        /// <summary>
        /// Processa um pedido completo realizando validações, cálculos de preços e disparando ações secundárias.
        /// </summary>
        /// <param name="idDoPedido">Identificador único do pedido.</param>
        /// <param name="nomeDoCliente">Nome completo do cliente.</param>
        /// <param name="emailDoCliente">Endereço de e-mail para notificações.</param>
        /// <param name="tipoDoCliente">Categoria do cliente (VIP, PREMIUM, NORMAL, NOVO).</param>
        /// <param name="itensDoPedido">Lista de produtos incluídos no pedido.</param>
        /// <param name="cupomDesconto">Código promocional para desconto adicional.</param>
        /// <param name="formaDePagamento">Método de pagamento (CARTAO, BOLETO, PIX, DINHEIRO).</param>
        /// <param name="enderecoDeEntrega">Logradouro para entrega.</param>
        /// <param name="pesoTotal">Peso físico total da carga em kg.</param>
        /// <param name="entregaExpressa">Flag para prioridade de entrega ágil.</param>
        /// <param name="clienteBloqueado">Status de crédito/bloqueio do cliente.</param>
        /// <param name="enviarNotificacaoEmail">Flag para execução de envio de e-mail ao final.</param>
        /// <param name="salvarLogProcessamento">Flag para persistência de logs internos.</param>
        /// <param name="paisDestino">Código do país de destino (ex: BR).</param>
        /// <param name="numeroDeParcelas">Quantidade de parcelas para pagamentos a prazo.</param>
        /// <returns>Relatório textual do processamento contendo erros, alertas e o total final.</returns>
        public string Processar(
            int idDoPedido,
            string nomeDoCliente,
            string emailDoCliente,
            string tipoDoCliente,
            List<ItemPedido> itensDoPedido,
            string cupomDesconto,
            string formaDePagamento,
            string enderecoDeEntrega,
            double pesoTotal,
            bool entregaExpressa,
            bool clienteBloqueado,
            bool enviarNotificacaoEmail,
            bool salvarLogProcessamento,
            string paisDestino,
            int numeroDeParcelas)
        {
            string relatorioProcessamento = "";
            double subtotalCalculado = 0;
            double valorDesconto = 0;
            double valorFrete = 0;
            double valorJuros = 0;
            double valorTotalFinal = 0;
            bool possuiViolacoesDeRegra = false;

            // --- FASE 1: VALIDAÇÃO DE DADOS BÁSICOS ---
            if (idDoPedido <= 0)
            {
                relatorioProcessamento += "PEDIDO_INVALIDO: ID deve ser maior que zero.\n";
                possuiViolacoesDeRegra = true;
            }

            if (string.IsNullOrEmpty(nomeDoCliente))
            {
                relatorioProcessamento += "CLIENTE_INVALIDO: Nome do cliente não informado.\n";
                possuiViolacoesDeRegra = true;
            }

            if (string.IsNullOrEmpty(emailDoCliente))
            {
                relatorioProcessamento += "AVISO: Email do cliente não informado (notificações serão desabilitadas).\n";
            }

            if (clienteBloqueado)
            {
                relatorioProcessamento += "BLOQUEIO: O cliente possui restrições e não pode realizar pedidos.\n";
                possuiViolacoesDeRegra = true;
            }

            if (itensDoPedido == null)
            {
                relatorioProcessamento += "ERRO_ITENS: A lista de itens está nula.\n";
                possuiViolacoesDeRegra = true;
            }
            else if (itensDoPedido.Count == 0)
            {
                relatorioProcessamento += "ERRO_ITENS: O pedido não contém nenhum produto.\n";
                possuiViolacoesDeRegra = true;
            }
            else
            {
                // Cálculo de Subtotal e Validação de Itens
                foreach (var item in itensDoPedido)
                {
                    if (item.Quantidade <= 0)
                    {
                        relatorioProcessamento += $"ITEM_INVALIDO: Quantidade inválida para o item {item.Nome}.\n";
                        possuiViolacoesDeRegra = true;
                    }

                    if (item.PrecoUnitario < 0)
                    {
                        relatorioProcessamento += $"ITEM_INVALIDO: Preço negativo para o item {item.Nome}.\n";
                        possuiViolacoesDeRegra = true;
                    }

                    subtotalCalculado += (item.PrecoUnitario * item.Quantidade);

                    // Regras extras de categoria (Sobretaxas de manuseio)
                    if (item.Categoria == "ALIMENTO") subtotalCalculado += 2;
                    if (item.Categoria == "IMPORTADO") subtotalCalculado += 5;
                }
            }

            // --- FASE 2: CÁLCULOS FINANCEIROS (Caso não haja erros bloqueantes) ---
            if (!possuiViolacoesDeRegra)
            {
                // 2.1 - Regras de Desconto por Tipo de Cliente
                if (tipoDoCliente == "VIP") valorDesconto = subtotalCalculado * 0.15;
                else if (tipoDoCliente == "PREMIUM") valorDesconto = subtotalCalculado * 0.10;
                else if (tipoDoCliente == "NORMAL") valorDesconto = subtotalCalculado * 0.02;
                else if (tipoDoCliente == "NOVO") valorDesconto = 0;
                else valorDesconto = 1; // Valor simbólico para categorias não mapeadas

                // 2.2 - Regras de Cupom de Desconto
                if (!string.IsNullOrEmpty(cupomDesconto))
                {
                    if (cupomDesconto == "DESC10") valorDesconto += (subtotalCalculado * 0.10);
                    else if (cupomDesconto == "DESC20") valorDesconto += (subtotalCalculado * 0.20);
                    else if (cupomDesconto == "FRETEGRATIS") valorFrete = 0; // Previne cobrança posterior
                    else if (cupomDesconto == "VIP50" && tipoDoCliente == "VIP") valorDesconto += 50;
                    else relatorioProcessamento += "CUPOM_INVALIDO: Cupom expirado ou não aplicável a esta categoria.\n";
                }

                if (string.IsNullOrEmpty(enderecoDeEntrega))
                {
                    relatorioProcessamento += "ERRO_ENTREGA: Endereço não informado.\n";
                    possuiViolacoesDeRegra = true;
                }

                // 2.3 - Regras de Cálculo de Frete (Nacional vs Internacional)
                if (paisDestino == "BR")
                {
                    if (pesoTotal <= 1) valorFrete = 10;
                    else if (pesoTotal <= 5) valorFrete = 25;
                    else if (pesoTotal <= 10) valorFrete = 40;
                    else valorFrete = 70;

                    if (entregaExpressa) valorFrete += 30;
                }
                else // Internacional
                {
                    if (pesoTotal <= 1) valorFrete = 50;
                    else if (pesoTotal <= 5) valorFrete = 80;
                    else valorFrete = 120;

                    if (entregaExpressa) valorFrete += 70;
                }

                // 2.4 - Regras de Forma de Pagamento e Juros
                if (formaDePagamento == "CARTAO")
                {
                    if (numeroDeParcelas > 1 && numeroDeParcelas <= 6) valorJuros = subtotalCalculado * 0.02;
                    else if (numeroDeParcelas > 6) valorJuros = subtotalCalculado * 0.05;
                }
                else if (formaDePagamento == "BOLETO") valorDesconto += 5;
                else if (formaDePagamento == "PIX") valorDesconto += 10;
                else if (formaDePagamento != "DINHEIRO")
                {
                    relatorioProcessamento += "ERRO_PAGAMENTO: Forma de pagamento não suportada.\n";
                    possuiViolacoesDeRegra = true;
                }

                // --- FASE 3: CONSOLIDAÇÃO E ALERTAS DE SEGURANÇA ---
                valorTotalFinal = subtotalCalculado - valorDesconto + valorFrete + valorJuros;
                if (valorTotalFinal < 0) valorTotalFinal = 0;

                // Alertas Baseados em Valor
                if (subtotalCalculado > 1000) relatorioProcessamento += "ALERTA: Pedido de alto valor identificado.\n";
                if (subtotalCalculado > 5000 && tipoDoCliente == "NOVO") relatorioProcessamento += "ALERTA_RISCO: Pedido suspeito - Cliente NOVO com alto valor.\n";
                if (formaDePagamento == "BOLETO" && subtotalCalculado > 3000) relatorioProcessamento += "AVISO_OPERACIONAL: Boleto acima do limite recomendado para processamento automático.\n";
                if (paisDestino != "BR" && subtotalCalculado < 100) relatorioProcessamento += "RECOMENDACAO: Pedido internacional com valor baixo; verifique viabilidade logística.\n";

                // --- FASE 4: INFRAESTRUTURA (LOGS E NOTIFICAÇÕES) ---
                if (salvarLogProcessamento)
                {
                    _logsDoSistema.Add($"[LOG] Pedido: {idDoPedido} | Cliente: {nomeDoCliente} | Total: {valorTotalFinal} | Data: {DateTime.Now}");
                }

                if (enviarNotificacaoEmail)
                {
                    if (!string.IsNullOrEmpty(emailDoCliente)) relatorioProcessamento += $"COMUNICACAO: Notificação de confirmação enviada para {emailDoCliente}.\n";
                    else relatorioProcessamento += "COMUNICACAO_FALHA: Notificação não enviada (email nulo).\n";
                }

                relatorioProcessamento += $"TOTAL_FINAL={valorTotalFinal}\n";
            }

            return relatorioProcessamento;
        }

        public List<string> ObterLogs()
        {
            return _logsDoSistema;
        }
    }

    /// <summary>
    /// Representa um item de produto dentro de um pedido.
    /// </summary>
    public class ItemPedido
    {
        public string Nome;
        public string Categoria;
        public int Quantidade;
        public double PrecoUnitario;
    }
}