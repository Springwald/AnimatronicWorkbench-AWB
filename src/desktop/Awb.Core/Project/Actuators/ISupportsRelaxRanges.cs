// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Servos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Actuators
{
    public interface ISupportsRelaxRanges
    {
        [Display(Name = "Relax-range", GroupName = "Values", Order = 1)]
        [Description("When the servo is some seconds unchanged and inside this ranges, the servo power will turned off.\r\nFormat:2000-2200\r\nUse commas to list multiple ranges.")]
        [RegularExpression(@"(\d{1,4}[-]\d{1,4},?)*")]
        [JsonIgnore]
        string? RelaxRangesAsString { get; set; }

        /// <summary>
        /// The companion property for the RelaxRangesAsString property.
        /// Needed for json serialization.
        /// </summary>
        ServoRelaxRange[] RelaxRanges { get; set; }
    }
}
