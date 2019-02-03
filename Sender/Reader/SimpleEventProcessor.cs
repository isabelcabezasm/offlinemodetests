using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Reader
{
    public class SimpleEventProcessor : IEventProcessor
    {
        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {

            Console.WriteLine($"Processor Shutting Down. Partition '{context.Lease.PartitionId}', Reason: '{reason}'.");
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.Lease.PartitionId}' Offset: {context.Lease.Offset}");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.Lease.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array,eventData.Body.Offset,eventData.Body.Count);
                Console.WriteLine($"Message received. Partition: '{context.Lease.PartitionId}', Data: '{data}'");
            }
            return context.CheckpointAsync();
        }
    }
}
