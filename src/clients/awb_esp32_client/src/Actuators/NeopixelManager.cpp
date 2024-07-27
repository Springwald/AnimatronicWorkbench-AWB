#include <Arduino.h>
#include "NeoPixelManager.h"

void NeopixelManager::setSingleLED(uint16_t LEDnum, uint8_t r, uint8_t g, uint8_t b)
{
#ifdef USE_NEOPIXEL
    leds[LEDnum] = CRGB(r, g, b);
    FastLED.show();
#endif
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