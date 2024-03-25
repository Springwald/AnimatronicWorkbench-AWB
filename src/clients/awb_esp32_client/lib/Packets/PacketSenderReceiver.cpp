#include "Arduino.h"
#include "PacketSenderReceiver.h"
#include "ByteArrayConverter.h"

using byte = unsigned char;

bool PacketSenderReceiver::loop()
{
    bool receivedPacket = false;

    // receive data
    for (int i = 0; i < 1000; i++)
    {
        if (Serial.available() > 0)
        {
            char value = Serial.read();

            if (value == REQUEST_ALIFE_PACKET_BYTE) // request for alife packet
            {
                // send alife packet header with client id
                byte packetType = 1; // 1 = alife packet
                this->sendPacketStart(packetType);

                // 4 bytes - clientId
                byte *clientIdArr = ByteArrayConverter::UintTo4Bytes(_clientId);
                Serial.write(clientIdArr[0]);
                Serial.write(clientIdArr[1]);
                Serial.write(clientIdArr[2]);
                Serial.write(clientIdArr[3]);
                free(clientIdArr);

                this->sendPacketEnd();
            }
            else
            {
                // normal data received
                _receiveBuffer += value;

                // check if _receiveBuffer ends with _packetHeader
                if (_receiveBuffer.endsWith(_packetHeaderString))
                // if (StringToolbox::stringEndsWith(_receiveBuffer, _packetHeader))
                {
                    // packet start/end byte found
                    if (_receiveBuffer.length() <= 9)
                    {
                        // only packet header received
                        _receiveBuffer = "";
                        return false;
                    }

                    // get packet type
                    byte packetType = _receiveBuffer[0];

                    // switch packet type
                    switch (packetType)
                    {

                    case 1: // alife packet
                        break;

                    case 2:                                                                          // data packet
                        processDataPacket(_receiveBuffer.substring(1, _receiveBuffer.length() - 9)); // remove packet header and packet type
                        receivedPacket = true;
                        break;

                    case 3: // packet response packet
                        break;

                    default: // unknown packet type
                        break;
                    }

                    // clear receive buffer
                    _receiveBuffer = "";
                }
            }
        }
    }
    return receivedPacket;
}

void PacketSenderReceiver::errorReceiving(String message)
{
    _errorOccured(message);
}

void PacketSenderReceiver::sendPacketHeader()
{
    for (int i = 0; i < 9; i++)
        Serial.write(_packetHeader[i]); // 9 bytes - packet header
}

void PacketSenderReceiver::sendPacketStart(byte packetType)
{
    sendPacketHeader();
    Serial.write(packetType); // 1 byte - packet type: 1 = alife packet, 2 = data packet, 3 = response packet
}

void PacketSenderReceiver::sendPacketEnd()
{
    sendPacketHeader();
}

static byte CalculateChecksumForDataPacket(byte payload[], int payloadLength, byte packetId[4])
{
    byte result = 0;
    // add all bytes from payload
    for (int i = 0; i < payloadLength; i++)
        result += payload[i];
    // add all bytes from packet id
    for (int i = 0; i < 4; i++)
        result += packetId[i];
    return result;
}

static byte CalculateChecksumForResponsePacket(String message, byte packetId[4], bool ok)
{
    byte result = 0;
    // add all bytes from message
    for (int i = 0; i < message.length(); i++)
        result += message[i];
    // add all bytes from packet id
    for (int i = 0; i < 4; i++)
        result += packetId[i];
    // add ok byte
    result += ok ? (byte)1 : (byte)0;
    return result;
}

void PacketSenderReceiver::processDataPacket(String packetContent)
{
    int pos = 0;

    if (packetContent.length() < 9) // sender id (4 bytes), packet id (4 bytes), checksum (1 byte)
    {
        // packet content not long enough
        errorReceiving("Packet content not long enough");
        return;
    }

    // sender id
    byte senderIdArr[4] = {packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++]}; // get 4 byte packet id
    unsigned int senderId = ByteArrayConverter::UintFrom4Bytes(senderIdArr);                                        // convert 4 bytes to int

    // packet id
    byte packetIdArr[4] = {packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++]}; // get 4 byte packet id
    unsigned int packetId = ByteArrayConverter::UintFrom4Bytes(packetIdArr);                                        // convert 4 bytes to int

    // get payload
    // get all bytes from _receiveBuffer except first 13 bytes (header, client id, packet type, packet id, checksum) till end byte
    uint payloadLength = packetContent.length() - pos - 1;
    byte payloadArr[payloadLength];
    for (int i = 0; i < payloadLength; i++)
        payloadArr[i] = packetContent[i + pos];

    // get checksum
    byte checksum = packetContent[packetContent.length() - 1];

    // calculate checksum
    byte checksumExpected = CalculateChecksumForDataPacket(payloadArr, payloadLength, packetIdArr);
    if (checksum != checksumExpected)
    {
        errorReceiving("check " + String(checksum) + "!=" + String(checksumExpected) + " packet " + String(packetId));
        return; // checksum not valid
    }

    // create received data packet
    Packet *packet = new Packet();
    packet->senderId = senderId;
    packet->id = packetId;
    String packetString = String(payloadArr, payloadLength);

    // if (false)
    {
        // packet->payload = packetString;
        //  call packet received callback
        _packetReceived(_clientId, packetString);
    }

    free(packet);

    // send response packet
    sendResponsePacket(packetId, true, "ok");
}

void PacketSenderReceiver::sendResponsePacket(unsigned int packetId, bool ok, String message)
{
    // send response packet header with client id
    byte packetType = 3; // 3 = response packet
    this->sendPacketStart(packetType);

    // 4 bytes - packet id
    byte *packetIdArr = ByteArrayConverter::UintTo4Bytes(packetId);
    Serial.write(packetIdArr[0]);
    Serial.write(packetIdArr[1]);
    Serial.write(packetIdArr[2]);
    Serial.write(packetIdArr[3]);

    // 1 byte - ok
    Serial.write(ok ? (byte)1 : (byte)0);

    // message
    Serial.print(message);

    // 1 byte - checksum
    byte checksum = CalculateChecksumForResponsePacket(message, packetIdArr, ok);
    Serial.write(checksum);

    free(packetIdArr);

    this->sendPacketEnd();
}
