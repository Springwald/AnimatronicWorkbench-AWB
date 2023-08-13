// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Configs;

namespace Awb.Core.Services
{
    public interface IActuatorsService
    {
        IServo[] Servos { get; }

    }

    public class ActuatorsService : IActuatorsService
    {
        public IServo[] Servos { get; }

        public string[] AllIds { get; }

        public ActuatorsService(AwbProject config, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            var servos = new List<IServo>();

            if (config.AdafruitPwmServos != null)
            {
                foreach (var adafruitPwmServoConfig in config.AdafruitPwmServos)
                {

                }
            }

            if (config.StsServos != null)
            {
                foreach (var stsServoConfig in config.StsServos)
                {
                    if (stsServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(stsServoConfig.ClientId);
                    if (client == null)
                    {
                        logger.LogError($"Client with Id '{stsServoConfig.ClientId}' not found!");
                    }
                    var stsServo = new StsServo(stsServoConfig, awbClientsService);
                    servos.Add(stsServo);
                }
            }

            AllIds = servos.Select(s => s.Id).ToArray();
            if (AllIds.Length != AllIds.Distinct().Count())
            {
                foreach (var id in AllIds)
                    if (AllIds.Count(i => id == i) != 1)
                        logger.LogError($"Duplicate servo Id '{id}' found!");
                AllIds = AllIds.Distinct().ToArray();
            }

            Servos = servos.ToArray();
        }

    }
}
