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
    int _lastAutoPlaySelectedStateId = -1; /// The last selected state id for autoplay timeline filter
    String _lastAutoPlayTimelineName = ""; /// The last selected timeline name for autoplay timeline filter

    PacketSenderReceiver *_packetSenderReceiver;       /// The packet sender receiver to communicate with the Animatronic Workbench Studio
    PacketProcessor *_packetProcessor;                 /// The packet processor to process the received packets from the Animatronic Workbench Studio
    Pca9685PwmManager *_pca9685pwmManager;             /// The pwm manager to control the Pca9685 pwm board
    StScsSerialServoManager *_stSerialServoManager;       /// The serial servo manager to control the sts serial servos
    StScsSerialServoManager *_scSerialServoManager;       /// The serial servo manager to control the scs serial servos
    InputManager *_inputManager;                       // the input manager
    Mp3PlayerYX5300Manager *_mp3Player;                /// The mp3 player to play sounds
    NeopixelManager *_neopixelManager;                 /// The neopixel manager to control the neopixel leds
    AutoPlayer *_autoPlayer;                           /// The auto player to play timeline animations
    WlanConnector *_wlanConnector;                     /// The wlan connector to open a WLAN AP and display status information as a web page
    ProjectData *_projectData;                         // the project data exported by Animatronic Workbench Studio
    Debugging *_debugging;                             // the debugging class
    StatusManagement *_statusManagement;               // the status management for acutator status information and health check
    ActualStatusInformation *_actualStatusInformation; /// The actual status information
    CustomCode *_customCode;                           // the custom code

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
    }

    void setup();
    void loop();
};

#endif