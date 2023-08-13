// contains original code from https://github.com/m5stack/M5Stack

#ifndef _DAC_SPEAKER_H_
#define _DAC_SPEAKER_H_

#include "Arduino.h"
// #include "Config.h"

#ifdef __cplusplus
extern "C"
{
#endif /* __cplusplus */
#include "esp32-hal-dac.h"
#ifdef __cplusplus
}
#endif /* __cplusplus */

class DacSpeaker
{
public:
    DacSpeaker(void);

    void begin();
    void end();
    void mute();
    void tone(uint16_t frequency);
    void tone(uint16_t frequency, uint32_t duration);
    void beep();
    void click();
    void setBeep(uint16_t frequency, uint16_t duration);
    void update();

    void write(uint8_t value);
    void setVolume(uint8_t volume);
    void playMusic(const uint8_t *music_data, uint16_t sample_rate);
    void playIntro();

private:
    uint32_t _count;
    uint8_t _volume;
    uint16_t _beep_duration;
    uint16_t _beep_freq;
    bool _begun;
    bool speaker_on;
};
#endif