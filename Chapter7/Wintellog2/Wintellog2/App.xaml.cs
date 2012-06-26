using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Networking.PushNotifications;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Wintellog2.Common;
using Wintellog2.DataModel;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Wintellog2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            RegisterChannel();
        }

        public PushNotificationChannel Channel { get; private set; }
        public string ChannelError { get; private set; }

        private async void RegisterChannel()
        {
            try
            {
                Channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            }
            catch (Exception ex)
            {
                ChannelError = ex.Message;
            }
        }

        public static App Instance
        {
            get { return ((App)Current); }
        }

        public BlogDataSource DataSource { get; private set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            DataSource = new BlogDataSource();

            var splashScreen = args.SplashScreen;
            var splash = new SplashPage(splashScreen, args);
            splashScreen.Dismissed += splash.DismissedEventHandler;
            Window.Current.Content = splash;
            Window.Current.Activate();
        }

        public async void PinToStart(object sender, string id, string shortName, string displayName, string args)
        {
            var logo = new Uri("ms-appx:///Assets/Logo.png");
            var smallLogo = new Uri("ms-appx:///Assets/SmallLogo.png");
            var wideLogo = new Uri("ms-appx:///Assets/WideLogo.png");
            var tile = new SecondaryTile(
                id, 
                shortName, 
                displayName, 
                args, 
                TileOptions.ShowNameOnLogo | TileOptions.ShowNameOnWideLogo,
                logo)
                           {
                               ForegroundText = ForegroundText.Dark, 
                               SmallLogo = smallLogo, 
                               WideLogo = wideLogo
                           };

            var element = sender as FrameworkElement;
            
            if (element == null) return;
            
            var buttonTransform = element.TransformToVisual(null);
            var point = buttonTransform.TransformPoint(new Point());
            var rect = new Rect(point, new Size(element.ActualWidth, element.ActualHeight));

            await tile.RequestCreateForSelectionAsync(rect, Windows.UI.Popups.Placement.Left);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }
    }
}