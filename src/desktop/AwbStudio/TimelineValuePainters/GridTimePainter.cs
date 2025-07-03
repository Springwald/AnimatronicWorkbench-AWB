// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using AwbStudio.TimelineEditing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace AwbStudio.TimelineValuePainters
{
    class GridTimePainter : IDisposable
    {

        private readonly Grid _opticalGrid;
        private readonly TimelineViewContext _viewContext;
        private readonly TimelineColors _timelineColors;

        public GridTimePainter(Grid opticalGrid, TimelineViewContext viewContext)
        {
            _opticalGrid = opticalGrid;
            _viewContext = viewContext;

            _opticalGrid.SizeChanged += OnSizeChanged;

            _timelineColors  = new TimelineColors();

            PaintGrid();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            PaintGrid();
        }

        private void PaintGrid()
        {
            const double _paintMarginTopBottom = 30;
            double height = _opticalGrid.ActualHeight;
            double width = _opticalGrid.ActualWidth;

            if (height < _paintMarginTopBottom * 2 || width < 100) return;

            // update the data optical grid lines
            _opticalGrid.Children.Clear();
            var duration = _viewContext.DurationMs;
            const int STEP = 500;
            for (int ms = 0; ms < duration; ms += STEP)
            {
                var x = _viewContext.GetXPos(ms);
                if (x > 0 && x < width)
                {
                    var color = ms % 1000 == 0 ? _timelineColors.GridLineVertical1000msBrush : _timelineColors.GridLineVertical500msBrush;
                    _opticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = color });
                    _opticalGrid.Children.Add(new Label { Content = (ms / 1000d).ToString("0.0"), BorderThickness = new Thickness(left: x - 15, top: height - 30, right: 0, bottom: 0) });
                }
            }
        }

        public void Dispose()
        {
            _opticalGrid.SizeChanged -= OnSizeChanged;
        }
    }
}
