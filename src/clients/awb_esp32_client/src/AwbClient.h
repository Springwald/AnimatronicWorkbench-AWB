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
    unsigned int _clientId;
    AwbDisplay _display;
    DacSpeaker _dacSpeaker;
    int _displayStateCounter = 0;
    long _lastStatusMillis = 0;
    long _startMillis = millis();
    PacketSenderReceiver *_packetSenderReceiver;
    AdafruitPwmManager *_adafruitpwmManager;
    StSerialServoManager *_stSerialServoManager;
    NeoPixelStatusControl *_neoPixelStatus;
    bool _servoCriticalTemp = false;
    bool _servoCriticalLoad = false;
    AutoPlayer *_autoPlayer;
    int _lastAutoPlaySelectedStateId = -1;
    String _lastAutoPlayTimelineName = "";
    WlanConnector *_wlanConnector;

    ActualStatusInformation *_actualStatusInformation;

    void processPacket(String payload);
    void updateActuators();
    void readActuatorsStatuses();
    void showValues();
    void showTemperaturStatuses();
    void showLoadStatuses();
    void showError(String message);
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
    }

    void setup();
    void loop();
};

#endif