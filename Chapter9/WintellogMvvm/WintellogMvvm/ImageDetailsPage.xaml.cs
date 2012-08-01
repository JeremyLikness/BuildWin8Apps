using System;
using System.Collections.Generic;
using PortableWintellog.Data;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WintellogMvvm.Common;
using WintellogMvvm.DataModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WintellogMvvm
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageDetailsPage
    {
        private string _itemId = string.Empty;

        public ImageDetailsPage()
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

            var item = App.Instance.DataSource.GetItem((String)navigationParameter);
            DefaultViewModel["Group"] = item.Group;
            DefaultViewModel["Items"] = item.Group.Items;
            DefaultViewModel["CurrentItem"] = item;
            LayoutRoot.DataContext = item; // binding above does not update automatically
            flipView.SelectedItem = item.DefaultImageUri;
            _itemId = item.Id;
        }

        private void Share(DataTransferManager dataTransferManager, DataRequestedEventArgs dataRequestedEventArgs)
        {
            var item = DefaultViewModel["CurrentItem"] as BlogItem;
            var image = (Uri) flipView.SelectedItem;

            if (item == null)
            {
                return;
            }

            dataRequestedEventArgs.Request.Data.Properties.Title = item.Title;            
            dataRequestedEventArgs.Request.Data.Properties.Description = string.Format("Image from blog post {0}.", item.Title);
            dataRequestedEventArgs.Request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(image));            
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["SelectedItem"] = _itemId;
        }
                        
        private void Home_Click_1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (GroupedItemsPage), "Groups");                
        }
    }
}
