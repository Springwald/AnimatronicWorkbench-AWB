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

// Created on 29.10.2023 17:47:54

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace";	 // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace"; // WLAN SSID Name
	const char *WlanPassword = "awb12345";			 // WLAN Password

	int stsServoCount = 0;
	int stsServoChannels[0] = {};
	int stsServoAccelleration[0] = {};
	int stsServoSpeed[0] = {};
	String stsServoName[0] = {};

	int pca9685PwmServoCount = 1;
	int pca9685PwmServoI2cAdresses[1] = {64};
	int pca9685PwmServoChannels[1] = {0};
	int pca9685PwmServoAccelleration[1] = {-1};
	int pca9685PwmServoSpeed[1] = {-1};
	String pca9685PwmServoName[1] = {"Demo PWM Servo 0"};

	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;
	std::vector<Timeline> *timelines;

	AutoPlayData()
	{
		timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64, 0, 0, 1503));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64, 0, 991, 2035));
		pca9685PwmServoPoints1->push_back(Pca9685PwmServoPoint(64, 0, 2000, 1486));
		auto state1 = new TimelineState(1, String("sleep"));
		Timeline *timeline1 = new Timeline(state1, String("Test Mouth"), stsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);
	}

	~AutoPlayData()
	{
	}
};

#endif
