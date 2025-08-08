#ifndef hardware_h
#define hardware_h

// #define M5STICKC_PLUS2 1 // M5StickC Plus2 has a different power on/off mechanism than M5StickC

// M5 Stick Input Pins
#define POS_X 0    // Joystick
#define POS_Y 1    // Joystick
#define BUTTON 2   // Joystick press
#define BUTTON2 37 // M5Stick middle button
#define BUTTON3 39 // M5Stick top button

#ifdef M5STICKC_PLUS2
#define RED_LED 19        // M5StickC Plus2 red LED
#define RED_LED_ON HIGH   // M5StickC Plus2 HIGH
#define RED_LED_OFF LOW   // M5StickC Plus2 LOW
#define MAX_VOLTAGE 4.77f // M5StickC Plus2 max voltage
#define MIN_VOLTAGE 3.0f  // M5StickC Plus2 min voltage
#else
#define RED_LED 10        // M5StickC red LED
#define RED_LED_ON LOW    // M5StickC LOW
#define RED_LED_OFF HIGH  // M5StickC HIGH
#define MAX_VOLTAGE 4.00f // M5StickC max voltage
#define MIN_VOLTAGE 3.0f  // M5StickC min voltage
#endif

#endif // hardware_h
