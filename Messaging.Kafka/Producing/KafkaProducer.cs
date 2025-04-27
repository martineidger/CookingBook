using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Producing.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Producing
{
    public class KafkaProducer<TMessage> : IKafkaProducer<TMessage>
    {
        private readonly IProducer<string, TMessage> producer;
        private readonly string topic;
        public KafkaProducer(IOptions<KafkaSettings> kafkaSettings)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
            };

            producer = new ProducerBuilder<string, TMessage>(config)
                .SetValueSerializer(new KafkaSerializer<TMessage>())
                .Build();

            topic = kafkaSettings.Value.TopicName;    
        }

        public async Task ProduceAsync(TMessage message, CancellationToken cancellationToken)
        {
            await producer.ProduceAsync(topic, new Message<string, TMessage>()
            {
                Key = "key",
                Value = message,

            }, cancellationToken);

            Console.WriteLine("Succesfully delivered");

        }
        public void Dispose()
        {
            producer?.Dispose();
        }
    }
}
