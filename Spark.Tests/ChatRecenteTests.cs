using System;
using System.Linq;
using System.Text.Json;
using Spark.Api.Hubs;
using Xunit;

namespace Spark.Tests
{
    public class ChatRecenteTests
    {
        private static JsonElement Msg(string id, string texto) =>
            JsonSerializer.SerializeToElement(new { id, senderMessage = "PATIENT", message = texto, type = "TEXT" });

        private static string[] Ids(string consulta) =>
            ChatRecente.Listar(consulta).Select(m => m.GetProperty("id").GetString()).ToArray();

        [Fact]
        public void Listar_DevolveNaOrdemDeChegada()
        {
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta, Msg("a", "oi"));
            ChatRecente.Guardar(consulta, Msg("b", "tudo bem?"));

            Assert.Equal(new[] { "a", "b" }, Ids(consulta));
            Assert.Equal("tudo bem?", ChatRecente.Listar(consulta)[1].GetProperty("message").GetString());
        }

        [Fact]
        public void Guardar_MesmoIdDuasVezes_GuardaUmaSo()
        {
            // O portal reenvia o aviso de início (mesmo id) a cada reconexão do paciente.
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta, Msg("system-consult-started", "início"));
            ChatRecente.Guardar(consulta, Msg("system-consult-started", "início"));

            Assert.Single(ChatRecente.Listar(consulta));
        }

        [Fact]
        public void Consultas_NaoSeMisturam()
        {
            var a = Guid.NewGuid().ToString();
            var b = Guid.NewGuid().ToString();
            ChatRecente.Guardar(a, Msg("1", "da consulta A"));

            Assert.Empty(ChatRecente.Listar(b));
        }

        [Fact]
        public void IdDaConsulta_IgnoraMaiusculas()
        {
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta.ToUpperInvariant(), Msg("1", "oi"));

            Assert.Single(ChatRecente.Listar(consulta));
        }

        [Fact]
        public void Encerrar_ApagaAConversa()
        {
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta, Msg("1", "oi"));

            ChatRecente.Encerrar(consulta);

            Assert.Empty(ChatRecente.Listar(consulta));
        }

        [Fact]
        public void AnexoEmBase64_EntraNoHistorico()
        {
            // /chat/upload devolve data URL: a foto viaja dentro da mensagem.
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta, Msg("foto", new string('x', 3 * 1024 * 1024)));

            Assert.Equal(new[] { "foto" }, Ids(consulta));
        }

        [Fact]
        public void MaiorQueOOrcamento_SoVaiAoVivo()
        {
            var consulta = Guid.NewGuid().ToString();
            ChatRecente.Guardar(consulta, Msg("1", "texto"));
            ChatRecente.Guardar(consulta, Msg("enorme", new string('x', ChatRecente.OrcamentoBytes + 1)));

            Assert.Equal(new[] { "1" }, Ids(consulta));
        }

        [Fact]
        public void OrcamentoEstourado_DescartaAsMaisAntigas()
        {
            var consulta = Guid.NewGuid().ToString();
            var quatroMb = new string('x', 4 * 1024 * 1024);
            ChatRecente.Guardar(consulta, Msg("a", quatroMb));
            ChatRecente.Guardar(consulta, Msg("b", quatroMb));
            ChatRecente.Guardar(consulta, Msg("c", quatroMb));

            Assert.Equal(new[] { "b", "c" }, Ids(consulta));
            ChatRecente.Guardar(consulta, Msg("a", "o id 'a' saiu e pode voltar"));
            Assert.Equal(new[] { "b", "c", "a" }, Ids(consulta));
        }

        [Fact]
        public void Limite_DescartaAsMaisAntigas()
        {
            var consulta = Guid.NewGuid().ToString();
            for (var i = 0; i < ChatRecente.Limite + 5; i++)
                ChatRecente.Guardar(consulta, Msg(i.ToString(), "m"));

            var ids = Ids(consulta);
            Assert.Equal(ChatRecente.Limite, ids.Length);
            Assert.Equal("5", ids[0]);
        }

        [Fact]
        public void ConsultaSemId_NaoQuebra()
        {
            Assert.Empty(ChatRecente.Listar(null));
            ChatRecente.Guardar(null, Msg("1", "oi"));
            ChatRecente.Encerrar(null);
        }
    }
}
