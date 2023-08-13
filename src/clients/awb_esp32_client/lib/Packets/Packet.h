#ifndef packet_h
#define packet_h

using byte = unsigned char;

class Packet
{
public:
    unsigned int senderId;
    unsigned int id;
    String payload;
    String *payload2;
};

#endif
