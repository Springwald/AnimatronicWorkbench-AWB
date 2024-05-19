#ifndef _STATUS_MANAGEMENT_H_
#define _STATUS_MANAGEMENT_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "Adafruit_PWMServoDriver.h"
#include "Actuators/StSerialServoManager.h"
#include "Actuators/Pca9685PwmManager.h"
#include "Actuators/Mp3PlayerYX5300Manager.h"
#include "Actuators/ActuatorValue.h"
#include "PacketSenderReceiver.h"
#include "PacketProcessor.h"
#include "DacSpeaker.h"
#include "NeoPixel/NeoPixelStatusControl.h"
#include "AutoPlay/AutoPlayer.h"
#include "AwbDataImport/ProjectData.h"
#include "WlanConnector.h"
#include "Hardware.h"
#include "ActualStatusInformation.h"
#include "Debugging.h"

using byte = unsigned char;

class StatusManagement
{

    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShowWithDuration = std::function<void(String, int)>;

private:
    AwbDisplay *_awbDisplay; /// The display, oled or lcd
    ProjectData *_projectData;
    StSerialServoManager *_stSerialServoManager;
    StSerialServoManager *_scSerialServoManager;
    Pca9685PwmManager *_pca9685PwmManager;
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShowWithDuration _messageToShow;
    int _displayStateCounter = 0; /// The counter for the display state
    long _millisLastDisplayChange = millis();
    String _debugState;     /// The debug state to show on the display
    int _freeMemoryOnStart; /// The free memory on start

    boolean _isAnyGlobalFaultActuatorInCriticalState = false;

    String _actualActuatorsStateInfo = "";

    /**
     * read the status information from the actuators
     */
    String updateActuatorsStatuses();

    String updateStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo);

    String getDebugInfos();

    int getFreeMemory();

public:
    StatusManagement(ProjectData *projectData, AwbDisplay *awbDisplay, StSerialServoManager *stSerialServoManager, StSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, TCallBackErrorOccured errorOccured)
        : _projectData(projectData), _awbDisplay(awbDisplay), _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _errorOccured(errorOccured)
    {
        resetDebugInfos();
    }

    ~StatusManagement()
    {
        // delete _packetSenderReceiver;
    }

    bool getIsAnyGlobalFaultActuatorInCriticalState()
    {
        return _isAnyGlobalFaultActuatorInCriticalState;
    }

    void setDebugStatus(String state);
    void update();
    void resetDebugInfos();
};

#endif