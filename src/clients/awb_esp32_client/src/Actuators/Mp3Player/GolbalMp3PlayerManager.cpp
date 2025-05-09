
#include <Arduino.h>
#include "GlobalMp3PlayerManager.h"
#include "Mp3PlayerDfPlayerMiniManager.h"
#include "Mp3PlayerYx5300Manager.h"

bool GlobalMp3PlayerManager::playSound(String playerTitle, int trackNo)
{
    if (_mp3PlayersDfPlayerMiniManager != nullptr)
    {
        int index = _mp3PlayersDfPlayerMiniManager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersDfPlayerMiniManager->playSound(index, trackNo);
    }

    if (_mp3PlayersYX5300Manager != nullptr)
    {
        int index = _mp3PlayersYX5300Manager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersYX5300Manager->playSound(index, trackNo);
    }

    return false;
}

bool GlobalMp3PlayerManager::stopSound(String playerTitle)
{
    if (_mp3PlayersDfPlayerMiniManager != nullptr)
    {
        int index = _mp3PlayersDfPlayerMiniManager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersDfPlayerMiniManager->stopSound(index);
    }

    if (_mp3PlayersYX5300Manager != nullptr)
    {
        int index = _mp3PlayersYX5300Manager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersYX5300Manager->stopSound(index);
    }
    return false;
}

bool GlobalMp3PlayerManager::setVolumeToMax(String playerTitle)
{
    if (_mp3PlayersDfPlayerMiniManager != nullptr)
    {
        int index = _mp3PlayersDfPlayerMiniManager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersDfPlayerMiniManager->setVolume(index, 30); // set to max volume
    }

    if (_mp3PlayersYX5300Manager != nullptr)
    {
        int index = _mp3PlayersYX5300Manager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return false; // XY5300 does not support volume control
    }

    return false;
}

bool GlobalMp3PlayerManager::setVolume(String playerTitle, uint8_t volume)
{
    if (_mp3PlayersDfPlayerMiniManager != nullptr)
    {
        int index = _mp3PlayersDfPlayerMiniManager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersDfPlayerMiniManager->setVolume(index, volume);
    }

    if (_mp3PlayersYX5300Manager != nullptr)
    {
        int index = _mp3PlayersYX5300Manager->getPlayerIndexByTitle(playerTitle);
        if (index != -1)
            return _mp3PlayersYX5300Manager->setVolume(index, volume);
    }

    return false;
}
