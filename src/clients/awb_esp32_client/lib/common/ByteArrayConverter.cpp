#include "ByteArrayConverter.h"

using byte = unsigned char;

// static function to convert unsigned int to an array of 4 byte
byte *ByteArrayConverter::UintTo4Bytes(unsigned int value)
{
    return new byte[4]{(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value};
}

// static function to convert an array of 4 byte to unsigned int
unsigned int ByteArrayConverter::UintFrom4Bytes(byte value[])
{
    return (unsigned int)(value[0] << 24 | value[1] << 16 | value[2] << 8 | value[3]);
}

// to prevent usage of the reserved bytes for packet headers, we split the bytes into 2 parts
byte *ByteArrayConverter::SplitByte(byte value)
{
    auto result = new byte[2];
    if (value > 200)
    {
        result[0] = 200;
        result[1] = (byte)(value - 200);
    }
    else
    {
        result[0] = value;
        result[1] = 0;
    }
    return result;
}

// to prevent usage of the reserved bytes for packet headers, we split the bytes into 2 parts
byte *ByteArrayConverter::SplitBytes4(byte value[4])
{
    auto result = new byte[8];
    for (int i = 0; i < 4; i++)
    {
        if (value[i] > 200)
        {
            result[i * 2] = 200;
            result[i * 2 + 1] = (byte)(value[i] - 200);
        }
        else
        {
            result[i * 2] = value[i];
            result[i * 2 + 1] = 0;
        }
    }
    return result;
}

byte *ByteArrayConverter::UnSplitBytes8(byte value[8])
{
    auto result = new byte[4];
    for (int i = 0; i < 4; i++)
        result[i] = (byte)(value[i * 2] + value[i * 2 + 1]);
    return result;
}

byte ByteArrayConverter::UnSplitBytes2(byte value[2])
{
    return (byte)(value[0] + value[1]);
}
