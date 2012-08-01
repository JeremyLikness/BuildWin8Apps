using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WintellogMvvm
{
    public sealed partial class WintellogSettings
    {
        public WintellogSettings()
        {
            InitializeComponent();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var message = App.Instance.Channel != null
                                    ? "The channel has been copied to the clipboard."
                                    : string.Format("Error: {0}", App.Instance.ChannelError);

            var dataPackage = new DataPackage();

            if (App.Instance.Channel != null)
            {
                dataPackage.SetText(App.Instance.Channel.Uri);
            }
            Clipboard.SetContent(dataPackage);

            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }
    }
}
