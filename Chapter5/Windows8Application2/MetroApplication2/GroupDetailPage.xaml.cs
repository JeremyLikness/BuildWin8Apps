using System;
using System.Collections.Generic;
using Windows8Application2.Data;
using Windows.UI.Xaml.Controls;

// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229

namespace Windows8Application2
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class GroupDetailPage
    {
        public GroupDetailPage()
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
            var group = App.Instance.DataSource.GetGroup((String) navigationParameter);
            DefaultViewModel["Group"] = group;
            DefaultViewModel["Items"] = group.Items;
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var itemId = ((SampleDataItem) e.ClickedItem).UniqueId;
            Frame.Navigate(typeof (ItemDetailPage), itemId);
        }

        private void ItemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            SampleDataItem selected = null;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                selected = e.AddedItems[0] as SampleDataItem;
            }
            App.Instance.CurrentItem = selected;
            AppBarCommands.SelectionChanged();
        }
    }
}