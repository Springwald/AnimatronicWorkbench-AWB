#ifndef hardware_config_h
#define hardware_config_h

/* Debugging settings */
#define DEBUGGING_IO_PIN 1 // the GPIO pin to use for debugging
#define DEBUGGING_IO_PIN_ACTIVE HIGH // if the debugging pin is active low, set this to true

/* Display settings */

// -- SSD1306 Displays  --
// e.g. for Waveshare Servo Driver with ESP32: 128x32, 0x02, 0x3C
#define DISPLAY_SSD1306
#define DISPLAY_SSD1306_I2C_ADDRESS 0x3D // 0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
#define DISPLAY_SSD1306_WIDTH 128
#define DISPLAY_SSD1306_HEIGHT 64
#define DISPLAY_SSD1306_COM_PINS 0x12 // 0x02, 0x12, 0x22 or 0x32

/* STS serial servo settings */
#define USE_STS_SERVO
#define STS_SERVO_RXD 16
#define STS_SERVO_TXD 17

/* SCS serial servo settings */
#define USE_SCS_SERVO
#define SCS_SERVO_RXD 18
#define SCS_SERVO_TXD 19

/* autoplay state selector */
// if a servo position feedback is used as a state selector, define the servo channel here.
// if you don't use a servo as state selector, set this to -1 or undefine it
// #define AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL 9
// if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
// if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
#define AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET 457

/* DAC speaker */
// #define USE_DAC_SPEAKER
/* Neopixel status LEDs */
// #define USE_NEOPIXEL_STATUS_CONTROL
#define STATUS_RGB_LED_GPIO 23      // the GPIO used to control RGB LEDs. GPIO 23, as default.
#define STATUS_RGB_LED_NUMPIXELS 13 // how many RGB LEDs are connected to the GPIO


#endif
