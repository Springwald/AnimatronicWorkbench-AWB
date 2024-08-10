#define M5STICKC_PLUS2 1 // M5StickC Plus2 has a different power on/off mechanism

#include <Arduino.h>
#include <WiFi.h>
#include "AwbDataImport/WifiConfig.h"
#include <HTTPClient.h>
#include "CommandSender.h"

void CommandSender::playTimeline(String timelineName)
{
    _display.draw_message("timeline:\r\" + timelineName, 500, MSG_TYPE_INFO);
    this->sendCommand("/remote/play/?timeline=" + timelineName);
}

void CommandSender::sendCommand(String command)
{
    if (WiFi.status() == WL_CONNECTED)
    {
        // url encode command
        command.replace(" ", "%20");

        HTTPClient http;
        String serverPath = "http://192.168.1.1" + command;

        //_display.draw_message(command, 500, MSG_TYPE_INFO);

        // Your Domain name with URL path or IP address with path
        http.begin(serverPath.c_str());

        // If you need Node-RED/server authentication, insert user and password below
        // http.setAuthorization("REPLACE_WITH_SERVER_USERNAME", "REPLACE_WITH_SERVER_PASSWORD");

        // Send HTTP GET request
        int httpResponseCode = http.GET();

        delay(500);

        if (httpResponseCode > 0)
        {
            String payload = http.getString();
            _display.draw_message(payload, 1500, MSG_TYPE_INFO);
            delay(1500);
        }
        else
        {
            _display.draw_message("Error code: " + httpResponseCode, 1500, MSG_TYPE_ERROR);
            delay(1500);
        }
        // Free resources
        http.end();
    }
    else
    {
        _display.draw_message("WiFi Disconnected", 1500, MSG_TYPE_ERROR);
        delay(1500);
    }
}
