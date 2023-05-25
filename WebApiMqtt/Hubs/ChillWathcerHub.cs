using Microsoft.AspNetCore.SignalR;
using static MQTTnet.Samples.Helpers.ObjectExtensions;

namespace WebApiMqtt.Hubs
{
    public class ChillWathcerHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastNotification(string message, BroadCastType type)
        {
            await Clients.All.SendAsync("RecieveNotification", message, type);
        }
        public async Task UpdateLatestTelementry(Telemetry telemetry)
        {
            await Clients.All.SendAsync("UpdateTelementry", telemetry);
        }
    }
}
