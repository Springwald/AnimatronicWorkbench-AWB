#ifndef Input_manager_h
#define Input_manager_h

#include <Arduino.h>
#include <vector>
#include <AwbDataImport/ProjectData.h>

class InputManager
{
    using TCallBackErrorOccured = std::function<void(String)>;
    using TCallBackMessageToShow = std::function<void(String)>;

private:
    TCallBackErrorOccured _errorOccured;
    ProjectData *_data; // the data exported by Animatronic Workbench Studio
    void init();

public:
    // the constructor
    InputManager(ProjectData *data, TCallBackErrorOccured errorOccured) : _errorOccured(errorOccured), _data(data)
    {
        init();
    }

    bool isInputPressed(int inputIndex);
    String getDebugInfo();
};

#endif
