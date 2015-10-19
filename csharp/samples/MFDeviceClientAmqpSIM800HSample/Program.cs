using Eclo.NetMF.SIM800H;
using Microsoft.Azure.Devices.Client;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System;
using System.Diagnostics;
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
#if TRACE
                //Amqp.Trace.TraceLevel = Amqp.TraceLevel.Frame;
                //Amqp.Trace.TraceListener = (f, a) => Debug.Print(Fx.Format(f, a));
#endif
                //Amqp.Trace.WriteLine(Amqp.TraceLevel.Frame, "{0}", "Starting...");

                InitCellularRadio();

            }
            catch { };

            while (true)
            {
                //Microsoft.SPOT.Debug.GC(true);
                Microsoft.SPOT.Debug.Print("Free RAM: " + Microsoft.SPOT.Debug.GC(false).ToString());

                Thread.Sleep(5000);
            };

            Debug.Print("Done!\n");
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

                    //foreach (byte b in receivedMessage.GetBytes())
                    //{
                    //    sb.Append((char)b);
                    //}

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
            SIM800H.GprsNetworkRegistrationChanged += cellularRadio_GprsNetworkRegistrationChanged;

            // set APN config
            // better do this inside a try/catch because value can be invalid
            try
            {
                SIM800H.AccessPointConfig = AccessPointConfiguration.Parse("internet.vodafone.pt|vodafone|vodafone");
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

                // sockets are only required when using AMQP
                SIM800H.GprsProvider.GprsSocketsBearerStateChanged -= GprsProvider_GprsSocketsBearerStateChanged;
                SIM800H.GprsProvider.GprsSocketsBearerStateChanged += GprsProvider_GprsSocketsBearerStateChanged;


                // open GPRS bearer async
                SIM800H.GprsProvider.OpenBearerAsync();

                // setup GPRS sockets connection
                SetupGprsConnection();
            }
            else
            {
            }
        }

        static void GprsProvider_GprsIpAppsBearerStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                // we have GPRS connectition for IP apps

                // check if need to update RTC
                //new Thread(() =>
                //{
                //    Thread.Sleep(500);
                //    //UpdateRTCFromNetwork();
                //}).Start();


            }
        }

        static void GprsProvider_GprsSocketsBearerStateChanged(bool isOpen)
        {
            if (isOpen)
            {
                // we have sockets GPRS connectition 


                // do it
                new Thread(() =>
                {
                    Thread.Sleep(1000);

                    // check if need to update RTC
                    //UpdateRTCFromNetwork();

                    DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(DeviceConnectionString, TransportType.Amqp);

                    //SendEvent(deviceClient);
                    ReceiveCommands(deviceClient);


                }).Start();

            }
        }

        static void SetupGprsConnection()
        {
            // open GPRS sockets connection async
            SIM800H.GprsProvider.OpenGprsConnectionAsync((a) =>
            {
                Eclo.NetMF.SIM800H.Gprs.ConnectGprsAsyncResult result = (Eclo.NetMF.SIM800H.Gprs.ConnectGprsAsyncResult)a;
                if (!(result.Result == Eclo.NetMF.SIM800H.Gprs.ConnectGprsResult.Open ||
                    result.Result == Eclo.NetMF.SIM800H.Gprs.ConnectGprsResult.AlreadyOpen))
                {
                    // failed to open GPRS sockets connection
                    // TBD

                }
            });
        }
        
        static void UpdateRTCFromNetwork()
        {
            byte retryCounter = 0;

            while (retryCounter <= 3)
            {
                try
                {
                    var request = cellularRadio.SntpClient.SyncNetworkTimeAsync("time.nist.gov", TimeSpan.Zero);
                    var result = request.End();

                    // check result
                    if (result == Eclo.NetMF.SIM800H.Sntp.SyncResult.SyncSuccessful)
                    {
                        // get current date time and update RTC
                        DateTime rtcValue = cellularRadio.GetDateTime();
                        // set framework date time
                        Utility.SetLocalTime(rtcValue);

                        // done here, dispose SNTP client to free up memory
                        cellularRadio.SntpClient = null;

                        return;
                    }
                }
                catch
                {
                    // failed updating RTC
                    // flag this
                }

                // add retry
                retryCounter++;

                // progressive wait 15*N seconds before next retry
                Thread.Sleep(15000 * retryCounter);
            }
        }

    }
    
    public static class Fx
    {

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }

        public static string Format(string format, params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return format;
            }

            StringBuilder sb = new StringBuilder(format.Length * 2);

            char[] array = format.ToCharArray();
            for (int i = 0; i < array.Length; ++i)
            {
                // max supported number of args is 10
                if (array[i] == '{' && i + 2 < array.Length && array[i + 2] == '}' && array[i + 1] >= '0' && array[i + 1] <= '9')
                {
                    int index = array[i + 1] - '0';
                    if (index < args.Length)
                    {
                        sb.Append(args[index]);
                    }

                    i += 2;
                }
                else
                {
                    sb.Append(array[i]);
                }
            }

            return sb.ToString();
        }

        public static void StartThread(ThreadStart threadStart)
        {
            new Thread(threadStart).Start();
        }
    }

}
