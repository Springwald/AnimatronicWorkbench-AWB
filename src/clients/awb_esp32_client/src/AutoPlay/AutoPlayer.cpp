#include <Arduino.h>
#include <vector>
#include "AwbDataImport/HardwareConfig.h"
#include "AutoPlayer.h"
#include "../ProjectData/Timeline.h"
#include "../ProjectData/TimelineStateReference.h"
#include "../ProjectData/Servos/ServoPoint.h"

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
 * Force the globale timeline state by custom code a single time. Buttons and other inputs defined in AWB Studio will be ignored for state selection.
 */
void AutoPlayer::forceTimelineState(bool permanent, int *stateId)
{
    // check if this is a set back to "not set" if the stateId value is -1
    if (*stateId == -1)
    {
        _timeLineStateForcedPermanentByRemoteOrCustomCodeId = nullptr;
        _timeLineStateForcedOnceByRemoteOrCustomCodeId = nullptr;
        return;
    }

    if (permanent)
    {
        _timeLineStateForcedPermanentByRemoteOrCustomCodeId = stateId;
        if (_timeLineStateForcedPermanentByRemoteOrCustomCodeId != nullptr)
            delete _timeLineStateForcedOnceByRemoteOrCustomCodeId;
    }
    else
    {
        _timeLineStateForcedOnceByRemoteOrCustomCodeId = stateId;
    }
}

/**
 * If a timeline is playing, this is the name of the timeline
 */
String AutoPlayer::getCurrentTimelineName(bool extendWithTimelineIndex)
{
    if (extendWithTimelineIndex == false)
    {
        if (_actualTimelineIndex == -1)
            return String("Off");
        return _data->timelines->at(_actualTimelineIndex).name;
    }
    else
    {
        if (_actualTimelineIndex == -1)
            return String("Off [-1]");
        return _data->timelines->at(_actualTimelineIndex).name + " [" + String(_actualTimelineIndex) + "]";
    }
}
/**
 * The name of the actual timeline state
 */
int AutoPlayer::getCurrentTimelineStateId()
{
    if (_actualTimelineIndex == -1)
        return -1;
    return _data->timelines->at(_actualTimelineIndex).state->id;
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

    _debugging->setState(Debugging::MJ_AUTOPLAY, 5);

    if (_data->returnToAutoModeAfterMinutes != -1)
    {
        if (_lastPacketReceivedMillis != -1 && millis() > _lastPacketReceivedMillis + _data->returnToAutoModeAfterMinutes * 60 * 1000) // x minutes (=60*1000ms) after the last packet received, we return to auto mode
            _lastPacketReceivedMillis = -1;                                                                                            // forget the last packet received
    }

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
        for (int servoIndex = 0; servoIndex < _data->servos->size(); servoIndex++)
        {
            if (_data->servos->at(servoIndex).config->type != ServoConfig::ServoTypes::STS_SERVO)
                continue;
            u8 servoChannel = _data->servos->at(servoIndex).config->channel;
            int servoSpeed = _data->servos->at(servoIndex).config->defaultSpeed;
            int servoAccelleration = _data->servos->at(servoIndex).config->defaultAcceleration;

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, actualTimelineData.stsServoPoints);
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
        for (int servoIndex = 0; servoIndex < _data->servos->size(); servoIndex++)
        {
            if (_data->servos->at(servoIndex).config->type != ServoConfig::ServoTypes::SCS_SERVO)
                continue;
            u8 servoChannel = _data->servos->at(servoIndex).config->channel;
            int servoSpeed = _data->servos->at(servoIndex).config->defaultSpeed;
            int servoAccelleration = _data->servos->at(servoIndex).config->defaultAcceleration;

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, actualTimelineData.scsServoPoints);
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
        for (int servoIndex = 0; servoIndex < _data->servos->size(); servoIndex++)
        {
            if (_data->servos->at(servoIndex).config->type != ServoConfig::ServoTypes::PWM_SERVO)
                continue;
            int servoChannel = _data->servos->at(servoIndex).config->channel;
            auto servoName = _data->servos->at(servoIndex).title;

            Pca9685PwmServoPoint *point1 = nullptr;
            Pca9685PwmServoPoint *point2 = nullptr;

            for (int iPoint = 0; iPoint < actualTimelineData.pca9685PwmPoints->size(); iPoint++)
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
                if (fillUpStart)
                    point1 = point2; // no point before the actual position found, so we take the first point after the actual position
                else
                    continue; // no start point found
            }
            else if (point2 == nullptr)
            {
                point2 = point1; // no point after the actual position found, so we take the last point before the actual position
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

    // Play MP3 on yx5300
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

    _debugging->setState(Debugging::MJ_AUTOPLAY, 41);

    // Play MP3 on DFPlayer Mini
    if (_mp3PlayerDfPlayerMiniManager != nullptr)
    {
        for (int iPoint = 0; iPoint < actualTimelineData.mp3PlayerDfPlayerMiniPoints->size(); iPoint++)
        {
            Mp3PlayerDfPlayerMiniPoint *point = &actualTimelineData.mp3PlayerDfPlayerMiniPoints->at(iPoint);
            if (point->ms > rememberLastPlayPos && point->ms <= _playPosInActualTimeline)
            {
                for (int trys = 0; trys < 3; trys++)
                {
                    if (_mp3PlayerDfPlayerMiniManager->playSound(point->soundPlayerIndex, point->soundId) == true)
                    {
                        _lastSoundPlayed = point->soundId;
                        break;
                    }
                    else
                    {
                        _mp3PlayerDfPlayerMiniManager->stopSound(point->soundPlayerIndex);
                        _lastSoundPlayed = -100;
                        delay(50);
                    }
                }
            }
        }
    }

    _debugging->setState(Debugging::MJ_AUTOPLAY, 99);
}

int AutoPlayer::calculateServoValueFromTimeline(u8 servoChannel, std::vector<StsServoPoint> *servoPoints)
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
        if (fillUpStart)
            point1 = point2; // no point before the actual position found, so we take the first point after the actual position
        else
            return -1; // no start point found
    }
    else if (point2 == nullptr)
    {
        point2 = point1; // no point after the actual position found, so we take the last point before the actual position
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
    if (_data == nullptr || _data->timelines->size() == 0)
    {
        _actualTimelineIndex = -1;
        return;
    }

    // print timeline size
    int size = _data->timelines->size();

    // check if the actual timeline had a next state once set
    int nextStateForcedId = -1;
    if (_timeLineStateForcedOnceByRemoteOrCustomCodeId != nullptr)
    {
        // There is a special next-state-once set by remote or custom code.
        // This is the highest priority
        nextStateForcedId = *_timeLineStateForcedOnceByRemoteOrCustomCodeId;
        _timeLineStateForcedOnceByRemoteOrCustomCodeId = nullptr;
    }
    else if (_timeLineStateForcedPermanentByRemoteOrCustomCodeId != nullptr)
    {
        // There is a special next-state-permanent set by remote or custom code.
        // This is the second highest priority
        nextStateForcedId = *_timeLineStateForcedPermanentByRemoteOrCustomCodeId;
    }
    else if (_actualTimelineIndex != -1)
    {
        // There is no special next-state set by remote or custom code,
        // so we check if the actual timeline has a next-state-once set
        auto actualTimelineData = _data->timelines->at(_actualTimelineIndex);
        if (actualTimelineData.nextStateOnceId != -1)
            nextStateForcedId = actualTimelineData.nextStateOnceId;
    }

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

        if (nextStateForcedId != -1) // there is a next state forced by remote, custom code or the actual ending timeline
        {
            if (_data->timelines->at(nextTimelineIndex).state->id == nextStateForcedId)
            {
                startNewTimelineByIndex(nextTimelineIndex);
                return;
            }
        }
        else
        {
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