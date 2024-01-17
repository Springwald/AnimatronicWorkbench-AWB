#ifndef _AUTOPLAYER_H_
#define _AUTOPLAYER_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "StsServoPoint.h"
#include "Pca9685PwmServoPoint.h"
#include "TimelineState.h"
#include "AutoPlayData.h"
#include "../Actuators/StSerialServoManager.h"
#include "../Actuators/Pca9685PwmManager.h"

using byte = unsigned char;

class AutoPlayer
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured; // the error occured callback

    StSerialServoManager *_stSerialServoManager; // the STS serial servo manager
    StSerialServoManager *_scSerialServoManager; // the SCS serial servo manager
    Pca9685PwmManager *_pca9685PwmManager;       // the PCA9685 PWM manager

    AutoPlayData *_data; // the data exported by Animatronic Workbench Studio

    long _lastMsUpdate;                  // millis() of last update
    long _lastPacketReceivedMillis = -1; // millis() of last received packet
    int _actualTimelineIndex = -1;       // the index of the actual playing timeline
    int _playPosInActualTimeline = 0;    // the actual play position in the actual playing timeline (in milliseconds)

    int _currentStateId = -1;             // the actual selected state id
    int _stateSelectorStsServoChannel;    // the channel of the sts servo which is used to select the state
    bool _stateSelectorAvailable = false; // is a state selector available?
    long _lastStateCheckMillis = -1;      // millis() of last state check

    int calculateServoValueFromTimeline(u8 servoChannel, int servoSpeed, int servoAccelleration, std::vector<StsServoPoint> *servoPoints);

public:
    AutoPlayer(StSerialServoManager *stSerialServoManager, StSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, int stateSelectorStsServoChannel, TCallBackErrorOccured errorOccured) : _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _stateSelectorStsServoChannel(stateSelectorStsServoChannel), _errorOccured(errorOccured)
    {
        _lastMsUpdate = millis();
        _data = new AutoPlayData();
    }

    ~AutoPlayer() {}

    /**
     * Returns the data exported by Animatronic Workbench Studio
     */
    bool isPlaying();

    /**
     * Returns true if the player is playing a timeline
     */
    bool isPlaying2();

    /**
     * The name of the timeline actually playing
     */
    String getCurrentTimelineName();

    /**
     * Returns true if a state selector is available
     */
    bool getStateSelectorAvailable();

    /**
     * Returns the channel of the sts servo which is used to select the state
     */
    int getStateSelectorStsServoChannel();

    /**
     * Returns the actual selected state id
     */
    int selectedStateId();

    /**
     * starts the timeline with the given index
     */
    void startNewTimeline(int timelineIndex);

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
