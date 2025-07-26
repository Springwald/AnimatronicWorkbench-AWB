// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Services;
using MidiControllerLiveControl;

var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);

var clientService = new AwbClientsService(logger);
await clientService.InitAsync();
await clientService.ScanForClients(fastMode: true);
var config = MockConfig.ConfigDemoPuppetTest;
IActuatorsService actuatorsService = new ActuatorsService(config, clientService, logger);

//var demo = new XTouchMiniMidiControlDemo(actuatorsService, clientService, logger);
var demo = new BCF2000MidiControllerDemo(actuatorsService, clientService, logger);
await demo.Run();