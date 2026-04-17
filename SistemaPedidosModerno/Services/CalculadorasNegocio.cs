using SistemaPedidosModerno.Core.Interfaces;
using SistemaPedidosModerno.Core.Models;
using SistemaPedidosModerno.Core.Enums;

namespace SistemaPedidosModerno.Services
{
    public class CalculadoraDesconto : ICalculadoraDesconto
    {
        public decimal Calcular(Pedido pedido, decimal subtotal)
        {
            decimal desconto = 0;

            // Desconto por tipo de cliente
            switch (pedido.TipoCliente)
            {
                case TipoCliente.Vip: desconto = subtotal * 0.15m; break;
                case TipoCliente.Premium: desconto = subtotal * 0.10m; break;
                case TipoCliente.Normal: desconto = subtotal * 0.02m; break;
                case TipoCliente.NaoMapeado: desconto = 1; break;
            }

            // Descontos adicionais por cupom
            if (!string.IsNullOrEmpty(pedido.CupomDesconto))
            {
                if (pedido.CupomDesconto == "DESC10") desconto += subtotal * 0.10m;
                else if (pedido.CupomDesconto == "DESC20") desconto += subtotal * 0.20m;
                else if (pedido.CupomDesconto == "VIP50" && pedido.TipoCliente == TipoCliente.Vip) desconto += 50;
            }

            // Descontos por forma de pagamento
            if (pedido.FormaPagamento == FormaPagamento.Boleto) desconto += 5;
            else if (pedido.FormaPagamento == FormaPagamento.Pix) desconto += 10;

            return desconto;
        }
    }

    public class CalculadoraFrete : ICalculadoraFrete
    {
        public decimal Calcular(Pedido pedido)
        {
            if (pedido.CupomDesconto == "FRETEGRATIS") return 0;

            decimal frete = 0;
            bool isNacional = pedido.PaisDestino == "BR";

            if (isNacional)
            {
                if (pedido.PesoTotal <= 1) frete = 10;
                else if (pedido.PesoTotal <= 5) frete = 25;
                else if (pedido.PesoTotal <= 10) frete = 40;
                else frete = 70;

                if (pedido.EntregaExpressa) frete += 30;
            }
            else
            {
                if (pedido.PesoTotal <= 1) frete = 50;
                else if (pedido.PesoTotal <= 5) frete = 80;
                else frete = 120;

                if (pedido.EntregaExpressa) frete += 70;
            }

            return frete;
        }
    }

    public class CalculadoraJuros : ICalculadoraJuros
    {
        public decimal Calcular(Pedido pedido, decimal subtotal)
        {
            if (pedido.FormaPagamento != FormaPagamento.Cartao) return 0;

            if (pedido.NumeroParcelas > 1 && pedido.NumeroParcelas <= 6) return subtotal * 0.02m;
            if (pedido.NumeroParcelas > 6) return subtotal * 0.05m;

            return 0;
        }
    }
}
