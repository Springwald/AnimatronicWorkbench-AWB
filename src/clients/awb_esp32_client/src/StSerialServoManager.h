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
    std::vector<ActuatorValue> *stsServoValues;
    TCallBackErrorOccured _errorOccured;
    int _speed;
    int _acc;
    int _gpioRxd;
    int _gpioTxd;

    void scanIds(); /// scan for Ids and store in "servoIds"

public:
    bool servoCriticalTemp = false;
    bool servoCriticalLoad = false;
    std::vector<u8> *servoIds;

    StSerialServoManager(std::vector<ActuatorValue> *stsServoValues, TCallBackErrorOccured errorOccured, int gpioRxd, int gpioTxd, int speed, int acc) : _errorOccured(errorOccured), _speed(speed), _acc(acc), _gpioRxd(gpioRxd), _gpioTxd(gpioTxd), stsServoValues(stsServoValues){};

    void setup();
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
