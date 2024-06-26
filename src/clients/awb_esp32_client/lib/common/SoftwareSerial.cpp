/*

SoftwareSerial.cpp - Implementation of the Arduino software serial for ESP8266.
Copyright (c) 2015-2016 Peter Lerup. All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

Adjusted for ESP32 December 2018 paulvha
See README for changes applied
*/

#include <Arduino.h>
#include <unistd.h>
#include "soc/gpio_struct.h"

// for potenial Debug
#define DPRINTLN(...) Serial.println(__VA_ARGS__)
#define DPRINT(...) Serial.print(__VA_ARGS__)

// The Arduino standard GPIO routines are not enough,
// must use some from the Espressif SDK as well
extern "C"
{
#include "esp32-hal-gpio.h"
}

#include "SoftwareSerial.h"

#define MAX_PIN 35

// As the Arduino attachInterrupt has no parameter, lists of objects
// and callbacks corresponding to each possible GPIO pins have to be defined
SoftwareSerial *ObjList[MAX_PIN + 1];

void IRAM_ATTR sws_isr_0() { ObjList[0]->rxRead(); };
void IRAM_ATTR sws_isr_1() { ObjList[1]->rxRead(); };
void IRAM_ATTR sws_isr_2() { ObjList[2]->rxRead(); };
void IRAM_ATTR sws_isr_3() { ObjList[3]->rxRead(); };
void IRAM_ATTR sws_isr_4() { ObjList[4]->rxRead(); };
void IRAM_ATTR sws_isr_5() { ObjList[5]->rxRead(); };
// Pin 6 to 11 can not be used
void IRAM_ATTR sws_isr_12() { ObjList[12]->rxRead(); };
void IRAM_ATTR sws_isr_13() { ObjList[13]->rxRead(); };
void IRAM_ATTR sws_isr_14() { ObjList[14]->rxRead(); };
void IRAM_ATTR sws_isr_15() { ObjList[15]->rxRead(); };
void IRAM_ATTR sws_isr_16() { ObjList[16]->rxRead(); };
void IRAM_ATTR sws_isr_17() { ObjList[17]->rxRead(); };
void IRAM_ATTR sws_isr_18() { ObjList[18]->rxRead(); };
void IRAM_ATTR sws_isr_19() { ObjList[19]->rxRead(); };
void IRAM_ATTR sws_isr_20() { ObjList[20]->rxRead(); };
void IRAM_ATTR sws_isr_21() { ObjList[21]->rxRead(); };
void IRAM_ATTR sws_isr_22() { ObjList[22]->rxRead(); };
void IRAM_ATTR sws_isr_23() { ObjList[23]->rxRead(); };
void IRAM_ATTR sws_isr_24() { ObjList[24]->rxRead(); };
void IRAM_ATTR sws_isr_25() { ObjList[25]->rxRead(); };
void IRAM_ATTR sws_isr_26() { ObjList[26]->rxRead(); };
void IRAM_ATTR sws_isr_27() { ObjList[27]->rxRead(); };
void IRAM_ATTR sws_isr_28() { ObjList[28]->rxRead(); };
void IRAM_ATTR sws_isr_29() { ObjList[29]->rxRead(); };
void IRAM_ATTR sws_isr_30() { ObjList[30]->rxRead(); };
void IRAM_ATTR sws_isr_31() { ObjList[31]->rxRead(); };
void IRAM_ATTR sws_isr_32() { ObjList[32]->rxRead(); };
void IRAM_ATTR sws_isr_33() { ObjList[33]->rxRead(); };
void IRAM_ATTR sws_isr_34() { ObjList[34]->rxRead(); };
void IRAM_ATTR sws_isr_35() { ObjList[35]->rxRead(); };

static void (*ISRList[MAX_PIN + 1])() = {
    sws_isr_0,
    sws_isr_1,
    sws_isr_2,
    sws_isr_3,
    sws_isr_4,
    sws_isr_5,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    NULL,
    sws_isr_12,
    sws_isr_13,
    sws_isr_14,
    sws_isr_15,
    sws_isr_16,
    sws_isr_17,
    sws_isr_18,
    sws_isr_19,
    sws_isr_20,
    sws_isr_21,
    sws_isr_22,
    sws_isr_23,
    sws_isr_24,
    sws_isr_25,
    sws_isr_26,
    sws_isr_27,
    sws_isr_28,
    sws_isr_29,
    sws_isr_30,
    sws_isr_31,
    sws_isr_32,
    sws_isr_33,
    sws_isr_34,
    sws_isr_35};

SoftwareSerial::SoftwareSerial(int receivePin, int transmitPin, bool inverse_logic, unsigned int buffSize)
{
    m_rxValid = m_txValid = m_txEnableValid = false;
    m_buffer = NULL;
    m_invert = inverse_logic;
    m_overflow = false;
    m_rxEnabled = false;
    if (isValidGPIOpin(receivePin))
    {
        m_rxPin = receivePin;
        m_buffSize = buffSize;
        m_buffer = (uint8_t *)malloc(m_buffSize);
        if (m_buffer != NULL)
        {
            m_rxValid = true;
            m_inPos = m_outPos = 0;
            pinMode(m_rxPin, INPUT);
            ObjList[m_rxPin] = this;
            // enableRx(true);              // if NOW enabled the sketch will hang if something is received before begin as baudrate is not set!
        }
    }

    if (isValidGPIOpin(transmitPin))
    {
        m_txValid = true;
        m_txPin = transmitPin;
        pinMode(m_txPin, OUTPUT);
        digitalWrite(m_txPin, !m_invert);
    }
}

SoftwareSerial::~SoftwareSerial()
{
    enableRx(false);

    if (m_rxValid)
        ObjList[m_rxPin] = NULL;
    if (m_buffer)
        free(m_buffer);
}

bool SoftwareSerial::isValidGPIOpin(int pin)
{
    return (pin >= 0 && pin <= 5) || (pin >= 12 && pin <= MAX_PIN);
}

void SoftwareSerial::begin(long speed)
{
    // Use getCycleCount() loop to get as exact timing as possible
    m_bitTime = ESP.getCpuFreqMHz() * 1000000 / speed;

    // By default enable interrupt during tx only for low speed
    m_intTxEnabled = speed < 9600;

    if (!m_rxEnabled)
        enableRx(true);
}

/* get baudrate that is set for softSerial */
long SoftwareSerial::baudRate()
{
    return ESP.getCpuFreqMHz() * 1000000 / m_bitTime;
}

/* set a debug pin for transmit */
void SoftwareSerial::setTransmitEnablePin(int transmitEnablePin)
{
    if (isValidGPIOpin(transmitEnablePin))
    {
        m_txEnableValid = true;
        m_txEnablePin = transmitEnablePin;
        pinMode(m_txEnablePin, OUTPUT);
    }
    else
    {
        m_txEnableValid = false;
    }
}

/* attach interrupt to the RX pin */
void SoftwareSerial::enableRx(bool on)
{
    if (m_rxValid)
    {

        if (on)
            // m_rxPin = receive pin to use, ISRList = user function to call, NO argurment,
            // rising in case of invert else falling (this is nearly always the case)
            attachInterrupt(m_rxPin, ISRList[m_rxPin], m_invert ? RISING : FALLING); // esp32-hal-gpio.c
        else
            detachInterrupt(m_rxPin);

        m_rxEnabled = on;
    }
}

/* read and remove byte from RX-buffer */
int SoftwareSerial::read()
{
    if (!m_rxValid || (m_inPos == m_outPos))
        return -1;
    uint8_t ch = m_buffer[m_outPos];
    m_outPos = (m_outPos + 1) % m_buffSize;
    return ch;
}

/* return the number of bytes available in RX-buffer */
int SoftwareSerial::available()
{
    if (!m_rxValid)
        return 0;
    /*
    DPRINT("avail : M_in  / m_out  ");
    DPRINT(m_inPos);
    DPRINT(" / ");
    DPRINTLN(m_outPos);
    */
    int avail = m_inPos - m_outPos;
    if (avail < 0)
        avail += m_buffSize;

    return avail;
}

#define WAIT                                            \
    {                                                   \
        start = ESP.getCycleCount();                    \
        while (ESP.getCycleCount() - start < m_bitTime) \
            if (m_intTxEnabled)                         \
                optimistic_yield(1);                    \
    }

size_t SoftwareSerial::write(uint8_t b)
{
    unsigned long start;

    if (!m_txValid)
        return 0;

    if (m_invert)
        b = ~b;

    if (m_txEnableValid)
        digitalWrite(m_txEnablePin, HIGH);

    // Disable interrupts in order to get a clean transmit
    portMUX_TYPE mux = portMUX_INITIALIZER_UNLOCKED;
    if (m_intTxEnabled)
        portENTER_CRITICAL(&mux);

    // make sure line is high to detect start
    digitalWrite(m_txPin, HIGH);
    WAIT;

    // Start bit;
    digitalWrite(m_txPin, LOW);
    WAIT;

    for (int i = 0; i < 8; i++)
    {

        digitalWrite(m_txPin, (b & 1) ? HIGH : LOW);
        WAIT;

        b >>= 1;
    }

    // Stop bit
    digitalWrite(m_txPin, HIGH);
    WAIT;

    // re-enable interrupts
    if (m_intTxEnabled)
        portEXIT_CRITICAL(&mux);

    if (m_txEnableValid)
        digitalWrite(m_txEnablePin, LOW);

    return 1;
}

/* flush RX buffer */
void SoftwareSerial::flush()
{
    m_inPos = m_outPos = 0;
}

/* get overflow status and reset */
bool SoftwareSerial::overflow()
{
    bool res = m_overflow;
    m_overflow = false;
    return res;
}

/* return next byte in RX-buffer if any */
int SoftwareSerial::peek()
{
    if (!m_rxValid || (m_inPos == m_outPos))
        return -1;
    return m_buffer[m_outPos];
}

#define WAITR                                      \
    {                                              \
        while (ESP.getCycleCount() - start < wait) \
            ;                                      \
        wait += m_bitTime;                         \
    }

void IRAM_ATTR SoftwareSerial::rxRead()
{

    // Advance the starting point for the samples
    // NO need to compensate for the initial delay which occurs before
    // the interrupt is delivered. The delay is 3.5us (measured). neglectable !
    unsigned long wait = m_bitTime + m_bitTime / 2.5;

    // this requires changes (adding IRAM_ATTR) in  ESP.cpp
    // line 103 uint32_t IRAM_ATTR EspClass::getCycleCount()
    unsigned long start = ESP.getCycleCount();
    uint8_t rec = 0;

    // Disable interrupts in order to get a clean transmit
    portMUX_TYPE mux = portMUX_INITIALIZER_UNLOCKED;
    portENTER_CRITICAL(&mux);

    for (int i = 0; i < 8; i++)
    {
        WAITR;
        // digitalWrite(m_txEnablePin, HIGH);    // debug only
        rec >>= 1;
        if (digitalRead(m_rxPin))
            rec |= 0x80;
        // digitalWrite(m_txEnablePin, LOW);     // debug only
    }
    if (m_invert)
        rec = ~rec;

    // Stop bit
    /* not waiting on the stop bit to pass will save time for the other
     * code below to execute & after the interrupt routine (yield()) to
     * be able to trigger on-time for the next byte.
     * The STOP bit is high anyway and who needs it ?? */
    // WAITR;
    portEXIT_CRITICAL(&mux);

    // Store the received value in the buffer unless we have an overflow
    int next = (m_inPos + 1) % m_buffSize;
    if (next != m_outPos)
    {
        m_buffer[m_inPos] = rec;
        m_inPos = next;
    }
    else
    {
        m_overflow = true;
    }

    // Must clear this bit in the interrupt register,
    // it gets set even when interrupts are disabled
    if (m_rxPin < 32)
        GPIO.status_w1tc = ((uint32_t)1 << m_rxPin);
    else
        GPIO.status1_w1tc.val = ((uint32_t)1 << (m_rxPin - 32));
}