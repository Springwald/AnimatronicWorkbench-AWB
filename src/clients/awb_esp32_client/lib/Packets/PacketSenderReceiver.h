#ifndef packet_sender_receiver_h
#define packet_sender_receiver_h

#include "Packet.h"
#include <Arduino.h>

#define PACKET_HEADER_START_BYTE 255
#define PACKET_HEADER_END_BYTE 254
#define ALIFE_PACKET_INTERVAL_MS 500

using byte = unsigned char;

class PacketSenderReceiver
{
    using TCallBackPacketReceived = std::function<void(unsigned int, String)>;
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    unsigned int _clientId;

    char *_packetHeader;
    String _packetHeaderString;

    TCallBackPacketReceived _packetReceived;
    TCallBackErrorOccured _errorOccured;
    unsigned long _next_alife_packet_to_send;
    String _receiveBuffer;

    void processDataPacket(String packetContent);
    void sendPacketHeader();
    void sendPacketStart(byte packetType);
    void sendPacketEnd();
    void sendResponsePacket(unsigned int packetId, bool ok, String message);
    void errorReceiving(String message);

public:
    PacketSenderReceiver(int clientId, char *packetHeader, TCallBackPacketReceived packetReceived, TCallBackErrorOccured errorOccured) : _clientId(clientId), _packetReceived(packetReceived), _errorOccured(errorOccured)
    {
        _next_alife_packet_to_send = millis();

        _packetHeader = new char[9]{
            (char)PACKET_HEADER_START_BYTE,
            (char)PACKET_HEADER_START_BYTE,
            (char)PACKET_HEADER_START_BYTE,
            (char)packetHeader[0],
            (char)packetHeader[1],
            (char)packetHeader[2],
            (char)PACKET_HEADER_END_BYTE,
            (char)PACKET_HEADER_END_BYTE,
            (char)PACKET_HEADER_END_BYTE};

        _packetHeaderString = String(_packetHeader).substring(0, 9);
    }
    bool loop();
};

#endif
