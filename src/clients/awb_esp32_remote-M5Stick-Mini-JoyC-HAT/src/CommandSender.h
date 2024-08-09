#ifndef _COMMAND_SENDER_H_
#define _COMMAND_SENDER_H_

#include <Arduino.h>
#include "AwbDisplay.h"
#include "AwbDataImport/WifiConfig.h"

using byte = unsigned char;

class CommandSender
{
protected:
    AwbDisplay _display;

public:
    CommandSender(AwbDisplay display) : _display(display)
    {
    }

    ~CommandSender()
    {
    }

    void playTimeline(String timelineName);
    void sendCommand(String command);
};

#endif