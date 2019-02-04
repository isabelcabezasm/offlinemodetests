namespace IoTEdgeModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;

    class Program
    {
        static int counter;

        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            // Register callback to be called when a message is received by the module
            // await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);

            //Register callback to be called when a Direct method is received by the module
            await ioTHubModuleClient.SetMethodHandlerAsync("sendMessages", Directmethod1, ioTHubModuleClient);
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                var pipeMessage = new Message(messageBytes);
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }


        /// <summary>
        /// This method is called whenever the module is sent a method from the EdgeHub. 
        /// It prints all the data (parameter) received in the message.
        /// As we expect a number (of messages have been asked), it is printed and send the method that sends the messages to IoTHub
        /// </summary>
        private static async Task<MethodResponse> Directmethod1(MethodRequest methodRequest, object userContext)
        {

            int num_messages = 0;
            Console.WriteLine("A direct method has arrived! ");
            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }
            String data = Encoding.UTF8.GetString(methodRequest.Data);
            Console.WriteLine("Received message: {0}", data);

            int.TryParse(data, out num_messages);
            if (num_messages != 0)
            {
                await SendEvents(moduleClient, num_messages);
            }

            return new MethodResponse(200);

        }

        /// <summary>
        /// Module behavior:
        ///        Sends #"num_messages" messages (with default frequency of 1/3 seconds).
        /// </summary>

        static async Task SendEvents(ModuleClient moduleClient, int num_messages)
        {

            for (int i = 0; i < num_messages; i++)
            {

                var tempData = "{\"count\":" + i + "}";
                var eventMessage = new Message(Encoding.UTF8.GetBytes(tempData));
                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending message: {i}, Body: [{tempData}]");
                await moduleClient.SendEventAsync(eventMessage);
                await Task.Delay(333);
            }
        }
    }



}
