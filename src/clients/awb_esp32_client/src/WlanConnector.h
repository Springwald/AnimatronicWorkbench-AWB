#ifndef _WLANCONNECTOR_H_
#define _WLANCONNECTOR_H_

#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
#include "hardware.h"
#include <WiFi.h>
#include <WebServer.h>
#include "ActuatorValue.h"

using byte = unsigned char;

class WlanConnector
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured;
    AutoPlayData *_data;
    WebServer *_server;
    ActuatorValue *_stsServoValues[MAX_ACTUATOR_VALUES];
    ActuatorValue *_pwmServoValues[MAX_ACTUATOR_VALUES];

    String GetHtml();
    void handle_Default();
    void handle_NotFound();

public:
    // the constructor which takes the stsServoValues and matches them to the variable
    WlanConnector(ActuatorValue *stsServoValues[MAX_ACTUATOR_VALUES], ActuatorValue *pwmServoValues[MAX_ACTUATOR_VALUES], TCallBackErrorOccured errorOccured)
        : _errorOccured(errorOccured), _stsServoValues(stsServoValues), _pwmServoValues(pwmServoValues)
    {
        _data = new AutoPlayData();
        _server = new WebServer(80);
    }

    ~WlanConnector()
    {
    }

    void setup();
    void update();
};

#endif