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

// Created on 17.11.2023 00:01:04

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 0;
	int stsServoChannels[0] = {};
	int stsServoAccelleration[0] = {};
	int stsServoSpeed[0] = {};
	String stsServoName[0] = {};

	int pca9685PwmServoCount = 4;
	int pca9685PwmServoI2cAdresses[4] = {64, 64, 64, 64};
	int pca9685PwmServoChannels[4] = {0, 1, 2, 3};
	int pca9685PwmServoAccelleration[4] = {-1, -1, -1, -1};
	int pca9685PwmServoSpeed[4] = {-1, -1, -1, -1};
	String pca9685PwmServoName[4] = {"Mouth", "Eyes top", "Eyes bot", "Ear right"};

	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,0,1432));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,0,1663));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,0,1167));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,1000,2300));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,2000,1438));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,4000,1663));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,4000,1167));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,4250,821));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,4250,1716));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,4500,1438));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,4750,1655));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,4750,1167));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,6000,1167));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,6000,1663));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,6000,1507));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,7000,1230));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,8000,1230));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,9000,1790));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,10000,1790));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,11000,1507));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,3,12000,1507));
		auto state1 = new TimelineState(1, String("sleep"));
		Timeline *timeline1 = new Timeline(state1, String("Test Face"), stsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
