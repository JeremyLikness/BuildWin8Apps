using System.Linq;
using Panels.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Panels
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            var list = new SimpleItemList();
            CVS.Source = list;
            CVSGrouped.Source = from item in list
                                group item by item.Type into g
                                orderby g.Key
                                select g;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Reset();
            FlipButton.IsEnabled = false;
            FlipView.Visibility = Visibility.Visible;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Reset();
            GridButton.IsEnabled = false;
            GridView.Visibility = Visibility.Visible;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Reset();
            ListButton.IsEnabled = false;
            List.Visibility = Visibility.Visible;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Reset();
            ListBoxButton.IsEnabled = false;
            ListBox.Visibility = Visibility.Visible;
        }

        private void Reset()
        {
            GridButton.IsEnabled = true;
            FlipButton.IsEnabled = true;
            ListButton.IsEnabled = true;
            ListBoxButton.IsEnabled = true;
            GridView.Visibility = Visibility.Collapsed;
            FlipView.Visibility = Visibility.Collapsed;
            List.Visibility = Visibility.Collapsed;
            ListBox.Visibility = Visibility.Collapsed;
        }
    }
}
