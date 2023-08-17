#ifndef _ACTUAL_STATUS_INFORMATION_H_
#define _ACTUAL_STATUS_INFORMATION_H_

#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

class ActualStatusInformation
{
protected:
public:
    String autoPlayerCurrentStateName;
    String autoPlayerCurrentTimelineName;
    bool autoPlayerIsPlaying;
    int autoPlayerSelectedStateId;
    bool autoPlayerStateSelectorAvailable;
    int autoPlayerStateSelectorStsServoChannel;

    std::vector<ActuatorValue> *stsServoValues;
    std::vector<ActuatorValue> *pwmServoValues;

    ActualStatusInformation()
    {
        stsServoValues = new std::vector<ActuatorValue>();
        pwmServoValues = new std::vector<ActuatorValue>();
    }

    ~ActualStatusInformation()
    {
    }
};

#endif