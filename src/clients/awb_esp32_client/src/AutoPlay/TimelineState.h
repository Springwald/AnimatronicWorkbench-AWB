#ifndef _TIMELINE_STATE_H_
#define _TIMELINE_STATE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>

using byte = unsigned char;

class TimelineState
{

protected:
public:
    int id;
    String name;

public:
    TimelineState(int id, String const name) : id(id), name(name)
    {
    }

    ~TimelineState()
    {
    }
};

#endif