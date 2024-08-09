#define M5STICKC_PLUS2 1 // M5StickC Plus2 has a different power on/off mechanism

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

    _display.setup(); // set up the display

    _wifiConfig = new WifiConfig();

    _display.draw_message(String("connect wifi\r\n") + String(this->_wifiConfig->WlanSSID), 1500, MSG_TYPE_INFO);
    delay(1500);
    WiFi.mode(WIFI_STA);                                                      // station mode: the ESP32 connects to an access point
    WiFi.begin(this->_wifiConfig->WlanSSID, this->_wifiConfig->WlanPassword); // connect to the WiFi network
    int trys = 10;
    while (trys-- > 0 && WiFi.status() != WL_CONNECTED)
    {
        _display.draw_message(String("connect wifi\r\n") + String(this->_wifiConfig->WlanSSID) + "\r\nRetrys " + String(trys), 1000, MSG_TYPE_INFO);
        delay(1000);
    }
    _display.draw_message(WiFi.localIP().toString(), 1000, MSG_TYPE_INFO);

    while (!(_joystick.begin(&Wire, JoyC_ADDR, 0, 26, 100000UL)))
    {
        _display.draw_message(String("I2C Error"), 500, MSG_TYPE_ERROR);
        delay(500);
    }

    this->_axp192 = new AXP192();
    this->_axp192->begin();

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

    auto batPower = this->_axp192->GetBatVoltage();
    _display.draw_message(String(this->_wifiConfig->WlanSSID) + "\r\n" + String(joy_pos_x) + "/" + String(joy_pos_y) + "\r\nBat:" + String(batPower) + "V", 500, MSG_TYPE_INFO);
    delay(500);
}
