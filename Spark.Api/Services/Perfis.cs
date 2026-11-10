namespace Spark.Api.Services
{
    /// <summary>Ids dos perfis (AspNetRoles). O claim role do token é o PerfilId.</summary>
    public static class Perfis
    {
        public const string Paciente = "acb3830a-402b-4865-9f70-7b28d39f66ad";
        public const string Medico = "efe703c2-2076-4d5b-aa04-aaee90953e49";
        // Existe em produção e já era aceito em todas as rotas; não é o perfil do app do paciente.
        public const string Terceiro = "e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca";

        /// <summary>Quem acessa o painel: todos menos paciente.</summary>
        public const string Equipe = Medico + "," + Terceiro;
    }
}
