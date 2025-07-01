#ifndef _PROJECTDATA_H_
#define _PROJECTDATA_H_

#include <Arduino.h>
#include <String.h>
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineStateReference.h"

#include "../ProjectData/Servos/StsServoPoint.h"
#include "../ProjectData/Servos/Pca9685PwmServoPoint.h"
#include "../ProjectData/Servos/StsScsServo.h"
#include "../ProjectData/Servos/Pca9685PwmServo.h"

#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench-EN

// Created on 25.06.2025 22:32:32

class ProjectData
{

using TCallBackErrorOccured = std::function<void(String)>;

public:
   const char *ProjectName = "AWB-Demo-Board";

   const int returnToAutoModeAfterMinutes  = -1 ;

   /* Names as const to prevent magic strings in custom code: */

   const String ScsServoName_Servoleft ="Servo left";
   const String ScsServoName_Servoright ="Servo right";


	std::vector<StsScsServo> *scsServos;
	std::vector<StsScsServo> *stsServos;
	std::vector<Pca9685PwmServo> *pca9685PwmServos;
	std::vector<TimelineState>* timelineStates;
	std::vector<Timeline>* timelines;
	std::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;
	std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;

	int inputIds[0] = {};
	String inputNames[0] = {};
	uint8_t  inputIoPins[0] = {};
	int inputCount = 0;


ProjectData(TCallBackErrorOccured errorOccured)
{

   scsServos = new std::vector<StsScsServo>();
   scsServos->push_back(StsScsServo(1, "Servo left", 0, 512, 55, 400, 256, 0, 200, false ));
   scsServos->push_back(StsScsServo(2, "Servo right", 0, 0, 55, 400, 0, 0, 0, false ));

   stsServos = new std::vector<StsScsServo>();

   pca9685PwmServos = new std::vector<Pca9685PwmServo>();

	mp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();

	mp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();

	timelineStates = new std::vector<TimelineState>();
	timelineStates->push_back(TimelineState(1, String("Default"), true, new std::vector<int>({  }), new std::vector<int>({  })));

	addTimelines();

}

void addTimelines() {
	timelines = new std::vector<Timeline>();


}

};

#endif // _PROJECTDATA_H_

