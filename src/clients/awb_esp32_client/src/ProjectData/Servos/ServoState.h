#ifndef _SERVO_STATE_H_
#define _SERVO_STATE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include "RelaxRange.h"
#include <string>
#include "Servo.h"

class ServoState
{

public:
    // actual states of the servo
    int targetSpeed = -1; /// the speed of the actuator (should be updated in the next update cycle)
    int targetAcc = -1;   /// the acceleration of the actuator (should be updated in the next update cycle)
    int targetValue = -1; /// the target value of the actuator (should be updated in the next update cycle)

    bool isFault = false; /// if this servo is to hot or overloaded

    int temperature = -1;         /// the temperature of the actuator
    int load = -1;                /// the actual read load of the actuator
    int maxLoad = -1;             /// the maximum read load of the actuator
    int minLoad = -1;             /// the minimum read load of the actuator
    int currentValue = -1;        /// the current value of the actuator (updated in the last update cycle)
    long isFaultCountDownMs = 0;  /// the time in milliseconds when the fault will be cleared
    long lastFaultMs = 0;         /// the time in milliseconds when the fault was detected
    String lastFaultMessage = ""; /// the message of the last fault
};

#endif