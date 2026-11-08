using System;
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
            await Clients.Group(consultId).SendAsync("ReceiveChatMessage", payload);
        }

        // Indicador de "digitando" no chat (repassa só para o outro peer)
        public async Task NotifyTyping(string consultId, object payload)
        {
            await Clients.GroupExcept(consultId, Context.ConnectionId).SendAsync("UserTyping", payload);
        }

        // Avisa o grupo da consulta (grupo = queueId) que o status da fila mudou (LEFT | DONE).
        // Usado pelo QueueController e pelo FilaAutoDoneWorker via IHubContext.
        public static Task NotificarStatusFila(IHubContext<ConsultHub> hub, Guid queueId, string status)
        {
            return hub.Clients.Group(queueId.ToString()).SendAsync("ConsultStatusChanged", new { queueId, status });
        }
    }
}
