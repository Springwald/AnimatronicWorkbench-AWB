#ifndef _PACKET_PROCESSOR_H_
#define _PACKET_PROCESSOR_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "Adafruit_PWMServoDriver.h"
#include "Actuators/Servos/StScsSerialServoManager.h"
#include "Actuators/Servos/Pca9685PwmManager.h"
#include "Actuators/ActuatorValue.h"
#include "PacketSenderReceiver.h"
#include "DacSpeaker.h"
#include "AwbDataImport/ProjectData.h"
#include "WlanConnector.h"
#include "AwbDataImport/HardwareConfig.h"
#include "ActualStatusInformation.h"
#include "Debugging.h"
#include <String.h>

using byte = unsigned char;

class PacketProcessor
{

    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShowWithDuration = std::function<void(String, int)>;

private:
    ProjectData *_projectData;
    StScsSerialServoManager *_stSerialServoManager;
    StScsSerialServoManager *_scSerialServoManager;
    Pca9685PwmManager *_pca9685PwmManager;
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShowWithDuration _messageToShow;
    Debugging *_debugging = nullptr;

    /**
     * Update the actuators
     */
    // void updateActuators();

public:
    PacketProcessor(ProjectData *projectData, StScsSerialServoManager *stSerialServoManager, StScsSerialServoManager *scSerialServoManager, Pca9685PwmManager *pca9685PwmManager, TCallBackErrorOccured errorOccured, TCallBackMessageToShowWithDuration messageToShow, Debugging *debugging)
        : _projectData(projectData), _stSerialServoManager(stSerialServoManager), _scSerialServoManager(scSerialServoManager), _pca9685PwmManager(pca9685PwmManager), _errorOccured(errorOccured), _messageToShow(messageToShow), _debugging(debugging)
    {
    }

    ~PacketProcessor()
    {
        // delete _packetSenderReceiver;
    }

    /**
     * Process a packet received from the Animatronic Workbench Studio.
     * Returns the string response for the respone message. Normally empty.
     */
    String processPacket(String payload);
};

#endif