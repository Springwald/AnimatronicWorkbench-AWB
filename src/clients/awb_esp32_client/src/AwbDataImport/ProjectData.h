#ifndef _PROJECTDATA_H_
#define _PROJECTDATA_H_

#include <Arduino.h>
#include <String.h>
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineState.h"
#include "../ProjectData/TimelineStateReference.h"

#include "../ProjectData/Servos/StsServoPoint.h"
#include "../ProjectData/Servos/Pca9685PwmServoPoint.h"
#include "../ProjectData/Servos/StsScsServo.h"
#include "../ProjectData/Servos/Pca9685PwmServo.h"

#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
#include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
#include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench-EN

// Created on 01.09.2024 01:00:59

class ProjectData
{

	using TCallBackErrorOccured = std::function<void(String)>;

public:
	const char *ProjectName = "PIP-Droid";

	const int returnToAutoModeAfterMinutes = -1;

	/* Names as const to prevent magic strings in custom code: */

	const String TimelineName_Rainbow = "Rainbow";
	const String TimelineName_StartUp = "StartUp";
	const String TimelineName_Wakeup = "Wake up";
	const String TimelineName_Idle2 = "Idle 2";
	const String TimelineName_Evil = "Evil";
	const String TimelineName_GoSleep = "Go Sleep";
	const String TimelineName_Sleeping = "Sleeping";
	const String TimelineName_Idle3 = "Idle 3";
	const String TimelineName_Idle1 = "Idle 1";
	const String TimelineName_Idle4 = "Idle 4";
	const String Mp3PlayerName_PipVoiceSound = "PipVoiceSound";
	const String StsServoName_HeadTiltLeftRight = "HeadTiltLeftRight";
	const String StsServoName_NeckInOut = "NeckInOut";
	const String StsServoName_HeadTiltUpDown = "HeadTiltUpDown";
	const String StsServoName_Ear = "Ear";

	std::vector<StsScsServo> *scsServos;
	std::vector<StsScsServo> *stsServos;
	std::vector<Pca9685PwmServo> *pca9685PwmServos;
	std::vector<TimelineState> *timelineStates;
	std::vector<Timeline> *timelines;
	std::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;
	std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;

	int inputIds[0] = {};
	String inputNames[0] = {};
	uint8_t inputIoPins[0] = {};
	int inputCount = 0;

	ProjectData(TCallBackErrorOccured errorOccured)
	{

		scsServos = new std::vector<StsScsServo>();

		stsServos = new std::vector<StsScsServo>();
		stsServos->push_back(StsScsServo(1, "HeadTiltLeftRight", 2600, 1600, 55, 500, 2150, 25, 1200, false));
		stsServos->push_back(StsScsServo(2, "NeckInOut", 2450, 1600, 55, 600, 1750, 10, 800, true));
		stsServos->push_back(StsScsServo(3, "HeadTiltUpDown", 2480, 1800, 55, 500, 2140, 25, 1200, true));
		stsServos->push_back(StsScsServo(4, "Ear", 500, 3800, 55, 200, 2048, 100, 1500, false));

		pca9685PwmServos = new std::vector<Pca9685PwmServo>();

		mp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();

		mp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();
		mp3PlayersDfPlayerMini->push_back(Mp3PlayerDfPlayerMiniSerial(17, 16, 18, "PipVoiceSound", "Mp3PlayerDFPlayerMini-1", errorOccured));

		timelineStates = new std::vector<TimelineState>();
		timelineStates->push_back(TimelineState(1, String("idle"), true, new std::vector<int>({}), new std::vector<int>({})));
		timelineStates->push_back(TimelineState(2, String("sleeping"), false, new std::vector<int>({}), new std::vector<int>({})));
		timelineStates->push_back(TimelineState(4, String("remote and custom code only"), false, new std::vector<int>({}), new std::vector<int>({})));

		addTimelines();
	}

	void addTimelines()
	{
		timelines = new std::vector<Timeline>();

		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		auto *scsServoPoints1 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints1 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points1 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints1 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints1->push_back(StsServoPoint(2, 0, 1694));
		stsServoPoints1->push_back(StsServoPoint(1, 1000, 2141));
		stsServoPoints1->push_back(StsServoPoint(3, 1000, 2137));
		stsServoPoints1->push_back(StsServoPoint(2, 1000, 1600));
		stsServoPoints1->push_back(StsServoPoint(1, 2000, 1889));
		stsServoPoints1->push_back(StsServoPoint(4, 2000, 1402));
		stsServoPoints1->push_back(StsServoPoint(1, 2500, 2600));
		stsServoPoints1->push_back(StsServoPoint(3, 3000, 2016));
		stsServoPoints1->push_back(StsServoPoint(1, 3500, 2592));
		stsServoPoints1->push_back(StsServoPoint(1, 4250, 1694));
		stsServoPoints1->push_back(StsServoPoint(3, 4500, 2237));
		stsServoPoints1->push_back(StsServoPoint(4, 4500, 2784));
		stsServoPoints1->push_back(StsServoPoint(1, 5000, 2143));
		stsServoPoints1->push_back(StsServoPoint(1, 5500, 1725));
		stsServoPoints1->push_back(StsServoPoint(1, 6000, 2363));
		stsServoPoints1->push_back(StsServoPoint(3, 6000, 2127));
		stsServoPoints1->push_back(StsServoPoint(3, 7125, 1800));
		stsServoPoints1->push_back(StsServoPoint(4, 7500, 1862));
		stsServoPoints1->push_back(StsServoPoint(3, 8500, 1888));
		stsServoPoints1->push_back(StsServoPoint(3, 10000, 2410));
		stsServoPoints1->push_back(StsServoPoint(4, 10000, 2994));
		stsServoPoints1->push_back(StsServoPoint(4, 11000, 1513));
		stsServoPoints1->push_back(StsServoPoint(3, 11500, 1858));
		stsServoPoints1->push_back(StsServoPoint(4, 12500, 2656));
		stsServoPoints1->push_back(StsServoPoint(3, 13000, 2164));
		stsServoPoints1->push_back(StsServoPoint(3, 14500, 2171));
		stsServoPoints1->push_back(StsServoPoint(4, 15500, 1617));
		mp3PlayerDfPlayerMiniPoints1->push_back(Mp3PlayerDfPlayerMiniPoint(15, 0, 2000));
		auto state1 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline1 = new Timeline(state1, 1, String("Idle 1"), stsServoPoints1, scsServoPoints1, pca9685PwmServoPoints1, mp3PlayerYX5300Points1, mp3PlayerDfPlayerMiniPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		auto *scsServoPoints2 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints2 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points2 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints2 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints2->push_back(StsServoPoint(2, 500, 1694));
		stsServoPoints2->push_back(StsServoPoint(2, 1000, 1693));
		stsServoPoints2->push_back(StsServoPoint(4, 1000, 1721));
		stsServoPoints2->push_back(StsServoPoint(2, 1500, 1694));
		stsServoPoints2->push_back(StsServoPoint(3, 1500, 2140));
		stsServoPoints2->push_back(StsServoPoint(2, 2500, 2048));
		stsServoPoints2->push_back(StsServoPoint(3, 2500, 1810));
		stsServoPoints2->push_back(StsServoPoint(3, 3000, 1810));
		stsServoPoints2->push_back(StsServoPoint(3, 3500, 2024));
		stsServoPoints2->push_back(StsServoPoint(4, 3500, 2292));
		stsServoPoints2->push_back(StsServoPoint(3, 5500, 1955));
		stsServoPoints2->push_back(StsServoPoint(4, 5500, 1279));
		stsServoPoints2->push_back(StsServoPoint(2, 5500, 1694));
		stsServoPoints2->push_back(StsServoPoint(2, 6500, 2209));
		stsServoPoints2->push_back(StsServoPoint(3, 6500, 2480));
		stsServoPoints2->push_back(StsServoPoint(2, 7500, 1694));
		stsServoPoints2->push_back(StsServoPoint(3, 7500, 1944));
		stsServoPoints2->push_back(StsServoPoint(4, 7500, 1513));
		stsServoPoints2->push_back(StsServoPoint(1, 7500, 2418));
		stsServoPoints2->push_back(StsServoPoint(1, 8500, 1670));
		stsServoPoints2->push_back(StsServoPoint(1, 9000, 1670));
		mp3PlayerDfPlayerMiniPoints2->push_back(Mp3PlayerDfPlayerMiniPoint(14, 0, 1000));
		auto state2 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline2 = new Timeline(state2, 1, String("Idle 2"), stsServoPoints2, scsServoPoints2, pca9685PwmServoPoints2, mp3PlayerYX5300Points2, mp3PlayerDfPlayerMiniPoints2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		auto *scsServoPoints3 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints3 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points3 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints3 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints3->push_back(StsServoPoint(2, 0, 1694));
		stsServoPoints3->push_back(StsServoPoint(2, 500, 1694));
		stsServoPoints3->push_back(StsServoPoint(1, 500, 1914));
		stsServoPoints3->push_back(StsServoPoint(3, 500, 2480));
		stsServoPoints3->push_back(StsServoPoint(4, 500, 993));
		stsServoPoints3->push_back(StsServoPoint(2, 1000, 1600));
		stsServoPoints3->push_back(StsServoPoint(2, 1500, 2115));
		stsServoPoints3->push_back(StsServoPoint(3, 1500, 1800));
		stsServoPoints3->push_back(StsServoPoint(4, 1500, 2422));
		stsServoPoints3->push_back(StsServoPoint(2, 2000, 1694));
		stsServoPoints3->push_back(StsServoPoint(1, 2500, 1891));
		stsServoPoints3->push_back(StsServoPoint(1, 3000, 2348));
		stsServoPoints3->push_back(StsServoPoint(3, 3000, 1800));
		stsServoPoints3->push_back(StsServoPoint(4, 3500, 1253));
		stsServoPoints3->push_back(StsServoPoint(1, 3500, 2371));
		stsServoPoints3->push_back(StsServoPoint(3, 4000, 1933));
		stsServoPoints3->push_back(StsServoPoint(4, 4000, 2266));
		stsServoPoints3->push_back(StsServoPoint(2, 4000, 1807));
		stsServoPoints3->push_back(StsServoPoint(1, 4000, 2214));
		stsServoPoints3->push_back(StsServoPoint(2, 4500, 1733));
		stsServoPoints3->push_back(StsServoPoint(2, 6000, 1733));
		stsServoPoints3->push_back(StsServoPoint(2, 7000, 2095));
		auto state3 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline3 = new Timeline(state3, 1, String("Idle 3"), stsServoPoints3, scsServoPoints3, pca9685PwmServoPoints3, mp3PlayerYX5300Points3, mp3PlayerDfPlayerMiniPoints3);
		timelines->push_back(*timeline3);

		auto *stsServoPoints4 = new std::vector<StsServoPoint>();
		auto *scsServoPoints4 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints4 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points4 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints4 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints4->push_back(StsServoPoint(4, 0, 1955));
		stsServoPoints4->push_back(StsServoPoint(3, 0, 1821));
		stsServoPoints4->push_back(StsServoPoint(2, 0, 1861));
		stsServoPoints4->push_back(StsServoPoint(1, 0, 2411));
		stsServoPoints4->push_back(StsServoPoint(3, 1000, 1800));
		stsServoPoints4->push_back(StsServoPoint(2, 1000, 1613));
		stsServoPoints4->push_back(StsServoPoint(1, 1000, 2182));
		stsServoPoints4->push_back(StsServoPoint(4, 1000, 3280));
		stsServoPoints4->push_back(StsServoPoint(1, 2000, 2458));
		stsServoPoints4->push_back(StsServoPoint(2, 2000, 1620));
		stsServoPoints4->push_back(StsServoPoint(3, 2000, 2480));
		stsServoPoints4->push_back(StsServoPoint(3, 2500, 2480));
		stsServoPoints4->push_back(StsServoPoint(4, 2500, 1097));
		stsServoPoints4->push_back(StsServoPoint(2, 2500, 1620));
		stsServoPoints4->push_back(StsServoPoint(1, 2500, 2458));
		stsServoPoints4->push_back(StsServoPoint(3, 3000, 2480));
		stsServoPoints4->push_back(StsServoPoint(1, 4000, 1600));
		stsServoPoints4->push_back(StsServoPoint(4, 4000, 2500));
		stsServoPoints4->push_back(StsServoPoint(1, 4500, 1600));
		stsServoPoints4->push_back(StsServoPoint(1, 6000, 2150));
		stsServoPoints4->push_back(StsServoPoint(2, 6000, 1753));
		stsServoPoints4->push_back(StsServoPoint(3, 6000, 1800));
		stsServoPoints4->push_back(StsServoPoint(3, 6500, 1800));
		stsServoPoints4->push_back(StsServoPoint(1, 7000, 2150));
		stsServoPoints4->push_back(StsServoPoint(2, 7000, 1700));
		stsServoPoints4->push_back(StsServoPoint(3, 7000, 2140));
		stsServoPoints4->push_back(StsServoPoint(4, 7000, 2048));
		mp3PlayerDfPlayerMiniPoints4->push_back(Mp3PlayerDfPlayerMiniPoint(25, 0, 1000));
		mp3PlayerDfPlayerMiniPoints4->push_back(Mp3PlayerDfPlayerMiniPoint(28, 0, 5000));
		auto state4 = new TimelineStateReference(1, String("idle"));
		Timeline *timeline4 = new Timeline(state4, 1, String("Idle 4"), stsServoPoints4, scsServoPoints4, pca9685PwmServoPoints4, mp3PlayerYX5300Points4, mp3PlayerDfPlayerMiniPoints4);
		timelines->push_back(*timeline4);

		auto *stsServoPoints5 = new std::vector<StsServoPoint>();
		auto *scsServoPoints5 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints5 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points5 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints5 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints5->push_back(StsServoPoint(4, 0, 500));
		stsServoPoints5->push_back(StsServoPoint(4, 3000, 500));
		auto state5 = new TimelineStateReference(2, String("sleeping"));
		Timeline *timeline5 = new Timeline(state5, 2, String("Sleeping"), stsServoPoints5, scsServoPoints5, pca9685PwmServoPoints5, mp3PlayerYX5300Points5, mp3PlayerDfPlayerMiniPoints5);
		timelines->push_back(*timeline5);

		auto *stsServoPoints6 = new std::vector<StsServoPoint>();
		auto *scsServoPoints6 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints6 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points6 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints6 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints6->push_back(StsServoPoint(2, 0, 1700));
		stsServoPoints6->push_back(StsServoPoint(2, 500, 1600));
		stsServoPoints6->push_back(StsServoPoint(3, 500, 1976));
		stsServoPoints6->push_back(StsServoPoint(4, 500, 1383));
		stsServoPoints6->push_back(StsServoPoint(1, 500, 2308));
		stsServoPoints6->push_back(StsServoPoint(2, 1000, 1794));
		stsServoPoints6->push_back(StsServoPoint(1, 1000, 2025));
		stsServoPoints6->push_back(StsServoPoint(2, 1500, 1600));
		stsServoPoints6->push_back(StsServoPoint(3, 1500, 2244));
		stsServoPoints6->push_back(StsServoPoint(4, 1500, 2994));
		stsServoPoints6->push_back(StsServoPoint(2, 2000, 1780));
		stsServoPoints6->push_back(StsServoPoint(1, 2000, 2371));
		stsServoPoints6->push_back(StsServoPoint(3, 2500, 2121));
		stsServoPoints6->push_back(StsServoPoint(1, 2500, 1859));
		stsServoPoints6->push_back(StsServoPoint(2, 3000, 1694));
		stsServoPoints6->push_back(StsServoPoint(4, 3000, 1643));
		stsServoPoints6->push_back(StsServoPoint(3, 3000, 1800));
		stsServoPoints6->push_back(StsServoPoint(3, 3500, 2239));
		stsServoPoints6->push_back(StsServoPoint(3, 4000, 2153));
		stsServoPoints6->push_back(StsServoPoint(4, 4000, 2708));
		stsServoPoints6->push_back(StsServoPoint(1, 4000, 2119));
		stsServoPoints6->push_back(StsServoPoint(1, 4500, 2009));
		stsServoPoints6->push_back(StsServoPoint(3, 5000, 2421));
		stsServoPoints6->push_back(StsServoPoint(1, 5000, 2529));
		stsServoPoints6->push_back(StsServoPoint(2, 6000, 1694));
		stsServoPoints6->push_back(StsServoPoint(3, 6000, 2110));
		stsServoPoints6->push_back(StsServoPoint(4, 6000, 1617));
		mp3PlayerDfPlayerMiniPoints6->push_back(Mp3PlayerDfPlayerMiniPoint(15, 0, 500));
		auto state6 = new TimelineStateReference(4, String("remote and custom code only"));
		Timeline *timeline6 = new Timeline(state6, 1, String("Evil"), stsServoPoints6, scsServoPoints6, pca9685PwmServoPoints6, mp3PlayerYX5300Points6, mp3PlayerDfPlayerMiniPoints6);
		timelines->push_back(*timeline6);

		auto *stsServoPoints7 = new std::vector<StsServoPoint>();
		auto *scsServoPoints7 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints7 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points7 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints7 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints7->push_back(StsServoPoint(2, 0, 1727));
		stsServoPoints7->push_back(StsServoPoint(3, 0, 2110));
		stsServoPoints7->push_back(StsServoPoint(4, 0, 2042));
		stsServoPoints7->push_back(StsServoPoint(1, 500, 2150));
		stsServoPoints7->push_back(StsServoPoint(2, 1000, 2450));
		stsServoPoints7->push_back(StsServoPoint(3, 1000, 2110));
		stsServoPoints7->push_back(StsServoPoint(4, 1000, 2042));
		stsServoPoints7->push_back(StsServoPoint(1, 1000, 2150));
		stsServoPoints7->push_back(StsServoPoint(4, 2500, 500));
		stsServoPoints7->push_back(StsServoPoint(4, 4500, 500));
		mp3PlayerDfPlayerMiniPoints7->push_back(Mp3PlayerDfPlayerMiniPoint(18, 0, 500));
		auto state7 = new TimelineStateReference(4, String("remote and custom code only"));
		Timeline *timeline7 = new Timeline(state7, 2, String("Go Sleep"), stsServoPoints7, scsServoPoints7, pca9685PwmServoPoints7, mp3PlayerYX5300Points7, mp3PlayerDfPlayerMiniPoints7);
		timelines->push_back(*timeline7);

		auto *stsServoPoints8 = new std::vector<StsServoPoint>();
		auto *scsServoPoints8 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints8 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points8 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints8 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints8->push_back(StsServoPoint(2, 0, 1600));
		stsServoPoints8->push_back(StsServoPoint(3, 0, 1800));
		stsServoPoints8->push_back(StsServoPoint(2, 1000, 2296));
		stsServoPoints8->push_back(StsServoPoint(3, 1000, 1800));
		stsServoPoints8->push_back(StsServoPoint(4, 1000, 2059));
		stsServoPoints8->push_back(StsServoPoint(1, 1000, 2150));
		stsServoPoints8->push_back(StsServoPoint(2, 2000, 1600));
		stsServoPoints8->push_back(StsServoPoint(3, 2000, 2480));
		stsServoPoints8->push_back(StsServoPoint(4, 2000, 500));
		stsServoPoints8->push_back(StsServoPoint(2, 3000, 2195));
		stsServoPoints8->push_back(StsServoPoint(3, 3000, 1800));
		stsServoPoints8->push_back(StsServoPoint(4, 3000, 2890));
		stsServoPoints8->push_back(StsServoPoint(2, 4000, 1600));
		stsServoPoints8->push_back(StsServoPoint(3, 4000, 2480));
		stsServoPoints8->push_back(StsServoPoint(4, 4000, 759));
		stsServoPoints8->push_back(StsServoPoint(2, 5000, 2202));
		stsServoPoints8->push_back(StsServoPoint(3, 5000, 1800));
		stsServoPoints8->push_back(StsServoPoint(4, 5000, 3800));
		stsServoPoints8->push_back(StsServoPoint(2, 6000, 1600));
		stsServoPoints8->push_back(StsServoPoint(3, 6000, 2019));
		stsServoPoints8->push_back(StsServoPoint(4, 6000, 1487));
		mp3PlayerDfPlayerMiniPoints8->push_back(Mp3PlayerDfPlayerMiniPoint(29, 0, 1000));
		mp3PlayerDfPlayerMiniPoints8->push_back(Mp3PlayerDfPlayerMiniPoint(22, 0, 3000));
		mp3PlayerDfPlayerMiniPoints8->push_back(Mp3PlayerDfPlayerMiniPoint(21, 0, 6000));
		auto state8 = new TimelineStateReference(4, String("remote and custom code only"));
		Timeline *timeline8 = new Timeline(state8, -1, String("Rainbow"), stsServoPoints8, scsServoPoints8, pca9685PwmServoPoints8, mp3PlayerYX5300Points8, mp3PlayerDfPlayerMiniPoints8);
		timelines->push_back(*timeline8);

		auto *stsServoPoints9 = new std::vector<StsServoPoint>();
		auto *scsServoPoints9 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints9 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points9 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints9 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints9->push_back(StsServoPoint(2, 0, 2450));
		stsServoPoints9->push_back(StsServoPoint(3, 0, 2140));
		stsServoPoints9->push_back(StsServoPoint(1, 0, 2150));
		stsServoPoints9->push_back(StsServoPoint(4, 0, 525));
		stsServoPoints9->push_back(StsServoPoint(1, 1000, 2150));
		stsServoPoints9->push_back(StsServoPoint(2, 1000, 2450));
		stsServoPoints9->push_back(StsServoPoint(3, 1000, 2140));
		stsServoPoints9->push_back(StsServoPoint(4, 1000, 525));
		auto state9 = new TimelineStateReference(4, String("remote and custom code only"));
		Timeline *timeline9 = new Timeline(state9, 2, String("StartUp"), stsServoPoints9, scsServoPoints9, pca9685PwmServoPoints9, mp3PlayerYX5300Points9, mp3PlayerDfPlayerMiniPoints9);
		timelines->push_back(*timeline9);

		auto *stsServoPoints10 = new std::vector<StsServoPoint>();
		auto *scsServoPoints10 = new std::vector<StsServoPoint>();
		auto *pca9685PwmServoPoints10 = new std::vector<Pca9685PwmServoPoint>();
		auto *mp3PlayerYX5300Points10 = new std::vector<Mp3PlayerYX5300Point>();
		auto *mp3PlayerDfPlayerMiniPoints10 = new std::vector<Mp3PlayerDfPlayerMiniPoint>();
		stsServoPoints10->push_back(StsServoPoint(1, -125, 2150));
		stsServoPoints10->push_back(StsServoPoint(3, -125, 2140));
		stsServoPoints10->push_back(StsServoPoint(2, 0, 1600));
		stsServoPoints10->push_back(StsServoPoint(1, 0, 2150));
		stsServoPoints10->push_back(StsServoPoint(3, 0, 2140));
		stsServoPoints10->push_back(StsServoPoint(4, 0, 2042));
		stsServoPoints10->push_back(StsServoPoint(3, 500, 2140));
		stsServoPoints10->push_back(StsServoPoint(3, 1000, 2480));
		stsServoPoints10->push_back(StsServoPoint(4, 1000, 655));
		stsServoPoints10->push_back(StsServoPoint(3, 1250, 2480));
		stsServoPoints10->push_back(StsServoPoint(3, 1750, 1960));
		stsServoPoints10->push_back(StsServoPoint(1, 1750, 2150));
		stsServoPoints10->push_back(StsServoPoint(4, 2000, 2162));
		stsServoPoints10->push_back(StsServoPoint(1, 2250, 1788));
		stsServoPoints10->push_back(StsServoPoint(1, 2500, 1788));
		stsServoPoints10->push_back(StsServoPoint(1, 2875, 2426));
		stsServoPoints10->push_back(StsServoPoint(3, 2875, 1960));
		stsServoPoints10->push_back(StsServoPoint(4, 2875, 2656));
		stsServoPoints10->push_back(StsServoPoint(3, 3500, 2140));
		stsServoPoints10->push_back(StsServoPoint(1, 3500, 2426));
		stsServoPoints10->push_back(StsServoPoint(4, 3750, 2042));
		stsServoPoints10->push_back(StsServoPoint(1, 4000, 2150));
		stsServoPoints10->push_back(StsServoPoint(1, 4500, 2150));
		mp3PlayerDfPlayerMiniPoints10->push_back(Mp3PlayerDfPlayerMiniPoint(12, 0, 750));
		auto state10 = new TimelineStateReference(4, String("remote and custom code only"));
		Timeline *timeline10 = new Timeline(state10, 1, String("Wake up"), stsServoPoints10, scsServoPoints10, pca9685PwmServoPoints10, mp3PlayerYX5300Points10, mp3PlayerDfPlayerMiniPoints10);
		timelines->push_back(*timeline10);
	}
};

#endif // _PROJECTDATA_H_
