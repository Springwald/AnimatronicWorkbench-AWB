#ifndef _WLANCONNECTOR_H_
#define _WLANCONNECTOR_H_

#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
#include "AutoPlay/AutoPlayer.h"
#include "hardware.h"
#include <WiFi.h>
#include <WebServer.h>
#include "Actuators/ActuatorValue.h"
#include <String.h>
#include "ActualStatusInformation.h"

using byte = unsigned char;

#define MAX_LOG_MESSAGES 10

/**
 * provides a webserver to control and monitor the animatronic figure
 */
class WlanConnector
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    int _clientId;                                     /// the id of the client
    TCallBackErrorOccured _errorOccured;               /// callback function to call if an error occured
    AutoPlayData *_data;                               /// the timelime data and project meta information for the auto play
    WebServer *_server;                                /// the webserver
    long _startTime = millis();                        /// the time when the webserver was started
    ActualStatusInformation *_actualStatusInformation; /// the actual status information of the animatronic figure

    String _messages[MAX_LOG_MESSAGES]; /// the log messages
    int _messagesCount = 0;             /// the number of log messages

    String GetHtml();                   /// get the html page for the webserver default site
    void handle_Default();              /// handle the root http request
    void handle_NotFound();             /// handle a not found http request
    void handle_remote_servo();         /// control a servo via the webserver
    void handle_remote_play_timeline(); /// start a timeline via the webserver

public:
    /**
     * the memory info to display on the webserver info page
     */
    String *memoryInfo;

    String *timelineNameToPlay; /// the name of the timeline to play by remote control

    int scsServoCannelToSet; /// the channel of the scs servo to set by remote control
    int scsServoValueToSet;  /// the value of the scsservo to set by remote control (in percent)

    int stsServoCannelToSet; /// the channel of the sts servo to set by remote control
    int stsServoValueToSet;  /// the value of the sts servo to set by remote control (in percent)

    WlanConnector(int clientId, ActualStatusInformation *actualStatusInformation, TCallBackErrorOccured errorOccured)
        : _errorOccured(errorOccured), _clientId(clientId), _actualStatusInformation(actualStatusInformation)
    {
        _data = new AutoPlayData();
        timelineNameToPlay = new String();
    }

    ~WlanConnector()
    {
    }

    /**
     * set up the webserver
     */
    void setup();

    /**
     * update loof of the webserver to handle http requests
     */
    void update();

    /**
     * log an error  message
     */
    void logError(String msg);

    /**
     * log an info message
     */
    void logInfo(String msg);
};

#endif