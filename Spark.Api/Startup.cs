using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Text;
using Spark.Api.Services;
using Spark.Domain.Entities.Usuarios;
using Spark.Domain.Handlers;
using Spark.Domain.Infra.Contexts;
using Spark.Domain.Infra.Repositories;
using Spark.Domain.Infra.Repositories.Autenticar;
using Spark.Domain.Repositories;
using Spark.Domain.Services;
using Spark.Infra.Repositories.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Spark.Domain.Handlers.Imagem;
using System.Text.Json.Serialization;
using System.Text.Json;
using Spark.Domain.Entities;
using Spark.Infra.Repositories;
using Spark.Domain.Repositories.Autenticar;

namespace Spark.Domain.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers();
            services.AddDbContext<DataContext>(options => options.UseNpgsql(Configuration.GetConnectionString("connectionString")));
            services.AddHealthChecks();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;


            });

            //services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<DataContext>();            

            services.AddIdentity<Usuario, Perfil>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();
     
            services.AddTransient<IUsuarioRepository, UsuarioRepository>();
            services.AddTransient<IAutenticarRepository, AutenticarRepository>();
            services.AddTransient<IMailService, SendGridEmailService>();
            services.AddTransient<IPerfilRepository, PerfilRepository>();
            services.AddTransient<IImagemRepository, ImagemRepository>();
            services.AddTransient<ICreditosRepository, CreditosRepository>();
            services.AddTransient<IAnamneseRepository, AnamneseRepository>();
            services.AddTransient<IFichaClinicaRepository, FichaClinicaRepository>();


            services.AddTransient<ITokenService, TokenService>();        
            services.AddTransient<UsuarioHandler, UsuarioHandler>();
            services.AddTransient<ImagemHandler, ImagemHandler>();
            services.AddTransient<CreditosHandler, CreditosHandler>();
            services.AddTransient<AnamneseHandler, AnamneseHandler>();
            services.AddTransient<FichaClinicaHandler, FichaClinicaHandler>();




            var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AuthenticationJWT:secretKey").Value);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddSwaggerGen(swagger =>
            {
                swagger.OperationFilter<SwaggerFileOperationFilter>();
                swagger.CustomSchemaIds(s => s.FullName.Replace("+", "."));

                //var xmlPath = Path.Combine(AppContext.BaseDirectory, "Spark.Api.xml");
                //var xmlModelPath = Path.Combine(AppContext.BaseDirectory, "Spark.Domain.xml");

                //swagger.IncludeXmlComments(xmlPath);
                //swagger.IncludeXmlComments(xmlModelPath);

                //This is to generate the Default UI of Swagger Documentation
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ASP.NET 6 Web API",
                    Description = "Authentication and Authorization in ASP.NET 6 with JWT and Swagger"
                });
                // To Enable authorization using Swagger (JWT)
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter ‘Bearer’ [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                new OpenApiSecurityScheme
                {
                Reference = new OpenApiReference
                {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
                }
                },
                new string[] {}
                }
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));

            app.UseCors(x => x
               .AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseHealthChecks("/health");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
