#ifndef packet_sender_receiver_h
#define packet_sender_receiver_h

#include "Packet.h"
#include <Arduino.h>
#include <String.h>

using byte = unsigned char;

#define PACKET_BUFFER_SIZE 6000

/**
 * sends and receives packets over the serial port
 */
class PacketSenderReceiver
{
    using TCallBackPacketReceived = std::function<String(unsigned int, String)>;
    using TCallBackErrorOccured = std::function<void(String)>;

private:
    /// <summary>
    /// Fallback type for unknown packets.
    /// </summary>
    const int PacketTypeNotSet = 0;
    /// <summary>
    /// Send by a client to by scanned by the server to check if the client is available
    /// </summary>
    const int PacketTypeAlivePacket = 1;
    /// <summary>
    /// A packet that tells the server if a packet was received successfully or not
    /// </summary>
    const int PacketTypeResponsePacket = 2;
    /// <summary>
    /// A packet that contains data to be processed by the server or client
    /// </summary>
    const int PacketTypePayloadPacket = 3;

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
