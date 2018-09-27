using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQ.Abstractions.Consumers.PublishServices;
using MQ.Abstractions.Consumers.PublishUpdateServices;
using MQ.Abstractions.Producers.PublishServices;
using MQ.Abstractions.Producers.UpdateServices;
using MQ.Configuration.Base;
using MQ.Configuration.Consumers.PublishSettings;
using MQ.Configuration.Consumers.PublishUpdateSettings;
using MQ.Configuration.Producers.PublishSettings;
using MQ.Configuration.Producers.PublishUpdateSettings;
using MQ.PersistentConnection;
using MQ.Services;
using MQ.Services.ProducerServices.PublishServices;
using MQ.Services.ProducerServices.PublishUpdateServices;
using RabbitMQ.Client;

namespace Client.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddQueueSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionSettings>(configuration.GetSection(nameof(ConnectionSettings)));

            services.Configure<DocumentOnePublishProducerServiceSettings>(
                configuration.GetSection(nameof(DocumentOnePublishProducerServiceSettings))
            );

            services.Configure<DocumentOnePublishUpdateProducerServiceSettings>(
                configuration.GetSection(nameof(DocumentOnePublishUpdateProducerServiceSettings))
            );

            services.Configure<DocumentOnePublishConsumerServiceSettings>(
                configuration.GetSection(nameof(DocumentOnePublishConsumerServiceSettings))
            );

            services.Configure<DocumentOnePublishUpdateConsumerServiceSettings>(
                configuration.GetSection(nameof(DocumentOnePublishUpdateConsumerServiceSettings))
            );

            services.PostConfigure<DocumentOnePublishProducerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentOnePublishUpdateProducerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentOnePublishConsumerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentOnePublishUpdateConsumerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });

            services.Configure<DocumentTwoPublishProducerServiceSettings>(
                configuration.GetSection(nameof(DocumentTwoPublishProducerServiceSettings))
            );

            services.Configure<DocumentTwoPublishUpdateProducerServiceSettings>(
                configuration.GetSection(nameof(DocumentTwoPublishUpdateProducerServiceSettings))
            );

            services.Configure<DocumentTwoPublishConsumerServiceSettings>(
                configuration.GetSection(nameof(DocumentTwoPublishConsumerServiceSettings))
            );

            services.Configure<DocumentTwoPublishUpdateConsumerServiceSettings>(
                configuration.GetSection(nameof(DocumentTwoPublishUpdateConsumerServiceSettings))
            );

            services.PostConfigure<DocumentTwoPublishProducerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentTwoPublishUpdateProducerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentTwoPublishConsumerServiceSettings>(settings =>
            {
                settings.ConnectionString = configuration.GetConnectionString("RabbitMQ");
            });
            services.PostConfigure<DocumentTwoPublishUpdateConsumerServiceSettings>(settings =>
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

            services.AddScoped<IDocumentTwoPublishConsumerService, DocumentTwoPublishConsumerService>();
            services.AddScoped<IDocumentTwoPublishProducerService, DocumentTwoPublishProducerService>();
            services.AddScoped<IDocumentTwoPublishUpdateProducerService, DocumentTwoPublishUpdateProducerService>();
            services.AddScoped<IDocumentTwoPublishUpdateConsumerService, DocumentTwoPublishUpdateConsumerService>();

            services.AddScoped<IDocumentOnePublishProducerService, DocumentOnePublishProducerService>();
            services.AddScoped<IDocumentOnePublishConsumerService, DocumentOnePublishConsumerService>();
            services.AddScoped<IDocumentOnePublishUpdateProducerService, DocumentOnePublishUpdateProducerService>();
            services.AddScoped<IDocumentOnePublishUpdateConsumerService, DocumentOnePublishUpdateConsumerService>();
        }

        public static void RegisterBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<DocumentOnePublishProcessingBackgroundService>();
            services.AddHostedService<DocumentOnePublishUpdateProcessingBackgroundService>();
            services.AddHostedService<DocumentTwoPublishProcessingBackgroundService>();
            services.AddHostedService<DocumentTwoPublishUpdateProcessingBackgroundService>();
        }
    }
}