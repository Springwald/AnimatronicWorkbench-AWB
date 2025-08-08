#include <Arduino.h>
#include <AwbRemote.h>
#include <WiFi.h>
#include "AwbDataImport/WifiConfig.h"
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"
#include <HTTPClient.h>
#include "Hardware.h"

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

#ifdef RED_LED
    pinMode(RED_LED, OUTPUT);           // init red led
    digitalWrite(RED_LED, RED_LED_OFF); // turn red led off
#endif

    _display.setup(); // set up the display

    this->_axp192 = new AXP192();
    this->_axp192->begin();

    _wifiConfig = new WifiConfig();

    _display.draw_message(String("connect wifi\r\n") + String(this->_wifiConfig->WlanSSID), 1500, MSG_TYPE_INFO);
    delay(1500);
    WiFi.mode(WIFI_STA);                                                      // station mode: the ESP32 connects to an access point
    WiFi.begin(this->_wifiConfig->WlanSSID, this->_wifiConfig->WlanPassword); // connect to the WiFi network
    int trys = 10;
    while (trys-- > 0 && WiFi.status() != WL_CONNECTED)
    {
        _lastBatteryCheckMs = millis();
        _batPower = this->_axp192->GetBatVoltage();

        _display.draw_message(String("connect wifi\r\n") + String(this->_wifiConfig->WlanSSID) + "\r\nRetrys " + String(trys), 1000, MSG_TYPE_INFO);
#ifdef RED_LED
        digitalWrite(RED_LED, RED_LED_ON); // turn red led on
#endif
        delay(1000);
    }
    if (WiFi.status() != WL_CONNECTED)
    {
        _display.draw_message(String("connect wifi\r\n") + String(this->_wifiConfig->WlanSSID) + "\r\nFailed", 1000, MSG_TYPE_ERROR);
        delay(1000);
    }
    else
    {
#ifdef RED_LED
        digitalWrite(RED_LED, RED_LED_OFF); // turn red led off
#endif
    }

    _display.draw_message(WiFi.localIP().toString(), 1000, MSG_TYPE_INFO);
    trys = 10;
    while (trys-- > 0 && !(_joystick.begin(&Wire, JoyC_ADDR, 0, 26, 100000UL)))
    {
        _display.draw_message(String("Joystick not found\r\nRetrying... ") + String(trys), 500, MSG_TYPE_ERROR);
        delay(500);
    }

    pinMode(BUTTON2, INPUT);
    pinMode(BUTTON3, INPUT);

    this->_customCode->setup();
}

/**
 * the main loop of the AWB remote control
 */
void AwbRemote::loop()
{
    int8_t joy_pos_x = _joystick.getPOSValue(POS_X, _8bit);
    int8_t joy_pos_y = _joystick.getPOSValue(POS_Y, _8bit);

    bool joyButton = _joystick.getButtonStatus() == false;
    bool button2 = digitalRead(BUTTON2) == LOW;
    bool button3 = digitalRead(BUTTON3) == LOW;

    this->_customCode->loop(joy_pos_x, joy_pos_y, joyButton, button2, button3);

    auto timeSinceLastBatteryCheck = millis() - _lastBatteryCheckMs;
    if (timeSinceLastBatteryCheck > 20000)
    {
        _lastBatteryCheckMs = millis();
        _batPower = this->_axp192->GetBatVoltage();
    }

    _display.draw_message(String(this->_wifiConfig->WlanSSID) + "\r\nConnected:" + String(WiFi.isConnected()) + "\r\n" + String(joy_pos_x) + "/" + String(joy_pos_y) + "\r\nBat: " + String(_batPower) + "V\r\n" + String(BatteryPercent(_batPower)) + "%", 500, MSG_TYPE_INFO);

    delay(100);
}

float AwbRemote::BatteryPercent(float batPower)
{
    // Calculate the battery percentage based on the voltage
    float percent = max(0.0f, batPower - MIN_VOLTAGE) / (MAX_VOLTAGE - MIN_VOLTAGE) * 100;
    return constrain(percent, 0, 100); // Ensure the percentage is between 0 and 100
}