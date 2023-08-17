#ifndef _WLANCONNECTOR_H_
#define _WLANCONNECTOR_H_

#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
#include "AutoPlay/AutoPlayer.h"
#include "hardware.h"
#include <WiFi.h>
#include <WebServer.h>
#include "ActuatorValue.h"
#include <String.h>

using byte = unsigned char;

#define MAX_LOG_MESSAGES 10

class WlanConnector
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    int _clientId;
    TCallBackErrorOccured _errorOccured;
    AutoPlayData *_data;
    WebServer *_server;
    long _startTime = millis();

    std::vector<ActuatorValue> *_stsServoValues;
    std::vector<ActuatorValue> *_pwmServoValues;

    String _messages[MAX_LOG_MESSAGES];
    int _messagesCount;

    String GetHtml();
    void handle_Default();
    void handle_NotFound();

public:
    AutoPlayer *_autoPlayer;
    String *memoryInfo;

    WlanConnector(int clientId, std::vector<ActuatorValue> *stsServoValues, std::vector<ActuatorValue> *pwmServoValues, AutoPlayer *autoPlayer, TCallBackErrorOccured errorOccured)
        : _errorOccured(errorOccured), _stsServoValues(stsServoValues), _pwmServoValues(pwmServoValues), _clientId(clientId), _autoPlayer(autoPlayer)
    {
        _data = new AutoPlayData();
    }

    ~WlanConnector()
    {
    }

    void setup();
    void update();
    void logError(String msg);
    void logInfo(String msg);
};

#endif