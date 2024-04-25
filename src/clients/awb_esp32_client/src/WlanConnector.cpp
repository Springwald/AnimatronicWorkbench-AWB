#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
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
    // open a wifi access point
    IPAddress local_ip(192, 168, 1, 1);
    IPAddress gateway(192, 168, 1, 1);
    IPAddress subnet(255, 255, 255, 0);
    WiFi.softAP(_data->WlanSSID, _data->WlanPassword);
    WiFi.softAPConfig(local_ip, gateway, subnet);

    // set up the webserver and define the url handlers
    _server = new WebServer(80);
    _server->on("/", [this]()
                { this->handle_Default(); });
    _server->on("/remote/servo/", [this]()
                { this->handle_remote_servo(); });
    _server->on("/remote/play/", [this]()
                { this->handle_remote_play_timeline(); });
    _server->onNotFound([this]()
                        { this->handle_NotFound(); });
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
void WlanConnector::update()
{
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
    _messagesCount++;
    if (_messagesCount >= MAX_LOG_MESSAGES)
    {
        _messagesCount = 0;
    }
    _messages[_messagesCount] = msg;
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
    _server->send(200, "text/plain", "OK");
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
    auto ageSeconds = (millis() - _startTime) / 1000;
    auto ageMinutes = ageSeconds / 60;
    auto ageHours = ageMinutes / 60;

    String ptr = "<!DOCTYPE html> <html>\n";
    ptr += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";
    ptr += "<title>" + String(_data->ProjectName) + " - (Animatronic Workbench)</title>\n";
    ptr += "<style>html { font-family: Helvetica; display: inline-block; margin: 0px auto; text-align: center;}\n";
    ptr += "body{ margin-top: 0px; width: 100%;}\n";
    ptr += ".region { display: table; margin: 0.5em auto; border: 1px solid black; padding: 0.2em 1em 0.2em 1em;}\n";
    ptr += "tr:nth-child(even) {background-color: #eeeeee;}\n";
    ptr += "table td { margin: 0; border: none; padding: 0 1em 0 1em; }\n";
    ptr += "</style>\n";
    ptr += "</head>\n";
    ptr += "<body>\n";
    if (true)
    {
        ptr += "<p>Animatronic Workbench - Client ID " + String(_clientId) + "</p>\n";
        ptr += "<h1>'" + String(_data->ProjectName) + "' figure</h1>\n";

        ptr += "<p> " + String(ageHours) + " hours, " + String(ageMinutes % 60) + " minutes, " + String(ageSeconds % 60) + " seconds uptime<br/>" + *memoryInfo + "</p>\n";

        // Servo status
        ptr += "<div class=\"region\">\n";
        ptr += "<span>" + String(this->_actualStatusInformation->stsServoValues->size()) + " STS-Servos, ";
        ptr += String(this->_actualStatusInformation->pwmServoValues->size()) + " PWM-Servos</span>\n";
        ptr += "<table>\n";
        ptr += "<tr><th>Id</th><th>Name</th><th>Pos</th><th>Temp</th><th>Load</th></tr>\n";
        for (int i = 0; i < this->_actualStatusInformation->stsServoValues->size(); i++)
        {
            auto servo = this->_actualStatusInformation->stsServoValues->at(i);
            auto name = servo.name;
            for (int a = 0; a < _data->stsServoCount; a++)
            {
                if (_data->stsServoChannels[a] == servo.id)
                {
                    name = _data->stsServoName[a];
                    break;
                }
            }
            ptr += "<tr><td>STS " + String(servo.id) + "</td><td>" + name + "</td><td>" + String(servo.currentValue) + "</td><td>" + String(servo.temperature) + "</td><td>" + String(servo.load) + " </tr>\n";
        }

        for (int i = 0; i < this->_actualStatusInformation->scsServoValues->size(); i++)
        {
            auto servo = this->_actualStatusInformation->scsServoValues->at(i);
            auto name = servo.name;
            for (int a = 0; a < _data->stsServoCount; a++)
            {
                if (_data->scsServoChannels[a] == servo.id)
                {
                    name = _data->scsServoName[a];
                    break;
                }
            }
            ptr += "<tr><td>SCS " + String(servo.id) + "</td><td>" + name + "</td><td>" + String(servo.currentValue) + "</td><td>" + String(servo.temperature) + "</td><td>" + String(servo.load) + " </tr>\n";
        }

        for (int i = 0; i < this->_actualStatusInformation->pwmServoValues->size(); i++)
        {
            auto servo = this->_actualStatusInformation->pwmServoValues->at(i);
            ptr += "<tr><td>PWM " + String(servo.id) + "</td><td>" + servo.name + "</td><td>" + String(servo.currentValue) + "</td><td>-</td><td>-</tr>\n";
        }

        ptr += "</table>\n";
        ptr += "</div>\n";

        //  AutoPlayer status
        ptr += "<div class=\"region\">\n";
        ptr += "<span>Autoplayer</span>\n";
        ptr += "<table>\n";
        ptr += "<tr><th>Info</th><th>ValueName</th></tr>\n";
        ptr += "<tr><td>current state</td><td> " + String(this->_actualStatusInformation->autoPlayerCurrentStateName) + "</td></tr>\n";
        ptr += "<tr><td>current timeline</td><td> " + String(_actualStatusInformation->autoPlayerCurrentTimelineName) + "</td></tr>\n";
        ptr += "<tr><td>is playing</td><td> " + String(_actualStatusInformation->autoPlayerIsPlaying == true ? "yes" : "no") + "</td></tr>\n";
        ptr += "<tr><td>selected state id</td><td> " + String(_actualStatusInformation->autoPlayerSelectedStateId) + "</td></tr>\n";
        ptr += "<tr><td>state selector available</td><td> " + String(_actualStatusInformation->autoPlayerStateSelectorAvailable == true ? "yes" : "no") + "</td></tr>\n";
        ptr += "<tr><td>state selector STS servo channel</td><td> " + String(_actualStatusInformation->autoPlayerStateSelectorStsServoChannel) + "</td></tr>\n";
        ptr += "<tr><td>states active by inputs</td><td> " + _actualStatusInformation->activeTimelineStateIdsByInput + "</td></tr>\n";
        ptr += "<tr><td>inputs</td><td> " + _actualStatusInformation->inputStates + "</td></tr>\n";

        ptr += "</table>\n";
        ptr += "</div>\n";

        //  System messages
        ptr += "<div class=\"region\">\n";
        ptr += "<table>\n";
        ptr += "<tr><th>Message</th></tr>\n";
        int msgPos = _messagesCount;
        for (int i = 0; i < MAX_LOG_MESSAGES; i++)
        {
            // if (msgPos >= MAX_LOG_MESSAGES)
            {
                msgPos = i;
            }
            ptr += "<tr><td>" + _messages[msgPos] + "</td></tr>\n";
            msgPos++;
        }
        ptr += "</table>\n";
        ptr += "</div>\n";
    }
    ptr += "<script type=\"text/javascript\">";
    ptr += " function Redirect() { window.location=\"http://192.168.1.1\";  }";
    ptr += " setTimeout('Redirect()', 5000); ";
    ptr += "</script>";

    ptr += "</body>\n";
    ptr += "</html>\n";
    return ptr;
}
