using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Api.Services;
using Spark.Domain.Entities;
using Xunit;

namespace Spark.Tests
{
    public class FiltroAtendimentosTests
    {
        private static readonly DateTime Agora = new DateTime(2026, 11, 9, 12, 0, 0);
        private static readonly Guid MedicoA = Guid.NewGuid();

        private static FilaAtendimento F(string status, int criadoHaDias, bool chamado, Guid? medico = null) => new FilaAtendimento
        {
            Id = Guid.NewGuid(),
            Status = status,
            CreatedAt = Agora.AddDays(-criadoHaDias),
            CalledAt = chamado ? Agora.AddDays(-criadoHaDias).AddMinutes(5) : null,
            MedicoId = medico
        };

        private static readonly List<FilaAtendimento> Base = new List<FilaAtendimento>
        {
            F("DONE", 1, true, MedicoA),
            F("LEFT", 2, true),
            F("LEFT", 3, false),
            F("DONE", 4, false),   // worker fechou alguém esquecido na fila
            F("CALLED", 0, true, MedicoA),
            F("WAITING", 0, false),
            F("DONE", 40, true),   // fora dos 30 dias
        };

        private static List<FilaAtendimento> Rodar(string status = null, Guid? medico = null) =>
            FiltroAtendimentos.Aplicar(Base.AsQueryable(), Agora.AddDays(-30), Agora.AddMinutes(1), medico, status).ToList();

        [Fact]
        public void Padrao_ExcluiAguardandoEForaDoPeriodo_OrdenaMaisRecentePrimeiro()
        {
            var r = Rodar();
            Assert.Equal(5, r.Count);
            Assert.DoesNotContain(r, f => f.Status == "WAITING");
            Assert.True(r.SequenceEqual(r.OrderByDescending(f => f.CreatedAt)));
        }

        [Theory]
        [InlineData("concluida", 1)]
        [InlineData("interrompida", 1)]
        [InlineData("desistiu", 2)]
        [InlineData("em_andamento", 1)]
        [InlineData("qualquer-coisa", 5)]
        public void FiltroDeStatus(string status, int esperado) => Assert.Equal(esperado, Rodar(status).Count);

        [Fact]
        public void FiltroDeMedico() => Assert.Equal(2, Rodar(medico: MedicoA).Count);

        [Theory]
        [InlineData("DONE", true, "concluida")]
        [InlineData("DONE", false, "desistiu")]
        [InlineData("LEFT", true, "interrompida")]
        [InlineData("LEFT", false, "desistiu")]
        [InlineData("CALLED", true, "em_andamento")]
        public void Status_Mapeia(string status, bool chamado, string esperado) =>
            Assert.Equal(esperado, FiltroAtendimentos.Status(F(status, 0, chamado)));
    }
}
