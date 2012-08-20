using System;
using System.Diagnostics;
using Windows8Application2.Common;
using Windows8Application2.Data;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Windows8Application2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App
    {
        private SampleDataItem _item;

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        public static App Instance
        {
            get { return ((App) Current); }
        }

        public Type NavigatedPage { get; set; }

        public SampleDataSource DataSource { get; private set; }

        public SampleDataGroup CurrentGroup { get; set; }

        public SampleDataItem CurrentItem
        {
            get { return _item; }

            set
            {
                if (value != null)
                {
                    SampleDataItem item = DataSource.GetItem(value.UniqueId);
                    CurrentGroup = item.Group;
                }
                _item = value;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DataSource = new SampleDataSource();
            
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;

            var splashScreen = args.SplashScreen;

            var eSplash = new ExtendedSplashScreen(splashScreen, false, args);
            splashScreen.Dismissed += eSplash.DismissedEventHandler;
            Window.Current.Content = eSplash;
            Window.Current.Activate();                       
        }

        static void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var about = new SettingsCommand("about", "About", handler =>
                                                                  {
                                                                      var settings = new SettingsFlyout();
                                                                      settings.ShowFlyout(new About());
                                                                  });
            args.Request.ApplicationCommands.Add(about);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private static async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Debug.WriteLine(string.Format("{0} remaining",
                e.SuspendingOperation.Deadline - DateTime.Now));
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            if (DataSource == null)
            {
                DataSource = new SampleDataSource();
                Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted +=
                (sender, queryArgs) => SearchResultsPage.Activate(queryArgs.QueryText, args.PreviousExecutionState);
                var previousContent = Window.Current.Content;
                var frame = previousContent as Frame;

                if (frame == null)
                {
                    frame = new Frame();
                    Window.Current.Content = frame;
                }

                frame.Navigate(typeof(SearchResultsPage), new Tuple<String, UIElement>(args.QueryText, previousContent));
                Window.Current.Activate();
            }
            else
            {
                SearchResultsPage.Activate(args.QueryText, args.PreviousExecutionState);
            }
        }
    }
}