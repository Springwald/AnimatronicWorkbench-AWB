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
    Mp3PlayerYX5300Serial(int rxPin, int txPin, String name, String id, TCallBackErrorOccured errorOccured) : _errorOccured(errorOccured), name(name), id(id)
    {
        _serial = new SoftwareSerial(rxPin, txPin);
        _serial->begin(MD_YX5300::SERIAL_BPS);
        _mp3 = new MD_YX5300(*_serial);
        _mp3->begin();
        _mp3->setSynchronous(false);
        if (!_mp3->check())
            _errorOccured("YX5300: check failed");          // check if the mp3 receiver is connected
        if (!this->setVolumeToMax())                        // set the volume to max
            _errorOccured("YX5300: setVolumeToMax failed"); // check if the volume is set to max
    }

    bool playSound(int trackNo);
    bool stopSound();
    bool setVolume(uint8_t volume);
    bool setVolumeToMax();
};

#endif
