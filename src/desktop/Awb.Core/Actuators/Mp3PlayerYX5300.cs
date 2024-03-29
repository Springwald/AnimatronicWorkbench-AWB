﻿// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
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
        /// Indicates if the sound has changed since the last call>
        /// </summary>
        public bool IsDirty { get; set; }

        

        public int ActualSoundId { get; private set; }

        public Mp3PlayerYX5300(Mp3PlayerYX5300Config config, int soundsCount)
        {
            Id = config.SoundPlayerId;
            ClientId = config.ClientId;
            TxPin = config.TxPin;
            RxPin = config.RxPin;
            ActualSoundId = 0;
            IsDirty = true;
        }

        public void PlaySound(int soundId)
        {
            ActualSoundId = soundId;
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
