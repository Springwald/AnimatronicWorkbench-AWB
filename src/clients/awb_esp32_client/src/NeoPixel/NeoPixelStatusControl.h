#ifndef neopixel_status_control_h
#define neopixel_status_control_h

#include <Arduino.h>
#include <Adafruit_NeoPixel.h>
#include "AwbDataImport/HardwareConfig.h"

class NeoPixelStatusControl
{

private:
#define ERROR_BRIGHTNESS 128
#define IDLE_BRIGHTNESS 10
#define ACIVITY_BRIGHTNESS 10

    Adafruit_NeoPixel _matrix = Adafruit_NeoPixel(STATUS_RGB_LED_NUMPIXELS, STATUS_RGB_LED_GPIO, NEO_GRB + NEO_KHZ800);
    int _activityStandardDurationMs = 150;
    int _activityDurationMs = 0;
    int _state = STATE_IDLE;
    int _stateDurationMs = 0;
    bool _isStartupRed = false;
    long _lastUpdateMs = millis(); // start with a delay to see the fake startup error state
    int _lastGroguAnimationMs = millis();

    void setSingleLED(uint16_t LEDnum, uint32_t c);
    int getRgbVal(int ledIndex, int speed, int base);
    void groguSpecialAnimation();

public:
    static constexpr int STATE_IDLE = 0;
    static constexpr int STATE_ALARM = 1;

    NeoPixelStatusControl()
    {
        _matrix.setBrightness(255);
    };

    void setState(int state, int durationMs);
    void setStateIdle();
    void showActivity();
    void update();
    void setStartUpAlert(); /// show alarm neopixel on startup to see unexpected restarts
};

#endif
