#ifndef _AUTOPLAYDATA_H_
#define _AUTOPLAYDATA_H_

#include <Arduino.h>
#include <String.h>
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"

/// <summary>
/// This is a prototype for the AutoPlayData class.
/// It is used as a template to inject the generated data export of the animatronic workbench studio app.
/// </summary>

// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AnimatronicWorkbench

// Created on 12.08.2023 13:18:54

class AutoPlayData
{

protected:
public:
    std::vector<Timeline> *timelines;
	int stsServoCount = 7;
	int stsServoChannels[7] = {10, 11, 12, 13, 14, 15, 16};
	int stsServoAccelleration[7] = {100, -1, -1, -1, -1, -1, -1};
	int stsServoSpeed[7] = {1500, -1, -1, -1, -1, -1, -1};
	int timelineStateIds[5] = {1, 2, 3, 4, 5};
	int timelineStateCount = 5;

    AutoPlayData()
    {
        timelines = new std::vector<Timeline>();
		auto *stsServoPoints1 = new std::vector<StsServoPoint>();
		stsServoPoints1->push_back(StsServoPoint(10,0,2100));
		stsServoPoints1->push_back(StsServoPoint(11,0,1825));
		stsServoPoints1->push_back(StsServoPoint(12,0,2236));
		stsServoPoints1->push_back(StsServoPoint(15,0,1941));
		stsServoPoints1->push_back(StsServoPoint(13,0,2570));
		stsServoPoints1->push_back(StsServoPoint(14,0,1427));
		stsServoPoints1->push_back(StsServoPoint(16,2000,1614));
		stsServoPoints1->push_back(StsServoPoint(12,3000,2600));
		stsServoPoints1->push_back(StsServoPoint(15,3000,1941));
		stsServoPoints1->push_back(StsServoPoint(11,3000,1842));
		stsServoPoints1->push_back(StsServoPoint(12,4250,2600));
		stsServoPoints1->push_back(StsServoPoint(15,4250,1941));
		stsServoPoints1->push_back(StsServoPoint(12,6000,2258));
		stsServoPoints1->push_back(StsServoPoint(15,6000,2048));
		stsServoPoints1->push_back(StsServoPoint(10,7000,2100));
		stsServoPoints1->push_back(StsServoPoint(15,8000,1687));
		stsServoPoints1->push_back(StsServoPoint(12,8000,2588));
		stsServoPoints1->push_back(StsServoPoint(11,8000,1842));
		stsServoPoints1->push_back(StsServoPoint(13,9250,2557));
		stsServoPoints1->push_back(StsServoPoint(11,9500,1893));
		stsServoPoints1->push_back(StsServoPoint(12,11000,2588));
		stsServoPoints1->push_back(StsServoPoint(15,11000,1687));
		stsServoPoints1->push_back(StsServoPoint(11,11000,1842));
		stsServoPoints1->push_back(StsServoPoint(15,12000,1687));
		stsServoPoints1->push_back(StsServoPoint(12,12000,2588));
		stsServoPoints1->push_back(StsServoPoint(12,13500,2412));
		stsServoPoints1->push_back(StsServoPoint(15,13500,1874));
		auto state1 = new TimelineState(1, String("Sleep"));
		Timeline *timeline1 = new Timeline(state1, String("Sleeping-Head-Moving-Forwards"), stsServoPoints1);
		timelines->push_back(*timeline1);

		auto *stsServoPoints2 = new std::vector<StsServoPoint>();
		stsServoPoints2->push_back(StsServoPoint(12,0,2125));
		stsServoPoints2->push_back(StsServoPoint(15,0,1852));
		stsServoPoints2->push_back(StsServoPoint(13,0,2570));
		stsServoPoints2->push_back(StsServoPoint(14,0,1427));
		stsServoPoints2->push_back(StsServoPoint(16,0,2422));
		stsServoPoints2->push_back(StsServoPoint(10,0,1790));
		stsServoPoints2->push_back(StsServoPoint(16,1000,2197));
		stsServoPoints2->push_back(StsServoPoint(11,1000,2429));
		stsServoPoints2->push_back(StsServoPoint(13,1000,2298));
		stsServoPoints2->push_back(StsServoPoint(12,1000,2125));
		stsServoPoints2->push_back(StsServoPoint(15,1000,1852));
		stsServoPoints2->push_back(StsServoPoint(12,2000,1905));
		stsServoPoints2->push_back(StsServoPoint(15,2000,2129));
		stsServoPoints2->push_back(StsServoPoint(14,3000,1427));
		stsServoPoints2->push_back(StsServoPoint(13,3000,2570));
		stsServoPoints2->push_back(StsServoPoint(15,3000,1852));
		stsServoPoints2->push_back(StsServoPoint(12,3000,2125));
		stsServoPoints2->push_back(StsServoPoint(11,3000,2429));
		stsServoPoints2->push_back(StsServoPoint(11,3250,1339));
		stsServoPoints2->push_back(StsServoPoint(12,3250,2313));
		stsServoPoints2->push_back(StsServoPoint(15,3250,2503));
		stsServoPoints2->push_back(StsServoPoint(16,3250,2197));
		stsServoPoints2->push_back(StsServoPoint(14,3500,1427));
		stsServoPoints2->push_back(StsServoPoint(12,3750,2313));
		stsServoPoints2->push_back(StsServoPoint(15,3750,2503));
		stsServoPoints2->push_back(StsServoPoint(10,4000,1790));
		stsServoPoints2->push_back(StsServoPoint(10,4250,2100));
		stsServoPoints2->push_back(StsServoPoint(10,4500,1790));
		stsServoPoints2->push_back(StsServoPoint(12,4750,2148));
		stsServoPoints2->push_back(StsServoPoint(15,4750,2155));
		stsServoPoints2->push_back(StsServoPoint(14,4750,1740));
		stsServoPoints2->push_back(StsServoPoint(11,6000,1339));
		stsServoPoints2->push_back(StsServoPoint(12,6000,2313));
		stsServoPoints2->push_back(StsServoPoint(15,6000,2503));
		stsServoPoints2->push_back(StsServoPoint(13,6000,2570));
		stsServoPoints2->push_back(StsServoPoint(14,6000,1427));
		stsServoPoints2->push_back(StsServoPoint(16,6000,2197));
		auto state2 = new TimelineState(2, String("Action"));
		Timeline *timeline2 = new Timeline(state2, String("Look-to-camera"), stsServoPoints2);
		timelines->push_back(*timeline2);

		auto *stsServoPoints3 = new std::vector<StsServoPoint>();
		stsServoPoints3->push_back(StsServoPoint(10,0,1790));
		stsServoPoints3->push_back(StsServoPoint(11,0,1859));
		stsServoPoints3->push_back(StsServoPoint(12,0,2125));
		stsServoPoints3->push_back(StsServoPoint(15,0,1852));
		stsServoPoints3->push_back(StsServoPoint(13,0,2570));
		stsServoPoints3->push_back(StsServoPoint(14,0,1427));
		stsServoPoints3->push_back(StsServoPoint(16,0,2706));
		stsServoPoints3->push_back(StsServoPoint(16,1000,3005));
		stsServoPoints3->push_back(StsServoPoint(16,2000,3005));
		stsServoPoints3->push_back(StsServoPoint(14,2000,2018));
		stsServoPoints3->push_back(StsServoPoint(13,2000,1890));
		stsServoPoints3->push_back(StsServoPoint(12,2250,2181));
		stsServoPoints3->push_back(StsServoPoint(15,2250,1861));
		stsServoPoints3->push_back(StsServoPoint(11,2250,1641));
		stsServoPoints3->push_back(StsServoPoint(16,4000,1659));
		stsServoPoints3->push_back(StsServoPoint(14,4000,2389));
		stsServoPoints3->push_back(StsServoPoint(13,4000,1519));
		stsServoPoints3->push_back(StsServoPoint(15,4000,2838));
		stsServoPoints3->push_back(StsServoPoint(12,4000,1707));
		stsServoPoints3->push_back(StsServoPoint(13,5000,1717));
		stsServoPoints3->push_back(StsServoPoint(14,5000,2180));
		stsServoPoints3->push_back(StsServoPoint(11,6000,1406));
		stsServoPoints3->push_back(StsServoPoint(12,6000,2291));
		stsServoPoints3->push_back(StsServoPoint(15,6000,2785));
		stsServoPoints3->push_back(StsServoPoint(12,7000,2291));
		stsServoPoints3->push_back(StsServoPoint(11,7000,1406));
		stsServoPoints3->push_back(StsServoPoint(15,7000,2785));
		stsServoPoints3->push_back(StsServoPoint(11,9000,2228));
		stsServoPoints3->push_back(StsServoPoint(12,9000,1508));
		stsServoPoints3->push_back(StsServoPoint(15,9000,2155));
		stsServoPoints3->push_back(StsServoPoint(13,9000,1729));
		stsServoPoints3->push_back(StsServoPoint(14,9000,2157));
		stsServoPoints3->push_back(StsServoPoint(11,11000,2228));
		stsServoPoints3->push_back(StsServoPoint(12,11000,1508));
		stsServoPoints3->push_back(StsServoPoint(15,11000,2155));
		stsServoPoints3->push_back(StsServoPoint(16,12000,1674));
		stsServoPoints3->push_back(StsServoPoint(10,13000,1790));
		stsServoPoints3->push_back(StsServoPoint(10,13250,2100));
		stsServoPoints3->push_back(StsServoPoint(10,13500,1790));
		stsServoPoints3->push_back(StsServoPoint(11,14000,2111));
		stsServoPoints3->push_back(StsServoPoint(12,14000,1266));
		stsServoPoints3->push_back(StsServoPoint(15,14000,2383));
		stsServoPoints3->push_back(StsServoPoint(11,15000,2077));
		stsServoPoints3->push_back(StsServoPoint(12,15500,2159));
		stsServoPoints3->push_back(StsServoPoint(11,16000,2111));
		stsServoPoints3->push_back(StsServoPoint(15,16000,2383));
		stsServoPoints3->push_back(StsServoPoint(12,18000,1431));
		stsServoPoints3->push_back(StsServoPoint(15,18750,2102));
		stsServoPoints3->push_back(StsServoPoint(15,20250,3200));
		stsServoPoints3->push_back(StsServoPoint(11,21000,1574));
		stsServoPoints3->push_back(StsServoPoint(12,21000,2114));
		stsServoPoints3->push_back(StsServoPoint(15,21000,2932));
		stsServoPoints3->push_back(StsServoPoint(14,21000,2192));
		stsServoPoints3->push_back(StsServoPoint(12,22000,1740));
		stsServoPoints3->push_back(StsServoPoint(11,23000,1574));
		stsServoPoints3->push_back(StsServoPoint(12,23000,2114));
		stsServoPoints3->push_back(StsServoPoint(15,23000,3039));
		stsServoPoints3->push_back(StsServoPoint(10,23250,1790));
		stsServoPoints3->push_back(StsServoPoint(10,23500,2100));
		stsServoPoints3->push_back(StsServoPoint(10,24000,1790));
		stsServoPoints3->push_back(StsServoPoint(11,25750,1876));
		stsServoPoints3->push_back(StsServoPoint(12,25750,1365));
		stsServoPoints3->push_back(StsServoPoint(15,25750,2463));
		stsServoPoints3->push_back(StsServoPoint(15,27750,2798));
		stsServoPoints3->push_back(StsServoPoint(12,27750,1740));
		stsServoPoints3->push_back(StsServoPoint(11,29250,1758));
		stsServoPoints3->push_back(StsServoPoint(12,29250,1696));
		stsServoPoints3->push_back(StsServoPoint(15,29250,2289));
		stsServoPoints3->push_back(StsServoPoint(11,30250,1691));
		stsServoPoints3->push_back(StsServoPoint(12,30250,1530));
		stsServoPoints3->push_back(StsServoPoint(15,30250,2664));
		stsServoPoints3->push_back(StsServoPoint(11,31750,1758));
		stsServoPoints3->push_back(StsServoPoint(12,31750,1696));
		stsServoPoints3->push_back(StsServoPoint(15,31750,2289));
		stsServoPoints3->push_back(StsServoPoint(12,33250,1365));
		stsServoPoints3->push_back(StsServoPoint(15,33750,2985));
		stsServoPoints3->push_back(StsServoPoint(11,33750,1976));
		stsServoPoints3->push_back(StsServoPoint(14,35000,2157));
		stsServoPoints3->push_back(StsServoPoint(11,36000,1775));
		stsServoPoints3->push_back(StsServoPoint(12,36000,1883));
		stsServoPoints3->push_back(StsServoPoint(15,36000,2691));
		stsServoPoints3->push_back(StsServoPoint(13,36000,1692));
		stsServoPoints3->push_back(StsServoPoint(14,36000,2238));
		stsServoPoints3->push_back(StsServoPoint(13,37000,1605));
		stsServoPoints3->push_back(StsServoPoint(12,37500,1629));
		stsServoPoints3->push_back(StsServoPoint(11,37500,1658));
		stsServoPoints3->push_back(StsServoPoint(11,39000,1775));
		stsServoPoints3->push_back(StsServoPoint(12,39000,1883));
		stsServoPoints3->push_back(StsServoPoint(15,39000,2677));
		stsServoPoints3->push_back(StsServoPoint(13,39000,1692));
		stsServoPoints3->push_back(StsServoPoint(14,39000,2238));
		stsServoPoints3->push_back(StsServoPoint(16,39000,1659));
		stsServoPoints3->push_back(StsServoPoint(16,40500,2721));
		stsServoPoints3->push_back(StsServoPoint(11,41500,1859));
		stsServoPoints3->push_back(StsServoPoint(12,41500,2258));
		stsServoPoints3->push_back(StsServoPoint(15,41500,2316));
		stsServoPoints3->push_back(StsServoPoint(13,41500,1766));
		stsServoPoints3->push_back(StsServoPoint(14,41500,2122));
		stsServoPoints3->push_back(StsServoPoint(14,42250,2111));
		stsServoPoints3->push_back(StsServoPoint(12,42500,2600));
		stsServoPoints3->push_back(StsServoPoint(11,42500,2010));
		stsServoPoints3->push_back(StsServoPoint(15,42500,1874));
		stsServoPoints3->push_back(StsServoPoint(13,43000,2570));
		stsServoPoints3->push_back(StsServoPoint(16,43250,2825));
		stsServoPoints3->push_back(StsServoPoint(14,43500,1427));
		stsServoPoints3->push_back(StsServoPoint(10,44750,1790));
		stsServoPoints3->push_back(StsServoPoint(10,45250,2100));
		stsServoPoints3->push_back(StsServoPoint(10,45750,1790));
		stsServoPoints3->push_back(StsServoPoint(13,46500,2557));
		stsServoPoints3->push_back(StsServoPoint(12,48250,2600));
		stsServoPoints3->push_back(StsServoPoint(15,48250,2048));
		stsServoPoints3->push_back(StsServoPoint(11,48250,2010));
		stsServoPoints3->push_back(StsServoPoint(12,48800,2302));
		stsServoPoints3->push_back(StsServoPoint(11,48800,1993));
		stsServoPoints3->push_back(StsServoPoint(15,48800,1807));
		stsServoPoints3->push_back(StsServoPoint(11,51250,1842));
		auto state3 = new TimelineState(3, String("Idle"));
		Timeline *timeline3 = new Timeline(state3, String("Lean-Forward"), stsServoPoints3);
		timelines->push_back(*timeline3);

		auto *stsServoPoints4 = new std::vector<StsServoPoint>();
		stsServoPoints4->push_back(StsServoPoint(10,0,1790));
		stsServoPoints4->push_back(StsServoPoint(13,0,2396));
		stsServoPoints4->push_back(StsServoPoint(14,0,1427));
		stsServoPoints4->push_back(StsServoPoint(16,0,2736));
		stsServoPoints4->push_back(StsServoPoint(11,0,1926));
		stsServoPoints4->push_back(StsServoPoint(12,0,1927));
		stsServoPoints4->push_back(StsServoPoint(15,0,2343));
		stsServoPoints4->push_back(StsServoPoint(13,1000,2372));
		stsServoPoints4->push_back(StsServoPoint(12,2000,2423));
		stsServoPoints4->push_back(StsServoPoint(11,2000,2278));
		stsServoPoints4->push_back(StsServoPoint(15,2000,1754));
		stsServoPoints4->push_back(StsServoPoint(10,2000,1790));
		stsServoPoints4->push_back(StsServoPoint(14,2000,1438));
		stsServoPoints4->push_back(StsServoPoint(13,2000,1605));
		stsServoPoints4->push_back(StsServoPoint(10,2250,2100));
		stsServoPoints4->push_back(StsServoPoint(10,2500,1790));
		stsServoPoints4->push_back(StsServoPoint(16,3000,2885));
		stsServoPoints4->push_back(StsServoPoint(14,3000,1438));
		stsServoPoints4->push_back(StsServoPoint(13,3000,1605));
		stsServoPoints4->push_back(StsServoPoint(11,3000,2278));
		stsServoPoints4->push_back(StsServoPoint(14,4000,2320));
		stsServoPoints4->push_back(StsServoPoint(16,4000,2885));
		stsServoPoints4->push_back(StsServoPoint(13,4000,2372));
		stsServoPoints4->push_back(StsServoPoint(11,4000,1339));
		stsServoPoints4->push_back(StsServoPoint(12,4000,2588));
		stsServoPoints4->push_back(StsServoPoint(15,4000,1700));
		stsServoPoints4->push_back(StsServoPoint(10,4250,1790));
		stsServoPoints4->push_back(StsServoPoint(10,4500,2073));
		stsServoPoints4->push_back(StsServoPoint(10,4750,1790));
		stsServoPoints4->push_back(StsServoPoint(11,5000,1339));
		stsServoPoints4->push_back(StsServoPoint(12,5000,2588));
		stsServoPoints4->push_back(StsServoPoint(15,5000,1700));
		stsServoPoints4->push_back(StsServoPoint(13,5000,2371));
		stsServoPoints4->push_back(StsServoPoint(14,5000,2319));
		stsServoPoints4->push_back(StsServoPoint(16,5000,2451));
		stsServoPoints4->push_back(StsServoPoint(11,6000,1926));
		stsServoPoints4->push_back(StsServoPoint(12,6000,1894));
		stsServoPoints4->push_back(StsServoPoint(15,6000,2570));
		stsServoPoints4->push_back(StsServoPoint(10,6000,1790));
		stsServoPoints4->push_back(StsServoPoint(16,6000,2451));
		stsServoPoints4->push_back(StsServoPoint(13,6000,2396));
		stsServoPoints4->push_back(StsServoPoint(14,6000,1438));
		auto state4 = new TimelineState(3, String("Idle"));
		Timeline *timeline4 = new Timeline(state4, String("Look at hand"), stsServoPoints4);
		timelines->push_back(*timeline4);

		auto *stsServoPoints5 = new std::vector<StsServoPoint>();
		stsServoPoints5->push_back(StsServoPoint(11,0,1859));
		stsServoPoints5->push_back(StsServoPoint(12,0,2125));
		stsServoPoints5->push_back(StsServoPoint(15,0,1852));
		stsServoPoints5->push_back(StsServoPoint(13,0,2570));
		stsServoPoints5->push_back(StsServoPoint(14,0,1427));
		stsServoPoints5->push_back(StsServoPoint(16,0,2706));
		stsServoPoints5->push_back(StsServoPoint(10,0,1790));
		stsServoPoints5->push_back(StsServoPoint(11,1000,1775));
		stsServoPoints5->push_back(StsServoPoint(14,1000,1438));
		stsServoPoints5->push_back(StsServoPoint(15,1000,1941));
		stsServoPoints5->push_back(StsServoPoint(13,1000,2570));
		stsServoPoints5->push_back(StsServoPoint(12,2000,2566));
		stsServoPoints5->push_back(StsServoPoint(15,2000,2905));
		stsServoPoints5->push_back(StsServoPoint(16,2000,2257));
		stsServoPoints5->push_back(StsServoPoint(15,2750,2557));
		stsServoPoints5->push_back(StsServoPoint(12,2750,2600));
		stsServoPoints5->push_back(StsServoPoint(11,2750,1960));
		stsServoPoints5->push_back(StsServoPoint(12,4000,1971));
		stsServoPoints5->push_back(StsServoPoint(15,4000,1941));
		stsServoPoints5->push_back(StsServoPoint(11,4000,1859));
		stsServoPoints5->push_back(StsServoPoint(10,4000,1790));
		stsServoPoints5->push_back(StsServoPoint(15,4250,1941));
		stsServoPoints5->push_back(StsServoPoint(10,4250,2100));
		stsServoPoints5->push_back(StsServoPoint(10,4500,1790));
		stsServoPoints5->push_back(StsServoPoint(12,5500,2566));
		stsServoPoints5->push_back(StsServoPoint(15,5500,2865));
		stsServoPoints5->push_back(StsServoPoint(11,5500,1758));
		stsServoPoints5->push_back(StsServoPoint(16,7000,1943));
		stsServoPoints5->push_back(StsServoPoint(14,7000,1600));
		stsServoPoints5->push_back(StsServoPoint(13,7000,2434));
		stsServoPoints5->push_back(StsServoPoint(12,7000,1971));
		stsServoPoints5->push_back(StsServoPoint(15,7000,2396));
		stsServoPoints5->push_back(StsServoPoint(15,9000,2865));
		stsServoPoints5->push_back(StsServoPoint(13,9000,2570));
		stsServoPoints5->push_back(StsServoPoint(11,9000,1658));
		stsServoPoints5->push_back(StsServoPoint(14,9000,1438));
		stsServoPoints5->push_back(StsServoPoint(11,9500,1473));
		stsServoPoints5->push_back(StsServoPoint(12,10000,2588));
		stsServoPoints5->push_back(StsServoPoint(16,10000,1584));
		stsServoPoints5->push_back(StsServoPoint(12,10500,2588));
		stsServoPoints5->push_back(StsServoPoint(15,10500,2865));
		stsServoPoints5->push_back(StsServoPoint(16,10500,1599));
		stsServoPoints5->push_back(StsServoPoint(16,11500,2242));
		stsServoPoints5->push_back(StsServoPoint(11,11500,1473));
		stsServoPoints5->push_back(StsServoPoint(12,11500,2214));
		stsServoPoints5->push_back(StsServoPoint(15,11500,2396));
		stsServoPoints5->push_back(StsServoPoint(13,11500,2570));
		stsServoPoints5->push_back(StsServoPoint(14,11500,1473));
		stsServoPoints5->push_back(StsServoPoint(11,13000,1893));
		stsServoPoints5->push_back(StsServoPoint(12,13000,1894));
		stsServoPoints5->push_back(StsServoPoint(15,13000,2343));
		stsServoPoints5->push_back(StsServoPoint(16,13000,2257));
		stsServoPoints5->push_back(StsServoPoint(14,13000,1427));
		stsServoPoints5->push_back(StsServoPoint(13,13000,2557));
		auto state5 = new TimelineState(3, String("Idle"));
		Timeline *timeline5 = new Timeline(state5, String("Move-Head"), stsServoPoints5);
		timelines->push_back(*timeline5);

    }

    ~AutoPlayData()
    {
    }
};

#endif
