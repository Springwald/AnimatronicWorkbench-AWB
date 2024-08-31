#ifndef _MP3PLAYERDFPLAYERMINI_POINT_H_
#define _MP3PLAYERDFPLAYERMINI_POINT_H_

#include <Arduino.h>
#include <String.h>

class Mp3PlayerDfPlayerMiniPoint
{
protected:
public:
    int soundId;          // id of the sound to play
    int soundPlayerIndex; // the player to play (0 = first in player list, 1 = second in player list, ...)
    int ms;               // time position in milliseconds

    Mp3PlayerDfPlayerMiniPoint(int soundId, int soundPlayerIndex, int ms) : soundId(soundId), ms(ms), soundPlayerIndex(soundPlayerIndex)
    {
    }

    ~Mp3PlayerDfPlayerMiniPoint()
    {
    }
};

#endif