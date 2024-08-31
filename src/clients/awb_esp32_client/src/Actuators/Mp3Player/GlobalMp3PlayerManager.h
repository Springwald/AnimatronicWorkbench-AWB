#ifndef Global_Mp3Player_manager_h
#define Global_Mp3Player_manager_h

#include <Arduino.h>
#include <vector>
#include "Mp3PlayerYX5300Manager.h"
#include "Mp3PlayerDfPlayerMiniManager.h"

class GlobalMp3PlayerManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    Mp3PlayerYX5300Manager *_mp3PlayersYX5300Manager;
    Mp3PlayerDfPlayerMiniManager *_mp3PlayersDfPlayerMiniManager;

public:
    // the constructor
    GlobalMp3PlayerManager(
        Mp3PlayerDfPlayerMiniManager *mp3PlayersDfPlayerMiniManager,
        Mp3PlayerYX5300Manager *mp3PlayersYX5300Manager,
        TCallBackErrorOccured errorOccured,
        TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured),
                                                _messageToShow(messageToShow),
                                                _mp3PlayersYX5300Manager(mp3PlayersYX5300Manager),
                                                _mp3PlayersDfPlayerMiniManager(mp3PlayersDfPlayerMiniManager)
    {
    }

    bool playSound(String playerId, int trackNo);
    bool stopSound(String playerId);
    bool setVolume(String playerId, int volume);
};

#endif