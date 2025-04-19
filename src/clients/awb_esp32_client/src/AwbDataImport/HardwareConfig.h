#ifndef hardware_config_h
#define hardware_config_h

#define AwbClientId 1 // If you use more than one AWB-client, you have to enter different IDs per client here

/* Debugging settings */
#define DEBUGGING_IO_PIN 25          // the GPIO pin to use for debugging
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

#endif
