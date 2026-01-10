using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MercadoPago.Client.Preference;

namespace Spark.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "acb3830a-402b-4865-9f70-7b28d39f66ad")]
    [Route("checkout")]
    public class CheckoutController : ControllerBase
    {
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ILogger<CheckoutController> logger)
        {
            _logger = logger;
        }

        // POST /checkout/create
        [HttpPost("create")]
        public async Task<IActionResult> CreatePreference([FromBody] CreateOrderDto dto)
        {
            _logger.LogInformation("[MP] CreatePreference chamado");

            if (dto == null)
            {
                _logger.LogWarning("[MP] DTO nulo");
                return BadRequest(new { Message = "Body inválido." });
            }

            if (dto.UserId == Guid.Empty || dto.Creditos <= 0)
            {
                _logger.LogWarning("[MP] Dados inválidos. UserId={UserId} Creditos={Creditos}", dto.UserId, dto.Creditos);
                return BadRequest(new { Message = "UserId e Creditos são obrigatórios." });
            }

            if (dto.UnitPrice <= 0)
            {
                _logger.LogWarning("[MP] UnitPrice inválido. UnitPrice={UnitPrice}", dto.UnitPrice);
                return BadRequest(new { Message = "UnitPrice deve ser maior que zero." });
            }

            var client = new PreferenceClient();

            // ExternalReference em formato fácil de parsear no webhook
            // Ex: uid:GUID|cred:50|ord:ABC123
            var externalReference = $"uid:{dto.UserId}|cred:{dto.Creditos}|ord:{dto.OrderId}";

            _logger.LogInformation("[MP] Criando preference. OrderId={OrderId} UserId={UserId} Creditos={Creditos} ExternalReference={ExternalReference}",
                dto.OrderId, dto.UserId, dto.Creditos, externalReference);

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
                {
                    new PreferenceItemRequest
                    {
                        Title = string.IsNullOrWhiteSpace(dto.Title) ? "Pedido" : dto.Title,
                        Quantity = dto.Quantity <= 0 ? 1 : dto.Quantity,
                        UnitPrice = dto.UnitPrice,
                        CurrencyId = "BRL"
                    }
                },

                ExternalReference = externalReference,

                // Metadados recomendados: o Payment tende a carregar isso e facilita sua vida no webhook
                Metadata = new Dictionary<string, object>
                {
                    ["userId"] = dto.UserId.ToString(),
                    ["creditos"] = dto.Creditos.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["orderId"] = dto.OrderId ?? string.Empty
                },

                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "myapp://checkout/success",
                    Failure = "myapp://checkout/failure",
                    Pending = "myapp://checkout/pending"
                },

                AutoReturn = "approved",

                NotificationUrl = "https://yong-segregational-zahra.ngrok-free.dev/api/webhooks/mercadopago"
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
        decimal Creditos
    );
}
