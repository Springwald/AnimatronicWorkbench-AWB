#ifndef _AUTOPLAYDATA_H_
#define _AUTOPLAYDATA_H_

#include <Arduino.h>
#include <String.h>
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"
#include "Pca9685PwmServoPoint.h"

/// <summary>
/// This is a prototype for the AutoPlayData class.
/// It is used as a template to inject the generated data export of the animatronic workbench studio app.
/// </summary>

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench

// Created on 20.11.2023 23:41:51

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 3;
	int stsServoChannels[3] = {6, 7, 8};
	int stsServoAccelleration[3] = {-1, -1, -1};
	int stsServoSpeed[3] = {-1, -1, -1};
	String stsServoName[3] = {"Neck 1", "Neck 2", "Head rotate"};

	int pca9685PwmServoCount = 4;
	int pca9685PwmServoI2cAdresses[4] = {64, 64, 64, 64};
	int pca9685PwmServoChannels[4] = {0, 1, 2, 3};
	int pca9685PwmServoAccelleration[4] = {-1, -1, -1, -1};
	int pca9685PwmServoSpeed[4] = {-1, -1, -1, -1};
	String pca9685PwmServoName[4] = {"Mouth", "Eyes top", "Eyes bot", "Ear right"};

	int timelineStateIds[3] = {1, 2, 3};
	int timelineStateCount = 3;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		stsServoPoints1->push_back(StsServoPoint(6,0,2000));
		stsServoPoints1->push_back(StsServoPoint(7,0,2050));
		stsServoPoints1->push_back(StsServoPoint(8,0,2050));
		stsServoPoints1->push_back(StsServoPoint(6,1000,2087));
		stsServoPoints1->push_back(StsServoPoint(6,2000,2000));
		auto state1 = new TimelineState(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, String("Neck Test"), stsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,0,0,1432));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,1,0,1663));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,2,0,1167));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,0,1000,2300));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,0,2000,1438));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,1,4000,1663));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,2,4000,1167));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,1,4250,821));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,2,4250,1716));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,0,4500,1438));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,1,4750,1655));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,2,4750,1167));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,2,6000,1167));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,1,6000,1663));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,6000,1507));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,7000,1230));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,8000,1230));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,9000,1790));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,10000,1790));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,11000,1507));
		pca9685PwmServoPoints2->push_back(Pca9685PwmServoPoint(64,3,12000,1507));
		auto state2 = new TimelineState(1, String("idle"));
		Timeline *timeline2 = new Timeline(state2, String("Test Face"), stsServoPoints2, pca9685PwmServoPoints2);
		timelines->push_back(*timeline2);

    }

    ~AutoPlayData()
    {
    }
};

#endif
