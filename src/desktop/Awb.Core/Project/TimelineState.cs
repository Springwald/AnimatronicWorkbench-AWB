// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Configs
{
    public class TimelineState
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }

        public TimelineState(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => $"[{Id}] {Name}";
    }
}