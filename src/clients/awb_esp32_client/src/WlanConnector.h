#ifndef _WLANCONNECTOR_H_
#define _WLANCONNECTOR_H_

#include <Arduino.h>
#include "AutoPlay/AutoPlayer.h"
#include "AwbDataImport/ProjectData.h"
#include "AwbDataImport/WifiConfig.h"
#include "AwbDataImport/HardwareConfig.h"
#include <WiFi.h>
#include <WebServer.h>
#include "Actuators/ActuatorValue.h"
#include <String.h>
#include "ActualStatusInformation.h"
#include "Debugging.h"

using byte = unsigned char;

#define MAX_LOG_MESSAGES 100

/**
 * provides a webserver to control and monitor the animatronic figure
 */
class WlanConnector
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    unsigned int _clientId;                            /// the id of the client
    TCallBackErrorOccured _errorOccured;               /// callback function to call if an error occured
    bool _liveDebuggingActive;                         /// the live debugging is active or not
    WifiConfig *_wifiConfig;                           /// the wifi configuration
    WebServer *_server;                                /// the webserver
    long _startTime = millis();                        /// the time when the webserver was started
    ProjectData *_projectData;                         /// the project data exported by Animatronic Workbench Studio
    Debugging *_debugging;                             /// the debugging class
    ActualStatusInformation *_actualStatusInformation; /// the actual status information of the animatronic figure

    String _messages[MAX_LOG_MESSAGES];     /// the log messages
    String _messageTimes[MAX_LOG_MESSAGES]; /// the time of the log messages
    int _messagesCount = 0;                 /// the number of log messages

    String GetHtml(); /// get the html page for the webserver default site
    void AddScsStsServoInfo(String &ptr, String typeTitle, StsScsServo &servo);
    void handle_Default();                         /// handle the root http request
    void handle_NotFound();                        /// handle a not found http request
    void handle_remote_servo();                    /// control a servo via the webserver
    void handle_remote_play_timeline();            /// start a timeline via the webserver
    String getTd(String content, boolean isError); /// get the  table style for a error message
    String getTdVal(String content, int maxValue, int minValue, int value);
    String getErrorTime(unsigned long errorMs); // get a displayable error time

public:
    /**
     * the memory info to display on the webserver info page
     */
    String *memoryInfo;

    String *timelineNameToPlay = nullptr; /// the name of the timeline to play by remote control

    int scsServoCannelToSet; /// the channel of the scs servo to set by remote control
    int scsServoValueToSet;  /// the value of the scsservo to set by remote control (in percent)

    int stsServoCannelToSet; /// the channel of the sts servo to set by remote control
    int stsServoValueToSet;  /// the value of the sts servo to set by remote control (in percent)

    WlanConnector(unsigned int clientId, ProjectData *projectData, ActualStatusInformation *actualStatusInformation, Debugging *debugging, TCallBackErrorOccured errorOccured)
        : _errorOccured(errorOccured), _projectData(projectData), _clientId(clientId), _actualStatusInformation(actualStatusInformation), _debugging(debugging)
    {
        timelineNameToPlay = new String();
        _wifiConfig = new WifiConfig();
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
    void update(bool liveDebuggingActive);

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