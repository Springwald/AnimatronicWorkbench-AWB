#ifndef _SERVOS_H_
#define _SERVOS_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "ServoState.h"
#include "ServoConfig.h"
#include "Servo.h"

class Servos
{

public:
    std::vector<Servo> *allServos;

    Servos()
    {
        allServos = new std::vector<Servo>();
    }

    ~Servos()
    {
    }

    void addServo(Servo servo);
    std::vector<Servo> *getServosByType(ServoConfig::ServoTypes servoType);
};

#endif