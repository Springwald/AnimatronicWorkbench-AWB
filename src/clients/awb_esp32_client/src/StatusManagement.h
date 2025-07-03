#ifndef _STATUS_MANAGEMENT_H_
#define _STATUS_MANAGEMENT_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "Adafruit_PWMServoDriver.h"
#include "Actuators/Servos/StScsSerialServoManager.h"
#include "Actuators/Servos/Pca9685PwmManager.h"
#include "Actuators/ActuatorValue.h"
#include "PacketSenderReceiver.h"
#include "PacketProcessor.h"
#include "DacSpeaker.h"
#include "AutoPlay/AutoPlayer.h"
#include "AwbDataImport/ProjectData.h"
#include "WlanConnector.h"
#include "AwbDataImport/HardwareConfig.h"
#include "ActualStatusInformation.h"
#include "Debugging.h"

using byte = unsigned char;

class StatusManagement
{

    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShowWithDuration = std::function<void(String, int)>;

private:
    const int durationPerDisplayState = 2000; // 2 seconds per display state

    int _clientId = 0;       /// The client id
    AwbDisplay *_awbDisplay; /// The display, oled or lcd
    ProjectData *_projectData;
    StScsSerialServoManager *_stSerialServoManager;
    StScsSerialServoManager *_scSerialServoManager;
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
    String updateActuatorsStatuses(unsigned long diffMs);

    String updateStsScsServoStatuses(StScsSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo, unsigned long diffMs);

    String getDebugInfos();

    int getFreeMemory();

public:
    StatusManagement(int clientId, ProjectData *projectData, AwbDisplay *awbDisplay, StScsSerialServoManager *stSerialServoManager, StScsSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, TCallBackErrorOccured errorOccured)
        : _clientId(clientId), _projectData(projectData), _awbDisplay(awbDisplay), _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _errorOccured(errorOccured)
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