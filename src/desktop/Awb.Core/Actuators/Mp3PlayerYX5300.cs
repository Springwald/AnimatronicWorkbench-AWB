// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Project;

namespace Awb.Core.Actuators
{
    public class Mp3PlayerYX5300 : ISoundPlayer
    {
        /// <summary>
        /// The requested target sound id to play for this sound player
        /// </summary>
        private int _targetValue;

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
        public string? Name { get; }

        /// <summary>
        /// The TX pin of the sound player for serial communication
        /// </summary>
        public uint TxPin { get; }

        /// <summary>
        /// The RX pin of the sound player for serial communication
        /// </summary>
        public uint RxPin { get; }

        /// <summary>
        /// The maximum index of sounds this sound player can play
        /// </summary>
        public int MaxSounds { get; } = 32;

        /// <summary>
        /// The "normal" startup value of this servo
        /// </summary>
        public int DefaultValue { get; }

        /// <summary>
        /// The requested target value of this soundplayer
        /// </summary>
        public int TargetValue
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
        /// Indicates if the servo has changed since the last call>
        /// </summary>
        public bool IsDirty { get; set; }

        public string Label => $"[C{ClientId}-MP3{Id}] {Name ?? string.Empty}";

        public Mp3PlayerYX5300(Mp3PlayerYX5300Config config)
        {
            var defaultValue = 0;

            Id = config.SoundPlayerId;
            ClientId = config.ClientId;
            TxPin = config.TxPin;
            RxPin = config.RxPin;
            DefaultValue = defaultValue;
            DefaultValue = defaultValue;
            TargetValue = defaultValue;
            IsDirty = true;
        }

        public bool TurnOff()
        {
            TargetValue = -1;
            return true;
        }

        public void Dispose()
        {
            TurnOff();
        }
    }
}
