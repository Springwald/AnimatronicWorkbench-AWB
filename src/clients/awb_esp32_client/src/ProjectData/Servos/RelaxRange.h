#ifndef _RELAX_RANGE_H_
#define _RELAX_RANGE_H_

#include <Arduino.h>
#include <String.h>

class RelaxRange
{
protected:
public:
    int minValue;  // lower bound of the relax range
    int maxValue;  // upper bound of the relax range
    int tolerance; // tolerance when the relaxed servo is moved inside the relax range

    RelaxRange(int minValue, int maxValue, int tolerance) : minValue(minValue), maxValue(maxValue), tolerance(tolerance)
    {
    }

    ~RelaxRange()
    {
    }
};

#endif