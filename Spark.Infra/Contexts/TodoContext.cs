using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;

namespace Spark.Domain.Infra.Contexts
{
    public class DataContext : IdentityDbContext<Usuario, Perfil, Guid>
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }
        public DbSet<ContaCredito> ContasCredito { get; set; }
        public DbSet<MovimentacaoCredito> MovimentacoesCredito { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Perfil> Perfils { get; set; }
        public DbSet<Imagem> Imagens { get; set; }      
        public DbSet<FichaClinica> FichaClinicas { get; set; }
        public DbSet<Chamada> Chamadas { get; set; }
        public DbSet<Conexao> Conexoes { get; set; }
        public DbSet<Anamnese> Anamneses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {         
            base.OnModelCreating(modelBuilder);

            // Conta de crédito é única por usuário
            modelBuilder.Entity<ContaCredito>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Movimentação com referência única (idempotência)
            modelBuilder.Entity<MovimentacaoCredito>()
                .HasIndex(m => new { m.UserId, m.Tipo, m.Referencia })
                .IsUnique();
        }
    }
}