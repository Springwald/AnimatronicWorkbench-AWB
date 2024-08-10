#ifndef _CUSTOM_CODE_H_
#define _CUSTOM_CODE_H_

#include <Arduino.h>
#include <String.h>
#include <Actuators/NeopixelManager.h>

/* cc-start-include - insert your include code here before the end-protected comment: */
/* cc-end-include - insert your include code here before the end-protected comment: */

/*
    Enter your custom code in this header file and the corresponding cpp file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

class CustomCode
{
protected:
    NeopixelManager *neopixelManager;

    /* cc-start-protected - insert your protected code here before the end-protected comment: */
    /* cc-end-protected  */

public:
    String *timelineNameToPlay = nullptr;         /// The name of the timeline to play by custom code. Beware: This is excuted immediately and will overwrite the current timeline and will interrupt the current timeline movements.
    int *soundNoToPlay = nullptr;                 /// The number of the sound to play by custom code
    int *timelineStateToForceOnce = nullptr;      /// Here the globale timeline state can be overwritten by custom code a single time. Buttons and other inputs defined in AWB Studio will be ignored.
    int *timelineStateToForcePermanent = nullptr; /// Here the globale timeline state can be overwritten by custom code permanent. Buttons and other inputs defined in AWB Studio will be ignored.

    CustomCode(NeopixelManager *neopixelManager) : neopixelManager(neopixelManager)
    {
        /* cc-start-constructor - insert your constructor code here before the end-constructor comment: */
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
