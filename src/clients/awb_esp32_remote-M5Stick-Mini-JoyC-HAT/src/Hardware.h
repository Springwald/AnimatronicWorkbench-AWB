#ifndef hardware_h
#define hardware_h

/* Display settings */

// -- M5 Stack Displays --
#define DISPLAY_M5STACK // if you use a M5Stack, uncomment this line. Uses GPIO 14+18 for display communication

// -- SSD1306 Displays  --
// e.g. for Waveshare Servo Driver with ESP32: 128x32, 0x02, 0x3C
// #define DISPLAY_SSD1306
#define DISPLAY_SSD1306_I2C_ADDRESS 0x3D // 0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32
#define DISPLAY_SSD1306_WIDTH 128
#define DISPLAY_SSD1306_HEIGHT 64
#define DISPLAY_SSD1306_COM_PINS 0x12 // 0x02, 0x12, 0x22 or 0x32
                                      // The SSD1306 may have different connection patterns between the panel and
                                      // controller depending on the product, and the 0xDA command adjusts for this.
                                      // If it does not display correctly, try the following values of the



#endif
