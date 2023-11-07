using PheasantTails.TwiHigh.Beta.Client.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PheasantTails.TwiHigh.Beta.Client.ViewModels
{
    internal abstract class ViewModelBase<TPage> : INotifyPropertyChanged, IDisposable where TPage : PageBase
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public TPage View { get; private set; }

        public ViewModelBase(TPage view)
        {
            View = view;
            PropertyChanged += View.InvokeStateHasChanged;
        }

        public virtual void Dispose()
        {
            PropertyChanged -= View.InvokeStateHasChanged;
            GC.SuppressFinalize(this);
        }

        protected void Set<TProperty>(ref TProperty storage, TProperty value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string? propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
