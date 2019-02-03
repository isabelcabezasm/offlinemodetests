using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// This application uses the Microsoft Azure Event Hubs Client for .NET
// For samples see: https://github.com/Azure/azure-event-hubs/tree/master/samples/DotNet
// For documenation see: https://docs.microsoft.com/azure/event-hubs/

//more samples: https://github.com/Azure/azure-event-hubs/blob/master/samples/DotNet/Microsoft.Azure.EventHubs/SampleEphReceiver/Program.cs

namespace Reader
{
    class Program
    {

        private static Config config;
        private static EventHubClient s_eventHubClient;

        public static Config ReadConfiguration()
        {

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            Config config2;
            // deserialize JSON directly from a file
            using (StreamReader file = File.OpenText("config.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                config2 = (Config)serializer.Deserialize(file, typeof(Config));
            }

            return config2;

        }



        public static void Main(string[] args)
        {

            config = ReadConfiguration();

            AsyncMain().GetAwaiter().GetResult();

            Console.ReadLine();

            
        }

        private static async Task SendDirectMethodAsync(int num_messages) {

            using (var servClient = ServiceClient.CreateFromConnectionString(config.ConnectionStringIoTHub))
            {
                var methodInvocation = new CloudToDeviceMethod("sendMessages", TimeSpan.FromSeconds(30));
                methodInvocation.SetPayloadJson(num_messages.ToString());

                await servClient.InvokeDeviceMethodAsync("raspi", "target", methodInvocation);

                Console.WriteLine("Asked for {0} messages", num_messages);
            }
        }

        private static async Task AsyncMain() {
                       

            var connectionString = new EventHubsConnectionStringBuilder(new Uri(config.EventHubsCompatibleEndpoint), config.EventHubsCompatiblePath, config.IotHubSasKeyName, config.IotHubSasKey);
            s_eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());


            // Let's create a PartitionReciever for each partition on the hub.

            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            var tasks = new List<Task>(); //let's asign each partitionreceiver to one task
            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition));
            }

            // Wait for all the PartitionReceivers to finsih.
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition)
        {
            int counter = 0; //let's count how many messages arrive.

            var eventHubReceiver = s_eventHubClient.CreateReceiver("signify", partition, EventPosition.FromStart());// EventPosition.FromEnqueuedTime(DateTime.Now.AddSeconds(-120)));

            while (true)
            {
                var events = await eventHubReceiver.ReceiveAsync(5);

                if (events != null)
                {

                    foreach (EventData eventData in events)
                    {
                        counter++;
                        string data = Encoding.UTF8.GetString(eventData.Body.Array);

                        Console.WriteLine(" dato: {0}, total: {1}", data, counter);
                    }
                }

            }
        }
    }
}
