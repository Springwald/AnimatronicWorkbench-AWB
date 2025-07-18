﻿// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;

namespace Awb.Core.Actuators
{
    /// <summary>
    /// A sound player to let the animatronic figure speak or play sounds
    /// </summary>
    public interface ISoundPlayer : IActuator
    {
        int? ActualSoundId { get; }
        ActuatorMovementBySound[] ActuatorMovementsBySound { get; }
        void SetActualSoundId(int? soundId, TimeSpan startTime);
        void SetActuatorMovementBySound(ActuatorMovementBySound[] actuatorMovementsBySound);
    }
}
