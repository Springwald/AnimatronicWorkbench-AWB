#ifndef hardware_h
#define hardware_h

/* Display settings */
// #define DISPLAY_M5STACK // if you use a M5Stack, uncomment this line. Uses GPIO 14+18 for display communication
#define DISPLAY_SSD1306 // e.g. for Waveshare Servo Driver with ESP32

/* Neopixel status LEDs */
// #define USE_NEOPIXEL_STATUS_CONTROL
#define STATUS_RGB_LED_GPIO 23      // the GPIO used to control RGB LEDs. GPIO 23, as default.
#define STATUS_RGB_LED_NUMPIXELS 13 // how many RGB LEDs are connected to the GPIO

/* autoplay state selector */
// if a servo position feedback is used as a state selector, define the servo channel here.
// if you don't use a servo as state selector, set this to -1 or undefine it
// #define AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL 9
// if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
#define AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET 457

/* STS serial servo settings */
#define USE_STS_SERVO
#define STS_SERVO_SPEED 1500         // speed, default = 1500
#define STS_SERVO_ACC 10             // acceleration, default = 100
#define STS_SERVO_RXD 18             // eg. GPIO 18 for wavesahre servo driver
#define STS_SERVO_TXD 19             // eg. GPIO 19 for wavesahre servo driver
#define STS_SERVO_MAX_TEMPERATURE 45 // max temperature (celsius) before servo is disabled
#define STS_SERVO_MAX_LOAD 250       // max load before servo is disabled

/* PCA9685 PWM servo settings */
#define USE_PCA9685_PWM_SERVO
#define PCA9685_I2C_ADDRESS 0x40
#define PCA9685_SPEED 1500
#define PCA9685_ACC 10

/* DAC speaker */
// #define USE_DAC_SPEAKER

#endif
