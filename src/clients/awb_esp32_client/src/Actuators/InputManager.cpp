

#include <Arduino.h>
#include "InputManager.h"

bool InputManager::isInputPressed(int inputIndex)
{
    if (inputIndex < 1 || inputIndex >= _data->inputCount)
    {
        _errorOccured("Input index " + String(inputIndex) + " out of range");
        return false;
    }

    return digitalRead(_data->inputIoPins[inputIndex]) == HIGH;
}
