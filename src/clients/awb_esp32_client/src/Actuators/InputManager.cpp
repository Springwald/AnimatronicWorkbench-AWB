

#include <Arduino.h>
#include "InputManager.h"

void InputManager::init()
{
    for (int i = 0; i < _data->inputCount; i++)
        pinMode(_data->inputIoPins[i], INPUT);
}

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
        return false;
    }

    return digitalRead(_data->inputIoPins[index]) == HIGH;
}

String InputManager::getDebugInfo()
{
    String result = "";
    for (int i = 0; i < _data->inputCount; i++)
    {
        if (i > 0)
            result += ", ";
        result += String(_data->inputNames[i]) + "[" + String(_data->inputIds[i]) + "]=" + String(digitalRead(_data->inputIoPins[i]));
    }
    return result;
}