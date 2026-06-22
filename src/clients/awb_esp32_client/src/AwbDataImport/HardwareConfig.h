#ifndef hardware_config_h
#define hardware_config_h

#define AwbClientId 100 // If you use more than one AWB-client, you have to enter different IDs per client here

/* Display settings */

// -- SSD1306 Displays  --
// e.g. for Waveshare Servo Driver with ESP32: 128x32, 0x02, 0x3C
#define DISPLAY_SSD1306
#define DISPLAY_SSD1306_I2C_ADDRESS 0x3C // 0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
#define DISPLAY_SSD1306_WIDTH 128
#define DISPLAY_SSD1306_HEIGHT 32
#define DISPLAY_SSD1306_COM_PINS 0x02 // 0x02, 0x12, 0x22 or 0x32

/* SCS serial servo settings */
#define USE_SCS_SERVO
#define SCS_SERVO_RXD 18
#define SCS_SERVO_TXD 19


#endif
