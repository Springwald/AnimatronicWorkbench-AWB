#ifndef stSerialServo_h
#define stSerialServo_h

#include <Arduino.h>
#include <vector>
#include <SCServo.h>

class StSerialServoManager
{

#define MAX_STS_SERVO_ID_SCAN_RANGE 20

    using TCallBackErrorOccured = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    int _speed;
    int _acc;
    int _gpioRxd;
    int _gpioTxd;
    void scanIds(); /// scan for Ids and store in "servoIds"
    

public:
    std::vector<u8> *servoIds;

    StSerialServoManager(TCallBackErrorOccured errorOccured, int gpioRxd, int gpioTxd, int speed, int acc) : _errorOccured(errorOccured), _speed(speed), _acc(acc), _gpioRxd(gpioRxd), _gpioTxd(gpioTxd){};

    void setup();
    void writePositionDetailed(u8 id, s16 position, u16 speed, u8 acc);
    void writePosition(u8 id, s16 position);
    int readPosition(u8 id);
    void setTorque(u8 id, bool on);
    int readTemperature(int id);
    int readLoad(int id);
    bool servoAvailable(int id);
};

#endif
