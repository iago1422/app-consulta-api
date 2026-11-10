using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MercadoPago.Client.Preference;
using Spark.Domain.Configuracao;
using Spark.Domain.Repositories;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad,efe703c2-2076-4d5b-aa04-aaee90953e49,e0e938ef-5ca3-48bc-b5a5-30a0fd0b64ca")]
    [Route("checkout")]
    public class CheckoutController : ControllerBase
    {
        // Piso de R$/crédito (também usado no webhook). Fonte: ConfigConsultas.
        public const decimal PrecoMinimoPorCredito = ConfigConsultas.PrecoMinimoPorCredito;

        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ILogger<CheckoutController> logger)
        {
            _logger = logger;
        }

        // POST /checkout/create
        [HttpPost("create")]
        public async Task<IActionResult> CreatePreference([FromBody] CreateOrderDto dto, [FromServices] IConfiguracaoRepository configRepo)
        {
            _logger.LogInformation("[MP] CreatePreference chamado");

            if (dto == null)
            {
                _logger.LogWarning("[MP] DTO nulo");
                return BadRequest(new { Message = "Body inválido." });
            }

            if (dto.UserId == Guid.Empty)
            {
                _logger.LogWarning("[MP] UserId vazio");
                return BadRequest(new { Message = "UserId e Creditos são obrigatórios." });
            }

            var quantidadeCliente = dto.Quantity <= 0 ? 1 : dto.Quantity;
            var config = (await configRepo.ObterConsultas()).Config;
            var (pedido, erro) = ResolvedorPedido.Resolver(dto.PacoteId, dto.Creditos, dto.UnitPrice * quantidadeCliente, dto.Title, config);
            if (erro != null)
            {
                _logger.LogWarning("[MP] Pedido recusado: {Erro}. PacoteId={PacoteId} UnitPrice={UnitPrice} Creditos={Creditos}", erro, dto.PacoteId, dto.UnitPrice, dto.Creditos);
                return BadRequest(new { Message = erro });
            }

            var client = new PreferenceClient();

            // ExternalReference em formato fácil de parsear no webhook
            // Ex: uid:GUID|cred:50|ord:ABC123
            var externalReference = $"uid:{dto.UserId}|cred:{pedido.Creditos}|ord:{dto.OrderId}";

            _logger.LogInformation("[MP] Criando preference. OrderId={OrderId} UserId={UserId} Creditos={Creditos} PrecoTotal={PrecoTotal} ExternalReference={ExternalReference}",
                dto.OrderId, dto.UserId, pedido.Creditos, pedido.PrecoTotal, externalReference);

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = pedido.Titulo,
                        Quantity = 1,
                        UnitPrice = pedido.PrecoTotal,
                        CurrencyId = "BRL"
                    }
                },

                ExternalReference = externalReference,

                // Metadados recomendados: o Payment tende a carregar isso e facilita sua vida no webhook
                Metadata = new Dictionary<string, object>
                {
                    ["userId"] = dto.UserId.ToString(),
                    ["creditos"] = pedido.Creditos.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["orderId"] = dto.OrderId ?? string.Empty
                },

                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "iconsulta://checkout/success",
                    Failure = "iconsulta://checkout/failure",
                    Pending = "iconsulta://checkout/pending"
                },

                AutoReturn = "approved",

                NotificationUrl = "https://app-consulta-api.onrender.com/api/webhooks/mercadopago"
            };

            try
            {
                var pref = await client.CreateAsync(request);

                _logger.LogInformation("[MP] Preference criada com sucesso. PreferenceId={PreferenceId} InitPoint={InitPoint}",
                    pref.Id, pref.InitPoint);

                return Ok(new
                {
                    preferenceId = pref.Id,
                    initPoint = pref.InitPoint,
                    sandboxInitPoint = pref.SandboxInitPoint
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MP] Erro ao criar preference");
                return StatusCode(500, new { Message = "Erro ao criar preferência no Mercado Pago." });
            }
        }
    }

    public record CreateOrderDto(
        string OrderId,
        string Title,
        int Quantity,
        decimal UnitPrice,
        Guid UserId,
        decimal Creditos,
        string PacoteId = null // app ≥ 1.3.10
    );
}
