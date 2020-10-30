Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF.Hardware

'  Copyright 2012 Stefan Thoolen (http://www.netmftoolbox.com/)
'
'  Licensed under the Apache License, Version 2.0 (the "License");
'  you may not use this file except in compliance with the License.
'  You may obtain a copy of the License at
'
'      http://www.apache.org/licenses/LICENSE-2.0
'
'  Unless required by applicable law or agreed to in writing, software
'  distributed under the License is distributed on an "AS IS" BASIS,
'  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'  See the License for the specific language governing permissions and
'  limitations under the License.
Module Module1

    Sub Main()
        ' There's a 1M strip (32 LEDs) connected to the first SPI bus on the Netduino
        Dim Chain As Lpd8806Chain = New Lpd8806Chain(32, SPI_Devices.SPI1)

        ' Repeats all demos infinitely
        Do
            ' Shows a red, green and blue LED
            Chain.Configure(0, &HFF0000)
            Chain.Configure(1, &HFF00)
            Chain.Configure(2, &HFF)
            Chain.Write()
            Thread.Sleep(5000)

            ' Loops R, G and B for one minute
            Chain.ConfigureAll(0, True)
            For Seconds As Integer = 0 To 30
                Chain.ShiftToRight(&HFF0000, True) : Thread.Sleep(333)
                Chain.ShiftToRight(&HFF00, True) : Thread.Sleep(334)
                Chain.ShiftToRight(&HFF, True) : Thread.Sleep(333)
            Next
            Chain.ConfigureAll(0, True)
            For Seconds As Integer = 0 To 30
                Chain.ShiftToLeft(&HFF, True) : Thread.Sleep(333)
                Chain.ShiftToLeft(&HFF00, True) : Thread.Sleep(334)
                Chain.ShiftToLeft(&HFF0000, True) : Thread.Sleep(333)
            Next

            ' Just changing colors
            Chain.ConfigureAll(&HFF0000, True) : Thread.Sleep(1000)
            Chain.ConfigureAll(&HFF00, True) : Thread.Sleep(1000)
            Chain.ConfigureAll(&HFF, True) : Thread.Sleep(1000)

            ' Loops nicely through all colors
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(CByte(Brightness), 0, 0, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(255, CByte(Brightness), 0, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(CByte(255 - Brightness), 255, 0, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(0, 255, CByte(Brightness), True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(0, CByte(255 - Brightness), 255, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(CByte(Brightness), 0, 255, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(255, CByte(Brightness), 255, True) : Thread.Sleep(1)
            Next
            For Brightness As Integer = 0 To 255
                Chain.ConfigureAll(CByte(255 - Brightness), CByte(255 - Brightness), CByte(255 - Brightness), True) : Thread.Sleep(1)
            Next
        Loop
    End Sub

End Module
