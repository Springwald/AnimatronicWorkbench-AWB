#ifndef NEOPIXEL_MANAGER_h
#define NEOPIXEL_MANAGER_h

#include <Arduino.h>
#include <vector>
#include <FastLED.h>
#include <AwbDataImport/HardwareConfig.h>

class NeopixelManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;

#ifdef USE_NEOPIXEL
    CRGB leds[NEOPIXEL_COUNT];
    // FastLED_NeoPixel<NEOPIXEL_COUNT, NEOPIXEL_GPIO, NEO_GRB> strip; // <- FastLED NeoPixel version
    int ledsCount = NEOPIXEL_COUNT;
#else
    CRGB leds[1];
    int ledsCount = 1;
#endif

public:
    // the constructor
    NeopixelManager(TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured), _messageToShow(messageToShow)
    {
        auto brightness = 255;
#ifdef USE_NEOPIXEL
        FastLED.addLeds<NEOPIXEL, NEOPIXEL_GPIO>(leds, ledsCount);
#endif
        // strip.begin(); // initialize strip (required!)
        // strip.setBrightness(brightness);
    }

    void setSingleLED(uint16_t LEDnum, byte r, byte g, byte b);
    int getRgbVal(int ledIndex, int speed, int base);
};

#endif