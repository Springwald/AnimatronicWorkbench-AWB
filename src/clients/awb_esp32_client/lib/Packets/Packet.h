#ifndef packet_h
#define packet_h

using byte = unsigned char;

/**
 * a packet to send over the serial port
 */
class Packet
{
public:
    /**
     * the id of the packet sender
     */
    unsigned int senderId;

    /**
     * the id of the packet
     */
    unsigned int id;
    String payload;
};

#endif
