# IoT Edge Offline mode. Basic tests.

Very simple test:

- **Sender**:  It's a basic IoT Edge module. You can easily create it following this steps:
https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-csharp-module
I modified it just for send a message (a number) every 0.3 seconds.
You can see it here:
https://github.com/isabelcabezasm/offlinemodetests/blob/master/Sender/SenderModule/Program.cs


- **Reader**: It's an application that reads the events from an IoT Hub.
You should add your own IoT Hub settings in this file: 
https://github.com/isabelcabezasm/offlinemodetests/blob/master/Sender/Reader/Program.cs
It reads **all** the messages in the IoT Hub. Not "from Now", not "from last checkpoint".

  *var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromStart());*


## Add the IoT Edge Module to the Raspberry Pi 3:
From my development machine, with Windows 10 and Visual Studio 2017.  
Docker for Windows installed.

You can test it in localhost:
https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-csharp-module#build-and-debug-single-c-module

Build and deploy the solution in Raspberry Pi 3:
Instead "Build and Push Edge Solution":
https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-csharp-module#build-and-push-images

(The current template in VS2017 only creates the docker image for AMD64)

I added my dockerfile: 
https://github.com/isabelcabezasm/offlinemodetests/blob/master/Sender/SenderModule/Dockerfile.arm32v7

Add the "RuntimeIdentifiers" in the .csproj file: (Look here: https://github.com/isabelcabezasm/offlinemodetests/blob/master/Sender/SenderModule/SenderModule.csproj)
<pre><code>
&lt;RuntimeIdentifiers&gt; win-arm;linux-arm &lt;/RuntimeIdentifiers&gt; 
</code></pre>

 *Help* (.NET Core on Raspberry Pi)https://github.com/dotnet/core/blob/master/samples/RaspberryPiInstructions.md

(The dockerfile is the same is generated for the IoT Edge Module template:
https://github.com/isabelcabezasm/offlinemodetests/blob/master/Sender/SenderModule/Dockerfile.amd64     
but publishing *dotnet publish -r linux-arm*     
and using the *dotnet runtime arm32v7*)

### Steps to compile/build the docker image for ARM from Windows:
<pre> <code>
docker build . -f Dockerfile.arm32v7 -t *name_your_container_repository*.azurecr.io/iotedgemodule1:0.1-arm

docker push *name_your_container_repository*.azurecr.io/iotedgemodule1:0.1-arm
</code> </pre>

