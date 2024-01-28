using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Extensions;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using PheasantTails.TwiHigh.Data.Model.Feeds;
using PheasantTails.TwiHigh.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public class FeedsViewModel : ViewModelBase, IFeedsViewModel
{
    private readonly IFeedWorkerService _feedService;
    public ReactiveCollection<FeedContext> MyFeeds { get; private set; } = default!;
    public AsyncReactiveCommand<ITweet> NavigateStatePageCommand { get; private set; } = default!;
    public AsyncReactiveCommand<ITweet> NavigateStatePageWithReplyCommand { get; private set; } = default!;
    public ReactiveCommandSlim<string> NavigateUserPageCommand { get; private set; } = default!;

    public FeedsViewModel(IFeedWorkerService feedService, NavigationManager navigationManager, IMessageService messageService) : base(navigationManager, messageService)
    {
        _feedService = feedService;
    }

    protected override void Initialize()
    {
        MyFeeds = new ReactiveCollection<FeedContext>().AddTo(_disposable);
        NavigateStatePageCommand = new AsyncReactiveCommand<ITweet>().AddTo(_disposable);
        NavigateStatePageWithReplyCommand = new AsyncReactiveCommand<ITweet>().AddTo(_disposable);
        NavigateUserPageCommand = new ReactiveCommandSlim<string>().AddTo(_disposable);
    }

    protected override void Subscribe()
    {
        NavigateStatePageCommand.Subscribe(async tweet => await OnClickDetailAsync(tweet));
        NavigateStatePageWithReplyCommand.Subscribe(async tweet => await OnClickDetailAsync(tweet, true));
        NavigateUserPageCommand.Subscribe((userDisplayId) => _navigationManager.NavigateToProfilePage(userDisplayId));
    }

    private async Task OnClickDetailAsync(ITweet tweet, bool isReply = false)
    {
        Guid? feedId = MyFeeds.FirstOrDefault(f => f.FeedByTweet?.Id == tweet.Id)?.Id;
        if (feedId == null)
        {
            return;
        }
        await _feedService.MarkAsReadedFeedsAsync(new[] { feedId.Value });
        _navigationManager.NavigateToStatePage(tweet, isReply);
    }
}
