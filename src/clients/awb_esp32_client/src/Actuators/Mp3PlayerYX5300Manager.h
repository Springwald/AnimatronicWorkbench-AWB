#ifndef Mp3PlayerYX5300_manager_h
#define Mp3PlayerYX5300_manager_h

#include <Arduino.h>
#include <vector>
#include <MD_YX5300.h>
#include "ActuatorValue.h"
#include <SoftwareSerial.h>

// SoftwareSerial MP3Stream(13, 14);
//  MD_YX5300 mp3(MP3Stream);

class Mp3PlayerYX5300Manager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    SoftwareSerial _mp3Stream; // MP3 player serial stream for comms
    MD_YX5300 _mp3;

    // MP3Stream Serial3;
    //  MP3Stream Serial3(13, 14);
    // MD_YX5300 _mp3(MP3Stream);
    // MD_YX5300 mp3(MP3Stream);
    //  SoftwareSerial _mp3Stream; // MP3 player serial stream for comms
    //  MD_YX5300 _mp3;

public:
    // the constructor
    Mp3PlayerYX5300Manager(int rxPin, int txPin, TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured), _messageToShow(messageToShow), _mp3Stream(rxPin, txPin), _mp3(MD_YX5300(_mp3Stream))
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
