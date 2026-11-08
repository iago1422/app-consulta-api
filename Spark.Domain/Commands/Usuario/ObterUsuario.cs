using System;

namespace Spark.Domain.Commands.Usuario
{
    public class ObterUsuario
    {
        // Dados públicos do usuário. Nunca devolver a entidade Usuario (IdentityUser):
        // ela expõe PasswordHash, SecurityStamp e ConcurrencyStamp.
        public class Response
        {
            public Guid Id { get; set; }
            public string Nome { get; set; }
            public string Email { get; set; }
            public string Cpf { get; set; }
            public string Imagem { get; set; }
            public string PhoneNumber { get; set; }
            public Guid PerfilId { get; set; }
        }
    }
}
