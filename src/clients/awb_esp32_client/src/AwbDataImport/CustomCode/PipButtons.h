#ifndef _PIP_BUTTONS_H_
#define _PIP_BUTTONS_H_

#include <Arduino.h>
#include <String.h>
#include <Actuators/NeopixelManager.h>

/*
    Enter your custom code in this header file and the corresponding cpp file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

class PipButtons
{
protected:
    NeopixelManager *neopixelManager;

    /* cc-start-protected - insert your protected code here before the end-protected comment: */

    void setButtonLightByTouch(uint16_t btnIndex, uint8_t touchPin);














    /* cc-end-protected  */

public:
    PipButtons(NeopixelManager *neopixelManager) : neopixelManager(neopixelManager)
    {
        /* cc-start-constructor - insert your protected code here before the end-constructor comment: */














        /* cc-end-constructor  */
    }

    ~PipButtons()
    {
        /* cc-start-destructor - insert your protected code here before the end-destrucutor comment: */














        /* cc-end-destructor  */
    }

    void setup();
    void loop();
};

#endif











