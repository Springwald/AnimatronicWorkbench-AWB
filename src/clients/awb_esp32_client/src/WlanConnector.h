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
#include "ActualStatusInformation.h"

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
    ActualStatusInformation *_actualStatusInformation;

    String _messages[MAX_LOG_MESSAGES];
    int _messagesCount = 0;

    String GetHtml();
    void handle_Default();
    void handle_NotFound();

public:
    String *memoryInfo;

    WlanConnector(int clientId, ActualStatusInformation *actualStatusInformation, TCallBackErrorOccured errorOccured)
        : _errorOccured(errorOccured), _clientId(clientId), _actualStatusInformation(actualStatusInformation)
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