# GUVA-S12SD UV SENSOR

https://uk.rs-online.com/web/p/sensor-development-tools/1245472



http://www.pibits.net/code/raspberry-pi-and-guva-s12sd-uv-sensor.php

http://www.arduinoprojects.net/sensor-projects/arduino-uno-guva-s12sd-uv-sensor-example.php


https://www.adafruit.com/product/1918

```
To use, power the sensor and op-amp by connecting V+ to 2.7-5.5VDC and GND to power ground. Then read the analog signal from the OUT pin. The output voltage is: Vo = 4.3 * Diode-Current-in-uA. So if the photocurrent is 1uA (9 mW/cm^2), the output voltage is 4.3V. You can also convert the voltage to UV Index by dividing the output voltage by 0.1V. So if the output voltage is 0.5V, the UV Index is about 5.
```


1. GND: 0V (Ground)
2. VCC: 3.3V to 5.5V
3. OUT: 0V to 1V ( 0 to 10 UV Index)


powerVoltage = 5.0;
sensorVoltage = sensorValue/1024*5.0;
uvIndex = sensorVoltage / 0.1;

           
![image](https://raw.githubusercontent.com/riotgibbon/netduino/main/AdaFruit-GUVA-S12SD/AdaFruit-GUVA-S12SD/docs/UV_index.png)