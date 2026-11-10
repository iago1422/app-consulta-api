using System;
using System.Security.Claims;

namespace Spark.Api.Services
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Id do usuário do token (claim nameid), ou null.</summary>
        public static Guid? UsuarioId(this ClaimsPrincipal user) =>
            Guid.TryParse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : (Guid?)null;
    }
}
