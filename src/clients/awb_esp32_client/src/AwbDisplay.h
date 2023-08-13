#ifndef awb_display_h
#define awb_display_h

#define MSG_TYPE_INFO 0
#define MSG_TYPE_ERROR 1

class AwbDisplay
{
private:
  int _freeMemoryOnStart;
  String _statusMsg;
  int _last_debugInfos_changed;
  unsigned long _last_loop;
  bool _statusDirty;
  bool _isSmallScreen;
  bool _debugInfosVisible;

  int _message_duration;
  int _message_duration_left;

  float _textSizeFont;
  float _textSizePx;
  float _textSizeLineHeight;

  int _primarySpriteTop;
  int _statusFooterSpriteTop;

  String _values[100];
  int _valuesCount = 0;
  bool _valuesDirty = false;

  int getFreeMemory();

  void showTopBar(String message);
  void draw_values();
  bool draw_debugInfos();

public:
  void setup(int clientId);
  void loop();

  void resetDebugInfos();

  void clear();

  void set_values(String values[], int count);
  void set_debugStatus(String message);
  void set_debugStatus_dirty();

  void draw_message(String message, int durationMs, int msgType);
  bool isShowingMessage();
};

#endif
