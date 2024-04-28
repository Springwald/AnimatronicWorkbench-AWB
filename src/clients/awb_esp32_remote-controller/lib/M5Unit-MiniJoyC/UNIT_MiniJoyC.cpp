#include "UNIT_MiniJoyC.h"

// taken from https://github.com/m5stack/M5Unit-MiniJoyC under the MIT license

void UNIT_JOYC::writeBytes(uint8_t addr, uint8_t reg, uint8_t *buffer,
                           uint8_t length)
{
    _wire->beginTransmission(addr);
    _wire->write(reg);
    for (int i = 0; i < length; i++)
    {
        _wire->write(*(buffer + i));
    }
    _wire->endTransmission();
}

void UNIT_JOYC::readBytes(uint8_t addr, uint8_t reg, uint8_t *buffer,
                          uint8_t length)
{
    uint8_t index = 0;
    _wire->beginTransmission(addr);
    _wire->write(reg);
    _wire->endTransmission();
    _wire->requestFrom(addr, length);
    for (int i = 0; i < length; i++)
    {
        buffer[index++] = _wire->read();
    }
}

bool UNIT_JOYC::begin(TwoWire *wire, uint8_t addr, uint8_t sda, uint8_t scl,
                      uint32_t speed)
{
    _wire = wire;
    _addr = addr;
    _sda = sda;
    _scl = scl;
    _speed = speed;
    _wire->begin(_sda, _scl, _speed);
    delay(10);
    _wire->beginTransmission(_addr);
    uint8_t error = _wire->endTransmission();
    if (error == 0)
    {
        return true;
    }
    else
    {
        return false;
    }
}

uint16_t UNIT_JOYC::getADCValue(uint8_t index)
{
    uint8_t data[4];
    if (index > 2)
        return 0;
    uint8_t reg = index * 2 + ADC_VALUE_REG;
    readBytes(_addr, reg, data, 2);
    uint32_t value = data[0] | (data[1] << 8);
    return value;
}

uint16_t UNIT_JOYC::getPOSValue(uint8_t index, minijoyc_pos_read_mode_t bit)
{
    uint8_t data[4];
    uint32_t value;

    if (index > 2)
        return 0;
    if (bit == _10bit)
    {
        uint8_t reg = index * 2 + POS_VALUE_REG_10_BIT;
        readBytes(_addr, reg, data, 2);
        value = data[0] | (data[1] << 8);
    }
    else if (bit == _8bit)
    {
        uint8_t reg = index + POS_VALUE_REG_8_BIT;
        readBytes(_addr, reg, data, 1);
        value = data[0];
    }

    return value;
}

bool UNIT_JOYC::getButtonStatus()
{
    uint8_t data;
    readBytes(_addr, BUTTON_REG, &data, 1);
    return data;
}

void UNIT_JOYC::setLEDColor(uint8_t index, uint32_t color)
{
    uint8_t data[4];
    data[2] = color & 0xff;
    data[1] = (color >> 8) & 0xff;
    data[0] = (color >> 16) & 0xff;
    writeBytes(_addr, RGB_LED_REG, data, 3);
}

void UNIT_JOYC::setOneCalValue(uint8_t index, uint16_t data)
{
    uint8_t reg;
    uint8_t buf[4];

    reg = index * 2 + CAL_REG;
    buf[0] = data & 0xff;
    buf[1] = (data >> 8) & 0xff;
    writeBytes(_addr, reg, buf, 2);
    delay(1000);
}

void UNIT_JOYC::setAllCalValue(uint16_t *data)
{
    writeBytes(_addr, CAL_REG, (uint8_t *)data, 12);
    delay(1000);
}

uint16_t UNIT_JOYC::getCalValue(uint8_t index)
{
    if (index > 5)
        return 0;
    uint8_t data[4];
    uint8_t reg = index * 2 + CAL_REG;
    readBytes(_addr, reg, data, 2);
    uint16_t value = data[0] | (data[1] << 8);
    return value;
}

uint8_t UNIT_JOYC::setI2CAddress(uint8_t addr)
{
    _wire->beginTransmission(_addr);
    _wire->write(I2C_ADDRESS_REG);
    _wire->write(addr);
    _wire->endTransmission();
    _addr = addr;
    return _addr;
}

uint8_t UNIT_JOYC::getI2CAddress(void)
{
    _wire->beginTransmission(_addr);
    _wire->write(I2C_ADDRESS_REG);
    _wire->endTransmission();

    uint8_t RegValue;

    _wire->requestFrom(_addr, 1);
    RegValue = Wire.read();
    return RegValue;
}

uint8_t UNIT_JOYC::getFirmwareVersion(void)
{
    _wire->beginTransmission(_addr);
    _wire->write(FIRMWARE_VERSION_REG);
    _wire->endTransmission();

    uint8_t RegValue;

    _wire->requestFrom(_addr, 1);
    RegValue = Wire.read();
    return RegValue;
}
