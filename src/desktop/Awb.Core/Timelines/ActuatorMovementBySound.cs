// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

namespace Awb.Core.Timelines
{
    public class ActuatorMovementBySound
    {
        public string? ActuatorId { get; set; }
        public bool MovementInverted { get; set; } // up/down movement inverted, e.g. for a jaw servo
        public int MovementOffsetMs { get; set; } = 0; // offset in ms to the sound start, e.g. for moving a servo before the sound starts
        public int MovementFrequencyMs { get; set; } = 50; // frequency in ms tp create servo points

        public string PainterChecksum => $"{ActuatorId}-{MovementInverted}-{MovementOffsetMs}-{MovementFrequencyMs}";

        public bool IsEqual(ActuatorMovementBySound? other)
        {
            if (other is null) return false;
            return ActuatorId == other.ActuatorId &&
                   MovementInverted == other.MovementInverted &&
                   MovementOffsetMs == other.MovementOffsetMs &&
                   MovementFrequencyMs == other.MovementFrequencyMs;
        }

        public static bool AreEqual(ActuatorMovementBySound[] a, ActuatorMovementBySound[] b)
        {
            if (a is null) return b is null;
            if (b is null) return a is null;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (!a[i].IsEqual(b[i])) return false;
            return true;
        }
    }
}
