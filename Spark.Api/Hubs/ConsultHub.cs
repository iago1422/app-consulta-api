using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Spark.Api.Hubs
{
    public class ConsultHub : Hub
    {
        // Cliente chama assim que entra: connection.invoke("JoinConsult", consultId)
        public async Task JoinConsult(string consultId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, consultId);
        }

        // Esses métodos são usados pelo servidor (controller) para enviar pro outro peer:

        public async Task SendSdpToGroup(string consultId, object payload)
        {
            await Clients.GroupExcept(consultId, Context.ConnectionId).SendAsync("ReceiveSdp", payload);
        }

        public async Task SendIceToGroup(string consultId, object payload)
        {
            await Clients.GroupExcept(consultId, Context.ConnectionId).SendAsync("ReceiveIce", payload);
        }

        public async Task SendChatToGroup(string consultId, object payload)
        {
            if (payload is JsonElement mensagem) ChatRecente.Guardar(consultId, mensagem);
            await Clients.Group(consultId).SendAsync("ReceiveChatMessage", payload);
        }

        // Clientes novos chamam depois de JoinConsult (e a cada reconexão) para recuperar o que
        // chegou enquanto estavam fora do grupo. Versões antigas não chamam e seguem só ao vivo.
        public IReadOnlyList<JsonElement> GetChatHistory(string consultId) => ChatRecente.Listar(consultId);

        // Indicador de "digitando" no chat (repassa só para o outro peer)
        public async Task NotifyTyping(string consultId, object payload)
        {
            await Clients.GroupExcept(consultId, Context.ConnectionId).SendAsync("UserTyping", payload);
        }

        // Avisa o grupo da consulta (grupo = queueId) que o status da fila mudou (LEFT | DONE).
        // Usado pelo QueueController e pelo FilaAutoDoneWorker via IHubContext.
        public static Task NotificarStatusFila(IHubContext<ConsultHub> hub, Guid queueId, string status)
        {
            if (status == "DONE") ChatRecente.Encerrar(queueId.ToString());
            return hub.Clients.Group(queueId.ToString()).SendAsync("ConsultStatusChanged", new { queueId, status });
        }
    }
}
