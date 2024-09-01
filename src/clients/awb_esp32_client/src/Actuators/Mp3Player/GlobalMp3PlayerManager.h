#ifndef Global_Mp3Player_manager_h
#define Global_Mp3Player_manager_h

#include <Arduino.h>
#include <vector>
#include "Mp3PlayerYX5300Manager.h"
#include "Mp3PlayerDfPlayerMiniManager.h"

class GlobalMp3PlayerManager
{

private:
    Mp3PlayerYX5300Manager *_mp3PlayersYX5300Manager;
    Mp3PlayerDfPlayerMiniManager *_mp3PlayersDfPlayerMiniManager;

public:
    // the constructor
    GlobalMp3PlayerManager(
        Mp3PlayerDfPlayerMiniManager *mp3PlayersDfPlayerMiniManager,
        Mp3PlayerYX5300Manager *mp3PlayersYX5300Manager) : _mp3PlayersYX5300Manager(mp3PlayersYX5300Manager),
                                                           _mp3PlayersDfPlayerMiniManager(mp3PlayersDfPlayerMiniManager)
    {
    }

    bool playSound(String playerTitle, int trackNo);
    bool stopSound(String playerTitle);
    bool setVolume(String playerTitle, uint8_t volume);
};

#endif