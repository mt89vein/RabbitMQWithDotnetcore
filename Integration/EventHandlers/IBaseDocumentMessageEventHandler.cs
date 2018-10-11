using System.Threading;

namespace Integration.EventHandlers
{
    public interface IBaseDocumentMessageEventHandler
    {
        void Start(CancellationToken cancellationToken);
    }
}