#include <Arduino.h>
#include <AwbRemote.h>
#include <WiFi.h>
#include "WifiConfig.h"
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"
#include <HTTPClient.h>

#define POS_X 0
#define POS_Y 1

/**
 * initialize the AWB remote control
 */
void AwbRemote::setup()
{
    _display.setup(); // set up the display

    auto wifiConfig = WifiConfig();

    _display.draw_message(String("connect to wifi network") + String(wifiConfig.WlanSSID), 1500, MSG_TYPE_INFO);
    delay(1500);
    WiFi.mode(WIFI_STA);                                      // station mode: the ESP32 connects to an access point
    WiFi.begin(wifiConfig.WlanSSID, wifiConfig.WlanPassword); // connect to the WiFi network
    int trys = 10;
    while (WiFi.status() != WL_CONNECTED)
    {
        _display.draw_message(String("connect to wifi network") + String(wifiConfig.WlanSSID) + " " + String(trys), 1000, MSG_TYPE_INFO);
        delay(1000);
    }
    _display.draw_message(WiFi.localIP().toString(), 1000, MSG_TYPE_INFO);

    while (!(_sensor.begin(&Wire, JoyC_ADDR, 0, 26, 100000UL)))
    {
        _display.draw_message(String("I2C Error"), 500, MSG_TYPE_ERROR);
        delay(500);
    }
}

/**
 * the main loop of the AWB remote control
 */
void AwbRemote::loop()
{
    int8_t pos_x = _sensor.getPOSValue(POS_X, _8bit);
    int8_t pos_y = _sensor.getPOSValue(POS_Y, _8bit);

    if (pos_y > 50)
        this->sendCommand("/remote/play/?timeline=YES");
    if (pos_y < -50)
        this->sendCommand("/remote/play/?timeline=NO");
    _display.draw_message(String(pos_x), 500, MSG_TYPE_ERROR);
    delay(500);
}

void AwbRemote::sendCommand(String command)
{
    if (WiFi.status() == WL_CONNECTED)
    {
        HTTPClient http;
        String serverPath = "http://192.168.1.1" + command;

        _display.draw_message(command, 500, MSG_TYPE_INFO);

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
            _display.draw_message("HTTP Response code: " + httpResponseCode + http.getString() + payload, 1500, MSG_TYPE_INFO);
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
