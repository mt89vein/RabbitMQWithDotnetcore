using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.EventBus.Abstractions;
using Integration.Abstractions;
using Microsoft.Extensions.Logging;

namespace Integration.EventHandlers
{
    public abstract class BaseDocumentMessageEventHandler<TEventMessage, TEventBus> : IBaseDocumentMessageEventHandler
        where TEventMessage : EventMessage where TEventBus : class, IEventBus
    {
        private readonly ILogger<BaseDocumentMessageEventHandler<TEventMessage, TEventBus>> _logger;
        protected readonly TEventBus EventBus;
        protected readonly INotifyService NotifyService;
        protected readonly IPublishDocumentTaskRepository PublishDocumentTaskRepository;

        protected BaseDocumentMessageEventHandler(
            IPublishDocumentTaskRepository publishDocumentTaskRepository,
            ILogger<BaseDocumentMessageEventHandler<TEventMessage, TEventBus>> logger,
            INotifyService notifyService,
            TEventBus eventBus)
        {
            PublishDocumentTaskRepository = publishDocumentTaskRepository ??
                                            throw new ArgumentNullException(nameof(publishDocumentTaskRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            NotifyService = notifyService ?? throw new ArgumentNullException(nameof(notifyService));
            EventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        protected abstract void RaiseException(Exception ex, TEventMessage publishMessage);

        protected abstract Task<bool> OnMessageReceivedAsync(TEventMessage message, CancellationToken cancellationToken);

        public void Start(CancellationToken cancellationToken)
        {
            _logger.LogInformation(GetType().Name + " started");
            try
            {
                EventBus.ProcessQueue<TEventMessage>(OnMessageReceivedAsync, RaiseException, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(GetType().Name + " Запрошена отмена работы сервиса");
            }
        }
    }
}