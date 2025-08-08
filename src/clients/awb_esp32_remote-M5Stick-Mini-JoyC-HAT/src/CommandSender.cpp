#include <Arduino.h>
#include <WiFi.h>
#include "AwbDataImport/WifiConfig.h"
#include <HTTPClient.h>
#include "CommandSender.h"
#include "Hardware.h"

bool CommandSender::playTimeline(String timelineName)
{
    _display.draw_message("timeline:\r\n" + timelineName, 500, MSG_TYPE_INFO);
    return this->sendCommand("/remote/play/?timeline=" + timelineName);
}

bool CommandSender::sendCommand(String command)
{
    bool success = false;

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
            digitalWrite(RED_LED, HIGH); // turn red led off
            String payload = http.getString();
            _display.draw_message(payload, 1500, MSG_TYPE_INFO);
            delay(1500);
            success = true;
        }
        else
        {
            digitalWrite(RED_LED, LOW); // turn red led on
            _display.draw_message("Error code: " + httpResponseCode, 1500, MSG_TYPE_ERROR);
            delay(1500);
        }
        // Free resources
        http.end();
        digitalWrite(RED_LED, HIGH); // turn red led off
    }
    else
    {
        digitalWrite(RED_LED, LOW); // turn red led on
        _display.draw_message("WiFi Disconnected", 1500, MSG_TYPE_ERROR);
        delay(1500);
    }

    return success;
}
