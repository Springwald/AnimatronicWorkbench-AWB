#ifndef _AUTOPLAYDATA_H_
#define _AUTOPLAYDATA_H_

#include <Arduino.h>
#include <String.h>
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"
#include "Pca9685PwmServoPoint.h"
#include "Mp3PlayerYX5300Point.h"

/// <summary>
/// This is a prototype for the AutoPlayData class.
/// It is used as a template to inject the generated data export of the animatronic workbench studio app.
/// </summary>

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench

// Created on 14.03.2024 12:38:12

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 5;
	int stsServoChannels[5] = {6, 7, 8, 9, 11};
	int stsServoAcceleration[5] = {10, 10, 10, 100, 100};
	int stsServoSpeed[5] = {1500, 1500, 1500, 500, 500};
	String stsServoName[5] = {"Head rotate", "Neck right", "Neck left", "Arm right", "Arm left"};

	int scsServoCount = 7;
	int scsServoChannels[7] = {1, 2, 3, 4, 5, 10, 12};
	int scsServoAcceleration[7] = {1, 1, 1, 1, 1, 1, 1};
	int scsServoSpeed[7] = {500, 500, 300, 200, 200, 500, 500};
	String scsServoName[7] = {"Eyes up", "Eyes low", "Mouth", "Ear right", "Ear left", "Arm lower right", "Arm lower left"};

	int pca9685PwmServoCount = 0;
	int pca9685PwmServoI2cAdresses[0] = {};
	int pca9685PwmServoChannels[0] = {};
	int pca9685PwmServoAccelleration[0] = {};
	int pca9685PwmServoSpeed[0] = {};
	String pca9685PwmServoName[0] = {};

	int mp3PlayerYX5300Count = 1;
	int mp3PlayerYX5300RxPin[1] = {13};
	int mp3PlayerYX5300TxPin[1] = {14};
	String mp3PlayerYX5300Id[1] = {"Mp3Player"};

	int timelineStateIds[2] = {1, 2};
	String timelineStateNames[2] = {"InBag", "Standing"};
	int timelineStateCount = 2;
    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
		scsServoPoints1->push_back(StsServoPoint(3,875,572));
		scsServoPoints1->push_back(StsServoPoint(3,2000,471));
		scsServoPoints1->push_back(StsServoPoint(3,2625,572));
		mp3PlayerYX5300Points1->push_back(Mp3PlayerYX5300Point(11, 0, 1000));
		auto state1 = new TimelineState(1, String("InBag"));
		Timeline *timeline1 = new Timeline(state1, String("Speak Hmmm"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

    }

    ~AutoPlayData()
    {
    }
};

#endif
