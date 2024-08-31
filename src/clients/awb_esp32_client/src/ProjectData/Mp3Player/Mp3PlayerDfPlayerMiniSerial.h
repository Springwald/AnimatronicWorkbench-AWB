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
    bool checkOk();

public:
    String name;
    TCallBackErrorOccured _errorOccured;

    // the constructor
    Mp3PlayerDfPlayerMiniSerial(int rxPin, int txPin, int volume, String name) : name(name)
    {
        _serial = new SoftwareSerial(/*rx =*/rxPin, /*tx =*/txPin);
        _serial->begin(9600);

        if (!_myDFPlayer.begin(*_serial, /*isACK = */ true, /*doReset = */ true))
        { // Use serial to communicate with mp3.
            _errorOccured("Unable to begin:");
            _errorOccured("1.Please recheck the connection!");
            _errorOccured("2.Please insert the SD card!");
        }
        else
        {
            _myDFPlayer.EQ(DFPLAYER_EQ_NORMAL);
            _myDFPlayer.outputDevice(DFPLAYER_DEVICE_SD);
            _myDFPlayer.volume(volume); // Set volume value (0~30).
        }
    }

    bool
    playSound(int trackNo);
    bool stopSound();
    bool setVolume(int volume);
};

#endif
