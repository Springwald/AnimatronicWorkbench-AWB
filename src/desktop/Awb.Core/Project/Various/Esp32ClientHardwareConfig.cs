// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project.Servos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Awb.Core.Project.Various
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

        /* STS serial servo settings */
        
        [DisplayName("STS servos")]
        [Description("Check if you use STS bus servos e.g. from feetech or waveshare")]
        public bool Use_Sts_Servo { get; set; } = false;

        [DisplayName("STS servo default speed")]
        [Description(StsFeetechServoConfig.SpeedDescriptionConst)]
        [Range(-1, StsFeetechServoConfig.MaxSpeedConst)]
        public int? StsServoSpeed { get; set; } = 1500;

        [DisplayName("STS servo default acceleration")]
        [Description(StsFeetechServoConfig.AccDescriptionConst)]
        [Range(-1, StsFeetechServoConfig.MaxAccConst)]
        public int? StsServoAcceleration { get; set; } = 100;

        [DisplayName("STS servo RXD pin")]
        [Description("eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int? StsServoRxd { get; set; } = 18;

        [DisplayName("STS servo TXD pin")]
        [Description("eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int? StsServoTxd { get; set; } = 19;

        [DisplayName("STS servo max temperature")]
        [Description("max temperature (celsius) before servo is disabled")]
        [Range(20, 70)]
        public int? StsServoMaxTemp{ get; set; } = 55;

        [DisplayName("STS servo max load")]
        [Description("max load before servo is disabled")]
        [Range(0, 4096)]
        public int? StsServoMaxLoad { get; set; } = 400;

        /* autoplay state selector */
        // if a servo position feedback is used as a state selector, define the servo channel here.
        // if you don't use a servo as state selector, set this to -1 or undefine it
        // #define AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL 9
        // if the servo position feedback is not exatly 0 at the first state, define the offset here (-4096 to 4096)
        //#define AUTOPLAY_STATE_SELECTOR_STS_SERVO_POS_OFFSET 457

        /* SCS serial servo settings */

        [DisplayName("SCS servos")]
        [Description("Check if you use SCS bus servos e.g. from feetech or waveshare")]
        public bool Use_Scs_Servo { get; set; } = false;

        [DisplayName("SCS servo default speed")]
        [Description(ScsFeetechServoConfig.SpeedDescriptionConst)]
        [Range(-1, ScsFeetechServoConfig.MaxSpeedConst)]
        public int? ScsServoSpeed { get; set; } = 500;

        [DisplayName("SCS servo RXD pin")]
        [Description("eg. GPIO 18 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int? ScsServoRxd { get; set; } = 18;

        [DisplayName("SCS servo TXD pin")]
        [Description("eg. GPIO 19 for waveshare servo driver")]
        [Range(1, Esp32.MaxGpIoPortNumber)]
        public int? ScsServoTxd { get; set; } = 19;

        [DisplayName("STS servo max temperature")]
        [Description("max temperature (celsius) before servo is disabled")]
        [Range(20, 70)]
        public int? ScsServoMaxTemp { get; set; } = 55;

        [DisplayName("STS servo max load")]
        [Description("max load before servo is disabled")]
        [Range(0, 4096)]
        public int? ScsServoMaxLoad { get; set; } = 600;

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
                    Source = this.TitleShort
                };
                if (string.IsNullOrWhiteSpace(Ssd1306ComPins)) yield return new ProjectProblem
                {
                    Message = "SSD1306 COM pins have to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = this.TitleShort
                };
                if (Ssd1306ScreenHeight == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen height has to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = this.TitleShort
                };
                if (Ssd1306ScreenWidth == null) yield return new ProjectProblem
                {
                    Message = "SSD1306 screen width has to be set",
                    Category = ProjectProblem.Categories.Various,
                    ProblemType = ProjectProblem.ProblemTypes.Error,
                    Source = this.TitleShort
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
