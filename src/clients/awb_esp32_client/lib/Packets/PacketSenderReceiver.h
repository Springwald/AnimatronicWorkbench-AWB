#ifndef packet_sender_receiver_h
#define packet_sender_receiver_h

#include "Packet.h"
#include <Arduino.h>

#define PACKET_HEADER_START_BYTE 255
#define PACKET_HEADER_END_BYTE 254
#define ALIFE_PACKET_INTERVAL_MS 500

using byte = unsigned char;

/**
 * sends and receives packets over the serial port
 */
class PacketSenderReceiver
{
    using TCallBackPacketReceived = std::function<void(unsigned int, String)>;
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    unsigned int _clientId; /// the id of the client

    char *_packetHeader;        /// the packet header to identify the software sending the packet
    String _packetHeaderString; /// the packet header to identify the software sending the packet

    TCallBackPacketReceived _packetReceived;  /// callback function to call if a packet was received
    TCallBackErrorOccured _errorOccured;      /// callback function to call if an error occured
    unsigned long _next_alife_packet_to_send; /// the time when the next alife packet should be sent
    String _receiveBuffer;                    /// the buffer to store the received data

    /**
     * process a received data packet
     */
    void processDataPacket(String packetContent);

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
        _next_alife_packet_to_send = millis();

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
