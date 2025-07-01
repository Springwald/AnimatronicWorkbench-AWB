// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License


using Awb.Core.Project;
using Awb.Core.Project.Servos;
using Awb.Core.Project.Various;

namespace MidiControllerLiveControl
{
    internal class MockConfig
    {
        public static AwbProject ConfigDemoPuppetTest
        {
            get
            {
                return new AwbProject()
                {
                    ProjectMetaData = new ProjectMetaData
                    {
                        ProjectTitle = "demo"
                    },
                    StsServos =
                     [
                         new StsFeetechServoConfig
                         {
                             Id = "Servo 1",
                             Title = "Mouth upper",
                             ClientId = 2,
                             Channel = 1,
                             MinValue = 1800,
                             MaxValue = 2200,
                             DefaultValue = null
                         },
                         new StsFeetechServoConfig
                         {
                             Id = "Servo 2",
                             Title = "Mouth lower",
                             ClientId = 2,
                             Channel = 2,
                             MinValue = 1580,
                             MaxValue = 2225,
                             DefaultValue = null
                         },
                         new StsFeetechServoConfig
                         {
                             Id = "Servo 3",
                             Title = "tilt left right",
                             ClientId =2,
                             Channel = 3,
                             MinValue = 1500,
                             MaxValue = 2500,
                             DefaultValue = null
                         },
                         new StsFeetechServoConfig
                         {
                             Id = "Servo 4",
                             Title = "rotate left right",
                             ClientId = 2,
                             Channel = 4,
                             MinValue = 1000,
                             MaxValue = 3000,
                             DefaultValue = null
                         },
                     ]
                };
            }
        }
    }
}
