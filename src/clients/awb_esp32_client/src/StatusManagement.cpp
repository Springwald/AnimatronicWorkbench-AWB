#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include <ArduinoJson.h>
#include "WlanConnector.h"
#include "StatusManagement.h"

void StatusManagement::setDebugStatus(String state)
{
    _debugState = state;
}

void StatusManagement::update()
{

    auto diff = millis() - _millisLastDisplayChange;
    if (diff < durationPerDisplayState)
        return;

    _millisLastDisplayChange = millis();

    _displayStateCounter++;
    switch (_displayStateCounter)
    {
    case 1:
        _awbDisplay->set_actual_status_info(getDebugInfos());
        break;
    case 3:
        _awbDisplay->set_actual_status_info(updateActuatorsStatuses(diff));
        break;
    case 4:
        _displayStateCounter = 0;
        break;
    }
}

void StatusManagement::resetDebugInfos()
{
    _freeMemoryOnStart = getFreeMemory();
}

String StatusManagement::getDebugInfos()
{
    int freeMemory = getFreeMemory();

    String message = "AWB Client " + String(_clientId);

    long lostMemory = _freeMemoryOnStart - freeMemory;
    String memoryInfo = "mem:" + String(freeMemory / 1024) + "k lost:" + String(lostMemory / 1024) + "." + String((lostMemory % 1024) / 100) + "k";

    message = message + "\r\n" + memoryInfo;
    message = message + "\r\n" + _debugState;

    return message;
}

/**
 * read the status information from the actuators
 */
String StatusManagement::updateActuatorsStatuses(unsigned long diffMs)
{
    _isAnyGlobalFaultActuatorInCriticalState = false;

    String actualActuatorsStateInfo = "";

    // check Sts serial bus servos
    if (this->_stSerialServoManager != nullptr)
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_stSerialServoManager, _projectData->servos, false, diffMs);

    // check Scs serial bus servos
    if (this->_scSerialServoManager != nullptr)
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_scSerialServoManager, _projectData->servos, true, diffMs);

    // check PWM servos
    if (this->_pca9685PwmManager != nullptr)
    {
        for (int i = 0; i < this->_projectData->servos->size(); i++)
        {
            Servo *servo = &this->_projectData->servos->at(i);
            if (servo->config->type != ServoConfig::ServoTypes::PWM_SERVO)
                continue;

            // PWM servo driver
            if (servo->title.length() > 0)
            {
                // PWM Servo support no status information reading
            }
        }
    }

    if (actualActuatorsStateInfo == "")
    {
        return "No actuators infos.";
    }

    return actualActuatorsStateInfo;
}

String StatusManagement::updateStsScsServoStatuses(StScsSerialServoManager *serialServoManager, std::vector<Servo> *servos, bool isScsServo, unsigned long diffMs)
{
    String errors = "";
    String servoInfos = "";

    // Scs and sts serial bus servos
    for (int i = 0; i < servos->size(); i++)
    {
        Servo *servo = &servos->at(i);
        if (isScsServo == true && (servo->config->type != ServoConfig::ServoTypes::SCS_SERVO))
            continue;
        if (isScsServo == false && (servo->config->type != ServoConfig::ServoTypes::STS_SERVO))
            continue;

        if (servo->title.length() > 0)
        {
            auto wasFault = servo->state->isFaultCountDownMs > 0;
            bool isFault = false;
            servo->state->isFaultCountDownMs = max((unsigned long)0, servo->state->isFaultCountDownMs - diffMs); // decrease fault countdown

            servo->state->temperature = serialServoManager->readTemperature(servo->config->channel);
            if (servo->state->temperature > servo->config->maxTemp) // servo is too hot
            {
                servo->state->isFaultCountDownMs = 5000; // pause servo for 5 seconds
                if (servo->config->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;

                servo->state->lastFaultMessage = String(servo->state->temperature) + "C";
                servo->state->lastFaultMs = millis();
                serialServoManager->setTorque(servo->config->channel, false);
                _errorOccured("Servo " + String(servo->title) + " critical temp! " + String(servo->state->temperature) + "C");
                if (errors != "")
                    errors += ", ";
                errors += String(servo->config->channel) + ":" + String(servo->state->temperature) + "C";
                isFault = true;
            }

            servo->state->load = serialServoManager->readLoad(servo->config->channel);
            servo->state->maxLoad = max(servo->state->maxLoad, servo->state->load); // store max load
            servo->state->minLoad = min(servo->state->minLoad, servo->state->load); // store min load
            if (abs(servo->state->load) > servo->config->maxTorque)                 // servo is overloaded
            {
                servo->state->isFaultCountDownMs = 2000; // pause servo for 2 seconds
                if (servo->config->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;
                serialServoManager->setTorque(servo->config->channel, false);
                servo->state->lastFaultMessage = String(servo->state->load) + "L";
                servo->state->lastFaultMs = millis();
                _errorOccured("Servo " + String(servo->config->channel) + "' critical load! " + String(servo->state->load));
                if (errors != "")
                    errors += ", ";
                errors += String(servo->config->channel) + ":L" + String(servo->state->load);
                isFault = true;
            }

            if (wasFault && servo->state->isFaultCountDownMs == 0) // fault cleared, re-enable servo
                serialServoManager->setTorque(servo->config->channel, true);

            if (!isFault)
            {
                servoInfos += (servoInfos == "" ? "" : ", ") + String(servo->config->channel);
            }
        }
    }

    if (errors == "")
        return (isScsServo ? "SCS OK: " : "STS: ") + servoInfos;

    return (isScsServo ? "SCS: " : "STS: ") + errors;
}

int StatusManagement::getFreeMemory()
{
    return ESP.getFreeHeap(); // ESP.getMaxAllocHeap(), ESP.getMinFreeHeap()
}