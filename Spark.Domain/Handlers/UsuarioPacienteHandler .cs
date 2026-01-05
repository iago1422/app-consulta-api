using System.Threading.Tasks;
using Flunt.Notifications;
using Spark.Domain.Commands;
using Spark.Domain.Handlers.Contracts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Handlers
{
    public class UsuarioPacienteHandler : Notifiable
    {
        private readonly ICriarVinculoPacienteRepository _repository;

        public UsuarioPacienteHandler(ICriarVinculoPacienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<GenericCommandResult> Handle(CriarVinculoUsuarioPaciente.Request command)
        {
            command.Validate();
            if (command.Invalid)
                return new GenericCommandResult(false, "Dados inválidos", command.Notifications);

            var success = await _repository.CriarVinculoPaciente(
                command.UsuarioLogadoId,
                command.PacienteId,
                command.TipoVinculo,
                command.IsResponsavelLegal
            );

            return success
                ? new GenericCommandResult(true, "Vínculo criado com sucesso", null)
                : new GenericCommandResult(false, "Erro ao criar vínculo", null);
        }
    }
}
