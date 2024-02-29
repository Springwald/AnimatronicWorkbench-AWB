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

// Created on 29.02.2024 23:02:41

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 5;
	int stsServoChannels[5] = {6, 7, 8, 9, 11};
	int stsServoAcceleration[5] = {10, 10, 10, 10, 10};
	int stsServoSpeed[5] = {1500, 1500, 1500, 1500, 1500};
	String stsServoName[5] = {"Head rotate", "Neck right", "Neck left", "Arm left", "Arm left"};

	int scsServoCount = 7;
	int scsServoChannels[7] = {1, 2, 3, 4, 4, 10, 12};
	int scsServoAcceleration[7] = {10, 10, 10, 10, 10, 10, 10};
	int scsServoSpeed[7] = {1500, 1500, 1500, 1500, 1500, 1500, 1500};
	String scsServoName[7] = {"Eyes up", "Eyes low", "Mouth", "Ear right", "Ear right", "Arm lower right", "Arm lower left"};

	int pca9685PwmServoCount = 0;
	int pca9685PwmServoI2cAdresses[0] = {};
	int pca9685PwmServoChannels[0] = {};
	int pca9685PwmServoAccelleration[0] = {};
	int pca9685PwmServoSpeed[0] = {};
	String pca9685PwmServoName[0] = {};

	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;
	std::vector<Timeline> *timelines;

	AutoPlayData()
	{
		timelines = new std::vector<Timeline>();
	}

	~AutoPlayData()
	{
	}
};

#endif
