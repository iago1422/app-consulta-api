using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Spark.Domain.Entities;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;

namespace Spark.Domain.Infra.Repositories
{
    public class CriarVinculoPacienteRepository : ICriarVinculoPacienteRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public CriarVinculoPacienteRepository(DataContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
        }

        public async Task<bool> CriarVinculoPaciente(Guid responsavelId, Guid pacienteId, string tipoVinculo, bool isResponsavelLegal)
        {
            var existe = await _context.UsuarioPacientes
                .AnyAsync(x => x.ResponsavelId == responsavelId && x.PacienteId == pacienteId);

            if (existe) return true;

            var vinculo = new UsuarioPaciente
            {
                ResponsavelId = responsavelId,
                PacienteId = pacienteId,
                TipoVinculo = tipoVinculo,
                IsResponsavelLegal = isResponsavelLegal,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UsuarioPacientes.AddAsync(vinculo);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
