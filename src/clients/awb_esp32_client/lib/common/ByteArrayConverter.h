#ifndef bytearray_converter_h
#define bytearray_converter_h

using byte = unsigned char;

#define ARRAY_SIZE(A) (sizeof(A) / sizeof((A)[0]))

/**
 * provides functions to convert byte arrays
 */
class ByteArrayConverter
{
public:
    /**
     * static function to convert unsigned int to an array of 4 byte
     */
    static byte *UintTo4Bytes(unsigned int value);

    /**
     * static function to convert an array of 4 byte to unsigned int
     */
    static unsigned int UintFrom4Bytes(byte value[4]);

    static byte *SplitBytes4(byte value[4]);
    static byte *SplitByte(byte value);
    static byte *UnSplitBytes8(byte value[8]);
    static byte UnSplitBytes2(byte value[2]);
};

#endif
