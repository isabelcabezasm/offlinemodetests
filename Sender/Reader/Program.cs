using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.EventHubs;


// This application uses the Microsoft Azure Event Hubs Client for .NET
// For samples see: https://github.com/Azure/azure-event-hubs/tree/master/samples/DotNet
// For documenation see: https://docs.microsoft.com/azure/event-hubs/

//more samples: https://github.com/Azure/azure-event-hubs/blob/master/samples/DotNet/Microsoft.Azure.EventHubs/SampleEphReceiver/Program.cs

namespace Reader
{
    class Program
    {
        
        private static EventHubClient s_eventHubClient;

        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        //Replace the value of the variable with the Event Hubs-compatible endpoint.
        private readonly static string s_eventHubsCompatibleEndpoint = "{your Event Hubs compatible endpoint}";      

        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        private readonly static string s_eventHubsCompatiblePath = "{your Event Hubs compatible name}";


        // az iot hub policy show --name iothubowner --query primaryKey --hub-name {your IoT Hub name}
        private readonly static string s_iotHubSasKey = "{your iothubowner primary key}";
        private readonly static string s_iotHubSasKeyName = "iothubowner";

        //az iot hub show-connection-string --hub-name [nombre iothub] --output table
        static string connectionStringIoTHub = "{your IoT Hub connection string}";







        public static void Main(string[] args)
        {

            

            while (true) {

                String option;
                    Console.Write("1- Ask 10 messages\n");
                    Console.Write("2- Ask 100 messages\n");
                    Console.Write("3- Ask 1000 messages\n");
                    option = Console.ReadLine();

                    int num_messages = 0;
                    switch (option) {
                        case "1":
                            num_messages = 10;
                            break;
                        case "2":
                            num_messages = 100;
                            break;
                        case "3":
                            num_messages = 1000;
                            break;
                    }
                

                SendDirectMethodAsync(10).GetAwaiter().GetResult();
                AsyncMain().GetAwaiter().GetResult();


                Console.ReadLine();

            }
        }

        private static async Task SendDirectMethodAsync(int num_messages) {

            using (var servClient = ServiceClient.CreateFromConnectionString(connectionStringIoTHub))
            {
                var methodInvocation = new CloudToDeviceMethod("sendMessages", TimeSpan.FromSeconds(30));
                methodInvocation.SetPayloadJson(num_messages.ToString());

                await servClient.InvokeDeviceMethodAsync("raspi", "target", methodInvocation);

                Console.WriteLine("Asked for {0} messages", num_messages);
            }
        }

        private static async Task AsyncMain() {
                       

            var connectionString = new EventHubsConnectionStringBuilder(new Uri(s_eventHubsCompatibleEndpoint), s_eventHubsCompatiblePath, s_iotHubSasKeyName, s_iotHubSasKey);
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
