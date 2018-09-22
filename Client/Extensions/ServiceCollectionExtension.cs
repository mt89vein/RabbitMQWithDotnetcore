using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.PersistentConnection;
using MQ.Services;
using MQ.Services.AggregatorService;
using RabbitMQ.Client;

namespace Client.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddQueueSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionSettings>(configuration.GetSection(nameof(ConnectionSettings)));
            services.Configure<DocumentPublishUpdateQueueSettings>(configuration.GetSection(nameof(DocumentPublishUpdateQueueSettings)));
            services.Configure<DocumentPublishQueueSettings>(configuration.GetSection(nameof(DocumentPublishQueueSettings)));
            services.PostConfigure<DocumentPublishQueueSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            
            services.PostConfigure<DocumentPublishUpdateQueueSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
        }

        public static void RegisterDocumentPublicationServices(this IServiceCollection services)
        {
            services.AddSingleton<IPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultPersistentConnection>>();
                var config = sp.GetRequiredService<IOptions<ConnectionSettings>>().Value;

                var factory = new ConnectionFactory
                {
                    HostName = config.HostName,
                    Port = config.Port ?? 5672,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(config.NetworkRecoveryInterval ?? 10),
                    RequestedHeartbeat = config.RequestedHeartbeat,
                    AutomaticRecoveryEnabled = true,
                };

                //if (!String.IsNullOrEmpty(config.UserName))
                //{
                //    factory.UserName = config.UserName;
                //}

                //if (!String.IsNullOrEmpty(config.Password))
                //{
                //    factory.Password = config.Password;
                //}

                return new DefaultPersistentConnection(factory, logger, config.RetryConnectionAttempt ?? 5);
            });
            services.AddSingleton<IDocumentPublishProcessingService, DocumentPublishProcessingService>();
            services.AddSingleton<IDocumentPublishService, DocumentPublishService>();
            services.AddSingleton<IDocumentPublishUpdateService, DocumentPublishUpdateService>();
            services.AddSingleton<IDocumentPublishUpdateProcessingService, DocumentPublishUpdateProcessingService>();
            services.AddSingleton<IPublishService, PublishService>();
        }

        public static void RegisterBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<DocumentPublishProcessingBackgroundService>();
            services.AddHostedService<DocumentPublishUpdateBackgroundService>();
        }
    }
}