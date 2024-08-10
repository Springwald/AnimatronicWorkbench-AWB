#ifndef _AWB_CLIENT_H_
#define _AWB_CLIENT_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "Adafruit_PWMServoDriver.h"
#include "Actuators/StScsSerialServoManager.h"
#include "Actuators/Pca9685PwmManager.h"
#include "Actuators/Mp3PlayerYX5300Manager.h"
#include "Actuators/ActuatorValue.h"
#include "PacketSenderReceiver.h"
#include "PacketProcessor.h"
#include "DacSpeaker.h"
#include "AutoPlay/AutoPlayer.h"
#include "AwbDataImport/ProjectData.h"
#include "WlanConnector.h"
#include "AwbDataImport/HardwareConfig.h"
#include "ActualStatusInformation.h"
#include "StatusManagement.h"
#include "Debugging.h"
#include "Actuators/NeopixelManager.h"
#include "AwbDataImport/CustomCode/CustomCode.h"

using byte = unsigned char;

class AwbClient
{
protected:
    int trackNo;

    unsigned int _clientId;  /// The client id of this client
    AwbDisplay _display;     /// The display, oled or lcd
    DacSpeaker *_dacSpeaker; /// The speaker if connected

    long _startMillis = millis();          /// The start millis
    String _lastAutoPlayTimelineName = ""; /// The last selected timeline name for autoplay timeline filter

    PacketSenderReceiver *_packetSenderReceiver = nullptr;       /// The packet sender receiver to communicate with the Animatronic Workbench Studio
    PacketProcessor *_packetProcessor = nullptr;                 /// The packet processor to process the received packets from the Animatronic Workbench Studio
    Pca9685PwmManager *_pca9685pwmManager = nullptr;             /// The pwm manager to control the Pca9685 pwm board
    StScsSerialServoManager *_stSerialServoManager = nullptr;    /// The serial servo manager to control the sts serial servos
    StScsSerialServoManager *_scSerialServoManager = nullptr;    /// The serial servo manager to control the scs serial servos
    InputManager *_inputManager = nullptr;                       // the input manager
    Mp3PlayerYX5300Manager *_mp3Player = nullptr;                /// The mp3 player to play sounds
    NeopixelManager *_neopixelManager = nullptr;                 /// The neopixel manager to control the neopixel leds
    AutoPlayer *_autoPlayer = nullptr;                           /// The auto player to play timeline animations
    WlanConnector *_wlanConnector = nullptr;                     /// The wlan connector to open a WLAN AP and display status information as a web page
    ProjectData *_projectData = nullptr;                         // the project data exported by Animatronic Workbench Studio
    Debugging *_debugging = nullptr;                             // the debugging class
    StatusManagement *_statusManagement = nullptr;               // the status management for acutator status information and health check
    ActualStatusInformation *_actualStatusInformation = nullptr; /// The actual status information
    CustomCode *_customCode = nullptr;                           // the custom code

    /**
     * Update the actuators
     */
    void updateActuators();

    /**
     * Show a error message on the display
     */
    void showError(String message);

    /**
     * Show a message on the display (no error!)
     */
    void showMsg(String message);

    /**
     * Show a message on the display (no error!)
     */
    void showMsgWithDuration(String message, int duration);
    /**
     * Show a message during the setup process on the display for only a short moment (no error!)
     */
    void showSetupMsg(String message);

public:
    AwbClient(const unsigned int clientId)
    {
        _clientId = clientId;
        _actualStatusInformation = new ActualStatusInformation();
    }

    ~AwbClient()
    {
        delete _packetSenderReceiver;
        delete _pca9685pwmManager;
        delete _stSerialServoManager;
        delete _neopixelManager;
        delete _autoPlayer;
        delete _wlanConnector;
        delete _projectData;
        delete _debugging;
        delete _statusManagement;
        delete _actualStatusInformation;
        delete _customCode;
    }

    void setup();
    void loop();
};

#endif