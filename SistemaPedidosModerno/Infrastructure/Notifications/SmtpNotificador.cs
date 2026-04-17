using System;
using SistemaPedidosModerno.Core.Interfaces;

namespace SistemaPedidosModerno.Infrastructure.Notifications
{
    public class SmtpNotificador : INotificador
    {
        public void Enviar(string mensagem, string destinatario)
        {
            // Simulação de envio de e-mail (SMTP)
            Console.WriteLine($"[SMTP] Enviando e-mail para {destinatario}...");
            Console.WriteLine($"[CONTEÚDO] {mensagem}");
        }
    }
}
