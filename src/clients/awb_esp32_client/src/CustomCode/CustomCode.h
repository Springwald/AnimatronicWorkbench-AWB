#ifndef _CUSTOM_CODE_H_
#define _CUSTOM_CODE_H_
/*
    Enter your custom code in this header file and the corresponding cpp file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
    ---
*/
#include <Arduino.h>
#include <String.h>
#include "../AwbDataImport/ProjectData.h"
#include <Actuators/NeopixelManager.h>
#include <Actuators/Servos/StScsSerialServoManager.h>
#include <Actuators/Servos/Pca9685PwmManager.h>
#include <Actuators/Mp3Player/GlobalMp3PlayerManager.h>
#include <Debugging.h>
/* cc-start-include - insert your include code here before the end-protected comment: */
#include "PipButtons.h"
#include "PipNeopixel.h"
/* cc-end-include - insert your include code here before the end-protected comment: */
class CustomCode
{
    using TCallBackErrorOccured = std::function<void(String)>;

protected:
    TCallBackErrorOccured _errorOccured;            // the error occured callback
    ProjectData *_projectData;                      // the project data exported by Animatronic Workbench Studio
    StScsSerialServoManager *_stSerialServoManager; // the STS serial servo manager
    StScsSerialServoManager *_scSerialServoManager; // the SCS serial servo manager
    Pca9685PwmManager *_pca9685PwmManager;          // the PCA9685 PWM manager
    GlobalMp3PlayerManager *_mp3PlayerManager;      // the MP3 player YX5300 manager
    Debugging *_debugging;                          /// the debugging class
    NeopixelManager *_neopixelManager;
    /* cc-start-protected - insert your protected code here before the end-protected comment: */
    const int dockedPin = 34;
    const int idleStateId = 1;
    unsigned long lastUpdateMs = 0;
    unsigned long idleStateDurationMs = 0;
    PipButtons *pipButtons = nullptr;
    PipNeopixel *pipNeopixel = nullptr;
    long sleepingTime = 0;
    bool wokeUpByBackHold = false;
    bool isDocked = false;
    void checkButtons(String actualTimelineName, int actualTimelineStateId);
    /* cc-end-protected  */
public:
    String *timelineNameToPlay = nullptr;         /// The name of the timeline to play by custom code. Beware: This is excuted immediately and will overwrite the current timeline and will interrupt the current timeline movements.
    int *soundNoToPlay = nullptr;                 /// The number of the sound to play by custom code
    int *timelineStateToForceOnce = nullptr;      /// Here the globale timeline state can be overwritten by custom code a single time. Buttons and other inputs defined in AWB Studio will be ignored.
    int *timelineStateToForcePermanent = nullptr; /// Here the globale timeline state can be overwritten by custom code permanent. Buttons and other inputs defined in AWB Studio will be ignored.
    CustomCode(ProjectData *projectData,
               NeopixelManager *neopixelManager,
               StScsSerialServoManager *stSerialServoManager,
               StScsSerialServoManager *scSerialServoManager,
               Pca9685PwmManager *pca9685PwmManager,
               GlobalMp3PlayerManager *mp3PlayerManager,
               TCallBackErrorOccured errorOccured, Debugging *debugging) : _projectData(projectData),
                                                                           _neopixelManager(neopixelManager),
                                                                           _stSerialServoManager(stSerialServoManager),
                                                                           _scSerialServoManager(scSerialServoManager),
                                                                           _pca9685PwmManager(pca9685PwmManager),
                                                                           _mp3PlayerManager(mp3PlayerManager),
                                                                           _errorOccured(errorOccured), _debugging(debugging)
    {
        /* cc-start-constructor - insert your constructor code here before the end-constructor comment: */
        pipNeopixel = new PipNeopixel(neopixelManager);
        pipButtons = new PipButtons(pipNeopixel);
        /* cc-end-constructor  */
    }
    ~CustomCode()
    {
        /* cc-start-destructor - insert your destructor code here before the end-destructor comment: */
        /* cc-end-destructor  */
    }
    void setup();
    void loop(String actualTimelineName, int actualTimelineStateId);
};
#endif
