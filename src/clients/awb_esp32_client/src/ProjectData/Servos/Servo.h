#ifndef _SERVO_H_
#define _SERVO_H_

#include <Arduino.h>
#include <String.h>
#include <vector>
#include <string>

class Servo
{

public:
    String id;            /// the project wide unique id of the servo
    String title;         /// the name of the servo
    bool globalFault;     /// if this servo is to hot or overloaded, should all servos stop?
    bool isFault = false; /// if this servo is to hot or overloaded

    Servo(String const id, String const title, bool globalFault) : id(id), title(title), globalFault(globalFault)
    {
    }

    ~Servo()
    {
    }
};

#endif