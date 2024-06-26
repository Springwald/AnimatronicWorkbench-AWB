/*
SoftwareSerial.h

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
* See README for changes applied
*/

#ifndef SoftwareSerial_h
#define SoftwareSerial_h

#include <inttypes.h>
#include <Stream.h>

// This class is compatible with the corresponding AVR one,
// the constructor however has an optional rx buffer size.

class SoftwareSerial : public Stream
{
public:
    SoftwareSerial(int receivePin, int transmitPin, bool inverse_logic = false, unsigned int buffSize = 128); // default buffsize was 64
    ~SoftwareSerial();

    void begin(long speed);
    long baudRate();                                  // GET current baudrate
    void setTransmitEnablePin(int transmitEnablePin); // debug pulse

    bool overflow(); // get and reset overflow status
    int peek();      // read but NOT remove next RX byte (if any)
                   
    void rxRead();  // read from pin based on interrupt

    virtual size_t write(uint8_t byte);
    virtual int read();
    virtual int available();
    virtual void flush();
    operator bool() { return m_rxValid || m_txValid; }

    // AVR compatibility methods
    bool listen()
    {
        enableRx(true);
        return true;
    }
    void end() { stopListening(); }
    bool isListening() { return m_rxEnabled; }
    bool stopListening()
    {
        enableRx(false);
        return true;
    }

    using Print::write;

private:
    bool isValidGPIOpin(int pin);

    // Disable or enable interrupts on the rx pin
    void enableRx(bool on);

    // Member variables
    int m_rxPin, m_txPin, m_txEnablePin;
    bool m_rxValid, m_rxEnabled;
    bool m_txValid, m_txEnableValid;
    bool m_invert, m_intTxEnabled;
    bool m_overflow;
    unsigned long m_bitTime;
    unsigned int m_inPos, m_outPos;
    int m_buffSize;
    uint8_t *m_buffer;
};

// If only one tx or rx wanted then use this as parameter for the unused pin
#define SW_SERIAL_UNUSED_PIN -1

#endif