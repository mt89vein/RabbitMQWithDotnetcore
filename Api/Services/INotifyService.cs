using System.Threading.Tasks;
using MQ.Models;

namespace Api.Services
{
    public interface INotifyService
    {
        Task SendNotificationAsync(PublishDocumentTask publishDocumentTask);
    }
}