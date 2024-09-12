
#include <Arduino.h>
#include "Mp3PlayerYX5300Manager.h"

bool Mp3PlayerYX5300Manager::playSound(int playerIndex, int trackNo)
{
    if (playerIndex < _mp3Players->size())
    {
        auto player = _mp3Players->at(playerIndex);
        return player.playSound(trackNo);
    }
    return false;
}

bool Mp3PlayerYX5300Manager::stopSound(int playerIndex)
{
    if (playerIndex < _mp3Players->size())
    {
        auto player = _mp3Players->at(playerIndex);
        return player.stopSound();
    }
    return false;
}

int Mp3PlayerYX5300Manager::getPlayerIndexByTitle(String playerTitle)
{
    for (int i = 0; i < _mp3Players->size(); i++)
        if (_mp3Players->at(i).id == playerTitle)
            return i;
    return -1;
}
