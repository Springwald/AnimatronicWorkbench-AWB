#ifndef Mp3PlayerYX5300_manager_h
#define Mp3PlayerYX5300_manager_h

#include <Arduino.h>
#include <vector>
#include "ActuatorValue.h"

class Mp3PlayerYX5300Manager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;

public:
    Mp3PlayerYX5300Manager(TCallBackErrorOccured errorOccured, int portRx, int portTx) : _errorOccured(errorOccured){

                                                                                             /*
                                                                                             this->_pwm = Adafruit_PWMServoDriver(); // called this way, it uses the default address 0x40
                                                                                             this->_pwm.setPWMFreq(SERVO_FREQ);      // Analog servos run at ~50 Hz updates
                                                                                             this->_pwm.begin();
                                                                                             */

                                                                                         };

    void playSound(int trackNo);
};

#endif
