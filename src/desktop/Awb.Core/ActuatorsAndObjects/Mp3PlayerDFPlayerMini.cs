// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project.Various;
using Awb.Core.Timelines;

namespace Awb.Core.ActuatorsAndObjects
{
    public class Mp3PlayerDFPlayerMini : ISoundPlayer
    {
        /// <summary>
        /// The requested target sound id to play for this sound player
        /// </summary>
        private int? _targetValue;

        /// <summary>
        /// The unique id of this sound player
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The unique id of the client this sound player is connected to
        /// </summary>
        public uint ClientId { get; }

        /// <summary>
        /// The optional visible name of the sound player
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The TX pin of the sound player for serial communication
        /// </summary>
        public uint TxPin { get; }

        /// <summary>
        /// The RX pin of the sound player for serial communication
        /// </summary>
        public uint RxPin { get; }

        /// <summary>
        /// The standard volume of this sound player
        /// </summary>
        public uint StandardVolume { get; }

        /// <summary>
        /// The requested target value of this soundplayer
        /// </summary>
        public int? TargetValue
        {
            get => _targetValue;
            set
            {
                if (value != _targetValue)
                {
                    _targetValue = value;
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Indicates if the sound has changed since the last call>
        /// </summary>
        public bool IsDirty { get; set; }

        public int? ActualSoundId { get; private set; }

        public bool IsControllerTuneable => false;

        public ActuatorMovementBySound[] ActuatorMovementsBySound { get; private set; } = [];

        public Mp3PlayerDFPlayerMini(Mp3PlayerDfPlayerMiniConfig config)
        {
            Id = config.Id;
            ClientId = config.ClientId;
            TxPin = config.TxPin;
            RxPin = config.RxPin;
            ActualSoundId = 0;
            StandardVolume = config.Volume;
            IsDirty = true;
            Title = string.IsNullOrWhiteSpace(config.Title) ? $"MP3-{config.Id}" : config.Title;
        }
        public void SetActualSoundId(int? soundId, TimeSpan startTime)
        {
            ActualSoundId = soundId;
        }

        public void SetActuatorMovementBySound(ActuatorMovementBySound[] actuatorMovementsBySound)
        {
            ActuatorMovementsBySound = actuatorMovementsBySound;
        }

        public bool TurnOff()
        {
            TargetValue = null;
            return true;
        }

        public void Dispose()
        {
            TurnOff();
        }


    }
}
