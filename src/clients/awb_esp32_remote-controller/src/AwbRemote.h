#ifndef _AWB_REMOTE_H_
#define _AWB_REMOTE_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"

using byte = unsigned char;

class AwbRemote
{
protected:
    AwbDisplay _display; /// The display, oled or lcd
    UNIT_JOYC _sensor;

    void sendCommand(String command);

public:
    AwbRemote()
    {
    }

    ~AwbRemote()
    {
        // delete _packetSenderReceiver;
    }

    void setup();
    void loop();
};

#endif