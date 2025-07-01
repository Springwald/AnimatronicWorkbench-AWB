// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using Awb.Core.Timelines;
using AwbStudio.TimelineEditing;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AwbStudio.TimelineValuePainters
{
    internal abstract class AbstractValuePainter : IDisposable
    {
        protected TimelineData? _timelineData;
        protected readonly TimelineViewContext _viewContext;
        protected readonly TimelineCaptions _captions;
        protected List<UIElement> _valueControls;
        public Grid PaintControl { get; }

        public AbstractValuePainter(Grid paintControl, TimelineViewContext viewContext, TimelineCaptions captions)
        {
            _captions = captions ?? throw new ArgumentNullException(nameof(captions));

            _viewContext = viewContext ?? throw new ArgumentNullException(nameof(viewContext));
            _viewContext.Changed += OnViewContextChanged;

            _valueControls = new List<UIElement>();

            this.PaintControl = paintControl ?? throw new ArgumentNullException(nameof(paintControl));
            this.PaintControl.SizeChanged += PaintControl_SizeChanged;
        }

        public void TimelineDataLoaded(TimelineData timelineData)
        {
            if (_timelineData != null)
                _timelineData.OnContentChanged -= TimelineData_OnContentChanged;
            _timelineData = timelineData ?? throw new ArgumentNullException(nameof(timelineData));
            _timelineData.OnContentChanged += TimelineData_OnContentChanged;
            TimelineDataLoadedInternal();
            PaintValues(_timelineData?.AllPoints);
        }

        protected abstract void TimelineDataLoadedInternal();

        protected abstract void PaintValues(IEnumerable<TimelinePoint>? timelinePoints);

        protected abstract bool IsChangedEventSuitableForThisPainter(TimelineDataChangedEventArgs changedEventArgs);

        private void TimelineData_OnContentChanged(object? sender, TimelineDataChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case TimelineDataChangedEventArgs.ChangeTypes.CopyNPaste:
                    PaintValues(_timelineData?.AllPoints);
                    break;
                case TimelineDataChangedEventArgs.ChangeTypes.ServoPointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.SoundPointChanged:
                case TimelineDataChangedEventArgs.ChangeTypes.NestedTimelinePointChanged:
                    if (this.IsChangedEventSuitableForThisPainter(e))
                        PaintValues(_timelineData?.AllPoints);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"{nameof(e.ChangeType)}:{e.ChangeType}");
            }
        }

        private void PaintControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintValues(_timelineData?.AllPoints);
        }

        private void OnViewContextChanged(object? sender, EventArgs e)
        {
            PaintValues(_timelineData?.AllPoints);
        }

        public void CleanUpValueControls()
        {
            // todo: only remove changed or not confirmed controls
            foreach (var control in _valueControls)
            {
                this.PaintControl.Children.Remove(control);
            }
            _valueControls.Clear();
        }

        public void Dispose()
        {
            CleanUpValueControls();
            this.PaintControl.SizeChanged -= PaintControl_SizeChanged;
            if (_timelineData != null)
            {
                _timelineData.OnContentChanged -= TimelineData_OnContentChanged;
                _timelineData = null;
            }
            if (_viewContext != null)
            {
                _viewContext.Changed += OnViewContextChanged;
            }
        }
    }
}
