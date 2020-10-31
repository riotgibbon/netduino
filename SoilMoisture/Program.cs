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
using Netduino.Foundation.Sensors.Atmospheric;

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
            int sleepMS = 0;

            // Create an output port (a port that can be written to) 
            // and wire it to the onboard LED
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);



            HumiditySensorController sensorOne = new HumiditySensorController(N.Pins.GPIO_PIN_A0, N.Pins.GPIO_PIN_D7);
            HumiditySensorController sensorTwo = new HumiditySensorController(N.Pins.GPIO_PIN_A1, N.Pins.GPIO_PIN_D6);
           // HumiditySensorController sensorThree = new HumiditySensorController(N.Pins.GPIO_PIN_A2, N.Pins.GPIO_PIN_D5);
            bool hasSi7021 = false;
            SI7021 si7021=null;
            try
            {
                si7021 = new SI7021(updateInterval: 0);
                Debug.Print("Serial number: " + si7021.SerialNumber);
                Debug.Print("Firmware revision: " + si7021.FirmwareRevision);
                Debug.Print("Sensor type: " + si7021.SensorType);
                hasSi7021 = true;

            }
            catch (Exception)
            {
                Debug.Print("Cannot find SI7021");
                hasSi7021 = false;
            }


            
            bool upload = true; 
            while (true)
            {
                Thread.Sleep(1000);
                string temp = "", hum="";
                
                led.Write(true); // turn on the LED
                //https://maker.ifttt.com/trigger/soil_moisture/with/key/d52lKnzf-xDid_NfD5tga-?value1=120
                if (hasSi7021)
                {
                    si7021.Reset();
                    temp = si7021.Temperature.ToString("f2");
                    hum = si7021.Humidity.ToString("f2");
                    Debug.Print("Temperature: " + temp + ", humidity: " + hum);

                }
                string soil1 = getReading(sensorOne).final.ToString();
                string soil2 = getReading(sensorTwo).final.ToString();
                string soil3 = "";// getReading(sensorThree).final.ToString();

                string display = "1:" + soil1 +",2:" + soil2 + ",3:" + soil3;
                Debug.Print(display);
                if (upload)
                {
                    sleepMS = 1000 * sleepMinutes * 60;
                    if (hasSi7021)
                    {
                        sendToIFTT("window_temp_hum", temp, hum);
                        sendToIFTT("soil_moisture", soil1, soil2, temp + "/" + hum);
                    }
                    else
                        sendToIFTT("soil_moisture", soil1, soil2);
                    led.Write(false);
                    Thread.Sleep(sleepMS);

                }
                led.Write(false); // turn off the LED
                Thread.Sleep(500);
            }
        }

        private static void sendToIFTT( string ifftMetric, string value1, string value2, string value3="")
        {
            int retrySeconds = 5;
            int uploadTries = 0;
            int maxTries = 5;
            while (uploadTries < maxTries)
            {
                try
                {
                    string requestUri = "http://maker.ifttt.com/trigger/" + ifftMetric + "/with/key/d52lKnzf-xDid_NfD5tga-?value1=" + value1 + "&value2=" + value2 + "&value3=" + value3;
                    HttpWebResponse response;

                    using (var request = (HttpWebRequest)WebRequest.Create(requestUri))
                    {
                        request.Method = "GET";
                        response = (HttpWebResponse)request.GetResponse();
                    }
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Debug.Print("Uploaded " + ifftMetric + ": value1=" + value1 + "&value2=" + value2 + "&value3=" + value3);
                        break;
                        //Display.Write(", ok (" + uploadTries.ToString() + ")        " + response.Headers["Date"].ToString());
                    }
                    else
                    {
                        throw new Exception("Couldn't send message");
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message.Length > 0 ? e.Message : e.InnerException.Message;
                    Debug.Print(e.Message);
                    Debug.Print(e.StackTrace);
                    uploadTries++;
                    Thread.Sleep(retrySeconds * 1000);

                }

            }
        }

        private static HumidityReading getReading(HumiditySensorController sensor)
        {
            int humidity = 0;
            int humidityTotal = 0;
            int reads = 5;
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
