using Flunt.Notifications;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Contracts;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Handlers.Contracts;
using Spark.Domain.Repositories;
using System.Threading.Tasks;

namespace Spark.Domain.Handlers
{
    public class AnamneseHandler :
        Notifiable
        //IHandler<CriarUsuario.Request>,
        //IHandler<AtualizarUsuario.Request>
    {
        private readonly IAnamneseRepository _repository;

        public AnamneseHandler(IAnamneseRepository repository)
        {
            _repository = repository;
        }

        public async Task<GenericCommandResult> Handle(CriarAnamnese.Request command)
        {
            command.Validate();
            if (command.Invalid)
                return new GenericCommandResult(false, "Ops, parece que seu usuario está incorreto!", command.Notifications);

            var response = await _repository.Create(command);

            if (response.Sucess)
            {
                return new GenericCommandResult(response.Sucess, response.Mensagem, command);
            }

            return new GenericCommandResult(response.Sucess, response.Erro, command);
        }

     
    }  
}