#ifndef _AUTOPLAYDATA_H_
#define _AUTOPLAYDATA_H_

#include <Arduino.h>
#include <String.h>
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"

/// <summary>
/// This is a prototype for the AutoPlayData class.
/// It is used as a template to inject the generated data export of the animatronic workbench studio app.
/// </summary>

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench

// Created on 16.08.2023 21:46:46

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Worm";   // Project Name
	const char *WlanSSID = "AWB-Worm";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password
	int stsServoCount = 4;
	int stsServoChannels[4] = {1, 2, 3, 4};
	int stsServoAccelleration[4] = {-1, -1, -1, -1};
	int stsServoSpeed[4] = {-1, -1, -1, -1};
	String stsServoName[4] = {"Mouth upper", "Mouth lower", "tilt left right", "rot left right"};
	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		stsServoPoints1->push_back(StsServoPoint(1,0,1997));
		stsServoPoints1->push_back(StsServoPoint(2,0,1500));
		stsServoPoints1->push_back(StsServoPoint(3,1000,1988));
		stsServoPoints1->push_back(StsServoPoint(4,1000,1740));
		stsServoPoints1->push_back(StsServoPoint(4,2000,2259));
		stsServoPoints1->push_back(StsServoPoint(4,3000,1897));
		stsServoPoints1->push_back(StsServoPoint(3,4000,1814));
		stsServoPoints1->push_back(StsServoPoint(3,5000,2082));
		stsServoPoints1->push_back(StsServoPoint(1,6000,1818));
		stsServoPoints1->push_back(StsServoPoint(2,6000,1237));
		stsServoPoints1->push_back(StsServoPoint(2,7000,1237));
		stsServoPoints1->push_back(StsServoPoint(1,7000,1940));
		stsServoPoints1->push_back(StsServoPoint(1,8000,1832));
		stsServoPoints1->push_back(StsServoPoint(2,8000,1299));
		stsServoPoints1->push_back(StsServoPoint(1,9000,2054));
		stsServoPoints1->push_back(StsServoPoint(2,9000,1200));
		stsServoPoints1->push_back(StsServoPoint(3,10000,1956));
		stsServoPoints1->push_back(StsServoPoint(2,10000,1294));
		stsServoPoints1->push_back(StsServoPoint(1,10000,1846));
		stsServoPoints1->push_back(StsServoPoint(3,11000,1996));
		stsServoPoints1->push_back(StsServoPoint(2,11000,1200));
		stsServoPoints1->push_back(StsServoPoint(1,11000,1969));
		stsServoPoints1->push_back(StsServoPoint(4,13000,1834));
		stsServoPoints1->push_back(StsServoPoint(3,13000,2051));
		stsServoPoints1->push_back(StsServoPoint(2,13000,1337));
		stsServoPoints1->push_back(StsServoPoint(1,13000,1855));
		auto state1 = new TimelineState(4, String("Talk"));
		Timeline *timeline1 = new Timeline(state1, String("Demo"), stsServoPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto state2 = new TimelineState(4, String("Talk"));
		Timeline *timeline2 = new Timeline(state2, String("Talking test"), stsServoPoints2);
		timelines->push_back(*timeline2);

    }

    ~AutoPlayData()
    {
    }
};

#endif
