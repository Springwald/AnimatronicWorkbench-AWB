
#include <Arduino.h>
#include <Adafruit_NeoPixel.h>
#include "NeoPixelStatusControl.h"

void NeoPixelStatusControl::showActivity()
{
    _activityDurationMs = _activityStandardDurationMs;
}

void NeoPixelStatusControl::setState(int state, int durationMs)
{
    _state = state;
    _stateDurationMs = durationMs;
    update();
}

/// show alarm neopixel on startup to see unexpected restarts
void NeoPixelStatusControl::setStartUpAlert()
{
    for (int i = 0; i < STATUS_RGB_LED_NUMPIXELS; i++)
    {
        _matrix.setPixelColor(i, _matrix.Color(128, 0, 0));
    }
    _isStartupRed = true;
    _matrix.show();
    _lastUpdateMs = millis() + 2000; // start with a delay to see the fake startup error state
}

void NeoPixelStatusControl::setStateIdle()
{
    _state = STATE_IDLE;
    _stateDurationMs = 0;
}

void NeoPixelStatusControl::update()
{
    if (_isStartupRed)
    {
        if (millis() < _lastUpdateMs)
            return;
        _isStartupRed = false;
        for (int i = 0; i < STATUS_RGB_LED_NUMPIXELS; i++)
        {
            _matrix.setPixelColor(i, _matrix.Color(0, 0, 0));
        }
    }

    long ms = millis();
    long diffMs = ms - _lastUpdateMs;

    if (_stateDurationMs > 0)
    {
        _stateDurationMs -= diffMs;
        if (_stateDurationMs <= 0)
            _state = STATE_IDLE;
    }

    _lastUpdateMs = ms;

    int speed = 2000; // normal: slow pulse
    int value;
    int bright;

    switch (_state)
    {
    case STATE_IDLE:

        // slow pulse green
        _matrix.setPixelColor(0, _matrix.Color(0, getRgbVal(0, 3000, IDLE_BRIGHTNESS), 0));

        // pulse blue, wehen activity is detected
        if (_activityDurationMs > 0)
        {
            auto value = min(ACIVITY_BRIGHTNESS, ACIVITY_BRIGHTNESS * _activityDurationMs / _activityStandardDurationMs);
            _matrix.setPixelColor(1, _matrix.Color(0, 0, value));
            _matrix.setPixelColor(5, _matrix.Color(0, getRgbVal(0, 3000, IDLE_BRIGHTNESS), 0)); // also one external LED
            _activityDurationMs -= diffMs;
        }
        else
        {
            _matrix.setPixelColor(1, _matrix.Color(0, 0, 0));
        }
        groguSpecialAnimation();
        break;

    case STATE_ALARM:
        // fast pulse red
        for (int i = 0; i < STATUS_RGB_LED_NUMPIXELS; i++)
        {
            _matrix.setPixelColor(i, _matrix.Color(getRgbVal(i, 1000, ERROR_BRIGHTNESS), 0, 0));
        }
        break;

    default:
        break;
    }

    _matrix.show();
}

/// special animation for Grogu, needs to put to a separate class later to keep this class more generic
void NeoPixelStatusControl::groguSpecialAnimation()

{
    if (millis() < _lastGroguAnimationMs + 100)
        return;
    _lastGroguAnimationMs = millis();

    // LED top
    for (int i = 2; i < 6; i++)
    {
        if (i == 3)
        {
            _matrix.setPixelColor(i, _matrix.Color(
                                         abs((int)((millis() / 50) % 128 - 64)),
                                         abs((int)((millis() / 30) % 128 - 64)),
                                         abs((int)((millis() / 70) % 128 - 64))));
        }
        else
        {
            if (random(1, 100) == 1)
            {
                auto rnd = random(0, 25);
                if (rnd < 20)
                {
                    _matrix.setPixelColor(i, _matrix.Color(0, 0, 0));
                }
                else
                {
                    auto randomColor = _matrix.Color(rnd, rnd, rnd);
                    _matrix.setPixelColor(i, randomColor);
                }
            }
        }
    }

    // Lens on ball housing
    for (int i = 6; i < STATUS_RGB_LED_NUMPIXELS; i++)
    {
        auto rnd = random(0, 6);
        if (rnd == 0)
        {
            auto randomColor = _matrix.Color(random(0, 10), random(0, 10), random(10, 24));
            _matrix.setPixelColor(i, randomColor);
        }
        else if (rnd == 1 || rnd == 2 || rnd == 3)
        {
            _matrix.setPixelColor(i, _matrix.Color(0, 0, 0));
        }
    }
}

void NeoPixelStatusControl::setSingleLED(uint16_t LEDnum, uint32_t c)
{
    _matrix.setPixelColor(LEDnum, c);
    _matrix.show();
}

int NeoPixelStatusControl::getRgbVal(int ledIndex, int speed, int base)
{
    double value = 2 * (millis() % speed) / (speed * 1.0);
    if (value > 1)
        value = 2 - value;
    if (ledIndex == 1)
        value = 1 - value; // second led is inverted
    int intValue = (int)(value * base);
    return min(base, max(0, intValue));
}