#ifndef Mp3PlayerDfPlayerMini_h
#define Mp3PlayerDfPlayerMini_h

#include <Arduino.h>
#include <vector>
#include <string>
#include <SoftwareSerial.h>
#include <DFRobotDFPlayerMini.h>

class Mp3PlayerDfPlayerMiniSerial
{
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    SoftwareSerial *_serial;
    DFRobotDFPlayerMini _myDFPlayer;
    uint8_t _initialVolume;
    bool checkOk();

public:
    String id;
    String name;
    TCallBackErrorOccured _errorOccured;

    // the constructor
    Mp3PlayerDfPlayerMiniSerial(int rxPin, int txPin, uint8_t volume, String name, String id, TCallBackErrorOccured errorOccured) : name(name), id(id), _errorOccured(errorOccured), _initialVolume(volume)
    {
        _serial = new SoftwareSerial(/*rx =*/rxPin, /*tx =*/txPin);
        _serial->begin(9600);

        if (!_myDFPlayer.begin(*_serial, /*isACK = */ true, /*doReset = */ true))
        {
            // Use serial to communicate with mp3.
            _errorOccured("DfPlayerMini: Unable to begin:");
            _errorOccured("1.Please recheck the connection!");
            _errorOccured("2.Please insert the SD card!");
        }
    }

    bool setup();
    bool playSound(int trackNo);
    bool stopSound();
    bool setVolume(uint8_t volume);
};

#endif
