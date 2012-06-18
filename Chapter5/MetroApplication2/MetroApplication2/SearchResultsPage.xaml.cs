using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MetroApplication.Common;
using MetroApplication.Data;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace MetroApplication
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage
    {
        private readonly Dictionary<string, List<SampleDataItem>> _results =
            new Dictionary<string, List<SampleDataItem>>();

        private UIElement _previousContent;
        private ApplicationExecutionState _previousExecutionState;

        public SearchResultsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Determines how best to support navigation back to the previous application state.
        /// </summary>
        public static void Activate(String queryText, ApplicationExecutionState previousExecutionState)
        {
            UIElement previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            if (frame != null)
            {
                // If the app is already running and uses top-level frame navigation we can just
                // navigate to the search results
                frame.Navigate(typeof (SearchResultsPage), queryText);
            }
            else
            {
                // Otherwise bypass navigation and provide the tools needed to emulate the back stack
                var page = new SearchResultsPage
                               {_previousContent = previousContent, _previousExecutionState = previousExecutionState};
                page.LoadState(queryText, null);
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
            var queryTextLocal = navigationParameter as String;

            var filterList = new List<Filter> {new Filter("All", 0, true)};

            // Communicate results through the view model
            DefaultViewModel["QueryText"] = '\u201c' + queryTextLocal + '\u201d';
            DefaultViewModel["CanGoBack"] = _previousContent != null;

            ObservableCollection<SampleDataGroup> groups = ((App) Application.Current).DataSource.ItemGroups;
            if (queryTextLocal != null)
            {
                var query = queryTextLocal.ToLower();
                var all = new List<SampleDataItem>();
                _results.Add("All", all);

                foreach (var group in groups)
                {
                    var items = new List<SampleDataItem>();
                    _results.Add(@group.Title, items);

                    foreach (
                        var item in
                            @group.Items.Where(
                                item =>
                                item.Title.ToLower().Contains(query) || item.Description.ToLower().Contains(query)))
                    {
                        all.Add(item);
                        items.Add(item);
                    }

                    filterList.Add(new Filter(@group.Title, items.Count));
                }

                filterList[0].Count = all.Count;
            }

            DefaultViewModel["Filters"] = filterList;
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
            else if (_previousContent != null)
            {
                Window.Current.Content = _previousContent;
            }
        }

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                DefaultViewModel["Results"] = _results[selectedFilter.Name];

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

        private void ItemClick_1(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof (ItemDetailPage), ((SampleDataItem) e.ClickedItem).UniqueId);
        }

        #region Nested type: Filter

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : BindableBase
        {
            private bool _active;
            private int _count;
            private String _name;

            public Filter(String name, int count, bool active = false)
            {
                Name = name;
                Count = count;
                Active = active;
            }

            public String Name
            {
                get { return _name; }
// ReSharper disable ExplicitCallerInfoArgument
                private set { if (SetProperty(ref _name, value)) OnPropertyChanged("Description"); }
// ReSharper restore ExplicitCallerInfoArgument
            }

            public int Count
            {
                get { return _count; }
// ReSharper disable ExplicitCallerInfoArgument
                set { if (SetProperty(ref _count, value)) OnPropertyChanged("Description"); }
// ReSharper restore ExplicitCallerInfoArgument
            }

            public bool Active
            {
                get { return _active; }
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
    }
}