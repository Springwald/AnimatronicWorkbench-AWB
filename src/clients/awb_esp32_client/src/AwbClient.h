#ifndef _AWB_CLIENT_H_
#define _AWB_CLIENT_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "Adafruit_PWMServoDriver.h"
#include "StSerialServoManager.h"
#include "DacSpeaker.h"
#include "AdafruitPwmManager.h"
#include "PacketSenderReceiver.h"
#include "ActuatorValue.h"
#include "NeoPixel/NeoPixelStatusControl.h"
#include "AutoPlay/AutoPlayer.h"
#include "WlanConnector.h"
#include "Hardware.h"
#include "ActualStatusInformation.h"

using byte = unsigned char;

class AwbClient
{
protected:
    unsigned int _clientId;                      /// The client id of this client
    AwbDisplay _display;                         /// The display, oled or lcd
    DacSpeaker _dacSpeaker;                      /// The speaker if connected
    int _displayStateCounter = 0;                /// The counter for the display state
    long _lastStatusMillis = 0;                  /// The last time the status was udated
    long _startMillis = millis();                /// The start millis
    PacketSenderReceiver *_packetSenderReceiver; /// The packet sender receiver to communicate with the Animatronic Workbench Studio
    AdafruitPwmManager *_adafruitpwmManager;     /// The pwm manager to control the adafruit pwm board
    StSerialServoManager *_stSerialServoManager; /// The serial servo manager to control the sts serial servos
    NeoPixelStatusControl *_neoPixelStatus;      /// The neopixel status control

    AutoPlayer *_autoPlayer;               /// The auto player to play timeline animations
    int _lastAutoPlaySelectedStateId = -1; /// The last selected state id for autoplay timeline filter
    String _lastAutoPlayTimelineName = ""; /// The last selected timeline name for autoplay timeline filter
    WlanConnector *_wlanConnector;         /// The wlan connector to open a WLAN AP and display status information as a web page

    ActualStatusInformation *_actualStatusInformation; /// The actual status information

    /**
     * Process a packet received from the Animatronic Workbench Studio
     */
    void processPacket(String payload);

    /**
     * Update the actuators
     */
    void updateActuators();

    /**
     * Read the actuators statuses
     */
    void readActuatorsStatuses();

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

public:
    AwbClient(const unsigned int clientId)
    {
        _clientId = clientId;
        _actualStatusInformation = new ActualStatusInformation();
    }

    ~AwbClient()
    {
        delete _packetSenderReceiver;
        delete _adafruitpwmManager;
        delete _stSerialServoManager;
        delete _neoPixelStatus;
        delete _autoPlayer;
        delete _wlanConnector;
    }

    void setup();
    void loop();
};

#endif