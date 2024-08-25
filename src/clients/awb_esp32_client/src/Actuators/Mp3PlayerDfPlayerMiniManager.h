#ifndef Mp3PlayerDfPlayerMini_manager_h
#define Mp3PlayerDfPlayerMini_manager_h

#include <Arduino.h>
#include <vector>
#include "../ProjectData/Mp3PlayerDfPlayerMiniSerial.h"

class Mp3PlayerDfPlayerMiniManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    std::vector<Mp3PlayerDfPlayerMiniSerial> *_mp3Players;

public:
    // the constructor
    Mp3PlayerDfPlayerMiniManager(std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3Players, TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured), _messageToShow(messageToShow), _mp3Players(mp3Players)
    {
    }

    bool playSound(int playerIndex, int trackNo);
    bool stopSound(int playerIndex);
    bool setVolume(int playerIndex, int volume);
};

#endif
