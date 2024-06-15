// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
{
    public class Mp3PlayerYX5300Config : IDeviceConfig, IProjectObjectListable
    {
        public required string Id { get; set; }

        [DisplayName("Client ID")]
        [Description("The ID of the AWB client device that controls this mp3 player.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; }

        [DisplayName("Title")]
        [Description("A descriptive title for this mp3 player like 'yx5300 mp3 player'.")]
        public required string Title { get; set; }

        [DisplayName("RX Pin")]
        [Description("The RX pin of the serial connection to the YX5300 MP3 player.")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public required uint RxPin { get; set; } = 13;

        [DisplayName("TX Pin")]
        [Description("The TX pin of the serial connection to the YX5300 MP3 player.")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public required uint TxPin { get; set; } = 14;

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            yield break;
        }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"Mp3PlayerYX5300 has no title set '{Id}'" : Title;


        [JsonIgnore]
        public string TitleDetailed => $"{TitleShort} (ClientId: {ClientId}, SoundPlayerId: {Id})";


    }
}
