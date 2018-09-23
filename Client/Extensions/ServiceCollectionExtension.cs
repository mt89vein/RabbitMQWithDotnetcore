using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQ.Configuration;
using MQ.Interfaces;
using MQ.Services;

namespace Client.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddQueueSettings(this IServiceCollection services, IConfiguration configuration)
        {
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
            services.AddScoped<IDocumentPublishProcessingService, DocumentPublishProcessingService>();
            services.AddScoped<IDocumentPublishService, DocumentPublishService>();
            services.AddScoped<IDocumentPublishUpdateService, DocumentPublishUpdateService>();
            services.AddScoped<IDocumentPublishUpdateProcessingService, DocumentPublishUpdateProcessingService>();
        }

        public static void RegisterBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<DocumentPublishProcessingBackgroundService>();
            services.AddHostedService<DocumentPublishUpdateBackgroundService>();
        }
    }
}