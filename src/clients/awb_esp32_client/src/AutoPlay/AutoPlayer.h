#ifndef _AUTOPLAYER_H_
#define _AUTOPLAYER_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "../ProjectData/StsServoPoint.h"
#include "../ProjectData/Pca9685PwmServoPoint.h"
#include "../ProjectData/TimelineStateReference.h"
#include "../AwbDataImport/ProjectData.h"
#include "../Actuators/StSerialServoManager.h"
#include "../Actuators/Pca9685PwmManager.h"
#include "../Actuators/Mp3PlayerYX5300Manager.h"
#include "../Actuators/InputManager.h"

using byte = unsigned char;

class AutoPlayer
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured; // the error occured callback

    StSerialServoManager *_stSerialServoManager;     // the STS serial servo manager
    StSerialServoManager *_scSerialServoManager;     // the SCS serial servo manager
    Pca9685PwmManager *_pca9685PwmManager;           // the PCA9685 PWM manager
    Mp3PlayerYX5300Manager *_mp3PlayerYX5300Manager; // the MP3 player manager
    InputManager *_inputManager;                     // the input manager
    ProjectData *_data;                              // the data exported by Animatronic Workbench Studio

    long _lastMsUpdate;                  // millis() of last update
    long _lastPacketReceivedMillis = -1; // millis() of last received packet
    int _actualTimelineIndex = -1;       // the index of the actual playing timeline
    long _playPosInActualTimeline = 0;   // the actual play position in the actual playing timeline (in milliseconds)

    int _lastSoundPlayed = -1;

    int _currentStateId = -1;             // the actual selected state id
    int _stateSelectorStsServoChannel;    // the channel of the sts servo which is used to select the state
    bool _stateSelectorAvailable = false; // is a state selector available?
    long _lastStateCheckMillis = -1;      // millis() of last state check

    int calculateServoValueFromTimeline(u8 servoChannel, int servoSpeed, int servoAccelleration, std::vector<StsServoPoint> *servoPoints);

public:
    AutoPlayer(ProjectData *data, StSerialServoManager *stSerialServoManager, StSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, Mp3PlayerYX5300Manager *mp3PlayerYX5300Manager, InputManager *inputManager, int stateSelectorStsServoChannel, TCallBackErrorOccured errorOccured) : _data(data), _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _mp3PlayerYX5300Manager(mp3PlayerYX5300Manager), _inputManager(inputManager), _stateSelectorStsServoChannel(stateSelectorStsServoChannel), _errorOccured(errorOccured)
    {
        _lastMsUpdate = millis();
    }

    ~AutoPlayer() {}

    String getStatesDebugInfo();

    /**
     * Returns true if the player is playing a timeline
     */
    bool isPlaying();

    /**
     * The name of the timeline actually playing
     */
    String getCurrentTimelineName();

    /**
     * Returns true if a state selector is available
     */
    bool getStateSelectorAvailable();

    String getLastSoundPlayed();

    /**
     * Returns the channel of the sts servo which is used to select the state
     */
    int getStateSelectorStsServoChannel();

    /**
     * Returns the actual selected state id based on the rotation of a sts servo
     */
    int selectedStateIdFromStsServoSelector();

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
