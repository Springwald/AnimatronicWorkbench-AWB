// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Services;
using Awb.Core.Timelines;

namespace AwbStudio.TimelineControls
{
    internal interface ITimelineControl
    {
        /// <summary>
        /// The timeline data to be displayed
        /// </summary>
        TimelineData? TimelineData { set; }

        TimelineCaptions TimelineCaptions { set; }

        /// <summary>
        /// The actual view and scroll position of the timeline
        /// </summary>
        TimelineViewPos ViewPos { set;  }

        IActuatorsService? ActuatorsService { set;  }
    }
}
