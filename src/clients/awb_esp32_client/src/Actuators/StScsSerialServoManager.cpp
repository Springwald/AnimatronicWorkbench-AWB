
#include <Arduino.h>
#include "StScsSerialServoManager.h"
#include "AwbDataImport/HardwareConfig.h"
#include "ActualStatusInformation.h"
#include "ActuatorValue.h"

// It's pretty messy and also a bit unclean that the library creates a global object.
// But it is the only way I found to use the library.
// Otherwise it seems to crash in various places - I guess because of memory problems.
static SMS_STS _serialServo_STS;
static SCSCL _serialServo_SCS;

/**
 * Set up the sts servos
 */
void StScsSerialServoManager::setup()
{
    if (this->_servoTypeIsScs)
    {
#ifndef USE_SCS_SERVO
        this->_errorOccured("SCS setup, but no USE_SCS_SERVO in hardware.h");
#endif
        Serial1.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
        _serialServo_SCS.pSerial = &Serial1;
    }
    else
    {
#ifndef USE_STS_SERVO
        this->_errorOccured("STS setup but no USE_STS_SERVO in hardware.h");
#endif
        Serial2.begin(1000000, SERIAL_8N1, _gpioRxd, _gpioTxd);
        _serialServo_STS.pSerial = &Serial2;
    }

    delay(100);
    scanIds();
}

/**
 * update the sts servos
 */
void StScsSerialServoManager::updateActuators(boolean anyServoWithGlobalFaultHasCiriticalState)
{
    for (int i = 0; i < this->_servos->size(); i++)
    {
        // get a pointer to the current servo
        StsScsServo *servo = &this->_servos->at(i);

        if (servo->isFault || anyServoWithGlobalFaultHasCiriticalState == true)
        {
            // turn servo off when is fault or another servo is defined as global fault and in critical state
            setTorque(servo->channel, false);
            continue;
        }

        if (servo->targetValue == -1)
        {
            // turn servo off
            setTorque(servo->channel, false);
        }
        else
        {

            // set new target value if changed
            if (servo->currentValue != servo->targetValue)
            {

                int speed = servo->targetSpeed;
                int acc = servo->targetAcc;
                if (speed == -1 && acc == -1)
                {
                    if (this->_servoTypeIsScs)
                    {
                        _serialServo_SCS.WritePosEx(servo->channel, servo->targetValue, servo->defaultSpeed, servo->defaultAcceleration);
                    }
                    else
                    {
                        _serialServo_STS.WritePosEx(servo->channel, servo->targetValue, servo->defaultSpeed, servo->defaultAcceleration);
                    }
                }
                else
                {
                    if (this->_servoTypeIsScs)
                    {
                        if (speed == -1)
                            speed = servo->defaultSpeed;
                        if (acc == -1)
                            acc = servo->defaultAcceleration;
                        _serialServo_SCS.WritePosEx(servo->channel, servo->targetValue, speed, acc);
                    }
                    else
                    {
                        if (speed == -1)
                            speed = servo->defaultSpeed;
                        if (acc == -1)
                            acc = servo->defaultAcceleration;
                        _serialServo_STS.WritePosEx(servo->channel, servo->targetValue, speed, acc);
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
void StScsSerialServoManager::writePositionDetailed(int id, int position, int speed, int acc)
{
    if (this->servoAvailable(id))
    {
        for (int i = 0; i < _servos->size(); i++)
        {
            if (_servos->at(i).channel == id)
            {
                _servos->at(i).targetValue = position;
                _servos->at(i).targetSpeed = speed;
                _servos->at(i).targetAcc = acc;
            }
        }
    }
    else
    {
        _errorOccured("STS Servo channel " + String(id) + " not attached!");
    }
}

/**
 * write the position to the servo, using the default speed and acceleration
 */
void StScsSerialServoManager::writePosition(int id, int position)
{
    for (int i = 0; i < _servos->size(); i++)
    {
        if (_servos->at(i).channel == id)
        {
            _servos->at(i).targetValue = position;
        }
    }
}

/**
 * read the actual position from the servo
 */
int StScsSerialServoManager::readPosition(u8 id)
{
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
void StScsSerialServoManager::setTorque(u8 id, bool on)
{
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
int StScsSerialServoManager::readLoad(int id)
{
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
int StScsSerialServoManager::readTemperature(int id)
{
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
bool StScsSerialServoManager::servoAvailable(int id)
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

/**
 * scan for all available servo ids.
 * Only inside the given range to prevent long delays.
 */
void StScsSerialServoManager::scanIds()
{
    servoIds = new std::vector<u8>();

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
