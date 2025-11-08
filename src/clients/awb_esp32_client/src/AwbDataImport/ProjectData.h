
#ifndef _PROJECTDATA_H_
#define _PROJECTDATA_H_

#include <Arduino.h>
#include <String.h>
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineStateReference.h"

#include "../ProjectData/Servos/ServoPoint.h"
#include "../ProjectData/Servos/Servo.h"
#include <ProjectData/Servos/Servos.h>

#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AWB/AnimatronicWorkbench

// Created on 08.11.2025 12:12:22

class ProjectData
{

    using TCallBackErrorOccured = std::function<void(String)>;

public:
    const char *ProjectName = "AWB-Demo-Board";
    const int returnToAutoModeAfterMinutes = -1;

    /* Names as const to prevent magic strings in custom code: */

    const String TimelineName_ = "";
    const String ScsServoName_Servoleft = "Servo left";
    const String ScsServoName_Servoright = "Servo right";

    Servos *servos;
    std::vector<TimelineState> *timelineStates;
    std::vector<Timeline> *timelines;
    std::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;
    std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;

    int inputIds[0] = {};
    String inputNames[0] = {};
    uint8_t inputIoPins[0] = {};
    int inputCount = 0;

    ProjectData(TCallBackErrorOccured errorOccured)
    {
        // the servos
        servos = new Servos();
        std::vector<RelaxRange> *servo_000_relaxRanges = new std::vector<RelaxRange>();
        servos->addServo(Servo("ScsServo-1", new ServoConfig(ServoConfig::ServoTypes::SCS_SERVO, "Servo left", 1, 0, 821, 230, -1, -1, 532, 0, 200, false, servo_000_relaxRanges)));

        std::vector<RelaxRange> *servo_001_relaxRanges = new std::vector<RelaxRange>();
        servos->addServo(Servo("ScsServo-2", new ServoConfig(ServoConfig::ServoTypes::SCS_SERVO, "Servo right", 2, 0, 236, 822, -1, -1, 537, 0, 0, false, servo_001_relaxRanges)));

        // sound player
        mp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();

        mp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();

        // timelines states
        timelineStates = new std::vector<TimelineState>();
        timelineStates->push_back(TimelineState(1, String("Default"), true, new std::vector<int>({}), new std::vector<int>({})));

        addTimelines();
    }

    // timelines
    void addTimelines()
    {
        timelines = new std::vector<Timeline>();
        auto *servoPoints1 = new std::vector<ServoPoint>();
        auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
        auto *mp3PlayerDfPlayerMiniPoints1 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
        servoPoints1->push_back(ServoPoint(ServoPoint("ScsServo-1", 0, 230)));
        servoPoints1->push_back(ServoPoint(ServoPoint("ScsServo-1", 2000, 821)));
        servoPoints1->push_back(ServoPoint(ServoPoint("ScsServo-1", 4000, 230)));
        auto state1 = new TimelineStateReference(1, String("Default"));
        Timeline *timeline1 = new Timeline(state1, -1, String(""), servoPoints1, mp3PlayerYX5300Points1, mp3PlayerDfPlayerMiniPoints1);
        timelines->push_back(*timeline1);
    }
};

#endif // _PROJECTDATA_H_
