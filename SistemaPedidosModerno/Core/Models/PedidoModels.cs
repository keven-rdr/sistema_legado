using System;
using System.Collections.Generic;
using SistemaPedidosModerno.Core.Enums;

namespace SistemaPedidosModerno.Core.Models
{
    public class ItemPedido
    {
        public string Nome { get; set; }
        public CategoriaItem Categoria { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }

        public decimal CalcularSubtotal()
        {
            decimal subtotal = PrecoUnitario * Quantidade;
            
            // Regras de sobretaxa por categoria
            if (Categoria == CategoriaItem.Alimento) subtotal += 2;
            if (Categoria == CategoriaItem.Importado) subtotal += 5;

            return subtotal;
        }
    }

    public class Pedido
    {
        public int Id { get; set; }
        public string NomeCliente { get; set; }
        public string EmailCliente { get; set; }
        public TipoCliente TipoCliente { get; set; }
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public string CupomDesconto { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public string EnderecoEntrega { get; set; }
        public double PesoTotal { get; set; }
        public bool EntregaExpressa { get; set; }
        public bool ClienteBloqueado { get; set; }
        public string PaisDestino { get; set; }
        public int NumeroParcelas { get; set; }

        public decimal CalcularSubtotalTotal()
        {
            decimal total = 0;
            foreach (var item in Itens)
            {
                total += item.CalcularSubtotal();
            }
            return total;
        }

        public bool PossuiEmailValido() => !string.IsNullOrWhiteSpace(EmailCliente);
    }
}
