﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;

namespace AwbStudio.TimelineEditing
{
    internal class NestedTimelinesFakeObject : IActuator
    {
        public static NestedTimelinesFakeObject Singleton { get; } = new NestedTimelinesFakeObject();

        public string Title => "Nested timelines";

        public string Id => "Nested timelines";

        public uint ClientId => 1;

        public bool IsDirty { get; set; }

        public bool IsControllerTuneable => false;

        public void Dispose()
        {
        }
    }
}
