using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Infra.Repositories;
using Xunit;

namespace Spark.Tests
{
    public class FichaClinicaRepositoryTests : IDisposable
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly FichaClinicaRepository _repository;

        public FichaClinicaRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TestDbContext(options);
            _context.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<FichaClinica, CriarFichaClinica.Request>().ReverseMap();
            });
            _mapper = config.CreateMapper();

            _repository = new FichaClinicaRepository(_context, _mapper);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private FichaClinica CriarFichaExemplo(Guid? userId = null)
        {
            return new FichaClinica
            {
                UserId = userId ?? Guid.NewGuid(),
                NomeCompleto = "Paciente Original",
                DataNascimento = new DateTime(1990, 5, 15),
                Sexo = "masculino",
                CPF = "12345678901",
                CartaoSUS = "SUS-001",
                Endereco = "Rua Original, 100",
                Telefones = new List<string> { "11999990000" },
                ContatoEmergenciaNome = "Maria Original",
                ContatoEmergenciaTelefone = "11988880000",
                ContatoEmergenciaRelacao = "Mae",
                Alergias = new List<string> { "Dipirona" },
                DoencasCronicas = new List<string> { "Asma" },
                HistoricoCirurgico = "Apendicectomia 2015",
                MedicacoesUsoContinuo = new List<string> { "Losartana" },
                HistoricoFamiliar = "Diabetes na familia",
                Tendencias = new List<string> { "Ansiedade" },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
        }

        // ============================================================
        // TESTES DE UPDATE BY ID
        // ============================================================

        [Fact]
        public async Task UpdateById_DeveAtualizarCamposCorretamente()
        {
            // Arrange
            var ficha = CriarFichaExemplo();
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var fichaId = ficha.Id;
            var originalUserId = ficha.UserId;

            var dto = new CriarFichaClinica.Request
            {
                UserId = originalUserId,
                NomeCompleto = "Paciente Atualizado",
                Sexo = "feminino",
                CPF = "98765432100",
                DataNascimento = new DateTime(1995, 10, 20),
                CartaoSUS = "SUS-002",
                Endereco = "Rua Nova, 200",
                Telefones = new List<string> { "11977770000" },
                ContatoEmergenciaNome = "Joao Atualizado",
                ContatoEmergenciaTelefone = "11966660000",
                ContatoEmergenciaRelacao = "Pai",
                Alergias = new List<string> { "Lactose", "Gluten" },
                DoencasCronicas = new List<string> { "Hipertensao" },
                HistoricoCirurgico = "Cirurgia cardiaca 2020",
                MedicacoesUsoContinuo = new List<string> { "Omeprazol", "Metformina" },
                HistoricoFamiliar = "Historico atualizado",
                Tendencias = new List<string> { "Tabagismo" },
            };

            // Act
            var result = await _repository.UpdateById(fichaId, dto);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");
            Assert.Equal(fichaId, result.Id);

            var fichaAtualizada = await _context.FichaClinicas.FindAsync(fichaId);
            Assert.NotNull(fichaAtualizada);
            Assert.Equal("Paciente Atualizado", fichaAtualizada.NomeCompleto);
            Assert.Equal("feminino", fichaAtualizada.Sexo);
            Assert.Equal("98765432100", fichaAtualizada.CPF);
            Assert.Equal("Rua Nova, 200", fichaAtualizada.Endereco);
            Assert.Equal("Joao Atualizado", fichaAtualizada.ContatoEmergenciaNome);
            Assert.Contains("Lactose", fichaAtualizada.Alergias);
            Assert.Contains("Gluten", fichaAtualizada.Alergias);
            Assert.Equal("Cirurgia cardiaca 2020", fichaAtualizada.HistoricoCirurgico);
            Assert.Equal(2, fichaAtualizada.MedicacoesUsoContinuo.Count);
        }

        [Fact]
        public async Task UpdateById_NaoDeveAlterarIdNemUserIdNemCreatedAt()
        {
            // Arrange
            var ficha = CriarFichaExemplo();
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var fichaId = ficha.Id;
            var originalUserId = ficha.UserId;
            var originalCreatedAt = ficha.CreatedAt;

            var dto = new CriarFichaClinica.Request
            {
                UserId = Guid.NewGuid(), // Tenta mudar o UserId — deve ser ignorado
                NomeCompleto = "Nome Alterado",
                Sexo = "feminino",
                CPF = "99999999999",
                Tendencias = new List<string>(),
            };

            // Act
            var result = await _repository.UpdateById(fichaId, dto);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");

            var fichaAtualizada = await _context.FichaClinicas.FindAsync(fichaId);
            Assert.NotNull(fichaAtualizada);
            // Id e UserId devem permanecer inalterados
            Assert.Equal(fichaId, fichaAtualizada.Id);
            Assert.Equal(originalUserId, fichaAtualizada.UserId);
            // CreatedAt deve permanecer inalterado
            Assert.Equal(originalCreatedAt, fichaAtualizada.CreatedAt);
        }

        [Fact]
        public async Task UpdateById_DeveAtualizarUpdatedAt()
        {
            // Arrange
            var ficha = CriarFichaExemplo();
            ficha.UpdatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var fichaId = ficha.Id;
            var updateAnterior = ficha.UpdatedAt;

            var dto = new CriarFichaClinica.Request
            {
                UserId = ficha.UserId,
                NomeCompleto = "Update timestamp",
                Sexo = "masculino",
                CPF = "12345678901",
                Tendencias = new List<string>(),
            };

            // Act
            var result = await _repository.UpdateById(fichaId, dto);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");

            var fichaAtualizada = await _context.FichaClinicas.FindAsync(fichaId);
            Assert.True(fichaAtualizada.UpdatedAt > updateAnterior);
        }

        [Fact]
        public async Task UpdateById_FichaInexistente_DeveRetornarErro()
        {
            // Arrange
            var fichaIdInexistente = Guid.NewGuid();
            var dto = new CriarFichaClinica.Request
            {
                NomeCompleto = "Teste",
                Sexo = "masculino",
                CPF = "12345678901",
            };

            // Act
            var result = await _repository.UpdateById(fichaIdInexistente, dto);

            // Assert
            Assert.False(result.Sucess);
            Assert.NotNull(result.Erro);
            Assert.NotEmpty(result.Erro);
        }

        [Fact]
        public async Task UpdateById_ComCamposNull_DeveManterValoresOriginais()
        {
            // Arrange
            var ficha = CriarFichaExemplo();
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var fichaId = ficha.Id;

            // DTO com varios campos null — devem manter o original
            var dto = new CriarFichaClinica.Request
            {
                UserId = ficha.UserId,
                NomeCompleto = "Nome Novo",
                Sexo = null,
                CPF = null,
                CartaoSUS = null,
                Endereco = null,
                Tendencias = null,
                Telefones = null,
                Alergias = null,
            };

            // Act
            var result = await _repository.UpdateById(fichaId, dto);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");

            var fichaAtualizada = await _context.FichaClinicas.FindAsync(fichaId);
            Assert.Equal("Nome Novo", fichaAtualizada.NomeCompleto);
            // Campos que eram null no DTO devem manter os valores originais
            Assert.Equal("masculino", fichaAtualizada.Sexo);
            Assert.Equal("12345678901", fichaAtualizada.CPF);
            Assert.Equal("SUS-001", fichaAtualizada.CartaoSUS);
            Assert.Equal("Rua Original, 100", fichaAtualizada.Endereco);
            Assert.Single(fichaAtualizada.Tendencias);
            Assert.Contains("Ansiedade", fichaAtualizada.Tendencias);
            Assert.Single(fichaAtualizada.Telefones);
            Assert.Single(fichaAtualizada.Alergias);
        }

        [Fact]
        public async Task UpdateById_SoComTendencias_NaoApagaAlergiasNemHistoricoMedico()
        {
            // Arrange: portal manda corpo parcial ({tendencias}) depois da consulta
            var ficha = CriarFichaExemplo();
            ficha.HistoricoMedico = new List<string> { "CID: J00 | Tratamento: repouso" };
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var dto = new CriarFichaClinica.Request { Tendencias = new List<string> { "Pontual" } };

            // Act
            var result = await _repository.UpdateById(ficha.Id, dto);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");
            var fichaAtualizada = await _context.FichaClinicas.FindAsync(ficha.Id);
            Assert.Equal(new List<string> { "Pontual" }, fichaAtualizada.Tendencias);
            Assert.Equal(new List<string> { "Dipirona" }, fichaAtualizada.Alergias);
            Assert.Equal(new List<string> { "Losartana" }, fichaAtualizada.MedicacoesUsoContinuo);
            Assert.Single(fichaAtualizada.HistoricoMedico);
        }

        [Fact]
        public async Task Create_SemListas_GravaListasVaziasENaoNull()
        {
            var dto = new CriarFichaClinica.Request
            {
                UserId = Guid.NewGuid(),
                NomeCompleto = "Paciente Novo",
                Sexo = "feminino",
                CPF = "12345678901",
            };

            var result = await _repository.Create(dto);

            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");
            var ficha = await _context.FichaClinicas.FindAsync(result.Id);
            Assert.NotNull(ficha.Tendencias);
            Assert.Empty(ficha.Tendencias);
            Assert.NotNull(ficha.HistoricoMedico);
            Assert.Empty(ficha.Alergias);
            Assert.Empty(ficha.Telefones);
            Assert.Empty(ficha.DoencasCronicas);
            Assert.Empty(ficha.MedicacoesUsoContinuo);
        }

        // ============================================================
        // TESTES DE DELETE BY ID
        // ============================================================

        [Fact]
        public async Task DeleteById_DeveRemoverFichaComSucesso()
        {
            // Arrange
            var ficha = CriarFichaExemplo();
            _context.FichaClinicas.Add(ficha);
            await _context.SaveChangesAsync();

            var fichaId = ficha.Id;
            Assert.Equal(1, await _context.FichaClinicas.CountAsync());

            // Act
            var result = await _repository.DeleteById(fichaId);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");
            Assert.Equal(fichaId, result.Id);
            Assert.Equal(0, await _context.FichaClinicas.CountAsync());
        }

        [Fact]
        public async Task DeleteById_FichaInexistente_DeveRetornarErro()
        {
            // Arrange
            var fichaIdInexistente = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteById(fichaIdInexistente);

            // Assert
            Assert.False(result.Sucess);
            Assert.NotNull(result.Erro);
            Assert.NotEmpty(result.Erro);
        }

        [Fact]
        public async Task DeleteById_NaoDeveAfetarOutrasFichas()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var ficha1 = CriarFichaExemplo(userId);
            var ficha2 = CriarFichaExemplo(userId);
            ficha2.NomeCompleto = "Segundo Paciente";

            _context.FichaClinicas.AddRange(ficha1, ficha2);
            await _context.SaveChangesAsync();

            Assert.Equal(2, await _context.FichaClinicas.CountAsync());

            // Act
            var result = await _repository.DeleteById(ficha1.Id);

            // Assert
            Assert.True(result.Sucess, $"Esperado sucesso, mas recebeu erro: {result.Erro}");
            Assert.Equal(1, await _context.FichaClinicas.CountAsync());

            var fichaRestante = await _context.FichaClinicas.FirstAsync();
            Assert.Equal("Segundo Paciente", fichaRestante.NomeCompleto);
            Assert.Equal(ficha2.Id, fichaRestante.Id);
        }
    }
}
