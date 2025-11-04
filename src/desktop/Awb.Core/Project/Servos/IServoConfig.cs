// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Awb.Core.Project.Servos
{
    public interface IServoConfig
    {
        [Display(Name = "Title", GroupName = "General", Order = 1)]
        [Description("A descriptive title for this servo like 'left-upper eyelid'.")]
        string Title { get; }

        /// <summary>
        /// Is this servo able to report its position?
        /// </summary>
        bool CanReadServoPosition { get; }

        int MinValue { get; }
        int MaxValue { get; }
    }
}
