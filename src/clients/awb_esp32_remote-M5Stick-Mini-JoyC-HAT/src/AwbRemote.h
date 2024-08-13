#ifndef _AWB_REMOTE_H_
#define _AWB_REMOTE_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"
#include <AXP192.h>
#include "AwbDataImport/WifiConfig.h"
#include "CustomCode/CustomCode.h"
#include "CommandSender.h"

using byte = unsigned char;

class AwbRemote
{
protected:
    AwbDisplay _display; /// The display, oled or lcd
    UNIT_JOYC _joystick;
    AXP192 *_axp192;
    WifiConfig *_wifiConfig;
    CustomCode *_customCode;
    CommandSender *_commandSender;

    void sendCommand(String command);

public:
    AwbRemote()
    {
        _commandSender = new CommandSender(_display);
        _customCode = new CustomCode(_display, _commandSender);
    }

    ~AwbRemote()
    {
        delete _customCode;
    }

    void setup();
    void loop();
};

#endif