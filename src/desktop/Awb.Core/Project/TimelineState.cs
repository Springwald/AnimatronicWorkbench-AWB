// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class TimelineState
    {
        public int Id { get; internal set; }
        public string Name { get; internal set; }

        public bool Export { get; internal set; } = true;

        /// <summary>
        /// The state is only available when one of this inputs are on
        /// </summary>
        public int[] PositiveInputs { get; internal set; }

        /// <summary>
        /// The state not available when one of this inputs are on
        /// </summary>
        public int[] NegativeInputs { get; internal set; }

        public TimelineState(int id, string name, bool export, int[]? positiveInputs = null, int[]? negativeInputs= null)
        {
            Id = id;
            Name = name;
            PositiveInputs = positiveInputs ?? Array.Empty<int>();
            NegativeInputs = negativeInputs ?? Array.Empty<int>();
            Export = export;
        }

        public override string ToString() => $"[{Id}] {Name}";
    }
}