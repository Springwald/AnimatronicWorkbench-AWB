#include "Arduino.h"
#include "PacketSenderReceiver.h"
#include "ByteArrayConverter.h"
#include <ArduinoJson.h>

using byte = unsigned char;

StaticJsonDocument<PACKET_BUFFER_SIZE> jsondocPacketSenderReceiver;

bool PacketSenderReceiver::loop()
{
    bool receivedPacket = false;

    // receive data
    for (int i = 0; i < 1000; i++)
    {
        if (Serial.available() > 0)
        {
            char value = Serial.read();

            if (value == ALIVE_PACKET_REQUEST_BYTE) // the request for an alive packet
            {
                sendResponsePacket(0, PacketTypeAlivePacket, true, String(_clientId)); // send the alive packet
                _lastTicketSentMs = millis();
                _receiveBufferCount = 0;
                _insidePacketStream = false;
                continue;
            }

            // add the received byte to the receive buffer
            _receiveBuffer[_receiveBufferCount] = value;
            _receiveBufferCount++;

            if (_receiveBufferCount >= _receiveBufferSize)
            {
                // buffer is full, reset it
                _errorOccured("Receive buffer is full, resetting it!");
                _receiveBufferCount = 0;
                _insidePacketStream = false;
                continue;
            }

            if (receiveBufferEndsWith(_packetHeaderString))
            {
                // Start of a packet stream received
                if (_insidePacketStream)
                    _errorOccured("Received packet header " + _packetHeaderString + " while already inside a packet stream!");
                _receiveBufferCount = 0;
                _insidePacketStream = true;
                continue;
            }

            if (receiveBufferEndsWith(_packetFooterString)) // the end of a packet stream is received
            {
                if (_insidePacketStream == false)
                {
                    // we received a packet footer without a start packet header
                    _errorOccured("Received packet footer without a start packet header!");
                    _receiveBufferCount = 0;
                    _insidePacketStream = false;
                    continue;
                }

                String packetContentString = String(_receiveBuffer, _receiveBufferCount - _packetFooterString.length());

                _receiveBufferCount = 0;
                _insidePacketStream = false;

                if (packetContentString.length() == 0)
                {
                    // Reveived an empty packet?!? ignore it
                    _errorOccured("Received an empty packet (only packet header and footer)!");
                    continue;
                }

                // load the packet content string as json
                jsondocPacketSenderReceiver.clear();
                DeserializationError error = deserializeJson(jsondocPacketSenderReceiver, packetContentString);
                if (error)
                {
                    // we could not deserialize the packet content
                    auto errMsg = String("Could not deserialize packet content '") + packetContentString + "' with error '" + error.c_str() + "'";
                    _errorOccured(errMsg);
                    continue;
                }

                uint packetId = jsondocPacketSenderReceiver["Id"].as<uint>();
                uint checksum = jsondocPacketSenderReceiver["Checksum"].as<uint>();
                uint packetType = jsondocPacketSenderReceiver["PacketType"].as<uint>();
                String payload = jsondocPacketSenderReceiver["Payload"].as<String>();

                if (packetId == 0 || packetType == 0)
                {
                    // we could not deserialize the packet content
                    auto errMsg = String("Could not deserialize packet content '") + packetContentString + "' with error 'Invalid packet content'";
                    _errorOccured(errMsg);
                    continue;
                }

                String response;

                // switch packet type
                switch (packetType)
                {
                case 1:                                                                              // alife packet
                    _errorOccured("Received alive packet from AWB Studio, this should not happen!"); // this should not happen, we are the client
                    break;

                case 2: // packet response packet
                    _errorOccured("received response type " + String(packetType) + "; not implemented yet!");
                    break;

                case 3: // payload data packet
                    receivedPacket = true;
                    response = _packetReceived(_clientId, payload);
                    // send response packet
                    sendResponsePacket(packetId, PacketTypeResponsePacket, true, response);
                    _lastTicketSentMs = millis();
                    break;

                case 4:  // read values packet
                default: // unknown packet type
                    break;
                }

                // clear receive buffer
                _receiveBufferCount = 0;
            }
        }
    }

    if (millis() > _lastTicketSentMs + 500)
    {
        // sendResponsePacket(0, PacketTypeAlivePacket, true, String(_clientId));
        // _lastTicketSentMs = millis();
    }

    return receivedPacket;
}

bool PacketSenderReceiver::receiveBufferEndsWith(String findWhat)
{
    if (_receiveBufferCount < findWhat.length())
        return false;

    for (int i = 0; i < findWhat.length(); i++)
    {
        if (_receiveBuffer[_receiveBufferCount - i - 1] != findWhat[findWhat.length() - i - 1])
            return false;
    }
    return true;
}

void PacketSenderReceiver::errorReceiving(String message)
{
    _errorOccured(message);
}

static int CalculateChecksumForDataPacket(String payload)
{

    int result = 0;
    // add all bytes from payload
    for (int i = 0; i < payload.length(); i++)
        result += payload[i];
    return result;
}

void PacketSenderReceiver::sendResponsePacket(unsigned int packetId, uint packetType, bool ok, String message)
{
    jsondocPacketSenderReceiver.clear();
    jsondocPacketSenderReceiver["Id"] = packetId;
    jsondocPacketSenderReceiver["ClientId"] = _clientId;
    jsondocPacketSenderReceiver["Checksum"] = CalculateChecksumForDataPacket(message);
    jsondocPacketSenderReceiver["PacketType"] = packetType;
    jsondocPacketSenderReceiver["Payload"] = message;

    // serialize the json document to a string
    String jsonString;
    serializeJson(jsondocPacketSenderReceiver, jsonString);

    Serial.write(_packetHeaderString.c_str(), _packetHeaderString.length());
    Serial.write(jsonString.c_str(), jsonString.length());
    Serial.write(_packetFooterString.c_str(), _packetFooterString.length());
}
