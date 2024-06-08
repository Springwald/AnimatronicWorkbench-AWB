// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
{
    public class Mp3PlayerYX5300Config : IDeviceConfig, IProjectObjectListable
    {
        public uint ClientId { get; set; }

        /// <summary>
        ///  A specific if for multi puppet / multi loudspeaker scenarios
        /// </summary>
        public string SoundPlayerId { get; set; }

        public string Title { get; set; }

        [JsonIgnore]
        public string TitleShort => Title ?? $"No Title for Mp3PlayerYX5300 '{Id}'";

        [JsonIgnore]
        public string TitleDetailled => $"{TitleShort} (ClientId: {ClientId}, SoundPlayerId: {SoundPlayerId})";


        /// <summary>
        /// the RX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint RxPin { get; } = 13;

        /// <summary>
        /// the TX pin of the serial connection to the YX5300 MP3 player
        /// </summary>
        public uint TxPin { get; } = 14;
        public string Id { get; set; }


        public Mp3PlayerYX5300Config(uint clientId, string id, uint rxPin, uint txPin, string soundPlayerId, string title)
        {
            ClientId = clientId;
            Id = id;
            RxPin = rxPin;
            TxPin = txPin;
            SoundPlayerId = soundPlayerId;
            Title = title;
        }


    }
}
