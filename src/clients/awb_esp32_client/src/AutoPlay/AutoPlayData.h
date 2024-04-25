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

// Created on 25.04.2024 19:37:32

class AutoPlayData
{

protected:
public:
	const char *ProjectName = "Grogu 2";   // Project Name
	const char *WlanSSID = "AWB-Grogu 2";  // WLAN SSID Name
	const char *WlanPassword = "awb12345"; // WLAN Password

	int stsServoCount = 5;
	int stsServoChannels[5] = {6, 7, 8, 9, 11};
	int stsServoAcceleration[5] = {10, 10, 10, 20, 20};
	int stsServoSpeed[5] = {1500, 1500, 1500, 1000, 1000};
	String stsServoName[5] = {"Head rotate", "Neck right", "Neck left", "Arm right", "Arm left"};

	int scsServoCount = 7;
	int scsServoChannels[7] = {1, 2, 3, 4, 5, 10, 12};
	int scsServoAcceleration[7] = {10, 10, 1, 1, 1, 1, 1};
	int scsServoSpeed[7] = {1000, 1000, 300, 200, 200, 500, 500};
	String scsServoName[7] = {"Eyes up", "Eyes low", "Mouth", "Ear right", "Ear left", "Arm lower right", "Arm lower left"};

	int pca9685PwmServoCount = 0;
	int pca9685PwmServoI2cAdresses[0] = {};
	int pca9685PwmServoChannels[0] = {};
	String pca9685PwmServoName[0] = {};

	int mp3PlayerYX5300Count = 1;
	int mp3PlayerYX5300RxPin[1] = {13};
	int mp3PlayerYX5300TxPin[1] = {14};
	String mp3PlayerYX5300Name[1] = {"YX5300_1"};

	int timelineStateIds[3] = {1, 2, 5};
	String timelineStateNames[3] = {"InBag", "Standing", "Only Remote"};
	int timelineStatePositiveInput[3] = {1, 0, 0};
	int timelineStateNegativeInput[3] =  {0, 1, 0};
	int timelineStateCount = 3;

	int inputIds[1] = {1};
	String inputNames[1] = {""};
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
		stsServoPoints1->push_back(StsServoPoint(11,0,3366));
		stsServoPoints1->push_back(StsServoPoint(9,0,744));
		stsServoPoints1->push_back(StsServoPoint(7,0,2113));
		stsServoPoints1->push_back(StsServoPoint(8,0,2015));
		stsServoPoints1->push_back(StsServoPoint(6,0,2219));
		scsServoPoints1->push_back(StsServoPoint(10,0,850));
		scsServoPoints1->push_back(StsServoPoint(12,0,86));
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
		stsServoPoints1->push_back(StsServoPoint(7,4625,1945));
		stsServoPoints1->push_back(StsServoPoint(8,4625,2199));
		auto state1 = new TimelineState(1, String("InBag"));
		Timeline *timeline1 = new Timeline(state1, String("InBag - Blink 1"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1, mp3PlayerYX5300Points1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points2 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints2->push_back(StsServoPoint(11,0,3366));
		stsServoPoints2->push_back(StsServoPoint(9,0,744));
		scsServoPoints2->push_back(StsServoPoint(12,0,124));
		scsServoPoints2->push_back(StsServoPoint(10,0,856));
		stsServoPoints2->push_back(StsServoPoint(6,0,2326));
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
		stsServoPoints2->push_back(StsServoPoint(6,3000,2409));
		stsServoPoints2->push_back(StsServoPoint(7,3000,2021));
		scsServoPoints2->push_back(StsServoPoint(5,3375,1024));
		stsServoPoints2->push_back(StsServoPoint(8,5000,2041));
		stsServoPoints2->push_back(StsServoPoint(7,5000,1761));
		stsServoPoints2->push_back(StsServoPoint(8,7000,2344));
		stsServoPoints2->push_back(StsServoPoint(6,7000,2044));
		stsServoPoints2->push_back(StsServoPoint(6,9000,2333));
		stsServoPoints2->push_back(StsServoPoint(8,9000,2028));
		stsServoPoints2->push_back(StsServoPoint(7,9000,2190));
		auto state2 = new TimelineState(1, String("InBag"));
		Timeline *timeline2 = new Timeline(state2, String("InBag - Look Around 1"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2, mp3PlayerYX5300Points2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto *scsServoPoints3 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints3 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points3 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints3->push_back(StsServoPoint(11,0,3366));
		stsServoPoints3->push_back(StsServoPoint(9,0,744));
		scsServoPoints3->push_back(StsServoPoint(12,0,86));
		scsServoPoints3->push_back(StsServoPoint(10,0,839));
		stsServoPoints3->push_back(StsServoPoint(6,0,2326));
		scsServoPoints3->push_back(StsServoPoint(4,0,137));
		scsServoPoints3->push_back(StsServoPoint(5,0,830));
		stsServoPoints3->push_back(StsServoPoint(8,0,1936));
		stsServoPoints3->push_back(StsServoPoint(7,0,2159));
		scsServoPoints3->push_back(StsServoPoint(10,1500,804));
		scsServoPoints3->push_back(StsServoPoint(5,1500,387));
		scsServoPoints3->push_back(StsServoPoint(4,1500,733));
		scsServoPoints3->push_back(StsServoPoint(12,1500,167));
		stsServoPoints3->push_back(StsServoPoint(6,2000,1854));
		scsServoPoints3->push_back(StsServoPoint(10,2250,873));
		scsServoPoints3->push_back(StsServoPoint(12,2250,53));
		stsServoPoints3->push_back(StsServoPoint(7,2750,2144));
		stsServoPoints3->push_back(StsServoPoint(8,3875,1765));
		scsServoPoints3->push_back(StsServoPoint(10,4000,804));
		scsServoPoints3->push_back(StsServoPoint(12,4000,118));
		scsServoPoints3->push_back(StsServoPoint(4,4625,733));
		scsServoPoints3->push_back(StsServoPoint(5,4625,387));
		stsServoPoints3->push_back(StsServoPoint(7,4625,1930));
		stsServoPoints3->push_back(StsServoPoint(8,4625,2107));
		stsServoPoints3->push_back(StsServoPoint(6,5000,1854));
		scsServoPoints3->push_back(StsServoPoint(4,6125,0));
		scsServoPoints3->push_back(StsServoPoint(5,6125,1024));
		scsServoPoints3->push_back(StsServoPoint(10,6500,810));
		scsServoPoints3->push_back(StsServoPoint(12,6500,188));
		stsServoPoints3->push_back(StsServoPoint(6,7000,2082));
		scsServoPoints3->push_back(StsServoPoint(10,7750,694));
		scsServoPoints3->push_back(StsServoPoint(12,7750,296));
		stsServoPoints3->push_back(StsServoPoint(8,7750,1936));
		stsServoPoints3->push_back(StsServoPoint(6,8625,2082));
		stsServoPoints3->push_back(StsServoPoint(6,10000,1998));
		stsServoPoints3->push_back(StsServoPoint(6,12250,2387));
		stsServoPoints3->push_back(StsServoPoint(8,12250,1949));
		stsServoPoints3->push_back(StsServoPoint(7,14000,1929));
		stsServoPoints3->push_back(StsServoPoint(8,14000,1949));
		stsServoPoints3->push_back(StsServoPoint(7,15000,2343));
		stsServoPoints3->push_back(StsServoPoint(8,15000,2317));
		stsServoPoints3->push_back(StsServoPoint(6,15000,2135));
		stsServoPoints3->push_back(StsServoPoint(7,17000,1731));
		stsServoPoints3->push_back(StsServoPoint(6,17000,2310));
		stsServoPoints3->push_back(StsServoPoint(8,17000,1791));
		stsServoPoints3->push_back(StsServoPoint(8,17750,1988));
		stsServoPoints3->push_back(StsServoPoint(6,17750,2364));
		stsServoPoints3->push_back(StsServoPoint(7,17750,2052));
		auto state3 = new TimelineState(1, String("InBag"));
		Timeline *timeline3 = new Timeline(state3, String("InBag - Pause"), stsServoPoints3, scsServoPoints3, pca9685PwmServoPoints3, mp3PlayerYX5300Points3);
		timelines->push_back(*timeline3);

		auto *stsServoPoints4 = new std::vector<StsServoPoint>();
		auto *scsServoPoints4 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints4 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points4 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints4->push_back(StsServoPoint(11,0,3366));
		stsServoPoints4->push_back(StsServoPoint(9,0,744));
		stsServoPoints4->push_back(StsServoPoint(6,0,2326));
		stsServoPoints4->push_back(StsServoPoint(7,0,2147));
		stsServoPoints4->push_back(StsServoPoint(8,0,1839));
		scsServoPoints4->push_back(StsServoPoint(4,0,0));
		scsServoPoints4->push_back(StsServoPoint(5,0,1024));
		scsServoPoints4->push_back(StsServoPoint(10,0,833));
		scsServoPoints4->push_back(StsServoPoint(12,0,113));
		scsServoPoints4->push_back(StsServoPoint(1,1375,512));
		scsServoPoints4->push_back(StsServoPoint(2,1375,512));
		scsServoPoints4->push_back(StsServoPoint(5,2000,1024));
		stsServoPoints4->push_back(StsServoPoint(7,2000,2006));
		stsServoPoints4->push_back(StsServoPoint(8,2000,2225));
		stsServoPoints4->push_back(StsServoPoint(6,2000,2295));
		scsServoPoints4->push_back(StsServoPoint(1,2250,580));
		scsServoPoints4->push_back(StsServoPoint(2,2250,443));
		scsServoPoints4->push_back(StsServoPoint(5,2750,322));
		scsServoPoints4->push_back(StsServoPoint(4,2750,709));
		stsServoPoints4->push_back(StsServoPoint(6,2750,2303));
		scsServoPoints4->push_back(StsServoPoint(3,2750,572));
		scsServoPoints4->push_back(StsServoPoint(3,3125,494));
		stsServoPoints4->push_back(StsServoPoint(6,3500,2265));
		stsServoPoints4->push_back(StsServoPoint(7,3500,1639));
		stsServoPoints4->push_back(StsServoPoint(8,3500,2646));
		scsServoPoints4->push_back(StsServoPoint(3,3625,572));
		stsServoPoints4->push_back(StsServoPoint(8,4625,1200));
		stsServoPoints4->push_back(StsServoPoint(7,4625,3200));
		stsServoPoints4->push_back(StsServoPoint(6,4625,2112));
		scsServoPoints4->push_back(StsServoPoint(1,4625,483));
		scsServoPoints4->push_back(StsServoPoint(2,4625,507));
		stsServoPoints4->push_back(StsServoPoint(6,5250,1892));
		stsServoPoints4->push_back(StsServoPoint(6,6000,2166));
		stsServoPoints4->push_back(StsServoPoint(6,7000,1899));
		scsServoPoints4->push_back(StsServoPoint(1,7000,527));
		scsServoPoints4->push_back(StsServoPoint(2,7000,499));
		stsServoPoints4->push_back(StsServoPoint(6,7875,2303));
		stsServoPoints4->push_back(StsServoPoint(7,9000,3200));
		stsServoPoints4->push_back(StsServoPoint(8,9000,1200));
		stsServoPoints4->push_back(StsServoPoint(6,9000,2303));
		stsServoPoints4->push_back(StsServoPoint(7,10000,1807));
		stsServoPoints4->push_back(StsServoPoint(8,10000,2357));
		scsServoPoints4->push_back(StsServoPoint(1,10375,527));
		scsServoPoints4->push_back(StsServoPoint(2,10375,499));
		scsServoPoints4->push_back(StsServoPoint(1,10625,322));
		scsServoPoints4->push_back(StsServoPoint(2,10625,633));
		scsServoPoints4->push_back(StsServoPoint(1,11000,527));
		scsServoPoints4->push_back(StsServoPoint(2,11000,499));
		stsServoPoints4->push_back(StsServoPoint(6,11125,2303));
		stsServoPoints4->push_back(StsServoPoint(6,12000,2052));
		mp3PlayerYX5300Points4->push_back(Mp3PlayerYX5300Point(6, 0, 3000));
		auto state4 = new TimelineState(1, String("InBag"));
		Timeline *timeline4 = new Timeline(state4, String("InBag - Sad"), stsServoPoints4, scsServoPoints4, pca9685PwmServoPoints4, mp3PlayerYX5300Points4);
		timelines->push_back(*timeline4);

		auto *stsServoPoints5 = new std::vector<StsServoPoint>();
		auto *scsServoPoints5 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints5 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points5 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints5->push_back(StsServoPoint(11,0,3366));
		stsServoPoints5->push_back(StsServoPoint(9,0,744));
		stsServoPoints5->push_back(StsServoPoint(6,0,2326));
		stsServoPoints5->push_back(StsServoPoint(7,0,2343));
		stsServoPoints5->push_back(StsServoPoint(8,0,2186));
		scsServoPoints5->push_back(StsServoPoint(4,0,40));
		scsServoPoints5->push_back(StsServoPoint(5,0,830));
		scsServoPoints5->push_back(StsServoPoint(1,0,512));
		scsServoPoints5->push_back(StsServoPoint(2,0,512));
		scsServoPoints5->push_back(StsServoPoint(10,0,798));
		scsServoPoints5->push_back(StsServoPoint(12,0,107));
		scsServoPoints5->push_back(StsServoPoint(3,875,572));
		scsServoPoints5->push_back(StsServoPoint(3,1500,435));
		stsServoPoints5->push_back(StsServoPoint(6,1500,2090));
		stsServoPoints5->push_back(StsServoPoint(7,1500,2159));
		scsServoPoints5->push_back(StsServoPoint(1,1500,580));
		scsServoPoints5->push_back(StsServoPoint(2,1500,443));
		scsServoPoints5->push_back(StsServoPoint(3,2125,572));
		scsServoPoints5->push_back(StsServoPoint(5,2375,395));
		scsServoPoints5->push_back(StsServoPoint(4,2375,507));
		stsServoPoints5->push_back(StsServoPoint(6,2375,2333));
		scsServoPoints5->push_back(StsServoPoint(1,3000,515));
		scsServoPoints5->push_back(StsServoPoint(2,3000,514));
		stsServoPoints5->push_back(StsServoPoint(7,3625,2557));
		stsServoPoints5->push_back(StsServoPoint(8,3625,2422));
		stsServoPoints5->push_back(StsServoPoint(6,3625,2128));
		scsServoPoints5->push_back(StsServoPoint(4,3875,0));
		scsServoPoints5->push_back(StsServoPoint(5,3875,854));
		stsServoPoints5->push_back(StsServoPoint(7,3875,2557));
		stsServoPoints5->push_back(StsServoPoint(8,3875,2422));
		stsServoPoints5->push_back(StsServoPoint(6,4750,2333));
		stsServoPoints5->push_back(StsServoPoint(7,4750,2373));
		stsServoPoints5->push_back(StsServoPoint(8,4750,2054));
		stsServoPoints5->push_back(StsServoPoint(7,6000,2373));
		stsServoPoints5->push_back(StsServoPoint(8,7000,2054));
		stsServoPoints5->push_back(StsServoPoint(7,7000,1976));
		stsServoPoints5->push_back(StsServoPoint(6,8000,2303));
		stsServoPoints5->push_back(StsServoPoint(8,8000,2278));
		stsServoPoints5->push_back(StsServoPoint(6,9000,1983));
		stsServoPoints5->push_back(StsServoPoint(6,10375,2333));
		stsServoPoints5->push_back(StsServoPoint(6,11750,2333));
		mp3PlayerYX5300Points5->push_back(Mp3PlayerYX5300Point(8, 0, 1000));
		auto state5 = new TimelineState(1, String("InBag"));
		Timeline *timeline5 = new Timeline(state5, String("InBag - Speak Hmmm"), stsServoPoints5, scsServoPoints5, pca9685PwmServoPoints5, mp3PlayerYX5300Points5);
		timelines->push_back(*timeline5);

		auto *stsServoPoints6 = new std::vector<StsServoPoint>();
		auto *scsServoPoints6 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints6 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points6 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints6->push_back(StsServoPoint(11,0,3366));
		stsServoPoints6->push_back(StsServoPoint(9,0,744));
		scsServoPoints6->push_back(StsServoPoint(12,0,107));
		scsServoPoints6->push_back(StsServoPoint(10,0,862));
		stsServoPoints6->push_back(StsServoPoint(6,0,2326));
		stsServoPoints6->push_back(StsServoPoint(7,0,2128));
		stsServoPoints6->push_back(StsServoPoint(8,0,2064));
		stsServoPoints6->push_back(StsServoPoint(6,1000,2219));
		stsServoPoints6->push_back(StsServoPoint(11,1000,3366));
		stsServoPoints6->push_back(StsServoPoint(11,2000,3712));
		scsServoPoints6->push_back(StsServoPoint(12,2375,512));
		stsServoPoints6->push_back(StsServoPoint(11,3000,3625));
		stsServoPoints6->push_back(StsServoPoint(6,3000,2303));
		stsServoPoints6->push_back(StsServoPoint(8,3000,2436));
		stsServoPoints6->push_back(StsServoPoint(7,3000,1670));
		scsServoPoints6->push_back(StsServoPoint(10,3000,867));
		scsServoPoints6->push_back(StsServoPoint(12,3000,415));
		scsServoPoints6->push_back(StsServoPoint(12,3375,550));
		stsServoPoints6->push_back(StsServoPoint(11,4000,3712));
		scsServoPoints6->push_back(StsServoPoint(12,4375,480));
		stsServoPoints6->push_back(StsServoPoint(11,5000,3614));
		scsServoPoints6->push_back(StsServoPoint(12,5500,469));
		stsServoPoints6->push_back(StsServoPoint(8,5500,1975));
		stsServoPoints6->push_back(StsServoPoint(7,5500,1868));
		stsServoPoints6->push_back(StsServoPoint(11,7000,3366));
		scsServoPoints6->push_back(StsServoPoint(12,7000,507));
		stsServoPoints6->push_back(StsServoPoint(7,7000,2205));
		stsServoPoints6->push_back(StsServoPoint(11,8000,3366));
		scsServoPoints6->push_back(StsServoPoint(12,8375,399));
		auto state6 = new TimelineState(1, String("InBag"));
		Timeline *timeline6 = new Timeline(state6, String("InBag - Wink"), stsServoPoints6, scsServoPoints6, pca9685PwmServoPoints6, mp3PlayerYX5300Points6);
		timelines->push_back(*timeline6);

		auto *stsServoPoints7 = new std::vector<StsServoPoint>();
		auto *scsServoPoints7 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints7 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points7 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints7->push_back(StsServoPoint(9,0,1741));
		stsServoPoints7->push_back(StsServoPoint(11,0,2342));
		scsServoPoints7->push_back(StsServoPoint(10,0,682));
		scsServoPoints7->push_back(StsServoPoint(12,0,453));
		stsServoPoints7->push_back(StsServoPoint(6,0,2326));
		stsServoPoints7->push_back(StsServoPoint(7,0,2113));
		stsServoPoints7->push_back(StsServoPoint(8,0,2015));
		scsServoPoints7->push_back(StsServoPoint(1,2000,515));
		scsServoPoints7->push_back(StsServoPoint(2,2000,514));
		scsServoPoints7->push_back(StsServoPoint(1,2250,376));
		scsServoPoints7->push_back(StsServoPoint(2,2250,636));
		scsServoPoints7->push_back(StsServoPoint(1,2625,512));
		scsServoPoints7->push_back(StsServoPoint(2,2625,510));
		scsServoPoints7->push_back(StsServoPoint(2,3000,510));
		scsServoPoints7->push_back(StsServoPoint(1,3000,512));
		scsServoPoints7->push_back(StsServoPoint(1,4000,508));
		scsServoPoints7->push_back(StsServoPoint(2,4000,514));
		scsServoPoints7->push_back(StsServoPoint(1,4250,360));
		scsServoPoints7->push_back(StsServoPoint(2,4250,626));
		stsServoPoints7->push_back(StsServoPoint(7,4625,1945));
		stsServoPoints7->push_back(StsServoPoint(8,4625,2199));
		scsServoPoints7->push_back(StsServoPoint(2,4625,512));
		scsServoPoints7->push_back(StsServoPoint(1,4625,512));
		stsServoPoints7->push_back(StsServoPoint(7,6000,2190));
		stsServoPoints7->push_back(StsServoPoint(8,6000,2304));
		stsServoPoints7->push_back(StsServoPoint(7,8500,2190));
		stsServoPoints7->push_back(StsServoPoint(8,8500,2304));
		stsServoPoints7->push_back(StsServoPoint(6,9125,2326));
		stsServoPoints7->push_back(StsServoPoint(7,9500,1899));
		stsServoPoints7->push_back(StsServoPoint(8,9500,2067));
		stsServoPoints7->push_back(StsServoPoint(6,10750,2135));
		stsServoPoints7->push_back(StsServoPoint(7,11500,1899));
		stsServoPoints7->push_back(StsServoPoint(8,11500,2067));
		auto state7 = new TimelineState(2, String("Standing"));
		Timeline *timeline7 = new Timeline(state7, String("Stand - Blink 1"), stsServoPoints7, scsServoPoints7, pca9685PwmServoPoints7, mp3PlayerYX5300Points7);
		timelines->push_back(*timeline7);

		auto *stsServoPoints8 = new std::vector<StsServoPoint>();
		auto *scsServoPoints8 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints8 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points8 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints8->push_back(StsServoPoint(9,0,1741));
		stsServoPoints8->push_back(StsServoPoint(11,0,2342));
		stsServoPoints8->push_back(StsServoPoint(6,0,2242));
		stsServoPoints8->push_back(StsServoPoint(7,0,2266));
		stsServoPoints8->push_back(StsServoPoint(8,0,2067));
		scsServoPoints8->push_back(StsServoPoint(4,0,0));
		scsServoPoints8->push_back(StsServoPoint(5,0,854));
		scsServoPoints8->push_back(StsServoPoint(10,0,659));
		scsServoPoints8->push_back(StsServoPoint(12,0,469));
		stsServoPoints8->push_back(StsServoPoint(6,1000,1922));
		stsServoPoints8->push_back(StsServoPoint(11,1000,2719));
		stsServoPoints8->push_back(StsServoPoint(6,1375,1922));
		stsServoPoints8->push_back(StsServoPoint(8,1375,2370));
		stsServoPoints8->push_back(StsServoPoint(7,1375,2878));
		stsServoPoints8->push_back(StsServoPoint(11,1375,2719));
		stsServoPoints8->push_back(StsServoPoint(9,1375,1741));
		scsServoPoints8->push_back(StsServoPoint(4,2250,854));
		scsServoPoints8->push_back(StsServoPoint(5,2250,169));
		scsServoPoints8->push_back(StsServoPoint(10,2875,567));
		scsServoPoints8->push_back(StsServoPoint(12,2875,296));
		stsServoPoints8->push_back(StsServoPoint(11,3000,2342));
		stsServoPoints8->push_back(StsServoPoint(9,3000,1360));
		stsServoPoints8->push_back(StsServoPoint(6,3000,2173));
		stsServoPoints8->push_back(StsServoPoint(7,3000,1868));
		stsServoPoints8->push_back(StsServoPoint(8,3000,1633));
		stsServoPoints8->push_back(StsServoPoint(9,3375,1360));
		stsServoPoints8->push_back(StsServoPoint(11,3375,2342));
		stsServoPoints8->push_back(StsServoPoint(7,3375,1868));
		stsServoPoints8->push_back(StsServoPoint(8,3375,1633));
		stsServoPoints8->push_back(StsServoPoint(6,3375,2173));
		stsServoPoints8->push_back(StsServoPoint(8,4625,2344));
		stsServoPoints8->push_back(StsServoPoint(9,4625,1741));
		stsServoPoints8->push_back(StsServoPoint(6,4625,1899));
		stsServoPoints8->push_back(StsServoPoint(7,4625,2939));
		stsServoPoints8->push_back(StsServoPoint(11,4625,2719));
		stsServoPoints8->push_back(StsServoPoint(7,5000,2939));
		stsServoPoints8->push_back(StsServoPoint(8,5000,2344));
		stsServoPoints8->push_back(StsServoPoint(6,5000,1899));
		stsServoPoints8->push_back(StsServoPoint(9,5000,1741));
		stsServoPoints8->push_back(StsServoPoint(11,5000,2719));
		stsServoPoints8->push_back(StsServoPoint(6,6125,2181));
		stsServoPoints8->push_back(StsServoPoint(7,6125,1884));
		stsServoPoints8->push_back(StsServoPoint(8,6125,1633));
		stsServoPoints8->push_back(StsServoPoint(9,6125,1382));
		stsServoPoints8->push_back(StsServoPoint(11,6125,2352));
		scsServoPoints8->push_back(StsServoPoint(4,6500,0));
		scsServoPoints8->push_back(StsServoPoint(5,6500,1024));
		scsServoPoints8->push_back(StsServoPoint(10,6625,694));
		scsServoPoints8->push_back(StsServoPoint(12,6625,447));
		stsServoPoints8->push_back(StsServoPoint(6,7125,2196));
		stsServoPoints8->push_back(StsServoPoint(7,7125,2404));
		stsServoPoints8->push_back(StsServoPoint(8,7125,2015));
		stsServoPoints8->push_back(StsServoPoint(9,7125,1741));
		stsServoPoints8->push_back(StsServoPoint(11,7125,2342));
		stsServoPoints8->push_back(StsServoPoint(6,9000,2356));
		stsServoPoints8->push_back(StsServoPoint(7,9000,2404));
		stsServoPoints8->push_back(StsServoPoint(8,9000,2015));
		stsServoPoints8->push_back(StsServoPoint(9,9000,1741));
		stsServoPoints8->push_back(StsServoPoint(11,9000,2342));
		stsServoPoints8->push_back(StsServoPoint(6,10000,2409));
		stsServoPoints8->push_back(StsServoPoint(7,10000,1868));
		auto state8 = new TimelineState(2, String("Standing"));
		Timeline *timeline8 = new Timeline(state8, String("Stand - Dance"), stsServoPoints8, scsServoPoints8, pca9685PwmServoPoints8, mp3PlayerYX5300Points8);
		timelines->push_back(*timeline8);

		auto *stsServoPoints9 = new std::vector<StsServoPoint>();
		auto *scsServoPoints9 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints9 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points9 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints9->push_back(StsServoPoint(9,0,1741));
		stsServoPoints9->push_back(StsServoPoint(11,0,2342));
		scsServoPoints9->push_back(StsServoPoint(10,0,682));
		scsServoPoints9->push_back(StsServoPoint(12,0,453));
		stsServoPoints9->push_back(StsServoPoint(6,0,2326));
		stsServoPoints9->push_back(StsServoPoint(7,0,2128));
		stsServoPoints9->push_back(StsServoPoint(8,0,2064));
		scsServoPoints9->push_back(StsServoPoint(4,0,0));
		scsServoPoints9->push_back(StsServoPoint(5,0,1024));
		stsServoPoints9->push_back(StsServoPoint(7,1000,1792));
		stsServoPoints9->push_back(StsServoPoint(8,1000,2396));
		stsServoPoints9->push_back(StsServoPoint(6,1000,2227));
		stsServoPoints9->push_back(StsServoPoint(7,1500,1792));
		stsServoPoints9->push_back(StsServoPoint(8,1500,2396));
		stsServoPoints9->push_back(StsServoPoint(6,1500,2120));
		stsServoPoints9->push_back(StsServoPoint(7,2000,1807));
		stsServoPoints9->push_back(StsServoPoint(8,2000,2330));
		scsServoPoints9->push_back(StsServoPoint(4,2000,887));
		scsServoPoints9->push_back(StsServoPoint(5,2000,193));
		scsServoPoints9->push_back(StsServoPoint(4,3000,177));
		stsServoPoints9->push_back(StsServoPoint(8,3000,2041));
		stsServoPoints9->push_back(StsServoPoint(6,3000,2409));
		stsServoPoints9->push_back(StsServoPoint(7,3000,2021));
		scsServoPoints9->push_back(StsServoPoint(5,3375,1024));
		auto state9 = new TimelineState(2, String("Standing"));
		Timeline *timeline9 = new Timeline(state9, String("Stand - Look Around 1"), stsServoPoints9, scsServoPoints9, pca9685PwmServoPoints9, mp3PlayerYX5300Points9);
		timelines->push_back(*timeline9);

		auto *stsServoPoints10 = new std::vector<StsServoPoint>();
		auto *scsServoPoints10 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints10 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points10 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints10->push_back(StsServoPoint(9,0,1741));
		stsServoPoints10->push_back(StsServoPoint(11,0,2342));
		scsServoPoints10->push_back(StsServoPoint(10,0,682));
		scsServoPoints10->push_back(StsServoPoint(12,0,453));
		stsServoPoints10->push_back(StsServoPoint(6,0,2326));
		scsServoPoints10->push_back(StsServoPoint(4,0,137));
		scsServoPoints10->push_back(StsServoPoint(5,0,830));
		stsServoPoints10->push_back(StsServoPoint(8,0,1936));
		stsServoPoints10->push_back(StsServoPoint(7,0,2159));
		scsServoPoints10->push_back(StsServoPoint(10,1500,677));
		scsServoPoints10->push_back(StsServoPoint(5,1500,387));
		scsServoPoints10->push_back(StsServoPoint(4,1500,733));
		stsServoPoints10->push_back(StsServoPoint(6,2000,1854));
		scsServoPoints10->push_back(StsServoPoint(10,2250,740));
		scsServoPoints10->push_back(StsServoPoint(12,2250,496));
		stsServoPoints10->push_back(StsServoPoint(7,2750,2144));
		stsServoPoints10->push_back(StsServoPoint(8,3875,1765));
		scsServoPoints10->push_back(StsServoPoint(10,4000,752));
		scsServoPoints10->push_back(StsServoPoint(12,4000,463));
		scsServoPoints10->push_back(StsServoPoint(4,4625,733));
		scsServoPoints10->push_back(StsServoPoint(5,4625,387));
		stsServoPoints10->push_back(StsServoPoint(7,4625,1930));
		stsServoPoints10->push_back(StsServoPoint(8,4625,2107));
		stsServoPoints10->push_back(StsServoPoint(6,5000,1854));
		scsServoPoints10->push_back(StsServoPoint(4,6125,0));
		scsServoPoints10->push_back(StsServoPoint(5,6125,1024));
		scsServoPoints10->push_back(StsServoPoint(10,6500,752));
		scsServoPoints10->push_back(StsServoPoint(12,6500,469));
		stsServoPoints10->push_back(StsServoPoint(6,7000,2082));
		scsServoPoints10->push_back(StsServoPoint(10,7750,694));
		scsServoPoints10->push_back(StsServoPoint(12,7750,501));
		stsServoPoints10->push_back(StsServoPoint(8,7750,1936));
		stsServoPoints10->push_back(StsServoPoint(6,8625,2082));
		stsServoPoints10->push_back(StsServoPoint(6,10000,1998));
		auto state10 = new TimelineState(2, String("Standing"));
		Timeline *timeline10 = new Timeline(state10, String("Stand - Pause"), stsServoPoints10, scsServoPoints10, pca9685PwmServoPoints10, mp3PlayerYX5300Points10);
		timelines->push_back(*timeline10);

		auto *stsServoPoints11 = new std::vector<StsServoPoint>();
		auto *scsServoPoints11 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints11 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points11 = new std::vector<Mp3PlayerYX5300Point>();
		stsServoPoints11->push_back(StsServoPoint(9,0,1741));
		stsServoPoints11->push_back(StsServoPoint(11,0,2342));
		scsServoPoints11->push_back(StsServoPoint(10,0,682));
		scsServoPoints11->push_back(StsServoPoint(12,0,453));
		stsServoPoints11->push_back(StsServoPoint(6,0,2326));
		stsServoPoints11->push_back(StsServoPoint(7,0,2343));
		stsServoPoints11->push_back(StsServoPoint(8,0,2186));
		scsServoPoints11->push_back(StsServoPoint(4,0,40));
		scsServoPoints11->push_back(StsServoPoint(5,0,830));
		scsServoPoints11->push_back(StsServoPoint(1,0,512));
		scsServoPoints11->push_back(StsServoPoint(2,0,512));
		scsServoPoints11->push_back(StsServoPoint(3,875,572));
		scsServoPoints11->push_back(StsServoPoint(3,1500,435));
		stsServoPoints11->push_back(StsServoPoint(6,1500,2090));
		stsServoPoints11->push_back(StsServoPoint(7,1500,2159));
		scsServoPoints11->push_back(StsServoPoint(1,1500,580));
		scsServoPoints11->push_back(StsServoPoint(2,1500,443));
		scsServoPoints11->push_back(StsServoPoint(3,2125,572));
		scsServoPoints11->push_back(StsServoPoint(5,2375,395));
		scsServoPoints11->push_back(StsServoPoint(4,2375,507));
		stsServoPoints11->push_back(StsServoPoint(6,2375,2333));
		scsServoPoints11->push_back(StsServoPoint(1,3000,515));
		scsServoPoints11->push_back(StsServoPoint(2,3000,514));
		stsServoPoints11->push_back(StsServoPoint(7,3625,2557));
		stsServoPoints11->push_back(StsServoPoint(8,3625,2422));
		stsServoPoints11->push_back(StsServoPoint(6,3625,2128));
		scsServoPoints11->push_back(StsServoPoint(4,3875,0));
		scsServoPoints11->push_back(StsServoPoint(5,3875,854));
		stsServoPoints11->push_back(StsServoPoint(7,3875,2557));
		stsServoPoints11->push_back(StsServoPoint(8,3875,2422));
		stsServoPoints11->push_back(StsServoPoint(6,4750,2333));
		stsServoPoints11->push_back(StsServoPoint(7,4750,2373));
		stsServoPoints11->push_back(StsServoPoint(8,4750,2054));
		mp3PlayerYX5300Points11->push_back(Mp3PlayerYX5300Point(8, 0, 1000));
		auto state11 = new TimelineState(2, String("Standing"));
		Timeline *timeline11 = new Timeline(state11, String("Stand - Speak Hmmm"), stsServoPoints11, scsServoPoints11, pca9685PwmServoPoints11, mp3PlayerYX5300Points11);
		timelines->push_back(*timeline11);

    }

    ~AutoPlayData()
    {
    }
};

#endif
