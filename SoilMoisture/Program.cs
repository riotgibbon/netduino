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
using Netduino_MQTT_Client_Library;
using AdaFruit_GUVA_S12SD;

namespace SoilMoisture
{
    public class Program
    {
         

        public static void Main()
        {
            int ifttIntervalMinutes = 10;
            int mqttIntervalSeconds = 5;

            int sleepMS = 0;

            // Create an output port (a port that can be written to) 
            // and wire it to the onboard LED
            OutputPort led = new OutputPort(Pins.ONBOARD_LED, false);

            HumiditySensorController sensorOne = new HumiditySensorController(N.Pins.GPIO_PIN_A0, N.Pins.GPIO_PIN_D7);
            HumiditySensorController sensorTwo = new HumiditySensorController(N.Pins.GPIO_PIN_A1, N.Pins.GPIO_PIN_D6);
            HumiditySensorController sensorThree = new HumiditySensorController(N.Pins.GPIO_PIN_A2, N.Pins.GPIO_PIN_D5);
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

            var lightSensor = new GUVAS12SD(N.Pins.GPIO_PIN_A3);

            DateTime uploadToIFTTTime = DateTime.Now.AddMinutes(-(ifttIntervalMinutes + 1));
            DateTime mqttMessageTime = DateTime.Now.AddSeconds(-(mqttIntervalSeconds + 1));

            while (true)
            {
                Thread.Sleep(1000);
                
                string temp = "", hum = "", humAdjusted="";             
                if (hasSi7021)
                {
                    si7021.Reset();
                    temp = si7021.Temperature.ToString("f2");
                    hum = si7021.Humidity.ToString("f2");
                    humAdjusted = (si7021.Humidity / 2).ToString("f2");
                    Debug.Print("Temperature: " + temp + ", humidity: " + hum);
                }
                string soil1 = getReading(sensorOne).final.ToString();
                string soil2 = getReading(sensorTwo).final.ToString();
                string soil3 = getReading(sensorThree).final.ToString();

                var lightValue = lightSensor.Read();
               // string lightValue.sensorReading + " = " + lightValue.sensorVoltage + "v, UV: " + lightValue.uvIndex
  
                string csvDelim = "|";
                string csv = soil1 + csvDelim + soil2 + csvDelim + soil3 + csvDelim + temp + csvDelim + hum + csvDelim + humAdjusted
                    + csvDelim + lightValue.sensorReading + csvDelim + lightValue.sensorVoltage + csvDelim + lightValue.uvIndex;

                Debug.Print(csv);

                if (DateTime.Now > uploadToIFTTTime)
                {
                    led.Write(true); // turn on the LED
                    try
                    {
                    sleepMS = 1000 * ifttIntervalMinutes * 60;
                    sendToIFTT("living_room_window", csv);

                    //led.Write(false);
                    uploadToIFTTTime = DateTime.Now.AddMinutes(ifttIntervalMinutes);
                    Debug.Print("Next upload to IFFT: " + uploadToIFTTTime);

                    }
                    catch (Exception)
                    {
                        Debug.Print("Upload batch to IFTTT failed");
                    }
                    led.Write(false);

                }
                if (DateTime.Now > mqttMessageTime)
                {
                    led.Write(true); // turn on the LED
                    try
                    {
                        
                        IPHostEntry hostEntry = Dns.GetHostEntry("192.168.0.63");
                        // Create socket and connect to the broker's IP address and port
                        var mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            mySocket.Connect(new IPEndPoint(hostEntry.AddressList[0], 1883));
                        }
                        catch (SocketException SE)
                        {
                            Debug.Print("Connection Error: " + SE.ErrorCode);
                            throw (SE);
                        }

                        int returnCode = NetduinoMQTT.ConnectMQTT(mySocket, "netduinoPlus", 500, true);
                        if (returnCode != 0)
                        {
                            var error = "Connection Error: " + returnCode.ToString();
                            Debug.Print(error);
                            throw new InvalidOperationException(error);
                        }


                        string windowLocation = "window";
                        if (hasSi7021)
                        {
                            
                            NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation, "temperature"), temp);
                            NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation,   "humidity"), hum); 
                            NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation,  "humidityAdjusted"), humAdjusted);
                        }
                        //lightValue.sensorReading + csvDelim + lightValue.sensorVoltage + csvDelim + lightValue.uvIndex
                        NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation, "lightReading"), lightValue.sensorReading.ToString());
                        NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation, "lightVoltage"), lightValue.sensorVoltage.ToString());
                        NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic(windowLocation, "uvIndex"), lightValue.uvIndex.ToString());

                        var metric = "soilmoisture";
                        NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic("aralia", metric), soil1);
                        NetduinoMQTT.PublishMQTT(mySocket, buildMqttTopic("bonsai",metric), soil2);
                        NetduinoMQTT.PublishMQTT(mySocket,buildMqttTopic( "amaryllis",metric), soil3);



                        mqttMessageTime = DateTime.Now.AddSeconds(mqttIntervalSeconds);
                        Debug.Print("Next MQTT messaging: " + mqttMessageTime);
                    }
                    catch (Exception e)
                    {
                        Debug.Print("MQTT failed" + e.InnerException.Message);
                            
                    }

                    led.Write(false); // turn off the LED
                }
                
                Thread.Sleep(500);
            }
        }

        static string buildMqttTopic(string device, string metric)
        {
            string mqttTopicRoot = "home/tele/";
            string room = "livingroom";
            string topic = mqttTopicRoot + metric + "/" + room + "/" + device;
            return topic;

        }

        private static void sendToIFTT( string ifftMetric, string value1, string value2="", string value3="")
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
                    
                }
                Thread.Sleep(retrySeconds * 1000);
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
