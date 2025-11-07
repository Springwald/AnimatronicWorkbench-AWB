#ifndef _AUTOPLAYER_H_
#define _AUTOPLAYER_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "../ProjectData/Servos/ServoPoint.h"
#include "../ProjectData/TimelineStateReference.h"
#include "../AwbDataImport/ProjectData.h"
#include "../Actuators/Servos/StScsSerialServoManager.h"
#include "../Actuators/Servos/Pca9685PwmManager.h"
#include "../Actuators/Mp3Player/Mp3PlayerYX5300Manager.h"
#include "../Actuators/Mp3Player/Mp3PlayerDfPlayerMiniManager.h"
#include "../Actuators/InputManager.h"
#include "../Debugging.h"

using byte = unsigned char;

class AutoPlayer
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured; // the error occured callback

    StScsSerialServoManager *_stSerialServoManager;              // the STS serial servo manager
    StScsSerialServoManager *_scSerialServoManager;              // the SCS serial servo manager
    Pca9685PwmManager *_pca9685PwmManager;                       // the PCA9685 PWM manager
    Mp3PlayerYX5300Manager *_mp3PlayerYX5300Manager;             // the MP3 player manager on YX5300
    Mp3PlayerDfPlayerMiniManager *_mp3PlayerDfPlayerMiniManager; // the MP3 player manager on DFPlayer Mini
    InputManager *_inputManager;                                 // the input manager
    ProjectData *_data;                                          // the data exported by Animatronic Workbench Studio
    Debugging *_debugging;                                       /// the debugging class

    long _lastMsUpdate;                  // millis() of last update
    long _lastPacketReceivedMillis = -1; // millis() of last received packet
    int _actualTimelineIndex = -1;       // the index of the actual playing timeline
    long _playPosInActualTimeline = 0;   // the actual play position in the actual playing timeline (in milliseconds)
    const bool fillUpStart = false;      // if true, the timeline will be filled up with the start point

    int _lastSoundPlayed = -1;

    // int _currentStateId = -1; // the actual selected state id

    int *_timeLineStateForcedOnceByRemoteOrCustomCodeId = nullptr;      // Here the globale timeline state can be overwritten by custom code a single time. Buttons and other inputs defined in AWB Studio will be ignored.
    int *_timeLineStateForcedPermanentByRemoteOrCustomCodeId = nullptr; // Here the globale timeline state can be overwritten by custom code permanent. Buttons and other inputs defined in AWB Studio will be ignored.

    int calculateServoValueFromTimeline(String const servoId, std::vector<ServoPoint> *servoPoints);

public:
    AutoPlayer(
        ProjectData *data,
        StScsSerialServoManager *stSerialServoManager,
        StScsSerialServoManager *scSerialServoManager,
        Pca9685PwmManager *pca9685PwmManager,
        Mp3PlayerYX5300Manager *mp3PlayerYX5300Manager,
        Mp3PlayerDfPlayerMiniManager *mp3PlayerDfPlayerMiniManager,
        InputManager *inputManager,
        TCallBackErrorOccured errorOccured,
        Debugging *debugging) : _data(data),
                                _stSerialServoManager(stSerialServoManager),
                                _scSerialServoManager(scSerialServoManager),
                                _pca9685PwmManager(pca9685PwmManager),
                                _mp3PlayerYX5300Manager(mp3PlayerYX5300Manager),
                                _mp3PlayerDfPlayerMiniManager(mp3PlayerDfPlayerMiniManager),
                                _inputManager(inputManager),
                                _errorOccured(errorOccured),
                                _debugging(debugging)
    {
        _lastMsUpdate = millis();
    }

    ~AutoPlayer() {}

    String getStatesDebugInfo();

    /**
     * Returns true if the player is playing a timeline
     */
    bool isPlaying();

    String getLastSoundPlayed();

    /**
     * Force the globale timeline state by custom code a single time. Buttons and other inputs defined in AWB Studio will be ignored for state selection.
     */
    void forceTimelineState(bool permanent, int *stateId);

    /**
     * The name of the timeline actually playing
     */
    String getCurrentTimelineName(bool extendWithTimelineIndex);

    /**
     * The name of the actual timeline state
     */
    int getCurrentTimelineStateId();

    /**
     * Check, if the selected state is active by inputs
     */
    std::vector<TimelineState> getActiveStatesByInputs();

    /**
     * starts the timeline with the given index
     */
    void startNewTimelineByIndex(int timelineIndex);

    /**
     * starts the timeline with the given name
     */
    void startNewTimelineByName(String name);

    /**
     * starts a new timeline suitable for the selected state
     */
    void startNewTimelineForSelectedState();

    /**
     * Sets the auto player up
     */
    void setup();

    /**
     * Updates the auto player and plays the timelines
     */
    void update(bool servoHaveErrorsLikeTooHot);

    /**
     * Stops the auto player because of an incomming package from the Animatronic Workbench Studio
     */
    void stopBecauseOfIncommingPackage();
};

#endif
