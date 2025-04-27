using Confluent.Kafka;
using Messaging.Kafka.Common;
using Messaging.Kafka.Consuming.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Consuming
{
    public class KafkaConsumer<TMessage> : BackgroundService
    {
        private string _topic;
        private IConsumer<string, TMessage> _consumer;
        private IMessageHandler<TMessage> _messageHandler;
        private readonly CancellationTokenSource _cts = new();

        public KafkaConsumer(IOptions<KafkaSettings> kafkaSettings, IMessageHandler<TMessage> messageHandler)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers,
                GroupId = kafkaSettings.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _topic = kafkaSettings.Value.TopicName;

            _consumer = new ConsumerBuilder<string, TMessage>(config)
                .SetValueDeserializer(new KafkaDeserializer<TMessage>())
                .Build();

            _messageHandler = messageHandler;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);
        }

        private async Task? ConsumeAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);
                    await _messageHandler.HandleAsync(result.Message.Value, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                //
                Console.WriteLine(ex.Message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            try
            {
                // Give the consumer a chance to gracefully close
                _consumer?.Close();
                _consumer?.Dispose();
            }
            catch (Exception ex)
            {
                // Log any errors during shutdown
                Console.WriteLine($"Error during consumer shutdown: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _cts?.Dispose();
            _consumer?.Dispose();
        }
    }
}

//using Confluent.Kafka;
//using Messaging.Kafka.Common;
//using Messaging.Kafka.Consuming.Abstractions;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Messaging.Kafka.Consuming
//{
//    public class KafkaConsumer<TMessage> : BackgroundService, IDisposable
//    {
//        private readonly string _topic;
//        private readonly IConsumer<string, TMessage> _consumer;
//        private readonly IMessageHandler<TMessage> _messageHandler;
//        private readonly ILogger<KafkaConsumer<TMessage>> _logger;
//        private bool _disposed;

//        public KafkaConsumer(
//            IOptions<KafkaSettings> kafkaSettings,
//            IMessageHandler<TMessage> messageHandler,
//            ILogger<KafkaConsumer<TMessage>> logger)
//        {
//            var config = new ConsumerConfig
//            {
//                BootstrapServers = kafkaSettings.Value.BootstrapServers,
//                GroupId = kafkaSettings.Value.GroupId,
//                AutoOffsetReset = AutoOffsetReset.Earliest,
//                EnableAutoCommit = false,
//                EnableAutoOffsetStore = false,
//                MaxPollIntervalMs = 300000,
//                SessionTimeoutMs = 10000
//            };

//            _topic = kafkaSettings.Value.TopicName;
//            _messageHandler = messageHandler;
//            _logger = logger;

//            _consumer = new ConsumerBuilder<string, TMessage>(config)
//                .SetErrorHandler((_, e) => logger.LogError($"Kafka error: {e.Reason}"))
//                .SetValueDeserializer(new KafkaDeserializer<TMessage>())
//                .Build();
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _consumer.Subscribe(_topic);

//            try
//            {
//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    try
//                    {
//                        var result = _consumer.Consume(stoppingToken);

//                        if (result != null)
//                        {
//                            await _messageHandler.HandleAsync(result.Message.Value, stoppingToken);
//                            _consumer.StoreOffset(result);
//                        }
//                    }
//                    catch (ConsumeException e) when (!e.Error.IsFatal)
//                    {
//                        _logger.LogError(e, $"Consume error: {e.Error.Reason}");
//                        await Task.Delay(1000, stoppingToken);
//                    }
//                    catch (OperationCanceledException)
//                    {
//                        _logger.LogInformation("Consuming was canceled (normal shutdown)");
//                        break;
//                    }
//                    catch (KafkaException e)
//                    {
//                        _logger.LogError(e, "Fatal Kafka error, stopping consumer");
//                        break;
//                    }
//                }
//            }
//            finally
//            {
//                SafeCloseConsumer();
//            }
//        }

//        private void SafeCloseConsumer()
//        {
//            if (_disposed) return;

//            try
//            {
//                _logger.LogInformation("Closing Kafka consumer...");
//                _consumer?.Close();
//                _consumer?.Dispose();
//                _logger.LogInformation("Kafka consumer closed successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while closing Kafka consumer");
//            }
//        }

//        public override async Task StopAsync(CancellationToken cancellationToken)
//        {
//            _logger.LogInformation("Initiating consumer shutdown...");
//            await base.StopAsync(cancellationToken);
//        }

//        public void Dispose()
//        {
//            if (!_disposed)
//            {
//                SafeCloseConsumer();
//                _disposed = true;
//                GC.SuppressFinalize(this);
//            }
//        }
//    }
//}