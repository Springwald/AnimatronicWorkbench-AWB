// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2024 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

using AwbStudio.TimelineEditing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AwbStudio.TimelineValuePainters
{
    class GridTimePainter: IDisposable
    {
        private readonly Brush _gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
        private readonly Grid _opticalGrid;
        private readonly TimelineViewContext _viewContext;

        public GridTimePainter(Grid opticalGrid, TimelineViewContext viewContext)
        {
            _opticalGrid = opticalGrid;
            _viewContext = viewContext;

            _opticalGrid.SizeChanged += OnSizeChanged;

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
            const int STEP = 1000;
            for (int ms = 0; ms < duration; ms += STEP)
            {
                var x = _viewContext.GetXPos(ms);
                if (x > 0 && x < width)
                {
                    _opticalGrid.Children.Add(new Line { X1 = x, X2 = x, Y1 = _paintMarginTopBottom, Y2 = height - _paintMarginTopBottom, Stroke = _gridLineBrush });
                    _opticalGrid.Children.Add(new Label { Content = ((ms) / STEP).ToString(), BorderThickness = new Thickness(left: x, top: height - 30, right: 0, bottom: 0) });
                }
            }
        }

        public void Dispose()
        {
            _opticalGrid.SizeChanged -= OnSizeChanged;
        }
    }
}
