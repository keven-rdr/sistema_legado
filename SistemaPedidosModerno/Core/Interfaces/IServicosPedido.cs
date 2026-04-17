using SistemaPedidosModerno.Core.Models;

namespace SistemaPedidosModerno.Core.Interfaces
{
    public interface ICalculadoraDesconto
    {
        decimal Calcular(Pedido pedido, decimal subtotal);
    }

    public interface ICalculadoraFrete
    {
        decimal Calcular(Pedido pedido);
    }

    public interface ICalculadoraJuros
    {
        decimal Calcular(Pedido pedido, decimal subtotal);
    }

    public interface ILogger
    {
        void Logar(string mensagem);
    }

    public interface INotificador
    {
        void Enviar(string mensagem, string destinatario);
    }
}
