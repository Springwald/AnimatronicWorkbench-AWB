#ifndef _PCA9685_PWM_SERVO_H_
#define _PCA9685_PWM_SERVO_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>

using byte = unsigned char;

class Pca9685PwmServo
{

public:
    // initial values for the servo
    String title;     /// the name of the servo
    int channel;      /// the channel of the servo
    int i2cAdress;    /// the i2c adress of the servo
    int minValue;     /// the minimum value of the servo
    int maxValue;     /// the maximum value of the servo
    int defaultValue; /// the default value of the servo

    // actual states of the servo
    int targetValue;       /// the target value of the actuator (should be updated in the next update cycle)
    int currentValue = -1; /// the current value of the actuator (updated in the last update cycle)

    Pca9685PwmServo(int i2cAdress, int channel, String const title, int minValue, int maxValue, int defaultValue) : i2cAdress(i2cAdress), channel(channel), title(title), minValue(minValue), maxValue(maxValue), defaultValue(defaultValue)
    {
        targetValue = defaultValue;
    }

    ~Pca9685PwmServo()
    {
    }
};

#endif