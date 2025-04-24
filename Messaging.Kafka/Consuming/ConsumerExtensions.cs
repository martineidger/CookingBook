using Messaging.Kafka.Common;
using Messaging.Kafka.Consuming.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Consuming
{
    public static class ConsumerExtensions
    {
        //public static IServiceCollection AddConsumer<TMessage, THandler>(this IServiceCollection services,
        //     IConfigurationSection configurationSection)
        //     where THandler : class, IMessageHandler<TMessage>  
        //     where TMessage : class  
        //{
        //    services.Configure<KafkaSettings>(configurationSection);
        //    services.AddHostedService<KafkaConsumer<TMessage>>();
        //    services.AddSingleton<IMessageHandler<TMessage>, THandler>();
        //    return services;
        //}
        public static IServiceCollection AddConsumer<TMessage, THandler>(this IServiceCollection services,
    IConfigurationSection configurationSection)
    where THandler : class, IMessageHandler<TMessage>
    where TMessage : class
        {
            services.Configure<KafkaSettings>(configurationSection);

            // Register the consumer as singleton to manage its lifecycle properly
            services.AddSingleton<KafkaConsumer<TMessage>>();

            // Register it as hosted service
            services.AddHostedService(provider => provider.GetRequiredService<KafkaConsumer<TMessage>>());

            services.AddSingleton<IMessageHandler<TMessage>, THandler>();
            return services;
        }
    }
}
