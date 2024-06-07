// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Awb.Core.Project
{
    public class ProjectMetaData : IProjectObjectListable
    {
        private string? _wifiSsid;

        [DataMember(Name = "Project title")]
        public string ProjectTitle { get; set; } = string.Empty;

        [JsonIgnore]
        public string TitleShort => "AWB project meta data";

        [JsonIgnore]
        public string TitleDetailled => TitleShort;

        public string? Info { get; set; }

        public string WifiSsid
        {
            get => _wifiSsid ?? $"AWB-{ProjectTitle}"; // todo: remove invalid chars
            set => _wifiSsid = value;
        }
        public string WifiPassword { get; set; } = "awb12345";



    }
}
