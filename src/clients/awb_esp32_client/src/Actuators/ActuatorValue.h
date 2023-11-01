#ifndef _ACTUATOR_VALUE_H_
#define _ACTUATOR_VALUE_H_

#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

/**
 * the status of a single actuator e.g. a servo
 */
class ActuatorValue
{
protected:
public:
    String name;           /// the name of the actuator
    int id;                /// the technical id of the actuator
    int targetValue;       /// the target value of the actuator (should be updated in the next update cycle)
    int currentValue = -1; /// the current value of the actuator (updated in the last update cycle)
    int speed = -1;        /// the speed of the actuator (should be updated in the next update cycle)
    int acc = -1;          /// the acceleration of the actuator (should be updated in the next update cycle)

    int temperature = -1; /// the temperature of the actuator
    int load = -1;        /// the load of the actuator

    ActuatorValue()
    {
    }

    ~ActuatorValue()
    {
    }
};

#endif