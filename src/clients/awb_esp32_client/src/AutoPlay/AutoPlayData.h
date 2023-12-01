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

// Created on 01.12.2023 00:59:47

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 4;
	int stsServoChannels[4] = {1, 6, 7, 8};
	int stsServoAccelleration[4] = {-1, -1, -1, -1};
	int stsServoSpeed[4] = {-1, -1, -1, -1};
	String stsServoName[4] = {"Right hand", "Neck 1", "Neck 2", "Head rotate"};

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
		stsServoPoints1->push_back(StsServoPoint(7,0,2531));
		stsServoPoints1->push_back(StsServoPoint(8,0,2051));
		stsServoPoints1->push_back(StsServoPoint(6,0,1562));
		stsServoPoints1->push_back(StsServoPoint(6,1000,692));
		stsServoPoints1->push_back(StsServoPoint(7,1000,1322));
		stsServoPoints1->push_back(StsServoPoint(8,1000,1388));
		stsServoPoints1->push_back(StsServoPoint(6,2000,2740));
		stsServoPoints1->push_back(StsServoPoint(7,2000,1322));
		stsServoPoints1->push_back(StsServoPoint(8,2000,2035));
		stsServoPoints1->push_back(StsServoPoint(6,4000,692));
		stsServoPoints1->push_back(StsServoPoint(7,4000,3370));
		stsServoPoints1->push_back(StsServoPoint(8,4000,2035));
		stsServoPoints1->push_back(StsServoPoint(7,5500,1322));
		stsServoPoints1->push_back(StsServoPoint(8,5750,2731));
		stsServoPoints1->push_back(StsServoPoint(7,7000,2595));
		stsServoPoints1->push_back(StsServoPoint(6,7000,692));
		stsServoPoints1->push_back(StsServoPoint(8,7000,2034));
		stsServoPoints1->push_back(StsServoPoint(6,9000,1514));
		stsServoPoints1->push_back(StsServoPoint(7,9000,2595));
		stsServoPoints1->push_back(StsServoPoint(8,9000,2034));
		auto state1 = new TimelineState(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, String("Neck Test"), stsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
