using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace PlasticQC.Helpers
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }

    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                var parts = options.Split('|');
                if (parts.Length >= 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string options)
            {
                var parts = options.Split(',');
                if (parts.Length >= 2)
                {
                    string colorName = boolValue ? parts[0] : parts[1];
                    return Microsoft.Maui.Graphics.Color.Parse(colorName);
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ItemNumberToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int itemNumber)
            {
                return itemNumber % 2 == 0 ? Colors.White : Color.Parse("#F0F0F0");
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToleranceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double standardValue && parameter is string paramString)
            {
                var parts = paramString.Split(',');
                if (parts.Length >= 2)
                {
                    string sign = parts[0]; // + or -
                    string tolerancePropertyName = parts[1];

                    // Get tolerance value using reflection
                    if (value is object sourceObject && tolerancePropertyName != null)
                    {
                        var property = sourceObject.GetType().GetProperty(tolerancePropertyName);
                        if (property != null)
                        {
                            var toleranceValue = (double)property.GetValue(sourceObject);

                            if (sign == "+")
                            {
                                return (standardValue + toleranceValue).ToString("F1");
                            }
                            else // "-"
                            {
                                return (standardValue - toleranceValue).ToString("F1");
                            }
                        }
                    }
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string thresholds)
            {
                var parts = thresholds.Split(',');
                if (parts.Length >= 2 && double.TryParse(parts[0], out double warningThreshold) &&
                    double.TryParse(parts[1], out double criticalThreshold))
                {
                    if (doubleValue < warningThreshold)
                        return Colors.Green;
                    else if (doubleValue < criticalThreshold)
                        return Colors.Orange;
                    else
                        return Colors.Red;
                }
            }
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}