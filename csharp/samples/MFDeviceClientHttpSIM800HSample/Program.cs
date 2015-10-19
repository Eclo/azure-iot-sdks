using Eclo.NetMF.SIM800H;
using Microsoft.Azure.Devices.Client;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace MFTestApplication
{
    public class Program
    {
        private const string DeviceConnectionString = "<replace>";
        private static int MESSAGE_COUNT = 5;

        public static void Main()
        {
            try
            {
                InitCellularRadio();
            }
            catch { };

            while (true)
            {
                //Microsoft.SPOT.Debug.GC(true);
                Microsoft.SPOT.Debug.Print("Free RAM: " + Microsoft.SPOT.Debug.GC(false).ToString());

                Thread.Sleep(5000);
            };
        }

        static void SendEvent(DeviceClient deviceClient)
        {
            string dataBuffer;

            Debug.Print("Device sending " + MESSAGE_COUNT + " messages to IoTHub...");

            for (int count = 0; count < MESSAGE_COUNT; count++)
            {
                dataBuffer = Guid.NewGuid().ToString();
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                Debug.Print(DateTime.Now.ToLocalTime() + "> Sending message: " + count + ", Data: [" + dataBuffer + "]");

                deviceClient.SendEvent(eventMessage);
            }
        }

        static void ReceiveCommands(DeviceClient deviceClient)
        {
            Debug.Print("Device waiting for commands from IoTHub...");
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = deviceClient.Receive();

                if (receivedMessage != null)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (byte b in receivedMessage.GetBytes())
                    {
                        sb.Append((char)b);
                    }

                    messageData = sb.ToString();

                    // dispose string builder
                    sb = null;

                    Debug.Print(DateTime.Now.ToLocalTime() + "> Received message: " + messageData);

                    deviceClient.Complete(receivedMessage);
                }

                Thread.Sleep(10000);
            }
        }

        static void InitCellularRadio()
        {
            // SIM800H power key
            OutputPort sim800PowerKey = new OutputPort(Cpu.Pin.GPIO_Pin6, false);
            // SIM800H serial port
            SerialPort sim800SerialPort = new SerialPort("COM2");

            // configure SIM800H device
            SIM800H.Configure(sim800PowerKey, sim800SerialPort);

            // set max GPRS sockets to 2
            SIM800H.MaxSockets = 2;

            // register eventhandlers for network
            //cellularRadio.GsmNetworkRegistrationChanged += cellularRadio_GsmNetworkRegistrationChanged;
            SIM800H.GprsNetworkRegistrationChanged += cellularRadio_GprsNetworkRegistrationChanged;

            // set APN config
            // better do this inside a try/catch because value can be invalid
            try
            {
                SIM800H.AccessPointConfig = AccessPointConfiguration.Parse("internet.vodafone.pt;vodafone;vodafone");
            }
            catch
            {
                // something wrong, flag that APN config may not be valid
            };

            // power on radio
            SIM800H.PowerOnAsync((a) =>
            {
                // power on sequence completed
                // check result
                if (((PowerOnAsyncResult)a).Result == PowerStatus.On)
                {

                }
                else
                {
                    // something went wrong...
                }
            });
        }

        static void cellularRadio_GprsNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            if (networkState == NetworkRegistrationState.Registered)
            {
                //Debug.Print("*** GPRS network registered ***");

                // add event handlers but remove them first so we don't have duplicate calls in case this a new registration
                SIM800H.GprsProvider.GprsIpAppsBearerStateChanged -= GprsProvider_GprsIpAppsBearerStateChanged;
                SIM800H.GprsProvider.GprsIpAppsBearerStateChanged += GprsProvider_GprsIpAppsBearerStateChanged;

                // open GPRS bearer async
                SIM800H.GprsProvider.OpenBearerAsync();
            }
        }

        static void GprsProvider_GprsIpAppsBearerStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                new Thread(() =>
                {
                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Http1);

                    SendEvent(deviceClient);
                    ReceiveCommands(deviceClient);

                    Debug.Print("Done!\n");

                }).Start();
            }
        }
    }
}
