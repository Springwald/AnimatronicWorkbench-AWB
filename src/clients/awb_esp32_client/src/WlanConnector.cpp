#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
#include "WlanConnector.h"
#include <vector>
#include <WiFi.h>
#include <WebServer.h>
#include <String.h>

void WlanConnector::setup()
{
    logInfo(String("Starting up..."));

    IPAddress local_ip(192, 168, 1, 1);
    IPAddress gateway(192, 168, 1, 1);
    IPAddress subnet(255, 255, 255, 0);

    WiFi.softAP(_data->WlanSSID, _data->WlanPassword);
    WiFi.softAPConfig(local_ip, gateway, subnet);

    _server = new WebServer(80);

    _server->on("/", [this]()
                { this->handle_Default(); });
    //_server.on("/led1on", handle_led1on);
    _server->onNotFound([this]()
                        { this->handle_NotFound(); });
    _server->begin();
}

void WlanConnector::update()
{
    _server->handleClient();
}

void WlanConnector::logError(String msg)
{
    logInfo(String("Error! ") + msg);
}

void WlanConnector::logInfo(String msg)
{
    _messagesCount++;
    if (_messagesCount >= MAX_LOG_MESSAGES)
    {
        _messagesCount = 0;
    }
    _messages[_messagesCount] = msg;
}

void WlanConnector::handle_Default()
{
    _server->send(200, "text/html", GetHtml());
}

void WlanConnector::handle_NotFound()
{
    _server->send(404, "text/plain", "Not found");
}

String WlanConnector::GetHtml()
{
    auto ageSeconds = (millis() - _startTime) / 1000;

    String ptr = "<!DOCTYPE html> <html>\n";
    ptr += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";
    ptr += "<title>" + String(_data->ProjectName) + " - (Animatronic Workbench)</title>\n";
    ptr += "<style>html { font-family: Helvetica; display: inline-block; margin: 0px auto; text-align: center;}\n";
    ptr += "body{ margin-top: 0px; width: 100%;}\n";
    ptr += ".region { display: table; margin: 0.5em auto; border: 1px solid black; padding: 0.2em 1em 0.2em 1em;}\n";
    ptr += "tr:nth-child(even) {background-color: #eeeeee;}\n";
    ptr += "table td { margin: 0; border: none; padding: 0 1em 0 1em;}\n";
    ptr += "</style>\n";
    ptr += "</head>\n";
    ptr += "<body>\n";
    ptr += "<p>Animatronic Workbench - Client ID " + String(_clientId) + "</p>\n";
    ptr += "<h1>'" + String(_data->ProjectName) + "' figure</h1>\n";
    ptr += "<p> " + String(ageSeconds) + " seconds uptime</p>\n";
    ptr += "<div class=\"region\">\n";
    ptr += "<span>" + String(_stsServoValues->size()) + " STS-Servos</span>\n";
    ptr += "<table>\n";
    ptr += "<tr><th>Id</th><th>Name</th><th>Pos</th><th>Temp</th><th>Load</th></tr>\n";
    for (int i = 0; i < _stsServoValues->size(); i++)
    {
        auto servo = _stsServoValues->at(i);
        ptr += "<tr><td>" + String(servo.id) + "</td><td>" + servo.name + "</td><td>" + String(servo.currentValue) + "</td><td>" + String(servo.temperature) + "</td><td>" + String(servo.load) + " </tr>\n";
    }
    ptr += "</table>\n";
    ptr += "</div>\n";

    ptr += "<div class=\"region\">\n";
    ptr += "<table>\n";
    ptr += "<tr><th>Message</th></th></tr>\n";
    auto msgPos = _messagesCount;
    for (int i = 0; i < MAX_LOG_MESSAGES; i++)
    {
        ptr += "<tr><td>" + _messages[msgPos] + "</td></tr>\n";
        msgPos++;
        if (msgPos >= MAX_LOG_MESSAGES)
        {
            msgPos = 0;
        }
    }
    ptr += "</table>\n";
    ptr += "</div>\n";

    /*   if (led1stat)
       {
           ptr += "<p>LED1 Status: ON</p><a class=\"button button-off\" href=\"/led1off\">OFF</a>\n";
       }
       else
       {
           ptr += "<p>LED1 Status: OFF</p><a class=\"button button-on\" href=\"/led1on\">ON</a>\n";
       }

       if (led2stat)
       {
           ptr += "<p>LED2 Status: ON</p><a class=\"button button-off\" href=\"/led2off\">OFF</a>\n";
       }
       else
       {
           ptr += "<p>LED2 Status: OFF</p><a class=\"button button-on\" href=\"/led2on\">ON</a>\n";
       }*/

    ptr += "<script type=\"text/javascript\">";
    ptr += " function Redirect() { window.location=\"http://192.168.1.1\";  }";
    ptr += " setTimeout('Redirect()', 2000); ";
    ptr += "</script>";

    ptr += "</body>\n";
    ptr += "</html>\n";
    return ptr;
}
