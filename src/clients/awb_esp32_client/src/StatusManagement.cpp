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

void StatusManagement::update(boolean criticalTemp)
{
    bool onlyLoadCheck = false;

    if (criticalTemp == true)
    {
        // critical temperature or load detected, so only check and show load status
        readActuatorsStatuses();
        showTemperaturStatuses();
    }
    else
    {
        // no critical temperature or load detected
        _displayStateCounter++;

        if (onlyLoadCheck)
        {
            // only check and show load status
            readActuatorsStatuses();
            showLoadStatuses();
        }
        else
        {
            if (_displayStateCounter < 100)
            {
                showValues();
                if (_displayStateCounter == 50)
                    readActuatorsStatuses();
            }
            else if (_displayStateCounter < 150)
            {
                showTemperaturStatuses();
            }
            else if (_displayStateCounter < 200)
            {
                if (_displayStateCounter == 151)
                {
                    draw_debugInfos();
                }
            }
            else
                _displayStateCounter = 0;
        }
    }
}

void StatusManagement::resetDebugInfos()
{
    _freeMemoryOnStart = getFreeMemory();
}

void StatusManagement::draw_debugInfos()
{
    int freeMemory = getFreeMemory();

    String message = "";

    int lostMemory = _freeMemoryOnStart - freeMemory;
    String memoryInfo = "mem:" + String(freeMemory / 1024) + "k lost:" + String(lostMemory / 1024) + "." + String((lostMemory % 1024) / 100) + "k";

    message = message + "\r\n" + memoryInfo;
    message = message + "\r\n" + _debugState;

    _awbDisplay->set_actual_status_info(message);

    /*
        statusFooterSprite.setTextColor(0xFFFFFFU, 0);
        statusFooterSprite.setTextDatum(top_center);

        int y = 0;

        statusFooterSprite.fillScreen(0x000000);

        y++;
        lostMemory = _freeMemoryOnStart - freeMemory;
        memoryInfo = "free:" + String(freeMemory / 1024) + "k lost:" + String(lostMemory / 1024) + "." + String((lostMemory % 1024) / 100) + "k";
        statusFooterSprite.drawString(memoryInfo, statusFooterSprite.width() / 2, y);

        y += _textSizeLineHeight;
        statusFooterSprite.setTextColor(0xAAAAFFU, 0);
        statusFooterSprite.drawString(_statusMsg, statusFooterSprite.width() / 2, y);

        statusFooterSprite.pushSprite(0, _statusFooterSpriteTop);
        draw_debuggingState();

        return true;*/
}

/**
 * read the status information from the actuators
 */
void StatusManagement::readActuatorsStatuses()
{
    // check Sts serial bus servos
    if (this->_stSerialServoManager != NULL)
        this->readStsScsServoStatuses(this->_stSerialServoManager, _projectData->stsServos, false);

    // check Scs serial bus servos
    if (this->_scSerialServoManager != NULL)
        this->readStsScsServoStatuses(this->_scSerialServoManager, _projectData->scsServos, true);

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
}

void StatusManagement::readStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<StsScsServo> *servos, bool isScsServo)
{
    bool criticalTemp = false;
    bool criticalLoad = false;

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
                criticalTemp = true;
                serialServoManager->setTorque(servo->channel, false);
                _errorOccured("Servo " + String(servo->channel) + " critical temperature! " + String(servo->temperature) + "C");
                servo->isFault = true;
            }

            servo->load = serialServoManager->readLoad(servo->channel);
            if (abs(servo->load) > (isScsServo ? SCS_SERVO_MAX_LOAD : STS_SERVO_MAX_LOAD))
            {
                criticalLoad = true;
                serialServoManager->setTorque(servo->channel, false);
                _errorOccured("Servo " + String(servo->channel) + " critical load! " + String(servo->load));
                servo->isFault = true;
            }
        }
    }
    serialServoManager->servoCriticalTempGlobal = criticalTemp;
    serialServoManager->servoCriticalLoadGlobal = criticalLoad;
}

/**
 * show the target values of the actuators on the display
 */
void StatusManagement::showValues()
{
    /*
    String values[100]; //_stsServoValues->size()];
    int used = 0;
    for (int i = 0; i < this->_projectData->stsServos->size(); i++)
    {
        // sts serial bus servos
        if (this->_projectData->stsServos->at(i).title.length() > 0)
        {
            values[used] = String(this->_projectData->stsServos->at(i).channel) + ":" + this->_projectData->stsServos->at(i).targetValue; // use ids only (needs less space)
            used++;
        }
    }
    for (int i = 0; i < this->_projectData->scsServos->size(); i++)
    {
        // scs serial bus servos
        if (this->_projectData->scsServos->at(i).title.length() > 0)
        {
            values[used] = String(this->_projectData->scsServos->at(i).channel) + ":" + this->_projectData->scsServos->at(i).targetValue; // use ids only (needs less space)
            used++;
        }
    }
    for (int i = 0; i < this->_projectData->pca9685PwmServos->size(); i++)
    {
        // Adafruit PWM servo driver
        if (this->_projectData->pca9685PwmServos->at(i).title.length() > 0)
        {
            values[used] = "P" + this->_projectData->pca9685PwmServos->at(i).title + ":" + this->_projectData->pca9685PwmServos->at(i).targetValue;
            used++;
        }
    }
    _awbDisplay.set_values(values, used);*/
}

/**
 * show the temperature values of the actuators on the display
 */
void StatusManagement::showTemperaturStatuses()
{
    String message = "";

    for (int i = 0; i < this->_projectData->stsServos->size(); i++)
    {
        // sts serial bus servos
        if (this->_projectData->stsServos->at(i).title.length() > 0)
        {
            message += String(this->_projectData->stsServos->at(i).channel) + ":" + this->_projectData->stsServos->at(i).temperature + "C|";
        }
    }
    for (int i = 0; i < this->_projectData->scsServos->size(); i++)
    {
        // scs serial bus servos
        if (this->_projectData->scsServos->at(i).title.length() > 0)
        {
            message += String(this->_projectData->scsServos->at(i).channel) + ":" + this->_projectData->scsServos->at(i).temperature + "C|";
        }
    }

    _awbDisplay->set_actual_status_info(message);
}

/**
 * show the load (torque) status of the actuators on the display
 */
void StatusManagement::showLoadStatuses()
{
    String message = "";

    for (int i = 0; i < this->_projectData->stsServos->size(); i++)
    {
        // sts serial bus servos
        if (this->_projectData->stsServos->at(i).title.length() > 0)
        {
            message += String(this->_projectData->stsServos->at(i).channel) + ":" + this->_projectData->stsServos->at(i).load + "|";
        }
    }
    for (int i = 0; i < this->_projectData->scsServos->size(); i++)
    {
        // scs serial bus servos
        if (this->_projectData->scsServos->at(i).title.length() > 0)
        {
            message += String(this->_projectData->scsServos->at(i).channel) + ":" + this->_projectData->scsServos->at(i).load + "|";
        }
    }
    _awbDisplay->set_actual_status_info(message);
}

int StatusManagement::getFreeMemory()
{
    return ESP.getFreeHeap(); // ESP.getMaxAllocHeap(), ESP.getMinFreeHeap()
}