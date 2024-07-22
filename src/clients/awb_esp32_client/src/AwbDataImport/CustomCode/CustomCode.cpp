
#include <Arduino.h>
#include "CustomCode.h"
#include <Actuators/NeopixelManager.h>

void CustomCode::setup()
{
}

void CustomCode::loop()
{
    auto touchIn1 = touchRead(4);
    if (touchIn1 < 40)
    {
        neopixelManager->setSingleLED(0, 128, 0, 0);
    }
    else
    {
        neopixelManager->setSingleLED(0, 0, 0, 128);
    }
}
