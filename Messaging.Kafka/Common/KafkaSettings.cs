using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Kafka.Common
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }
        public string? TopicName { get; set; }
        public string? GroupId { get; set; }
        public List<string>? Topics { get; set; } 
    }
}
