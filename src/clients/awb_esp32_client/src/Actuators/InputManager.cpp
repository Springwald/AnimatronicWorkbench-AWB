

#include <Arduino.h>
#include "InputManager.h"

bool InputManager::isInputPressed(int inputId)
{
    int index = -1;
    for (int i = 0; i < _data->inputCount; i++)
    {
        if (_data->inputIds[i] == inputId)
        {
            index = i;
            break;
        }
    }

    if (index == -1)
    {
        _errorOccured("Input id " + String(inputId) + " not defined!");
    }

    pinMode(_data->inputIoPins[index], INPUT);
    return digitalRead(_data->inputIoPins[index]) == HIGH;
}
