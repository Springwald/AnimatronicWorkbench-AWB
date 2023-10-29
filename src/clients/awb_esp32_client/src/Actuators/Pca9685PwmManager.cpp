
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
    

    int index = -1;
    for (int f = 0; f < this->pwmServoValues->size(); f++)
    {
        if (this->pwmServoValues->at(f).id == channel)
        {
            // pwm servo already added
            index = f;
            break;
        }
    }
    if (index == -1)
    {
        // pwm servo not added yet. Insert it now
        this->_messageToShow("added pwm servo id " + String(channel) + " '" + name + "' to list.");
        ActuatorValue actuatorValue;
        actuatorValue.id = channel;
        actuatorValue.name = name;
        this->pwmServoValues->push_back(actuatorValue);
        index = this->pwmServoValues->size() - 1;
    }

    // set servo target value
    this->pwmServoValues->at(index).targetValue = value;
}

void Pca9685PwmManager::updateActuators()
{
    for (int i = 0; i < this->pwmServoValues->size(); i++)
    {
        // get a pointer to the current servo
        ActuatorValue *servo = &this->pwmServoValues->at(i);
        if (servo->targetValue == -1)
        {
            // turn servo off
            // todo: implement
        }
        else
        {
            // set new target value if changed
            if (servo->currentValue != servo->targetValue)
            {
                int speed = servo->speed;
                int acc = servo->acc;

                if (speed == -1 && acc == -1)
                {
                    // use default values for speed and acc
                }
                else
                {
                }

                uint8_t servoNo = servo->id;
                uint16_t microseconds = servo->targetValue;
                _pwm.writeMicroseconds(servoNo, microseconds);
                servo->currentValue = servo->targetValue;
            }
        }
    }
}
