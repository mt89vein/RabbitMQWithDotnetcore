using MQ.Interfaces;

namespace MQ.Services.AggregatorService
{
    public interface IPublishService
    {
        IDocumentPublishService DocumentPublishService { get; }

        IDocumentPublishUpdateService DocumentUpdateService { get; }

        IDocumentPublishProcessingService DocumentPublishProcessingService { get; }

        IDocumentPublishUpdateProcessingService DocumentPublishUpdateProcessingService { get; }
    }
}
