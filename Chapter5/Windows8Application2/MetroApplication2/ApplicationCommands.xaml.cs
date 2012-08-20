using System;
using Windows8Application2.Data;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Windows8Application2
{
    public sealed partial class ApplicationCommands
    {
        public ApplicationCommands()
        {
            InitializeComponent();

            if (DesignMode.DesignModeEnabled)
                return;

            SelectionChanged();

            if (App.NavigatedPage == typeof (ItemDetailPage))
            {
                Add.Visibility = Visibility.Collapsed;
            }
            else if (App.NavigatedPage == typeof (GroupedItemsPage))
            {
                Home.IsEnabled = false;
            }
        }

        private App App
        {
            get { return (App) Application.Current; }
        }

        public void SelectionChanged()
        {
            SampleDataItem selected = App.CurrentItem;
            Delete.Visibility = selected == null
                                    ? Visibility.Collapsed
                                    : Visibility.Visible;

            SampleDataGroup group = App.CurrentGroup;
            Add.IsEnabled = group != null;
        }

        private void Home_Click_1(object sender, RoutedEventArgs e)
        {
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                frame.Navigate(typeof (GroupedItemsPage), App.DataSource.ItemGroups);
            }
        }

        private void Add_Click_1(object sender, RoutedEventArgs e)
        {
            App.DataSource.AddItem(App.CurrentGroup);
        }

        private async void Delete_Click_1(object sender, RoutedEventArgs e)
        {
            var msg = new MessageDialog("Confirm Delete",
                                        string.Format("Are you sure you wish to delete the item \"{0}\"",
                                                      App.CurrentItem.Title));
            msg.Commands.Add(new UICommand("OK",
                                           args =>
                                               {
                                                   App.CurrentGroup.Items.Remove(App.CurrentItem);
                                                   if (App.NavigatedPage == typeof (ItemDetailPage))
                                                   {
                                                       ((Frame) Window.Current.Content).GoBack();
                                                   }
                                               }));
            msg.Commands.Add(new UICommand("Cancel"));
            await msg.ShowAsync();
        }
    }
}