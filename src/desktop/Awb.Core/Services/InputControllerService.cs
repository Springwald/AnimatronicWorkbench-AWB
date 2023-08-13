// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.BCF2000;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.InputControllers.XTouchMini;

namespace Awb.Core.Services
{
    public interface IInputControllerService: IDisposable
    {
        ITimelineController TimelineController { get; }
    }

    public class InputControllerService : IInputControllerService
    {
        private ITimelineController? _timelineController;
        private readonly IAwbLogger _logger;

        public ITimelineController? TimelineController
        {
            get
            {
                if (_timelineController == null)
                {
                    // check behringer x-touch mini
                    var xtouchMidiController = new XTouchMiniController(_logger);
                    if (xtouchMidiController.Available)
                    {
                        _timelineController = new XTouchMiniTimelineController(xtouchMidiController);
                        return _timelineController;
                    }

                    // check behringer BCF2000
                    var bcf2000 = new Bcf2000Controller(_logger);
                    if (bcf2000.Available)
                    {
                        _timelineController = new Bcf2000TimelineController(bcf2000);
                        return _timelineController;
                    }
                }
                return _timelineController;
            }
        }

        public InputControllerService(IAwbLogger logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _timelineController?.Dispose();
            _timelineController = null;
        }
    }
}
