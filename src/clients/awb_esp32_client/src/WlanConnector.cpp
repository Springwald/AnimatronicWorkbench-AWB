#include <Arduino.h>
#include "AutoPlay/AutoPlayData.h"
#include "WlanConnector.h"
#include <vector>

void WlanConnector::setup()
{
    IPAddress local_ip(192, 168, 1, 1);
    IPAddress gateway(192, 168, 1, 1);
    IPAddress subnet(255, 255, 255, 0);

    WiFi.softAP(_data->WlanSSID, _data->WlanPassword);
    WiFi.softAPConfig(local_ip, gateway, subnet);

    _server->on("/", [this]()
                { this->handle_Default(); });
    //_server.on("/led1on", handle_led1on);
    _server->onNotFound([this]()
                        { this->handle_NotFound(); });
}


void WlanConnector::update()
{
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
    String ptr = "<!DOCTYPE html> <html>\n";
    ptr += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";
    ptr += "<title>" + String(_data->ProjectName) + " - (Animatronic Workbench)</title>\n";
    ptr += "<style>html { font-family: Helvetica; display: inline-block; margin: 0px auto; text-align: center;}\n";
    ptr += "body{margin-top: 50px;} h1 {color: #444444;margin: 50px auto 30px;} h3 {color: #444444;margin-bottom: 50px;}\n";
    ptr += ".button {display: block;width: 80px;background-color: #3498db;border: none;color: white;padding: 13px 30px;text-decoration: none;font-size: 25px;margin: 0px auto 35px;cursor: pointer;border-radius: 4px;}\n";
    ptr += ".button-on {background-color: #3498db;}\n";
    ptr += ".button-on:active {background-color: #2980b9;}\n";
    ptr += ".button-off {background-color: #34495e;}\n";
    ptr += ".button-off:active {background-color: #2c3e50;}\n";
    ptr += "p {font-size: 14px;color: #888;margin-bottom: 10px;}\n";
    ptr += "</style>\n";
    ptr += "</head>\n";
    ptr += "<body>\n";
    ptr += "<h1>(Animatronic Workbench)</h1>\n";
    ptr += "<h3>" + String(_data->ProjectName) + "</h3>\n";

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

    ptr += "</body>\n";
    ptr += "</html>\n";
    return ptr;
}