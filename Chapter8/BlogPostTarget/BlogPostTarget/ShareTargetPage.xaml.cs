using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

// The Share Target Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234241

namespace BlogPostTarget
{
    /// <summary>
    /// This page allows other applications to share content through this application.
    /// </summary>
    public sealed partial class ShareTargetPage
    {
        /// <summary>
        /// Provides a channel to communicate with Windows about the sharing operation.
        /// </summary>
        private ShareOperation _shareOperation;

        public ShareTargetPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when another application wants to share content through this application.
        /// </summary>
        /// <param name="args">Activation data used to coordinate the process with Windows.</param>
        public async void Activate(ShareTargetActivatedEventArgs args)
        {
            _shareOperation = args.ShareOperation;

            // Communicate metadata about the shared content through the view model
            DataPackagePropertySetView shareProperties = _shareOperation.Data.Properties;
            var thumbnailImage = new BitmapImage();
            DefaultViewModel["Title"] = shareProperties.Title;
            DefaultViewModel["Description"] = shareProperties.Description;
            DefaultViewModel["Image"] = thumbnailImage;
            DefaultViewModel["Sharing"] = false;
            DefaultViewModel["ShowImage"] = false;
            DefaultViewModel["Comment"] = String.Empty;
            DefaultViewModel["SupportsComment"] = true;
            Window.Current.Content = this;
            Window.Current.Activate();

            // Update the shared content's thumbnail image in the background
            if (shareProperties.Thumbnail != null)
            {
                var stream = await shareProperties.Thumbnail.OpenReadAsync();
                thumbnailImage.SetSource(stream);
                DefaultViewModel["ShowImage"] = true;
            }

            const string BLOG_POST = "http://schema.org/BlogPosting";
            if (!_shareOperation.Data.Contains(BLOG_POST)) return;

            var data = await _shareOperation.Data.GetDataAsync(BLOG_POST);

            if (data == null)
            {
                DefaultViewModel["ShowBlog"] = false;
                return;
            }

            DefaultViewModel["ShowBlog"] = true;
            DefaultViewModel["BlogPost"] = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType((string) data,
                new
                {
                    type = "http://shema.org/BlogPosting",
                    properties = new
                    {
                        description = string.Empty,
                        image = new Uri("http://schema.org/"),
                        name = string.Empty,
                        url = new Uri("http://schema.org/"),
                        audience = "Windows 8 Developers",
                        datePublished = DateTime.Now,
                        headline = string.Empty,
                        articleBody = string.Empty
                    }
                });
        }

        /// <summary>
        /// Invoked when the user clicks the Share button.
        /// </summary>
        /// <param name="sender">Instance of Button used to initiate sharing.</param>
        /// <param name="e">Event data describing how the button was clicked.</param>
        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DefaultViewModel["Sharing"] = true;
            _shareOperation.ReportStarted();
            _shareOperation.ReportCompleted();
        }
    }
}