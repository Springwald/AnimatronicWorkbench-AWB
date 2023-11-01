#ifndef _PWM_SERVO_POINT_H_
#define _PWM_SERVO_POINT_H_

#include <Arduino.h>
#include <String.h>

class Pca9685PwmServoPoint
{
protected:
public:
    uint i2cAdress;
    byte channel; // channel of the target servo
    int ms;       // time position in milliseconds
    int value;    // value of the target servo

    Pca9685PwmServoPoint(uint i2cAdress, byte channel, int ms, int value) : i2cAdress(i2cAdress), channel(channel), ms(ms), value(value)
    {
    }

    ~Pca9685PwmServoPoint()
    {
    }
};

#endif