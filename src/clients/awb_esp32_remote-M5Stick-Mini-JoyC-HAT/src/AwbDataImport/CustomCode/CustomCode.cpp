
#include <Arduino.h>
#include "CustomCode.h"
#include <Hardware.h>

/*
    Enter your custom code in this cpp file and the corresponding header file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

void CustomCode::setup()
{
    /* cc-start-setup - insert your setup code here before the end-setup comment: */

    /* cc-end-setup  */
}

void CustomCode::loop(int8_t joyPosX, int8_t joyPosY, bool joyButton, bool button2, bool button3)
{
    /* cc-start-loop - insert your loop code here before the end-loop comment: */

    if (joyPosY > 50)
        this->_commandSender->playTimeline("YES");
    if (joyPosY < -50)
        this->_commandSender->playTimeline("NO");
    if (joyPosX > 50)
        this->_commandSender->playTimeline("LookUpRight");
    if (joyPosX < -50)
        this->_commandSender->playTimeline("LookUpMiddle");

    if (joyButton == true)
        this->_commandSender->playTimeline("Wink");
    if (button2 == true)
        this->_commandSender->playTimeline("The+Force+raw");
    if (button3 == true)
        this->_commandSender->playTimeline("Stand+-+Dance");

    /* cc-end-loop  */
}

/* cc-start-functions - insert your functions here before the end-functions comment: */

/* cc-end-functions  */
