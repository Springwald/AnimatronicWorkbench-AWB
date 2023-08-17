#ifndef _ACTUATOR_VALUE_H_
#define _ACTUATOR_VALUE_H_

#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

class ActuatorValue
{
protected:
public:
    String name;
    int id;
    int targetValue;
    int currentValue;
    int speed = -1;
    int acc = -1;

    int temperature;
    int load;

    ActuatorValue()
    {
    }

    ~ActuatorValue()
    {
    }
};

#endif