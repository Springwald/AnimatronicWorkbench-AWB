#include <Arduino.h>
#include "AutoPlayer.h"
#include "AutoPlayData.h"
#include "Timeline.h"
#include "TimelineState.h"
#include "StsServoPoint.h"
#include "hardware.h"
#include <vector>

void AutoPlayer::setup()
{
    _lastMsUpdate = millis();
    _lastPacketReceivedMillis = -1;
    _actualTimelineIndex = -1;
    _playPosInActualTimeline = 0;
}

bool AutoPlayer::isPlaying()
{
    return _actualTimelineIndex != -1;
}

TimelineState *AutoPlayer::getCurrentState()
{
    return new TimelineState(1, String("Idle"));
}

String AutoPlayer::getCurrentTimelineName()
{
    if (_actualTimelineIndex == -1)
    {
        return String("Off [-1]");
    }
    return _data->timelines->at(_actualTimelineIndex).name + " [" + String(_actualTimelineIndex) + "]";
}

void AutoPlayer::update(bool servoHaveErrorsLikeTooHot)
{
    if (servoHaveErrorsLikeTooHot)
    {
        _actualTimelineIndex = -1;
        return;
    }

    int diff = millis() - _lastMsUpdate;
    if (diff < 50)
        return;

    _lastMsUpdate = millis();

    // return of no data is set
    if (_data == nullptr)
        return;

    if (!isPlaying())
    {
        if (_lastPacketReceivedMillis == -1 || millis() > _lastPacketReceivedMillis + 20000) // never got a awb studio packet or last packet is older than 20 seconds
        {
            // no data received, so we stop the auto mode
            _actualTimelineIndex = -1;
            return;
        }
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

    // Play Servos
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
            if (servoSpeed == -1 && servoAccelleration == -1)
            {
                // no speed and no accelleration defined
                _stSerialServoManager->writePosition(servoChannel, targetValue);
            }
            else if (servoSpeed == -1 && servoAccelleration != -1)
            {
                // no speed defined, but accelleration
                _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, STS_SERVO_SPEED, servoAccelleration);
            }
            else if (servoSpeed != -1 && servoAccelleration == -1)
            {
                // speed defined, but no accelleration
                _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, STS_SERVO_ACC);
            }
            else
            {
                // speed and accelleration defined
                _stSerialServoManager->writePositionDetailed(servoChannel, targetValue, servoSpeed, servoAccelleration);
            }
        }
        else
        {
            _errorOccured("Servo channel " + String(servoChannel) + " not attached!");
            _stSerialServoManager->scanIds();
        }
    }
}

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

void AutoPlayer::startNewTimeline(int timelineIndex)
{
    _actualTimelineIndex = timelineIndex;
    _playPosInActualTimeline = 0;
}

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

void AutoPlayer::stopBecauseOfIncommingPackage()
{
    _actualTimelineIndex = -1;
    _lastPacketReceivedMillis = millis();
}