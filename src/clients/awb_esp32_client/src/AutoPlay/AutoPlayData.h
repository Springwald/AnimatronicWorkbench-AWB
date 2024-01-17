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

// Created on 17.01.2024 16:01:06

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace 2";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace 2";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 0;
	int stsServoChannels[0] = {};
	int stsServoAcceleration[0] = {};
	int stsServoSpeed[0] = {};
	String stsServoName[0] = {};

	int scsServoCount = 3;
	int scsServoChannels[3] = {1, 2, 3};
	int scsServoAcceleration[3] = {-1, -1, -1};
	int scsServoSpeed[3] = {-1, -1, -1};
	String scsServoName[3] = {"Top Eyes", "Bottom Eyes", "Mouth"};

	int pca9685PwmServoCount = 0;
	int pca9685PwmServoI2cAdresses[0] = {};
	int pca9685PwmServoChannels[0] = {};
	int pca9685PwmServoAccelleration[0] = {};
	int pca9685PwmServoSpeed[0] = {};
	String pca9685PwmServoName[0] = {};

	int timelineStateIds[3] = {1, 2, 3};
	int timelineStateCount = 3;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		scsServoPoints1->push_back(StsServoPoint(1,0,2048));
		scsServoPoints1->push_back(StsServoPoint(2,0,2048));
		scsServoPoints1->push_back(StsServoPoint(3,0,2048));
		auto state1 = new TimelineState(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, String("Eyes Test"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
