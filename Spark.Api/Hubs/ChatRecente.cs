using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Spark.Api.Hubs
{
    /// <summary>
    /// Últimas mensagens de cada consulta. O hub só entrega a quem está no grupo naquele instante:
    /// quem estava reconectando (rede trocou, app em segundo plano, aba suspensa) ou recarregou a
    /// página perdia a mensagem. Os clientes pedem esta lista ao entrar e a cada reconexão.
    /// </summary>
    // ponytail: em memória, some num restart/deploy e não escala para mais de uma instância
    // (o SignalR sem backplane também não). Banco se a conversa precisar sobreviver a isso.
    public static class ChatRecente
    {
        public const int Limite = 500;
        // Anexos vão em base64 dentro da mensagem (/chat/upload devolve data URL): sem orçamento,
        // algumas fotos esgotariam a memória da instância. Estourou, saem as mais antigas.
        public const int OrcamentoBytes = 10 * 1024 * 1024;
        private static readonly TimeSpan Validade = TimeSpan.FromHours(12);

        private sealed class Conversa
        {
            public readonly List<JsonElement> Mensagens = new();
            public readonly HashSet<string> Ids = new();
            public long Bytes;
            public DateTime Ultima = DateTime.UtcNow;
        }

        private static readonly ConcurrentDictionary<string, Conversa> Conversas = new(StringComparer.OrdinalIgnoreCase);

        public static void Guardar(string consultId, JsonElement mensagem)
        {
            if (string.IsNullOrWhiteSpace(consultId)) return;
            if (mensagem.ValueKind != JsonValueKind.Object) return;
            var tamanho = mensagem.GetRawText().Length;
            if (tamanho > OrcamentoBytes) return; // maior que o orçamento inteiro: só ao vivo

            LimparExpiradas();
            var conversa = Conversas.GetOrAdd(consultId, _ => new Conversa());
            var id = mensagem.TryGetProperty("id", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

            lock (conversa)
            {
                if (id != null && !conversa.Ids.Add(id)) return;
                conversa.Mensagens.Add(mensagem.Clone());
                conversa.Bytes += tamanho;
                conversa.Ultima = DateTime.UtcNow;
                while (conversa.Mensagens.Count > Limite || conversa.Bytes > OrcamentoBytes)
                {
                    var antiga = conversa.Mensagens[0];
                    conversa.Mensagens.RemoveAt(0);
                    conversa.Bytes -= antiga.GetRawText().Length;
                    if (antiga.TryGetProperty("id", out var a) && a.ValueKind == JsonValueKind.String)
                        conversa.Ids.Remove(a.GetString());
                }
            }
        }

        public static IReadOnlyList<JsonElement> Listar(string consultId)
        {
            if (string.IsNullOrWhiteSpace(consultId) || !Conversas.TryGetValue(consultId, out var conversa))
                return Array.Empty<JsonElement>();
            lock (conversa) return conversa.Mensagens.ToArray();
        }

        public static void Encerrar(string consultId)
        {
            if (!string.IsNullOrWhiteSpace(consultId)) Conversas.TryRemove(consultId, out _);
        }

        private static void LimparExpiradas()
        {
            var limite = DateTime.UtcNow - Validade;
            foreach (var par in Conversas.Where(c => c.Value.Ultima < limite))
                Conversas.TryRemove(par.Key, out _);
        }
    }
}
