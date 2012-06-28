using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NotificationsExtensions.BadgeContent;
using NotificationsExtensions.TileContent;
using NotificationsExtensions.ToastContent;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PushNotificationTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private Uri _channel;
        private string _accessCode;

        public MainPage()
        {
            InitializeComponent();
            GetAccessCode();
        }

        private async void GetAccessCode()
        {
            var auth = new Authentication();
            _accessCode = await auth.GetAccessToken();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async Task<bool> ValidateUri()
        {
            if (!Uri.TryCreate(ChannelUri.Text, UriKind.Absolute, out _channel))
            {
                var dialog = new MessageDialog("The URI is invalid.");
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (await ValidateUri())
            {
                var badge = new BadgeNumericNotificationContent((uint)BadgeValue.Value);
                await PostToCloud(badge.CreateNotification().Content, "wns/badge");
            }
        }        

        private string _title, _text;

        private void SetTitleAndText()
        {
            _title = string.IsNullOrEmpty(TitleText.Text.Trim()) ? 
                "Test Title" : TitleText.Text.Trim();
            _text = string.IsNullOrEmpty(BodyText.Text.Trim()) ? 
                "Test Body" : BodyText.Text.Trim();
        }

        private async void TileButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (await ValidateUri())
            {
                SetTitleAndText();
                var tile = TileContentFactory.CreateTileSquareText02();
                tile.TextHeading.Text = _title;
                tile.TextBodyWrap.Text = _text;

                var bigTile = TileContentFactory.CreateTileWideText01();
                bigTile.SquareContent = tile;
                bigTile.TextHeading.Text = _title;
                bigTile.TextBody1.Text = _text;

                await PostToCloud(bigTile.CreateNotification().Content); 
            }
        }

        private async void NotificationButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (await ValidateUri())
            {
                SetTitleAndText();
                var notification = ToastContentFactory.CreateToastText02();
                notification.TextHeading.Text = _title;
                notification.TextBodyWrap.Text = _text;
                await PostToCloud(notification.CreateNotification().Content, "wns/toast"); 
            }
        }

        private async Task PostToCloud(XmlDocument xml, string type = "wns/tile")
        {
            var uri = _channel.ToString();
            string message;

            try
            {
                var content = xml.GetXml();
                var requestMsg = new HttpRequestMessage(HttpMethod.Post, uri)
                                     {
                                         Content = new ByteArrayContent(
                                             Encoding.UTF8.GetBytes(content))
                                     };

                requestMsg.Content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
                requestMsg.Headers.Add("X-WNS-Type", type);
                requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessCode);

                var responseMsg = await new HttpClient().SendAsync(requestMsg);
                message = string.Format("{0}: {1}", responseMsg.StatusCode, await responseMsg.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }
    }
}
