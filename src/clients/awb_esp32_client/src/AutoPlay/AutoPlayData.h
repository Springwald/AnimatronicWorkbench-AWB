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

// Created on 19.01.2024 16:23:27

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
		scsServoPoints1->push_back(StsServoPoint(1,0,478));
		scsServoPoints1->push_back(StsServoPoint(2,0,528));
		scsServoPoints1->push_back(StsServoPoint(3,0,617));
		scsServoPoints1->push_back(StsServoPoint(3,1000,617));
		scsServoPoints1->push_back(StsServoPoint(1,1000,568));
		scsServoPoints1->push_back(StsServoPoint(2,1000,478));
		scsServoPoints1->push_back(StsServoPoint(1,2000,568));
		scsServoPoints1->push_back(StsServoPoint(2,2000,478));
		scsServoPoints1->push_back(StsServoPoint(3,2000,617));
		scsServoPoints1->push_back(StsServoPoint(1,3000,484));
		scsServoPoints1->push_back(StsServoPoint(2,3000,525));
		scsServoPoints1->push_back(StsServoPoint(1,4000,484));
		scsServoPoints1->push_back(StsServoPoint(2,4000,525));
		scsServoPoints1->push_back(StsServoPoint(3,4000,617));
		scsServoPoints1->push_back(StsServoPoint(2,5000,671));
		scsServoPoints1->push_back(StsServoPoint(1,5000,235));
		scsServoPoints1->push_back(StsServoPoint(1,6000,235));
		scsServoPoints1->push_back(StsServoPoint(2,6000,671));
		scsServoPoints1->push_back(StsServoPoint(3,6000,615));
		scsServoPoints1->push_back(StsServoPoint(2,7000,523));
		scsServoPoints1->push_back(StsServoPoint(1,7000,484));
		scsServoPoints1->push_back(StsServoPoint(3,7000,617));
		scsServoPoints1->push_back(StsServoPoint(1,8000,484));
		scsServoPoints1->push_back(StsServoPoint(2,8000,523));
		scsServoPoints1->push_back(StsServoPoint(3,8000,617));
		scsServoPoints1->push_back(StsServoPoint(3,9000,340));
		scsServoPoints1->push_back(StsServoPoint(2,9000,626));
		scsServoPoints1->push_back(StsServoPoint(1,9000,562));
		scsServoPoints1->push_back(StsServoPoint(1,10000,562));
		scsServoPoints1->push_back(StsServoPoint(2,10000,626));
		scsServoPoints1->push_back(StsServoPoint(3,10000,340));
		scsServoPoints1->push_back(StsServoPoint(2,11000,523));
		scsServoPoints1->push_back(StsServoPoint(1,11000,484));
		scsServoPoints1->push_back(StsServoPoint(3,11000,617));
		scsServoPoints1->push_back(StsServoPoint(1,12000,484));
		scsServoPoints1->push_back(StsServoPoint(2,12000,523));
		scsServoPoints1->push_back(StsServoPoint(3,12000,617));
		scsServoPoints1->push_back(StsServoPoint(1,12250,329));
		scsServoPoints1->push_back(StsServoPoint(2,12250,611));
		scsServoPoints1->push_back(StsServoPoint(1,12500,484));
		scsServoPoints1->push_back(StsServoPoint(2,12500,521));
		scsServoPoints1->push_back(StsServoPoint(3,12500,617));
		scsServoPoints1->push_back(StsServoPoint(1,13000,484));
		scsServoPoints1->push_back(StsServoPoint(2,13000,521));
		scsServoPoints1->push_back(StsServoPoint(3,13000,617));
		auto state1 = new TimelineState(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, String("Face Test 1"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
