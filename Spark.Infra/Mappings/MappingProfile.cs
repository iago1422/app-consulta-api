using AutoMapper;
using Spark.Domain.Commands;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Spark.Domain.Infra.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Usuario, CriarUsuario.RequestBanco>()
                .ForMember(x => x.Email, opt => opt.MapFrom(s => s.UserName)).ReverseMap();
        }
    }
}
