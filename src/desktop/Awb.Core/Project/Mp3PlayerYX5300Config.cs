// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.Project
{
    public class Mp3PlayerYX5300Config
    {
        public uint ClientId { get; set; }

        /// <summary>
        ///  A specific if for multi puppet / multi loudspeaker scenarios
        /// </summary>
        public string SoundPlayerId { get; set; }

        public string? Name { get; set; } 

        /// <summary>
        /// the RX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint RxPin { get; } = 13;

        /// <summary>
        /// the TX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint TxPin { get; } = 14;

        public Mp3PlayerYX5300Config(uint clientId, uint rxPin, uint txPin, string soundPlayerId, string? name)
        {
            ClientId = clientId;
            RxPin = rxPin;
            TxPin = txPin;
            SoundPlayerId = soundPlayerId;
            Name = name;
        }


    }
}
