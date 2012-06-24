using System;
using Windows.UI.Xaml.Data;

namespace WeatherService.Common
{
    public class DateConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((DateTime) value).ToString("D");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
