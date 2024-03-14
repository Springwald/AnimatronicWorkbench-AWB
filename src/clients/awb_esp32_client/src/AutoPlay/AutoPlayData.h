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

// Created on 14.03.2024 01:08:59

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
		stsServoPoints1->push_back(StsServoPoint(6,0,2048));
		stsServoPoints1->push_back(StsServoPoint(7,0,2128));
		stsServoPoints1->push_back(StsServoPoint(8,0,2064));
		stsServoPoints1->push_back(StsServoPoint(9,0,1838));
		stsServoPoints1->push_back(StsServoPoint(11,0,2322));
		scsServoPoints1->push_back(StsServoPoint(1,0,580));
		scsServoPoints1->push_back(StsServoPoint(2,0,492));
		scsServoPoints1->push_back(StsServoPoint(3,0,572));
		scsServoPoints1->push_back(StsServoPoint(4,0,137));
		scsServoPoints1->push_back(StsServoPoint(5,0,830));
		scsServoPoints1->push_back(StsServoPoint(10,0,548));
		scsServoPoints1->push_back(StsServoPoint(12,0,346));
		scsServoPoints1->push_back(StsServoPoint(1,375,580));
		scsServoPoints1->push_back(StsServoPoint(2,375,492));
		scsServoPoints1->push_back(StsServoPoint(2,500,636));
		scsServoPoints1->push_back(StsServoPoint(1,500,431));
		scsServoPoints1->push_back(StsServoPoint(1,875,580));
		scsServoPoints1->push_back(StsServoPoint(2,875,492));
		scsServoPoints1->push_back(StsServoPoint(1,1625,580));
		scsServoPoints1->push_back(StsServoPoint(2,1625,492));
		auto state1 = new TimelineState(1, String("InBag"));
		Timeline *timeline1 = new Timeline(state1, String("Blink eyes"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		stsServoPoints2->push_back(StsServoPoint(6,0,2048));
		stsServoPoints2->push_back(StsServoPoint(7,0,2128));
		stsServoPoints2->push_back(StsServoPoint(8,0,2064));
		stsServoPoints2->push_back(StsServoPoint(9,0,1838));
		stsServoPoints2->push_back(StsServoPoint(11,0,2322));
		scsServoPoints2->push_back(StsServoPoint(1,0,512));
		scsServoPoints2->push_back(StsServoPoint(2,0,512));
		scsServoPoints2->push_back(StsServoPoint(3,0,572));
		scsServoPoints2->push_back(StsServoPoint(4,0,137));
		scsServoPoints2->push_back(StsServoPoint(5,0,830));
		scsServoPoints2->push_back(StsServoPoint(10,0,548));
		scsServoPoints2->push_back(StsServoPoint(12,0,346));
		scsServoPoints2->push_back(StsServoPoint(1,1000,546));
		scsServoPoints2->push_back(StsServoPoint(2,1000,443));
		scsServoPoints2->push_back(StsServoPoint(5,1000,306));
		scsServoPoints2->push_back(StsServoPoint(10,1000,596));
		scsServoPoints2->push_back(StsServoPoint(4,1000,612));
		stsServoPoints2->push_back(StsServoPoint(7,1000,3169));
		stsServoPoints2->push_back(StsServoPoint(8,1000,1200));
		stsServoPoints2->push_back(StsServoPoint(7,1500,3169));
		stsServoPoints2->push_back(StsServoPoint(8,1500,1200));
		scsServoPoints2->push_back(StsServoPoint(1,1500,546));
		scsServoPoints2->push_back(StsServoPoint(2,1500,443));
		stsServoPoints2->push_back(StsServoPoint(6,1500,2048));
		stsServoPoints2->push_back(StsServoPoint(7,1750,3169));
		stsServoPoints2->push_back(StsServoPoint(8,1750,1200));
		scsServoPoints2->push_back(StsServoPoint(4,2000,612));
		scsServoPoints2->push_back(StsServoPoint(5,2000,306));
		stsServoPoints2->push_back(StsServoPoint(6,2250,2112));
		stsServoPoints2->push_back(StsServoPoint(7,2250,1976));
		stsServoPoints2->push_back(StsServoPoint(8,2250,2212));
		scsServoPoints2->push_back(StsServoPoint(5,2625,854));
		scsServoPoints2->push_back(StsServoPoint(4,2625,274));
		scsServoPoints2->push_back(StsServoPoint(10,2625,443));
		scsServoPoints2->push_back(StsServoPoint(12,2625,354));
		stsServoPoints2->push_back(StsServoPoint(9,2625,1199));
		stsServoPoints2->push_back(StsServoPoint(11,2625,2914));
		scsServoPoints2->push_back(StsServoPoint(2,2625,603));
		scsServoPoints2->push_back(StsServoPoint(3,2625,435));
		scsServoPoints2->push_back(StsServoPoint(1,2625,412));
		stsServoPoints2->push_back(StsServoPoint(6,3375,1846));
		stsServoPoints2->push_back(StsServoPoint(7,3375,1792));
		stsServoPoints2->push_back(StsServoPoint(8,3375,2330));
		stsServoPoints2->push_back(StsServoPoint(9,3375,1199));
		scsServoPoints2->push_back(StsServoPoint(3,3375,572));
		scsServoPoints2->push_back(StsServoPoint(1,4000,412));
		scsServoPoints2->push_back(StsServoPoint(2,4000,603));
		scsServoPoints2->push_back(StsServoPoint(4,4250,274));
		scsServoPoints2->push_back(StsServoPoint(5,4250,854));
		scsServoPoints2->push_back(StsServoPoint(1,4625,580));
		scsServoPoints2->push_back(StsServoPoint(2,4625,443));
		scsServoPoints2->push_back(StsServoPoint(4,4875,588));
		scsServoPoints2->push_back(StsServoPoint(5,4875,306));
		scsServoPoints2->push_back(StsServoPoint(12,4875,645));
		scsServoPoints2->push_back(StsServoPoint(10,4875,427));
		scsServoPoints2->push_back(StsServoPoint(12,5500,290));
		scsServoPoints2->push_back(StsServoPoint(10,5500,693));
		scsServoPoints2->push_back(StsServoPoint(5,5500,862));
		scsServoPoints2->push_back(StsServoPoint(4,5500,217));
		stsServoPoints2->push_back(StsServoPoint(7,6000,2190));
		stsServoPoints2->push_back(StsServoPoint(6,6000,2036));
		stsServoPoints2->push_back(StsServoPoint(9,6000,1657));
		stsServoPoints2->push_back(StsServoPoint(11,6000,2404));
		scsServoPoints2->push_back(StsServoPoint(2,6000,478));
		scsServoPoints2->push_back(StsServoPoint(3,6000,570));
		auto state2 = new TimelineState(1, String("InBag"));
		Timeline *timeline2 = new Timeline(state2, String("Face Expression Tests"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2);
		timelines->push_back(*timeline2);

    }

    ~AutoPlayData()
    {
    }
};

#endif
