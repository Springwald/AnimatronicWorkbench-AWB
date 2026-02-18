// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;
using System.Windows.Controls;

namespace AwbStudio.TimelineControls
{
    internal interface ITimelineEditorControl 
    {
        void TimelineDataLoaded(TimelineData timelineData);
    }
}
