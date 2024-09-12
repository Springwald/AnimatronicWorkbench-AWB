#ifndef _PIP_BUTTONS_H_
#define _PIP_BUTTONS_H_
#include <Arduino.h>
#include <String.h>
#include <Actuators/NeopixelManager.h>
#include <Actuators/Mp3Player/Mp3PlayerYX5300Manager.h>
#include "PipNeopixel.h"
class PipButtons
{
protected:
    int touchPins[5] = {32, 33, 27, 14, 12};
    // the last state of the buttons as array
    bool lastPressState[5] = {false, false, false, false, false};
    bool actualPress[5] = {false, false, false, false, false};
    int holdTimeMs[5] = {0, 0, 0, 0, 0};
    int pressEffect[5] = {0, 0, 0, 0, 0}; // when a button is pressed, it will be set to 100, when released it will slow down to 0
    int maxEffectValue = 200;
    unsigned long lastCheck;
    unsigned long lastPressEffectUpdate;
    PipNeopixel *neopixel;
    void calculatePressEffectAnimations(int diffTime);

public:
    const int btnEvil = 0;
    const int btnRainbow = 1;
    const int btnEmpty3 = 2;
    const int btnSleep = 3;
    const int btnHoldBack = 4;
    PipButtons(PipNeopixel *neopixel) : neopixel(neopixel)
    {
    }
    ~PipButtons()
    {
    }
    void setup();
    void loop();
    bool isButtonPressed(int btnIndex);
    int buttonHoldTimeMs(int btnIndex);
};
#endif
