using System;
using Windows.UI.Xaml.Data;

namespace DataBinding.Converters
{
    public class TextConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = value ?? true;
            bool truth;
            Boolean.TryParse(val.ToString(), out truth);
            return truth ?
                "The truth will set you free." :
                "Is falsehood so established?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
