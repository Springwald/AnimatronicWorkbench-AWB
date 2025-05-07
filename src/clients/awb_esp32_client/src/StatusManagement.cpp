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
    if (diff < 1000)
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

    String message = "";

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
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_stSerialServoManager, _projectData->stsServos, false, diffMs);

    // check Scs serial bus servos
    if (this->_scSerialServoManager != nullptr)
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_scSerialServoManager, _projectData->scsServos, true, diffMs);

    // check PWM servos
    if (this->_pca9685PwmManager != nullptr)
    {
        for (int i = 0; i < this->_projectData->pca9685PwmServos->size(); i++)
        {
            // PWM servo driver
            if (this->_projectData->pca9685PwmServos->at(i).title.length() > 0)
            {
                // PWM Servo support no status information reading
            }
        }
    }

    if (actualActuatorsStateInfo == "")
    {
        return "All actuators OK";
    }

    return actualActuatorsStateInfo;
}

String StatusManagement::updateStsScsServoStatuses(StScsSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo, unsigned long diffMs)
{
    String errors = "";

    // Scs serial bus servos
    for (int i = 0; i < servos->size(); i++)
    {
        StsScsServo *servo = &servos->at(i);
        if (servo->title.length() > 0)
        {
            auto wasFault = servo->isFaultCountDownMs > 0;
            servo->isFaultCountDownMs = max((unsigned long)0, servo->isFaultCountDownMs - diffMs); // decrease fault countdown

            servo->temperature = serialServoManager->readTemperature(servo->channel);
            if (servo->temperature > servo->maxTemp) // servo is too hot
            {
                servo->isFaultCountDownMs = 5000; // pause servo for 5 seconds
                if (servo->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;

                servo->lastFaultMessage = String(servo->temperature) + "C";
                servo->lastFaultMs = millis();
                serialServoManager->setTorque(servo->channel, false);
                _errorOccured("Servo " + String(servo->title) + " critical temp! " + String(servo->temperature) + "C");
                errors += "Servo " + String(servo->title) + ":" + String(servo->temperature) + "C\r\n";
            }

            servo->load = serialServoManager->readLoad(servo->channel);
            servo->maxLoad = max(servo->maxLoad, servo->load); // store max load
            servo->minLoad = min(servo->minLoad, servo->load); // store min load
            if (abs(servo->load) > servo->maxTorque)           // servo is overloaded
            {
                servo->isFaultCountDownMs = 2000; // pause servo for 2 seconds
                if (servo->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;
                serialServoManager->setTorque(servo->channel, false);
                servo->lastFaultMessage = String(servo->load) + "L";
                servo->lastFaultMs = millis();
                _errorOccured("Servo " + String(servo->channel) + "' critical load! " + String(servo->load));
                errors += "Servo " + String(servo->title) + ":L" + String(servo->load) + "\r\n";
            }

            if (wasFault && servo->isFaultCountDownMs == 0) // fault cleared, reenable servo
            {
                serialServoManager->setTorque(servo->channel, true);
            }
        }
    }
    return errors;
}

int StatusManagement::getFreeMemory()
{
    return ESP.getFreeHeap(); // ESP.getMaxAllocHeap(), ESP.getMinFreeHeap()
}