
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
