#include <Arduino.h>
#include "CustomCode.h"
#include "../AwbDataImport/ProjectData.h"
#include <Actuators/NeopixelManager.h>
/*
    Enter your custom code in this cpp file and the corresponding header file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/
/* cc-start-include - insert your include code here before the end-protected comment: */
/* cc-end-include - insert your include code here before the end-protected comment: */
void CustomCode::setup()
{
    /* cc-start-setup - insert your setup code here before the end-setup comment: */
    this->pipButtons->setup();
    this->pipNeopixel->setup();
    this->pipNeopixel->setEyeState(PipNeopixel::BigEyeStateOff);
    timelineNameToPlay = new String(this->_projectData->TimelineName_StartUp);
    // set docked pin as input
    pinMode(dockedPin, INPUT);
    lastUpdateMs = millis();

    /* cc-end-setup  */
}
void CustomCode::loop(String actualTimelineName, int actualTimelineStateId)
{
    /* cc-start-loop - insert your loop code here before the end-loop comment: */
    auto diff = millis() - lastUpdateMs;
    lastUpdateMs = millis();
    int bigEyeState = PipNeopixel::BigEyeStateDefault;
    pipButtons->loop();                                   // call the loop function of the pipButtons to update the button states
    pipNeopixel->loop();                                  // call the loop function of the pipNeopixel to update the neopixel states
    if (actualTimelineStateId == CustomCode::idleStateId) // check if the idle state is active
    {
        const int sleepAfterMinutes = 5;
        idleStateDurationMs += diff;
        if (idleStateDurationMs > sleepAfterMinutes * 60000)
        {
            // play the timeline "Go Sleep" after 60 seconds of idle time
            timelineNameToPlay = new String(this->_projectData->TimelineName_GoSleep);
            idleStateDurationMs = 0;
        }
    }
    else
        idleStateDurationMs = 0;
    if (actualTimelineName == this->_projectData->TimelineName_Sleeping)
    {
        sleepingTime += diff;
        if (sleepingTime > 5000) // if the timeline "Sleeping" is active for more than 30 seconds
        {
            for (int servoChannel = 1; servoChannel <= 4; servoChannel++) // turn off all neck and ear servos
                this->_stSerialServoManager->setTorque(servoChannel, 0);
        }
        bigEyeState = PipNeopixel::BigEyeStateSleeping;
        auto holdTime = pipButtons->buttonHoldTimeMs(pipButtons->btnHoldBack);
        if (wokeUpByBackHold)
        {
            if (holdTime < 1000) // if the button "Hold Back" is released
                wokeUpByBackHold = false;
        }
        else
        {
            if (holdTime > 3000) // if the button "Hold Back" is pressed for more than 2 seconds
            {
                this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 29);
                timelineNameToPlay = new String(this->_projectData->TimelineName_Wakeup);
                wokeUpByBackHold = true;
            }
        }
    }
    else
    {
        sleepingTime = 0;
    }
    if (actualTimelineName == this->_projectData->TimelineName_GoSleep)
        bigEyeState = PipNeopixel::BigEyeStateGoSleep;
    if (actualTimelineName == this->_projectData->TimelineName_Evil)
        bigEyeState = PipNeopixel::BigEyeStateEvil;
    if (actualTimelineName == this->_projectData->TimelineName_Rainbow)
        bigEyeState = PipNeopixel::BigEyeStateRainbow;
    checkButtons(actualTimelineName, actualTimelineStateId);
    pipNeopixel->setEyeState(bigEyeState);
    /* cc-end-loop  */
}
/* cc-start-functions - insert your functions here before the end-functions comment: */
void CustomCode::checkButtons(String actualTimelineName, int actualTimelineStateId)
{
    // check if a button is pressed and set e.g. the action to play a timeline
    if (pipButtons->isButtonPressed(pipButtons->btnSleep))
    {
        this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 3);
        if (actualTimelineName == this->_projectData->TimelineName_Sleeping || actualTimelineName == this->_projectData->TimelineName_GoSleep) // if the timeline "Sleeping" is active
            timelineNameToPlay = new String(this->_projectData->TimelineName_Wakeup);
        else
            timelineNameToPlay = new String(this->_projectData->TimelineName_GoSleep);
    }
    if (pipButtons->isButtonPressed(pipButtons->btnRainbow))
    {
        this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 4);
        timelineNameToPlay = new String(this->_projectData->TimelineName_Rainbow);
    }
    // check if a button is pressed and set e.g. the action to play a timeline
    if (pipButtons->isButtonPressed(pipButtons->btnEvil))
    {
        this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 9);
        timelineNameToPlay = new String(this->_projectData->TimelineName_Evil);
    }
    // check if docked into the charger
    //_errorOccured(String(analogRead(dockedPin)));
    if (false)
    {
        if (digitalRead(dockedPin) == HIGH)
        {
            if (!isDocked)
                this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 2); // dock into charger sound
            isDocked = true;
        }
        else
        {
            if (isDocked)
                this->_mp3PlayerManager->playSound(this->_projectData->Mp3PlayerName_PipVoiceSound, 1); // undock from charger sound
            isDocked = false;
        }
    }
}
/* cc-end-functions  */
