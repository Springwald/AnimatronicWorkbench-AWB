
#include <Arduino.h>
#include "Mp3PlayerDfPlayerMiniSerial.h"

bool Mp3PlayerDfPlayerMiniSerial::playSound(int trackNo)
{
    _myDFPlayer.play(trackNo);
    if (checkOk())
        return true;
    _errorOccured("DfPlayerMini: Unable to play sound track " + String(trackNo));
    return false;
}

bool Mp3PlayerDfPlayerMiniSerial::stopSound()
{
    _myDFPlayer.stop();
    if (checkOk())
        return true;
    _errorOccured("DfPlayerMini: Unable to stop sound");
    return false;
}

bool Mp3PlayerDfPlayerMiniSerial::setVolume(int volume)
{
    _myDFPlayer.volume(volume); // Set volume value (0~30).
    if (checkOk())
        return true;
    _errorOccured("DfPlayerMini: Unable to set volume");
    return false;
}

bool Mp3PlayerDfPlayerMiniSerial::checkOk()
{
    if (_myDFPlayer.available())
    {
        uint8_t type = _myDFPlayer.readType();
        uint8_t value = _myDFPlayer.read();
        if (type == DFPlayerError)
        {
            switch (value)
            {
            case Busy:
                //_errorOccured("DFPlayer Error: Busy");
                break;
            case Sleeping:
                //_errorOccured("DFPlayer Error: Sleeping");
                break;
            case SerialWrongStack:
                _errorOccured("DFPlayer Error: Serial Wrong Stack");
                break;
            case CheckSumNotMatch:
                _errorOccured("DFPlayer Error: Check Sum Not Match");
                break;
            case FileIndexOut:
                _errorOccured("DFPlayer Error: File Index Out");
                break;
            case FileMismatch:
                _errorOccured("DFPlayer Error: File Mismatch");
                break;
            case Advertise:
                _errorOccured("DFPlayer Error: Advertise");
                break;
            default:
                _errorOccured("DFPlayer Error: Unknown");
                break;
            }
            return false;
        }
        return true;
    }
}
