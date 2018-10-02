using System;
using System.Threading.Tasks;
using Domain;
using Microsoft.Extensions.Logging;
using MQ.Abstractions.Repositories;
using MQ.Models;

namespace MQ.Repositories
{
    public class PublishDocumentTaskEfRepository : IPublishDocumentTaskRepository
    {
        private readonly PublishDocumentTaskContext _context;
        private readonly ILogger<PublishDocumentTaskEfRepository> _logger;

        public PublishDocumentTaskEfRepository(PublishDocumentTaskContext context, ILogger<PublishDocumentTaskEfRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PublishDocumentTask> Get(Guid id)
        {
            return await _context.FindAsync<PublishDocumentTask>(id);
        }

        public void SavePublishAttempt(PublishDocumentTask publishDocumentTask, DocumentPublicationInfo documentPublicationInfo)
        {
            var publishDocumentTaskAttempt = new PublishDocumentTaskAttempt
            {
                PublishDocumentTaskId = publishDocumentTask.Id,
                Response = documentPublicationInfo.Response,
                Request = documentPublicationInfo.Request,
                IsHasEisError = documentPublicationInfo.IsHasEisError,
                Result = documentPublicationInfo.ResultType
            };

            publishDocumentTask.State = ConvertToPublishState(documentPublicationInfo.ResultType);
            
            publishDocumentTask.UpdatedAt = DateTime.Now;
            publishDocumentTask.PublishDocumentTaskAttempts.Add(publishDocumentTaskAttempt);

            _logger.LogDebug($"id={publishDocumentTask.DocumentId} PublishDocumentTask.State= {publishDocumentTask.State} attempt result= {publishDocumentTaskAttempt.Result}");

            _context.SaveChanges();

            PublishState ConvertToPublishState(PublicationResultType resultType)
            {
                switch (resultType)
                {
                    case PublicationResultType.Success:
                        return PublishState.Published;
                    case PublicationResultType.Failed:
                        return PublishState.Failed;
                    case PublicationResultType.Processing:
                        return PublishState.Processing;
                    case PublicationResultType.XmlValidationError:
                        return PublishState.XmlValidationError;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(resultType));
                }
            }
        }
    }
}
