
#include <Arduino.h>
#include "CustomCode.h"
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

    /* cc-end-setup  */
}

void CustomCode::loop(String actualTimelineName, int actualTimelineStateId)
{
    /* cc-start-loop - insert your loop code here before the end-loop comment: */

    _mp3PlayerDfPlayerMiniManager->playSound(0, 1);

    /* cc-end-loop  */
}

/* cc-start-functions - insert your functions here before the end-functions comment: */

/* cc-end-functions  */
