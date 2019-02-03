namespace SenderModule
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
    using Newtonsoft.Json;

    class Program
    {
               
        static void Main(string[] args)
        {
            Init().Wait();
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

            await ioTHubModuleClient.SetMethodHandlerAsync("sendMessages", Directmethod1, ioTHubModuleClient);
            await SendEvents(ioTHubModuleClient, 10);
            Console.ReadKey();
            
           
        }

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
     
          for(int i=0;i<num_messages; i++) { 

                var tempData = "{'count':" + i + "}";
                string dataBuffer = JsonConvert.SerializeObject(tempData);
                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending message: {i}, Body: [{dataBuffer}]");
                await moduleClient.SendEventAsync("message", eventMessage);
                await Task.Delay(333);
            }
        }

    }
}
