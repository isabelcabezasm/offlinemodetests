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
            await SendEvents(ioTHubModuleClient);
           
        }


        /// <summary>

        /// Module behavior:
        ///        Sends data periodically (with default frequency of 1/3 seconds).
        /// </summary>

        static async Task SendEvents(ModuleClient moduleClient)
        {
            int count = 1; //messages counter

            while (true)
            {

                var tempData = "{'count':" + count + "}";
                string dataBuffer = JsonConvert.SerializeObject(tempData);
                var eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                Console.WriteLine($"\t{DateTime.Now.ToLocalTime()}> Sending message: {count}, Body: [{dataBuffer}]");
                await moduleClient.SendEventAsync("message", eventMessage);
                count++;
                await Task.Delay(333);
            }

        }


    }
}
