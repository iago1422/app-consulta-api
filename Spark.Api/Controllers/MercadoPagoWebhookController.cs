using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MercadoPago.Client.Payment;
using MercadoPago.Resource.Payment;
using Spark.Domain.Commands;
using Spark.Domain.Repositories;

namespace Spark.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/webhooks/mercadopago")]
    public class MercadoPagoWebhookController : ControllerBase
    {
        private readonly ICreditosRepository _creditosRepository;
        private readonly IConfiguration _config;
        private readonly ILogger<MercadoPagoWebhookController> _logger;

        // trava somente para o "crédito" (approved), não para pending
        private static readonly ConcurrentDictionary<string, byte> _credited = new();

        public MercadoPagoWebhookController(
            ICreditosRepository creditosRepository,
            IConfiguration config,
            ILogger<MercadoPagoWebhookController> logger)
        {
            _creditosRepository = creditosRepository;
            _config = config;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receive()
        {
            _logger.LogInformation("[MP] Webhook recebido");

            MercadoPagoWebhookPayload payload = null;
            string rawBody;

            using (var reader = new StreamReader(Request.Body))
                rawBody = await reader.ReadToEndAsync();

            _logger.LogInformation("[MP] RawBody: {rawBody}", rawBody);

            if (!string.IsNullOrWhiteSpace(rawBody))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<MercadoPagoWebhookPayload>(
                        rawBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    _logger.LogInformation("[MP] Payload deserializado com sucesso");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[MP] Erro ao deserializar payload");
                    return Ok();
                }
            }

            var eventType = FirstNonEmpty(payload?.Type, payload?.Topic, Request.Query["type"], Request.Query["topic"]);
            _logger.LogInformation("[MP] eventType: {eventType}", eventType);

            // ignora merchant_order nesta controller
            if (IsMerchantOrderEvent(eventType))
            {
                _logger.LogInformation("[MP] Evento merchant_order ignorado nesta controller");
                return Ok();
            }

            var paymentIdStr = FirstNonEmpty(
                ReadId(payload?.Data?.Id),
                ReadId(payload?.Id),
                Request.Query["data.id"],
                Request.Query["id"],
                ExtractIdFromResource(payload?.Resource),
                ExtractIdFromResource(Request.Query["resource"])
            );

            _logger.LogInformation("[MP] paymentIdStr resolvido: {paymentIdStr}", paymentIdStr);

            if (!IsPaymentEvent(eventType))
            {
                _logger.LogInformation("[MP] Evento ignorado (não é payment)");
                return Ok();
            }

            if (string.IsNullOrWhiteSpace(paymentIdStr))
            {
                _logger.LogWarning("[MP] paymentIdStr vazio - abortando");
                return Ok();
            }

            if (!long.TryParse(paymentIdStr, out var paymentId))
            {
                _logger.LogWarning("[MP] paymentIdStr não é número válido: {paymentIdStr}", paymentIdStr);
                return Ok();
            }

            _logger.LogInformation("[MP] Buscando payment no MercadoPago API. ID: {paymentId}", paymentId);

            Payment payment;
            try
            {
                var client = new PaymentClient();
                payment = await client.GetAsync(paymentId);

                _logger.LogInformation("[MP] Payment encontrado. Status: {status}", payment.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MP] Erro ao buscar payment no MercadoPago API");
                return Ok();
            }

            // logs úteis
            _logger.LogInformation("[MP] Payment.Id: {id}", payment.Id);
            _logger.LogInformation("[MP] Payment.Status: {status}", payment.Status);
            _logger.LogInformation("[MP] Payment.ExternalReference: {externalReference}", payment.ExternalReference);

            if (payment.Metadata == null)
            {
                _logger.LogWarning("[MP] Payment.Metadata: null");
            }
            else
            {
                _logger.LogInformation("[MP] Payment.Metadata keys: {keys}", string.Join(",", payment.Metadata.Keys));
                foreach (var kv in payment.Metadata)
                    _logger.LogInformation("[MP] Payment.Metadata[{key}] = {value}", kv.Key, kv.Value?.ToString());
            }

            if (!string.Equals(payment.Status, "approved", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("[MP] Payment não aprovado. Status atual: {status}", payment.Status);
                return Ok();
            }

            _logger.LogInformation("[MP] Payment aprovado. Iniciando crédito...");

            var (userId, creditos) = ExtractUserAndCredits(payment);

            _logger.LogInformation("[MP] userId extraído: {userId}", userId);
            _logger.LogInformation("[MP] creditos extraídos: {creditos}", creditos);

            if (userId == Guid.Empty || creditos <= 0)
            {
                _logger.LogWarning("[MP] userId ou creditos inválidos. userId={userId} creditos={creditos}", userId, creditos);
                return Ok();
            }

            // Idempotência correta: trava por Payment.Id (apenas no approved)
            var creditKey = $"mp:credit:{payment.Id}";
            if (!_credited.TryAdd(creditKey, 0))
            {
                _logger.LogInformation("[MP] Crédito já processado (memória). key={key}", creditKey);
                return Ok();
            }

            var referencia = $"MP:{payment.Id}";
            _logger.LogInformation("[MP] Referência gerada: {referencia}", referencia);

            var req = new CriarCreditos.Request
            {
                UserId = userId,
                Saldo = (double)creditos,
                Referencia = referencia
            };

            try
            {
                await _creditosRepository.Create(req);
                _logger.LogInformation("[MP] Créditos criados com sucesso. userId={userId} referencia={referencia}", userId, referencia);
            }
            catch (Exception ex)
            {
                // se falhar salvar, libera a chave pra permitir retry do MP
                _credited.TryRemove(creditKey, out _);
                _logger.LogError(ex, "[MP] Erro ao salvar créditos no repositório. userId={userId} referencia={referencia}", userId, referencia);
            }

            _logger.LogInformation("[MP] Webhook finalizado");
            return Ok();
        }

        private static bool IsPaymentEvent(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType)) return true;
            return eventType.ToLowerInvariant().Contains("payment");
        }

        private static bool IsMerchantOrderEvent(string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventType)) return false;
            return eventType.ToLowerInvariant().Contains("merchant_order");
        }

        private static string FirstNonEmpty(params object[] values)
        {
            foreach (var v in values)
            {
                var s = v?.ToString();
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
            }
            return null;
        }

        private static (Guid userId, decimal creditos) ExtractUserAndCredits(Payment payment)
        {
            // metadata: aceita userId ou user_id
            try
            {
                if (payment.Metadata != null)
                {
                    Guid uid = Guid.Empty;
                    decimal cred = 0;

                    object uidObj = null;
                    if (payment.Metadata.TryGetValue("userId", out uidObj) || payment.Metadata.TryGetValue("user_id", out uidObj))
                        Guid.TryParse(uidObj?.ToString(), out uid);

                    if (payment.Metadata.TryGetValue("creditos", out var credObj))
                        decimal.TryParse(credObj?.ToString(), out cred);

                    if (uid != Guid.Empty && cred > 0)
                        return (uid, cred);
                }
            }
            catch { }

            // external_reference: uid:GUID|cred:10|ord:xxx
            var ext = payment.ExternalReference ?? "";
            if (!string.IsNullOrWhiteSpace(ext))
            {
                Guid uid = Guid.Empty;
                decimal cred = 0;

                foreach (var part in ext.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (part.StartsWith("uid:", StringComparison.OrdinalIgnoreCase))
                        Guid.TryParse(part.Substring(4), out uid);

                    if (part.StartsWith("cred:", StringComparison.OrdinalIgnoreCase))
                        decimal.TryParse(part.Substring(5), out cred);
                }

                if (uid != Guid.Empty && cred > 0)
                    return (uid, cred);
            }

            return (Guid.Empty, 0);
        }

        private static string ReadId(JsonElement? el)
        {
            if (el is null) return null;

            var v = el.Value;
            return v.ValueKind switch
            {
                JsonValueKind.Number => v.TryGetInt64(out var n) ? n.ToString() : null,
                JsonValueKind.String => v.GetString(),
                _ => null
            };
        }

        private static string ExtractIdFromResource(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource)) return null;

            if (long.TryParse(resource.Trim(), out _))
                return resource.Trim();

            var parts = resource.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var last = parts.Length > 0 ? parts[^1] : null;

            if (!string.IsNullOrWhiteSpace(last) && long.TryParse(last, out _))
                return last;

            return null;
        }

        private class MercadoPagoWebhookPayload
        {
            public string Type { get; set; }
            public string Topic { get; set; }
            public string Resource { get; set; }
            public JsonElement? Id { get; set; }
            public MercadoPagoWebhookData Data { get; set; }
        }

        private class MercadoPagoWebhookData
        {
            public JsonElement? Id { get; set; }
        }
    }
}
