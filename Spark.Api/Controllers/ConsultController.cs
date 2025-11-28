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
using Microsoft.AspNetCore.SignalR;
using Spark.Api.Hubs;
using Spark.Domain.Infra.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Spark.Api.Controllers
{
    [ApiController]
    //[Authorize(Roles = "d7ccf34b-b722-4735-a1c2-a95c2b472eb1")]
    [Route("consulta")]
    public class ConsultController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IHubContext<ConsultHub> _hubContext;

        public ConsultController(DataContext context, IHubContext<ConsultHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // POST /consult
        [HttpPost]
        public async Task<ActionResult<CreateConsultResponse>> CreateConsult([FromBody] CreateConsultRequest request)
        {
            var doctorId = request.DoutorId;
            var patientId = request.UserId;

            var consult = new Consulta
            {
                DoutorId = doctorId,
                PacienteId = patientId,
                Status = "OPEN"
            };

            _context.Consultas.Add(consult);
            await _context.SaveChangesAsync();

            return Ok(new CreateConsultResponse
            {
                ConsultId = consult.Id,
                DoctorId = doctorId,
                PatientId = patientId
            });
        }

        // GET /consult/:id
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ConsultDto>> GetConsult([FromRoute] Guid id)
        {
            var consult = await _context.Consultas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consult == null)
                return NotFound(new { Message = "Consulta não encontrada" });

            var dto = new ConsultDto
            {
                ConsultId = consult.Id,
                DoctorId = consult.DoutorId,
                PatientId = consult.PacienteId,
                Status = consult.Status
            };

            return Ok(dto);
        }

        // POST /consult/:id/sdp
        [HttpPost("{id:guid}/sdp")]
        public async Task<IActionResult> PostSdp([FromRoute] Guid id, [FromBody] SdpRequest request)
        {
            var consult = await _context.Consultas.FindAsync(id);
            if (consult == null)
                return NotFound(new { Message = "Consulta não encontrada" });

            var payload = new
            {
                consultId = id,
                userId = request.UserId,
                type = request.Type,
                sdp = request.Sdp
            };

            // envia para todos do grupo (médico + paciente)
            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("ReceiveSdp", payload);

            return Ok(new { Message = "SDP recebida e encaminhada" });
        }

        // POST /consult/:id/ice
        [HttpPost("{id:guid}/ice")]
        public async Task<IActionResult> PostIce([FromRoute] Guid id, [FromBody] IceCandidateRequest request)
        {
            var consult = await _context.Consultas.FindAsync(id);
            if (consult == null)
                return NotFound(new { Message = "Consulta não encontrada" });

            var payload = new
            {
                consultId = id,
                userId = request.UserId,
                candidate = request.Candidate
            };

            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("ReceiveIce", payload);

            return Ok(new { Message = "ICE candidate recebida e encaminhada" });
        }

        // POST /consult/:id/chat
        [HttpPost("{id:guid}/chat")]
        public async Task<IActionResult> SendChatMessage([FromRoute] Guid id, [FromBody] ChatMessageRequest request)
        {
            var consult = await _context.Consultas.FindAsync(id);
            if (consult == null)
                return NotFound(new { Message = "Consulta não encontrada" });

            var msgEntity = new ConsultaChatMessage
            {
                UsuarioId = request.UserId,
                Message = request.Message,
                SentAt = DateTime.UtcNow
            };

            _context.ConsultasChats.Add(msgEntity);
            await _context.SaveChangesAsync();

            var response = new ChatMessageResponse
            {
                UserId = msgEntity.UsuarioId,
                Message = msgEntity.Message,
                SentAt = msgEntity.SentAt
            };

            await _hubContext.Clients.Group(id.ToString())
                .SendAsync("ReceiveChatMessage", new
                {
                    consultId = id,
                    response.UserId,
                    response.Message,
                    response.SentAt
                });

            return Ok(new { Message = "Mensagem enviada", Data = response });
        }

        // GET /consult/:id/chat
        [HttpGet("{id:guid}/chat")]
        public async Task<ActionResult<IEnumerable<ChatMessageResponse>>> GetChatHistory([FromRoute] Guid id)
        {
            var consult = await _context.Consultas.FindAsync(id);
            if (consult == null)
                return NotFound(new { Message = "Consulta não encontrada" });

            var messages = await _context.ConsultasChats
                .AsNoTracking()
                .Where(m => m.Id == id)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            var response = messages.Select(m => new ChatMessageResponse
            {
                UserId = m.UsuarioId,
                Message = m.Message,
                SentAt = m.SentAt
            });

            return Ok(response);
        }
    }
}