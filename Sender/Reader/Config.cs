using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader
{
    public class Config
    {
        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        public String EventHubsCompatibleEndpoint { get; set; }

        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        public String EventHubsCompatiblePath { get; set; }

        // az iot hub policy show --name iothubowner --query primaryKey --hub-name {your IoT Hub name}
        public String IotHubSasKey { get; set; }


        public String IotHubSasKeyName { get; set; }
        
        //az iot hub show-connection-string --hub-name [nombre iothub] --output table
        public String ConnectionStringIoTHub { get; set; }
    }
}
