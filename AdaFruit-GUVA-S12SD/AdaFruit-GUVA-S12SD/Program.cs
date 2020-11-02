using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using N = SecretLabs.NETMF.Hardware.NetduinoPlus;
using SLH = SecretLabs.NETMF.Hardware;

namespace AdaFruit_GUVA_S12SD
{
    public class Program
    {
        public static void Main()
        {
            var lightSensor = new  GUVAS12SD(N.Pins.GPIO_PIN_A3);
            int readingCount = 1;
            while (true)
            {
                //lightSensor.SetRange(0, 1);
                var lightValue = lightSensor.Read();
                Debug.Print(readingCount + ": " + lightValue.sensorReading + " = " + lightValue.sensorVoltage + "v, UV: " + lightValue.uvIndex);
                readingCount++;
                Thread.Sleep(1000);
                
            }


        }

    }
}
