using System;
using Panels.Data;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Panels
{
    public class ShapeConverter : IValueConverter 
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var item = value as SimpleItem;
            object retVal = null;
            if (item != null)
            {
                var color = Color.FromArgb(0xff, (byte)item.Red, (byte)item.Green, (byte)item.Blue);
                var brush = new SolidColorBrush(color);

                switch (item.Type)
                {
                    case ItemType.Circle:
                        var circle = new Ellipse
                                         {
                                             Width = 200, 
                                             Height = 200, 
                                             Fill = brush
                                         };
                        retVal = circle;
                        break;
                    case ItemType.Ellipse:
                        var ellipse = new Ellipse
                                          {
                                              Width = 300, 
                                              Height = 200, 
                                              Fill = brush
                                          };
                        retVal = ellipse;
                        break;
                    case ItemType.Square:
                        var square = new Rectangle
                                         {
                                             Width = 200, 
                                             Height = 200, 
                                             Fill = brush
                                         };
                        retVal = square;
                        break;
                    case ItemType.Rectangle:
                        var rect = new Rectangle
                                       {
                                           Width = 300, 
                                           Height = 200, 
                                           Fill = brush
                                       };
                        retVal = rect;
                        break;
                }
            }
            return retVal; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
