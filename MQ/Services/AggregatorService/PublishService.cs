using MQ.Interfaces;

namespace MQ.Services.AggregatorService
{
    public class PublishService : IPublishService
    {
        public PublishService(
            IDocumentPublishService publishService,
            IDocumentPublishUpdateService updateService,
            IDocumentPublishProcessingService publishProcessingService,
            IDocumentPublishUpdateProcessingService publishUpdateProcessingService)
        {
            DocumentPublishService = publishService;
            DocumentUpdateService = updateService;
            DocumentPublishProcessingService = publishProcessingService;
            DocumentPublishUpdateProcessingService = publishUpdateProcessingService;
        }

        public IDocumentPublishService DocumentPublishService { get; }

        public IDocumentPublishUpdateService DocumentUpdateService { get; }

        public IDocumentPublishProcessingService DocumentPublishProcessingService { get; }

        public IDocumentPublishUpdateProcessingService DocumentPublishUpdateProcessingService { get; }
    }
}