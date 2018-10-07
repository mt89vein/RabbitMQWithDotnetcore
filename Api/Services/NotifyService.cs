using System.Threading.Tasks;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using MQ.Models;
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

            return _hub.Clients.All.SendAsync(message.State.ToString(), msg);
        }
    }
}
