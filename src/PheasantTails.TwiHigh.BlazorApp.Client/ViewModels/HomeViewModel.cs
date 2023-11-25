using PheasantTails.TwiHigh.BlazorApp.Client.Models;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

internal class HomeViewModel : IDisposable
{
    private readonly ITimelineWorkerService _timelineWorkerService;
    private CompositeDisposable _disposable = new();

    public ReadOnlyReactiveCollection<DisplayTweet> Timeline { get; }
    public ReactiveCommand<Guid> DeleteCommand { get; }
    public ReactiveCommand FavoriteCommand { get; }
    public ReactiveCommand RetweetCommand { get; }
    public ReactiveCommand NavigateStatePageCommand { get; }
    public ReactiveCommand NavigateUserPageCommad { get; }
    public ReactiveCommand ReplayCommand { get; }

    public HomeViewModel(ITimelineWorkerService timelineWorkerService)
    {
        _timelineWorkerService = timelineWorkerService;
        Timeline = _timelineWorkerService.Timeline.ToObservable().ToReadOnlyReactiveCollection().AddTo(_disposable);
        DeleteCommand = new ReactiveCommand<Guid>();
        DeleteCommand.Subscribe(x => _timelineWorkerService.Remove(x))
            .AddTo(_disposable);

    }

    public void Dispose() => _disposable.Dispose();
}
