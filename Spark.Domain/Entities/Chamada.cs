using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class Chamada : Entity
    {
        /// <summary>
        /// Usuário que iniciou a chamada.
        /// </summary>
        public Guid FichaClinicaId { get; set; }

        [ForeignKey(nameof(FichaClinicaId))]
        public FichaClinica FichaClinica { get; set; }

        /// <summary>
        /// Usuário que iniciou a chamada.
        /// </summary>
        public Guid UsuarioOrigemId { get; set; }

        [ForeignKey(nameof(UsuarioOrigemId))]
        public Usuario UsuarioOrigem { get; set; }

        /// <summary>
        /// Usuário que recebeu a chamada.
        /// </summary>
        public Guid UsuarioDestinoId { get; set; }

        [ForeignKey(nameof(UsuarioDestinoId))]
        public Usuario UsuarioDestino { get; set; }

        /// <summary>
        /// Offer WebRTC (SDP) enviada pelo caller (JSON/SDP).
        /// </summary>
        public string Solicitacao { get; set; }

        /// <summary>
        /// Answer WebRTC (SDP) enviada pelo callee.
        /// </summary>
        public string Resposta { get; set; }

        /// <summary>
        /// Data de criação da chamada.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
