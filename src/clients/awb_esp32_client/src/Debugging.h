#ifndef _DEBUGGING_H_
#define _DEBUGGING_H_

#include "AwbDisplay.h"
#include "AwbDataImport/HardwareConfig.h"

/**
 * the actual hardware- and autoplayer-status
 */
class Debugging
{
public:
    static const int MJ_AWB_CLIENT_LOOP = 1;
    static const int MJ_WLAN = 2;

private:
    AwbDisplay *_display;
    bool _debugging = true;
    long _lastInputUpdateMillis = -1;

public:
    Debugging(AwbDisplay *display) : _display(display)
    {
        _lastInputUpdateMillis = millis();
#ifdef DEBUGGING_IO_PIN
        pinMode(DEBUGGING_IO_PIN, INPUT);
#endif
    }

    void setState(int major, int minor);
    bool isDebugging();

    ~Debugging()
    {
    }
};

#endif