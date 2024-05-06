#ifndef _STS_SERVO_POINT_H_
#define _STS_SERVO_POINT_H_

#include <Arduino.h>
#include <String.h>

class StsServoPoint
{
protected:
public:
    byte channel; // channel of the target servo
    int ms;       // time position in milliseconds
    int value;    // value of the target servo

    StsServoPoint(byte channel, int ms, int value) : channel(channel), ms(ms), value(value)
    {
    }

    ~StsServoPoint()
    {
    }
};

#endif