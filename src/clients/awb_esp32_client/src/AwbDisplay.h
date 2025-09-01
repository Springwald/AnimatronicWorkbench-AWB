#ifndef awb_display_h
#define awb_display_h

#include <Arduino.h>
#include <vector>

#define MSG_TYPE_INFO 0
#define MSG_TYPE_ERROR 1

/**
 * The display class for the OLED or LCD display
 */
class AwbDisplay
{
private:
  int _debugStateMajor;  /// The major debug state
  int _debugStateMinor;  /// The minor debug state
  bool _debuggingActive; /// The debug state is active

  String _actualStatusInfo; /// The actual status message
  unsigned long _last_loop; /// The last time the loop was called
  bool _isSmallScreen;      /// The screen is small (e.g. 128x32)

  int _message_duration;      /// how long should the actual message be shown (in ms)
  int _message_duration_left; /// how long should the actual message still be shown (in ms)

  float _textSizeFont;       /// The text size for the font
  float _textSizePx;         /// The text size for the font in pixel
  float _textSizeLineHeight; /// The text size for the font in line height

  int _primarySpriteTop; /// The top position of the primary sprite

  String _values[100];       /// The actuator values to show
  int _valuesCount = 0;      /// The count of values to show
  bool _valuesDirty = false; /// The values are dirty and must be redrawn

  void showTopBar(String message); /// Show the top bar with the given message
  void draw_debuggingState();      // Draw the small live(!) debug state

  void draw_string(String message, int backCol);
  String getNextLine(String input, uint maxLineLength, std::vector<char> splitChars, bool forceFirstSplit);

public:
  /**
   * a human readbable information about the actual memory usage
   */
  String memoryInfo;

  /**
   * Set up the display
   */
  void setup(unsigned int clientId);

  /**
   * Loop the display
   */
  void loop();

  void clear();

  /**
   * Set the status message
   */
  void set_actual_status_info(String infos);

  void set_debuggingState(bool isDebugging, int major, int minor);

  /**
   * Show a message on the display
   */
  void draw_message(String message, int durationMs, int msgType);

  /**
   * true, if actually a message is shown
   */
  bool isShowingMessage();
};

#endif
