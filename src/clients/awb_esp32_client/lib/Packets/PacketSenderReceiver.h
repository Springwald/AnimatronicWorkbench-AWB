#ifndef packet_sender_receiver_h
#define packet_sender_receiver_h

#include "Packet.h"
#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

#define PACKET_BUFFER_SIZE 32 * 1024

/**
 * sends and receives packets over the serial port
 */
class PacketSenderReceiver
{
    using TCallBackPacketReceived = std::function<String(unsigned int, String)>;
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    const byte ALIVE_PACKET_REQUEST_BYTE = 0xFF; /// alive packet request byte, is sent by the studio to the client to check if the client is available

    const int PacketTypeNotSet = 0;         /// Fallback type for unknown packets.
    const int PacketTypeAlivePacket = 1;    /// Send by a client to by scanned by the server to check if the client is available
    const int PacketTypeResponsePacket = 2; ///  A packet that tells the server if a packet was received successfully or not
    const int PacketTypePayloadPacket = 3;  ///  A packet that contains data to be processed by the server or client

    unsigned int _clientId; /// the id of the client

    bool _insidePacketStream;   /// true if we are inside a packet stream, false otherwise
    String _packetHeaderString; /// the packet header to identify the software sending the packet
    String _packetFooterString; /// the packet header to identify the software sending the packet

    TCallBackPacketReceived _packetReceived; /// callback function to call if a packet was received
    TCallBackErrorOccured _errorOccured;     /// callback function to call if an error occured

    const int _receiveBufferSize = PACKET_BUFFER_SIZE;
    unsigned char _receiveBuffer[PACKET_BUFFER_SIZE];
    int _receiveBufferCount = 0;

    unsigned long _lastTicketSentMs = millis(); /// the last time a ticket was sent to the serial port

    bool receiveBufferEndsWith(String findWhat);

    /**
     * process a received data packet
     */
    void processDataPacket(u_int length);

    /**
     * send a packet to the serial port to inform the other side that the last packet was received
     */
    void sendResponsePacket(unsigned int packetId, uint packetType, bool ok, String message);

    /**
     * notity an error
     */
    void errorReceiving(String message);

    uint calculateChecksumForDataPacket(String payload);

public:
    PacketSenderReceiver(int clientId, String packetHeader, String packerFooter, TCallBackPacketReceived packetReceived, TCallBackErrorOccured errorOccured) : _clientId(clientId),
                                                                                                                                                               _packetHeaderString(packetHeader),
                                                                                                                                                               _packetFooterString(packerFooter),
                                                                                                                                                               _packetReceived(packetReceived),
                                                                                                                                                               _errorOccured(errorOccured)
    {
    }

    /**
     * the packet sender / Receiver loop
     */
    bool loop();
};

#endif
