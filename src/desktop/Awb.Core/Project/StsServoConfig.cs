// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using System.Text.Json.Serialization;

namespace Awb.Core.Project
{
    public class StsServoConfig : IDeviceConfig, IProjectObjectListable
    {
        public string Id { get; set; }
        public uint ClientId { get; set; }
        public uint Channel { get; set; }
        public string Title { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int? DefaultValue { get; set; }
        public int? Acceleration { get; set; }
        public int? Speed { get; set; }

        /// <summary>
        /// If this servo is in fault state (e.g.  overheat, overtorque, etc.) should all actuators be deactivated or only this one?
        /// </summary>
        public bool GlobalFault { get; set; }

        [JsonIgnore]
        public string TitleShort => Title ?? $"StsServo has no title set '{Id}'";

        [JsonIgnore]
        public string TitleDetailled => $"StsServo '{TitleShort}' (Id: {Id}, ClientId: {ClientId}, Channel: {Channel})";

        public StsServoConfig(string id, string title, uint clientId, uint channel)
        {
            this.Id = id;
            this.ClientId = clientId;
            this.Channel = channel;
            this.Title = title;
        }
    }
}
