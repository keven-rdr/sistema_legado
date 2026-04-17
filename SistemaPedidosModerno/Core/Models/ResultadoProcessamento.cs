using System.Collections.Generic;

namespace SistemaPedidosModerno.Core.Models
{
    /// <summary>
    /// Representa o resultado estruturado do processamento de um pedido.
    /// Substitui o retorno textual (string) do sistema legado por um objeto rico.
    /// </summary>
    public class ResultadoProcessamento
    {
        public bool Sucesso { get; set; }
        public int PedidoId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Frete { get; set; }
        public decimal Juros { get; set; }
        public decimal TotalFinal { get; set; }
        
        // Lista de mensagens informativas, alertas ou erros encontrados
        public List<string> Mensagens { get; set; } = new List<string>();

        public void AdicionarMensagem(string mensagem) => Mensagens.Add(mensagem);

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- RESULTADO DO PEDIDO #{PedidoId} ---");
            sb.AppendLine($"Status: {(Sucesso ? "SUCESSO" : "FALHA")}");
            
            foreach (var msg in Mensagens) sb.AppendLine($"- {msg}");
            
            if (Sucesso)
            {
                sb.AppendLine($"Subtotal: {Subtotal:C}");
                sb.AppendLine($"Desconto: {Desconto:C}");
                sb.AppendLine($"Frete: {Frete:C}");
                sb.AppendLine($"Juros: {Juros:C}");
                sb.AppendLine($"TOTAL_FINAL={TotalFinal:F2}");
            }
            
            return sb.ToString();
        }
    }
}
