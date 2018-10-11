using System.Threading.Tasks;
using Integration.Models;

namespace Integration.Abstractions
{
    public interface INotifyService
    {
        Task SendNotificationAsync(PublishDocumentTask publishDocumentTask);
    }
}