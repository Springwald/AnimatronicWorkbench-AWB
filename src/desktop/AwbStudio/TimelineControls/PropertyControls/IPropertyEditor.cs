// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.ActuatorsAndObjects;
using System;
using System.Threading.Tasks;

namespace AwbStudio.TimelineControls.PropertyControls
{
    internal interface IPropertyEditor
    {
        IAwbObject AwbObject { get; }

        event EventHandler OnValueChanged;

        Task UpdateValue();
    }
}
