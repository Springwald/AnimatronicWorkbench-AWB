#ifndef Mp3PlayerYX5300_manager_h
#define Mp3PlayerYX5300_manager_h

#include <Arduino.h>
#include <vector>
#include "../../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"

class Mp3PlayerYX5300Manager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    std::vector<Mp3PlayerYX5300Serial> *_mp3Players;

public:
    // the constructor
    Mp3PlayerYX5300Manager(std::vector<Mp3PlayerYX5300Serial> *mp3Players, TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured), _messageToShow(messageToShow), _mp3Players(mp3Players)
    {
    }

    bool playSound(int playerIndex, int trackNo);
    bool stopSound(int playerIndex);
    int getPlayerIndexByTitle(String playerTitle);
};

#endif
