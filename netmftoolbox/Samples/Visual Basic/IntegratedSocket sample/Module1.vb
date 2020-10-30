Imports Microsoft.SPOT
Imports Microsoft.SPOT.Hardware
Imports SecretLabs.NETMF.Hardware
Imports SecretLabs.NETMF.Hardware.NetduinoPlus
Imports Toolbox.NETMF.NET

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
        ' Defines the socket, including the remote host and port
        Dim Socket As SimpleSocket = New IntegratedSocket("www.netmftoolbox.com", 80)

        ' Connects to the socket
        Socket.Connect()

        ' Does a plain HTTP request
        Socket.Send("GET /helloworld/ HTTP/1.1\r\n")
        Socket.Send("Host: " & Socket.Hostname & "\r\n")
        Socket.Send("Connection: Close\r\n")
        Socket.Send("\r\n")

        ' Prints all received data to the debug window, until the connection is terminated and there's no data left anymore
        Dim Text As String
        Do While Socket.IsConnected Or Socket.BytesAvailable > 0
            Text = Socket.Receive()
            If Not Text = "" Then Debug.Print(Text)
        Loop

        ' Closes down the socket
        Socket.Close()
    End Sub

End Module
