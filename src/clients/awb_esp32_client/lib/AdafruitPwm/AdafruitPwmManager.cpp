
#include <Arduino.h>
#include "Adafruit_PWMServoDriver.h"
#include "AdafruitPwmManager.h"

void AdafruitPwmManager::writeMicroseconds(uint8_t adr, uint8_t num, uint16_t microSeconds)
{
    if (adr != 0x40)
    {
        _errorOccured("AdafruitPwmManager::writeMicroseconds: adr != 0x40 not implemented yet");
        return;
    }
    this->_pwm.writeMicroseconds(num, microSeconds);
}

void AdafruitPwmManager::setOscillatorFrequency(uint8_t adr, uint32_t freq)
{
    if (adr != 0x40)
    {
        _errorOccured("AdafruitPwmManager::writeMicroseconds: adr != 0x40 not implemented yet");
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
