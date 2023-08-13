#ifndef bytearray_converter_h
#define bytearray_converter_h

using byte = unsigned char;

#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

class ByteArrayConverter
{
public:
    static byte *UintTo4Bytes(unsigned int value);
    static unsigned int UintFrom4Bytes(byte value[4]);
    // static bool EndsWith(byte value[], byte ending[]);
};

#endif
