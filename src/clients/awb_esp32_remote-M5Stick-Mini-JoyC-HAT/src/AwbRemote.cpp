#define M5STICKC_PLUS2 1 // M5StickC Plus2 has a different power on/off mechanism

#include <Arduino.h>
#include <AwbRemote.h>
#include <WiFi.h>
#include "AwbDataImport/WifiConfig.h"
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"
#include <HTTPClient.h>

#define POS_X 0    // Joystick
#define POS_Y 1    // Joystick
#define BUTTON 2   // Joystick press
#define BUTTON2 37 // M5Stick middle button
#define BUTTON3 39 // M5Stick top button

/**
 * initialize the AWB remote control
 */
void AwbRemote::setup()
{

// especially for the M5StickC Plus2 (what a monster name): we need to hold the power on by setting the GPIO pin G4 to HIGH
#ifdef M5STICKC_PLUS2
    pinMode(4, OUTPUT);
    digitalWrite(4, HIGH);
#endif

    _display.setup(); // set up the display

    _wifiConfig = WifiConfig();

    _display.draw_message(String("connect wifi\r\n") + String(_wifiConfig.WlanSSID), 1500, MSG_TYPE_INFO);
    delay(1500);
    WiFi.mode(WIFI_STA);                                        // station mode: the ESP32 connects to an access point
    WiFi.begin(_wifiConfig.WlanSSID, _wifiConfig.WlanPassword); // connect to the WiFi network
    int trys = 10;
    while (trys-- > 0 && WiFi.status() != WL_CONNECTED)
    {
        _display.draw_message(String("connect wifi\r\n") + String(_wifiConfig.WlanSSID) + "\r\nRetrys " + String(trys), 1000, MSG_TYPE_INFO);
        delay(1000);
    }
    _display.draw_message(WiFi.localIP().toString(), 1000, MSG_TYPE_INFO);

    while (!(_sensor.begin(&Wire, JoyC_ADDR, 0, 26, 100000UL)))
    {
        _display.draw_message(String("I2C Error"), 500, MSG_TYPE_ERROR);
        delay(500);
    }

    _axp192 = AXP192();
    _axp192.begin();

    pinMode(BUTTON2, INPUT);
    pinMode(BUTTON3, INPUT);
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
    if (pos_x > 50)
        this->sendCommand("/remote/play/?timeline=LookUpRight");
    if (pos_x < -50)
        this->sendCommand("/remote/play/?timeline=LookUpMiddle");

    if (_sensor.getButtonStatus() == false)
        this->sendCommand("/remote/play/?timeline=Wink");

    if (digitalRead(BUTTON2) == LOW)
        this->sendCommand("/remote/play/?timeline=Stand+-+Dance");

    if (digitalRead(BUTTON3) == LOW)
        this->sendCommand("/remote/play/?timeline=The+Force+raw");

    auto batPower = _axp192.GetBatVoltage();

    _display.draw_message(String(_wifiConfig.WlanSSID) + "\r\n" + String(pos_x) + "/" + String(pos_y) + "\r\nBat:" + String(batPower) + "V", 500, MSG_TYPE_INFO);
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
