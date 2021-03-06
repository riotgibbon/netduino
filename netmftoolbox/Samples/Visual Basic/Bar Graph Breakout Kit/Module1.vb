Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.Netduino
Imports Toolbox.NETMF.Hardware

'  Copyright 2011-2012 Stefan Thoolen (http://www.netmftoolbox.com/)
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
        ' We got 4 74HC595's in a chain
        Dim IcChain As Ic74hc595 = New Ic74hc595(SPI_Devices.SPI1, Pins.GPIO_PIN_D10, 4)

        ' Led loop back and forward
        Do
            For Counter = 0 To 29
                IcChain.Pins(Counter).Write(True)
                Thread.Sleep(50)
                IcChain.Pins(Counter).Write(False)
            Next
            For Counter = 28 To 1 Step -1
                IcChain.Pins(Counter).Write(True)
                Thread.Sleep(50)
                IcChain.Pins(Counter).Write(False)
            Next
        Loop
    End Sub

End Module
