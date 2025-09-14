using Spark.Domain.Commands.Contracts;
using System.Threading.Tasks;

namespace Spark.Domain.Handlers.Contracts
{
    public interface IHandler<T> where T : ICommand
    {
      ICommandResult Handle(T command);
    }
}