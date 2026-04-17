using System;
using System.Collections.Generic;
using SistemaPedidosModerno.Core.Interfaces;

namespace SistemaPedidosModerno.Infrastructure.Logging
{
    public class ConsoleLogger : ILogger
    {
        private List<string> _logs = new List<string>();
        public void Logar(string mensagem)
        {
            string logFormatado = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensagem}";
            _logs.Add(logFormatado);
            Console.WriteLine(logFormatado);
        }

        public List<string> ObterLogs() => _logs;
    }
}
