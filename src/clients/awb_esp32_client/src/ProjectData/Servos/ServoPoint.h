#ifndef _SERVO_POINT_H_
#define _SERVO_POINT_H_

#include <Arduino.h>
#include <String.h>

class ServoPoint
{
protected:
public:
    String servoId; // the project wide unique id of the target servo
    int ms;         // time position in milliseconds
    int value;      // value of the target servo

    ServoPoint(String servoId, int ms, int value) : servoId(servoId), ms(ms), value(value)
    {
    }

    ~ServoPoint()
    {
    }
};

#endif