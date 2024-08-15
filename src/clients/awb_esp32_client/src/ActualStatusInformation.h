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

    String activeTimelineStateIdsByInput; /// the active timeline states by input
    String inputStates;                   /// the active timeline states by input
    String lastSoundPlayed;               /// the last sound played

    ActualStatusInformation()
    {
    }

    ~ActualStatusInformation()
    {
    }
};

#endif