#ifndef _STS_SCS_SERVO_H_
#define _STS_SCS_SERVO_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>

using byte = unsigned char;

class StsScsServo
{

public:
    // initial values for the servo
    int channel;             /// the channel of the servo
    String title;            /// the name of the servo
    int minValue;            /// the minimum value of the servo
    int maxValue;            /// the maximum value of the servo
    int maxTorque;           /// the maximum torque of the servo
    int maxTemp;             ///  the maximum temperature of the servo in degrees Celsius
    int defaultValue;        /// the default value of the servo
    int defaultAcceleration; /// the acceleration of the servo
    int defaultSpeed;        /// the speed of the servo
    bool globalFault;        /// if this servo is to hot or overloaded, should all servos stop?

    // actual states of the servo
    int targetSpeed = -1; /// the speed of the actuator (should be updated in the next update cycle)
    int targetAcc = -1;   /// the acceleration of the actuator (should be updated in the next update cycle)
    int targetValue;      /// the target value of the actuator (should be updated in the next update cycle)

    int temperature = -1;  /// the temperature of the actuator
    int load = -1;         /// the load of the actuator
    int currentValue = -1; /// the current value of the actuator (updated in the last update cycle)
    bool isFault = false;  /// if this servo is to hot or overloaded

    StsScsServo(int channel, String const title, int minValue, int maxValue, int maxTemp, int maxTorque, int defaultValue, int defaultAcceleration, int defaultSpeed, bool globalFault) : channel(channel), title(title), minValue(minValue), maxValue(maxValue), maxTemp(maxTemp), maxTorque(maxTorque), defaultValue(defaultValue), defaultAcceleration(defaultAcceleration), defaultSpeed(defaultSpeed), globalFault(globalFault)
    {
        targetValue = defaultValue;
    }

    ~StsScsServo()
    {
    }
};

#endif