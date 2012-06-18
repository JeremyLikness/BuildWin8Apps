using System;
using MetroApplication.Common;
using MetroApplication.Data;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace MetroApplication
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

        public Type NavigatedPage { get; private set; }

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
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            DataSource = new SampleDataSource();
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
            rootFrame.Navigating += RootFrame_Navigating;
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                await SuspensionManager.RestoreAsync();
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof (GroupedItemsPage), "ItemGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();

            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
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

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            NavigatedPage = e.SourcePageType;
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