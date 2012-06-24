using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace WeatherService.Common
{
    public class ImageConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var filename =
                string.Format("ms-appx:///Assets/{0}.gif",
                                ((string) value).Replace(" ", string.Empty).ToLower());
            return new BitmapImage(new Uri(filename, UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
