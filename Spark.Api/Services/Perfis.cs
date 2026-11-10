namespace Spark.Api.Services
{
    /// <summary>
    /// Ids dos perfis (AspNetRoles) conferidos em PRODUÇÃO em 2026-11-10 (GET /perfil/0/25).
    /// O claim role do token é o PerfilId. Atenção: bancos locais antigos podem ter ids trocados.
    /// </summary>
    public static class Perfis
    {
        public const string Administrador = "acb3830a-402b-4865-9f70-7b28d39f66ad";
        public const string Medico = "efe703c2-2076-4d5b-aa04-aaee90953e49";
        public const string Paciente = "e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca";

        /// <summary>Quem acessa o painel: administrador e médico (nunca paciente).</summary>
        public const string Equipe = Administrador + "," + Medico;
    }
}
