#ifndef _CUSTOM_CODE_H_
#define _CUSTOM_CODE_H_

#include <Arduino.h>
#include <String.h>

/*
    Enter your custom code in this header file and the corresponding cpp file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

class CustomCode
{
protected:

    /* cc-start-protected - insert your protected code here before the end-protected comment: */

    /* cc-end-protected  */

public:
    CustomCode() : neopixelManager()
    {
        /* cc-start-constructor - insert your protected code here before the end-constructor comment: */
        /* cc-end-constructor  */
    }

    ~CustomCode()
    {
        /* cc-start-destrucutor - insert your protected code here before the end-destrucutor comment: */
        /* cc-end-destrucutor  */
    }

    void setup();
    void loop();
};

#endif