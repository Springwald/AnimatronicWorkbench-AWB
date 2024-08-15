// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Clients
{
    public class Esp32ClientHardwareConfig : IProjectObjectListable
    {
        #region Main

        [Display(Name = "Client ID", GroupName ="General", Order = 1)]
        [Description("The ID of this ESP32 client device.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        #endregion


        #region Autoplay

        [Display(Name = "Return to automode", GroupName = "AutoPlay", Order = 1)]
        [Description("Number of minutes after the last signal from Animatronic WorkBench Studio before returning to AutoPlay")]
        [Range(1, 5)]
        public int? AutoPlayAfter { get; set; } = null;

        #endregion

        #region Debugging

        [Display(Name = "Debugging IO pin", GroupName = "Debugging", Order = 1)]
        [Description(" The GPIO pin to use for debugging")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? DebuggingIoPin { get; set; } = null;

        [Display(Name = "Debugging IO pin active state", GroupName = "Debugging", Order = 1)]
        [Description("Define the active state of the debugging GPIO pin")]
        [RegularExpression("HIGH|LOW")]
        public string? DebuggingIoPinActiveState { get; set; } = "HIGH";

        #endregion


        #region Display settings

        /**  -- M5 Stack Displays -- **/

        [Display(Name = "M5STACK display", GroupName = "Display M5Stack", Order = 1)]
        [Description("Check to use a M5Stack. Uses GPIO 14+18 for display communication")]
        public bool Display_M5Stack { get; set; }

        /** -- SSD1306 Displays  -- **/

        [Display(Name = "use SSD1306 display", GroupName = "SSD1306 display", Order = 1)]
        [Description("Check e.g. for Waveshare Servo Driver with ESP32 (using 128x32, 0x02, 0x3C)")]
        public bool Display_Ssd1306 { get; set; } = true;

        [Display(Name = "I2C address", GroupName = "SSD1306 display", Order = 2)]
        [Description("0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32")]
        [RegularExpression("(0x3C|0x3D)")]
        public string? Ssd1306I2cAddress { get; set; } = "0x3D";

        [Display(Name = "screen width", GroupName = "SSD1306 display", Order = 3)]
        [Description("Screen width of SDD1306 display")]
        [Range(1, 1024)]
        public int? Ssd1306ScreenWidth { get; set; } = 128;

        [Display(Name = "screen height", GroupName = "SSD1306 display", Order = 4)]
        [Description("Screen height of SDD1306 display")]
        [Range(1, 1024)]
        public int? Ssd1306ScreenHeight { get; set; } = 64;

        [Display(Name = "COM pins", GroupName = "SSD1306 display", Order = 5)]
        [Description("The SSD1306 may have different connection patterns between the panel and controller depending on the product, and the 0xDA command adjusts for this.\r\nIf it does not display correctly, try the following values: 0x02, 0x12, 0x22 or 0x32")]
        [RegularExpression("(0x02|0x12|0x22|0x32)")]
        public string? Ssd1306ComPins { get; set; } = "0x12";

        #endregion


        #region SCS servos

        [Display(Name = "use SCS servo bus", GroupName = "SCS servo bus", Order = 1)]
        [Description("Use a feetech/waveshare servo SCS bus adapter")]
        public bool UseScsServos { get; set; }

        [Display(Name = "TX IO pin", GroupName = "SCS servo bus", Order = 2)]
        [Description("Define the SCS servo GPIO pin for TX, eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? ScsTXPin { get; set; } = 19;

        [Display(Name = "RX IO pin", GroupName = "SCS servo bus", Order = 3)]
        [Description("Define the SCS servo GPIO pin for RX, eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? ScsRXPin { get; set; } = 18;

        #endregion


        #region STS Servos

        [Display(Name = "use STS servo bus", GroupName = "STS servo bus", Order = 1)]
        [Description("Use a feetech/waveshare servo STS bus adapter")]
        public bool UseStsServos { get; set; }

        [Display(Name = "TX IO pin", GroupName = "STS servo bus", Order = 2)]
        [Description("Define the STS servo GPIO pin for TX, eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? StsTXPin { get; set; } = 17;

        [Display(Name = "RX IO pin", GroupName = "STS servo bus", Order = 3)]
        [Description("Define the STS servo GPIO pin for RX, eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? StsRXPin { get; set; } = 16;

        #endregion


        #region Neopixel

        [Display(Name = "use NeoPixel", GroupName = "Neopixel", Order = 1)]
        [Description("Use adressable RGB LEDs")]
        public bool UseNeoPixel { get; set; } = false;

        [Display(Name = "Data pin", GroupName = "Neopixel", Order = 2)]
        [Description("the GPIO used to control RGB LEDs. GPIO 26, as default.")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? NeoPixelPin { get; set; }

        [Display(Name = "Count", GroupName = "Neopixel", Order = 3)]
        [Description("How many RGB LEDs are connected to the data pin")]
        [Range(1, 255)]
        public uint? NeoPixelCount { get; set; }

        #endregion

        /* DAC speaker */
        // #define USE_DAC_SPEAKER


        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            if (Display_Ssd1306 == true)
            {
                if (string.IsNullOrWhiteSpace(Ssd1306I2cAddress)) yield return new ProjectProblem
                {
                    Message = "SSD1306 I2C address has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (string.IsNullOrWhiteSpace(Ssd1306ComPins)) yield return new ProjectProblem
                {
                    Message = "SSD1306 COM pins have to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (Ssd1306ScreenHeight == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen height has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (Ssd1306ScreenWidth == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen width has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
            }

            if (Display_M5Stack == true && Display_Ssd1306 == true)
                {
                yield return new ProjectProblem
                {
                    Message = "Only one display can be used at a time",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
            }

            if (UseScsServos == true)
            {
                if (ScsTXPin == null) yield return new ProjectProblem
                {
                    Message = "SCS servo TX IO pin has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (ScsRXPin == null) yield return new ProjectProblem
                {
                    Message = "SCS servo RX IO pin has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
            }

            if (UseStsServos == true)
            {
                if (StsTXPin == null) yield return new ProjectProblem
                {
                    Message = "STS servo TX IO pin has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (StsRXPin == null) yield return new ProjectProblem
                {
                    Message = "STS servo RX IO pin has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
            }

            if (UseNeoPixel)
            {
                if (NeoPixelPin == null) yield return new ProjectProblem
                {
                    Message = "NeoPixel data pin has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (NeoPixelCount == null) yield return new ProjectProblem
                {
                    Message = "NeoPixel count has to be set",
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
            }

            yield break;
        }

        [JsonIgnore]
        public string TitleShort => "ESP32 client hardware";

        [JsonIgnore]
        public string TitleDetailed => TitleShort;
    }
}
