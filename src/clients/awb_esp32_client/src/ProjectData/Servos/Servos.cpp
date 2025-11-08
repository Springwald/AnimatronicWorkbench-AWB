#include <Arduino.h>
#include "Servos.h"

void Servos::addServo(Servo servo)
{
    allServos->push_back(servo);
}

std::vector<Servo> *Servos::getServosByType(ServoConfig::ServoTypes servoType)
{
    auto result = new std::vector<Servo>();
    for (int servoIndex = 0; servoIndex < this->allServos->size(); servoIndex++)
    {
        if (this->allServos->at(servoIndex).config->type == servoType)
            result->push_back(this->allServos->at(servoIndex));
    }
    return result;
}
