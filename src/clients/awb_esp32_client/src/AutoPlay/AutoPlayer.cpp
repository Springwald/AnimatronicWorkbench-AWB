#include <Arduino.h>
#include "AutoPlayer.h"
#include "AutoPlayData.h"
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"
#include "Pca9685PwmServoPoint.h"
#include "hardware.h"
#include <vector>

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

/**
 * Updates the autoplayer and plays the timelines
 */
void AutoPlayer::update(bool servoHaveErrorsLikeTooHot)
{
    if (servoHaveErrorsLikeTooHot)
    {
        _actualTimelineIndex = -1;
        return;
    }

    int diff = millis() - _lastMsUpdate;
    if (diff < 5) // update interval in milliseconds
        return;

    _lastMsUpdate = millis();

    // return of no data is set
    if (_data == nullptr)
        return;

    bool packedReceivedLately = _lastPacketReceivedMillis != -1 && millis() < _lastPacketReceivedMillis + 60 * 1000; //  got a awb studio packet or last packet is newer than given seconds * 1000ms
    if (packedReceivedLately == true)
    {
        // no data received, so we stop the auto mode
        _actualTimelineIndex = -1;
        return;
    }

    if (!isPlaying())
    {
        // start automode
        startNewTimelineForSelectedState();
        return;
    }

    auto actualTimelineData = _data->timelines->at(_actualTimelineIndex);
    int rememberLastPlayPos = _playPosInActualTimeline;
    _playPosInActualTimeline += diff;

    if (_playPosInActualTimeline > actualTimelineData.durationMs)
    {
        // the actual timeline is finished
        startNewTimelineForSelectedState();
        return;
    }

    // Play STS Servos
    if (_stSerialServoManager != NULL)
    {
        for (int servoIndex = 0; servoIndex < _data->stsServoCount; servoIndex++)
        {
            u8 servoChannel = _data->stsServoChannels[servoIndex];
            int servoSpeed = _data->stsServoSpeed[servoIndex];
            int servoAccelleration = _data->stsServoAcceleration[servoIndex];

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, servoSpeed, servoAccelleration, actualTimelineData.stsServoPoints);
            if (targetValue == -1)
                continue;

            _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
        }
        _stSerialServoManager->updateActuators();
    }

    // Play SCS Servos
    if (_scSerialServoManager != NULL)
    {
        for (int servoIndex = 0; servoIndex < _data->scsServoCount; servoIndex++)
        {
            u8 servoChannel = _data->scsServoChannels[servoIndex];
            int servoSpeed = _data->scsServoSpeed[servoIndex];
            int servoAccelleration = _data->scsServoAcceleration[servoIndex];

            int targetValue = this->calculateServoValueFromTimeline(servoChannel, servoSpeed, servoAccelleration, actualTimelineData.scsServoPoints);
            if (targetValue == -1)
                continue;

            _scSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
        }
        _scSerialServoManager->updateActuators();
    }

    // Play PWM Servos
    if (_pca9685PwmManager != NULL)
    {
        for (int servoIndex = 0; servoIndex < _data->pca9685PwmServoCount; servoIndex++)
        {
            int servoChannel = _data->pca9685PwmServoChannels[servoIndex];
            int servoSpeed = _data->pca9685PwmServoSpeed[servoIndex];
            int servoAccelleration = _data->pca9685PwmServoAccelleration[servoIndex];
            auto servoName = _data->pca9685PwmServoName[servoIndex];

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
        _pca9685PwmManager->updateActuators();
    }

    // Play MP3
    if (_mp3PlayerYX5300Manager != NULL)
    {
        for (int soundPlayerIndex = 0; soundPlayerIndex < _data->mp3PlayerYX5300Count; soundPlayerIndex++)
        {
            for (int iPoint = 0; iPoint < actualTimelineData.mp3PlayerYX5300Points->size(); iPoint++)
            {
                Mp3PlayerYX5300Point *point = &actualTimelineData.mp3PlayerYX5300Points->at(iPoint);
                if (point->soundPlayerIndex == soundPlayerIndex && point->ms > rememberLastPlayPos && point->ms <= _playPosInActualTimeline)
                {
                    _mp3PlayerYX5300Manager->playSound(point->soundId);
                }
            }
        }
    }
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

// function to return the active states as array
std::vector<int> AutoPlayer::getActiveStateIdsByInputs()
{
    std::vector<int> activeStateIds;
    bool foundAnyPositive = false;

    for (int iState = 0; iState < _data->timelineStateCount; iState++)
    {
        // check the positive inputs
        int inputId = _data->timelineStatePositiveInput[iState];
        if (inputId > 0)
        {
            if (_inputManager->isInputPressed(inputId))
            {
                foundAnyPositive = true;
                activeStateIds.push_back(_data->timelineStateIds[iState]);
            }
        }
    }

    if (activeStateIds.size() > 0) // is any state by positive inputs found, return only them
        return activeStateIds;

    // return all, which are not disabled by negative inputs
    for (int iState = 0; iState < _data->timelineStateCount; iState++)
    {
        bool hasPositiveInput = false;
        int inputId = _data->timelineStatePositiveInput[iState];
        if (inputId > 0)
        {
            hasPositiveInput = true;
        }

        if (hasPositiveInput) // has positive input, but this was not presses
            continue;

        bool negativeInputActive = false;
        inputId = _data->timelineStateNegativeInput[iState];
        if (inputId > 0)
        {
            if (_inputManager->isInputPressed(inputId))
                negativeInputActive = true;
        }

        if (negativeInputActive)
            continue;

        activeStateIds.push_back(_data->timelineStateIds[iState]);
    }

    return activeStateIds;
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
void AutoPlayer::startNewTimeline(int timelineIndex)
{
    _actualTimelineIndex = timelineIndex;
    _playPosInActualTimeline = 0;
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
                startNewTimeline(nextTimelineIndex);
                return;
            }
        }
        else
        {
            // if no state selector is available, we check the inputs
            auto activeStateIds = getActiveStateIdsByInputs();

            // iterate over the active states and check if the next timeline is for one of them
            for (int i = 0; i < activeStateIds.size(); i++)
            {
                if (_data->timelines->at(nextTimelineIndex).state->id == activeStateIds[i])
                {
                    startNewTimeline(nextTimelineIndex);
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