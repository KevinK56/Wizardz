using Microsoft.AspNetCore.SignalR;

namespace Wizardz.Web.Hubs;

public class GameHub : Hub
{
    public async Task BroadcastStateChange(string message)
    {
        await Clients.Others.SendAsync("StateUpdated", message);
    }

    public async Task NotifyAffordability(string component)
    {
        await Clients.All.SendAsync("AffordabilityUpdated", component);
    }
}
