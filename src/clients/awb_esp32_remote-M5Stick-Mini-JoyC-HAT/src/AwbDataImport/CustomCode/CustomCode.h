#ifndef _CUSTOM_CODE_H_
#define _CUSTOM_CODE_H_

#include <Arduino.h>
#include <String.h>
#include <CommandSender.h>
#include "../lib/M5Unit-MiniJoyC/UNIT_MiniJoyC.h"
#include <AXP192.h>

/*
    Enter your custom code in this header file and the corresponding cpp file.
    Only write code beween the cc-start and cc-end comments, otherwise it will be overwritten and lost.
*/

class CustomCode
{
protected:
    AwbDisplay _display; /// The display, oled or lcd
    CommandSender *_commandSender;

    /* cc-start-protected - insert your protected code here before the end-protected comment: */

    /* cc-end-protected  */

public:
    CustomCode(AwbDisplay display, CommandSender *commandSender) : _display(display), _commandSender(commandSender)
    {
        /* cc-start-constructor - insert your constructor code here before the end-constructor comment: */
        /* cc-end-constructor  */
    }

    ~CustomCode()
    {
        /* cc-start-destructor - insert your destructor code here before the end-destrucutor comment: */
        /* cc-end-destructor  */
    }

    void setup();
    void loop(int8_t joyPosX, int8_t joyPosY, bool joyButton, bool button2, bool button3);
};

#endif