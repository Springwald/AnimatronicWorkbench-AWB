#include "Debugging.h"

bool Debugging::isDebugging()
{
#ifdef DEBUGGING_IO_PIN
    // debugging on/off is controlled by a pin
    if (_lastInputUpdateMillis == -1 || millis() > _lastInputUpdateMillis + 1000)
    {
        _lastInputUpdateMillis = millis();
        _debugging = (digitalRead(DEBUGGING_IO_PIN) == DEBUGGING_IO_PIN_ACTIVE);
    }
#endif
    return _debugging;
}

void Debugging::setState(int major, int minor)
{
    _display->set_debuggingState(this->isDebugging(), major, minor);
}
