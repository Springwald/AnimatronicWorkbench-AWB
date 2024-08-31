#include <Arduino.h>
#include "CustomCode.h"
#include <Actuators/NeopixelManager.h>
#include "PipNeopixel.h"
void PipNeopixel::setup()
{
    lastUpdateMs = millis();
    for (int i = 0; i <= lastLeds; i++)
        this->neopixelManager->setSingleLED(i, 0, 0, 0);
}
void PipNeopixel::loop()
{
    auto diff = millis() - lastUpdateMs;
    lastUpdateMs = millis();
    updateButtons();
    updateBigEye();
    // set the small eyes to random colors
    smallEyeMsCounter += diff;
    if (smallEyeMsCounter > 1000)
    {
        smallEyeMsCounter = 0;
        if (actualEyeState == BigEyeStateSleeping)
        {
            // turn off the small eyes
            for (int i = smallEyeStart; i < bigEyeStart; i++)
                this->neopixelManager->setSingleLED(i, 0, 0, 0);
        }
        else
        {
            // set the small eyes to random colors
            for (int i = smallEyeStart; i < bigEyeStart; i++)
            {
                auto rnd = random(0, 5);
                if (rnd < 3)
                {
                    this->neopixelManager->setSingleLED(i, 0, 0, 0);
                }
                else
                {
                    this->neopixelManager->setSingleLED(i, random(0, 100), random(0, 100), random(0, 100));
                }
            }
        }
    }
}
void PipNeopixel::updateButtons()
{
    if (noButtonEffectsSince++ < 10)
        return; // do not update the buttons if effects are active
    int maxValue = 20;
    double valueRaw = abs(((millis() % 3000) / 3000.0 * maxValue) - maxValue / 2);
    uint8_t value = valueRaw;
    for (int i = 0; i < 8; i++)
    {
        this->neopixelManager->setSingleLED(i, value, value, value);
    }
}
void PipNeopixel::overrideButtonEffects(int effects[4])
{
    for (int i = 0; i < 4; i++)
    {
        noButtonEffectsSince = 0;
        this->neopixelManager->setSingleLED(i * 2, effects[i], effects[i] / 2, effects[i] / 3);
        this->neopixelManager->setSingleLED(i * 2 + 1, effects[i], effects[i] / 2, effects[i] / 3);
    }
}
void PipNeopixel::updateBigEye()
{
    if (actualEyeState == BigEyeStateSleeping)
    {
        double valueRaw = abs((((millis() % 3000) / 3000.0) * bigEyeMaxValue / 4) - bigEyeMaxValue / 8);
        uint8_t value = valueRaw;
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, value, value, value);
    }
    else if (actualEyeState == BigEyeStateWakeUp)
    {
        uint8_t value = min(bigEyeMaxValue, eyeStateDurationMs / 10);
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, value, value, value);
    }
    else if (actualEyeState == BigEyeStateGoSleep)
    {
        uint8_t value = bigEyeMaxValue / 3 - (millis() % 1000) / 1000.0 * bigEyeMaxValue / 3;
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, value, value, value);
    }
    else if (actualEyeState == BigEyeStateOff)
    {
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, 0, 0, 0);
    }
    else if (actualEyeState == BigEyeStateEvil)
    {
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, bigEyeMaxValue, 0, 0);
    }
    else if (actualEyeState == BigEyeStateRainbow)
    {
        uint8_t hue = (millis() / 10) % 255;
        this->neopixelManager->rainbow(bigEyeStart, lastLeds - bigEyeStart, hue, 10);
    }
    else
    {
        for (int i = bigEyeStart; i <= lastLeds; i++)
            this->neopixelManager->setSingleLED(i, bigEyeMaxValue, bigEyeMaxValue, bigEyeMaxValue);
    }
}
void PipNeopixel::setEyeState(int state)
{
    if (state != actualEyeState)
    {
        actualEyeState = state;
        eyeStateDurationMs = 0;
    }
}
