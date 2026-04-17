using System;
using System.Collections.Generic;
using System.Text;
using SistemaPedidosModerno.Core.Interfaces;
using SistemaPedidosModerno.Core.Models;
using SistemaPedidosModerno.Core.Enums;

namespace SistemaPedidosModerno.Services
{
    public class ProcessadorPedidoService
    {
        private readonly ICalculadoraDesconto _calculadoraDesconto;
        private readonly ICalculadoraFrete _calculadoraFrete;
        private readonly ICalculadoraJuros _calculadoraJuros;
        private readonly ILogger _logger;
        private readonly INotificador _notificador;

        public ProcessadorPedidoService(
            ICalculadoraDesconto calculadoraDesconto,
            ICalculadoraFrete calculadoraFrete,
            ICalculadoraJuros calculadoraJuros,
            ILogger logger,
            INotificador notificador)
        {
            _calculadoraDesconto = calculadoraDesconto;
            _calculadoraFrete = calculadoraFrete;
            _calculadoraJuros = calculadoraJuros;
            _logger = logger;
            _notificador = notificador;
        }

        public string Processar(Pedido pedido, bool salvarLogs, bool enviarEmail)
        {
            var relatorio = new StringBuilder();

            // 1. Validação Básica
            if (pedido.ClienteBloqueado)
            {
                return "ERRO_BLOQUEIO: Cliente possui restrições financeiras.\n";
            }

            if (pedido.Itens == null || pedido.Itens.Count == 0)
            {
                return "ERRO_ITENS: Pedido vazio.\n";
            }

            // 2. Cálculos
            decimal subtotal = pedido.CalcularSubtotalTotal();
            decimal frete = _calculadoraFrete.Calcular(pedido);
            decimal desconto = _calculadoraDesconto.Calcular(pedido, subtotal);
            decimal juros = _calculadoraJuros.Calcular(pedido, subtotal);

            decimal total = subtotal - desconto + frete + juros;
            if (total < 0) total = 0;

            // 3. Alertas de Segurança (Regras Implícitas)
            ProcessarAlertas(pedido, subtotal, relatorio);

            // 4. Ações Externas (Logs e Notificações)
            if (salvarLogs)
            {
                _logger.Logar($"Pedido {pedido.Id} processado. Total: {total}");
            }

            if (enviarEmail && pedido.PossuiEmailValido())
            {
                _notificador.Enviar("Seu pedido foi processado com sucesso.", pedido.EmailCliente);
                relatorio.AppendLine($"CONFIRMACAO: Notificação enviada para {pedido.EmailCliente}");
            }
            else if (enviarEmail)
            {
                relatorio.AppendLine("AVISO: Email não enviado por falta de dados.");
            }

            relatorio.AppendLine($"TOTAL_FINAL={total}");

            return relatorio.ToString();
        }

        private void ProcessarAlertas(Pedido pedido, decimal subtotal, StringBuilder relatorio)
        {
            if (subtotal > 1000) relatorio.AppendLine("ALERTA: Pedido de alto valor.");
            if (subtotal > 5000 && pedido.TipoCliente == TipoCliente.Novo) relatorio.AppendLine("RISCO: Cliente novo com pedido de valor crítico.");
            if (pedido.FormaPagamento == FormaPagamento.Boleto && subtotal > 3000) relatorio.AppendLine("AVISO: Boleto acima do limite operacional.");
            if (pedido.PaisDestino != "BR" && subtotal < 100) relatorio.AppendLine("LOGISTICA: Pedido internacional de baixo valor.");
        }
    }

    // Implementações simples de Infraestrutura para demonstração
    public class ConsoleLogger : ILogger
    {
        public List<string> Logs = new List<string>();
        public void Logar(string mensagem) => Logs.Add($"[LOG {DateTime.Now}] {mensagem}");
    }

    public class SmtpNotificador : INotificador
    {
        public void Enviar(string mensagem, string destinatario) => Console.WriteLine($"Email enviado para {destinatario}: {mensagem}");
    }
}
