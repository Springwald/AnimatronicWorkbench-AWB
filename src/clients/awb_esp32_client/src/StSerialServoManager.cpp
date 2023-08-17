
#include <Arduino.h>
#include "StSerialServoManager.h"
#include "hardware.h"
#include "ActualStatusInformation.h"
#include "ActuatorValue.h"

// It's pretty messy and also a bit unclean that the library creates a global object.
// But it is the only way I found to use the library.
// Otherwise it seems to crash in various places - I guess because of memory problems.
SMS_STS _serialServo;

void StSerialServoManager::setup()
{
    Serial1.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
    _serialServo.pSerial = &Serial1;
    delay(100);
    scanIds();
}

void StSerialServoManager::updateActuators()
{
    if (servoCriticalTemp == true || servoCriticalLoad == true)
        return;

    for (int i = 0; i < this->stsServoValues->size(); i++)
    {
        // get a pointer to the current servo
        ActuatorValue *servo = &this->stsServoValues->at(i);
        if (servo->targetValue == -1)
        {
            // turn servo off
            setTorque(servo->id, false);
        }
        else
        {
            // set new target value if changed
            if (servo->currentValue != servo->targetValue)
            {
                int speed = servo->speed;
                int acc = servo->acc;
                if (speed == -1 && acc == -1)
                {
                    _serialServo.WritePosEx(servo->id, servo->targetValue, STS_SERVO_SPEED, STS_SERVO_ACC);
                }
                else
                {
                    if (speed == -1)
                        speed = STS_SERVO_SPEED;
                    if (acc == -1)
                        acc = STS_SERVO_ACC;
                    _serialServo.WritePosEx(servo->id, servo->targetValue, speed, acc);
                }
                servo->currentValue = servo->targetValue;
            }
        }
    }
}

void StSerialServoManager::writePositionDetailed(int id, int position, int speed, int acc)
{
    for (int i = 0; i < stsServoValues->size(); i++)
    {
        if (stsServoValues->at(i).id == id)
        {
            stsServoValues->at(i).targetValue = position;
            stsServoValues->at(i).speed = speed;
            stsServoValues->at(i).acc = acc;
        }
    }
}

void StSerialServoManager::writePosition(int id, int position)
{
    for (int i = 0; i < stsServoValues->size(); i++)
    {
        if (stsServoValues->at(i).id == id)
        {
            stsServoValues->at(i).targetValue = position;
        }
    }
}

int StSerialServoManager::readPosition(u8 id)
{
    return _serialServo.ReadPos(id);
}

void StSerialServoManager::setTorque(u8 id, bool on)
{
    _serialServo.EnableTorque(id, on ? 1 : 0);
}

int StSerialServoManager::readLoad(int id)
{
    return _serialServo.ReadLoad(id);
}

int StSerialServoManager::readTemperature(int id)
{
    return _serialServo.ReadTemper(id);
}

bool StSerialServoManager::servoAvailable(int id)
{
    for (int i = 0; i < servoIds->size(); i++)
    {
        if (servoIds->at(i) == id)
        {
            return true;
        }
    }

    // not already found with scanIds(). Try to ping it directly.
    // int scanId = _serialServo.Ping(id);
    // if (scanId != -1)
    // {
    //     servoIds->push_back(scanId);
    //     return true;
    // }

    return false;
}

void StSerialServoManager::scanIds()
{
    servoIds = new std::vector<u8>();
    for (int i = 1; i < MAX_STS_SERVO_ID_SCAN_RANGE; i++)
    {
        int retries = 5;
        while (retries-- > 0)
        {
            int id = _serialServo.Ping(i);
            if (_serialServo.Error != 0)
            {
                delay(100);
            }
            else
            {
                if (id != -1)
                    servoIds->push_back(id);
                break;
            }
        }
    }
}
