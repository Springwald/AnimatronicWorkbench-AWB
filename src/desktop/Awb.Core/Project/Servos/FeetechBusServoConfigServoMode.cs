// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2026 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Project.Actuators;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Servos
{
    public abstract class FeetechBusServoConfigServoMode : FeetechBusServoConfig, ISupportsRelaxRanges
    {
        /// <summary>
        /// The companion property for the RelaxRangesAsString property.
        /// Needed for json serialization.
        /// </summary>
        public ServoRelaxRange[] RelaxRanges { get; set; } = Array.Empty<ServoRelaxRange>();


        // [Display(Name = "Relax-range", GroupName = "Values", Order = 1)]
        //  [Description("When the servo is some seconds unchanged and inside this ranges, the servo power will turned off.\r\nFormat:2000-2200\r\nUse commas to list multiple ranges.")]
        //  [RegularExpression(@"(\d{1,4}[-]\d{1,4},?)*")]
        [JsonIgnore]
        public string? RelaxRangesAsString
        {
            get => ServoRelaxRange.ToString(RelaxRanges);
            set => RelaxRanges = ServoRelaxRange.FromString(value);
        }

        [Display(Name = "Speed value", GroupName = "Values", Order = 7)]
        public abstract int? Speed { get; set; }

       
    }
}
