﻿using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wintellog3.DataModel;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Wintellog3
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedItemsPage
    {
        public GroupedItemsPage()
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
            App.Instance.Share = null;
            DefaultViewModel["Groups"] = App.Instance.DataSource.GroupList;
            groupGridView.ItemsSource = groupedItemsViewSource.View.CollectionGroups;
        }

        /// <summary>
        /// Invoked when a group header is clicked.
        /// </summary>
        /// <param name="sender">The Button used as a group header for the selected group.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        private void Header_Click(object sender, RoutedEventArgs e)
        {
            // Determine what group the Button instance represents
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;
            var group = frameworkElement.DataContext;

            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            Frame.Navigate(typeof (GroupDetailPage), ((BlogGroup) group).Id);
        }

        /// <summary>
        /// Invoked when an item within a group is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((BlogItem) e.ClickedItem).Id;
            Frame.Navigate(typeof (ItemDetailPage), itemId);
        }        
    }
}