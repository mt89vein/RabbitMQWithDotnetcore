using System;
using Api.BackgroundServices;
using Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.QueueServices;
using MQ.Abstractions.Repositories;
using MQ.Configuration;
using MQ.Configuration.Base;
using MQ.PersistentConnection;
using MQ.Repositories;
using MQ.Services;
using RabbitMQ.Client;

namespace Api.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddQueueSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionSettings>(configuration.GetSection(nameof(ConnectionSettings)));
            services.Configure<ConnectionStrings>(configuration.GetSection(nameof(ConnectionStrings)));

            services.Configure<DocumentPublishQueueSettings>(
                configuration.GetSection(nameof(DocumentPublishQueueSettings)));

            services.Configure<DocumentPublishUpdateQueueSettings>(
                configuration.GetSection(nameof(DocumentPublishUpdateQueueSettings)));

            services.Configure<DocumentPublishCancelQueueSettings>(
                configuration.GetSection(nameof(DocumentPublishCancelQueueSettings)));

            services.Configure<DocumentPublishResultQueueSettings>(
                configuration.GetSection(nameof(DocumentPublishResultQueueSettings)));
        }

        public static void AddDocumentPublicationServices(this IServiceCollection services)
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
                    AutomaticRecoveryEnabled = true
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

            services.AddScoped<IDocumentPublishQueueService, DocumentPublishQueueService>();
            services.AddScoped<IDocumentPublishUpdateQueueService, DocumentPublishUpdateQueueService>();
            services.AddScoped<IDocumentPublishCancelQueueService, DocumentPublishPublishCancelQueueService>();
            services.AddScoped<IDocumentPublishResultQueueService, DocumentPublishResultQueueService>();
        }

        public static void AddBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<DocumentPublishBackgroundService>();
            services.AddHostedService<DocumentPublishUpdateBackgroundService>();
            services.AddHostedService<DocumentPublishCancelBackgroundService>();
            services.AddHostedService<DocumentPublishResultBackgroundService>();
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IPublishDocumentTaskRepository, PublishDocumentTaskEfRepository>();
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<INotifyService, NotifyService>();
        }
    }
}