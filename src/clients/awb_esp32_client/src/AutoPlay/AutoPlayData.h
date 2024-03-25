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

// Created on 25.03.2024 11:41:56

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2.0 TestFace";	 // Project Name
	const char *WlanSSID = "AWB-Grogu 2.0 TestFace"; // WLAN SSID Name
	const char *WlanPassword = "awb12345";			 // WLAN Password

	int stsServoCount = 4;
	int stsServoChannels[4] = {2, 6, 7, 8};
	int stsServoAcceleration[4] = {500, -1, -1, -1};
	int stsServoSpeed[4] = {4000, -1, -1, -1};
	String stsServoName[4] = {"Right hand", "Neck 1", "Neck 2", "Head rotate"};

	int scsServoCount = 0;
	int scsServoChannels[0] = {};
	int scsServoAcceleration[0] = {};
	int scsServoSpeed[0] = {};
	String scsServoName[0] = {};

	int pca9685PwmServoCount = 4;
	int pca9685PwmServoI2cAdresses[4] = {64, 64, 64, 64};
	int pca9685PwmServoChannels[4] = {0, 1, 2, 3};
	String pca9685PwmServoName[4] = {"Mouth", "Eyes top", "Eyes bot", "Ear right"};

	int mp3PlayerYX5300Count = 0;
	int mp3PlayerYX5300RxPin[0] = {};
	int mp3PlayerYX5300TxPin[0] = {};
	String mp3PlayerYX5300Name[0] = {};

	int timelineStateIds[3] = {1, 2, 3};
	String timelineStateNames[3] = {"idle", "action", "look around"};
	int timelineStatePositiveInput[3] = {0, 0, 0};
	int timelineStateNegativeInput[3] = {0, 0, 0};
	int timelineStateCount = 3;

	int inputIds[0] = {};
	String inputNames[0] = {};
	uint8_t inputIoPins[0] = {};
	int inputCount = 0;

	std::vector<Timeline> *timelines;

	AutoPlayData()
	{
		timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints1->push_back(StsServoPoint(6, 0, 1700));
		stsServoPoints1->push_back(StsServoPoint(7, 0, 2393));
		stsServoPoints1->push_back(StsServoPoint(8, 0, 2047));
		stsServoPoints1->push_back(StsServoPoint(2, 0, 2000));
		stsServoPoints1->push_back(StsServoPoint(6, 1000, 1836));
		stsServoPoints1->push_back(StsServoPoint(7, 1000, 2434));
		stsServoPoints1->push_back(StsServoPoint(8, 1000, 1954));
		stsServoPoints1->push_back(StsServoPoint(2, 1000, 1600));
		stsServoPoints1->push_back(StsServoPoint(2, 1250, 2500));
		stsServoPoints1->push_back(StsServoPoint(7, 2000, 1434));
		stsServoPoints1->push_back(StsServoPoint(8, 2000, 2423));
		stsServoPoints1->push_back(StsServoPoint(6, 2000, 2530));
		stsServoPoints1->push_back(StsServoPoint(2, 2000, 2500));
		stsServoPoints1->push_back(StsServoPoint(7, 3000, 2257));
		stsServoPoints1->push_back(StsServoPoint(8, 3000, 2084));
		stsServoPoints1->push_back(StsServoPoint(6, 3000, 1933));
		stsServoPoints1->push_back(StsServoPoint(2, 3000, 1996));
		auto state1 = new TimelineState(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, String("Demo"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1, mp3PlayerYX5300Points1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points2 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints2->push_back(StsServoPoint(7, 0, 2531));
		stsServoPoints2->push_back(StsServoPoint(8, 0, 2051));
		stsServoPoints2->push_back(StsServoPoint(6, 0, 1562));
		stsServoPoints2->push_back(StsServoPoint(6, 1000, 692));
		stsServoPoints2->push_back(StsServoPoint(7, 1000, 1322));
		stsServoPoints2->push_back(StsServoPoint(8, 1000, 1388));
		stsServoPoints2->push_back(StsServoPoint(6, 2000, 2740));
		stsServoPoints2->push_back(StsServoPoint(7, 2000, 1322));
		stsServoPoints2->push_back(StsServoPoint(8, 2000, 2035));
		stsServoPoints2->push_back(StsServoPoint(6, 4000, 692));
		stsServoPoints2->push_back(StsServoPoint(7, 4000, 3370));
		stsServoPoints2->push_back(StsServoPoint(8, 4000, 2035));
		stsServoPoints2->push_back(StsServoPoint(7, 5500, 1322));
		stsServoPoints2->push_back(StsServoPoint(8, 5750, 2731));
		stsServoPoints2->push_back(StsServoPoint(7, 7000, 2595));
		stsServoPoints2->push_back(StsServoPoint(6, 7000, 692));
		stsServoPoints2->push_back(StsServoPoint(8, 7000, 2034));
		stsServoPoints2->push_back(StsServoPoint(6, 9000, 1514));
		stsServoPoints2->push_back(StsServoPoint(7, 9000, 2595));
		stsServoPoints2->push_back(StsServoPoint(8, 9000, 2034));
		auto state2 = new TimelineState(1, String("idle"));
		Timeline *timeline2 = new Timeline(state2, String("Neck Test"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2, mp3PlayerYX5300Points2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto *scsServoPoints3 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints3 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points3 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints3->push_back(StsServoPoint(2, -1750, 1600));
		stsServoPoints3->push_back(StsServoPoint(2, -750, 2500));
		stsServoPoints3->push_back(StsServoPoint(2, -250, 2500));
		stsServoPoints3->push_back(StsServoPoint(6, 0, 1700));
		stsServoPoints3->push_back(StsServoPoint(7, 0, 2393));
		stsServoPoints3->push_back(StsServoPoint(8, 0, 2047));
		stsServoPoints3->push_back(StsServoPoint(2, 0, 1607));
		stsServoPoints3->push_back(StsServoPoint(2, 1250, 2500));
		stsServoPoints3->push_back(StsServoPoint(2, 1750, 2500));
		stsServoPoints3->push_back(StsServoPoint(2, 2750, 2046));
		stsServoPoints3->push_back(StsServoPoint(2, 3000, 2500));
		stsServoPoints3->push_back(StsServoPoint(2, 3500, 2500));
		stsServoPoints3->push_back(StsServoPoint(2, 5000, 2053));
		stsServoPoints3->push_back(StsServoPoint(2, 5750, 2053));
		stsServoPoints3->push_back(StsServoPoint(2, 6500, 1600));
		auto state3 = new TimelineState(1, String("idle"));
		Timeline *timeline3 = new Timeline(state3, String("Push 2"), stsServoPoints3, scsServoPoints3, pca9685PwmServoPoints3, mp3PlayerYX5300Points3);
		timelines->push_back(*timeline3);

		auto *stsServoPoints4 = new std::vector<StsServoPoint>();
		auto *scsServoPoints4 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints4 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points4 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints4->push_back(StsServoPoint(6, 0, 1700));
		stsServoPoints4->push_back(StsServoPoint(7, 0, 2393));
		stsServoPoints4->push_back(StsServoPoint(8, 0, 2047));
		stsServoPoints4->push_back(StsServoPoint(2, 0, 2046));
		stsServoPoints4->push_back(StsServoPoint(2, 1750, 1600));
		stsServoPoints4->push_back(StsServoPoint(2, 1787, 2051));
		stsServoPoints4->push_back(StsServoPoint(2, 2000, 2500));
		stsServoPoints4->push_back(StsServoPoint(2, 3000, 2500));
		stsServoPoints4->push_back(StsServoPoint(2, 4000, 2053));
		auto state4 = new TimelineState(1, String("idle"));
		Timeline *timeline4 = new Timeline(state4, String("Push Button"), stsServoPoints4, scsServoPoints4, pca9685PwmServoPoints4, mp3PlayerYX5300Points4);
		timelines->push_back(*timeline4);
	}

	~AutoPlayData()
	{
	}
};

#endif
