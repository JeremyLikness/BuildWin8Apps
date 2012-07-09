using System;
using System.Linq;
using System.Text.RegularExpressions;
using NotificationsExtensions.BadgeContent;
using NotificationsExtensions.TileContent;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Wintellog3.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Wintellog3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SplashPage
    {
        private readonly SplashScreen _splash;
        private readonly LaunchActivatedEventArgs _activationArgs;
        private readonly SearchActivatedEventArgs _searchArgs;

        public SplashPage(SplashScreen splash, object args)
        {            
            _splash = splash;

            if (args is LaunchActivatedEventArgs)
            {
                _activationArgs = args as LaunchActivatedEventArgs;
            }
            else
            {
                _searchArgs = args as SearchActivatedEventArgs;
            }

            Loaded += ExtendedSplashScreen_Loaded;
            Window.Current.SizeChanged += Current_SizeChanged;

            InitializeComponent();

            PositionElements();
        }

        private void PositionElements()
        {
            var x = _splash == null ? 2 : _splash.ImageLocation.X;
            var y = _splash == null ? 2 : _splash.ImageLocation.Y;
            var height = _splash == null ? 480 : _splash.ImageLocation.Height;
            var width = _splash == null ? 640 : _splash.ImageLocation.Width; 

            SplashImage.SetValue(Canvas.LeftProperty, x);
            SplashImage.SetValue(Canvas.TopProperty, y);
            SplashImage.Height =  height;
            SplashImage.Width = width;

            var topWithBuffer = y + height - 50;
            var textTop = topWithBuffer + 30;
            var left = x + 40;

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
            }

            Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().SuggestionsRequested += SplashPage_SuggestionsRequested;

            App.Instance.Extended = true;

            if (_searchArgs != null)
            {
                SearchResultsPage.Activate(_searchArgs);
                return;
            }

            if (_activationArgs != null)
            {
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

        void SplashPage_SuggestionsRequested(Windows.ApplicationModel.Search.SearchPane sender, 
            Windows.ApplicationModel.Search.SearchPaneSuggestionsRequestedEventArgs args)
        {
            var query = args.QueryText.ToLower();

            if (query.Length < 3) return;

            var suggestions = (from g in App.Instance.DataSource.GroupList
                                from i in g.Items
                                from keywords in i.Title.Split(' ')
                                let keyword = Regex.Replace(
                                    keywords.ToLower(), @"[^\w\.@-]", "")
                                where i.Title.ToLower().Contains(query)
                                && keyword.StartsWith(query)
                                orderby keyword
                                select keyword).Distinct();

            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);
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