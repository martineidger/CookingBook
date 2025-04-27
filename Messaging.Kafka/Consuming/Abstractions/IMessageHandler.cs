using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Consuming.Abstractions
{
    public interface IMessageHandler<in TMessage>
    {
        Task HandleAsync(TMessage message, CancellationToken cancellationToken);
    }
}
