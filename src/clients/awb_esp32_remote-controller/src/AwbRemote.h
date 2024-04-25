#ifndef _AWB_REMOTE_H_
#define _AWB_REMOTE_H_

#include <Arduino.h>


using byte = unsigned char;

class AwbRemote
{
protected:
public:
    AwbRemote()
    {

    }

    ~AwbRemote()
    {
        //delete _packetSenderReceiver;
    }

    void setup();
    void loop();
};

#endif