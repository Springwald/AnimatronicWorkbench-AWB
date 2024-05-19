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
    if (diff < 2000)
        return;

    _millisLastDisplayChange = millis();

    _displayStateCounter++;
    switch (_displayStateCounter)
    {
    case 1:
        _awbDisplay->set_actual_status_info(getDebugInfos());
        break;
    case 2:
        _awbDisplay->set_actual_status_info(updateActuatorsStatuses());
        break;
    case 3:
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

    int lostMemory = _freeMemoryOnStart - freeMemory;
    String memoryInfo = "mem:" + String(freeMemory / 1024) + "k lost:" + String(lostMemory / 1024) + "." + String((lostMemory % 1024) / 100) + "k";

    message = message + "\r\n" + memoryInfo;
    message = message + "\r\n" + _debugState;

    return message;
}

/**
 * read the status information from the actuators
 */
String StatusManagement::updateActuatorsStatuses()
{
    _isAnyGlobalFaultActuatorInCriticalState = false;

    String actualActuatorsStateInfo = "";

    // check Sts serial bus servos
    if (this->_stSerialServoManager != NULL)
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_stSerialServoManager, _projectData->stsServos, false);

    // check Scs serial bus servos
    if (this->_scSerialServoManager != NULL)
        actualActuatorsStateInfo += this->updateStsScsServoStatuses(this->_scSerialServoManager, _projectData->scsServos, true);

    // check PWM servos
    if (this->_pca9685PwmManager != NULL)
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

String StatusManagement::updateStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo)
{
    String errors = "";

    // Scs serial bus servos
    for (int i = 0; i < servos->size(); i++)
    {
        StsScsServo *servo = &servos->at(i);
        if (servo->title.length() > 0)
        {
            servo->isFault = false;

            servo->temperature = serialServoManager->readTemperature(servo->channel);
            if (servo->temperature > (isScsServo ? SCS_SERVO_MAX_TEMPERATURE : STS_SERVO_MAX_TEMPERATURE))
            {
                servo->isFault = true;
                if (servo->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;

                serialServoManager->setTorque(servo->channel, false);
                _errorOccured("Servo " + String(servo->title) + " critical temp! " + String(servo->temperature) + "C");
                errors += "Servo " + String(servo->title) + ":" + String(servo->temperature) + "C\r\n";
            }

            servo->load = serialServoManager->readLoad(servo->channel);
            if (abs(servo->load) > (isScsServo ? SCS_SERVO_MAX_LOAD : STS_SERVO_MAX_LOAD))
            {
                servo->isFault = true;
                if (servo->globalFault == true)
                    _isAnyGlobalFaultActuatorInCriticalState = true;
                serialServoManager->setTorque(servo->channel, false);
                _errorOccured("Servo " + String(servo->channel) + "' critical load! " + String(servo->load));
                errors += "Servo " + String(servo->title) + ":L" + String(servo->temperature) + "\r\n";
            }
        }
    }
    return errors;
}

int StatusManagement::getFreeMemory()
{
    return ESP.getFreeHeap(); // ESP.getMaxAllocHeap(), ESP.getMinFreeHeap()
}