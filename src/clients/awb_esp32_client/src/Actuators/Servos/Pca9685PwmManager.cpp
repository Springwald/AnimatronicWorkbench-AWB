
#include <Arduino.h>
#include "Adafruit_PWMServoDriver.h"
#include "Pca9685PwmManager.h"

void Pca9685PwmManager::writeMicroseconds(uint8_t adr, uint8_t num, uint16_t microSeconds)
{
    if (adr != 0x40)
    {
        _errorOccured("Pca9685PwmManager::writeMicroseconds: adr != 0x40 not implemented yet");
        return;
    }
    this->_pwm.writeMicroseconds(num, microSeconds);
}

void Pca9685PwmManager::setOscillatorFrequency(uint8_t adr, uint32_t freq)
{
    if (adr != 0x40)
    {
        _errorOccured("Pca9685PwmManager::writeMicroseconds: adr != 0x40 not implemented yet");
        return;
    }
    this->_pwm.reset();
    this->_pwm.begin();
    this->_pwm.setOscillatorFrequency(freq);
    this->_pwm.setPWMFreq(SERVO_FREQ); // Analog servos run at ~50 Hz updates
    this->_pwm.reset();
    this->_pwm.begin();
    this->_pwm.setOscillatorFrequency(freq);
    this->_pwm.setPWMFreq(SERVO_FREQ); // Analog servos run at ~50 Hz updates
}

void Pca9685PwmManager::setTargetValue(int channel, int value, String name)
{
    for (int f = 0; f < this->_pwmServos->size(); f++)
        if (this->_pwmServos->at(f).channel == channel)
        {
            this->_pwmServos->at(f).targetValue = value; // set servo target value
            return;
        }
    _errorOccured("Pca9685Pwm servo for channel " + String(channel) + " not defined!");
}

void Pca9685PwmManager::updateActuators(boolean anyServoWithGlobalFaultHasCiriticalState)
{
    for (int i = 0; i < this->_pwmServos->size(); i++)
    {
        // get a pointer to the current servo
        auto *servo = &this->_pwmServos->at(i);

        if (servo->targetValue == -1 || anyServoWithGlobalFaultHasCiriticalState == true)
        {
            // turn servo off
            // todo: implement
        }
        else
        {
            // set new target value if changed
            if (servo->currentValue != servo->targetValue)
            {

                uint8_t servoNo = servo->channel;
                uint16_t microseconds = servo->targetValue;

                _pwm.writeMicroseconds(servoNo, microseconds);
                servo->currentValue = servo->targetValue;
            }
        }
    }
}
