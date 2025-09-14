using Flunt.Notifications;
using Microsoft.AspNetCore.Http;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Contracts;
using Spark.Domain.Entities;
using Spark.Domain.Handlers.Contracts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Handlers.Imagem
{
    public class ImagemHandler :
        Notifiable,
        IHandler<DeletarImagem.Request>
    {
        private readonly IImagemRepository _repository;

        public ImagemHandler(IImagemRepository repository)
        {
            _repository = repository;
        }

        public ICommandResult Handle(CriarImagem.Request command, IFormFile File)
        {
            command.Validate();
            if (command.Invalid)
                return new GenericCommandResult(false, "Ops, parece que sua Imagem está incorreta!", command.Notifications);

            _repository.Create(command, File);

            return new GenericCommandResult(true, "Imagem Criada", command);
        }

        public ICommandResult Handle(DeletarImagem.Request command)
        {
            command.Validate();
            if (command.Invalid)
                return new GenericCommandResult(false, "Ops, parece que sua Despesa não existe ou já foi removida!", command.Notifications);

            _repository.Delete(command);

            return new GenericCommandResult(true, "Imagem Removida", command);
        }

        public ICommandResult Handle(CriarImagemUsuario.Request command, IFormFile file)
        {

            command.Validate();
            if (command.Invalid)
                return new GenericCommandResult(false, "Ops, parece que sua Imagem está incorreta!", command.Notifications);

            _repository.UploadImageToS3(command, file);

            return new GenericCommandResult(true, "Imagem Criada", command);
        }

        //public ICommandResult Handle(DeletarImagemUsuario.Request command)
        //{
        //    command.Validate();
        //    if (command.Invalid)
        //        return new GenericCommandResult(false, "Ops, parece que sua Despesa não existe ou já foi removida!", command.Notifications);

        //    _repository.DeleteImagemUsuario(command);

        //    return new GenericCommandResult(true, "Imagem Removida", command);
        //}

    }
}