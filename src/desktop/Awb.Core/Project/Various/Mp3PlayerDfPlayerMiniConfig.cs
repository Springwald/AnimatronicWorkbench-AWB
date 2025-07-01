// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Clients;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
{
    public class Mp3PlayerDfPlayerMiniConfig : IDeviceConfig, IProjectObjectListable
    {
        public required string Id { get; set; }

        [Display(Name = "Title", GroupName = "General", Order = 1)]
        [Description("A descriptive title for this mp3 player like 'DFPlayer Mini mp3 player'.")]
        public required string Title { get; set; }

        [Display(Name = "Client ID", GroupName = "General", Order = 2)]
        [Description("The ID of the AWB client device that controls this mp3 player.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; }

        [DisplayName("RX Pin")]
        [Description("The RX pin of the serial connection to the DFPlayer Mini MP3 player.")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public required uint RxPin { get; set; } = 13;

        [DisplayName("TX Pin")]
        [Description("The TX pin of the serial connection to the DFPlayer Mini MP3 player.")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public required uint TxPin { get; set; } = 14;

        [DisplayName("Volume")]
        [Description("The initial volume of the mp3 player.")]
        [Range(1, 30)]
        public required uint Volume { get; set; } = 10;

        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            yield break;
        }

        [JsonIgnore]
        public string TitleShort => String.IsNullOrWhiteSpace(Title) ? $"DFPlayer Mini has no title set '{Id}'" : Title;


        [JsonIgnore]
        public string TitleDetailed => $"{TitleShort} (ClientId: {ClientId}, SoundPlayerId: {Id})";


    }
}
