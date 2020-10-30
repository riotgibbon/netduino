using System;
using Microsoft.SPOT.Hardware;

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
namespace Toolbox.NETMF.Hardware
{
    /// <summary>
    /// A chain of LPD8806-driven RGB LEDs
    /// </summary>
    public class Lpd8806Chain
    {
        /// <summary>
        /// Reference to the SPI connection
        /// </summary>
        private MultiSPI _Conn;

        /// <summary>
        /// The SPI write buffer
        /// </summary>
        private byte[] _Buffer;

        /// <summary>
        /// Amount of LEDs
        /// </summary>
        public int LedCount { get; protected set; }

        /// <summary>
        /// Defines a chain of LPD8806-driven RGB LEDs
        /// </summary>
        /// <param name="LedCount">The amount of LEDs in the chain (2 per IC)</param>
        /// <param name="SPI_Device">The SPI bus the chain is connected to</param>
        public Lpd8806Chain(int LedCount, SPI.SPI_module SPI_Device) : this(LedCount, SPI_Device, Cpu.Pin.GPIO_NONE, false) { }

        /// <summary>
        /// Defines a chain of LPD8806-driven RGB LEDs
        /// </summary>
        /// <param name="LedCount">The amount of LEDs in the chain (2 per IC)</param>
        /// <param name="SPI_Device">The SPI bus the chain is connected to</param>
        /// <param name="ChipSelect_Port">If there's a CS circuitry, specify it's pin</param>
        /// <param name="ChipSelect_ActiveState">If there's a CS circuitry, specify it's active state</param>
        public Lpd8806Chain(int LedCount, SPI.SPI_module SPI_Device, Cpu.Pin ChipSelect_Port, bool ChipSelect_ActiveState)
        {
            // Configures the SPI bus
            this._Conn = new MultiSPI(new SPI.Configuration(
                ChipSelect_Port: ChipSelect_Port,
                ChipSelect_ActiveState: ChipSelect_ActiveState,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: true,
                Clock_RateKHz: 10000,
                SPI_mod: SPI_Device
            ));
            
            // Stores the amount of LEDs
            this.LedCount = LedCount;

            // Creates a new buffer (final 3 bytes should always be 0 and tells the chain we're done for now)
            this._Buffer = new byte[LedCount * 3 + 3];

            // Turns off all LEDs
            this.ConfigureAll(0, true);
        }

        /// <summary>
        /// Shifts all LEDs to the right and adds a new one at the left
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ShiftToRight(byte Red, byte Green, byte Blue, bool Write = false)
        {
            for (int LedNo = this.LedCount - 2; LedNo >= 0; --LedNo)
            {
                this._Buffer[LedNo * 3 + 3] = this._Buffer[LedNo * 3 + 0];
                this._Buffer[LedNo * 3 + 4] = this._Buffer[LedNo * 3 + 1];
                this._Buffer[LedNo * 3 + 5] = this._Buffer[LedNo * 3 + 2];
            }
            this.Configure(0, Red, Green, Blue, Write);
        }

        /// <summary>
        /// Shifts all LEDs to the right and adds a new one at the left
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ShiftToRight(int Color, bool Write = false)
        {
            byte[] Colors = this._HexToRgb(Color);
            this.ShiftToRight(Colors[0], Colors[1], Colors[2], Write);
        }

        /// <summary>
        /// Shifts all LEDs to the left and adds a new one at the right
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ShiftToLeft(byte Red, byte Green, byte Blue, bool Write = false)
        {
            for (int LedNo = 0; LedNo < this.LedCount - 1; ++LedNo)
            {
                this._Buffer[LedNo * 3 + 0] = this._Buffer[LedNo * 3 + 3];
                this._Buffer[LedNo * 3 + 1] = this._Buffer[LedNo * 3 + 4];
                this._Buffer[LedNo * 3 + 2] = this._Buffer[LedNo * 3 + 5];
            }
            this.Configure(this.LedCount - 1, Red, Green, Blue, Write);
        }

        /// <summary>
        /// Shifts all LEDs to the left and adds a new one at the right
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ShiftToLeft(int Color, bool Write = false)
        {
            byte[] Colors = this._HexToRgb(Color);
            this.ShiftToLeft(Colors[0], Colors[1], Colors[2], Write);
        }

        /// <summary>
        /// Configures all LEDs to a specific color
        /// </summary>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ConfigureAll(byte Red, byte Green, byte Blue, bool Write = false)
        {
            for (int LedNo = 0; LedNo < this.LedCount; ++LedNo)
                this.Configure(LedNo, Red, Green, Blue);

            if (Write) this.Write();
        }

        /// <summary>
        /// Configures all LEDs to a specific color
        /// </summary>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void ConfigureAll(int Color, bool Write = false)
        {
            byte[] Colors = this._HexToRgb(Color);
            this.ConfigureAll(Colors[0], Colors[1], Colors[2], Write);
        }

        /// <summary>
        /// Configures a specific LED
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        /// <param name="Red">Red brightness (0 to 255)</param>
        /// <param name="Green">Green brightness (0 to 255)</param>
        /// <param name="Blue">Blue brightness (0 to 255)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void Configure(int LedNo, byte Red, byte Green, byte Blue, bool Write = false)
        {
            // Sets the first bit to 1 and shifts all other bits one position
            this._Buffer[LedNo * 3 + 2] = (byte)(0x80 | (Green >> 1));
            this._Buffer[LedNo * 3 + 1] = (byte)(0x80 | (Red >> 1));
            this._Buffer[LedNo * 3 + 0] = (byte)(0x80 | (Blue >> 1));

            if (Write) this.Write();
        }

        /// <summary>
        /// Configures a specific LED
        /// </summary>
        /// <param name="LedNo">The LED to configure (starts counting at 0)</param>
        /// <param name="Color">The color (0x000000 to 0xffffff)</param>
        /// <param name="Write">Do we have to write all LEDs immediately?</param>
        public void Configure(int LedNo, int Color, bool Write = false)
        {
            byte[] Colors = this._HexToRgb(Color);
            this.Configure(LedNo, Colors[0], Colors[1], Colors[2], Write);
        }

        /// <summary>
        /// Writes the status of all LEDs
        /// </summary>
        public void Write()
        {
            this._Conn.Write(this._Buffer);
        }

        /// <summary>
        /// Converts an integer color code to RGB
        /// </summary>
        /// <param name="HexColor">The integer hex color (0x000000 to 0xffffff)</param>
        /// <returns>A new byte[] { Red, Green, Blue }</returns>
        private byte[] _HexToRgb(int HexColor)
        {
            byte Red = (byte)((HexColor & 0xff0000) >> 16);
            byte Green = (byte)((HexColor & 0xff00) >> 8);
            byte Blue = (byte)(HexColor & 0xff);

            return new byte[] { Red, Green, Blue };
        }
    }
}
