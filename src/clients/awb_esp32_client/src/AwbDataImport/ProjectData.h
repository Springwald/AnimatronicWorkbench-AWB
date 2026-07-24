
    #ifndef _PROJECTDATA_H_
#define _PROJECTDATA_H_

    #include <Arduino.h>
    #include <String.h>
    #include "../ProjectData/Timeline.h"
    #include "../ProjectData/TimelineState.h"
    #include "../ProjectData/TimelineState.h"
    #include "../ProjectData/TimelineStateReference.h"

    #include "../ProjectData/Servos/ServoPoint.h"
    #include "../ProjectData/Servos/Servo.h"
    #include <ProjectData/Servos/Servos.h>

    #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Serial.h"
    #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniSerial.h"
    #include "../ProjectData/Mp3Player/Mp3PlayerYX5300Point.h"
    #include "../ProjectData/Mp3Player/Mp3PlayerDfPlayerMiniPoint.h"

    
// Created with Animatronic Workbench Studio
// https://daniel.springwald.de/post/AWB/AnimatronicWorkbench

// Created on 24.07.2026 13:19:32

class ProjectData
{


    using TCallBackErrorOccured = std::function<void(String)>;

    public:
        const char *ProjectName = "MotorModeTest";
        const int returnToAutoModeAfterMinutes  = -1;

       /* Names as const to prevent magic strings in custom code: */

   const String StsServoName_TestServo20 ="Test Servo 20";



    Servos *servos;
    std::vector<TimelineState>* timelineStates;
    std::vector<Timeline>* timelines;
    std::vector<Mp3PlayerYX5300Serial> *mp3PlayersYX5300;
    std::vector<Mp3PlayerDfPlayerMiniSerial> *mp3PlayersDfPlayerMini;

    	int inputIds[0] = {};
	String inputNames[0] = {};
	uint8_t  inputIoPins[0] = {};
	int inputCount = 0;



    ProjectData(TCallBackErrorOccured errorOccured)
    {
        // the servos
        servos = new Servos();
        
        std::vector<RelaxRange> *servo_000_relaxRanges = new std::vector<RelaxRange>();
		servos->addServo(Servo("StsServo-1", new ServoConfig(ServoConfig::ServoTypes::STS_SERVO, "Test Servo 20", 20, 0, 0, 4095, 55, 400, 2048, 150, 4000, false, true, servo_000_relaxRanges)));

        

        // sound player
        mp3PlayersYX5300 = new std::vector<Mp3PlayerYX5300Serial>();

        mp3PlayersDfPlayerMini = new std::vector<Mp3PlayerDfPlayerMiniSerial>();


        // timelines states
        timelineStates = new std::vector<TimelineState>();
		timelineStates->push_back(TimelineState(1, String("Default"), true, new std::vector<int>({  }), new std::vector<int>({  })));

    

        addTimelines();
    }

    // timelines
    void addTimelines() 
    {
        timelines = new std::vector<Timeline>();
        
    }

    
};

#endif // _PROJECTDATA_H_


