using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Spark.Domain.Commands.Usuario;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Repositories;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Spark.Infra.Repositories.Services;

namespace Spark.Domain.Infra.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<Usuario> _userManager;
        private IConfiguration _configuration;
        private IMailService _mailService;
        private readonly IPerfilRepository _perfilRepository;

        public UsuarioRepository(DataContext context, IMapper mapper, UserManager<Usuario> userManager, IMailService mailService, IConfiguration configuration,
            IPerfilRepository perfilRepository)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _mailService = mailService;
            _configuration = configuration;
            _perfilRepository = perfilRepository;
        }

        public async Task<CriarUsuario.Response> Create(CriarUsuario.Request DTO)
        {
            
            
            var response = new CriarUsuario.Response();
            var requestBanco = new CriarUsuario.RequestBanco();

            try
            {
                var emailExist = await _userManager.FindByEmailAsync(DTO.Email);
                
                if (emailExist != null)
                {
                    response.Sucess = true;
                    response.Mensagem = "Esse email já está cadastrado!";

                    return response;
                }                     

                var perfils = await _perfilRepository.GetAll(0, 25);
                
                foreach(var perfil in perfils)
                {
                    if (DTO.Perfil == perfil.Name)
                    {
                        requestBanco.PerfilId = perfil.Id;
                        requestBanco.Nome = DTO.Nome;
                        requestBanco.Senha = DTO.Senha;
                        requestBanco.Email = DTO.Email;
                        requestBanco.Cpf = DTO.Cpf;
                        requestBanco.Imagem = DTO.Imagem;
                    }
                }
                var entidade = _mapper.Map<Usuario>(requestBanco);
                var result = await _userManager.CreateAsync(entidade, DTO.Senha);

                if (result.Succeeded)
                {
                    var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(entidade);

                    var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
                    var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

                    string url = $"{_configuration["AppUrl"]}/autenticar/confirmaremail?userid={entidade.Id}&token={validEmailToken}"; //TODO: CRIAR ROTA NO FRONT COM TELA BONITA

                    await _mailService.SendEmailAsync(entidade.Email, "Confime seu email", "Confime seu email", $"<h1> Confirmação de email</h1>" +
                        $"<p>Por favor confirme seu email clicando no link -> <a href='{url}'>Clique AQUI</a></p>");

                    response.Sucess = result.Succeeded;
                    response.Mensagem = "Usuario criado com sucesso!";
                    return response;
                }

                if(!result.Succeeded)
                {
                    response.Sucess = false;
                    response.Erro = "Erro ao tentar criar usuario.";
                    return response;
                }
            }
            catch(Exception ex)
            {
                response.Mensagem = "Erro ao tentar criar usuario!";
                response.Erro = ex.ToString();
            }
            return response;

        }

        public Usuario GetById(Guid id)
        {
            return _context
                .Usuarios.AsNoTracking()
                .FirstOrDefault(x => x.Id == id);
        }

        public List<Usuario> GetAll()
        {
            return _context.Usuarios
               .AsNoTracking().ToList();
        }

        public async Task<AtualizarUsuario.Response> Update(AtualizarUsuario.Request DTO)
        {
            AtualizarUsuario.Response response = new AtualizarUsuario.Response();

            try 
            {
                var user = await _userManager.FindByIdAsync(DTO.Id.ToString());
                
                if(!string.IsNullOrEmpty(DTO.Email))
                {
                    user.Email = DTO.Email;
                }

                if (!string.IsNullOrEmpty(DTO.Nome))
                {
                    user.Nome = DTO.Nome;
                }

                if (!string.IsNullOrEmpty(DTO.Imagem))
                {
                    user.Imagem = DTO.Imagem;
                }
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    response.Sucess = result.Succeeded;
                    response.Mensagem = "Usuario alterado com sucesso!";
                }                

                return response;
            }
            catch(Exception ex)
            {
                response.Mensagem = "Erro ao tentar alterar usuario!";
                response.Erro = ex.ToString();
            }

            return response;
        }
    }
}