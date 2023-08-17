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

// Created on 17.08.2023 19:58:43

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
		stsServoPoints1->push_back(StsServoPoint(1,0,2000));
		stsServoPoints1->push_back(StsServoPoint(2,0,1500));
		stsServoPoints1->push_back(StsServoPoint(3,0,2000));
		stsServoPoints1->push_back(StsServoPoint(4,0,2000));
		stsServoPoints1->push_back(StsServoPoint(2,500,1393));
		stsServoPoints1->push_back(StsServoPoint(1,500,2073));
		stsServoPoints1->push_back(StsServoPoint(3,1000,1940));
		stsServoPoints1->push_back(StsServoPoint(2,1000,1507));
		stsServoPoints1->push_back(StsServoPoint(1,1000,1907));
		stsServoPoints1->push_back(StsServoPoint(2,1500,1417));
		stsServoPoints1->push_back(StsServoPoint(1,1500,2073));
		stsServoPoints1->push_back(StsServoPoint(3,1500,1996));
		stsServoPoints1->push_back(StsServoPoint(4,1750,1976));
		stsServoPoints1->push_back(StsServoPoint(3,2000,1956));
		stsServoPoints1->push_back(StsServoPoint(2,2000,1511));
		stsServoPoints1->push_back(StsServoPoint(1,2000,1931));
		stsServoPoints1->push_back(StsServoPoint(2,2500,1370));
		stsServoPoints1->push_back(StsServoPoint(1,2500,2054));
		stsServoPoints1->push_back(StsServoPoint(2,2750,1440));
		stsServoPoints1->push_back(StsServoPoint(1,2750,1974));
		stsServoPoints1->push_back(StsServoPoint(3,3000,1854));
		stsServoPoints1->push_back(StsServoPoint(2,3000,1360));
		stsServoPoints1->push_back(StsServoPoint(1,3000,2068));
		stsServoPoints1->push_back(StsServoPoint(4,3000,2275));
		stsServoPoints1->push_back(StsServoPoint(2,3500,1469));
		stsServoPoints1->push_back(StsServoPoint(1,3500,1983));
		stsServoPoints1->push_back(StsServoPoint(4,3750,2007));
		stsServoPoints1->push_back(StsServoPoint(3,4000,2066));
		stsServoPoints1->push_back(StsServoPoint(2,4000,1379));
		stsServoPoints1->push_back(StsServoPoint(1,4000,2073));
		stsServoPoints1->push_back(StsServoPoint(2,4500,1459));
		stsServoPoints1->push_back(StsServoPoint(1,4500,1974));
		stsServoPoints1->push_back(StsServoPoint(3,5000,2003));
		stsServoPoints1->push_back(StsServoPoint(2,5000,1388));
		stsServoPoints1->push_back(StsServoPoint(1,5000,2035));
		auto state1 = new TimelineState(1, String("Sleep"));
		Timeline *timeline1 = new Timeline(state1, String("no title"), stsServoPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		stsServoPoints2->push_back(StsServoPoint(1,0,2000));
		stsServoPoints2->push_back(StsServoPoint(2,0,1500));
		stsServoPoints2->push_back(StsServoPoint(3,0,2000));
		stsServoPoints2->push_back(StsServoPoint(4,0,2000));
		stsServoPoints2->push_back(StsServoPoint(3,750,2248));
		stsServoPoints2->push_back(StsServoPoint(3,1500,1744));
		stsServoPoints2->push_back(StsServoPoint(4,2000,2000));
		stsServoPoints2->push_back(StsServoPoint(3,3000,1996));
		stsServoPoints2->push_back(StsServoPoint(4,3000,1519));
		stsServoPoints2->push_back(StsServoPoint(4,4000,2496));
		stsServoPoints2->push_back(StsServoPoint(4,4500,2007));
		auto state2 = new TimelineState(2, String("Action"));
		Timeline *timeline2 = new Timeline(state2, String("Demo"), stsServoPoints2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto state3 = new TimelineState(4, String("Talk"));
		Timeline *timeline3 = new Timeline(state3, String("Talking"), stsServoPoints3);
		timelines->push_back(*timeline3);

    }

    ~AutoPlayData()
    {
    }
};

#endif
