using System;
using System.Linq;
using NotificationsExtensions.BadgeContent;
using NotificationsExtensions.TileContent;
using NotificationsExtensions.ToastContent;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Wintellog2.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Wintellog2
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
                && (bool)ApplicationData.Current.LocalSettings.Values["Initialized"]
                                    ? "Loading blogs..."
                                    : "Initializing for first use: this may take several minutes...";

            await App.Instance.DataSource.LoadGroups();

            foreach (var group in App.Instance.DataSource.GroupList)
            {
                Progress.IsActive = true;
                ProgressText.Text = "Loading " + group.Title;
                await App.Instance.DataSource.LoadAllItems(group);
            }

            ApplicationData.Current.LocalSettings.Values["Initialized"] = true;

            // Create a Frame to act as the navigation context and associate it with
            // a SuspensionManager key
            var rootFrame = new Frame();
            SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

            var list = App.Instance.DataSource.GroupList;

            var totalNew = list.Sum(g => g.NewItemCount);

            if (totalNew > 99)
            {
                totalNew = 99;
            }

            if (totalNew > 0)
            {
                var badgeContent = new BadgeNumericNotificationContent((uint) totalNew);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badgeContent.CreateNotification());
            }
            else
            {
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
            }

            // load most recent 5 items then order from oldest to newest

            var query = from i in
                            (from g in list
                             from i in g.Items
                             orderby i.PostDate descending
                             select i).Take(5)
                        orderby i.PostDate
                        select i;

            var x = 0;

            var notificationCount = totalNew;

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            foreach (var item in query)
            {
                var squareTile = new TileSquarePeekImageAndText04();
                squareTile.TextBodyWrap.Text = item.Title;
                squareTile.Image.Alt = item.Title;
                squareTile.Image.Src = item.DefaultImageUri.ToString();

                var wideTile = new TileWideSmallImageAndText03
                                   {
                                       SquareContent = squareTile
                                   };
                wideTile.Image.Alt = item.Title;
                wideTile.Image.Src = item.DefaultImageUri.ToString();
                wideTile.TextBodyWrap.Text = item.Title;

                var notification = wideTile.CreateNotification();
                notification.Tag = string.Format("Item {0}", x++);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

                // send a notification
                if (notificationCount-- <= 0) continue;

                var notifier = ToastNotificationManager.CreateToastNotifier();

                if (notifier.Setting == NotificationSetting.Enabled)
                {
                    // send a toast notification
                    var notificationTemplate = ToastContentFactory.CreateToastImageAndText02();
                    notificationTemplate.Launch = string.Format("Item={0}", item.Id);
                    notificationTemplate.TextHeading.Text = item.Group.Title;
                    notificationTemplate.TextBodyWrap.Text = item.Title;
                    notificationTemplate.Image.Src = item.DefaultImageUri.ToString();
                    notificationTemplate.Image.Alt = item.Title;

                    var toast = notificationTemplate.CreateNotification();
                    var date = DateTimeOffset.Now.AddSeconds(30);
                    var scheduledToast = new ScheduledToastNotification(toast.Content, date);
                    notifier.AddToSchedule(scheduledToast);
                }
            }

            if (_activationArgs.Arguments.StartsWith("Group"))
            {
                var group = _activationArgs.Arguments.Split('=');
                rootFrame.Navigate(typeof (GroupDetailPage), group[1]);                
            }
            else if (_activationArgs.Arguments.StartsWith("Item"))
            {
                var item = _activationArgs.Arguments.Split('=');
                rootFrame.Navigate(typeof (ItemDetailPage), item[1]);
            }
            else if (_activationArgs.PreviousExecutionState == ApplicationExecutionState.Terminated)
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