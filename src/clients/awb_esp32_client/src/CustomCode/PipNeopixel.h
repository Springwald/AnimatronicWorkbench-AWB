#ifndef _PIP_NEOPIXEL_H_
#define _PIP_NEOPIXEL_H_
#include <Arduino.h>
#include <String.h>
#include <Actuators/NeopixelManager.h>
class PipNeopixel
{
protected:
    NeopixelManager *neopixelManager;
    unsigned long lastUpdateMs = 0;
    int eyeStateDurationMs = 0;
    int actualEyeState = 0;
    int smallEyeMsCounter = 0;
    int noButtonEffectsSince = 0;
    const int bigEyeMaxValue = 50;
    const int smallEyeStart = 8;               // id of the first small eye
    const int bigEyeStart = smallEyeStart + 3; // id of the first big eye led
    const int lastLeds = bigEyeStart + 5;      // end id of all leds
    void updateButtons();
    void updateBigEye();
public:
    static const int BigEyeStateDefault = 0;
    static const int BigEyeStateOff = 1;
    static const int BigEyeStateWakeUp = 2;
    static const int BigEyeStateGoSleep = 3;
    static const int BigEyeStateSleeping = 4;
    static const int BigEyeStateRainbow = 5;
    static const int BigEyeStateEvil = 6;
    PipNeopixel(NeopixelManager *neopixelManager) : neopixelManager(neopixelManager)
    {
    }
    ~PipNeopixel()
    {
    }
    void setup();
    void loop();
    void setEyeState(int state);
    void overrideButtonEffects(int effects[4]);
};
#endif
