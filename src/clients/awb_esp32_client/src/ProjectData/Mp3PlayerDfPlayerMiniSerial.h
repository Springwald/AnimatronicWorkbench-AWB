#ifndef Mp3PlayerDfPlayerMini_h
#define Mp3PlayerDfPlayerMini_h

#include <Arduino.h>
#include <vector>
#include <MD_YX5300.h>
#include <string>
#include <SoftwareSerial.h>

class Mp3PlayerDfPlayerMiniSerial
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    SoftwareSerial *_serial;

public:
    String name;
    TCallBackErrorOccured _errorOccured;
    int volume;

    // the constructor
    Mp3PlayerDfPlayerMiniSerial(int rxPin, int txPin, int volume, String name) : name(name), volume(volume)
    {
    }

    bool playSound(int trackNo);
    bool stopSound();
    bool setVolume(int volume);
};

#endif
