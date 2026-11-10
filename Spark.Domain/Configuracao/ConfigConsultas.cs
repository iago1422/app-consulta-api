using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Spark.Domain.Configuracao
{
    public class TipoConsultaConfig
    {
        public string Tipo { get; set; }
        public string Rotulo { get; set; }
        public int Creditos { get; set; }
    }

    public class PacoteConfig
    {
        public string Id { get; set; }
        public string Tipo { get; set; }
        public int Consultas { get; set; }
        public decimal Preco { get; set; }
        public bool Ativo { get; set; } = true;
    }

    /// <summary>
    /// Custos das consultas, editados no painel do portal e guardados como JSON em Configuracoes["consultas"].
    /// Os créditos de um pacote não são guardados: valem Consultas × Creditos do tipo.
    /// </summary>
    public class ConfigConsultas
    {
        public const string Chave = "consultas";

        // Piso de R$/crédito: o webhook não credita abaixo disso (menor preço já vendido: 1000 créditos por R$950).
        public const decimal PrecoMinimoPorCredito = 0.95m;

        public static readonly string[] TiposObrigatorios = { "bate_papo", "audio_video" };

        public List<TipoConsultaConfig> Tipos { get; set; } = new List<TipoConsultaConfig>();
        public List<PacoteConfig> Pacotes { get; set; } = new List<PacoteConfig>();

        public static ConfigConsultas Padrao() => new ConfigConsultas
        {
            Tipos = new List<TipoConsultaConfig>
            {
                new TipoConsultaConfig { Tipo = "bate_papo", Rotulo = "Bate-papo", Creditos = 60 },
                new TipoConsultaConfig { Tipo = "audio_video", Rotulo = "Áudio/vídeo", Creditos = 70 },
            },
            Pacotes = new List<PacoteConfig>
            {
                new PacoteConfig { Id = "chat-1", Tipo = "bate_papo", Consultas = 1, Preco = 72.90m },
                new PacoteConfig { Id = "chat-3", Tipo = "bate_papo", Consultas = 3, Preco = 196.90m },
                new PacoteConfig { Id = "chat-5", Tipo = "bate_papo", Consultas = 5, Preco = 309.90m },
                new PacoteConfig { Id = "video-1", Tipo = "audio_video", Consultas = 1, Preco = 84.90m },
                new PacoteConfig { Id = "video-3", Tipo = "audio_video", Consultas = 3, Preco = 229.90m },
                new PacoteConfig { Id = "video-5", Tipo = "audio_video", Consultas = 5, Preco = 359.90m },
            }
        };

        public int? CreditosDe(string tipo) => Tipos?.FirstOrDefault(t => t.Tipo == tipo)?.Creditos;

        public int CreditosDoPacote(PacoteConfig pacote) => pacote.Consultas * (CreditosDe(pacote.Tipo) ?? 0);

        private static string Reais(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');

        /// <summary>Erros em português, prontos para o toast do portal. Vazio = válido.</summary>
        public List<string> Validar()
        {
            var erros = new List<string>();
            var tipos = Tipos ?? new List<TipoConsultaConfig>();
            var pacotes = Pacotes ?? new List<PacoteConfig>();

            foreach (var obrigatorio in TiposObrigatorios)
                if (tipos.Count(t => t.Tipo == obrigatorio) != 1)
                    erros.Add($"Informe o tipo \"{obrigatorio}\" uma única vez.");

            foreach (var t in tipos)
            {
                if (!TiposObrigatorios.Contains(t.Tipo))
                {
                    erros.Add($"Tipo \"{t.Tipo}\" não existe.");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(t.Rotulo) || t.Rotulo.Length > 40)
                    erros.Add($"{t.Tipo}: o nome precisa ter de 1 a 40 caracteres.");
                if (t.Creditos < 1 || t.Creditos > 1000)
                    erros.Add($"{t.Rotulo ?? t.Tipo}: créditos por consulta devem ficar entre 1 e 1000.");
            }

            foreach (var id in pacotes.GroupBy(p => p.Id).Where(g => g.Count() > 1).Select(g => g.Key))
                erros.Add($"Pacote \"{id}\" repetido.");

            foreach (var p in pacotes)
            {
                if (string.IsNullOrWhiteSpace(p.Id))
                {
                    erros.Add("Todo pacote precisa de um id.");
                    continue;
                }
                var nome = $"Pacote {p.Id}";
                if (!TiposObrigatorios.Contains(p.Tipo))
                {
                    erros.Add($"{nome}: tipo \"{p.Tipo}\" não existe.");
                    continue;
                }
                if (p.Consultas < 1 || p.Consultas > 50)
                    erros.Add($"{nome}: quantidade de consultas deve ficar entre 1 e 50.");
                if (p.Preco < 1 || p.Preco > 10000)
                    erros.Add($"{nome}: preço deve ficar entre R$ 1 e R$ 10.000.");
                if (decimal.Round(p.Preco, 2) != p.Preco)
                    erros.Add($"{nome}: preço com no máximo 2 casas decimais.");

                var creditos = CreditosDoPacote(p);
                var minimo = creditos * PrecoMinimoPorCredito;
                if (p.Ativo && creditos > 0 && p.Preco < minimo)
                    erros.Add($"{nome}: R$ {Reais(p.Preco)} por {creditos} créditos fica abaixo do mínimo de R$ {Reais(PrecoMinimoPorCredito)} por crédito (mínimo R$ {Reais(minimo)}).");
            }

            foreach (var obrigatorio in TiposObrigatorios)
                if (!pacotes.Any(p => p.Tipo == obrigatorio && p.Ativo))
                    erros.Add($"\"{obrigatorio}\" precisa de pelo menos um pacote ativo.");

            return erros;
        }
    }
}
