#ifndef _TIMELINE_H_
#define _TIMELINE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "StsServoPoint.h"
#include "TimelineState.h"

using byte = unsigned char;

class Timeline
{

protected:
public:
    String name;                                /// the name of the timeline
    TimelineState *state;                       /// the state of the timeline
    std::vector<StsServoPoint> *stsServoPoints; /// the sts servo points of the timeline
    int durationMs;                             /// the duration of the timeline in milliseconds

public:
    Timeline(TimelineState *state, String name, std::vector<StsServoPoint> *p_stsPoints) : stsServoPoints(p_stsPoints), name(name), state(state)
    {
        durationMs = 0;
        // calculate duration for sts servo points
        for (int i = 0; i < stsServoPoints->size(); i++)
        {
            // get the ms value of the point
            if (stsServoPoints->at(i).ms > durationMs)
                durationMs = stsServoPoints->at(i).ms;
        }

        // calculate duration for other points
        // todo
    }

    ~Timeline()
    {
    }
};

#endif