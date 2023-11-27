using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Controls.Converters
{
    public class DiameterAndThicknessToStrokeDashArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2
                || !double.TryParse(values[0].ToString(), out double diameter)
                || !double.TryParse(values[1].ToString(), out double thickness))
            {
                return new DoubleCollection();
            }

            double circunference = Math.PI * diameter;
            
            double lineLength = circunference * 0.75;
            double gapLength = circunference - lineLength;

            return new DoubleCollection(new[] { lineLength / thickness, gapLength / thickness });
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
