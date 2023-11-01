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

// Created on 31.10.2023 18:51:16

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

	int pca9685PwmServoCount = 3;
	int pca9685PwmServoI2cAdresses[3] = {64, 64, 64};
	int pca9685PwmServoChannels[3] = {0, 1, 2};
	int pca9685PwmServoAccelleration[3] = {-1, -1, -1};
	int pca9685PwmServoSpeed[3] = {-1, -1, -1};
	String pca9685PwmServoName[3] = {"Mouth", "Eyes top", "Eyes bot"};

	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,0,1805));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,0,1610));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,0,1769));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,1000,2301));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,1000,759));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,1000,2495));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,0,2000,1805));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,1,2000,1610));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64,2,2000,1769));
		auto state1 = new TimelineState(1, String("sleep"));
		Timeline *timeline1 = new Timeline(state1, String("Test Face"), stsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
