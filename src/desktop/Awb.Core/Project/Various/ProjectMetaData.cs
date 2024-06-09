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
    public class ProjectMetaData : IProjectObjectListable
    {
        private string? _wifiSsid;

        [DisplayName("Project title")]
        [Description("The working title of the project, often a figure name.")]
        [Length(1,16)]
        [RegularExpression(pattern:"[A-Za-z0-9_-]+",ErrorMessage ="Only chars A-Z,a-z, 0-9 and '__' or '-' allowed (e.g. no space).")]
        public string ProjectTitle { get; set; } = string.Empty;

        [DisplayName("Info description")]
        public string? Info { get; set; }

        public string WifiSsid
        {
            get => _wifiSsid ?? $"AWB-{ProjectTitle}"; // todo: remove invalid chars
            set => _wifiSsid = value;
        }

        [DisplayName("Wifi password")]
        [Length(8, 32)]
        public string WifiPassword { get; set; } = "awb12345";

        [JsonIgnore]
        public string TitleShort => "AWB project meta data";

        [JsonIgnore]
        public string TitleDetailled => TitleShort;

    }
}
