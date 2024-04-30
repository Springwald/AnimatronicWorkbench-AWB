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

                         new StsServoConfig(id:"Servo 1",title:"Mouth upper", clientId: 2, channel:1)
                         {
                             MinValue = 1800,
                             MaxValue = 2200,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 2",title: "Mouth lower",clientId: 2, channel:2)
                         {
                             MinValue = 1580,
                             MaxValue = 2225,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 3", title:"tilt left right",clientId: 2, channel:3)
                         {
                             MinValue = 1500,
                             MaxValue = 2500,
                             DefaultValue = null
                         },
                         new StsServoConfig(id:"Servo 4", title: "rot left right", clientId: 2, channel:4)
                         {
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
