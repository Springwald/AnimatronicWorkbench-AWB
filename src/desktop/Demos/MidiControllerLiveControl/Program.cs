using Awb.Core.Services;
using MidiControllerLiveControl;

var logger = new AwbLoggerConsole(throwWhenInDebugMode: false);

var clientService = new AwbClientsService(logger);
await clientService.Init();
var config = MockConfig.ConfigDemoPuppetTest;
IActuatorsService actuatorsService = new ActuatorsService(config, clientService, logger);

//var demo = new XTouchMiniMidiControlDemo(actuatorsService, clientService, logger);
var demo = new BCF2000MidiControllerDemo(actuatorsService, clientService, logger);
await demo.Run();