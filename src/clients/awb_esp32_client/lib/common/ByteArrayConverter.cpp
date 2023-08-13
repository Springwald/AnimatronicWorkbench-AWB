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

// bool ByteArrayConverter::EndsWith(String value[], byte ending[])
// {
//     int valueLength = sizeof(value);
//     int endingLength = sizeof(ending);

//     if (endingLength > valueLength)
//     {
//         return false;
//     }

//     for (int i = 0; i < endingLength; i++)
//     {
//         if (value[valueLength - endingLength + i] != ending[i])
//         {
//             return false;
//         }
//     }

//     return true;
// }
