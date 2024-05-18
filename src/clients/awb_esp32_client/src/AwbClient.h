#ifndef _AWB_CLIENT_H_
#define _AWB_CLIENT_H_

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

class AwbClient
{
protected:
    int trackNo;

    unsigned int _clientId;                /// The client id of this client
    AwbDisplay _display;                   /// The display, oled or lcd
    DacSpeaker *_dacSpeaker;               /// The speaker if connected
    int _displayStateCounter = 0;          /// The counter for the display state
    long _lastStatusMillis = 0;            /// The last time the status was udated
    long _startMillis = millis();          /// The start millis
    int _lastAutoPlaySelectedStateId = -1; /// The last selected state id for autoplay timeline filter
    String _lastAutoPlayTimelineName = ""; /// The last selected timeline name for autoplay timeline filter

    PacketSenderReceiver *_packetSenderReceiver;       /// The packet sender receiver to communicate with the Animatronic Workbench Studio
    PacketProcessor *_packetProcessor;                 /// The packet processor to process the received packets from the Animatronic Workbench Studio
    Pca9685PwmManager *_pca9685pwmManager;             /// The pwm manager to control the Pca9685 pwm board
    StSerialServoManager *_stSerialServoManager;       /// The serial servo manager to control the sts serial servos
    StSerialServoManager *_scSerialServoManager;       /// The serial servo manager to control the scs serial servos
    InputManager *_inputManager;                       // the input manager
    Mp3PlayerYX5300Manager *_mp3Player;                /// The mp3 player to play sounds
    NeoPixelStatusControl *_neoPixelStatus;            /// The neopixel status control
    AutoPlayer *_autoPlayer;                           /// The auto player to play timeline animations
    WlanConnector *_wlanConnector;                     /// The wlan connector to open a WLAN AP and display status information as a web page
    AutoPlayData *_data;                               // the data exported by Animatronic Workbench Studio
    ProjectData *_projectData;                         // the project data exported by Animatronic Workbench Studio
    Debugging *_debugging;                             // the debugging class
    ActualStatusInformation *_actualStatusInformation; /// The actual status information

    /**
     * Update the actuators
     */
    void updateActuators();

    /**
     * Read the actuators statuses
     */
    void readActuatorsStatuses();
    void readStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo);

    /**
     * Show the actuator values (=mostly positions) on the display
     */
    void showValues();

    /**
     * Show the temperature status of the actuators on the display
     */
    void showTemperaturStatuses();

    /**
     * Show the load status of the actuators on the display
     */
    void showLoadStatuses();

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
        delete _neoPixelStatus;
        delete _autoPlayer;
        delete _wlanConnector;
    }

    void setup();
    void loop();
};

#endif