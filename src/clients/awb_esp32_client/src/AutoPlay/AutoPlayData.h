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

// Created on 15.03.2024 10:39:07

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
	int scsServoAcceleration[7] = {10, 10, 1, 1, 1, 1, 1};
	int scsServoSpeed[7] = {1000, 1000, 300, 200, 200, 500, 500};
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
	String mp3PlayerYX5300Name[1] = {"YX5300_1"};

	int timelineStateIds[2] = {1, 2};
	String timelineStateNames[2] = {"InBag", "Standing"};
	int timelineStatePositiveInput[2] = {1, 0};
	int timelineStateNegativeInput[2] =  {0, 1};
	int timelineStateCount = 2;

	int inputIds[1] = {1};
	String inputNames[1] = {"InBag"};
	uint8_t  inputIoPins[1] = {26};
	int inputCount = 1;

    std::vector<Timeline> *timelines;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
		scsServoPoints1->push_back(StsServoPoint(1,-3250,515));
		scsServoPoints1->push_back(StsServoPoint(2,-3250,514));
		scsServoPoints1->push_back(StsServoPoint(1,-2250,515));
		scsServoPoints1->push_back(StsServoPoint(2,-2250,514));
		scsServoPoints1->push_back(StsServoPoint(1,-2125,441));
		scsServoPoints1->push_back(StsServoPoint(2,-2125,613));
		scsServoPoints1->push_back(StsServoPoint(1,-2000,515));
		scsServoPoints1->push_back(StsServoPoint(2,-2000,514));
		scsServoPoints1->push_back(StsServoPoint(1,-1250,515));
		scsServoPoints1->push_back(StsServoPoint(2,-1250,514));
		stsServoPoints1->push_back(StsServoPoint(6,0,1945));
		stsServoPoints1->push_back(StsServoPoint(7,0,2113));
		stsServoPoints1->push_back(StsServoPoint(8,0,2015));
		scsServoPoints1->push_back(StsServoPoint(1,2000,515));
		scsServoPoints1->push_back(StsServoPoint(2,2000,514));
		scsServoPoints1->push_back(StsServoPoint(1,2250,376));
		scsServoPoints1->push_back(StsServoPoint(2,2250,636));
		scsServoPoints1->push_back(StsServoPoint(1,2625,512));
		scsServoPoints1->push_back(StsServoPoint(2,2625,510));
		scsServoPoints1->push_back(StsServoPoint(2,3000,510));
		scsServoPoints1->push_back(StsServoPoint(1,3000,512));
		scsServoPoints1->push_back(StsServoPoint(1,4625,512));
		scsServoPoints1->push_back(StsServoPoint(2,4625,510));
		stsServoPoints1->push_back(StsServoPoint(7,4625,2236));
		stsServoPoints1->push_back(StsServoPoint(6,4625,1945));
		stsServoPoints1->push_back(StsServoPoint(8,4625,2054));
		auto state1 = new TimelineState(1, String("InBag"));
		Timeline *timeline1 = new Timeline(state1, String("Blink 1"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1, mp3PlayerYX5300Points1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points2 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints2->push_back(StsServoPoint(6,0,2048));
		stsServoPoints2->push_back(StsServoPoint(7,0,2128));
		stsServoPoints2->push_back(StsServoPoint(8,0,2064));
		scsServoPoints2->push_back(StsServoPoint(4,0,0));
		scsServoPoints2->push_back(StsServoPoint(5,0,1024));
		stsServoPoints2->push_back(StsServoPoint(7,1000,1792));
		stsServoPoints2->push_back(StsServoPoint(8,1000,2396));
		stsServoPoints2->push_back(StsServoPoint(6,1000,2227));
		stsServoPoints2->push_back(StsServoPoint(7,1500,1792));
		stsServoPoints2->push_back(StsServoPoint(8,1500,2396));
		stsServoPoints2->push_back(StsServoPoint(6,1500,2120));
		stsServoPoints2->push_back(StsServoPoint(7,2000,1807));
		stsServoPoints2->push_back(StsServoPoint(8,2000,2330));
		scsServoPoints2->push_back(StsServoPoint(4,2000,887));
		scsServoPoints2->push_back(StsServoPoint(5,2000,193));
		scsServoPoints2->push_back(StsServoPoint(4,3000,177));
		stsServoPoints2->push_back(StsServoPoint(8,3000,2041));
		stsServoPoints2->push_back(StsServoPoint(6,3000,2044));
		stsServoPoints2->push_back(StsServoPoint(7,3000,2021));
		scsServoPoints2->push_back(StsServoPoint(5,3375,1024));
		auto state2 = new TimelineState(1, String("InBag"));
		Timeline *timeline2 = new Timeline(state2, String("Look Around 1"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2, mp3PlayerYX5300Points2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto *scsServoPoints3 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints3 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points3 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints3->push_back(StsServoPoint(6,0,1991));
		scsServoPoints3->push_back(StsServoPoint(12,0,282));
		scsServoPoints3->push_back(StsServoPoint(10,0,628));
		scsServoPoints3->push_back(StsServoPoint(4,0,137));
		scsServoPoints3->push_back(StsServoPoint(5,0,830));
		stsServoPoints3->push_back(StsServoPoint(8,0,1936));
		stsServoPoints3->push_back(StsServoPoint(7,0,2159));
		scsServoPoints3->push_back(StsServoPoint(10,1500,540));
		scsServoPoints3->push_back(StsServoPoint(5,1500,387));
		scsServoPoints3->push_back(StsServoPoint(4,1500,733));
		stsServoPoints3->push_back(StsServoPoint(6,2000,1854));
		scsServoPoints3->push_back(StsServoPoint(10,2250,628));
		scsServoPoints3->push_back(StsServoPoint(12,2250,282));
		stsServoPoints3->push_back(StsServoPoint(7,2750,2144));
		stsServoPoints3->push_back(StsServoPoint(8,3875,1765));
		scsServoPoints3->push_back(StsServoPoint(10,4000,645));
		scsServoPoints3->push_back(StsServoPoint(12,4000,330));
		scsServoPoints3->push_back(StsServoPoint(4,4625,733));
		scsServoPoints3->push_back(StsServoPoint(5,4625,387));
		stsServoPoints3->push_back(StsServoPoint(7,4625,1930));
		stsServoPoints3->push_back(StsServoPoint(8,4625,2107));
		stsServoPoints3->push_back(StsServoPoint(6,5000,1854));
		scsServoPoints3->push_back(StsServoPoint(4,6125,0));
		scsServoPoints3->push_back(StsServoPoint(5,6125,1024));
		scsServoPoints3->push_back(StsServoPoint(10,6500,645));
		scsServoPoints3->push_back(StsServoPoint(12,6500,322));
		stsServoPoints3->push_back(StsServoPoint(6,7000,2082));
		scsServoPoints3->push_back(StsServoPoint(10,7750,564));
		scsServoPoints3->push_back(StsServoPoint(12,7750,274));
		stsServoPoints3->push_back(StsServoPoint(8,7750,1936));
		stsServoPoints3->push_back(StsServoPoint(6,8625,2082));
		stsServoPoints3->push_back(StsServoPoint(6,10000,1998));
		auto state3 = new TimelineState(1, String("InBag"));
		Timeline *timeline3 = new Timeline(state3, String("Pause"), stsServoPoints3, scsServoPoints3, pca9685PwmServoPoints3, mp3PlayerYX5300Points3);
		timelines->push_back(*timeline3);

		auto *stsServoPoints4 = new std::vector<StsServoPoint>();
		auto *scsServoPoints4 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints4 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points4 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints4->push_back(StsServoPoint(6,0,1854));
		stsServoPoints4->push_back(StsServoPoint(7,0,2147));
		stsServoPoints4->push_back(StsServoPoint(8,0,1839));
		scsServoPoints4->push_back(StsServoPoint(4,0,0));
		scsServoPoints4->push_back(StsServoPoint(5,0,1024));
		scsServoPoints4->push_back(StsServoPoint(1,1375,512));
		scsServoPoints4->push_back(StsServoPoint(2,1375,512));
		scsServoPoints4->push_back(StsServoPoint(5,2000,1024));
		stsServoPoints4->push_back(StsServoPoint(7,2000,2006));
		stsServoPoints4->push_back(StsServoPoint(8,2000,2225));
		stsServoPoints4->push_back(StsServoPoint(6,2000,1922));
		scsServoPoints4->push_back(StsServoPoint(1,2250,580));
		scsServoPoints4->push_back(StsServoPoint(2,2250,443));
		scsServoPoints4->push_back(StsServoPoint(5,2750,322));
		scsServoPoints4->push_back(StsServoPoint(4,2750,709));
		stsServoPoints4->push_back(StsServoPoint(6,2750,1945));
		scsServoPoints4->push_back(StsServoPoint(3,2750,572));
		scsServoPoints4->push_back(StsServoPoint(3,3125,494));
		stsServoPoints4->push_back(StsServoPoint(6,3500,1960));
		stsServoPoints4->push_back(StsServoPoint(7,3500,1639));
		stsServoPoints4->push_back(StsServoPoint(8,3500,2646));
		scsServoPoints4->push_back(StsServoPoint(3,3625,572));
		stsServoPoints4->push_back(StsServoPoint(8,4625,1200));
		stsServoPoints4->push_back(StsServoPoint(7,4625,3200));
		stsServoPoints4->push_back(StsServoPoint(6,4625,1945));
		scsServoPoints4->push_back(StsServoPoint(1,4625,483));
		stsServoPoints4->push_back(StsServoPoint(11,4625,2392));
		scsServoPoints4->push_back(StsServoPoint(2,4625,507));
		stsServoPoints4->push_back(StsServoPoint(6,5250,1816));
		stsServoPoints4->push_back(StsServoPoint(6,6000,2067));
		stsServoPoints4->push_back(StsServoPoint(6,7000,1823));
		scsServoPoints4->push_back(StsServoPoint(1,7000,527));
		scsServoPoints4->push_back(StsServoPoint(2,7000,499));
		stsServoPoints4->push_back(StsServoPoint(6,7875,1831));
		mp3PlayerYX5300Points4->push_back(Mp3PlayerYX5300Point(6, 0, 3000));
		auto state4 = new TimelineState(1, String("InBag"));
		Timeline *timeline4 = new Timeline(state4, String("Sad"), stsServoPoints4, scsServoPoints4, pca9685PwmServoPoints4, mp3PlayerYX5300Points4);
		timelines->push_back(*timeline4);

		auto *stsServoPoints5 = new std::vector<StsServoPoint>();
		auto *scsServoPoints5 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints5 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points5 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints5->push_back(StsServoPoint(6,0,1998));
		stsServoPoints5->push_back(StsServoPoint(7,0,2343));
		stsServoPoints5->push_back(StsServoPoint(8,0,2186));
		scsServoPoints5->push_back(StsServoPoint(4,0,40));
		scsServoPoints5->push_back(StsServoPoint(5,0,830));
		scsServoPoints5->push_back(StsServoPoint(1,0,512));
		scsServoPoints5->push_back(StsServoPoint(2,0,512));
		scsServoPoints5->push_back(StsServoPoint(3,875,572));
		scsServoPoints5->push_back(StsServoPoint(3,1500,435));
		stsServoPoints5->push_back(StsServoPoint(6,1500,1960));
		stsServoPoints5->push_back(StsServoPoint(7,1500,2159));
		scsServoPoints5->push_back(StsServoPoint(1,1500,580));
		scsServoPoints5->push_back(StsServoPoint(2,1500,443));
		scsServoPoints5->push_back(StsServoPoint(3,2125,572));
		scsServoPoints5->push_back(StsServoPoint(5,2375,395));
		scsServoPoints5->push_back(StsServoPoint(4,2375,507));
		scsServoPoints5->push_back(StsServoPoint(1,3000,515));
		scsServoPoints5->push_back(StsServoPoint(2,3000,514));
		stsServoPoints5->push_back(StsServoPoint(7,3625,2557));
		stsServoPoints5->push_back(StsServoPoint(8,3625,2422));
		stsServoPoints5->push_back(StsServoPoint(6,3625,2021));
		scsServoPoints5->push_back(StsServoPoint(4,3875,0));
		scsServoPoints5->push_back(StsServoPoint(5,3875,854));
		stsServoPoints5->push_back(StsServoPoint(7,3875,2557));
		stsServoPoints5->push_back(StsServoPoint(8,3875,2422));
		stsServoPoints5->push_back(StsServoPoint(6,4750,2029));
		stsServoPoints5->push_back(StsServoPoint(7,4750,2373));
		stsServoPoints5->push_back(StsServoPoint(8,4750,2054));
		mp3PlayerYX5300Points5->push_back(Mp3PlayerYX5300Point(8, 0, 1000));
		auto state5 = new TimelineState(1, String("InBag"));
		Timeline *timeline5 = new Timeline(state5, String("Speak Hmmm"), stsServoPoints5, scsServoPoints5, pca9685PwmServoPoints5, mp3PlayerYX5300Points5);
		timelines->push_back(*timeline5);

    }

    ~AutoPlayData()
    {
    }
};

#endif
