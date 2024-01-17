
#include <Arduino.h>
#include "StSerialServoManager.h"
#include "hardware.h"
#include "ActualStatusInformation.h"
#include "ActuatorValue.h"

// It's pretty messy and also a bit unclean that the library creates a global object.
// But it is the only way I found to use the library.
// Otherwise it seems to crash in various places - I guess because of memory problems.
SMS_STS _serialServo_STS;
SCSCL _serialServo_SCS;

/**
 * Set up the sts servos
 */
void StSerialServoManager::setup()
{
#if defined(USE_STS_SERVO) || defined(USE_SCS_SERVO)

    if (this->_servoTypeIsScs)
    {
        Serial1.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
        _serialServo_SCS.pSerial = &Serial1;
    }
    else
    {
        Serial2.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
        _serialServo_STS.pSerial = &Serial2;
    }
    delay(100);
#endif

    scanIds();
}

/**
 * update the sts servos
 */
void StSerialServoManager::updateActuators()
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return;
#endif

    if (servoCriticalTemp == true || servoCriticalLoad == true)
        return;

    for (int i = 0; i < this->servoValues->size(); i++)
    {
        // get a pointer to the current servo
        ActuatorValue *servo = &this->servoValues->at(i);
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
                    if (this->_servoTypeIsScs)
                    {
                        _serialServo_SCS.WritePosEx(servo->id, servo->targetValue, SCS_SERVO_SPEED, SCS_SERVO_ACC);
                    }
                    else
                    {
                        _serialServo_STS.WritePosEx(servo->id, servo->targetValue, STS_SERVO_SPEED, STS_SERVO_ACC);
                    }
                }
                else
                {
                    if (this->_servoTypeIsScs)
                    {
                        if (speed == -1)
                            speed = SCS_SERVO_SPEED;
                        if (acc == -1)
                            acc = SCS_SERVO_ACC;
                        _serialServo_SCS.WritePosEx(servo->id, servo->targetValue, speed, acc);
                    }
                    else
                    {
                        if (speed == -1)
                            speed = STS_SERVO_SPEED;
                        if (acc == -1)
                            acc = STS_SERVO_ACC;
                        _serialServo_STS.WritePosEx(servo->id, servo->targetValue, speed, acc);
                    }
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
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return;
#endif

    for (int i = 0; i < servoValues->size(); i++)
    {
        if (servoValues->at(i).id == id)
        {
            servoValues->at(i).targetValue = position;
            servoValues->at(i).speed = speed;
            servoValues->at(i).acc = acc;
        }
    }
}

/**
 * write the position to the servo, using the default speed and acceleration
 */
void StSerialServoManager::writePosition(int id, int position)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return;
#endif
    for (int i = 0; i < servoValues->size(); i++)
    {
        if (servoValues->at(i).id == id)
        {
            servoValues->at(i).targetValue = position;
        }
    }
}

/**
 * read the actual position from the servo
 */
int StSerialServoManager::readPosition(u8 id)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return -1;
#endif
    if (this->_servoTypeIsScs)
    {
        return _serialServo_SCS.ReadPos(id);
    }
    else
    {
        return _serialServo_STS.ReadPos(id);
    }
}

/**
 * set the maximum torque of the servo. Set 0 to turn the servo off
 */
void StSerialServoManager::setTorque(u8 id, bool on)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return;
#endif
    if (this->_servoTypeIsScs)
    {
        _serialServo_SCS.EnableTorque(id, on ? 1 : 0);
    }
    else
    {
        _serialServo_STS.EnableTorque(id, on ? 1 : 0);
    }
}

/**
 * read the actual load of the servo
 */
int StSerialServoManager::readLoad(int id)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return -1;
#endif
    if (this->_servoTypeIsScs)
    {
        return _serialServo_SCS.ReadLoad(id);
    }
    else
    {
        return _serialServo_STS.ReadLoad(id);
    }
}
/**
 * read the temperature of the servo in degree celsius
 */
int StSerialServoManager::readTemperature(int id)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return -1;
#endif

    if (this->_servoTypeIsScs)
    {
        return _serialServo_SCS.ReadTemper(id);
    }
    else
    {
        return _serialServo_STS.ReadTemper(id);
    }
}

/**
 * is the servo available?
 */
bool StSerialServoManager::servoAvailable(int id)
{
#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
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

#if !defined(USE_STS_SERVO) && !defined(USE_SCS_SERVO)
    return;
#endif

    for (int i = 1; i < MAX_STS_SCS_SERVO_ID_SCAN_RANGE; i++)
    {
        int retries = 5;
        while (retries-- > 0)
        {
            int id = -1;
            bool error = false;

            if (this->_servoTypeIsScs)
            {
                id = _serialServo_SCS.Ping(i);
                error = (_serialServo_SCS.Error != 0);
            }
            else
            {
                id = _serialServo_STS.Ping(i);
                error = (_serialServo_STS.Error != 0);
            }
            if (error == true)
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
