using Messaging.Kafka.Common;
using Messaging.Kafka.Producing.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Producing
{
    public static class ProducerExtensions
    {
        public static void AddProducer<TMessage>(this IServiceCollection services, 
            IConfigurationSection configurationSection)
        {
            services.Configure<KafkaSettings>(configurationSection);
            services.AddSingleton<IKafkaProducer<TMessage>, KafkaProducer<TMessage>>();
        }
    }
}
