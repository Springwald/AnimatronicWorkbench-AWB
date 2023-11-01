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
    _playPosInActualTimeline += diff;

    if (_playPosInActualTimeline > actualTimelineData.durationMs)
    {
        // the actual timeline is finished
        startNewTimelineForSelectedState();
        return;
    }

    // Play STS Servos
    for (int servoIndex = 0; servoIndex < _data->stsServoCount; servoIndex++)
    {
        u8 servoChannel = _data->stsServoChannels[servoIndex];
        int servoSpeed = _data->stsServoSpeed[servoIndex];
        int servoAccelleration = _data->stsServoAccelleration[servoIndex];

        StsServoPoint *point1 = nullptr;
        StsServoPoint *point2 = nullptr;

        for (int iPoint = 0; iPoint < actualTimelineData.stsServoPoints->size(); iPoint++)
        {
            StsServoPoint *point = &actualTimelineData.stsServoPoints->at(iPoint);
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

        if (_stSerialServoManager->servoAvailable(servoChannel))
        {
            _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
        }
        else
        {
            _errorOccured("Servo channel " + String(servoChannel) + " not attached!");
        }
    }
    _stSerialServoManager->updateActuators();

    // Play PWM Servos
    for (int servoIndex = 0; servoIndex < _data->pca9685PwmServoCount; servoIndex++)
    {
        int servoChannel = _data->pca9685PwmServoChannels[servoIndex];
        int servoSpeed = _data->pca9685PwmServoSpeed[servoIndex];
        int servoAccelleration = _data->pca9685PwmServoAccelleration[servoIndex];
        auto servoName = _data->pca9685PwmServoName[servoIndex];

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

/**
 * If a state selector is used, this is the selected state id
 */
int AutoPlayer::selectedStateId()
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
    auto stateId = selectedStateId();

    if (stateId == 0 || _data == nullptr || _data->timelines->size() == 0)
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

        if (_stateSelectorAvailable == false || _data->timelines->at(nextTimelineIndex).state->id == stateId)
        {
            // found the next timeline for the selected state
            startNewTimeline(nextTimelineIndex);
            return;
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