#ifndef _TIMELINE_H_
#define _TIMELINE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>
#include "Servos/StsServoPoint.h"
#include "Servos/Pca9685PwmServoPoint.h"
#include "Mp3Player/Mp3PlayerYX5300Point.h"
#include "Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"
#include "TimelineStateReference.h"

using byte = unsigned char;

class Timeline
{

protected:
public:
    String name;                                                          /// the name of the timeline
    TimelineStateReference *state;                                        /// the state of the timeline
    int nextStateOnceId;                                                  /// if != -1: the next state set once after the timeline is finished
    std::vector<StsServoPoint> *stsServoPoints;                           /// the sts servo points of the timeline
    std::vector<StsServoPoint> *scsServoPoints;                           /// the scs servo points of the timeline
    std::vector<Pca9685PwmServoPoint> *pca9685PwmPoints;                  /// the Pca9685 Pwm servo points of the timeline
    std::vector<Mp3PlayerYX5300Point> *mp3PlayerYX5300Points;             /// the mp3 YX5300 sound points of the timeline
    std::vector<Mp3PlayerDfPlayerMiniPoint> *mp3PlayerDfPlayerMiniPoints; /// the mp3 DfPlayer Mini sound points of the timeline

    int durationMs; /// the duration of the timeline in milliseconds

public:
    Timeline(
        TimelineStateReference *state,
        int nextStateOnceId,
        String name,
        std::vector<StsServoPoint> *p_stsPoints,
        std::vector<StsServoPoint> *p_scsPoints, std::vector<Pca9685PwmServoPoint> *p_pca9685PwmPoints,
        std::vector<Mp3PlayerYX5300Point> *p_mp3PlayerYX5300Points,
        std::vector<Mp3PlayerDfPlayerMiniPoint> *p_mp3PlayerDfPlayerMiniPoints) : stsServoPoints(p_stsPoints),
                                                                                  scsServoPoints(p_scsPoints),
                                                                                  pca9685PwmPoints(p_pca9685PwmPoints),
                                                                                  mp3PlayerYX5300Points(p_mp3PlayerYX5300Points),
                                                                                  mp3PlayerDfPlayerMiniPoints(p_mp3PlayerDfPlayerMiniPoints),
                                                                                  name(name),
                                                                                  state(state),
                                                                                  nextStateOnceId(nextStateOnceId)
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

        // calculate highest duration for Mp3PlayerYX5300 points
        for (int i = 0; i < mp3PlayerYX5300Points->size(); i++)
        {
            // get the ms value of the point
            if (mp3PlayerYX5300Points->at(i).ms > durationMs)
                durationMs = mp3PlayerYX5300Points->at(i).ms;
        }

        // calculate duration for other points
        // todo
    }

    ~Timeline()
    {
    }
};

#endif