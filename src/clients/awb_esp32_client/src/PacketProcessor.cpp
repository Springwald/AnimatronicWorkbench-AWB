#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include <ArduinoJson.h>
#include "WlanConnector.h"
#include "PacketProcessor.h"

StaticJsonDocument<PACKET_BUFFER_SIZE> jsondocPacketProcessor;

/**
 * process a received packet from the Animatronic Workbench Studio
 */
String PacketProcessor::processPacket(String payload)
{
    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 0);

    boolean sendServoUpdateDirectly = true; // should the servo update directly send to the servo controller or via the project data? If set to false, the project data is used to set the target value of the servo. Servos not defined in the project data are ignored.

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 1);

    DeserializationError error = deserializeJson(jsondocPacketProcessor, payload);

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 2);
    if (error)
    {
        // packet content is not valid json
        //_errorOccured("json:" + String(error.c_str()));
        return String("json:") + String(error.c_str()) + " " + payload;
    }

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 3);

    // #### READ VALUES ####

    if (jsondocPacketProcessor.containsKey("ReadValue")) // AWB studio requests a value from the client e.g. Servo Position
    {
        // read a value from the client
        String typeName = jsondocPacketProcessor["ReadValue"]["TypeName"];
        if (typeName == nullptr)
        {
            // should not happen, instead the whole ReadValue should be missing
            _errorOccured("ReadValue?!? " + payload);
            return String("ReadValue but no type received?!? " + payload);
        }
        else
        {
            if (typeName == "ScsServo")
            {
                // read a value from the SCS bus servo
                u8 id = jsondocPacketProcessor["ReadValue"]["Id"];
                if (this->_scSerialServoManager != nullptr)
                {
                    int value = this->_scSerialServoManager->readPosition(id);
                    // send the value back to the AWB studio
                    String response = "{\"ScsServo\":{\"Id\":" + String(id) + ",\"Position\":" + String(value) + "}}";
                    return response;
                }
                else
                {
                    _errorOccured("ScsServoManager not configured!");
                    return String("ScsServoManager not configured!");
                }
            }

            if (typeName = "StsServo")
            {
                // read a value from the STS bus servo
                u8 id = jsondocPacketProcessor["ReadValue"]["Id"];
                if (this->_stSerialServoManager != nullptr)
                {
                    int value = this->_stSerialServoManager->readPosition(id);
                    // send the value back to the AWB studio
                    String response = "{\"StsServo\":{\"Id\":" + String(id) + ",\"Position\":" + String(value) + "}}";
                    return response;
                }
                else
                {
                    _errorOccured("StsServoManager not configured!");
                    return String("StsServoManager not configured!");
                }
            }

            _errorOccured("ReadValue type not supported: " + typeName);
            return String("ReadValue type not supported: " + typeName);
        }
    }

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 10);

    // #### DISPLAY MESSAGE ####

    if (jsondocPacketProcessor.containsKey("DispMsg")) // packat contains a display message
    {
        const char *message = jsondocPacketProcessor["DispMsg"]["Msg"];
        if (message == nullptr)
        {
            // should not happen, instead the whole DispMsg should be missing
            _errorOccured("DispMsg?!? " + payload);
        }
        else
        {
            int duration = jsondocPacketProcessor["DispMsg"]["Ms"];
            if (duration < 100)
                duration = 5000; // default duration
            _messageToShow(message, duration);
        }
    }

    // #### RECEIVE VALUES ####

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 20);

    if (jsondocPacketProcessor.containsKey("Pca9685Pwm")) // packet contains Pca9685 PWM driver data
    {
        JsonArray servos = jsondocPacketProcessor["Pca9685Pwm"]["Servos"];
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_pca9685PwmManager == nullptr)
            {
                _errorOccured("Pca9685Pwm not configured!");
                return String("Pca9685Pwm not configured!");
            }
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 27);

            // store the method showMsg in an anonymous function
            _pca9685PwmManager->setTargetValue(channel, value, name);

            _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 28);
        }
    }
    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 29);
    if (this->_pca9685PwmManager != nullptr)
        _pca9685PwmManager->updateActuators(false);

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 30);

    if (jsondocPacketProcessor.containsKey("STS")) // package contains STS bus servo data
    {
        JsonArray servos = jsondocPacketProcessor["STS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_stSerialServoManager == nullptr)
            {
                _errorOccured("STS not configured!");
                return String("STS not configured!");
            }

            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            int speed = servos[i]["Speed"];
            int acc = servos[i]["Acc"];
            String name = servos[i]["Name"];

            if (sendServoUpdateDirectly)
            {
                // send the value to the STS bus servo
                if (value < 0) // -1 means stop the servo
                    this->_stSerialServoManager->setTorque(channel, false);
                else
                {
                    this->_stSerialServoManager->setTorque(channel, true);
                    this->_stSerialServoManager->writePositionDirectToHardware(channel, value, speed, acc);
                }
            }
            else
            {
                // use the project data to set the target value
                bool done = false;
                for (int f = 0; f < this->_projectData->servos->size(); f++)
                {
                    if (this->_projectData->servos->at(f).config->type == ServoConfig::ServoTypes::STS_SERVO)
                        if (this->_projectData->servos->at(f).config->channel == channel)
                        {
                            // set servo target value
                            this->_projectData->servos->at(f).state->targetValue = value;
                            done = true;
                            break;
                        }
                }
                if (!done)
                    _errorOccured("STS Servo " + String(channel) + "/" + name + " not attached or not defined in awb export!");
            }
        }
    }
    if (!sendServoUpdateDirectly && this->_stSerialServoManager != nullptr)
        _stSerialServoManager->updateActuators(false);

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 40);

    if (jsondocPacketProcessor.containsKey("SCS")) // package contains SCS bus servo data
    {
        JsonArray servos = jsondocPacketProcessor["SCS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_scSerialServoManager == nullptr)
            {
                _errorOccured("SCS not configured!");
                return String("SCS not configured!");
            }
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];
            int speed = servos[i]["Speed"];

            if (sendServoUpdateDirectly)
            {
                if (value < 0) // -1 means stop the servo
                    this->_scSerialServoManager->setTorque(channel, false);
                else
                {
                    // send the value to the SCS bus servo
                    this->_scSerialServoManager->setTorque(channel, true);
                    this->_scSerialServoManager->writePositionDirectToHardware(channel, value, speed, 0);
                }
            }
            else
            {
                // use the project data to set the target value
                bool done = false;
                for (int f = 0; f < this->_projectData->servos->size(); f++)
                {
                    if (this->_projectData->servos->at(f).config->type == ServoConfig::ServoTypes::SCS_SERVO)
                        if (this->_projectData->servos->at(f).config->channel == channel)
                        {
                            // set servo target value
                            this->_projectData->servos->at(f).state->targetValue = value;
                            done = true;
                            break;
                        }
                }
                if (!done)
                    _errorOccured("SCS Servo " + String(channel) + "/" + name + " not attached or not defined in awb export!");
            }
        }
    }

    if (!sendServoUpdateDirectly && this->_scSerialServoManager != nullptr)
        _scSerialServoManager->updateActuators(false);

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->showActivity();
#endif

    _debugging->setState(Debugging::MJ_PROCESSING_PACKET, 99);

    // return empty string, because no response is needed
    return String();
}