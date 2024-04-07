// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Timelines;
using System.Windows.Controls;

namespace AwbStudio.TimelineValuePainters
{
    interface ITimelineValuePainter
    {
        Grid PaintControl { get; }

        void PaintValues();
        void TimelineDataLoaded(TimelineData timelineData);
    }
}
