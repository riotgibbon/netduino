using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

/*
 * Copyright 2011-2012 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sharp_GP2Y0A02YK_Proximity_Sensor
{
    public class Program
    {
        public static void Main()
        {
            SharpGP2Y0A02YK Distance = new SharpGP2Y0A02YK(new Netduino.ADC(Pins.GPIO_PIN_A0));

            while (true)
            {
                int cm = Distance.Distance;
                int inch = (int)(cm / 2.54);
                Debug.Print("Approximate distance: " + cm.ToString() + "cm / " + inch.ToString() + "\"");
                Thread.Sleep(1000);
            }
        }

    }
}
