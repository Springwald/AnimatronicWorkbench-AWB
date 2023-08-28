#ifndef _ACTUAL_STATUS_INFORMATION_H_
#define _ACTUAL_STATUS_INFORMATION_H_

#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

/**
 * the actual hardware- and autoplayer-status
 */
class ActualStatusInformation
{
protected:
public:
    String autoPlayerCurrentStateName;          /// the name of the current auto player timeline filter state
    String autoPlayerCurrentTimelineName;       /// the name of the current timeline played in auto player
    bool autoPlayerIsPlaying;                   /// true if auto player is playing
    int autoPlayerSelectedStateId;              /// the id of the current auto player timeline filter state
    bool autoPlayerStateSelectorAvailable;      /// true if a hardware auto player timeline filter state selector is available
    int autoPlayerStateSelectorStsServoChannel; /// the channel of the sts servo for the hardware auto player timeline filter state selector

    std::vector<ActuatorValue> *stsServoValues; /// the current sts servo status
    std::vector<ActuatorValue> *pwmServoValues; /// the current pwm servo status

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