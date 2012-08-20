using System;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Touch
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            _events = new ObservableCollection<string> { "Started" };
            EventList.ItemsSource = _events;            
        }

        private readonly ObservableCollection<string> _events;       

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {           
        }

        private void Grid_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            var pos = e.GetPosition(this);
            AddWithFocus(string.Format("{0}:{1} Tap", (int)pos.X, (int)pos.Y));            
        }

        private void Grid_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            var pos = e.GetPosition(this);
            AddWithFocus(string.Format("{0}:{1} Double Tap", (int)pos.X, (int)pos.Y));            
            Transformation.ScaleX = 1.0;
            Transformation.ScaleY = 1.0;
            Transformation.TranslateX = 0;
            Transformation.TranslateY = 0;
            Transformation.Rotation = 0;            
        }

        private void Grid_Holding_1(object sender, HoldingRoutedEventArgs e)
        {
            var pos = e.GetPosition(this);
            AddWithFocus(string.Format("{0}:{1} Hold {2}", (int)pos.X, (int)pos.Y, e.HoldingState));
        }

        private void Grid_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(this);
            AddWithFocus(string.Format("{0}:{1} Pointer Press", (int)pos.Position.X, (int)pos.Position.Y));            
        }

        private void Grid_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            var pos = e.GetCurrentPoint(this);
            AddWithFocus(string.Format("{0}:{1} Pointer Release", (int)pos.Position.X, (int)pos.Position.Y));
        }        

        private void TouchGrid_PointerEntered_1(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerMove = DateTime.MinValue;
            var pos = e.GetCurrentPoint(this);
            AddWithFocus(string.Format("{0}:{1} Pointer Entered", (int)pos.Position.X, (int)pos.Position.Y));
        }

        private void TouchGrid_PointerExited_1(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerMove = DateTime.MinValue;
            var pos = e.GetCurrentPoint(this);
            AddWithFocus(string.Format("{0}:{1} Pointer Exited", (int)pos.Position.X, (int)pos.Position.Y));
        }

        private DateTime _lastPointerMove = DateTime.MinValue;

        private void TouchGrid_PointerMoved_1(object sender, PointerRoutedEventArgs e)
        {
            if (DateTime.Now - _lastPointerMove < TimeSpan.FromSeconds(1))
            {
                return;
            }
            _lastPointerMove = DateTime.Now;
            var pos = e.GetCurrentPoint(this);
            AddWithFocus(string.Format("{0}:{1} Pointer Moved", (int)pos.Position.X, (int)pos.Position.Y));
        }

        private void Grid_ManipulationDelta_1(object sender, ManipulationDeltaRoutedEventArgs e)
        {           
            Transformation.ScaleX *= e.Delta.Scale;
            Transformation.ScaleY *= e.Delta.Scale;
            Transformation.TranslateX += e.Delta.Translation.X;
            Transformation.TranslateY += e.Delta.Translation.Y;
            Transformation.Rotation += e.Delta.Rotation;
        }        

        private void TouchGrid_PointerWheelChanged_1(object sender, PointerRoutedEventArgs e)
        {
            var factor = e.GetCurrentPoint((UIElement)sender)
                .Properties.MouseWheelDelta > 0
                ? 0.1 : -0.1;
            Transformation.ScaleX += factor;
            Transformation.ScaleY += factor;                        
        }

        private void EventList_Loaded_1(object sender, RoutedEventArgs e)
        {
            EventList.Focus(FocusState.Programmatic);
        }   

        private bool _isCtrlKeyPressed;

        private void EventList_KeyUp_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.Equals(VirtualKey.Control))
            {
                _isCtrlKeyPressed = false;
                AddWithFocus("Ctrl key released.");
            }
        }

        private void EventList_KeyDown_1(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key.Equals(VirtualKey.Control) && !_isCtrlKeyPressed)
            {
                _isCtrlKeyPressed = true;
                AddWithFocus("Ctrl Key pressed.");
            }
            else if (_isCtrlKeyPressed)
            {
                var factor = 0d;
                if (e.Key.Equals(VirtualKey.Add))
                {
                    factor = 0.1;
                }
                else if (e.Key.Equals(VirtualKey.Subtract))
                {
                    factor = -0.1;
                }
                Transformation.ScaleX += factor;
                Transformation.ScaleY += factor;  
            }
        }   

        private void AddWithFocus(string item)
        {
            _events.Add(item);
            EventList.ScrollIntoView(item);
        }

        private async void EventList_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            _events.Add("Context Menu invoked.");
            var contextMenu = new PopupMenu();
            contextMenu.Commands.Add(new UICommand("Clear list", 
                args => _events.Clear()));
            var dismissed = await contextMenu.ShowAsync(
                e.GetPosition(EventList));
            _events.Add(string.Format("Context Menu dismissed: {0}", 
                dismissed == null ? "Canceled" : dismissed.Label));
        }                               
    }    
}
