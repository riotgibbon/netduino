﻿using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using Toolbox.NETMF.Hardware;

/*
 * Copyright 2012 Stefan Thoolen (http://www.netmftoolbox.com/)
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
namespace LPD8806_RGB_Strip
{
    public class Program
    {
        public static void Main()
        {
            // There's a 1M strip (32 LEDs) connected to the first SPI bus on the Netduino
            Lpd8806Chain Chain = new Lpd8806Chain(32, SPI_Devices.SPI1);

            // Repeats all demos infinitely
            while (true)
            {
                // Shows a red, green and blue LED
                Chain.Configure(0, 0xff0000);
                Chain.Configure(1, 0x00ff00);
                Chain.Configure(2, 0x0000ff);
                Chain.Write();
                Thread.Sleep(5000);

                // Loops R, G and B for one minute
                Chain.ConfigureAll(0, true);
                for (int Seconds = 0; Seconds < 30; ++Seconds)
                {
                    Chain.ShiftToRight(0xff0000, true); Thread.Sleep(333);
                    Chain.ShiftToRight(0x00ff00, true); Thread.Sleep(333);
                    Chain.ShiftToRight(0x0000ff, true); Thread.Sleep(334);
                }
                Chain.ConfigureAll(0, true);
                for (int Seconds = 0; Seconds < 30; ++Seconds)
                {
                    Chain.ShiftToLeft(0x0000ff, true); Thread.Sleep(334);
                    Chain.ShiftToLeft(0x00ff00, true); Thread.Sleep(333);
                    Chain.ShiftToLeft(0xff0000, true); Thread.Sleep(333);
                }

                // Just changing colors
                Chain.ConfigureAll(0xff0000, true); Thread.Sleep(1000);
                Chain.ConfigureAll(0x00ff00, true); Thread.Sleep(1000);
                Chain.ConfigureAll(0x0000ff, true); Thread.Sleep(1000);

                // Loops nicely through all colors
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(Brightness, 0, 0, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(255, Brightness, 0, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll((byte)(255 - Brightness), 255, 0, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(0, 255, Brightness, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(0, (byte)(255 - Brightness), 255, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(Brightness, 0, 255, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll(255, Brightness, 255, true); Thread.Sleep(1);
                }
                for (byte Brightness = 0; Brightness < 255; ++Brightness)
                {
                    Chain.ConfigureAll((byte)(255 - Brightness), (byte)(255 - Brightness), (byte)(255 - Brightness), true); Thread.Sleep(1);
                }
            }
        }

    }
}
