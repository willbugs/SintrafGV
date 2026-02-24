using Microsoft.AspNet.SignalR;
using Web.Hubs;

namespace Web.Models
{
    public class Functions
    {
        public static void SendProgress(string progressMessage, int progressCount, int totalItems, string quem)
        {
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            var percentage = progressCount * 100 / totalItems;
            hubContext.Clients.All.AddProgress(progressMessage, percentage + "%", quem);
        }
    }
}