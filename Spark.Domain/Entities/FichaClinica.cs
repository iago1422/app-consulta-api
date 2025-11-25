using Spark.Domain.Entities.Usuarios;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class FichaClinica : Entity
    {
        // Usuário
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public Usuario Usuario { get; set; }

        /// <summary>
        /// Tendências clínicas (ex: "Ansiedade", "Tabagismo", etc.).
        /// </summary>
        public List<string> Tendencias { get; set; } = new();

        /// <summary>
        /// Nome completo do paciente.
        /// </summary>
        public string NomeCompleto { get; set; }

        /// <summary>
        /// Data de nascimento.
        /// </summary>
        public DateTime? DataNascimento { get; set; }

        /// <summary>
        /// Sexo biológico.
        /// </summary>
        public string Sexo { get; set; }

        /// <summary>
        /// CPF do paciente.
        /// </summary>
        public string CPF { get; set; }

        /// <summary>
        /// Cartão do SUS (se existir).
        /// </summary>
        public string CartaoSUS { get; set; }

        /// <summary>
        /// Endereço completo.
        /// </summary>
        public string Endereco { get; set; }

        /// <summary>
        /// Telefones de contato.
        /// </summary>
        public List<string> Telefones { get; set; } = new();

        /// <summary>
        /// Contato de emergência (nome, telefone e relação).
        /// </summary>
        public string ContatoEmergenciaNome { get; set; }
        public string ContatoEmergenciaTelefone { get; set; }
        public string ContatoEmergenciaRelacao { get; set; }

        /// <summary>
        /// Alergias diversas.
        /// </summary>
        public List<string> Alergias { get; set; } = new();

        /// <summary>
        /// Doenças crônicas ou condições pré-existentes.
        /// </summary>
        public List<string> DoencasCronicas { get; set; } = new();

        /// <summary>
        /// Histórico cirúrgico relevante.
        /// </summary>
        public string HistoricoCirurgico { get; set; }

        /// <summary>
        /// Medicações de uso contínuo.
        /// </summary>
        public List<string> MedicacoesUsoContinuo { get; set; } = new();

        /// <summary>
        /// Histórico familiar relevante.
        /// </summary>
        public string HistoricoFamiliar { get; set; }

        /// <summary>
        /// Data de criação da conta de créditos.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização de saldo.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
