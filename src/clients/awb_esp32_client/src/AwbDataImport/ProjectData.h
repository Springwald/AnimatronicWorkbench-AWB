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

// Created on 10.08.2024 13:53:46

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

   stsServos = new std::vector<StsScsServo>();
   stsServos->push_back(StsScsServo(1, "HeadTiltLeftRight", 2600, 1600, 55, 400, 2150, 25, 1200, false ));
   stsServos->push_back(StsScsServo(2, "NeckInOut", 2550, 1650, 55, 400, 1750, 10, 800, true ));
   stsServos->push_back(StsScsServo(3, "HeadTiltUpDown", 2480, 1800, 55, 400, 2140, 25, 1200, true ));
   stsServos->push_back(StsScsServo(4, "Ear", 500, 3800, 55, 400, 2048, 100, 1500, false ));

   pca9685PwmServos = new std::vector<Pca9685PwmServo>();

	mp3Players = new std::vector<Mp3PlayerYX5300Serial>();
	mp3Players->push_back(Mp3PlayerYX5300Serial(17, 16, "PipVoiceSound"));

	timelineStates = new std::vector<TimelineState>();
	timelineStates->push_back(TimelineState(1, String("idle"), true, new std::vector<int>({  }), new std::vector<int>({  })));
	timelineStates->push_back(TimelineState(2, String("sleeping"), false, new std::vector<int>({  }), new std::vector<int>({  })));
	timelineStates->push_back(TimelineState(4, String("for custom code only"), false, new std::vector<int>({  }), new std::vector<int>({  })));

	addTimelines();

}

void addTimelines() {
	timelines = new std::vector<Timeline>();

		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints1->push_back(StsServoPoint(2,0,1785));
		stsServoPoints1->push_back(StsServoPoint(3,0,2110));
		stsServoPoints1->push_back(StsServoPoint(4,0,2042));
		stsServoPoints1->push_back(StsServoPoint(2,1000,2550));
		stsServoPoints1->push_back(StsServoPoint(3,1000,2110));
		stsServoPoints1->push_back(StsServoPoint(4,1000,2042));
		stsServoPoints1->push_back(StsServoPoint(4,2500,500));
		stsServoPoints1->push_back(StsServoPoint(4,4500,500));
		mp3PlayerYX5300Points1->push_back(Mp3PlayerYX5300Point(18, 0, 500));
		auto state1 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, nullptr, String("Go Sleep"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1, mp3PlayerYX5300Points1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points2 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints2->push_back(StsServoPoint(1,-125,2150));
		stsServoPoints2->push_back(StsServoPoint(3,-125,2140));
		stsServoPoints2->push_back(StsServoPoint(2,0,1650));
		stsServoPoints2->push_back(StsServoPoint(1,0,2150));
		stsServoPoints2->push_back(StsServoPoint(3,0,2140));
		stsServoPoints2->push_back(StsServoPoint(4,0,2042));
		stsServoPoints2->push_back(StsServoPoint(3,500,2140));
		stsServoPoints2->push_back(StsServoPoint(3,1000,2480));
		stsServoPoints2->push_back(StsServoPoint(4,1000,655));
		stsServoPoints2->push_back(StsServoPoint(3,1250,2480));
		stsServoPoints2->push_back(StsServoPoint(3,1750,1960));
		stsServoPoints2->push_back(StsServoPoint(1,1750,2150));
		stsServoPoints2->push_back(StsServoPoint(4,2000,2162));
		stsServoPoints2->push_back(StsServoPoint(1,2250,1788));
		stsServoPoints2->push_back(StsServoPoint(1,2500,1788));
		stsServoPoints2->push_back(StsServoPoint(1,2875,2426));
		stsServoPoints2->push_back(StsServoPoint(3,2875,1960));
		stsServoPoints2->push_back(StsServoPoint(4,2875,2656));
		stsServoPoints2->push_back(StsServoPoint(3,3500,2140));
		stsServoPoints2->push_back(StsServoPoint(1,3500,2426));
		stsServoPoints2->push_back(StsServoPoint(4,3750,2042));
		stsServoPoints2->push_back(StsServoPoint(1,4000,2150));
		stsServoPoints2->push_back(StsServoPoint(1,4500,2150));
		mp3PlayerYX5300Points2->push_back(Mp3PlayerYX5300Point(12, 0, 750));
		auto state2 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline2 = new Timeline(state2, nullptr, String("Wake up"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2, mp3PlayerYX5300Points2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto *scsServoPoints3 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints3 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points3 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints3->push_back(StsServoPoint(1,0,2150));
		stsServoPoints3->push_back(StsServoPoint(2,0,2550));
		stsServoPoints3->push_back(StsServoPoint(3,0,2140));
		stsServoPoints3->push_back(StsServoPoint(4,0,500));
		stsServoPoints3->push_back(StsServoPoint(4,4000,500));
		stsServoPoints3->push_back(StsServoPoint(4,10000,500));
		auto state3 = new TimelineStateReference(2, String("sleeping"));
		Timeline *timeline3 = new Timeline(state3, nullptr, String("Sleeping"), stsServoPoints3, scsServoPoints3, pca9685PwmServoPoints3, mp3PlayerYX5300Points3);
		timelines->push_back(*timeline3);


}

};

#endif // _PROJECTDATA_H_

