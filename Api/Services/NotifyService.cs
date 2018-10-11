using System.Threading.Tasks;
using Api.Hubs;
using Integration.Abstractions;
using Integration.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Api.Services
{
    public class NotifyService : INotifyService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotifyService(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public Task SendNotificationAsync(PublishDocumentTask message)
        {
            var msg = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml,

            });

            return _hub.Clients.All.SendAsync("OnPublicationStateChanged", msg);
        }
    }
}
