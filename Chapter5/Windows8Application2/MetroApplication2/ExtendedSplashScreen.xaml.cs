using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows8Application2.Common;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Windows8Application2
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ExtendedSplashScreen : Page
    {
        private SplashScreen _splash;
        private bool _dismissed = false;
        LaunchActivatedEventArgs _activationArgs = null;

        public ExtendedSplashScreen(SplashScreen splash, bool dismissed, LaunchActivatedEventArgs activationArgs)
        {
            _splash = splash;
            _dismissed = dismissed;
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

            var topWithBuffer = _splash.ImageLocation.Y + _splash.ImageLocation.Height + 20;
            var middle = _splash.ImageLocation.X + (_splash.ImageLocation.Width / 2) - 30;
            
            Progress.SetValue(Canvas.TopProperty, topWithBuffer);
            Progress.SetValue(Canvas.LeftProperty, middle);
        }

        async  void ExtendedSplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            await ((App)Application.Current).DataSource.Initialize();
            await Task.Run(() =>
                               {
                                   for (var x = 0; x < 2000; x++)
                                   {
                                       Debug.WriteLine(x);
                                   }
                               });

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
            rootFrame.Navigating += rootFrame_Navigating;
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
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "ItemGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();            
        }

        void rootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            ((App)Application.Current).NavigatedPage = e.SourcePageType;
        }
        
        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (null != _splash)
            {
                // Re-position the extended splash screen image due to window resize event.
                PositionElements();
            }
        }

        internal void DismissedEventHandler(SplashScreen sender, object e)
        {
            _dismissed = true;
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
