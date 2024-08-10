
#include <Arduino.h>
#include "CustomCode.h"
#include <Actuators/NeopixelManager.h>

/*
    Enter your custom code in this cpp file and the corresponding header file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

void CustomCode::setup()
{
    /* cc-start-setup - insert your setup code here before the end-setup comment: */

    this->pipButtons->setup();
    this->pipNeopixel->setup();


    /* cc-end-setup  */
}

void CustomCode::loop(String actualTimelineName, int actualTimelineStateId)
{
    /* cc-start-loop - insert your loop code here before the end-loop comment: */

    pipButtons->loop(); // call the loop function of the pipButtons to update the button states

    // check if a button is pressed and set e.g. the action to play a timeline
    if (pipButtons->isButtonPressed(pipButtons->btnSleep))
    {
        timelineNameToPlay = new String("Go Sleep");
    }

    pipNeopixel->loop(); // call the loop function of the pipNeopixel to update the neopixel states

    /* neopixelManager->setSingleLED(0, 128, 0, 0);
    neopixelManager->setSingleLED(1, 128, 0, 0);
    neopixelManager->setSingleLED(2, 128, 128, 0);
    neopixelManager->setSingleLED(3, 128, 128, 0);
    neopixelManager->setSingleLED(4, 0, 128, 0);
    neopixelManager->setSingleLED(5, 0, 128, 0);
    neopixelManager->setSingleLED(6, 0, 0, 128);
    neopixelManager->setSingleLED(7, 0, 0, 128);*/

    // working touch pins on ESP-32-WROOM: 4, 12, 13, 14, 27, 32, 33

    /*setButtonLightByTouch(0, 32);
     setButtonLightByTouch(1, 33);
     setButtonLightByTouch(2, 27);
     setButtonLightByTouch(3, 14);

 */


    /* cc-end-loop  */
}

/* cc-start-functions - insert your functions here before the end-functions comment: */

void CustomCode::setButtonLightByTouch(uint16_t btnIndex, uint8_t touchPin)
{
    auto touchIn = touchRead(touchPin);
    if (touchIn < 40)
    {
        neopixelManager->setSingleLED(btnIndex * 2, 64, 0, 0);
        neopixelManager->setSingleLED(btnIndex * 2 + 1, 64, 0, 0);
    }
    else
    {
        neopixelManager->setSingleLED(btnIndex * 2, 0, 0, 12);
        neopixelManager->setSingleLED(btnIndex * 2 + 1, 0, 0, 12);
    }

    for (int i = 8; i < 17; i++)
    {
        neopixelManager->setSingleLED(i, 2, 2, 2);
    }
}


/* cc-end-functions  */

