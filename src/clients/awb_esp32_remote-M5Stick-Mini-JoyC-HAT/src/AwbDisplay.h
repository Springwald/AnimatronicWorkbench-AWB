#ifndef awb_display_h
#define awb_display_h

#include <vector>

#define MSG_TYPE_INFO 0
#define MSG_TYPE_ERROR 1

/**
 * The display class for the OLED or LCD display
 */
class AwbDisplay
{
private:
  int _freeMemoryOnStart;       /// The free memory on start
  String _statusMsg;            /// The last / actual status message
  int _last_debugInfos_changed; /// The last time the debug infos were changed
  unsigned long _last_loop;     /// The last time the loop was called
  bool _statusDirty;            /// The status is dirty and must be redrawn
  bool _isSmallScreen;          /// The screen is small (e.g. 128x32)
  bool _debugInfosVisible;      /// The debug infos are visible

  int _message_duration;      /// how long should the actual message be shown (in ms)
  int _message_duration_left; /// how long should the actual message still be shown (in ms)

  float _textSizeFont;       /// The text size for the font
  float _textSizePx;         /// The text size for the font in pixel
  float _textSizeLineHeight; /// The text size for the font in line height

  int _primarySpriteTop;      /// The top position of the primary sprite
  int _statusFooterSpriteTop; /// The top position of the status footer sprite

  String _values[100];       /// The actuator values to show
  int _valuesCount = 0;      /// The count of values to show
  bool _valuesDirty = false; /// The values are dirty and must be redrawn

  int getFreeMemory(); /// Get the free memory

  void showTopBar(String message); /// Show the top bar with the given message
  void draw_values();              /// Draw the values
  bool draw_debugInfos();          /// Draw the debug infos
  String getNextLine(String input, uint maxLineLength, std::vector<char> splitChars, bool forceFirstSplit);

public:
  /**
   * a human readbable information about the actual memory usage
   */
  String memoryInfo;

  /**
   * Set up the display
   */
  void setup();

  /**
   * Loop the display
   */
  void loop();

  void resetDebugInfos();
  void clear();

  /**
   * Set the status message
   */
  void set_statusMsg(String message);

  /**
   * Set the actuator values to show
   */
  void set_values(String values[], int count);

  /**
   * Set a debug status message to show
   */
  void set_debugStatus(String message);

  /**
   * force to redraw the debug status message
   */
  void set_debugStatus_dirty();

  /**
   * Show a message on the display
   */
  void draw_message(String message, int durationMs, int msgType);

  /**
   * true, if actually a message is shown
   */
  bool isShowingMessage();

  /*
  disables any input for the given time
  */
  void pause(int seconds, String message);
};

#endif
