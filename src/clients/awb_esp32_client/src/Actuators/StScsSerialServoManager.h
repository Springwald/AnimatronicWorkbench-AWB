#ifndef stSerialServo_h
#define stSerialServo_h

#include <Arduino.h>
#include <vector>
#include <SCServo.h>
#include "ActuatorValue.h"
#include "ProjectData/StsScsServo.h"
#include <Debugging.h>
class StScsSerialServoManager
{

#define MAX_STS_SCS_SERVO_ID_SCAN_RANGE 20

    using TCallBackErrorOccured = std::function<void(String)>;

private:
    std::vector<StsScsServo> *_servos;   /// The sts / scs servos
    TCallBackErrorOccured _errorOccured; /// callback functio to call if an error occured
    int _gpioRxd;                        /// the gpio pin for the rxd communication to the sts / scs servos
    int _gpioTxd;                        /// the gpio pin for the txd communication to the sts / scsservos
    bool _servoTypeIsScs;                /// is the servo type SCS or STS?
    Debugging *_debugging;               /// the debugging class

    /**
     * Scan for Ids and store in "servoIds"
     */
    void scanIds();

public:
    /**
     * the ids of the servos (1-253)
     */
    std::vector<u8> *servoIds;

    StScsSerialServoManager(std::vector<StsScsServo> *servos, bool servoTypeIsScs, TCallBackErrorOccured errorOccured, int gpioRxd, int gpioTxd) : _errorOccured(errorOccured), _servoTypeIsScs(servoTypeIsScs), _gpioRxd(gpioRxd), _gpioTxd(gpioTxd), _servos(servos) {};

    /**
     * Set up the sts servos
     */
    void setup();

    /**
     * update the sts servos
     */
    void updateActuators(boolean anyServoWithGlobalFaultHasCiriticalState);

    /**
     * write the position to the servo, including speed and acceleration
     */
    void writePositionDetailed(int id, int position, int speed, int acc);

    /**
     * write the position to the servo, using the default speed and acceleration
     */
    void writePosition(int id, int position);

    /**
     * read the actual position from the servo
     */
    int readPosition(u8 id);

    /**
     * set the maximum torque of the servo. Set 0 to turn the servo off
     */
    void setTorque(u8 id, bool on);

    /**
     * read the temperature of the servo in degree celsius
     */
    int readTemperature(int id);

    /**
     * read the actual load of the servo
     */
    int readLoad(int id);

    /**
     * is the servo available?
     */
    bool servoAvailable(int id);
};

#endif
