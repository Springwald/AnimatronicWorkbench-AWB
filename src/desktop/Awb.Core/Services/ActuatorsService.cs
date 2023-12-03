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

        ISoundPlayer SoundPlayer { get; }

         string[] AllIds { get; }
        IActuator[] AllActuators { get; }
    }

    public class ActuatorsService : IActuatorsService
    {
        public IServo[] Servos { get; }

        public string[] AllIds { get; }

        public IActuator[] AllActuators { get; }

        public ISoundPlayer SoundPlayer { get; }

        public ActuatorsService(AwbProject config, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            var servos = new List<IServo>();

            // add PWM servos
            if (config.Pca9685PwmServos != null)
            {
                foreach (var pca9685PwmServoConfig in config.Pca9685PwmServos)
                {
                    if (pca9685PwmServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(pca9685PwmServoConfig.ClientId);
                    if (client == null)
                        logger.LogError($"ActuatorsService: Client with Id '{pca9685PwmServoConfig.ClientId}' for Pca9685PwmServo '{pca9685PwmServoConfig.Name}' not found!");
                    var stsServo = new Pca9685PwmServo(pca9685PwmServoConfig);
                    servos.Add(stsServo);
                }
            }

            // add STS servos
            if (config.StsServos != null)
            {
                foreach (var stsServoConfig in config.StsServos)
                {
                    if (stsServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(stsServoConfig.ClientId);
                    if (client == null)
                        logger.LogError($"ActuatorsService: Client with Id '{stsServoConfig.ClientId}' for stsServo '{stsServoConfig.Name}' not found!");
                    var stsServo = new StsServo(stsServoConfig);
                    servos.Add(stsServo);
                }
            }

            // add sound player
            if (config.Mp3PlayerYX5300 != null) 
            {
                SoundPlayer = new Mp3PlayerYX5300(config.Mp3PlayerYX5300);
            }

            
            var allActuators = new List<IActuator>();
            allActuators.AddRange(servos);    
            if (SoundPlayer != null) allActuators.Add(SoundPlayer);
            AllActuators = allActuators.ToArray();

            // check for duplicate Ids
            AllIds = allActuators.Select(a => a.Id).ToArray();
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
