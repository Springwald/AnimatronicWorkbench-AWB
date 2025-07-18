#include <Arduino.h>
#include "AutoPlay/AutoPlayer.h"
#include "WlanConnector.h"
#include <vector>
#include <WiFi.h>
#include <WebServer.h>
#include <String.h>

/**
 * set up the webserver
 */
void WlanConnector::setup()
{
    _debugging->setState(Debugging::MJ_WLAN, 0);

    // open a wifi access point
    IPAddress local_ip(192, 168, 1, 1);
    IPAddress gateway(192, 168, 1, 1);
    IPAddress subnet(255, 255, 255, 0);
    WiFi.softAP(_wifiConfig->WlanSSID, _wifiConfig->WlanPassword);
    WiFi.softAPConfig(local_ip, gateway, subnet);

    _debugging->setState(Debugging::MJ_WLAN, 1);

    // set up the webserver and define the url handlers
    _server = new WebServer(80);
    _server->on("/", [this]()
                { 
                    _debugging->setState(Debugging::MJ_WLAN, 2); 
                    this->handle_Default(); 
                    _debugging->setState(Debugging::MJ_WLAN, 3); });
    _server->on("/remote/servo/", [this]()
                { 
                    _debugging->setState(Debugging::MJ_WLAN, 4);
                    this->handle_remote_servo(); 
                    _debugging->setState(Debugging::MJ_WLAN, 5); });
    _server->on("/remote/play/", [this]()
                { 
                    _debugging->setState(Debugging::MJ_WLAN, 6);
                    this->handle_remote_play_timeline(); 
                    _debugging->setState(Debugging::MJ_WLAN, 7); });
    _server->onNotFound([this]()
                        { 
                    _debugging->setState(Debugging::MJ_WLAN, 8);
                    this->handle_NotFound();
                    _debugging->setState(Debugging::MJ_WLAN, 9); });
    _server->begin();

    // fill messages with empty strings
    for (int i = 0; i < MAX_LOG_MESSAGES; i++)
    {
        _messages[i] = String("");
    }
}

/**
 * update loof of the webserver
 */
void WlanConnector::update(bool liveDebuggingActive)
{
    _liveDebuggingActive = liveDebuggingActive;
    _server->handleClient();
}

/**
 * log an error message
 */
void WlanConnector::logError(String msg)
{
    logInfo(String("Error! ") + msg);
}

/**
 * log an info message
 */
void WlanConnector::logInfo(String msg)
{
    if (_messagesCount < MAX_LOG_MESSAGES)
        _messagesCount++;

    for (int i = _messagesCount - 1; i > 0; i--)
    { // shift the messages down in history
        _messages[i] = _messages[i - 1];
        _messageTimes[i] = _messageTimes[i - 1];
    }

    auto ageSeconds = (millis() - _startTime) / 1000;
    auto ageMinutes = ageSeconds / 60;
    auto ageHours = ageMinutes / 60;

    String time = String(ageHours) + "h-" + String(ageMinutes % 60) + "m-" + String(ageSeconds % 60) + "s";

    _messages[0] = msg;
    _messageTimes[0] = time;
}

/**
 * handle the root http request
 */
void WlanConnector::handle_Default()
{
    _server->send(200, "text/html", GetHtml());
}

/**
 * remote control a servo via the webserver
 */
void WlanConnector::handle_remote_servo()
{
    _server->send(200, "text/plain", "OK");
}

/**
 * play a timeline via the webserver
 */
void WlanConnector::handle_remote_play_timeline()
{
    if (_server->args() == 0)
    {
        _server->send(400, "text/plain", "BAD REQUEST");
        return;
    }
    // get arg "timeline" from server
    String timeline = _server->arg("timeline");
    if (timeline == "")
    {
        _server->send(400, "text/plain", "BAD REQUEST");
        return;
    }
    timelineNameToPlay = new String(timeline);
    bool timelineExists = false; // check if timeline exists
    for (int i = 0; i < _projectData->timelines->size(); i++)
    {
        if (_projectData->timelines->at(i).name == timeline)
        {
            timelineExists = true;
            break;
        }
    }
    if (timelineExists == false)
    {
        _server->send(400, "text/plain", "BAD REQUEST");
        return;
    }
    _server->send(200, "text/plain", "OK " + timeline);
}

/**
 * handle a not found http request
 */
void WlanConnector::handle_NotFound()
{
    _server->send(404, "text/plain", "Not found");
}

/**
 * get the html page for the webserver default site
 */
String WlanConnector::GetHtml()
{
    _debugging->setState(Debugging::MJ_WLAN, 30);

    auto ageSeconds = (millis() - _startTime) / 1000;
    auto ageMinutes = ageSeconds / 60;
    auto ageHours = ageMinutes / 60;

    if (_projectData == nullptr)
    {
        return "<!DOCTYPE html> <html><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\"><title>Animatronic Workbench</title></head><body><h1>Animatronic Workbench</h1><p>Project data not available</p></body></html>";
    }

    String ptr = "<!DOCTYPE html> <html>\n";
    ptr += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";
    ptr += "<title>" + String(this->_projectData->ProjectName) + " - (Animatronic Workbench)</title>\n";
    ptr += "<style>html { font-family: Helvetica; display: inline-block; margin: 0px auto; text-align: center;}\n";
    ptr += "body{ margin-top: 0px; width: 100%;}\n";
    ptr += ".region { display: table; margin: 0.5em auto; border: 1px solid black; padding: 0.2em 1em 0.2em 1em;}\n";
    ptr += "tr:nth-child(even) {background-color: #eeeeee;}\n";
    ptr += "table td { margin: 0; border: none; padding: 0 1em 0 1em; }\n";
    ptr += "</style>\n";
    ptr += "</head>\n";
    ptr += "<body>\n";

    _debugging->setState(Debugging::MJ_WLAN, 32);

    if (true)
    {
        ptr += "<p>Animatronic Workbench - Client ID " + String(_clientId) + "</p>\n";
        if (_liveDebuggingActive)
        {
            ptr += "<p><i>Live-debugging is active</i></p>\n";
        }
        ptr += "<h1>'" + String(this->_projectData->ProjectName) + "' figure</h1>\n";

        ptr += "<p> " + String(ageHours) + " hours, " + String(ageMinutes % 60) + " minutes, " + String(ageSeconds % 60) + " seconds uptime<br/>" + *memoryInfo + "</p>\n";

        _debugging->setState(Debugging::MJ_WLAN, 34);

        // Servo status
        ptr += "<div class=\"region\">\n";
        ptr += "<span>" + String(this->_projectData->stsServos->size()) + " STS-Servos, ";
        ptr += String(this->_projectData->scsServos->size()) + " SCS-Servos, ";
        ptr += String(this->_projectData->pca9685PwmServos->size()) + " PWM-Servos</span>\n";
        ptr += "<table>\n";
        ptr += "<tr><th>Channel</th><th>Name</th><th>Pos</th><th>Temp</th><th>Load</th><th>Min/Max</th><th>fault</th><th>last</th></tr>\n";

        _debugging->setState(Debugging::MJ_WLAN, 36);

        for (int i = 0; i < this->_projectData->stsServos->size(); i++)
        {
            auto servo = this->_projectData->stsServos->at(i);
            AddScsStsServoInfo(ptr, "STS", servo);
        }

        _debugging->setState(Debugging::MJ_WLAN, 38);

        for (int i = 0; i < this->_projectData->scsServos->size(); i++)
        {
            auto servo = this->_projectData->scsServos->at(i);
            AddScsStsServoInfo(ptr, "SCS", servo);
        }

        _debugging->setState(Debugging::MJ_WLAN, 40);

        for (int i = 0; i < this->_projectData->pca9685PwmServos->size(); i++)
        {
            auto servo = this->_projectData->pca9685PwmServos->at(i);
            ptr += "<tr><td>PWM " + String(servo.channel) + "</td><td>" + servo.title + "</td><td>" + String(servo.currentValue) + "</td><td>-</td><td>-</td><td>-</td></tr>\n";
        }

        _debugging->setState(Debugging::MJ_WLAN, 42);

        ptr += "</table>\n";
        ptr += "</div>\n";

        _debugging->setState(Debugging::MJ_WLAN, 44);

        //  AutoPlayer status
        ptr += "<div class=\"region\">\n";
        ptr += "<span>Autoplayer</span>\n";
        ptr += "<table>\n";
        ptr += "<tr><th>Info</th><th>ValueName</th></tr>\n";
        ptr += "<tr><td>current state</td><td> " + String(this->_actualStatusInformation->autoPlayerCurrentStateName) + "</td></tr>\n";
        ptr += "<tr><td>current timeline</td><td> " + String(_actualStatusInformation->autoPlayerCurrentTimelineName) + "</td></tr>\n";
        ptr += "<tr><td>is playing</td><td> " + String(_actualStatusInformation->autoPlayerIsPlaying == true ? "yes" : "no") + "</td></tr>\n";
        ptr += "<tr><td>states active by inputs</td><td> " + _actualStatusInformation->activeTimelineStateIdsByInput + "</td></tr>\n";
        ptr += "<tr><td>inputs</td><td> " + _actualStatusInformation->inputStates + "</td></tr>\n";
        ptr += "<tr><td>last sound</td><td> " + _actualStatusInformation->lastSoundPlayed + "</td></tr>\n";

        _debugging->setState(Debugging::MJ_WLAN, 46);

        ptr += "</table>\n";
        ptr += "</div>\n";

        //  System messages
        ptr += "<div class=\"region\">\n";
        ptr += "<table>\n";
        ptr += "<tr><th></th><th>Message</th></tr>\n";
        int msgPos = _messagesCount;

        _debugging->setState(Debugging::MJ_WLAN, 48);

        for (int i = 0; i < MAX_LOG_MESSAGES; i++)
        {
            auto messageHtmlEncode = _messages[i];
            messageHtmlEncode.replace("<", "&lt;");
            messageHtmlEncode.replace(">", "&gt;");
            messageHtmlEncode.replace("&", "&amp;");
            messageHtmlEncode.replace("\"", "&quot;");
            ptr += "<tr><td>" + _messageTimes[i] + "</td><td>" + messageHtmlEncode + "</td></tr>\n";
            msgPos++;
        }
        ptr += "</table>\n";
        ptr += "</div>\n";
    }

    _debugging->setState(Debugging::MJ_WLAN, 50);

    ptr += "<script type=\"text/javascript\">";
    ptr += " function Redirect() { window.location=\"http://192.168.1.1\";  }";
    ptr += " setTimeout('Redirect()', 5000); ";
    ptr += "</script>";

    ptr += "</body>\n";
    ptr += "</html>\n";
    return ptr;
}

void WlanConnector::AddScsStsServoInfo(String &ptr, String typeTitle, StsScsServo &servo)
{
    ptr += "<tr><td>" + typeTitle + " " + String(servo.channel) + "</td><td>" + servo.title + "</td><td>" + String(servo.currentValue) + "</td>" +
           this->getTdVal(String(servo.temperature), servo.maxTemp, 20, servo.temperature) +
           this->getTd(String(servo.load), abs(servo.load) > servo.maxTorque) +
           this->getTd(String(servo.minLoad) + "/" + String(servo.maxLoad), abs(servo.maxLoad) > servo.maxTorque || abs(servo.minLoad) > servo.maxTorque) +
           this->getTd(String(servo.isFaultCountDownMs > 0 ? String(servo.isFaultCountDownMs / 1000) : ""), servo.isFaultCountDownMs > 0) +
           this->getTd(servo.lastFaultMs != 0 ? getErrorTime(servo.lastFaultMs) + " " + servo.lastFaultMessage : "", false) + "</tr>\n";
}

String WlanConnector::getErrorTime(unsigned long errorMs)
{
    auto ageSeconds = (millis() - errorMs) / 1000;
    auto ageMinutes = ageSeconds / 60;
    auto ageHours = ageMinutes / 60;
    return String(ageHours) + ":" + String(ageMinutes % 60) + ":" + String(ageSeconds % 60);
}

String WlanConnector::getTd(String content, boolean isError)
{
    if (isError)
    {
        return "<td style=\"background-color: #ff0000; color: #ffffff;\"> " + content + "</td>";
    }
    return "<td> " + content + "</td>";
}

String WlanConnector::getTdVal(String content, int maxValue, int minValue, int value)
{

    if (value >= maxValue)
        return "<td style=\"background-color: #ff0000; color: #ffffff;\"> " + content + "</td>";

    if (value > (maxValue - minValue) * 0.8 + minValue)
        return "<td style=\"background-color: #ffa060; color: #000000;\"> " + content + "</td>";

    return "<td> " + content + "</td>";
}