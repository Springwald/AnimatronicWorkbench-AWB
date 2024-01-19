#ifndef _TIMELINE_H_
#define _TIMELINE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "StsServoPoint.h"
#include "Pca9685PwmServoPoint.h"
#include "TimelineState.h"

using byte = unsigned char;

class Timeline
{

protected:
public:
    String name;                                         /// the name of the timeline
    TimelineState *state;                                /// the state of the timeline
    std::vector<StsServoPoint> *stsServoPoints;          /// the sts servo points of the timeline
    std::vector<StsServoPoint> *scsServoPoints;          /// the scs servo points of the timeline
    std::vector<Pca9685PwmServoPoint> *pca9685PwmPoints; /// the Pca9685 Pwm servo points of the timeline
    int durationMs;                                      /// the duration of the timeline in milliseconds

public:
    Timeline(TimelineState *state, String name, std::vector<StsServoPoint> *p_stsPoints, std::vector<StsServoPoint> *p_scsPoints, std::vector<Pca9685PwmServoPoint> *p_pca9685PwmPoints) : stsServoPoints(p_stsPoints), scsServoPoints(p_scsPoints), pca9685PwmPoints(p_pca9685PwmPoints), name(name), state(state)
    {
        durationMs = 0;

        // calculate highest duration for sts servo points
        for (int i = 0; i < stsServoPoints->size(); i++)
        {
            // get the ms value of the point
            if (stsServoPoints->at(i).ms > durationMs)
                durationMs = stsServoPoints->at(i).ms;
        }

        // calculate highest duration for scs servo points
        for (int i = 0; i < scsServoPoints->size(); i++)
        {
            // get the ms value of the point
            if (scsServoPoints->at(i).ms > durationMs)
                durationMs = scsServoPoints->at(i).ms;
        }

        // calculate highest duration for Pca9685 Pwm servo points
        for (int i = 0; i < pca9685PwmPoints->size(); i++)
        {
            // get the ms value of the point
            if (pca9685PwmPoints->at(i).ms > durationMs)
                durationMs = pca9685PwmPoints->at(i).ms;
        }

        // calculate duration for other points
        // todo
    }

    ~Timeline()
    {
    }
};

#endif