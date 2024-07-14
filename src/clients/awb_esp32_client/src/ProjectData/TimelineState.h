#ifndef _TIMELINE_STATE_H_
#define _TIMELINE_STATE_H_

#include <Arduino.h>
#include <String.h>
#include <vector>

class TimelineState
{
public:
    int id;
    String name;
    bool autoplay;
    std::vector<int> *positiveInputIds;
    std::vector<int> *negativeInputIds;

    TimelineState(int _id, String const _name, bool _autoplay, std::vector<int> *_positiveInputIds, std::vector<int> *_negativeInputIds) : id(_id), name(_name), autoplay(_autoplay), positiveInputIds(_positiveInputIds), negativeInputIds(_negativeInputIds)
    {
        // find out the negative input ids count preventing null devision
    }

    ~TimelineState()
    {
    }
};

#endif