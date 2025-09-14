using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Infra.Repositories
{
    public class PerfilRepository : IPerfilRepository
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;
        public PerfilRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Perfil GetById(Guid id)
        {
            return _context
                .Perfils.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

        public async Task<List<Perfil>> GetAll( int skip = 0, int take = 25)
        {
             var lista = await _context
                .Perfils
                .AsNoTracking()
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return lista;
        }

    }
}