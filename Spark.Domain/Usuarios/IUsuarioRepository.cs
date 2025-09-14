using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities.Usuarios;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Spark.Domain.Repositories
{
    public interface IUsuarioRepository
    {
        Task<CriarUsuario.Response> Create(CriarUsuario.Request usuario);
        Task<AtualizarUsuario.Response> Update(AtualizarUsuario.Request usuario);
        Usuario GetById(Guid id);
        List<Usuario> GetAll();
    }
}