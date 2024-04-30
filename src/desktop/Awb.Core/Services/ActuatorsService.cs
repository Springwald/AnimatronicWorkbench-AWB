// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Actuators;
using Awb.Core.Project;

namespace Awb.Core.Services
{
    public interface IActuatorsService
    {
        IServo[] Servos { get; }

        ISoundPlayer[] SoundPlayers { get; }

         string[] AllIds { get; }
        IActuator[] AllActuators { get; }
    }

    public class ActuatorsService : IActuatorsService
    {
        public IServo[] Servos { get; }

        public string[] AllIds { get; }

        public IActuator[] AllActuators { get; }


        public ISoundPlayer[] SoundPlayers { get; }

        public ActuatorsService(AwbProject projectConfig, IAwbClientsService awbClientsService, IAwbLogger logger)
        {
            var servos = new List<IServo>();

            // add PWM servos
            if (projectConfig.Pca9685PwmServos != null)
            {
                foreach (var pca9685PwmServoConfig in projectConfig.Pca9685PwmServos)
                {
                    if (pca9685PwmServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(pca9685PwmServoConfig.ClientId);
                    if (client == null)
                        logger.LogErrorAsync($"ActuatorsService: Client with Id '{pca9685PwmServoConfig.ClientId}' for Pca9685PwmServo '{pca9685PwmServoConfig.Title}' not found!");
                    var stsServo = new Pca9685PwmServo(pca9685PwmServoConfig);
                    servos.Add(stsServo);
                }
            }

            // add STS servos
            if (projectConfig.StsServos != null)
            {
                foreach (var stsServoConfig in projectConfig.StsServos)
                {
                    if (stsServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(stsServoConfig.ClientId);
                    if (client == null)
                        logger.LogErrorAsync($"ActuatorsService: Client with Id '{stsServoConfig.ClientId}' for stsServo '{stsServoConfig.Title}' not found!");
                    var stsServo = new StsScsServo(stsServoConfig, StsScsServo.StsScsTypes.Sts);
                    servos.Add(stsServo);
                }
            }

            // add SCS servos
            if (projectConfig.ScsServos != null)
            {
                foreach (var scsServoConfig in projectConfig.ScsServos)
                {
                    if (scsServoConfig?.ClientId == null) throw new ArgumentNullException("ClientId must be set.");
                    var client = awbClientsService.GetClient(scsServoConfig.ClientId);
                    if (client == null)
                        logger.LogErrorAsync($"ActuatorsService: Client with Id '{scsServoConfig.ClientId}' for scsServo '{scsServoConfig.Title}' not found!");
                    var scsServo = new StsScsServo(scsServoConfig, StsScsServo.StsScsTypes.Scs);
                    servos.Add(scsServo);
                }
            }

            // add sound player
            if (projectConfig.Mp3PlayersYX5300 != null)
            {
                // actual is only one sound player supported
                SoundPlayers = projectConfig.Mp3PlayersYX5300.Select(p => new Mp3PlayerYX5300(p)).ToArray();
            }
            else
            {
                SoundPlayers = Array.Empty<ISoundPlayer>();
            }
            
            var allActuators = new List<IActuator>();
            allActuators.AddRange(servos);    
            allActuators.AddRange(SoundPlayers);
            AllActuators = allActuators.ToArray();

            // check for duplicate Ids
            AllIds = allActuators.Select(a => a.Id).ToArray();
            if (AllIds.Length != AllIds.Distinct().Count())
            {
                foreach (var id in AllIds)
                    if (AllIds.Count(i => id == i) != 1)
                        logger.LogErrorAsync($"Duplicate servo Id '{id}' found!");
                AllIds = AllIds.Distinct().ToArray();
            }

            Servos = servos.ToArray();
        }

    }
}
