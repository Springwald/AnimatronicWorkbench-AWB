#ifndef _SERVO_CONFIG_H_
#define _SERVO_CONFIG_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include "RelaxRange.h"
#include <string>
#include "Servo.h"

using byte = unsigned char;

class ServoConfig
{

public:
    const int NOT_SUPPORTED = -1;

    enum class ServoTypes
    {
        PWM_SERVO = 0,
        STS_SERVO = 1,
        SCS_SERVO = 2
    };

public:
    ServoTypes type;         /// the type of the servo
    String const title;      /// the name of the servo
    int channel;             /// the channel (PWM servo) or ID (serial servo) of the servo
    uint i2cAdress;          /// the I2C adress (only for PWM servos)
    int minValue;            /// the minimum value of the servo
    int maxValue;            /// the maximum value of the servo
    int maxTorque;           /// the maximum torque of the servo
    int maxTemp;             ///  the maximum temperature of the servo in degrees Celsius
    int defaultValue;        /// the default value of the servo
    int defaultAcceleration; /// the acceleration of the servo
    int defaultSpeed;        /// the speed of the servo
    bool globalFault;        /// if this servo is to hot or overloaded, should all servos stop?

    std::vector<RelaxRange> *relaxRanges; // the relax ranges of the servo

    ServoConfig(
        ServoTypes servoType,
        String const title,
        int channel,
        uint i2cAdress,
        int minValue,
        int maxValue,
        int maxTemp,
        int maxTorque,
        int defaultValue,
        int defaultAcceleration,
        int defaultSpeed,
        bool globalFault,
        std::vector<RelaxRange> *relaxRanges)

        : type(servoType),
          title(title),
          channel(channel),
          i2cAdress(i2cAdress),
          minValue(minValue),
          maxValue(maxValue),
          maxTemp(maxTemp),
          maxTorque(maxTorque),
          defaultValue(defaultValue),
          defaultAcceleration(defaultAcceleration),
          defaultSpeed(defaultSpeed),
          globalFault(globalFault),
          relaxRanges(relaxRanges)
    {
    }

    ~ServoConfig()
    {
    }
};

#endif