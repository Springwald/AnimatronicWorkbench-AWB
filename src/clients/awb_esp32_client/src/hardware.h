#ifndef hardware_h
#define hardware_h

/* Display settings */

// -- M5 Stack Displays --
// #define DISPLAY_M5STACK // if you use a M5Stack, uncomment this line. Uses GPIO 14+18 for display communication

// -- SSD1306 Displays  --
// e.g. for Waveshare Servo Driver with ESP32: 128x32, 0x02, 0x3C
#define DISPLAY_SSD1306
#define DISPLAY_SSD1306_I2C_ADDRESS 0x3D // 0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
#define DISPLAY_SSD1306_WIDTH 128
#define DISPLAY_SSD1306_HEIGHT 64
#define DISPLAY_SSD1306_COM_PINS 0x12 // 0x02, 0x12, 0x22 or 0x32
                                      // The SSD1306 may have different connection patterns between the panel and
                                      // controller depending on the product, and the 0xDA command adjusts for this.
                                      // If it does not display correctly, try the following values of the

/* Neopixel status LEDs */
// #define USE_NEOPIXEL_STATUS_CONTROL
#define STATUS_RGB_LED_GPIO 23      // the GPIO used to control RGB LEDs. GPIO 23, as default.
#define STATUS_RGB_LED_NUMPIXELS 13 // how many RGB LEDs are connected to the GPIO

/* STS serial servo settings */
#define USE_STS_SERVO                // if you use a STS serial servo, uncomment this line
#define STS_SERVO_SPEED 1500         // speed, default = 1500
#define STS_SERVO_ACC 10             // acceleration, default = 100
#define STS_SERVO_RXD 16             // eg. GPIO 18 for waveshare servo driver
#define STS_SERVO_TXD 17             // eg. GPIO 19 for waveshare servo driver
#define STS_SERVO_MAX_TEMPERATURE 45 // max temperature (celsius) before servo is disabled
#define STS_SERVO_MAX_LOAD 300       // max load before servo is disabled

/* autoplay state selector */
// if a servo position feedback is used as a state selector, define the servo channel here.
// if you don't use a servo as state selector, set this to -1 or undefine it
// #define AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL 9
// if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
#define AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET 457

/* SCS serial servo settings */
#define USE_SCS_SERVO                // uncomment this line if you want to use SCS servos
#define SCS_SERVO_SPEED 1500         // speed, default = 1500
#define SCS_SERVO_ACC 10             // acceleration, default = 100
#define SCS_SERVO_RXD 18             // eg. GPIO 18 for waveshare servo driver
#define SCS_SERVO_TXD 19             // eg. GPIO 19 for waveshare servo driver
#define SCS_SERVO_MAX_TEMPERATURE 45 // max temperature (celsius) before servo is disabled
#define SCS_SERVO_MAX_LOAD 400       // max load before servo is disabled

/* PCA9685 PWM servo settings */
// #define USE_PCA9685_PWM_SERVO    // uncomment this line if you want to use PCA9685 PWM servos
#define PCA9685_I2C_ADDRESS 0x40
#define PCA9685_SPEED 1500
#define PCA9685_ACC 10

/* MP3-Player YX5300 */
#define USE_MP3_PLAYER_YX5300
#define MP3_PLAYER_YX5300_RXD 13
#define MP3_PLAYER_YX5300_TXD 14

/* DAC speaker */
// #define USE_DAC_SPEAKER

#endif
