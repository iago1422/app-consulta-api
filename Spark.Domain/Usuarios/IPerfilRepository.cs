using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities.Usuarios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spark.Domain.Repositories
{
    public interface IPerfilRepository
    {
        Perfil GetById(Guid id);
        Task <List<Perfil>> GetAll(int skip, int take);
    }
}