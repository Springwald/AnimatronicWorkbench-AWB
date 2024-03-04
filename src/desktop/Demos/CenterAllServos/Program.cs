// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.Configs;
using Awb.Core.Services;
using ResetAllServos;


var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);
var clientService = new AwbClientsService(logger);
await clientService.Init();
var config = new AwbProject(title:"demo")
{
    StsServos = Enumerable.Range(1, 2).Select(id =>
                         new StsServoConfig(id: $"servo {id}", clientId: 2, channel: (uint)id)
                         {
                             Name = $"Servo {id}",
                             MinValue = 1,
                             MaxValue = 4096,
                             DefaultValue = 2048
                         }).ToArray()
};
IActuatorsService actuatorsService = new ActuatorsService(config, clientService, logger);
var stsServoReset = new StsServoCenter(actuatorsService, clientService, logger);


Console.WriteLine("Start sending");

await stsServoReset.Run();

Console.WriteLine("Done sending");