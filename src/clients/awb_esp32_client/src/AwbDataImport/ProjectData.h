#ifndef _PROJECTDATA_H_
#define _PROJECTDATA_H_

#include <Arduino.h>
#include <String.h>
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineStateReference.h"
#include "../ProjectData/StsServoPoint.h"
#include "../ProjectData/Pca9685PwmServoPoint.h"
#include "../ProjectData/Mp3PlayerYX5300Point.h"
#include "../ProjectData/StsScsServo.h"
#include "../ProjectData/Pca9685PwmServo.h"
#include "../ProjectData/Mp3PlayerYX5300Serial.h"

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench-EN

// Created on 22.07.2024 23:30:58

class ProjectData
{

public:
   const char *ProjectName = "PIP-Animatronic";

	std::vector<StsScsServo> *scsServos;
	std::vector<StsScsServo> *stsServos;
	std::vector<Pca9685PwmServo> *pca9685PwmServos;
	std::vector<TimelineState>* timelineStates;
	std::vector<Timeline>* timelines;
	std::vector<Mp3PlayerYX5300Serial> *mp3Players;

	int inputIds[0] = {};
	String inputNames[0] = {};
	uint8_t  inputIoPins[0] = {};
	int inputCount = 0;


ProjectData()
{

   scsServos = new std::vector<StsScsServo>();
   scsServos->push_back(StsScsServo(1, "NeckRotateLeftRight", 0, 1023, 55, 400, 511, 0, 100, false ));
   scsServos->push_back(StsScsServo(2, "NeckInOut", 0, 1023, 55, 400, 511, 0, 100, true ));
   scsServos->push_back(StsScsServo(3, "HeadUpDown", 0, 0, 55, 400, 0, 0, 0, true ));

   stsServos = new std::vector<StsScsServo>();

   pca9685PwmServos = new std::vector<Pca9685PwmServo>();

	mp3Players = new std::vector<Mp3PlayerYX5300Serial>();
	mp3Players->push_back(Mp3PlayerYX5300Serial(13, 14, "PipVoiceSound"));

	timelineStates = new std::vector<TimelineState>();
	timelineStates->push_back(TimelineState(1, String("idle"), true, new std::vector<int>({  }), new std::vector<int>({  })));

	addTimelines();

}

void addTimelines() {
	timelines = new std::vector<Timeline>();


}

};

#endif // _PROJECTDATA_H_

