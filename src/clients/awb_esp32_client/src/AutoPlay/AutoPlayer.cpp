#include <Arduino.h>
#include <vector>
#include "AwbDataImport/HardwareConfig.h"
#include "AutoPlayer.h"
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineStateReference.h"
#include "../ProjectData/StsServoPoint.h"
#include "../ProjectData/Pca9685PwmServoPoint.h"

/**
 * Initializes a new instance of the <see cref="AutoPlayer"/> class.
 */
void AutoPlayer::setup()
{
    _lastMsUpdate = millis();
    _lastPacketReceivedMillis = -1;
    _actualTimelineIndex = -1;
    _playPosInActualTimeline = 0;
}

/**
 * Is actually playing a timeline?
 */
bool AutoPlayer::isPlaying()
{
    return _actualTimelineIndex != -1;
}

/**
 * Is a hardware state selector available?
 */
bool AutoPlayer::getStateSelectorAvailable()
{
    return _stateSelectorAvailable;
}

/**
 * If a sts servo is used to select the state, this is the channel
 */
int AutoPlayer::getStateSelectorStsServoChannel()
{
    return _stateSelectorStsServoChannel;
}

/**
 * If a timeline is playing, this is the name of the timeline
 */
String AutoPlayer::getCurrentTimelineName()
{
    if (_actualTimelineIndex == -1)
    {
        return String("Off [-1]");
    }
    return _data->timelines->at(_actualTimelineIndex).name + " [" + String(_actualTimelineIndex) + "]";
}

String AutoPlayer::getLastSoundPlayed()
{
    if (_lastSoundPlayed == -1)
    {
        return String("None");
    }
    return String(_lastSoundPlayed);
}

/**
 * Updates the autoplayer and plays the timelines
 */
void AutoPlayer::update(bool anyServoWithGlobalFaultHasCiriticalState)
{

    _debugging->setState(Debugging::MJ_AUTOPLAY, 1);

    // return of no data is set
    if (_data == nullptr)
        return;

    int updateInterval = 30; // ms

    if (anyServoWithGlobalFaultHasCiriticalState)
    {
        _actualTimelineIndex = -1;
        return;
    }

    long diff = millis() - _lastMsUpdate;
    if (diff < updateInterval) // update interval in milliseconds
        return;

    _lastMsUpdate = millis();

    bool forgetLastPacketReceived = false;

    _debugging->setState(Debugging::MJ_AUTOPLAY, 5);

    if (forgetLastPacketReceived == true && _lastPacketReceivedMillis != -1 && millis() > _lastPacketReceivedMillis + 60 * 1000) // 60 seconds
        _lastPacketReceivedMillis = -1;                                                                                          // forget the last packet received

    bool packedReceivedLately = _lastPacketReceivedMillis != -1;
    if (packedReceivedLately == true)
    {
        // data received, so we stop the auto mode
        _actualTimelineIndex = -1;
        return;
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 10);

    if (!isPlaying())
    {
        // start automode
        startNewTimelineForSelectedState();
        return;
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 20);

    auto actualTimelineData = _data->timelines->at(_actualTimelineIndex);
    long rememberLastPlayPos = _playPosInActualTimeline;
    _playPosInActualTimeline += diff;

    if (_playPosInActualTimeline > actualTimelineData.durationMs)
    {
        // the actual timeline is finished
        startNewTimelineForSelectedState();
        return;
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 25);

    // Play STS Servos
    if (_stSerialServoManager != nullptr)
    {
        for (int servoIndex = 0; servoIndex < _data->stsServos->size(); servoIndex++)
        {
            u8 servoChannel = _data->stsServos->at(servoIndex).channel;
            int servoSpeed = _data->stsServos->at(servoIndex).defaultSpeed;
            int servoAccelleration = _data->stsServos->at(servoIndex).defaultAcceleration;

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, servoSpeed, servoAccelleration, actualTimelineData.stsServoPoints);
            if (targetValue == -1)
                continue;

            _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
        }
        _stSerialServoManager->updateActuators(anyServoWithGlobalFaultHasCiriticalState);
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 30);

    // Play SCS Servos
    if (_scSerialServoManager != nullptr)
    {
        for (int servoIndex = 0; servoIndex < _data->scsServos->size(); servoIndex++)
        {
            u8 servoChannel = _data->scsServos->at(servoIndex).channel;
            int servoSpeed = _data->scsServos->at(servoIndex).defaultSpeed;
            int servoAccelleration = _data->scsServos->at(servoIndex).defaultAcceleration;

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, servoSpeed, servoAccelleration, actualTimelineData.scsServoPoints);
            if (targetValue == -1)
                continue;

            _scSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
        }
        _scSerialServoManager->updateActuators(anyServoWithGlobalFaultHasCiriticalState);
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 35);

    // Play PWM Servos
    if (_pca9685PwmManager != nullptr)
    {
        for (int servoIndex = 0; servoIndex < _data->pca9685PwmServos->size(); servoIndex++)
        {
            int servoChannel = _data->pca9685PwmServos->at(servoIndex).channel;
            auto servoName = _data->pca9685PwmServos->at(servoIndex).title;

            Pca9685PwmServoPoint *point1 = nullptr;
            Pca9685PwmServoPoint *point2 = nullptr;

            for (int iPoint = 0; iPoint < actualTimelineData.scsServoPoints->size(); iPoint++)
            {
                Pca9685PwmServoPoint *point = &actualTimelineData.pca9685PwmPoints->at(iPoint);
                if (point->channel == servoChannel)
                {
                    if (point->ms <= _playPosInActualTimeline)
                        point1 = point;

                    if (point->ms >= _playPosInActualTimeline)
                    {
                        point2 = point;
                        break;
                    }
                }
            }

            if (point1 == nullptr && point2 == nullptr)
                continue; // no points found for this object before or after the actual position

            if (point1 == nullptr)
            {
                // no point before the actual position found, so we take the first point after the actual position
                point1 = point2;
            }
            else if (point2 == nullptr)
            {
                // no point after the actual position found, so we take the last point before the actual position
                point2 = point1;
            }

            int pointDistanceMs = point2->ms - point1->ms;
            int targetValue = 0;
            if (pointDistanceMs == 0)
            {
                targetValue = point1->value;
            }
            else
            {
                double posBetweenPoints = (_playPosInActualTimeline - point1->ms * 1.0) / pointDistanceMs;
                targetValue = point1->value + (point2->value - point1->value) * posBetweenPoints;
            }
            _pca9685PwmManager->setTargetValue(servoChannel, targetValue, servoName);
        }
        _pca9685PwmManager->updateActuators(anyServoWithGlobalFaultHasCiriticalState);
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 40);

    // Play MP3
    if (_mp3PlayerYX5300Manager != nullptr)
    {
        for (int iPoint = 0; iPoint < actualTimelineData.mp3PlayerYX5300Points->size(); iPoint++)
        {
            Mp3PlayerYX5300Point *point = &actualTimelineData.mp3PlayerYX5300Points->at(iPoint);
            if (point->ms > rememberLastPlayPos && point->ms <= _playPosInActualTimeline)
            {
                for (int trys = 0; trys < 3; trys++)
                {
                    if (_mp3PlayerYX5300Manager->playSound(point->soundPlayerIndex, point->soundId) == true)
                    {
                        _lastSoundPlayed = point->soundId;
                        break;
                    }
                    else
                    {
                        _mp3PlayerYX5300Manager->stopSound(point->soundPlayerIndex);
                        _lastSoundPlayed = -100;
                        delay(50);
                    }
                }
            }
        }
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 99);
}

int AutoPlayer::calculateServoValueFromTimeline(u8 servoChannel, int servoSpeed, int servoAccelleration, std::vector<StsServoPoint> *servoPoints)
{
    StsServoPoint *point1 = nullptr;
    StsServoPoint *point2 = nullptr;

    for (int iPoint = 0; iPoint < servoPoints->size(); iPoint++)
    {
        StsServoPoint *point = &servoPoints->at(iPoint);
        if (point->channel == servoChannel)
        {
            if (point->ms <= _playPosInActualTimeline)
                point1 = point;

            if (point->ms >= _playPosInActualTimeline)
            {
                point2 = point;
                break;
            }
        }
    }

    if (point1 == nullptr && point2 == nullptr)
        return -1; // no points found for this object before or after the actual position

    if (point1 == nullptr)
    {
        // no point before the actual position found, so we take the first point after the actual position
        point1 = point2;
    }
    else if (point2 == nullptr)
    {
        // no point after the actual position found, so we take the last point before the actual position
        point2 = point1;
    }

    int pointDistanceMs = point2->ms - point1->ms;
    int targetValue = 0;
    if (pointDistanceMs == 0)
    {
        targetValue = point1->value;
    }
    else
    {
        double posBetweenPoints = (_playPosInActualTimeline - point1->ms * 1.0) / pointDistanceMs;
        targetValue = point1->value + (point2->value - point1->value) * posBetweenPoints;
    }

    return targetValue;
}

String AutoPlayer::getStatesDebugInfo()
{
    String result = "Active: ";
    auto activeStates = getActiveStatesByInputs();

    // iterate over the active states and check if the next timeline is for one of them
    for (int i = 0; i < activeStates.size(); i++)
    {
        auto state = activeStates.at(i);
        result += String(state.name) + "[" + String(state.id) + "]";
    }

    return result;
}

// function to return the active states as array
std::vector<TimelineState> AutoPlayer::getActiveStatesByInputs()
{
    std::vector<TimelineState> activeStates;
    bool foundAnyActivePositive = false;

    // iterate over _data->timelineStates and check if they are active
    for (int iState = 0; iState < _data->timelineStates->size(); iState++)
    {
        auto state = _data->timelineStates->at(iState);
        if (state.autoplay == false) // is this state only available for remote activation?
            continue;

        // check the positive inputs in the timeline state
        for (int iInput = 0; iInput < state.positiveInputIds->size(); iInput++)
        {
            int inputId = state.positiveInputIds->at(iInput);
            if (inputId > 0)
            {
                if (_inputManager->isInputPressed(inputId))
                {
                    foundAnyActivePositive = true;
                    activeStates.push_back(state);
                }
            }
        }
    }

    if (foundAnyActivePositive == true) // is any state by positive inputs found, return only them
        return activeStates;

    // return all, which are not disabled by negative inputs
    for (int iState = 0; iState < _data->timelineStates->size(); iState++)
    {
        auto state = _data->timelineStates->at(iState);
        if (state.autoplay == false) // is this state only available for remote activation?
            continue;

        bool hasPositiveInput = false;
        for (int iInput = 0; iInput < state.positiveInputIds->size(); iInput++)
        {
            int inputId = state.positiveInputIds->at(iInput);
            if (inputId > 0)
            {
                hasPositiveInput = true;
            }
        }

        if (hasPositiveInput) // has positive input, but this was not presses
            continue;

        bool negativeInputActive = false;
        for (int iInput = 0; iInput < state.negativeInputIds->size(); iInput++)
        {
            int inputId = state.negativeInputIds->at(iInput);
            if (inputId > 0 && _inputManager->isInputPressed(inputId))
            {
                negativeInputActive = true;
            }
        }

        if (negativeInputActive)
            continue;

        activeStates.push_back(state);
    }

    return activeStates;
}

/**
 * If a state selector is used, this is the selected state id
 */
int AutoPlayer::selectedStateIdFromStsServoSelector()
{
    if (_stateSelectorStsServoChannel == -1)
        return -1;

    if (millis() < _lastStateCheckMillis + 500) // update interval
        return _currentStateId;

    _lastStateCheckMillis = millis();

    if (_stateSelectorAvailable == false)
    {
        _stateSelectorAvailable = _stSerialServoManager->servoAvailable(_stateSelectorStsServoChannel);
        if (_stateSelectorAvailable == false)
            return -1;
    }

    int pos = _stSerialServoManager->readPosition(_stateSelectorStsServoChannel);

    if (pos == -1)
    {
        return -1;
    }

    int statesPerRound = 6;
    int segment = 4096 / statesPerRound;
    int stateId = (pos + AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET) / segment;
    if (stateId >= statesPerRound)
        stateId = 0;
    _currentStateId = stateId;
    return _currentStateId;
}

/**
 * Starts the auto player with the given timeline
 */
void AutoPlayer::startNewTimelineByIndex(int timelineIndex)
{
    _actualTimelineIndex = timelineIndex;
    _playPosInActualTimeline = 0;
}

/**
 * starts the timeline with the given name
 */
void AutoPlayer::startNewTimelineByName(String name)
{
    for (int i = 0; i < _data->timelines->size(); i++)
    {
        if (_data->timelines->at(i).name == name)
        {
            startNewTimelineByIndex(i);
            return;
        }
    }
}

/**
 * Starts a new timeline for the selected state
 */
void AutoPlayer::startNewTimelineForSelectedState()
{
    auto stateIdFromStsServoSelector = selectedStateIdFromStsServoSelector();

    if (stateIdFromStsServoSelector == 0 || _data == nullptr || _data->timelines->size() == 0)
    {
        _actualTimelineIndex = -1;
        return;
    }

    // print timeline size
    int size = _data->timelines->size();

    // find the next timeline for the selected state
    int nextTimelineIndex = _actualTimelineIndex;
    int tries = 0;
    while (tries++ <= _data->timelines->size() + 1)
    {
        nextTimelineIndex++;
        if (nextTimelineIndex >= size)
        {
            nextTimelineIndex = 0; // last timeline reached, start from the beginning
        }

        if (_stateSelectorAvailable == true) // state selector is the most important selector
        {
            // check if the next timeline is for the selected state
            if (_data->timelines->at(nextTimelineIndex).state->id == stateIdFromStsServoSelector)
            {
                // found the next timeline for the selected state
                startNewTimelineByIndex(nextTimelineIndex);
                return;
            }
        }
        else
        {
            // if no state selector is available, we check the inputs
            auto activeStates = getActiveStatesByInputs();

            // iterate over the active states and check if the next timeline is for one of them
            for (int i = 0; i < activeStates.size(); i++)
            {
                if (_data->timelines->at(nextTimelineIndex).state->id == activeStates.at(i).id)
                {
                    startNewTimelineByIndex(nextTimelineIndex);
                    return;
                }
            }
        }
    }

    _actualTimelineIndex = -1;
}

/**
 * Stops the auto player because of incomming data package of Animatronic Workbench Studio
 */
void AutoPlayer::stopBecauseOfIncommingPackage()
{
    _actualTimelineIndex = -1;
    _lastPacketReceivedMillis = millis();
}