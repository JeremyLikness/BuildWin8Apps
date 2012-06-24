using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WeatherService
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        private const string ZIPCODE_ERROR = "ZipCode must be a valid integer between 0 and 99999";

        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // really basic zip code validtion
            int zip; 
            
            if (!int.TryParse(ZipCode.Text, out zip))
            {
                await ShowDialog(
                    ZIPCODE_ERROR);
                return;
            }

            if (zip < 0 || zip > 99999)
            {
                await ShowDialog(ZIPCODE_ERROR);
                return;
            }

            var client = new WeatherWebService.WeatherSoapClient();
            var result = await client.GetCityForecastByZIPAsync(zip.ToString());
            
            if (!result.Success)
            {
                await ShowDialog("There was an error accessing the weather service.");
                return;
            }

            ResultsGrid.DataContext = result;                        
        }

        private static async Task ShowDialog(string message)
        {
            var dialog = new MessageDialog(message);
            await dialog.ShowAsync();
        }
    }
}
