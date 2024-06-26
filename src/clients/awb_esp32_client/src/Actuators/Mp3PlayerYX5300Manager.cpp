
#include <Arduino.h>
#include "Mp3PlayerYX5300Manager.h"

bool Mp3PlayerYX5300Manager::playSound(int playerIndex, int trackNo)
{
    if (playerIndex < _mp3Players->size())
        return _mp3Players->at(playerIndex).playSound(trackNo);
    return false;
}

bool Mp3PlayerYX5300Manager::stopSound(int playerIndex)
{
    if (playerIndex < _mp3Players->size())
        return _mp3Players->at(playerIndex).stopSound();
    return false;
}
