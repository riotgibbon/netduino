using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using N = SecretLabs.NETMF.Hardware.NetduinoPlus;


using System.IO;

using System.Text;
using Microsoft.SPOT.Net.NetworkInformation;
using Toolbox.NETMF.Hardware;

namespace SoilMoisture
{
    public class Program
    {
         

        public static void Main()
        {
            //Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].GatewayAddress);
            //Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].EnableDhcp();

            //var interf = NetworkInterface.GetAllNetworkInterfaces()[0];
            //interf.EnableStaticIP("192.168.0.55", "255.255.255.0", "192.168.0.1");
            ////interf.EnableStaticDns(new string[] { <settings.primaryDNSAddress>, <settings.secondaryDNSAddress> });
            //Debug.Print(Microsoft.SPOT.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()[0].GatewayAddress);

            int sleepMinutes = 10;
            int retrySeconds = 10;
            int sleepMS = 0;
            int uploadTries = 0;
            int maxTries = 5;

            

            // Create an output port (a port that can be written to) 
            // and wire it to the onboard LED
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            //HumiditySensorController sensorFour = new HumiditySensorController(N.Pins.GPIO_PIN_A3, N.Pins.GPIO_PIN_D4);
            // The Adafruit LCD Shield uses a MCP23017 IC as multiplex chip
            Mcp23017 Mux = new Mcp23017();
            // Pins 9 to 15 are connected to the HD44780 LCD
            Hd44780Lcd Display = new Hd44780Lcd(
                Data: Mux.CreateParallelOut(9, 4),
                ClockEnablePin: Mux.Pins[13],
                ReadWritePin: Mux.Pins[14],
                RegisterSelectPin: Mux.Pins[15]
            );

            HumiditySensorController sensorOne = new HumiditySensorController(N.Pins.GPIO_PIN_A0, N.Pins.GPIO_PIN_D7);
            HumiditySensorController sensorTwo = new HumiditySensorController(N.Pins.GPIO_PIN_A1, N.Pins.GPIO_PIN_D6);
            HumiditySensorController sensorThree = new HumiditySensorController(N.Pins.GPIO_PIN_A2, N.Pins.GPIO_PIN_D5);
            bool upload = true;
            while (true)
            {
                Thread.Sleep(1000);
                uploadTries++;
                led.Write(true); // turn on the LED
                //https://maker.ifttt.com/trigger/soil_moisture/with/key/d52lKnzf-xDid_NfD5tga-?value1=120

                      
                string sensorOneHumidity = getReading(sensorOne).final.ToString();
                //Thread.Sleep(500);

                string sensorTwoHumidity = getReading(sensorTwo).final.ToString();
                //Thread.Sleep(500);

                string sensorThreeHumidity = getReading(sensorThree).final.ToString();
                //string sensorFourHumidity = getReading(sensorFour);
                string display = "1:" + sensorOneHumidity + ",2:" + sensorTwoHumidity +",3:" + sensorThreeHumidity;// + ",4:" + sensorFourHumidity;
               // string sensorOneHumidity = getReading(sensorOne);
                //string display = sensorOneHumidity;
                Debug.Print(display);
                Display.ClearDisplay();
                Display.Write(display);
                if (upload)
                {
                    try
                    {
                        string requestUri = "http://maker.ifttt.com/trigger/soil_moisture/with/key/d52lKnzf-xDid_NfD5tga-?value1=" + sensorOneHumidity + "&value2=" + sensorTwoHumidity + "&value3=" + sensorThreeHumidity;
                        sleepMS = 1000 * sleepMinutes * 60;
                        Display.ClearDisplay();
                        Display.Write(display);
                        byte[] buffer;
                        Stream stream;
                        HttpWebResponse response;

                        using (var request = (HttpWebRequest)WebRequest.Create(requestUri))
                        {
                            request.Method = "GET";
                            response = (HttpWebResponse)request.GetResponse();
                        }
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Display.Write(", ok (" + uploadTries.ToString() + ")        " + response.Headers["Date"].ToString());
                        }
                        else
                        {
                            throw new Exception("Couldn't send message");
                        }

                        uploadTries = 0;

                        //buffer = new byte[response.ContentLength];
                        //stream = response.GetResponseStream();

                        //int read;
                        //using (var ms = new MemoryStream())
                        //{
                        //    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        //    {
                        //        ms.Write(buffer, 0, read);
                        //    }
                        //}
                        //char[] chars = Encoding.UTF8.GetChars(buffer);
                        //var text = new string(chars);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message.Length > 0 ? e.Message : e.InnerException.Message;
                        Display.Write(" Error recording data:" + msg);

                        Debug.Print(e.Message);
                        Debug.Print(e.StackTrace);

                        if (uploadTries < maxTries)
                            sleepMS = retrySeconds * 1000;
                        else
                            uploadTries = 0;


                    }

                    led.Write(false); // turn off the LED
                    Thread.Sleep(sleepMS);

                    //led.Write(true); // turn on the LED
                    //Thread.Sleep(Humidity); // sleep for 250ms
                    //led.Write(false); // turn off the LED
                    //Thread.Sleep(Humidity); // sleep for 250ms
                }
                Thread.Sleep(500);
            }
        }

        private static HumidityReading getReading(HumiditySensorController sensor)
        {
            int humidity = 0;
            int humidityTotal = 0;
            int reads = 10;
            for (int i = 0; i < reads; i++)
            {
                humidity = sensor.Read().raw;
                humidityTotal += humidity;
            }
            return new HumidityReading { final = humidity, average = (humidityTotal / reads) };
            
        }

        struct HumidityReading
        {
            public int final;
            public double average;

        }

    }
}
