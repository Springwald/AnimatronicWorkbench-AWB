// contains original code from https://github.com/m5stack/M5Stack

#include "DacSpeaker.h"
#include "Melody.h"

// BEEP PIN
#define SPEAKER_PIN 25
#define TONE_PIN_CHANNEL 0

DacSpeaker::DacSpeaker(void)
{
    _volume = 8;
    _begun = false;
}

void DacSpeaker::begin()
{
    _begun = true;
    ledcSetup(TONE_PIN_CHANNEL, 0, 13);
    ledcAttachPin(SPEAKER_PIN, TONE_PIN_CHANNEL);
    setBeep(1000, 100);
}

void DacSpeaker::end()
{
    mute();
    ledcDetachPin(SPEAKER_PIN);
    _begun = false;
}

void DacSpeaker::tone(uint16_t frequency)
{
    if (!_begun)
        begin();
    ledcWriteTone(TONE_PIN_CHANNEL, frequency);
    ledcWrite(TONE_PIN_CHANNEL, 0x400 >> _volume);
}

void DacSpeaker::tone(uint16_t frequency, uint32_t duration)
{
    tone(frequency);
    _count = millis() + duration;
    speaker_on = 1;
}

void DacSpeaker::beep()
{
    if (!_begun)
        begin();
    tone(_beep_freq, _beep_duration);
}

void DacSpeaker::click()
{
    if (!_begun)
        begin();
    setVolume(1);
    tone(200, _beep_duration);
    delay(20);
    mute();
}

void DacSpeaker::setBeep(uint16_t frequency, uint16_t duration)
{
    _beep_freq = frequency;
    _beep_duration = duration;
}

void DacSpeaker::setVolume(uint8_t volume)
{
    _volume = 11 - volume;
}

void DacSpeaker::mute()
{
    ledcWriteTone(TONE_PIN_CHANNEL, 0);
    digitalWrite(SPEAKER_PIN, 0);
}

void DacSpeaker::update()
{
    if (speaker_on)
    {
        if (millis() > _count)
        {
            speaker_on = 0;
            mute();
        }
    }
}

void DacSpeaker::write(uint8_t value)
{
    dacWrite(SPEAKER_PIN, value);
}

#define isdigit(n) (n >= '0' && n <= '9')

void DacSpeaker::playIntro()
{
    // original code from https://wiki.epfl.ch/robopoly/mario

#define OCTAVE_OFFSET -1

    Melody melody = Melody();

    char *p = melody.song;

    // Absolutely no error checking in here

    byte default_dur = 4;
    byte default_oct = 6;
    int bpm = 63;
    int num;
    long wholenote;
    long duration;
    byte note;
    byte scale;

    // format: d=N,o=N,b=NNN:
    // find the start (skip name, etc)

    while (*p != ':')
        p++; // ignore name
    p++;     // skip ':'

    // get default duration
    if (*p == 'd')
    {
        p++;
        p++; // skip "d="
        num = 0;
        while (isdigit(*p))
        {
            num = (num * 10) + (*p++ - '0');
        }
        if (num > 0)
            default_dur = num;
        p++; // skip comma
    }

    // Serial.print("ddur: "); Serial.println(default_dur, 10);

    // get default octave
    if (*p == 'o')
    {
        p++;
        p++; // skip "o="
        num = *p++ - '0';
        if (num >= 3 && num <= 7)
            default_oct = num;
        p++; // skip comma
    }

    // Serial.print("doct: "); Serial.println(default_oct, 10);

    // get BPM
    if (*p == 'b')
    {
        p++;
        p++; // skip "b="
        num = 0;
        while (isdigit(*p))
        {
            num = (num * 10) + (*p++ - '0');
        }
        bpm = num;
        p++; // skip colon
    }

    // Serial.print("bpm: "); Serial.println(bpm, 10);

    // BPM usually expresses the number of quarter notes per minute
    wholenote = (60 * 1000L / bpm) * 4; // this is the time for whole note (in milliseconds)

    // Serial.print("wn: "); Serial.println(wholenote, 10);

    // now begin note loop
    while (*p)
    {
        // first, get note duration, if available
        int num = 0;
        while (isdigit(*p))
        {
            num = (num * 10) + (*p++ - '0');
        }

        if (num > 0)
            duration = wholenote / num;
        else
            duration = wholenote / default_dur; // we will need to check if we are a dotted note after

        // now get the note
        note = 0;

        switch (*p)
        {
        case 'c':
            note = 1;
            break;
        case 'd':
            note = 3;
            break;
        case 'e':
            note = 5;
            break;
        case 'f':
            note = 6;
            break;
        case 'g':
            note = 8;
            break;
        case 'a':
            note = 10;
            break;
        case 'b':
            note = 12;
            break;
        case 'p':
        default:
            note = 0;
        }
        p++;

        // now, get optional '#' sharp
        if (*p == '#')
        {
            note++;
            p++;
        }

        // now, get optional '.' dotted note
        if (*p == '.')
        {
            duration += duration / 2;
            p++;
        }

        // now, get scale
        if (isdigit(*p))
        {
            scale = *p - '0';
            p++;
        }
        else
        {
            scale = default_oct;
        }

        scale += OCTAVE_OFFSET;

        if (*p == ',')
            p++; // skip comma for next note (or we may be at the end)

        // now play the note

        if (note)
        {
            // Serial.print("Playing: ");
            // Serial.print(scale, 10); Serial.print(' ');
            // Serial.print(note, 10); Serial.print(" (");
            // Serial.print(notes[(scale - 4) * 12 + note], 10);
            // Serial.print(") ");
            // Serial.println(duration, 10);
            tone(melody.notes[(scale - 4) * 12 + note]);
            delay(duration);
            // noTone(BUZZER);
            mute();
        }
        else
        {
            // Serial.print("Pausing: ");
            // Serial.println(duration, 10);
            mute();
            delay(duration);
        }
    }
}

void DacSpeaker::playMusic(const uint8_t *music_data, uint16_t sample_rate)
{
    uint32_t length = strlen((char *)music_data);
    uint16_t delay_interval = ((uint32_t)1000000 / sample_rate);
    if (_volume != 11)
    {
        for (int i = 0; i < length; i++)
        {
            dacWrite(SPEAKER_PIN, music_data[i] / _volume);
            delayMicroseconds(delay_interval);
        }

        for (int t = music_data[length - 1] / _volume; t >= 0; t--)
        {
            dacWrite(SPEAKER_PIN, t);
            delay(2);
        }
    }
    // ledcSetup(TONE_PIN_CHANNEL, 0, 13);
    ledcAttachPin(SPEAKER_PIN, TONE_PIN_CHANNEL);
}