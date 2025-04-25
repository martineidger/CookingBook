using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Consuming.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Kafka.Consuming
{
    public class MultiTopicKafkaConsumer<TKey> : IDisposable
    {
        private readonly KafkaSettings _config;
        private readonly ILogger<MultiTopicKafkaConsumer<TKey>> _logger;
        private readonly Dictionary<string, Task> _consumptionTasks = new();
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public MultiTopicKafkaConsumer(
            IOptions<KafkaSettings> config,
            ILogger<MultiTopicKafkaConsumer<TKey>> logger)
        {
            _config = config.Value;
            _logger = logger;
        }

        public void Subscribe<TMessage>(
            string topic,
            IMessageHandler<TMessage> handler,
            string? groupId = null) where TMessage : class
        {
            if (_disposed) throw new ObjectDisposedException(nameof(MultiTopicKafkaConsumer<TKey>));
            if (string.IsNullOrEmpty(topic)) throw new ArgumentNullException(nameof(topic));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                GroupId = groupId ?? _config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            var consumer = new ConsumerBuilder<TKey, TMessage>(consumerConfig)
                .SetValueDeserializer(new KafkaDeserializer<TMessage>())
                .SetErrorHandler((_, error) => _logger.LogError($"Kafka error: {error.Reason}"))
                .Build();

            consumer.Subscribe(topic);

            var task = Task.Run(() => ConsumeMessages(consumer, handler, topic, _cts.Token), _cts.Token);
            _consumptionTasks.Add(topic, task);
        }

        private async Task ConsumeMessages<TMessage>(
            IConsumer<TKey, TMessage> consumer,
            IMessageHandler<TMessage> handler,
            string topic,
            CancellationToken ct) where TMessage : class
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(ct);
                        if (result?.Message?.Value == null)
                        {
                            _logger.LogWarning($"Null message received from topic {topic}");
                            continue;
                        }

                        await handler.HandleAsync(result.Message.Value, ct);
                        consumer.Commit(result);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, $"Consume error in topic {topic}: {ex.Error.Reason}");
                        await Task.Delay(1000, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Unexpected error processing message in topic {topic}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Consumption stopped for topic {topic}");
            }
            finally
            {
                consumer.Close();
                consumer.Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _cts.Cancel();
                Task.WaitAll(_consumptionTasks.Values.ToArray(), TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during consumer disposal");
            }
            finally
            {
                _cts.Dispose();
                _disposed = true;
            }
        }
    }
}