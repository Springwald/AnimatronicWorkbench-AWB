#ifndef Mp3PlayerYX5300Serial_manager_h
#define Mp3PlayerYX5300Serial_manager_h

#include <Arduino.h>
#include <vector>
#include <MD_YX5300.h>
#include <string>
#include <SoftwareSerial.h>

class Mp3PlayerYX5300Serial
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    SoftwareSerial _mp3Stream; // MP3 player serial stream for comms
    MD_YX5300 _mp3;

public:
    String id;
    String name;

    // the constructor
    Mp3PlayerYX5300Serial(int rxPin, int txPin, String id, String name) : _mp3Stream(rxPin, txPin), _mp3(MD_YX5300(_mp3Stream)), id(id), name(name)
    {
        _mp3Stream.begin(MD_YX5300::SERIAL_BPS);
        _mp3.begin();
        _mp3.setSynchronous(false);
        _mp3.check(); // run the mp3 receiver
    }

    bool playSound(int trackNo);
    bool stopSound();
};

#endif
