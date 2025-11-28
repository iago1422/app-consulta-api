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
            await Clients.Group(consultId).SendAsync("ReceiveSdp", payload);
        }

        public async Task SendIceToGroup(string consultId, object payload)
        {
            await Clients.Group(consultId).SendAsync("ReceiveIce", payload);
        }

        public async Task SendChatToGroup(string consultId, object payload)
        {
            await Clients.Group(consultId).SendAsync("ReceiveChatMessage", payload);
        }
    }
}
