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

            // The SSD1306 may have different connection patterns between the panel and
            // controller depending on the product, and the 0xDA command adjusts for this.
            // If it does not display correctly, try the following values of the
            // setComPins function one at a time:
            _panel_instance.setComPins(0x02);
            //  _panel_instance.setComPins(0x12);
            //  _panel_instance.setComPins(0x22);
            //  _panel_instance.setComPins(0x32);
        }
        {
            auto cfg = _panel_instance.config();
            cfg.panel_width = 128;
            cfg.panel_height = 32;
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
static LGFX_Sprite statusFooterSprite(&lcd);

static const lgfx::IFont *font = nullptr;

/**
 * Setup the display
 */
void AwbDisplay::setup(int clientId)
{
    resetDebugInfos();
    lcd.init();
    _isSmallScreen = lcd.height() <= 64 || lcd.width() <= 1290;

    const lgfx::IFont *font = nullptr;

#ifdef DISPLAY_SSD1306
    int colorDepth = 0;
    lcd.setRotation(2);
#endif

#ifdef DISPLAY_M5STACK
    int colorDepth = 8;
    lcd.setRotation(1);
#endif

    if (_isSmallScreen)
    {
        font = &fonts::Font0;
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

    statusFooterSprite.setFont(font);
    statusFooterSprite.setTextSize(_textSizeFont);
    statusFooterSprite.setColorDepth(colorDepth);

    if (_isSmallScreen)
    {
        lcd.fillScreen(0xFFFFFFU);

        // set up the top bar
        // on small screen we don't have enough space for the top bar, so it's only shown fullscreen for a short time at startup
        topBarSprite.createSprite(lcd.width(), lcd.height());

        // set up the status footer
        _statusFooterSpriteTop = 0;
        statusFooterSprite.createSprite(lcd.width(), lcd.height());

        // set up the primary sprite
        _primarySpriteTop = 0;
        primarySprite.createSprite(lcd.width(), lcd.height());
    }
    else
    {
        // set up the top bar
        topBarSprite.createSprite(lcd.width(), _textSizeLineHeight + 2);

        // set up the status footer
        statusFooterSprite.createSprite(lcd.width(), _textSizeLineHeight * 2);
        _statusFooterSpriteTop = lcd.height() - statusFooterSprite.height();

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

    set_debugStatus("started...");
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

int AwbDisplay::getFreeMemory()
{
    return ESP.getFreeHeap(); // ESP.getMaxAllocHeap(), ESP.getMinFreeHeap()
}

void AwbDisplay::resetDebugInfos()
{
    _last_loop = millis();
    _freeMemoryOnStart = getFreeMemory();
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
                primarySprite.drawFastHLine(bar, primarySprite.height() - i, primarySprite.width() - bar);
            primarySprite.pushSprite(0, _primarySpriteTop);
        }
        else
        {
            // remove the message
            _message_duration_left = 0;
            primarySprite.fillSprite(0xFFFFFFU);
            primarySprite.pushSprite(0, _primarySpriteTop);
        }
    }
    else
    {
        _debugInfosVisible = draw_debugInfos();
        if (_debugInfosVisible == false || _isSmallScreen == false)
        {
            draw_values();
        }
    }
}

bool AwbDisplay::isShowingMessage()
{
    return _message_duration_left > 0;
}

void AwbDisplay::set_values(String values[], int count)
{
    bool changed = count != _valuesCount;

    for (int i = 0; i < count; i++)
    {
        if (values[i] != _values[i])
        {
            changed = true;
            _values[i] = values[i];
        }
    }
    _valuesDirty = true; // changed;
    _valuesCount = count;
}

void AwbDisplay::draw_values()
{
    if (_valuesDirty == false)
        return;

    _valuesDirty = false;
    int columns = 3;

    if (_isSmallScreen)
    {
        primarySprite.fillSprite(0x000000U);
        primarySprite.setTextColor(0xFFDDFFU);
    }
    else
    {
        primarySprite.fillSprite(0xFFDDFFU);
        primarySprite.setTextColor(0x000000U);
    }

    primarySprite.setTextDatum(top_left);

    int margin = _isSmallScreen ? 0 : 4;
    int y = margin;
    int x = margin;
    int column = 0;
    int columnWidth = primarySprite.width() / columns;
    bool colCount;

    for (int i = 0; i < _valuesCount; i++)
    {
        if (_isSmallScreen)
        {
            colCount = !colCount;
            if (colCount)
            {
                primarySprite.setTextColor(0xFFFFFFU, 0x000000U);
            }
            else
            {
                primarySprite.setTextColor(0x000000U, 0xFFFFFFU);
            }
        }

        x = column * columnWidth + margin;
        if (column > 0)
            x += 3;

        primarySprite.drawString(_values[i], x, y);
        column++;
        if (column >= columns)
        {
            column = 0;
            y += _textSizeLineHeight;
        }
        if (y > primarySprite.height())
            break;
    }

    primarySprite.pushSprite(0, _primarySpriteTop);
}

// send alife packet

void AwbDisplay::set_debugStatus(String message)
{
    _statusDirty = true;
    _last_debugInfos_changed = millis();
    _statusMsg = message;
}

/// @brief draw a fullscreen info text
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
            set_debugStatus(message); // Mirror to debug status
            break;
        }
    }
    primarySprite.setTextColor(0xFFFFFFU, backCol);

    // primarySprite.fillScreen(backCol);
    primarySprite.fillScreen(backCol);
    primarySprite.setTextDatum(top_center);

    int maxPerLine = (int)(primarySprite.width() / primarySprite.textWidth("o"));

    String line = "";
    int lineCount = 0;

    std::vector<String> lines{};

    while (message.length() > 0)
    {
        int spacePos = message.indexOf(" ");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf(".");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf(",");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf(":");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf("=");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf("{");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = message.indexOf("}");
        if (spacePos > maxPerLine || spacePos == -1)
            spacePos = maxPerLine;

        // check if there is a space in the message (if not, spacePos will be -1
        if (spacePos == -1)
        {
            spacePos = message.length() - 1;
        }
        if (spacePos > 0)
        {
            if (line.length() + spacePos < maxPerLine)
            {
                // enough space in the line
                if (line.length() > 0)
                {
                    line += " ";
                }
            }
            else
            {
                // no more space in the line
                if (line.length() > 0)
                {
                    // draw the line and start a new one
                    lines.push_back(line);
                    lineCount++;
                    line = "";
                }
            }
            line += message.substring(0, spacePos + 1);
            message = message.substring(spacePos + 1);
        }
        else
        {
            line = message;
            message = "";
        }
    }

    if (line.length() > 0)
    {
        lines.push_back(line);
    }

    int y = max(0, (int)(((primarySprite.height()) - ((lineCount + 1) * _textSizeLineHeight)) / 2));
    for (int i = 0; i < lines.size(); i++)
    {
        // draw the line
        primarySprite.drawString(lines[i], primarySprite.width() / 2, y);
        y += _textSizeLineHeight;
    }

    primarySprite.pushSprite(0, _primarySpriteTop);
}

void AwbDisplay::set_debugStatus_dirty()
{
    _statusDirty = true;
}

bool AwbDisplay::draw_debugInfos()
{
    if (_message_duration_left > 0) // primary message is visible
    {
        if (_isSmallScreen == true)
        {
            return false; // message is visible and concurs with debug infos
        }
    }

    if (_statusDirty == false)
        return false; // no change

    if (millis() > _last_debugInfos_changed + 2000) // show debug infos for 2 seconds
    {
        _statusDirty = false;
    }

    int freeMemory = getFreeMemory();

    statusFooterSprite.setTextColor(0xFFFFFFU, 0);
    statusFooterSprite.setTextDatum(top_center);

    int y = 0;

    statusFooterSprite.fillScreen(0x000000);

    y++;
    int lostMemory = _freeMemoryOnStart - freeMemory;
    memoryInfo = "free:" + String(freeMemory / 1024) + "k lost:" + String(lostMemory / 1024) + "." + String((lostMemory % 1024) / 100) + "k";
    statusFooterSprite.drawString(memoryInfo, statusFooterSprite.width() / 2, y);

    y += _textSizeLineHeight;
    statusFooterSprite.setTextColor(0xAAAAFFU, 0);
    statusFooterSprite.drawString(_statusMsg, statusFooterSprite.width() / 2, y);

    statusFooterSprite.pushSprite(0, _statusFooterSpriteTop);

    return true;
}