#ifndef _MP3PLAYERXY5300_POINT_H_
#define _MP3PLAYERXY5300_POINT_H_

#include <Arduino.h>
#include <String.h>

class Mp3PlayerYX5300Point
{
protected:
public:
    int soundId;          // id of the sound to play
    int soundPlayerIndex; // the player to play (0 = first in player list, 1 = second in player list, ...)
    int ms;               // time position in milliseconds

    Mp3PlayerYX5300Point(int soundId, int soundPlayerIndex, int ms) : soundId(soundId), ms(ms), soundPlayerIndex(soundPlayerIndex)
    {
    }

    ~Mp3PlayerYX5300Point()
    {
    }
};

#endif