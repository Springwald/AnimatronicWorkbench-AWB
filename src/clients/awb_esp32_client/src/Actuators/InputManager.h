#ifndef Input_manager_h
#define Input_manager_h

#include <Arduino.h>
#include <vector>
#include <AutoPlay/AutoPlayData.h>

class InputManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    TCallBackMessageToShow _messageToShow;
    AutoPlayData *_data; // the data exported by Animatronic Workbench Studio

public:
    // the constructor
    InputManager(AutoPlayData *data, TCallBackErrorOccured errorOccured, TCallBackMessageToShow messageToShow) : _errorOccured(errorOccured), _messageToShow(messageToShow), _data(data)
    {
    }

    bool isInputPressed(int inputIndex);
};

#endif
