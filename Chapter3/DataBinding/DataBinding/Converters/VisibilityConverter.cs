using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace DataBinding.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || !(value is bool))
                return Visibility.Visible;

            var parm = parameter ?? false;
            bool flip;
            Boolean.TryParse(parm.ToString(), out flip);
            var visible = flip ? (bool)value : !((bool)value);
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
