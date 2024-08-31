#include <Arduino.h>
#include "PipButtons.h"
void PipButtons::setup()
{
    // define the touch pins as array
    for (int i = 0; i < 5; i++)
    {
        pinMode(touchPins[i], INPUT);
    }
    lastCheck = millis();
}
void PipButtons::loop()
{
    auto diff = millis() - lastCheck;
    lastCheck = millis();
    // define the touch pins as array
    for (int i = 0; i < 5; i++)
    {
        lastPressState[i] = actualPress[i];
        auto touchIn = touchRead(touchPins[i]);
        bool pressed = touchIn < 40;
        actualPress[i] = pressed;
        if (pressed)
        {
            // button is pressed
            if (holdTimeMs[i] == 0)
            {
                // button was just pressed
                pressEffect[i] = maxEffectValue;
            }
            if (diff > 0)
                holdTimeMs[i] += diff; // add the time since the last check to the hold time
        }
        else
        {
            // button is not pressed
            holdTimeMs[i] = 0;
        }
    }
    this->calculatePressEffectAnimations(diff);
}
bool PipButtons::isButtonPressed(int btnIndex)
{
    return actualPress[btnIndex] == true && lastPressState[btnIndex] == false;
}
int PipButtons::buttonHoldTimeMs(int btnIndex)
{
    return holdTimeMs[btnIndex];
}
void PipButtons::calculatePressEffectAnimations(int diffTime)
{
    lastPressEffectUpdate += diffTime;
    if (lastPressEffectUpdate > 300)
    {
        int newValues[4] = {0, 0, 0, 0};
        lastPressEffectUpdate -= 100;
        bool anyEffect = false;
        // calculate the press effect animations
        for (int i = 0; i < 4; i++)
        {
            int neighborValueLeft = i == 0 ? 0 : pressEffect[i - 1];
            int neighborValueRight = i == 3 ? 0 : pressEffect[i + 1];
            int newValue = (pressEffect[i] * 2 + neighborValueLeft + neighborValueRight) / 5;
            newValues[i] = newValue;
            anyEffect = anyEffect || newValue > 0;
        }
        // save the new values
        for (int i = 0; i < 4; i++)
            pressEffect[i] = newValues[i];
        if (anyEffect)
            neopixel->overrideButtonEffects(newValues);
    }
}
