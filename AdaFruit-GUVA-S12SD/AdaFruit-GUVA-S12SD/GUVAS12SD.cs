using System;
using Microsoft.SPOT;
using MSH = Microsoft.SPOT.Hardware;
using SLH = SecretLabs.NETMF.Hardware;
using System.Threading;

namespace AdaFruit_GUVA_S12SD
{
    struct GUVA_Reading
    {
        public int sensorReading;
        public double sensorVoltage;
        public double uvIndex;

    }
 
    class GUVAS12SD
    {
        protected SLH.AnalogInput _analogPort;
        protected double _powerVoltage;

        public GUVAS12SD(MSH.Cpu.Pin analogPort, double powerVoltage=5.0)
        {
            _analogPort = new SLH.AnalogInput(analogPort);
            _powerVoltage = powerVoltage;
        }
        public GUVA_Reading Read()
        {
            int sample=0;

            var sensorReading = _analogPort.Read();

            double sensorVoltage = (double)sensorReading / 1024 * _powerVoltage;
            int uvIndex = (int)(sensorVoltage / 0.1);

            return new GUVA_Reading { sensorReading=sensorReading, sensorVoltage = sensorVoltage, uvIndex = uvIndex };
        }

    }
}
