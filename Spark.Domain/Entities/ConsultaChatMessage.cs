using Spark.Domain.Entities.Usuarios;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spark.Domain.Entities
{
    public class ConsultaChatMessage : Entity
    {
        // Usuário
        public Guid UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; }
        public string Message { get; set; }

        // Arquivo anexo (nullable - apenas quando type != TEXT)
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
        public string MimeType { get; set; }
        public string FileType { get; set; } // "TEXT", "IMAGE", "VIDEO", "FILE"

        // Datas sistema
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
