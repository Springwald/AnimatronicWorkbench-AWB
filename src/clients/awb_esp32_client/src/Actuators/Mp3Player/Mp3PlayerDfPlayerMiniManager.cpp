
#include <Arduino.h>
#include "Mp3PlayerDfPlayerMiniManager.h"

bool Mp3PlayerDfPlayerMiniManager::playSound(int playerIndex, int trackNo)
{
    if (playerIndex < _mp3Players->size())
    {
        auto player = _mp3Players->at(playerIndex);
        return player.playSound(trackNo);
    }
    return false;
}

bool Mp3PlayerDfPlayerMiniManager::stopSound(int playerIndex)
{
    if (playerIndex < _mp3Players->size())
    {
        auto player = _mp3Players->at(playerIndex);
        return player.stopSound();
    }
    return false;
}

bool Mp3PlayerDfPlayerMiniManager::setVolume(int playerIndex, int volume)
{
    if (playerIndex < _mp3Players->size())
    {
        auto player = _mp3Players->at(playerIndex);
        return player.setVolume(volume);
    }
    return false;
}

int Mp3PlayerDfPlayerMiniManager::getPlayerIndexByTitle(String playerTitle)
{
    for (int i = 0; i < _mp3Players->size(); i++)
        if (_mp3Players->at(i).name == playerTitle)
            return i;
    return -1;
}
