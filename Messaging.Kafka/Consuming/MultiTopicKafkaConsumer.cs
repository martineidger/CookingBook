using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Consuming.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Kafka.Consuming
{
    public class MultiTopicKafkaConsumer<TMessage> : BackgroundService
    {
        private readonly IConsumer<string, TMessage> _consumer;
        private readonly IMultiTopicMessageHandler<TMessage> _messageHandler;
        private readonly IReadOnlyCollection<string> _topics;
        private readonly CancellationTokenSource _cts = new();

        public MultiTopicKafkaConsumer(
            IOptions<KafkaSettings> kafkaSettings,
            IMultiTopicMessageHandler<TMessage> messageHandler)
        {
            if (kafkaSettings?.Value == null)
                throw new ArgumentNullException(nameof(kafkaSettings));

            if (string.IsNullOrEmpty(kafkaSettings.Value.BootstrapServers))
                throw new ArgumentException("BootstrapServers is not configured");

            if (string.IsNullOrEmpty(kafkaSettings.Value.GroupId))
                throw new ArgumentException("GroupId is not configured");

            _topics = kafkaSettings.Value.Topics ?? throw new ArgumentException("Topics are not configured");
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));

            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                GroupId = kafkaSettings.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, TMessage>(config)
                .SetValueDeserializer(new KafkaDeserializer<TMessage>())
                .SetErrorHandler((_, e) => Console.WriteLine($"Kafka error: {e.Reason}"))
                .Build();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);
        }

        private async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topics);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(stoppingToken);
                        await _messageHandler.HandleAsync(result.Message.Value, result.Topic, stoppingToken);
                        _consumer.StoreOffset(result);
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Error consuming message: {ex.Error.Reason}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                }
            }
            finally
            {
                _consumer.Close();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            try
            {
                _consumer?.Close();
                _consumer?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during consumer shutdown: {ex.Message}");
            }
        }

        public override void Dispose()
        {
            _cts?.Dispose();
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}