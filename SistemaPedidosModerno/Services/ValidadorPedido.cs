using System.Collections.Generic;
using SistemaPedidosModerno.Core.Models;
using SistemaPedidosModerno.Core.Enums;

namespace SistemaPedidosModerno.Services
{
    /// <summary>
    /// Serviço especializado em validação de integridade e regras de negócio de pedidos.
    /// Recupera 100% da cobertura de validação do sistema legado original.
    /// </summary>
    public class ValidadorPedido
    {
        public (bool IsValido, List<string> Erros) Validar(Pedido pedido)
        {
            var erros = new List<string>();

            // 1. Validação de Pedido
            if (pedido.Id <= 0) erros.Add("Pedido inválido: ID deve ser maior que zero.");

            // 2. Validação de Cliente
            if (string.IsNullOrWhiteSpace(pedido.NomeCliente)) erros.Add("Nome do cliente não informado.");
            if (pedido.ClienteBloqueado) erros.Add("Cliente bloqueado.");

            // 3. Validação de Entrega
            if (string.IsNullOrWhiteSpace(pedido.EnderecoEntrega)) erros.Add("Endereço de entrega não informado.");

            // 4. Validação de Itens
            if (pedido.Itens == null)
            {
                erros.Add("Lista de itens nula.");
            }
            else if (pedido.Itens.Count == 0)
            {
                erros.Add("Pedido sem itens.");
            }
            else
            {
                foreach (var item in pedido.Itens)
                {
                    if (item.Quantidade <= 0) erros.Add($"Item com quantidade inválida: {item.Nome}");
                    if (item.PrecoUnitario < 0) erros.Add($"Item com preço inválido: {item.Nome}");
                }
            }

            return (erros.Count == 0, erros);
        }
    }
}
