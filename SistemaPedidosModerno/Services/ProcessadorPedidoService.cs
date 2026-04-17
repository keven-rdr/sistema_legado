using System;
using System.Collections.Generic;
using System.Text;
using SistemaPedidosModerno.Core.Interfaces;
using SistemaPedidosModerno.Core.Models;
using SistemaPedidosModerno.Core.Enums;

namespace SistemaPedidosModerno.Services
{
    /// <summary>
    /// Orquestrador de alto nível que coordena o fluxo de processamento de pedidos.
    /// Segue o princípio da Inversão de Dependência e Orquestração de Serviços.
    /// </summary>
    public class ProcessadorPedidoService
    {
        private readonly ICalculadoraDesconto _calculadoraDesconto;
        private readonly ICalculadoraFrete _calculadoraFrete;
        private readonly ICalculadoraJuros _calculadoraJuros;
        private readonly ValidadorPedido _validador;
        private readonly ILogger _logger;
        private readonly INotificador _notificador;

        public ProcessadorPedidoService(
            ICalculadoraDesconto calculadoraDesconto,
            ICalculadoraFrete calculadoraFrete,
            ICalculadoraJuros calculadoraJuros,
            ValidadorPedido validador,
            ILogger logger,
            INotificador notificador)
        {
            _calculadoraDesconto = calculadoraDesconto;
            _calculadoraFrete = calculadoraFrete;
            _calculadoraJuros = calculadoraJuros;
            _validador = validador;
            _logger = logger;
            _notificador = notificador;
        }

        public ResultadoProcessamento Processar(Pedido pedido, bool salvarLogs, bool enviarEmail)
        {
            var resultado = new ResultadoProcessamento { PedidoId = pedido.Id };

            // 1. Validação Completa (Restauração da lógica legada)
            var (isValido, erros) = _validador.Validar(pedido);
            if (!isValido)
            {
                resultado.Sucesso = false;
                foreach (var erro in erros) resultado.AdicionarMensagem(erro);
                return resultado;
            }

            // 2. Cálculos Financeiros (Uso de Strategies)
            decimal subtotal = pedido.CalcularSubtotalTotal();
            decimal frete = _calculadoraFrete.Calcular(pedido);
            decimal desconto = _calculadoraDesconto.Calcular(pedido, subtotal);
            decimal juros = _calculadoraJuros.Calcular(pedido, subtotal);

            decimal totalFinal = subtotal - desconto + frete + juros;
            if (totalFinal < 0) totalFinal = 0;

            // Preenchimento do resultado
            resultado.Sucesso = true;
            resultado.Subtotal = subtotal;
            resultado.Desconto = desconto;
            resultado.Frete = frete;
            resultado.Juros = juros;
            resultado.TotalFinal = totalFinal;

            // 3. Geração de Alertas e Regras Implícitas de Segurança
            GerarAlertas(pedido, subtotal, resultado);

            // 4. Execução de Tarefas de Infraestrutura (Logs e Notificações)
            ExecutarTarefasPosProcessamento(pedido, resultado, salvarLogs, enviarEmail);

            return resultado;
        }

        private void GerarAlertas(Pedido pedido, decimal subtotal, ResultadoProcessamento resultado)
        {
            if (subtotal > 1000) resultado.AdicionarMensagem("ALERTA: Pedido de alto valor identificado.");
            if (subtotal > 5000 && pedido.TipoCliente == TipoCliente.Novo) resultado.AdicionarMensagem("ALERTA_RISCO: Pedido suspeito - Cliente NOVO com alto valor.");
            if (pedido.FormaPagamento == FormaPagamento.Boleto && subtotal > 3000) resultado.AdicionarMensagem("AVISO_OPERACIONAL: Boleto acima do limite recomendado.");
            if (pedido.PaisDestino != "BR" && subtotal < 100) resultado.AdicionarMensagem("RECOMENDACAO: Pedido internacional com valor abaixo do recomendado.");
        }

        private void ExecutarTarefasPosProcessamento(Pedido pedido, ResultadoProcessamento resultado, bool salvarLogs, bool enviarEmail)
        {
            if (salvarLogs)
            {
                _logger.Logar($"Pedido processado com sucesso. ID: {pedido.Id} | Total: {resultado.TotalFinal:C}");
            }

            if (enviarEmail)
            {
                if (pedido.PossuiEmailValido())
                {
                    _notificador.Enviar($"Seu pedido #{pedido.Id} foi processado. Total: {resultado.TotalFinal:C}", pedido.EmailCliente);
                    resultado.AdicionarMensagem($"COMUNICACAO: Notificação enviada para {pedido.EmailCliente}.");
                }
                else
                {
                    resultado.AdicionarMensagem("COMUNICACAO_FALHA: Notificação não enviada (E-mail inexistente).");
                }
            }
        }
    }
}
