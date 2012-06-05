using System.Diagnostics;
using Windows.UI.Xaml;

namespace DependencyProperties
{
    public class MyDependencyObject : DependencyObject 
    {
        public static readonly DependencyProperty
            MyNumberProperty = DependencyProperty.Register(
            "MyNumber", typeof(int), typeof(MyDependencyObject), 
            new PropertyMetadata(2, PropertyChange));

        private static void PropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("Property changed: {0}", e.NewValue);
        }

        public int MyNumber
        {
            get { return (int)GetValue(MyNumberProperty); }
            set { SetValue(MyNumberProperty, value); }
        }
    }


}
