// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using System;
using System.Windows.Controls;

namespace AwbStudio.TimelineValuePainters
{
    class SoundValuePainter : ITimelineValuePainter
    {
        private readonly ISoundPlayer _soundPlayer;

        public SoundValuePainter(ISoundPlayer soundPlayer, Grid paintControl)
        {
            this._soundPlayer = soundPlayer;
            this.PaintControl = paintControl;
        }

        public Grid PaintControl { get; }

        public void PaintValues()
        {
            throw new NotImplementedException();
        }

    }
}
