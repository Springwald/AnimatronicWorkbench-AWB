// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Sounds;

namespace Awb.Core.Actuators
{
    /// <summary>
    /// A sound player to let the animatronic figure speak or play sounds
    /// </summary>
    public interface ISoundPlayer : IActuator
    {
        int? ActualSoundId { get;}
        void PlaySound(int soundId, TimeSpan startTime);

        void SetNoSound();
    }
}
