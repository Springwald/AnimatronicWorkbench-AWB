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

                // 4*2 bytes - clientId
                byte *clientIdArrRaw = ByteArrayConverter::UintTo4Bytes(_clientId);
                auto clientIdArr = ByteArrayConverter::SplitBytes4(clientIdArrRaw);
                for (int i = 0; i < 8; i++)
                    Serial.write(clientIdArr[i]);

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

    if (packetContent.length() < 18) // sender id (4*2 bytes), packet id (4*2 bytes), checksum (1*2 byte)
    {
        // packet content not long enough
        errorReceiving("Packet content not long enough");
        return;
    }

    // sender id
    byte senderIdArrRaw[8] = {packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++]}; // get 4*2 byte packet id
    byte *senderIdArr = ByteArrayConverter::UnSplitBytes8(senderIdArrRaw);                                                                                                                                     // split 8 bytes to 4 bytes
    unsigned int senderId = ByteArrayConverter::UintFrom4Bytes(senderIdArr);                                                                                                                                   // convert 4 bytes to int

    // packet id
    byte packetIdArrRaw[8] = {packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++], packetContent[pos++]}; // get 4 byte packet id
    byte *packetIdArr = ByteArrayConverter::UnSplitBytes8(packetIdArrRaw);                                                                                                                                     // split 8 bytes to 4 bytes
    unsigned int packetId = ByteArrayConverter::UintFrom4Bytes(packetIdArr);                                                                                                                                   // convert 4 bytes to int

    // get payload
    // get all bytes from _receiveBuffer except first 28 bytes (header 9, client id 4*2, packet type 1, packet id 4*2 checksum * 2) till end byte
    uint payloadLength = packetContent.length() - pos - 2;
    byte payloadArr[payloadLength];
    for (int i = 0; i < payloadLength; i++)
        payloadArr[i] = packetContent[i + pos];

    // get checksum
    byte checksumRaw[2] = {packetContent[packetContent.length() - 2], packetContent[packetContent.length() - 1]};
    byte checksum = ByteArrayConverter::UnSplitBytes2(checksumRaw);

    // calculate checksum
    byte checksumExpected = CalculateChecksumForDataPacket(payloadArr, payloadLength, packetIdArr);
    if (checksum != checksumExpected)
    {
        errorReceiving("check " + String(checksum) + "!=" + String(checksumExpected) + " packet " + String(packetId));
        sendResponsePacket(packetId, false, "response " + String(packetId) + ": check " + String(checksum) + "!=" + String(checksumExpected) + " packet " + String(packetId));
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
    // sendResponsePacket(packetId, true, "packet " + String(packetId) + " ok");
    sendResponsePacket(packetId, true, "ok");
}

void PacketSenderReceiver::sendResponsePacket(unsigned int packetId, bool ok, String message)
{
    // send response packet header with client id
    byte packetType = 3; // 3 = response packet
    this->sendPacketStart(packetType);

    // 4*2 bytes - packet id
    byte *packetIdArrRaw = ByteArrayConverter::UintTo4Bytes(packetId);
    auto packetIdArr = ByteArrayConverter::SplitBytes4(packetIdArrRaw);
    for (int i = 0; i < 8; i++)
        Serial.write(packetIdArr[i]);

    // 1 byte - ok
    Serial.write(ok ? (byte)1 : (byte)0);

    // message
    Serial.print(message);

    // 1*2 byte - checksum
    byte checksumRaw = CalculateChecksumForResponsePacket(message, packetIdArrRaw, ok);
    auto checksum = ByteArrayConverter::SplitByte(checksumRaw);
    Serial.write(checksum[0]);
    Serial.write(checksum[1]);

    free(packetIdArr);

    this->sendPacketEnd();
}
