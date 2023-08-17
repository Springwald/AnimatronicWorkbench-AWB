
#include <Arduino.h>
#include "StSerialServoManager.h"
#include "hardware.h"

// It's pretty messy and also a bit unclean that the library creates a global object.
// But it is the only way I found to use the library.
// Otherwise it seems to crash in various places - I guess because of memory problems.
SMS_STS _serialServo;

void StSerialServoManager::setup()
{
    Serial1.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
    _serialServo.pSerial = &Serial1;
    delay(1000);
    scanIds();
}

void StSerialServoManager::writePositionDetailed(u8 id, s16 position, u16 speed, u8 acc)
{
    _serialServo.WritePosEx(id, position, speed, acc);
}

void StSerialServoManager::writePosition(u8 id, s16 position)
{
    _serialServo.WritePosEx(id, position, STS_SERVO_SPEED, STS_SERVO_ACC);
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
        int retries = 3;
        while (retries-- > 0)
        {
            int id = _serialServo.Ping(i);
            if (_serialServo.Error != 0)
            {
                delay(500);
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
