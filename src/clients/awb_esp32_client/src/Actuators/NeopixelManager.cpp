#include <Arduino.h>
#include "NeoPixelManager.h"



void NeopixelManager::setSingleLED(uint16_t LEDnum, byte r, byte g, byte b)
{
    leds[LEDnum] = CHSV(r, g, b);
    FastLED.show();
}

int NeopixelManager::getRgbVal(int ledIndex, int speed, int base)
{
    double value = 2 * (millis() % speed) / (speed * 1.0);
    if (value > 1)
        value = 2 - value;
    if (ledIndex == 1)
        value = 1 - value; // second led is inverted
    int intValue = (int)(value * base);
    return min(base, max(0, intValue));
}