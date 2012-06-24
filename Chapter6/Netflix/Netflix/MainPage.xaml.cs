using System;
using System.Data.Services.Client;
using System.Linq;
using Netflix.NetflixService;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Netflix
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();            
        }        

        private DataServiceCollection<Title> _collection; 

        private void LoadTitles()
        {
            var netflix =
                new NetflixCatalog(
                    new Uri(
                        "http://odata.netflix.com/Catalog/",
                        UriKind.Absolute));
            _collection = new DataServiceCollection<Title>(netflix);
            TitleGrid.ItemsSource = _collection;
            _collection.LoadCompleted += Collection_LoadCompleted;

            var query = (from t in netflix.Titles
                            where t.Name.StartsWith("Y")
                            orderby t.Rating descending
                            select t).Take(100);
            
            _collection.LoadAsync(query);
        }

        async void Collection_LoadCompleted(object sender, LoadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (_collection.Continuation != null)
                {
                    _collection.LoadNextPartialSetAsync();                    
                }
                TitleGrid.UpdateLayout();
            }
            else
            {
                var dialog = new MessageDialog(e.Error.Message);
                await dialog.ShowAsync();

            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_collection == null)
            {
                LoadTitles();
            }
        }
    }
}
