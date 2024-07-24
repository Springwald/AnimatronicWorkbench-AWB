#ifndef _CUSTOM_CODE_H_
#define _CUSTOM_CODE_H_

#include <Arduino.h>
#include <String.h>
#include <Actuators/NeopixelManager.h>

class CustomCode
{
protected:
    NeopixelManager *neopixelManager;
    void setButtonLightByTouch(uint16_t btnIndex, uint8_t touchPin);

public:
    CustomCode(NeopixelManager *neopixelManager) : neopixelManager(neopixelManager)
    {
    }

    ~CustomCode()
    {
    }

    void setup();
    void loop();
};

#endif