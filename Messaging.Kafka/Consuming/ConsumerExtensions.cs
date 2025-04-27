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
        public static IServiceCollection AddConsumer<TMessage, THandler>(this IServiceCollection services,
            IConfigurationSection configurationSection)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class
        {
            services.Configure<KafkaSettings>(configurationSection);

            services.AddSingleton<KafkaConsumer<TMessage>>();

            services.AddHostedService(provider => provider.GetRequiredService<KafkaConsumer<TMessage>>());

            services.AddSingleton<IMessageHandler<TMessage>, THandler>();
            return services;
        }

        public static IServiceCollection AddMultiConsumer<TKey>(this IServiceCollection services,
            IConfigurationSection configurationSection)
        {
            services.Configure<KafkaSettings>(configurationSection);
            services.AddSingleton<MultiTopicKafkaConsumer<TKey>>();
            
            return services;
        }


    }
}
