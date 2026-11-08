using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;

namespace Spark.Tests
{
    /// <summary>
    /// DbContext simplificado para testes InMemory.
    /// O InMemory provider nao suporta List(string) nativamente
    /// (o Npgsql do Postgres mapeia como text[]), entao usamos
    /// value converters para serializar como JSON string.
    /// </summary>
    public class TestDbContext : DataContext
    {
        public TestDbContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ignora todas as entidades do ASP.NET Identity
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>();
            modelBuilder.Ignore<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>();

            // Converter para List<string> => JSON string (InMemory nao suporta text[])
            var listConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
            );

            modelBuilder.Entity<FichaClinica>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Ignore(e => e.Usuario);

                // Converte todas as propriedades List<string> para JSON
                entity.Property(e => e.Tendencias).HasConversion(listConverter);
                entity.Property(e => e.Telefones).HasConversion(listConverter);
                entity.Property(e => e.Alergias).HasConversion(listConverter);
                entity.Property(e => e.DoencasCronicas).HasConversion(listConverter);
                entity.Property(e => e.MedicacoesUsoContinuo).HasConversion(listConverter);
                entity.Property(e => e.HistoricoMedico).HasConversion(listConverter);
            });

            // Ignora as demais entidades que dependem de Identity/Usuarios
            modelBuilder.Ignore<Spark.Domain.Entities.Usuarios.Usuario>();
            modelBuilder.Ignore<Spark.Domain.Entities.Usuarios.Perfil>();
            modelBuilder.Ignore<ContaCredito>();
            modelBuilder.Ignore<MovimentacaoCredito>();
            modelBuilder.Ignore<Imagem>();
            modelBuilder.Ignore<Chamada>();
            modelBuilder.Ignore<Conexao>();
            modelBuilder.Ignore<Anamnese>();
            modelBuilder.Ignore<Consulta>();
            modelBuilder.Ignore<ConsultaChatMessage>();
            modelBuilder.Ignore<FilaAtendimento>();
            modelBuilder.Ignore<UsuarioPaciente>();
        }
    }
}
