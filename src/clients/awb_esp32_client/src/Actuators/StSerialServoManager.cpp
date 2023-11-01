
#include <Arduino.h>
#include "StSerialServoManager.h"
#include "hardware.h"
#include "ActualStatusInformation.h"
#include "ActuatorValue.h"

// It's pretty messy and also a bit unclean that the library creates a global object.
// But it is the only way I found to use the library.
// Otherwise it seems to crash in various places - I guess because of memory problems.
SMS_STS _serialServo;

/**
 * Set up the sts servos
 */
void StSerialServoManager::setup()
{
#ifdef USE_STS_SERVO
    Serial1.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
    _serialServo.pSerial = &Serial1;
    delay(100);
#endif
    scanIds();
}

/**
 * update the sts servos
 */
void StSerialServoManager::updateActuators()
{
#ifndef USE_STS_SERVO
    return;
#endif

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
                    // use default values for speed and acc
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

/**
 * write the position to the servo, including speed and acceleration
 */
void StSerialServoManager::writePositionDetailed(int id, int position, int speed, int acc)
{
#ifndef USE_STS_SERVO
    return;
#endif
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

/**
 * write the position to the servo, using the default speed and acceleration
 */
void StSerialServoManager::writePosition(int id, int position)
{
#ifndef USE_STS_SERVO
    return;
#endif
    for (int i = 0; i < stsServoValues->size(); i++)
    {
        if (stsServoValues->at(i).id == id)
        {
            stsServoValues->at(i).targetValue = position;
        }
    }
}

/**
 * read the actual position from the servo
 */
int StSerialServoManager::readPosition(u8 id)
{
#ifndef USE_STS_SERVO
    return -1;
#endif
    return _serialServo.ReadPos(id);
}

/**
 * set the maximum torque of the servo. Set 0 to turn the servo off
 */
void StSerialServoManager::setTorque(u8 id, bool on)
{
#ifndef USE_STS_SERVO
    return;
#endif
    _serialServo.EnableTorque(id, on ? 1 : 0);
}

/**
 * read the actual load of the servo
 */
int StSerialServoManager::readLoad(int id)
{
#ifndef USE_STS_SERVO
    return -1;
#endif
    return _serialServo.ReadLoad(id);
}

/**
 * read the temperature of the servo in degree celsius
 */
int StSerialServoManager::readTemperature(int id)
{
#ifndef USE_STS_SERVO
    return -1;
#endif
    return _serialServo.ReadTemper(id);
}

/**
 * is the servo available?
 */
bool StSerialServoManager::servoAvailable(int id)
{
#ifndef USE_STS_SERVO
    return false;
#endif
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

/**
 * scan for all available servo ids.
 * Only inside the given range to prevent long delays.
 */
void StSerialServoManager::scanIds()
{
    servoIds = new std::vector<u8>();

#ifndef USE_STS_SERVO
    return;
#endif
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
