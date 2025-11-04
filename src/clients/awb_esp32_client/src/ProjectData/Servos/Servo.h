#ifndef _SERVO_H_
#define _SERVO_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "ServoState.h"
#include "ServoConfig.h"

class Servo
{
public:
    String id;    /// the project wide unique id of the servo
    String title; /// the name of the servo

    ServoState *state = nullptr;   /// the actual state of the servo
    ServoConfig *config = nullptr; /// the configuration of the servo

    Servo(String const id, ServoConfig *config) : id(id), config(config)
    {
        this->title = config->title;
        this->state = new ServoState();
        this->state->currentValue = this->config->defaultValue;
        this->state->targetValue = this->config->defaultValue;
        this->state->targetSpeed = this->config->defaultSpeed;
        this->state->targetAcc = this->config->defaultAcceleration;
    }

    ~Servo()
    {
    }
};

#endif