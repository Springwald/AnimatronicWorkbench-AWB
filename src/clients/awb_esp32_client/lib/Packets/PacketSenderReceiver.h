#ifndef packet_sender_receiver_h
#define packet_sender_receiver_h

#include "Packet.h"
#include <Arduino.h>
#include <String.h>

#define PACKET_HEADER_START_BYTE 250
#define PACKET_HEADER_END_BYTE 251
#define REQUEST_ALIFE_PACKET_BYTE 252

using byte = unsigned char;

#define PACKET_BUFFER_SIZE 4000

/**
 * sends and receives packets over the serial port
 */
class PacketSenderReceiver
{
    using TCallBackPacketReceived = std::function<String(unsigned int, String)>;
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    unsigned int _clientId; /// the id of the client

    char *_packetHeader;        /// the packet header to identify the software sending the packet
    String _packetHeaderString; /// the packet header to identify the software sending the packet

    TCallBackPacketReceived _packetReceived; /// callback function to call if a packet was received
    TCallBackErrorOccured _errorOccured;     /// callback function to call if an error occured

    const int _receiveBufferSize = PACKET_BUFFER_SIZE;
    unsigned char _receiveBuffer[PACKET_BUFFER_SIZE];
    int _receiveBufferCount = 0;

    /**
     * process a received data packet
     */
    void processDataPacket(u_int length);

    /**
     * send a packet to the serial port
     */
    void sendPacketHeader();

    /**
     * send the packet start bytes to the serial port
     */
    void sendPacketStart(byte packetType);

    /**
     * send the packet end bytes to the serial port
     */
    void sendPacketEnd();

    /**
     * send a packet to the serial port to inform the other side that the last packet was received
     */
    void sendResponsePacket(unsigned int packetId, bool ok, String message);

    /**
     * notity an error
     */
    void errorReceiving(String message);

public:
    PacketSenderReceiver(int clientId, char *packetHeader, TCallBackPacketReceived packetReceived, TCallBackErrorOccured errorOccured) : _clientId(clientId), _packetReceived(packetReceived), _errorOccured(errorOccured)
    {
        _packetHeader = new char[10]{
            (char)PACKET_HEADER_START_BYTE,
            (char)PACKET_HEADER_START_BYTE,
            (char)PACKET_HEADER_START_BYTE,
            (char)packetHeader[0],
            (char)packetHeader[1],
            (char)packetHeader[2],
            (char)PACKET_HEADER_END_BYTE,
            (char)PACKET_HEADER_END_BYTE,
            (char)PACKET_HEADER_END_BYTE,
            0};

        _packetHeaderString = String(_packetHeader);
    }

    /**
     * the packet sender / Receiver loop
     */
    bool loop();
};

#endif
