using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DataBinding
{
    /// <summary>
    /// Host for data-binding 
    /// </summary>
    public class DataBindingHost : INotifyPropertyChanged 
    {
        private bool _isOn;

        /// <summary>
        /// Determine whether it is "on"
        /// </summary>
        public bool IsOn
        {
            get
            {
                return _isOn;
            }

            set
            {
                _isOn = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Notify when the property changes
        /// </summary>
        /// <param name="caller">Caller is the property invoking the method</param>
        protected void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        /// <summary>
        /// Wire in an empty delegate to avoid having to check for null
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
