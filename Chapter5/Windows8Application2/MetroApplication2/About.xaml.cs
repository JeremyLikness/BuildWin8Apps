using System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Windows8Application2
{
    public sealed partial class About
    {
        public About()
        {
            this.InitializeComponent();
        }

        private async void HyperlinkButton_Click_1(object sender, 
            RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
                new Uri("http://csharperimage.jeremylikness.com/"));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (Parent is Popup)
            {
                ((Popup)Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }
    }
}
