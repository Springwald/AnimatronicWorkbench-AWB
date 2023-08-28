#ifndef _TIMELINE_STATE_H_
#define _TIMELINE_STATE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>

using byte = unsigned char;

/**
 * Represents a timeline state
 * this is kind of category for timelines like "idle", "sleeping" or "talking" to filter timelines for autoplay
 */
class TimelineState
{

protected:
public:
    int id;      /// the id of the state
    String name; /// the name of the state

public:
    TimelineState(int id, String const name) : id(id), name(name)
    {
    }

    ~TimelineState()
    {
    }
};

#endif