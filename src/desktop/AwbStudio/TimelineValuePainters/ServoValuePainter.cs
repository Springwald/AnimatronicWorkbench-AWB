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
    class ServoValuePainter : ITimelineValuePainter
    {
        private IServo _servo;

        public ServoValuePainter(IServo servo, Grid paintControl)
        {
            this._servo = servo;
            this.PaintControl = paintControl;
        }

        public Grid PaintControl { get; }

        public void PaintValues()
        {
            throw new NotImplementedException();
        }

    }
}
