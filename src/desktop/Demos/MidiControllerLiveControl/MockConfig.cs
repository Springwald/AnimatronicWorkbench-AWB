// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License


using Awb.Core.Project;

namespace MidiControllerLiveControl
{
    internal class MockConfig
    {
        public static AwbProject ConfigDemoPuppetTest
        {
            get
            {
                return new AwbProject(title: "demo")
                {
                    StsServos = new StsServoConfig[]
                     {

                         new StsServoConfig(id:"Servo 1", clientId: 2, channel:1)
                         {
                             Name = "Mouth upper",
                             MinValue = 1800,
                             MaxValue = 2200,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 2", clientId: 2, channel:2)
                         {
                             Name = "Mouth lower",
                             MinValue = 1580,
                             MaxValue = 2225,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 3", clientId: 2, channel:3)
                         {
                             Name = "tilt left right",
                             MinValue = 1500,
                             MaxValue = 2500,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 4", clientId: 2, channel:4)
                         {
                             Name = "rot left right",
                             MinValue = 1000,
                             MaxValue = 3000,
                             DefaultValue = null
                         },

                     }
                };
            }
        }
    }
}
