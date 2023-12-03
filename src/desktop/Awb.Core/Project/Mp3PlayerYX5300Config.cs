// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
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
        public string SoundPlayerId { get; }

        /// <summary>
        /// the RX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint RxPin { get; } = 15;

        /// <summary>
        /// the TX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint TxPin { get; } = 17;

        public Mp3PlayerYX5300Config(uint rxPin, uint txPin, string soundPlayerId)
        {
            RxPin = rxPin;
            TxPin = txPin;
            SoundPlayerId = soundPlayerId;
        }

     
    }
}
