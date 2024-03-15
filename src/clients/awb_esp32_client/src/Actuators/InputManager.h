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
    AutoPlayData *_data; // the data exported by Animatronic Workbench Studio
    void init();

public:
    // the constructor
    InputManager(AutoPlayData *data, TCallBackErrorOccured errorOccured) : _errorOccured(errorOccured), _data(data)
    {
        init();
    }

    bool isInputPressed(int inputIndex);
    String getDebugInfo();
};

#endif
