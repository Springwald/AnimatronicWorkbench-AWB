<img src="images/AWB-Logo.png" style="width:100% margin: 5px;"/>

# AnimatronicWorkBench-AWB

An desktop editor and thin clients to choreograph animatronic movements

See project website at
https://daniel.springwald.de/post/2023/AnimatronicWorkbench-EN

## Setup

Check the hardware.h file for the correct setting:

### display settings

Choose the display type your hardware is using

For M5-Stack display hardware use:

```cpp
#define DISPLAY_M5STACK
```

For SSD1306 OLED displays e.g. *Waveshare Servo Driver with ESP32* use:

```cpp
#define DISPLAY_SSD1306
```

## Third party material

This project also contains modules from other authors - in unchanged or revised form.

For details see [the license info](LICENSE.md).
