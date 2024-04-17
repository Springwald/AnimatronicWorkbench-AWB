// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using Awb.Core.InputControllers.BCF2000;
using Awb.Core.InputControllers.TimelineInputControllers;
using Awb.Core.InputControllers.XTouchMini;
using Awb.Core.Tools;

namespace Awb.Core.Services
{
    public interface IInputControllerService : IDisposable
    {
        ITimelineController[] TimelineControllers { get; }
    }

    public class InputControllerService : IInputControllerService
    {
        private ITimelineController[]? _timelineControllers;
        private readonly IAwbLogger _logger;
        private readonly ITimelineController[] _additionalTimelineControllers;
        private readonly IInvoker _invoker;

        public ITimelineController[] TimelineControllers
        {
            get
            {
                if (_timelineControllers == null)
                {
                    var list = new List<ITimelineController>();

                    // check behringer x-touch mini
                    var xtouchMidiController = new XTouchMiniController(_logger, _invoker);
                    if (xtouchMidiController.Available)
                        list.Add(new XTouchMiniTimelineController(xtouchMidiController));

                    // check behringer BCF2000
                    var bcf2000 = new Bcf2000Controller(_logger, _invoker);
                    if (bcf2000.Available)
                        list.Add(new Bcf2000TimelineController(bcf2000));

                    foreach (var additional in _additionalTimelineControllers)
                        list.Add(additional);

                    _timelineControllers = list.ToArray();
                }
                return _timelineControllers!;
            }
        }

        public InputControllerService(IAwbLogger logger, IInvokerService invokerService, ITimelineController[] additionalTimelineControllers)
        {
            _logger = logger;
            _additionalTimelineControllers = additionalTimelineControllers;
            _invoker = invokerService.GetInvoker();
        }

        public void Dispose()
        {
            if (_timelineControllers != null)
                foreach (var controller in _timelineControllers)
                    controller.Dispose();
            _timelineControllers = null;
        }
    }
}
