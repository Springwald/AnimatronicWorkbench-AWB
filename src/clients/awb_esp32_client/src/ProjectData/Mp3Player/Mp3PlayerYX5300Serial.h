#ifndef Mp3PlayerYX5300Serial_h
#define Mp3PlayerYX5300Serial_h

#include <Arduino.h>
#include <vector>
#include <MD_YX5300.h>
#include <string>
#include <SoftwareSerial.h>

class Mp3PlayerYX5300Serial
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    MD_YX5300 *_mp3;
    SoftwareSerial *_serial;

public:
    String id;
    String name;
    TCallBackErrorOccured _errorOccured;

    // the constructor
    Mp3PlayerYX5300Serial(int rxPin, int txPin, String name, String id) : name(name), id(id)
    {
        _serial = new SoftwareSerial(rxPin, txPin);
        _serial->begin(MD_YX5300::SERIAL_BPS);
        _mp3 = new MD_YX5300(*_serial);
        _mp3->begin();
        _mp3->setSynchronous(false);
        _mp3->check(); // run the mp3 receiver
    }

    bool playSound(int trackNo);
    bool stopSound();
};

#endif
