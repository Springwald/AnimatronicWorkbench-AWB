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
        [DisplayName("Client ID")]
        [Description("The ID of this ESP32 client device.")]
        [Range(1, 254)]
        public required uint ClientId { get; set; } = 1;

        [DisplayName("Debugging IO pin")]
        [Description(" The GPIO pin to use for debugging")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public uint? DebuggingIoPin { get; set; } = 1;

        [DisplayName("Debugging IO pin active state")]
        [Description("Define the active state of the debugging GPIO pin")]
        [RegularExpression("HIGH|LOW")]
        public string? DebuggingIoPinActiveState { get; set; } = "HIGH";

        /* Display settings */

        /**  -- M5 Stack Displays -- **/

        [DisplayName("M5STACK display")]
        [Description("Check to use a M5Stack. Uses GPIO 14+18 for display communication")]
        public bool Display_M5Stack { get; set; }

        /** -- SSD1306 Displays  -- **/

        [DisplayName("SSD1306 display")]
        [Description("Check e.g. for Waveshare Servo Driver with ESP32 (using 128x32, 0x02, 0x3C)")]
        public bool Display_Ssd1306 { get; set; } = true;

        [DisplayName("SSD1306 I2C address")]
        [Description("0x3C or 0x3D, See datasheet for Address; 0x3D for 128x64, 0x3C for 128x32")]
        [RegularExpression("(0x3C|0x3D)")]
        public string? Ssd1306I2cAddress { get; set; } = "0x3D";

        [DisplayName("SSD1306 screen width")]
        [Description("Screen width of SDD1306 display")]
        [Range(1, 1024)]
        public int? Ssd1306ScreenWidth { get; set; } = 128;

        [DisplayName("SSD1306 screen height")]
        [Description("Screen height of SDD1306 display")]
        [Range(1, 1024)]
        public int? Ssd1306ScreenHeight { get; set; } = 64;

        [DisplayName("SSD1306 COM pins")]
        [Description("The SSD1306 may have different connection patterns between the panel and controller depending on the product, and the 0xDA command adjusts for this.\r\nIf it does not display correctly, try the following values: 0x02, 0x12, 0x22 or 0x32")]
        [RegularExpression("(0x02|0x12|0x22|0x32)")]
        public string? Ssd1306ComPins { get; set; } = "0x12";

        /* Neopixel status LEDs */
        // #define USE_NEOPIXEL_STATUS_CONTROL
        //#define STATUS_RGB_LED_GPIO 23      // the GPIO used to control RGB LEDs. GPIO 23, as default.
        //#define STATUS_RGB_LED_NUMPIXELS 13 // how many RGB LEDs are connected to the GPIO

        /* PCA9685 PWM servo settings */
        // #define USE_PCA9685_PWM_SERVO // uncomment this line if you want to use PCA9685 PWM servos
        //#define PCA9685_I2C_ADDRESS 0x40
        //#define PCA9685_OSC_FREQUENCY 25000000

        /* MP3-Player YX5300 */
        //#define USE_MP3_PLAYER_YX5300
        //#define MP3_PLAYER_YX5300_RXD 13
        //#define MP3_PLAYER_YX5300_TXD 14

        /* DAC speaker */
        // #define USE_DAC_SPEAKER


        public IEnumerable<ProjectProblem> GetContentProblems(AwbProject project)
        {
            if (Display_Ssd1306 == true)
            {
                if (string.IsNullOrWhiteSpace(Ssd1306I2cAddress)) yield return new ProjectProblem
                {
                    Message = "SSD1306 I2C address has to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (string.IsNullOrWhiteSpace(Ssd1306ComPins)) yield return new ProjectProblem
                {
                    Message = "SSD1306 COM pins have to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (Ssd1306ScreenHeight == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen height has to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = TitleShort
                };
                if (Ssd1306ScreenWidth == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen width has to be set",
                    Category = ProjectProblem.Categories.Various,
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
