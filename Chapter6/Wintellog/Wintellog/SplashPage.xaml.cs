using System;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Wintellog.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Wintellog
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SplashPage
    {
        private readonly SplashScreen _splash;
        private readonly LaunchActivatedEventArgs _activationArgs;

        public SplashPage(SplashScreen splash, LaunchActivatedEventArgs activationArgs)
        {
            _splash = splash;
            _activationArgs = activationArgs;

            Loaded += ExtendedSplashScreen_Loaded;
            Window.Current.SizeChanged += Current_SizeChanged;

            InitializeComponent();

            PositionElements();
        }

        private void PositionElements()
        {
            SplashImage.SetValue(Canvas.LeftProperty, _splash.ImageLocation.X);
            SplashImage.SetValue(Canvas.TopProperty, _splash.ImageLocation.Y);
            SplashImage.Height = _splash.ImageLocation.Height;
            SplashImage.Width = _splash.ImageLocation.Width;

            var topWithBuffer = _splash.ImageLocation.Y + _splash.ImageLocation.Height - 50;
            var textTop = topWithBuffer + 30;
            var left = _splash.ImageLocation.X + 40;

            Progress.SetValue(Canvas.TopProperty, topWithBuffer);
            Progress.SetValue(Canvas.LeftProperty, left);
            Progress.IsActive = true;            

            ProgressText.SetValue(Canvas.TopProperty, textTop - 15);
            ProgressText.SetValue(Canvas.LeftProperty, left + 90);

            ProgressText.Text = "Initializing...";
        }

        private async void ExtendedSplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressText.Text = ApplicationData.Current.LocalSettings.Values.ContainsKey("Initialized")
                                    ? "Loading blogs..."
                                    : "Initializing for first use: this may take several minutes...";

            await ((App) Application.Current).DataSource.LoadGroups();

            foreach (var group in ((App) Application.Current).DataSource.GroupList)
            {
                Progress.IsActive = true;
                ProgressText.Text = "Loading " + group.Title;
                await ((App) Application.Current).DataSource.LoadAllItems(group);
            }

            ApplicationData.Current.LocalSettings.Values["Initialized"] = true;

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

            if (_activationArgs.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                // Restore the saved session state only when appropriate
                await SuspensionManager.RestoreAsync();
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof (GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (null != _splash)
            {
                // Re-position the extended splash screen image due to window resize event.
                PositionElements();
            }
        }

        internal void DismissedEventHandler(SplashScreen sender, object e)
        {
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}