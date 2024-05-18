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
    String _debugState;           /// The debug state to show on the display
    int _freeMemoryOnStart;       /// The free memory on start

    /**
     * read the status information from the actuators
     */
    void readActuatorsStatuses();

    void readStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo);

    /**
     * show the target values of the actuators on the display
     */
    void showValues();

    /**
     * show the temperature values of the actuators on the display
     */
    void showTemperaturStatuses();

    /**
     * show the load (torque) status of the actuators on the display
     */
    void showLoadStatuses();

    void draw_debugInfos();

    int getFreeMemory();

public:
    StatusManagement(ProjectData *projectData, AwbDisplay *awbDisplay, StSerialServoManager *stSerialServoManager, StSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, TCallBackErrorOccured errorOccured)
        : _projectData(projectData), _awbDisplay(awbDisplay), _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _errorOccured(errorOccured)
    {
    }

    ~StatusManagement()
    {
        // delete _packetSenderReceiver;
    }

    void update(boolean criticalTemp);
    void setDebugStatus(String state);
    void resetDebugInfos();
};

#endif