#ifndef _RELAX_RANGE_H_
#define _RELAX_RANGE_H_

#include <Arduino.h>
#include <String.h>

class RelaxRange
{
protected:
public:
    int minValue; // lower bound of the relax range
    int maxValue; // upper bound of the relax range

    RelaxRange(int min, int max) : minValue(min), maxValue(max)
    {
    }

    ~RelaxRange()
    {
    }
};

#endif