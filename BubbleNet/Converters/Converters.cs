using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BubbleNet.Converters
{
    /// <summary>
    /// Converts a boolean value to Visibility, with inverse logic.
    /// True -> Collapsed, False -> Visible
    /// Used for hiding elements when a condition is true.
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If value is true, collapse the element; if false, show it
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Convert visibility back to boolean (inverse logic)
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts a null/non-null value to Visibility.
    /// Not null -> Visible, Null -> Collapsed
    /// Used for conditionally showing elements based on data presence.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Show element if value is not null
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // One-way converter - ConvertBack not implemented
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an integer to Visibility based on whether it equals zero.
    /// Zero -> Visible (show empty state), Non-zero -> Collapsed (hide empty state)
    /// Used for showing "empty list" messages when collections are empty.
    /// </summary>
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Show element only if value is zero (empty state indicator)
            if (value is int intValue)
            {
                return intValue == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // One-way converter - ConvertBack not implemented
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to Visibility with optional inverse via parameter.
    /// Parameter = "Inverse" -> inverts the logic.
    /// Useful when you need both normal and inverse behavior in one converter.
    /// </summary>
    public class BoolToVisibilityConverterWithInverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            bool inverse = parameter?.ToString() == "Inverse";

            // Apply inverse logic if parameter is "Inverse"
            if (inverse)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // One-way converter - ConvertBack not implemented
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a TransferType enum to its display name string.
    /// Used in the redesigned Send menu for showing the selected payload type.
    /// </summary>
    public class TransferTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BubbleNet.Models.TransferType transferType)
            {
                return transferType switch
                {
                    BubbleNet.Models.TransferType.File => "File",
                    BubbleNet.Models.TransferType.Text => "Text",
                    BubbleNet.Models.TransferType.Link => "Link",
                    BubbleNet.Models.TransferType.Screenshot => "Screenshot",
                    BubbleNet.Models.TransferType.Image => "Image",
                    BubbleNet.Models.TransferType.StreamShare => "Stream Share",
                    BubbleNet.Models.TransferType.FileBubble => "FileBubble",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a TransferType enum to its emoji icon.
    /// Used in the redesigned Send menu for showing payload type icons.
    /// </summary>
    public class TransferTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BubbleNet.Models.TransferType transferType)
            {
                return transferType switch
                {
                    BubbleNet.Models.TransferType.File => "📁",
                    BubbleNet.Models.TransferType.Text => "📝",
                    BubbleNet.Models.TransferType.Link => "🔗",
                    BubbleNet.Models.TransferType.Screenshot => "📸",
                    BubbleNet.Models.TransferType.Image => "🖼️",
                    BubbleNet.Models.TransferType.StreamShare => "📺",
                    BubbleNet.Models.TransferType.FileBubble => "📦",
                    _ => "📦"
                };
            }
            return "📦";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to its inverse.
    /// True -> False, False -> True
    /// Useful for enabling/disabling UI elements based on inverse conditions.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts enum value equality to boolean for RadioButton binding.
    /// Compares the bound value to the parameter and returns true if equal.
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter != null)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converts TransferType enum to Visibility based on parameter matching.
    /// Shows element (Visible) when SelectedPayloadType equals parameter, hides (Collapsed) otherwise.
    /// Used in the dynamic input field section of the Send menu.
    /// </summary>
    public class TransferTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            // Compare enum value to string parameter
            string valueStr = value.ToString() ?? "";
            string paramStr = parameter.ToString() ?? "";

            return valueStr.Equals(paramStr, StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
