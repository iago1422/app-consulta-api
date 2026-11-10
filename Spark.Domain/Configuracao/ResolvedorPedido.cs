using System.Collections.Generic;
using System.Linq;

namespace Spark.Domain.Configuracao
{
    public record PedidoResolvido(decimal PrecoTotal, int Creditos, string Titulo);

    /// <summary>Decide preço, créditos e título do checkout. Sem I/O, para testar sem o SDK do Mercado Pago.</summary>
    public static class ResolvedorPedido
    {
        // Créditos que o app ≤ 1.3.9 envia → pacote equivalente. Valores fixos daquelas versões: não mudar.
        public static readonly IReadOnlyDictionary<int, string> PacotesLegados = new Dictionary<int, string>
        {
            [60] = "chat-1", [180] = "chat-3", [300] = "chat-5",
            [70] = "video-1", [210] = "video-3", [350] = "video-5",
        };

        public static (PedidoResolvido Pedido, string Erro) Resolver(
            string pacoteId, decimal creditosCliente, decimal precoTotalCliente, string tituloCliente, ConfigConsultas config)
        {
            // App novo: o pacote manda
            if (!string.IsNullOrWhiteSpace(pacoteId))
            {
                var pacote = config.Pacotes.FirstOrDefault(p => p.Id == pacoteId && p.Ativo);
                return pacote == null ? (null, "Pacote indisponível.") : (DoPacote(pacote, config), null);
            }

            if (creditosCliente <= 0)
                return (null, "UserId e Creditos são obrigatórios.");

            // App antigo: reconhece o pacote pelos créditos e cobra o preço atual
            if (creditosCliente == decimal.Truncate(creditosCliente)
                && PacotesLegados.TryGetValue((int)creditosCliente, out var legado))
            {
                var pacote = config.Pacotes.FirstOrDefault(p => p.Id == legado && p.Ativo);
                if (pacote != null)
                    return (DoPacote(pacote, config), null);
            }

            // Fora da tabela (pacote de desenvolvimento, versões muito antigas): regra de antes
            if (precoTotalCliente <= 0)
                return (null, "UnitPrice deve ser maior que zero.");
            if (precoTotalCliente < creditosCliente * ConfigConsultas.PrecoMinimoPorCredito)
                return (null, "Valor do pedido incompatível com a quantidade de créditos.");

            return (new PedidoResolvido(precoTotalCliente, (int)creditosCliente,
                string.IsNullOrWhiteSpace(tituloCliente) ? "Pedido" : tituloCliente), null);
        }

        private static PedidoResolvido DoPacote(PacoteConfig p, ConfigConsultas config)
        {
            var rotulo = (config.Tipos.FirstOrDefault(t => t.Tipo == p.Tipo)?.Rotulo ?? p.Tipo).ToLowerInvariant();
            var titulo = p.Consultas == 1 ? $"1 consulta por {rotulo}" : $"{p.Consultas} consultas por {rotulo}";
            return new PedidoResolvido(p.Preco, config.CreditosDoPacote(p), titulo);
        }
    }
}
