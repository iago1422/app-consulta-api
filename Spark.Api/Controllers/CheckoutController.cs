using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spark.Domain.Commands;
using Spark.Domain.Entities;
using Spark.Domain.Handlers;
using Spark.Domain.Handlers.Imagem;
using Spark.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using MercadoPago.Client.Preference;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using System.IO;
using System.Text.Json;

namespace Spark.Api.Controllers
{
    [ApiController]
    //[Authorize(Roles = "d7ccf34b-b722-4735-a1c2-a95c2b472eb1")]
    [Route("checkout")]
    public class CheckoutController : ControllerBase
    {

        [HttpPost("create")]
        public async Task<IActionResult> CreatePreference([FromBody] CreateOrderDto dto)
        {
            var client = new PreferenceClient();

            var request = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest> {
                new PreferenceItemRequest {
                    Title = dto.Title ?? "Pedido",
                    Quantity = dto.Quantity <= 0 ? 1 : dto.Quantity,
                    UnitPrice = dto.UnitPrice <= 0 ? 1 : dto.UnitPrice,
                    CurrencyId = "BRL"
                }
            },
                ExternalReference = dto.OrderId, // id do seu pedido
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = "myapp://checkout/success",
                    Failure = "myapp://checkout/failure",
                    Pending = "myapp://checkout/pending"
                },
                AutoReturn = "approved",
                NotificationUrl = "http://localhost:8080/api/webhooks/mercadopago"
            };

            var pref = await client.CreateAsync(request);
            return Ok(new
            {
                preferenceId = pref.Id,
                initPoint = pref.InitPoint,             // URL para abrir o checkout
                sandboxInitPoint = pref.SandboxInitPoint
            });
        }

        [Route("api/webhooks/mercadopago")]
        public class WebhooksController : ControllerBase
        {
            // idempotência simples: evite reprocessar o mesmo evento
            private static readonly HashSet<string> _processed = new();

            [HttpPost]
            public async Task<IActionResult> Receive()
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                // MP às vezes manda querystring também: ?type=payment&id=123
                var qsType = Request.Query["type"].ToString();
                var qsId = Request.Query["id"].ToString();

                string? topic = null;
                string? id = null;

                if (!string.IsNullOrWhiteSpace(qsType) && !string.IsNullOrWhiteSpace(qsId))
                {
                    topic = qsType;
                    id = qsId;
                }
                else
                {
                    using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("type", out var typeProp))
                        topic = typeProp.GetString();

                    // novo formato: { "data": { "id": "###" }, "action": "payment.updated" }
                    if (root.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var dataId))
                        id = dataId.GetString();

                    // formato legado: { "id": "###" }
                    if (string.IsNullOrWhiteSpace(id) && root.TryGetProperty("id", out var idProp))
                        id = idProp.GetString();
                }

                if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(id))
                    return Ok(); // sempre 200 rápido

                var uniqueKey = $"{topic}:{id}";
                if (!_processed.Add(uniqueKey))
                    return Ok(); // já processado

                // ---- Fonte da verdade: consultar API ----
                if (topic.Equals("payment", StringComparison.OrdinalIgnoreCase))
                {
                    var payClient = new PaymentClient();
                    Payment payment = await payClient.GetAsync(Convert.ToInt64(id));

                    // Ex.: approved / pending / rejected
                    var status = payment.Status; // string
                    var externalRef = payment.ExternalReference; // mapeia pro seu pedido

                    // TODO: atualizar seu pedido no banco conforme status (approved/pending/rejected)
                    // TODO: log/observabilidade
                }
                else if (topic.Equals("merchant_order", StringComparison.OrdinalIgnoreCase))
                {
                    // Se quiser, trate merchant_order (somar pagamentos, etc.)
                    // var moClient = new MerchantOrderClient();
                    // var mo = await moClient.GetAsync(long.Parse(id));
                }

                return Ok();
            }
        }
    }

    public record CreateOrderDto(string OrderId, string Title, int Quantity, decimal UnitPrice);
}