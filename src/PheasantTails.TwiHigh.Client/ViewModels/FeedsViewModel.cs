using PheasantTails.TwiHigh.Client.Pages;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PheasantTails.TwiHigh.Client.ViewModels
{
    internal class FeedsViewModel : ViewModelBase<Feeds>, IDisposable
    {

        private ObservableCollection<FeedContext> _myFeeds;

        public void MyFeedsChangedCallback(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                MyFeeds = new ObservableCollection<FeedContext>();
            }
            else
            {
                MyFeeds = new((IEnumerable<FeedContext>)e.NewItems);
            }
        }

        internal ObservableCollection<FeedContext> MyFeeds
        {
            get
            {
                return _myFeeds;
            }
            set
            {
                _myFeeds.CollectionChanged -= View.InvokeStateHasChanged;
                Set(ref _myFeeds, value, nameof(MyFeeds));
                _myFeeds.CollectionChanged += View.InvokeStateHasChanged;
            }
        }

        public FeedsViewModel(Feeds view) : base(view)
        {
            _myFeeds = new ObservableCollection<FeedContext>();
        }

        public override void Dispose()
        {
            _myFeeds.CollectionChanged -= View.InvokeStateHasChanged;
            base.Dispose();
        }
    }
}
