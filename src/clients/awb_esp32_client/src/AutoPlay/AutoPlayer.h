#ifndef _AUTOPLAYER_H_
#define _AUTOPLAYER_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "StsServoPoint.h"
#include "TimelineState.h"
#include "AutoPlayData.h"
#include "../StSerialServoManager.h"

using byte = unsigned char;

class AutoPlayer
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured;

    StSerialServoManager *_stSerialServoManager;
    AutoPlayData *_data;
    long _lastMsUpdate;
    long _lastPacketReceivedMillis = -1;
    int _actualTimelineIndex = -1;
    int _playPosInActualTimeline = 0;

    int _currentStateId = -1;
    int _stateSelectorStsServoChannel;
    bool _stateSelectorAvailable = false;
    long _lastStateCheckMillis = -1;

public:
    AutoPlayer(StSerialServoManager *stSerialServoManager, int stateSelectorStsServoChannel, TCallBackErrorOccured errorOccured) : _stSerialServoManager(stSerialServoManager), _stateSelectorStsServoChannel(stateSelectorStsServoChannel), _errorOccured(errorOccured)
    {
        _lastMsUpdate = millis();
        _data = new AutoPlayData();
    }

    ~AutoPlayer()
    {
    }

    bool isPlaying();
    TimelineState *getCurrentState();
    String getCurrentTimelineName();
    bool getStateSelectorAvailable();
    int getStateSelectorStsServoChannel();
    int selectedStateId();
    void startNewTimeline(int timelineIndex);
    void startNewTimelineForSelectedState();
    void setup();
    void update(bool servoHaveErrorsLikeTooHot);
    void stopBecauseOfIncommingPackage();
};

#endif
