using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wintellog3.Common;
using Wintellog3.DataModel;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Wintellog3
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage
    {
        private string _selection = string.Empty;

        public ItemDetailPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            App.Instance.Share = Share; 

            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            var item = App.Instance.DataSource.GetItem((String) navigationParameter);
            DefaultViewModel["Group"] = item.Group;
            DefaultViewModel["Items"] = item.Group.Items;
            flipView.SelectedItem = item;
        }

        private async void Share(DataTransferManager dataTransferManager, DataRequestedEventArgs dataRequestedEventArgs)
        {
            var item = flipView.SelectedItem as BlogItem;
            
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_selection))
            {
                dataRequestedEventArgs.Request.Data.Properties.Title = item.Title;            
                dataRequestedEventArgs.Request.Data.Properties.Description =
                    string.Format("Blog post from {0}.", item.Group.Title);
                dataRequestedEventArgs.Request.Data.SetText(item.Description);
                dataRequestedEventArgs.Request.Data.SetUri(item.PageUri);
    
                if (item.DefaultImageUri != null)
                {
                    dataRequestedEventArgs.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(item.DefaultImageUri));
                }

                var data = await CustomData(item);
                dataRequestedEventArgs.Request.Data.SetData("http://schema.org/BlogPosting", data);
            }
            else
            {
                dataRequestedEventArgs.Request.Data.Properties.Title = string.Format("Excerpt from {0}", item.Title);
                dataRequestedEventArgs.Request.Data.Properties.Description =
                    string.Format("An excerpt from the {0} blog at {1}.", item.Group.Title, item.Group.RssUri);
                dataRequestedEventArgs.Request.Data.SetText(string.Format("{0}\r\n\r\n{1}",
                    _selection,
                    item.PageUri));                
            }            
        }

        private static async Task<string> CustomData(BlogItem item)
        {
            var schema = new
                {
                    type = "http://shema.org/BlogPosting",
                    properties = new
                        {
                            description = string.Format(
                            "Blog post from {0}.", 
                            item.Group.Title),
                            image = item.DefaultImageUri,
                            name = item.Title,
                            url = item.PageUri,
                            audience = "Windows 8 Developers",
                            datePublished = item.PostDate,
                            headline = item.Title,
                            articleBody = item.Description
                        }
                };
            return await JsonConvert.SerializeObjectAsync(schema);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (BlogItem) flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.Id;
        }

        private void Home_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (GroupedItemsPage), "GroupList");
        }

        private async void Open_Click_1(object sender, RoutedEventArgs e)
        {
            var item = flipView.SelectedItem as BlogItem;

            // Launch the retrieved file
            if (item != null)
            {
                await Windows.System.Launcher.LaunchUriAsync(item.PageUri);
            }
        }

        private void Image_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (ImageDetailsPage), ((BlogItem) flipView.SelectedItem).Id);
        }

        private void Pin_Click_1(object sender, RoutedEventArgs e)
        {
            var item = flipView.SelectedItem as BlogItem;
            if (item != null)
            {
                var title = string.Format("Post: {0}", item.Title);
                App.Instance.PinToStart(sender,
                                        string.Format("Wintellog.{0}", item.Id.GetHashCode()),
                                        title,
                                        title,
                                        string.Format("Item={0}", item.Id));
            }
        }

        private void RichTextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {
            var richTextControl = sender as RichTextBlock;
            _selection = richTextControl != null ? 
                richTextControl.SelectedText : string.Empty;
        }
    }
}