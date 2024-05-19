#include "Arduino.h"
#include "Hardware.h"
#include "AwbDisplay.h"
#include <vector>

#define LGFX_AUTODETECT
// #define LGFX_USE_V1

#include <LovyanGFX.hpp>
#include <LGFX_AUTODETECT.hpp>

#ifdef DISPLAY_SSD1306

class LGFX_SSD1306 : public lgfx::LGFX_Device
{
    lgfx::Panel_SSD1306 _panel_instance;
    lgfx::Bus_I2C _bus_instance;

public:
    LGFX_SSD1306()
    {
        {
            auto cfg = _bus_instance.config();
            cfg.i2c_port = 1;
            cfg.freq_write = 400000;
            cfg.freq_read = 400000;
            cfg.pin_sda = 21;
            cfg.pin_scl = 22;
            cfg.i2c_addr = 0x3C;

            _bus_instance.config(cfg);
            _panel_instance.setBus(&_bus_instance);
            _panel_instance.setComPins(DISPLAY_SSD1306_COM_PINS);
        }
        {
            auto cfg = _panel_instance.config();
            cfg.panel_width = DISPLAY_SSD1306_WIDTH;
            cfg.panel_height = DISPLAY_SSD1306_HEIGHT;
            _panel_instance.config(cfg);
        }
        setPanel(&_panel_instance);
    }
};

static LGFX_SSD1306 lcd; // Create an instance of LGFX_SSD1306 (class LGFX_SSD1306 to do things with lcd)
#endif

#ifdef DISPLAY_M5STACK
static LGFX lcd;
#endif

static LGFX_Sprite topBarSprite(&lcd);
static LGFX_Sprite primarySprite(&lcd);
static LGFX_Sprite debugStateSprite(&lcd);
static const lgfx::IFont *font = nullptr;

/**
 * Setup the display
 */
void AwbDisplay::setup(int clientId)
{
    lcd.init();
    _isSmallScreen = lcd.height() <= 64 || lcd.width() <= 290;

    const lgfx::IFont *font = nullptr;

#ifdef DISPLAY_SSD1306
    int colorDepth = 0;
    lcd.setRotation(2);
#endif

#ifdef DISPLAY_M5STACK
    int colorDepth = 8;
    lcd.setRotation(1);
#endif

    // set up the debug sprite
    debugStateSprite.setTextColor(0xFFFFFFU, 0x000000U);
    debugStateSprite.setFont(&fonts::DejaVu9);
    debugStateSprite.setTextSize(1.0f);
    debugStateSprite.setColorDepth(colorDepth);
    debugStateSprite.createSprite(40, 10);

    // set up the fonts
    if (_isSmallScreen)
    {
        font = &fonts::Font0;
        // font = &fonts::DejaVu9;
        // font = &fonts::TomThumb;
        _textSizeFont = 1.0f;
        _textSizePx = _textSizeFont * 8;
        _textSizeLineHeight = _textSizePx + 1;
    }
    else
    {
        font = &fonts::DejaVu18;
        _textSizeFont = 1.0f;
        _textSizePx = _textSizeFont * 18;
        _textSizeLineHeight = _textSizePx + 2;
    }

    topBarSprite.setFont(font);
    topBarSprite.setTextSize(_textSizeFont);
    topBarSprite.setColorDepth(colorDepth);

    primarySprite.setFont(font);
    primarySprite.setTextSize(_textSizeFont);
    primarySprite.setColorDepth(colorDepth);

    if (_isSmallScreen)
    {
        lcd.fillScreen(0xFFFFFFU);

        // set up the top bar
        // on small screen we don't have enough space for the top bar, so it's only shown fullscreen for a short time at startup
        topBarSprite.createSprite(lcd.width(), lcd.height());

        // set up the primary sprite
        _primarySpriteTop = 0;
        primarySprite.createSprite(lcd.width(), lcd.height());
    }
    else
    {
        // set up the top bar
        topBarSprite.createSprite(lcd.width(), _textSizeLineHeight + 2);

        // set up the primary sprite
        _primarySpriteTop = topBarSprite.height();
        primarySprite.createSprite(lcd.width(), _statusFooterSpriteTop - _primarySpriteTop);
    }

    lcd.setColorDepth(colorDepth);

    String title = _isSmallScreen ? "AWB" : "AnimatronicWorkBench";
    showTopBar(title + " - ID " + (String)clientId + "");

    if (_isSmallScreen)
    {
        // on small screen we don't have enough space for the top bar, so it's only shown fullscreen for a short time at startup
        // delay(3000);
    }
}

void AwbDisplay::set_actual_status_info(String infos)
{
    _actualStatusInfo = infos;
}

void AwbDisplay::showTopBar(String message)
{
    topBarSprite.fillSprite(0xCC3300U);
    topBarSprite.setTextColor(0xFFFFFFU);
    topBarSprite.setTextDatum(middle_center);
    topBarSprite.drawString(message, topBarSprite.width() / 2, topBarSprite.height() / 2);
    topBarSprite.setColor(0, 0, 0);
    topBarSprite.drawFastHLine(0, topBarSprite.height() - 1, lcd.width());
    topBarSprite.pushSprite(0, 0);
}

void AwbDisplay::clear()
{
}

void AwbDisplay::loop()
{
    int diff = millis() - _last_loop;
    _last_loop = millis();

    if (_message_duration_left > 0)
    {
        _message_duration_left -= diff;

        if (_message_duration_left > 0)
        {
            // draw a progress bar for the message
            primarySprite.setColor(255, 255, 255);
            long bar = min(primarySprite.width(), (_message_duration_left * lcd.width()) / _message_duration);
            for (int i = 0; i < (_isSmallScreen ? 2 : 4); i++)
                primarySprite.drawFastHLine(0, primarySprite.height() - i, bar);
            primarySprite.pushSprite(0, _primarySpriteTop);
        }
        else
        {
            // remove the message
            _message_duration_left = 0;
            primarySprite.fillSprite(0x000000U);
            primarySprite.pushSprite(0, _primarySpriteTop);
        }
    }
    else
    {
        draw_string(_actualStatusInfo, 0x000000U);
    }
    draw_debuggingState();
}

bool AwbDisplay::isShowingMessage()
{
    return _message_duration_left > 0;
}

void AwbDisplay::set_debuggingState(bool isDebugging, int major, int minor)
{
    _debuggingActive = isDebugging;
    _debugStateMajor = major;
    _debugStateMinor = minor;
    draw_debuggingState();
}

void AwbDisplay::draw_debuggingState()
{
    // if (_debuggingActive == false)
    //     return;

    debugStateSprite.fillSprite(0x000000U);
    debugStateSprite.setTextDatum(top_left);
    auto debugState = String(_debugStateMajor) + "-" + String(_debugStateMinor);
    debugStateSprite.drawString(debugState, 0, 0);
    debugStateSprite.pushSprite(lcd.width() - debugStateSprite.width(), lcd.height() - debugStateSprite.height());
}

void AwbDisplay::draw_message(String message, int durationMs, int msgType)
{
    _message_duration_left = durationMs;
    _message_duration = durationMs;

    unsigned int backCol = 0;

    if (!_isSmallScreen)
    {
        switch (msgType)
        {
        case MSG_TYPE_INFO: // info
            backCol = 0x0000FFU;
            break;
        case MSG_TYPE_ERROR: // error
            backCol = 0xFF0000U;
            break;
        }
    }

    draw_string(message, backCol);
}

/// @brief draw a fullscreen info text
void AwbDisplay::draw_string(String message, int backCol)
{
    primarySprite.setTextColor(0xFFFFFFU, backCol);
    primarySprite.fillScreen(backCol);
    primarySprite.setTextDatum(top_center);

    int maxPerLine = (int)(primarySprite.width() / primarySprite.textWidth("o"));

    auto nativeLineBreaks = std::vector<char>{'\r', '\n'};
    auto attractiveLineBreaks = std::vector<char>{' ', '\t'};
    auto fallbackBreaks = std::vector<char>{',', ':', '/', '=', '{', '}', '?'};

    std::vector<String> lines{};
    while (message.length() > 0)
    {
        String line = getNextLine(message, maxPerLine, nativeLineBreaks, true);
        if (line.length() == 0 || line.length() > maxPerLine)
        {
            line = getNextLine(message, maxPerLine, attractiveLineBreaks, false);
        }
        if (line.length() == 0 || line.length() > maxPerLine)
        {
            line = getNextLine(message, maxPerLine, fallbackBreaks, false);
        }
        if (line.length() == 0 || line.length() > maxPerLine)
        {
            line = message.substring(0, maxPerLine);
        }

        if (line.length() == 0)
        {
            lines.push_back(message);
            break;
        }
        else
        {
            lines.push_back(line);
            message = message.substring(line.length());
        }
    }

    int y = max(0, (int)(((primarySprite.height()) - ((lines.size() + 1) * _textSizeLineHeight)) / 2));
    for (int i = 0; i < lines.size(); i++)
    {
        // draw the line
        primarySprite.drawString(lines[i], primarySprite.width() / 2, y);
        y += _textSizeLineHeight;
    }

    primarySprite.pushSprite(0, _primarySpriteTop);
    draw_debuggingState();
}

String AwbDisplay::getNextLine(String input, uint maxLineLength, std::vector<char> splitChars, bool forceFirstSplit)
{
    int bestSplitPos = -1;
    for (int i = 2; i < min(maxLineLength, input.length()); i++)
    {
        char c = input.charAt(i);
        for (int j = 0; j < splitChars.size(); j++)
        {
            char splitChar = splitChars[j];
            if (c == splitChar)
            {
                bestSplitPos = i;
                if (forceFirstSplit)
                    return input.substring(0, bestSplitPos);
            }
        }
    }

    if (bestSplitPos == -1)
        return input;
    return input.substring(0, bestSplitPos);
}