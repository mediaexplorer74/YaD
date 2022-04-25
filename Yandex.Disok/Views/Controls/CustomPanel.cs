using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Ya.D.Views.Controls
{
    public class CustomPanel : Panel
    {
        private int _columnCount;

        public bool Stretch
        {
            get { return (bool)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(bool), typeof(CustomPanel), new PropertyMetadata(true));

        public double ColumnWidth
        {
            get { return (double)GetValue(ColumnWidthProperty); }
            set { SetValue(ColumnWidthProperty, value); }
        }

        public static readonly DependencyProperty ColumnWidthProperty =
            DependencyProperty.Register("ColumnWidth", typeof(double), typeof(CustomPanel), new PropertyMetadata(200.0, (o, args) => (o as CustomPanel).InvalidateArrange()));

        protected override Size MeasureOverride(Size availableSize)
        {
            _columnCount = (int)Math.Floor(availableSize.Width / ColumnWidth);
            var columnHeights = new double[_columnCount];
            foreach (var child in Children)
            {
                var columnIndex = Array.IndexOf(columnHeights, columnHeights.Min());
                (child as FrameworkElement).Measure(new Size(ColumnWidth, availableSize.Height));
                var elementSize = (child as FrameworkElement).DesiredSize;
                columnHeights[columnIndex] += elementSize.Height;
            }

            return new Size(availableSize.Width, columnHeights.Max());
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var columnWidth = finalSize.Width / _columnCount;
            var columnHeights = new double[_columnCount];

            foreach (var child in Children)
            {
                var columnIndex = Array.IndexOf(columnHeights, columnHeights.Min());
                var bounds = new Rect(
                        new Point(ColumnWidth * columnIndex, columnHeights[columnIndex]),
                        !Stretch ? (child as FrameworkElement).DesiredSize : new Size(ColumnWidth, (child as FrameworkElement).DesiredSize.Height)
                    );
                (child as FrameworkElement).Arrange(bounds);
                columnHeights[columnIndex] += (child as FrameworkElement).DesiredSize.Height;
            }

            return base.ArrangeOverride(finalSize);
        }
    }
}
