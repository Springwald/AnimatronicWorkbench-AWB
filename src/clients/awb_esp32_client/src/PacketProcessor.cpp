#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include <ArduinoJson.h>
#include "WlanConnector.h"
#include "PacketProcessor.h"

StaticJsonDocument<1024 * 32> jsondoc;

/**
 * process a received packet from the Animatronic Workbench Studio
 */
void PacketProcessor::processPacket(String payload)
{
    DeserializationError error = deserializeJson(jsondoc, payload);
    if (error)
    {
        // packet content is not valid json
        _errorOccured("json:" + String(error.c_str()));
        return;
    }

    if (jsondoc.containsKey("DispMsg")) // packat contains a display message
    {
        const char *message = jsondoc["DispMsg"]["Msg"];
        if (message == NULL)
        {
            // should not happen, instead the whole DispMsg should be missing
            _errorOccured("DispMsg?!? " + payload);
        }
        else
        {
            int duration = jsondoc["DispMsg"]["Ms"];
            if (duration < 100)
                duration = 5000; // default duration
            _messageToShow(message, duration);
        }
    }

    if (jsondoc.containsKey("Pca9685Pwm")) // packet contains Pca9685 PWM driver data
    {
        JsonArray servos = jsondoc["Pca9685Pwm"]["Servos"];
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_pca9685PwmManager == NULL)
            {
                _errorOccured("Pca9685Pwm not configured!");
                return;
            }
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            // store the method showMsg in an anonymous function
            _pca9685PwmManager->setTargetValue(channel, value, name);
        }
    }
    if (this->_pca9685PwmManager != NULL)
        _pca9685PwmManager->updateActuators(false);

    if (jsondoc.containsKey("STS")) // package contains STS bus servo data
    {
        JsonArray servos = jsondoc["STS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_stSerialServoManager == NULL)
            {
                _errorOccured("STS not configured!");
                return;
            }

            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            bool done = false;

            for (int f = 0; f < this->_projectData->stsServos->size(); f++)
            {
                if (this->_projectData->stsServos->at(f).channel == channel)
                {
                    // set servo target value
                    this->_projectData->stsServos->at(f).targetValue = value;
                    done = true;
                    break;
                }
            }
            if (!done)
                _errorOccured("STS Servo " + String(channel) + "/" + name + " not attached or not defined in awb export!");
        }
    }
    if (this->_stSerialServoManager != NULL)
        _stSerialServoManager->updateActuators(false);

    if (jsondoc.containsKey("SCS")) // package contains SCS bus servo data
    {
        JsonArray servos = jsondoc["SCS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_scSerialServoManager == NULL)
            {
                _errorOccured("SCS not configured!");
                return;
            }
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            bool done = false;
            for (int f = 0; f < this->_projectData->scsServos->size(); f++)
            {
                if (this->_projectData->scsServos->at(f).channel == channel)
                {
                    // set servo target value
                    this->_projectData->scsServos->at(f).targetValue = value;
                    done = true;
                    break;
                }
            }
            if (!done)
                _errorOccured("SCS Servo " + String(channel) + "/" + name + " not attached or not defined in awb export!");
        }
    }
    if (this->_scSerialServoManager != NULL)
        _scSerialServoManager->updateActuators(false);

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->showActivity();
#endif
}