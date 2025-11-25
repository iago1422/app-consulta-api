using System;
using System.Collections.Generic;
using Flunt.Notifications;
using Flunt.Validations;
using Spark.Domain.Commands.Contracts;

namespace Spark.Domain.Commands
{
    public class CriarFichaClinica
    {
        public class Request : Notifiable, ICommand
        {
            public Guid UserId { get; set; }

            // Tendências clínicas (ex: "Ansiedade", "Tabagismo", etc.)
            public List<string> Tendencias { get; set; } = new();

            // Dados básicos do paciente
            public string NomeCompleto { get; set; }
            public DateTime? DataNascimento { get; set; }
            public string Sexo { get; set; }
            public string CPF { get; set; }
            public string CartaoSUS { get; set; }
            public string Endereco { get; set; }

            // Telefones
            public List<string> Telefones { get; set; } = new();

            // Contato de emergência
            public string ContatoEmergenciaNome { get; set; }
            public string ContatoEmergenciaTelefone { get; set; }
            public string ContatoEmergenciaRelacao { get; set; }

            // Alergias
            public List<string> Alergias { get; set; } = new();

            // Doenças crônicas / condições pré-existentes
            public List<string> DoencasCronicas { get; set; } = new();

            // Histórico cirúrgico
            public string HistoricoCirurgico { get; set; }

            // Medicações de uso contínuo
            public List<string> MedicacoesUsoContinuo { get; set; } = new();

            // Histórico familiar
            public string HistoricoFamiliar { get; set; }

            public void Validate()
            {
                var contract = new Contract()
                    .Requires()
                    // Guid não nulo/empty
                    .AreNotEquals(
                        Guid.Empty,
                        UserId,
                        nameof(UserId),
                        "UserId não pode ser vazio."
                    )
                    .IsNotNullOrEmpty(
                        NomeCompleto,
                        nameof(NomeCompleto),
                        "Nome completo não pode ser vazio ou nulo."
                    )
                    .IsNotNullOrEmpty(
                        Sexo,
                        nameof(Sexo),
                        "Sexo não pode ser vazio ou nulo."
                    )
                    .IsNotNullOrEmpty(
                        Endereco,
                        nameof(Endereco),
                        "Endereço não pode ser vazio ou nulo."
                    )
                    .IsNotNullOrEmpty(
                        ContatoEmergenciaNome,
                        nameof(ContatoEmergenciaNome),
                        "Nome do contato de emergência não pode ser vazio ou nulo."
                    )
                    .IsNotNullOrEmpty(
                        ContatoEmergenciaTelefone,
                        nameof(ContatoEmergenciaTelefone),
                        "Telefone do contato de emergência não pode ser vazio ou nulo."
                    );

                // Validação simples de CPF (opcional, só se vier preenchido)
                if (!string.IsNullOrWhiteSpace(CPF))
                {
                    contract.HasMinLen(CPF, 11, nameof(CPF), "CPF deve ter pelo menos 11 caracteres.")
                            .HasMaxLen(CPF, 14, nameof(CPF), "CPF não deve exceder 14 caracteres (com máscara).");
                }

               
                if (Telefones == null || Telefones.Count == 0)
                {
                    contract.AddNotification(nameof(Telefones), "Informe pelo menos um telefone de contato.");
                }

                AddNotifications(contract);
            }
        }

        public class Response
        {
            public Guid Id { get; set; }
            public bool Sucess { get; set; } = false;
            public string Mensagem { get; set; }
            public string Erro { get; set; }
        }
    }
}
