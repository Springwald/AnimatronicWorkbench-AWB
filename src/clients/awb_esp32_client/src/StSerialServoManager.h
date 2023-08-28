#ifndef stSerialServo_h
#define stSerialServo_h

#include <Arduino.h>
#include <vector>
#include <SCServo.h>
#include "ActuatorValue.h"

class StSerialServoManager
{

#define MAX_STS_SERVO_ID_SCAN_RANGE 20

    using TCallBackErrorOccured = std::function<void(String)>;

private:
    std::vector<ActuatorValue> *stsServoValues; /// The sts servo values
    TCallBackErrorOccured _errorOccured;        /// callback functio to call if an error occured
    int _speed;                                 /// the speed to use for the sts servos
    int _acc;                                   /// the acceleration to use for the sts servos
    int _gpioRxd;                               /// the gpio pin for the rxd communication to the sts servos
    int _gpioTxd;                               /// the gpio pin for the txd communication to the sts servos

    /**
     * Scan for Ids and store in "servoIds"
     */
    void scanIds();

public:
    /**
     * has any servo a critical temperature?
     */
    bool servoCriticalTemp = false;

    /**
     * has any servo a critical load?
     */
    bool servoCriticalLoad = false;

    /**
     * the ids of the servos (1-253)
     */
    std::vector<u8> *servoIds;

    StSerialServoManager(std::vector<ActuatorValue> *stsServoValues, TCallBackErrorOccured errorOccured, int gpioRxd, int gpioTxd, int speed, int acc) : _errorOccured(errorOccured), _speed(speed), _acc(acc), _gpioRxd(gpioRxd), _gpioTxd(gpioTxd), stsServoValues(stsServoValues){};

    /**
     * Set up the sts servos
     */
    void setup();

    /**
     * update the sts servos
     */
    void updateActuators();

    void writePositionDetailed(int id, int position, int speed, int acc);
    void writePosition(int id, int position);
    int readPosition(u8 id);
    void setTorque(u8 id, bool on);
    int readTemperature(int id);
    int readLoad(int id);
    bool servoAvailable(int id);
};

#endif
