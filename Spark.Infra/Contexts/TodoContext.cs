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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {         
            base.OnModelCreating(modelBuilder);

            // Conta de cr�dito � �nica por usu�rio
            modelBuilder.Entity<ContaCredito>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Movimenta��o com refer�ncia �nica (idempot�ncia)
            modelBuilder.Entity<MovimentacaoCredito>()
                .HasIndex(m => new { m.UserId, m.Tipo, m.Referencia })
                .IsUnique();
        }
    }
}