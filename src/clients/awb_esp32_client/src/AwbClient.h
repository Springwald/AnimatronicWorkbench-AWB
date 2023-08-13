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

using byte = unsigned char;

#define maxActuatorValues 32

class AwbClient
{
protected:
    unsigned int _clientId;
    AwbDisplay _display;
    DacSpeaker _dacSpeaker;
    int _displayStateCounter = 0;
    long _lastStatusMillis = 0;
    PacketSenderReceiver *_packetSenderReceiver;
    AdafruitPwmManager *_adafruitpwmManager;
    StSerialServoManager *_stSerialServoManager;
    NeoPixelStatusControl *_neoPixelStatus;
    bool _servoCriticalTemp = false;
    bool _servoCriticalLoad = false;
    AutoPlayer *_autoPlayer;
    int _lastAutoPlaySelectedStateId = -1;
    String _lastAutoPlayTimelineName = "";

    ActuatorValue _stsServoValues[maxActuatorValues];
    ActuatorValue _pwmServoValues[maxActuatorValues];

    void processPacket(String payload);
    void updateActuators();
    void readActuatorsStatuses();
    void showValues();
    void showTemperaturStatuses();
    void showLoadStatuses();
    void showError(String message);

public:
    AwbClient(const unsigned int clientId)
    {
        _clientId = clientId;
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