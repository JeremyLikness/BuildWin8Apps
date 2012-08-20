using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Windows8Application2.Common
{
    internal class SettingsFlyout
    {
        private const int Width = 346;
        private Popup _popup;

        public void ShowFlyout(UserControl control)
        {
            _popup = new Popup();
            _popup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            _popup.IsLightDismissEnabled = true;
            _popup.Width = Width;
            _popup.Height = Window.Current.Bounds.Height;

            control.Width = Width;
            control.Height = Window.Current.Bounds.Height;

            _popup.Child = control;
            _popup.SetValue(Canvas.LeftProperty, Window.Current.Bounds.Width - Width);
            _popup.SetValue(Canvas.TopProperty, 0);
            _popup.IsOpen = true;
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                _popup.IsOpen = false;
            }
        }

        private void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }
    }
}