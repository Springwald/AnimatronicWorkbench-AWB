#ifndef Pca9685_pwm_manager_h
#define Pca9685_pwm_manager_h

#include <Arduino.h>
#include "Adafruit_PWMServoDriver.h"
#include <vector>
#include "ActuatorValue.h"

#define SERVO_FREQ 50 // Analog servos run at ~50 Hz updates

class Pca9685PwmManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    Adafruit_PWMServoDriver _pwm;
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;

    std::vector<ActuatorValue> *pwmServoValues; /// The pwm servo values
    int _i2cAdress;                             /// the i2c adress of the pca9685 pwm board

    void writeMicroseconds(uint8_t adr, uint8_t num, uint16_t microSeconds);
    void setOscillatorFrequency(uint8_t adr, uint32_t freq);

public:
    Pca9685PwmManager(std::vector<ActuatorValue> *pwmServoValues, TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow, uint i2cAdress) : _errorOccured(errorOccured), _messageToShow(messageToShow), _i2cAdress(i2cAdress), pwmServoValues(pwmServoValues)
    {
        this->_pwm = Adafruit_PWMServoDriver(); // called this way, it uses the default address 0x40
        this->_pwm.setPWMFreq(SERVO_FREQ);      // Analog servos run at ~50 Hz updates
        this->_pwm.begin();

        /*
         * In theory the internal oscillator (clock) is 25MHz but it really isn't
         * that precise. You can 'calibrate' this by tweaking this number until
         * you get the PWM update frequency you're expecting!
         * The int.osc. for the PCA9685 chip is a range between about 23-27MHz and
         * is used for calculating things like writeMicroseconds()
         * Analog servos run at ~50 Hz updates, It is importaint to use an
         * oscilloscope in setting the int.osc frequency for the I2C PCA9685 chip.
         * 1) Attach the oscilloscope to one of the PWM signal pins and ground on
         *    the I2C PCA9685 chip you are setting the value for.
         * 2) Adjust setOscillatorFrequency() until the PWM update frequency is the
         *    expected value (50Hz for most ESCs)
         * Setting the value here is specific to each individual I2C PCA9685 chip and
         * affects the calculations for the PWM update frequency.
         * Failure to correctly set the int.osc value will cause unexpected PWM results
         */
        // this->setOscillatorFrequency(oscillatorFrequency);

        // delay(10);

        // // Drive each servo one at a time using writeMicroseconds(), it's not precise due to calculation rounding!
        // // The writeMicroseconds() function is used to mimic the Arduino Servo library writeMicroseconds() behavior.
        // for (uint16_t microsec = USMIN; microsec < USMAX; microsec++)
        // {
        //     this->_pwm.writeMicroseconds(servonum, microsec);
        // }

        // delay(500);
        // for (uint16_t microsec = USMAX; microsec > USMIN; microsec--)
        // {
        //     this->_pwm.writeMicroseconds(servonum, microsec);
        // }
    };

    void setTargetValue(int channel, int value, String name);
    void updateActuators();
};

#endif
