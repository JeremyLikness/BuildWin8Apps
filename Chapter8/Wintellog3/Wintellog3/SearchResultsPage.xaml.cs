using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Wintellog3.Common;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace Wintellog3
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage
    {
        private UIElement _previousContent;
        private string _query;

        public SearchResultsPage()
        {
            InitializeComponent();            
        }

        /// <summary>
        /// Determines how best to support navigation back to the previous application state.
        /// </summary>
        public static void Activate(SearchActivatedEventArgs args)
        {
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            if (frame != null)
            {
                // If the app is already running and uses top-level frame navigation we can just
                // navigate to the search results
                frame.Navigate(typeof (SearchResultsPage), args.QueryText);
            }
            else
            {
                // Otherwise bypass navigation and provide the tools needed to emulate the back stack
                var page = new SearchResultsPage
                                {
                                    _previousContent = previousContent                                   
                                };
                page.LoadState(args.QueryText, null);
                Window.Current.Content = page;
            }

            // Either way, active the window
            Window.Current.Activate();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            _query = navigationParameter as String;

            var query = _query == null ? string.Empty : _query.ToLower();

            var total = (from g in App.Instance.DataSource.GroupList
                            from i in g.Items
                            where i.Title.ToLower().Contains(query) ||
                                i.Description.ToLower().Contains(query) 
                            select i).Count();

            var filterList = new List<Filter>
                                    {
                                        new Filter(string.Empty, "All", total, true)
                                    };
            filterList.AddRange(from blogGroup in App.Instance.DataSource.GroupList
                                let count = blogGroup.Items.Count(
                                    i => i.Title.ToLower().Contains(query)
                                            || i.Description.ToLower().Contains(query))
                                where count > 0
                                select new Filter(blogGroup.Id, blogGroup.Title, count));


            // Communicate results through the view model
            DefaultViewModel["QueryText"] = '\u201c' + _query + '\u201d';
            DefaultViewModel["CanGoBack"] = _previousContent != null;
            DefaultViewModel["Filters"] = new ObservableCollection<Filter>(filterList);
            DefaultViewModel["ShowFilters"] = filterList.Count > 1;
        }

        /// <summary>
        /// Invoked when the back button is pressed.
        /// </summary>
        /// <param name="sender">The Button instance representing the back button.</param>
        /// <param name="e">Event data describing how the button was clicked.</param>
        protected override void GoBack(object sender, RoutedEventArgs e)
        {
            // Return the application to the state it was in before search results were requested
            if (Frame != null && Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Navigate(typeof (GroupedItemsPage), "Groups");                
            }
        }

        private void Navigate(Type type, string parameter)
        {
            if (Frame == null)
            {
                var rootFrame = new Frame();
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                rootFrame.Navigate(type, parameter);
                Window.Current.Content = rootFrame;
                Window.Current.Activate();
            }
            else
            {
                Frame.Navigate(type, parameter);
            }
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var query = _query.ToLower();

            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                if (selectedFilter.Name.Equals("All"))
                {
                    DefaultViewModel["Results"] =
                        (from g in App.Instance.DataSource.GroupList
                            from i in g.Items
                            where i.Title.ToLower().Contains(query)
                                || i.Description.ToLower().Contains(query)
                            select
                                new SearchResult
                                    {
                                        Image = i.DefaultImageUri,
                                        Title = i.Title,
                                        Id = i.Id,
                                        Description = i.Description
                                    }).ToList();
                }
                else
                {
                    var blogGroup = App.Instance.DataSource.GetGroup(selectedFilter.Id);
                    DefaultViewModel["Results"] =
                        (from i in blogGroup.Items
                            where i.Title.ToLower().Contains(query)
                                || i.Description.ToLower().Contains(query)
                            select
                                new SearchResult
                                    {
                                        Id = i.Id,
                                        Image = i.DefaultImageUri,
                                        Title = i.Title,
                                        Description = i.Description
                                    }).ToList();
                }

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);
        }

        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        private void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View == null) return;
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;
            var filter = frameworkElement.DataContext;
            filtersViewSource.View.MoveCurrentTo(filter);
        }

        #region Nested type: Filter

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        public sealed class Filter : BindableBase
        {
            private bool _active;
            private int _count;

            private string _id;
            private String _name;

            public Filter(String id, String name, int count, bool active = false)
            {
                Id = id;
                Name = name;
                Count = count;
                Active = active;
            }

            public String Id
            {
                get { return _id; }
                private set { if (SetProperty(ref _id, value)) OnPropertyChanged("Id"); }
            }

            public String Name
            {
                get { return _name; }
                private set { if (SetProperty(ref _name, value)) OnPropertyChanged("Description"); }
            }

            public int Count
            {
                set { if (SetProperty(ref _count, value)) OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                set { SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }

            public override String ToString()
            {
                return Description;
            }
        }

        #endregion

        private void ResultsGridView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Navigate(typeof (ItemDetailPage), ((SearchResult) e.ClickedItem).Id);
        }

        private void ResultsListView_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Navigate(typeof(ItemDetailPage), ((SearchResult)e.ClickedItem).Id);
        }

        private class SearchResult
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public Uri Image { get; set; }
            public string Description { get; set; }
        }
    }
}